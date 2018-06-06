using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner;
using DominoPlanner.Document_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Dominorechner_V2.ColorMine
{
    unsafe class JarvisJudiceNinkeDithering : Dithering
    {
    IColorSpaceComparison comp;
        private List<Lab> lab_colors;
        private List<DominoColor> palette;
        private int[,] field;
        WriteableBitmap bitmap;
        byte* pbuff;
        public JarvisJudiceNinkeDithering(IColorSpaceComparison comparison, List<DominoColor> palette)
        {
            comp = comparison;
            this.palette = palette;
            lab_colors = new List<Lab>();
            for (int i = 0; i < palette.Count; i++)
            {
                lab_colors.Add((new Rgb { R = palette[i].rgb.R, G = palette[i].rgb.G, B = palette[i].rgb.B }).To<Lab>());
            }
        }
        override public int[,] Dither(WriteableBitmap input, IColorSpaceComparison comparison, float threshold = Int32.MaxValue)
        {
            bitmap = input;
            IntPtr backBuffer = bitmap.BackBuffer;
            pbuff = (byte*)backBuffer.ToPointer();
            field = new int[input.PixelWidth, input.PixelHeight];
            input.Lock();
            int bwidth = input.PixelWidth;
            int bheight = input.PixelHeight;
            int stride = input.BackBufferStride;
            int bytesPerPixel = (input.Format.BitsPerPixel + 7) / 8;
            unsafe
            {
                byte* ptr = (byte*)input.BackBuffer;
                int cRowStart = 0;
                int cColStart = 0;
                for (int row = 0; row < bheight; row++)
                {
                    cColStart = cRowStart;
                    for (int col = 0; col < bwidth; col++)
                    {

                        byte* bPixel = ptr + cColStart;
                        Lab lab = (new Rgb { R = bPixel[2], G = bPixel[1], B = bPixel[0] }).To<Lab>();

                        int newp = Compare(lab, comparison, lab_colors);
                        System.Windows.Media.Color newpixel = palette[newp].rgb;
                        field[col, row] = newp;
                        cColStart += bytesPerPixel;
                        DiffuseError(col, row, bPixel[2] - newpixel.R, bPixel[1] - newpixel.G, bPixel[0] - newpixel.B);
                    }
                    cRowStart += stride;
                }
            }
            input.Unlock();
            return field;
        }
        void SetPixel(int x, int y, int r, int g, int b)
        {

                pbuff[4 * x + (y * bitmap.BackBufferStride)] = (byte)b;
                pbuff[4 * x + (y * bitmap.BackBufferStride) + 1] = (byte)g;
                pbuff[4 * x + (y * bitmap.BackBufferStride) + 2] = (byte)r;


        }
        Color GetPixel(int x, int y)
        {

                return Color.FromArgb(pbuff[4 * x + (y * bitmap.BackBufferStride) + 2],
                    pbuff[4 * x + (y * bitmap.BackBufferStride) + 1],
                    pbuff[4 * x + (y * bitmap.BackBufferStride)]);
        }
        private void DiffuseError(int x, int y, int r, int g, int b)
        {
            //right pixels
            if (x + 1 < bitmap.Width)
            {
                SetPixel(x + 1, y, 
                        Scale((int)((double)(GetPixel(x + 1, y).R) + ((double)r * 7d / 48d))),
                        Scale((int)((double)(GetPixel(x + 1, y).G) + ((double)g * 7d / 48d))),
                        Scale((int)((double)(GetPixel(x + 1, y).B) + ((double)b * 7d / 48d))));
            }
            if (x + 2 < bitmap.Width)
            {
                SetPixel(x + 2, y, 
                        Scale((int)((double)(GetPixel(x + 2, y).R) + ((double)r * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x + 2, y).G) + ((double)g * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x + 2, y).B) + ((double)b * 5d / 48d))));
            }
            //first row
            if (y + 1 < bitmap.Height)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((double)(GetPixel(x, y + 1).R) + ((double)r * 7d / 48d))),
                        Scale((int)((double)(GetPixel(x, y + 1).G) + ((double)g * 7d / 48d))),
                        Scale((int)((double)(GetPixel(x, y + 1).B) + ((double)b * 7d / 48d))));
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 1, 
                        Scale((int)((double)(GetPixel(x + 1, y + 1).R) + ((double)r * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x + 1, y + 1).G) + ((double)g * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x + 1, y + 1).B) + ((double)b * 5d / 48d))));
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 1, 
                        Scale((int)((double)(GetPixel(x + 2, y + 1).R) + ((double)r * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x + 2, y + 1).G) + ((double)g * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x + 2, y + 1).B) + ((double)b * 3d / 48d))));
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 1, 
                        Scale((int)((double)(GetPixel(x - 1, y + 1).R) + ((double)r * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x - 1, y + 1).G) + ((double)g * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x - 1, y + 1).B) + ((double)b * 5d / 48d))));
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 1, 
                        Scale((int)((double)(GetPixel(x - 2, y + 1).R) + ((double)r * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x - 2, y + 1).G) + ((double)g * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x - 2, y + 1).B) + ((double)b * 3d / 48d))));
                }
            }
            //second row
            if (y + 2 < bitmap.Height)
            {
                SetPixel(x, y + 2, 
                        Scale((int)((double)(GetPixel(x, y + 2).R) + ((double)r * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x, y + 2).G) + ((double)g * 5d / 48d))),
                        Scale((int)((double)(GetPixel(x, y + 2).B) + ((double)b * 5d / 48d))));
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 2, 
                        Scale((int)((double)(GetPixel(x + 1, y + 2).R) + ((double)r * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x + 1, y + 2).G) + ((double)g * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x + 1, y + 2).B) + ((double)b * 3d / 48d))));
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 2, 
                        Scale((int)((double)(GetPixel(x + 2, y + 2).R) + ((double)r * 1d / 48d))),
                        Scale((int)((double)(GetPixel(x + 2, y + 2).G) + ((double)g * 1d / 48d))),
                        Scale((int)((double)(GetPixel(x + 2, y + 2).B) + ((double)b * 1d / 48d))));
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 2, 
                        Scale((int)((double)(GetPixel(x - 1, y + 2).R) + ((double)r * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x - 1, y + 2).G) + ((double)g * 3d / 48d))),
                        Scale((int)((double)(GetPixel(x - 1, y + 2).B) + ((double)b * 3d / 48d))));
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 2, 
                        Scale((int)((double)(GetPixel(x - 2, y + 2).R) + ((double)r * 1d / 48d))),
                        Scale((int)((double)(GetPixel(x - 2, y + 2).G) + ((double)g * 1d / 48d))),
                        Scale((int)((double)(GetPixel(x - 2, y + 2).B) + ((double)b * 1d / 48d))));
                }
            }
        }
        private int Scale(int value)
        {
            return (value > 255) ? 255 : ((value < 0) ? 0 : value);
        }
    }
}
