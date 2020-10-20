using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using ProtoBuf;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(100, typeof(BlendTextFilter))]
    [ProtoInclude(101, typeof(BlendFileFilter))]
    public abstract class BlendImageFilter : ImageFilter
    {
        private double center_x;
        [ProtoMember(1)]
        public double CenterX { get => center_x; set
            {
                SetField(ref center_x, value);  } }
        private double center_y;
        [ProtoMember(2)]
        public double CenterY { get => center_y; set { SetField(ref center_y, value); } }
        
        private double scale_x = 1;
        [ProtoMember(3)]
        public double ScaleX { get => scale_x; set => SetField(ref scale_x, value); }
        
        private double scale_y = 1;
        [ProtoMember(4)]
        public double ScaleY { get => scale_y; set => SetField(ref scale_y, value); }
        
        private double rotate_angle = 0;
        [ProtoMember(5)]
        public double RotateAngle { get => rotate_angle; set => SetField(ref rotate_angle, value); }
        private Image<Bgra, byte> _to_blend;
        internal Image<Bgra, byte> to_blend
        {
            get
            {
                if (_to_blend == null) UpdateMat();
                return _to_blend;
            }
            set
            {
                _to_blend = value;
                
            }
        }
        public override void Apply(Image<Bgra, byte> input)
        {
            if (!mat_valid) UpdateMat();
            var image = to_blend.Clone();
            if (scale_x != 1 && scale_y != 1)
                image = image.Resize((int)(image.Width * scale_x), (int)(image.Height * scale_y), Emgu.CV.CvEnum.Inter.Lanczos4);
            if (rotate_angle != 0)
                image = image.Rotate(rotate_angle, new Bgra(0, 0, 0, 0), false);
            input.OverlayImage(image, (int)(center_x - image.Width / 2d), (int)(center_y - image.Height / 2d));
        }
        public Size GetSizeOfMat()
        {
            return to_blend.Size;
        }
        public bool mat_valid;
        public abstract void UpdateMat();
    }
    [ProtoContract]
    public class BlendTextFilter : BlendImageFilter
    {
        private string _text;
        [ProtoMember(1)]
        public string Text { get => _text; set { if (SetField(ref _text, value)) mat_valid = false;} }

        private string _fontfamily;
        [ProtoMember(2)]
        public string FontFamily { get => _fontfamily; set { if (SetField(ref _fontfamily, value)) mat_valid = false; } }

        private int _fontsize;

        [ProtoMember(3)]
        public int FontSize { get => _fontsize; set { if (SetField(ref _fontsize, value)) mat_valid = false; } }

        private FontStyle _fontStyle;
        [ProtoMember(4)]
        public FontStyle FontStyle { get => _fontStyle; set { if (SetField(ref _fontStyle, value)) mat_valid = false; } }

        [ProtoMember(5)]
        private string before_surrogate { get { return ColorTranslator.ToHtml(_color); } set { _color = ColorTranslator.FromHtml(value); } }
        private Color _color;
        public Color Color { get => _color; set { SetField(ref _color, value); mat_valid = false; } }
        [ProtoAfterDeserialization]
        public override void UpdateMat()
        {

            var font = new Font(new FontFamily(FontFamily), FontSize, FontStyle);
            // dummy image zum ausmessen des Texts
            var bmp = new System.Drawing.Bitmap(5, 10);
            var graphics = Graphics.FromImage(bmp);
            var size = graphics.MeasureString(Text, font);
            bmp = new System.Drawing.Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
            
            graphics = Graphics.FromImage(bmp);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphics.Clear(Color.Transparent);
            graphics.DrawString(Text, font, new SolidBrush(Color), new PointF(0, 0));
            to_blend = bmp.ToImage<Bgra, byte>();
            mat_valid = true;
        }
    }
    [ProtoContract]
    public class BlendFileFilter : BlendImageFilter
    {
        
        string _filepath;
        [ProtoMember(1)]
        public string FilePath
        {
            get => _filepath;
            set { if (SetField(ref _filepath, value)) { mat_valid = false; } }
        }
        public override void UpdateMat()
        {
            to_blend = new Image<Bgra, byte>(Workspace.AbsolutePathFromReference(ref _filepath, parent));
        }
        public BlendFileFilter()
        {
            mat_valid = true;
        }
    }
    [ProtoContract]
    public class ContrastLightFilter : ImageFilter
    {
        private double _a;
        [ProtoMember(1)]
        public double Alpha { get => _a; set => SetField(ref _a, value); }
        private double _b;
        [ProtoMember(2)]
        public double Beta { get => _b; set => SetField(ref _b, value); }
        public override void Apply(Image<Bgra, byte> input)
        {
            Parallel.For(0, input.Height, (y) =>
            {
                for (int x = input.Width - 1; x >= 0; x--)
                {
                    for (int c = 2; c >= 0; c--)
                    {
                        input.Data[y, x, c] = Saturate(input.Data[y, x, c] * Alpha + Beta);
                    }
                }
            });
        }
    }
        
    
    [ProtoContract]
    public class GammaCorrectFilter : ImageFilter
    {
        private double _gamma;
        [ProtoMember(1)]
        public double Gamma { get => _gamma; set { SetField(ref _gamma, value); updateLUT(); } }

        private byte[] LUT;
        public override void Apply(Image<Bgra, byte> input)
        {
            Parallel.For(0, input.Height, (y) =>
            {
                for (int x = input.Width - 1; x >= 0; x--)
                {
                    for (int c = 2; c >= 0; c--)
                    {
                        input.Data[y, x, c] = LUT[input.Data[y, x, c]];

                    }
                }
            });

        }
        public void updateLUT()
        {
            LUT = new byte[256];
            for (int i = 0; i < LUT.Length; i++)
            {
                LUT[i] = Saturate(Math.Pow(i / 255.0, Gamma) * 255.0);
            }            
        }
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(GaussianSharpenFilter))]
    public class GaussianBlurFilter : ImageFilter
    {
        private int kernel_size = 1;
        [ProtoMember(1)]
        public int KernelSize { get => kernel_size; set { if (value % 2 == 1) SetField(ref kernel_size, value);  } }
        private double std_dev = 1;
        [ProtoMember(2)]
        public double StandardDeviation { get => std_dev; set {  SetField(ref std_dev, value); } }

        public override void Apply(Image<Bgra, byte> input)
        {
            //var result = input.Clone();
            CvInvoke.GaussianBlur(input, input, new Size(KernelSize, KernelSize), StandardDeviation);
            //input = result;
        }
    }
    [ProtoContract]
    public class GaussianSharpenFilter : GaussianBlurFilter
    {
        private double weight = 1;
        [ProtoMember(1)]
        public double SharpenWeight { get => weight; set { SetField(ref weight, value); } }
        public override void Apply(Image<Bgra, byte>  input)
        {
            var blurred = new Mat();
            CvInvoke.GaussianBlur(input, blurred, new Size(KernelSize, KernelSize), StandardDeviation);
            CvInvoke.AddWeighted(input, (1.0 + weight), blurred, -weight, 0, input, Emgu.CV.CvEnum.DepthType.Cv8U);
            
        }
    }
    [ProtoContract]
    public class ReplaceColorFilter : ImageFilter
    {
        [ProtoMember(2)]
        private string before_surrogate { get { return ColorTranslator.ToHtml(_before); } set { _before = ColorTranslator.FromHtml(value); } }
        private Color _before;
        public Color BeforeColor { get => _before; set { SetField(ref _before, value); } }
        [ProtoMember(3)]
        private string after_surrogate { get { return ColorTranslator.ToHtml(_after); } set { _after = ColorTranslator.FromHtml(value); } }
        private Color _after;
        public Color AfterColor { get => _after; set { SetField(ref _after, value); } }
        private int _tol;
        [ProtoMember(1)]
        public int Tolerance { get => _tol; set { SetField(ref _tol, value); } }

        public override void Apply(Image<Bgra, byte> input)
        {
            Parallel.For(0, input.Height, (y) =>
            {
                for (int x = input.Width - 1; x >= 0; x--)
                {
                    if (Math.Abs(input.Data[y, x, 0] - BeforeColor.B) < Tolerance &&
                    Math.Abs(input.Data[y, x, 1] - BeforeColor.G) < Tolerance &&
                    Math.Abs(input.Data[y, x, 2] - BeforeColor.R) < Tolerance)
                    {
                        input.Data[y, x, 0] = AfterColor.B;
                        input.Data[y, x, 1] = AfterColor.G;
                        input.Data[y, x, 2] = AfterColor.R;
                        input.Data[y, x, 3] = AfterColor.A;
                    }
                }
            });
        }
    }
    public static class ImageExtensions
    {
        public static void OverlayImage(this Image<Bgra, byte> background, Image<Bgra, byte> overlay, int start_x = 0, int start_y = 0)
        {
            Bitmap source = background.ToBitmap();
            Bitmap over = overlay.AsBitmap();
            unsafe
            {
                BitmapData backgroundData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), 
                    ImageLockMode.ReadWrite, source.PixelFormat);
                BitmapData overlayData = over.LockBits(new Rectangle(0, 0, over.Width, over.Height),
                    ImageLockMode.ReadWrite, over.PixelFormat);

                int bytesPerPixel_source = Image.GetPixelFormatSize(source.PixelFormat) / 8;
                int heightInPixels_source = backgroundData.Height;
                int widthInPixels_source = backgroundData.Width;
                int widthInBytes_source = backgroundData.Width * bytesPerPixel_source;
                byte* PtrFirstPixel_source = (byte*)backgroundData.Scan0;

                int bytesPerPixel_overlay = Image.GetPixelFormatSize(over.PixelFormat) / 8;
                int heightInPixels_overlay = overlayData.Height;
                int widthInPixels_overlay = overlayData.Width;
                int widthInBytes_overlay = overlayData.Width * bytesPerPixel_overlay;
                byte* PtrFirstPixel_overlay = (byte*)overlayData.Scan0;
                Parallel.For(start_y < 0 ? start_y : 0, overlayData.Height, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, y =>
                {
                    if (y + start_y < heightInPixels_source)
                    {
                        byte* currentLine_overlay = PtrFirstPixel_overlay + (y * overlayData.Stride);
                        byte* currentLine_background = PtrFirstPixel_source + (y + start_y) * backgroundData.Stride;
                        int x = start_x < 0 ? start_x : 0;
                        while (x + start_x < widthInPixels_source && x < widthInPixels_overlay)
                        {
                            int pixelx_source = (x + start_x) * bytesPerPixel_source;
                            int pixelx_overlay = x * bytesPerPixel_overlay;
                            double opacity_overlay = currentLine_overlay[pixelx_overlay + 3] / 255.0d;
                            for (int c = 0; c < 3; c++)
                            {
                                currentLine_background[pixelx_source + c] = (byte)(currentLine_background[pixelx_source + c] * (1 - opacity_overlay)
                                    + currentLine_overlay[pixelx_overlay + c] * opacity_overlay);
                            }
                            int transp_neu = currentLine_background[pixelx_source + 3] + currentLine_overlay[pixelx_overlay + 3];
                            currentLine_background[pixelx_source + 3] = (byte)(transp_neu > 255 ? 255 : transp_neu);
                            x++;
                        }
                    }
                });
                source.UnlockBits(backgroundData);
            }
            background = source.ToImage<Bgra, byte>();
        }
        public static void OpacityReduction(this Image<Bgra, byte> input, double opacity)
        {
            int height = input.Height;
            int width = input.Width;
            Parallel.For(0, height, (y) =>
            {
                for (int x = width - 1; x >= 0; x--)
                {

                    var new_opacity = input.Data[y, x, 3] * opacity;
                    input.Data[y, x, 3] = (byte) (new_opacity > 255 ? 255 : new_opacity);
                }
            });
        }
    }
}
