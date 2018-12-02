using System;
using System.Collections.Generic;
using System.Drawing;
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
    [ProtoInclude(100, typeof(BlendFileFilter))]
    public abstract class BlendImageFilter : ImageFilter
    {
        private double center_x;
        [ProtoMember(1)]
        public double CenterX { get => center_x; set { SetField(ref center_x, value);  } }
        private double center_y;
        [ProtoMember(2)]
        public double CenterY { get => center_y; set { SetField(ref center_y, value); } }
        
        private double scale_x;
        [ProtoMember(3)]
        public double ScaleX { get => scale_x; set => SetField(ref scale_x, value); }
        
        private double scale_y;
        [ProtoMember(4)]
        public double ScaleY { get => scale_y; set => SetField(ref scale_y, value); }
        
        private double rotate_angle;
        [ProtoMember(5)]
        public double RotateAngle { get => rotate_angle; set => SetField(ref rotate_angle, value); }
        
        internal Mat to_blend;
        public override void Apply(Mat input)
        {
            if (!mat_valid) UpdateMat();
            using (var image = to_blend.ToImage<Bgra, byte>())
            {
                image.Resize((int)(image.Width * scale_x), (int)(image.Height * scale_y), Emgu.CV.CvEnum.Inter.Lanczos4);
                image.Rotate(rotate_angle, new Bgra(0, 0, 0, 255), false);
                input.OverlayImage(image, (int) (center_x - image.Width / 2d), (int) (center_y - image.Height / 2d));
            }
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

        private Color _color;
        [ProtoMember(5)]
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
            graphics.Clear(Color.Transparent);
            graphics.DrawString(Text, font, new SolidBrush(Color), new PointF(0, 0));
            to_blend = new Image<Bgra, byte>(bmp).Mat;

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
            set { if (SetField(ref _filepath, value)) mat_valid = false; }
        }
        [ProtoAfterDeserialization]
        public override void UpdateMat()
        {
            to_blend = new Mat(Workspace.Instance.MakePathAbsolute(_filepath));
        }
    }
    public class ContrastLightFilter : ImageFilter
    {
        private double _a;
        public double Alpha { get => _a; set => SetField(ref _a, value); }
        private double _b;
        public double Beta { get => _b; set => SetField(ref _b, value); }
        public override void Apply(Mat input)
        {
            using (Image<Bgra, byte> image = input.ToImage<Bgra, byte>())
            {
                Parallel.For(0, input.Height, (y) =>
                {
                    for (int x = input.Width - 1; x >= 0; x--)
                    {
                        for (int c = 2; c >= 0; c--)
                        {
                            image.Data[y, x, c] = Saturate(image.Data[y, x, c] * Alpha + Beta);
                        }
                    }
                });
                input = image.Mat;
            }
        }
        
    
    }
    public class GammaCorrectFilter : ImageFilter
    {
        private double _gamma;
        public double Gamma { get => _gamma; set { SetField(ref _gamma, value); updateLUT(); } }

        private byte[] LUT;
        public override void Apply(Mat input)
        {
            using (Image<Bgra, byte> image = input.ToImage<Bgra, byte>())
            {
                Parallel.For(0, input.Height, (y) =>
                {
                    for (int x = input.Width - 1; x >= 0; x--)
                    {
                        for (int c = 2; c >= 0; c--)
                        {
                            image.Data[y, x, c] = LUT[image.Data[y, x, c]];

                        }
                    }
                });
                input = image.Mat;
            }
            
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
        
        public override void Apply(Mat input)
        {
            using (var image = input.ToImage<Bgra, byte>())
            {
                CvInvoke.GaussianBlur(image, input, new Size(KernelSize, KernelSize), StandardDeviation);
            }
        }
    }
    [ProtoContract]
    public class GaussianSharpenFilter : GaussianBlurFilter
    {
        private double weight = 1;
        [ProtoMember(1)]
        public double SharpenWeight { get => weight; set { SetField(ref weight, value); } }
        public override void Apply(Mat input)
        {
            using (var image = input.ToImage<Bgra, byte>())
            {
                var blurred = new Mat();
                CvInvoke.GaussianBlur(image, blurred, new Size(KernelSize, KernelSize), StandardDeviation);
                CvInvoke.AddWeighted(image, (1.0 + weight), blurred, -weight, 0, input, Emgu.CV.CvEnum.DepthType.Cv8U);
            }
        }
    }
    public static class ImageExtensions
    {
        public static void OverlayImage(this Image<Bgra, byte> background, Image<Bgra, byte> overlay, int start_x = 0, int start_y = 0)
        {
            Parallel.For(0, overlay.Height, (y) =>
            {
                for (int x = overlay.Width - 1; x >= 0; x--)
                {
                    if (y + start_y > 0 && x + start_x > 0 && y + start_y < overlay.Height && x + start_x < overlay.Width)
                    {
                        double opacity_overlay = overlay.Data[y + start_y, x + start_x, 3] / 255.0d;

                        for (int c = 2; c >= 0; c--)
                        {
                            background.Data[y, x, c] = (byte)(background.Data[y, x, c] * (1 - opacity_overlay)
                            + overlay.Data[y + start_y, x + start_x, c] * opacity_overlay);
                        }
                        background.Data[y, x, 3] += overlay.Data[y + start_y, x + start_x, 3];
                    }

                }
            });
        }
        public static void OverlayImage(this Mat background, Image<Bgra, byte> overlay, int start_x=0, int start_y =0)
        {
            using (var bg = background.ToImage<Bgra, byte>())
            {
                OverlayImage(bg, overlay, start_x, start_y);
                background = bg.Mat;
            }
        }
    }
}
