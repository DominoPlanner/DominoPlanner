using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner;
using DominoPlanner.Document_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Dominorechner_V2.ColorMine
{
    class NoDithering : Dithering
    {
        IColorSpaceComparison comp;
        private List<Lab> lab_colors;
        private int[,] field;
        private WriteableBitmap bitmap;
        public NoDithering(IColorSpaceComparison comparison, List<DominoColor> palette)
        {

            comp = comparison;
            lab_colors = new List<Lab>();
            for (int i = 0; i < palette.Count; i++)
            {
                lab_colors.Add((new Rgb { R = palette[i].rgb.R, G = palette[i].rgb.G, B = palette[i].rgb.B}).To<Lab>());
            }
        }
        override public int[,] Dither(WriteableBitmap input, IColorSpaceComparison comparison, float threshold = Int32.MaxValue)
        {
            bitmap = input;
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
                        field[col, row] = newp;
                        cColStart += bytesPerPixel;
                    }
                    cRowStart += stride;
                }
            }
            input.Unlock();
            return field;
            
        }
        void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            unsafe
            {
                IntPtr backBuffer = bitmap.BackBuffer;
                byte* pbuff = (byte*)backBuffer.ToPointer();

                pbuff[4 * x + (y * bitmap.BackBufferStride)] = b;
                pbuff[4 * x + (y * bitmap.BackBufferStride) + 1] = g;
                pbuff[4 * x + (y * bitmap.BackBufferStride) + 2] = r;

            }
        }
        Color GetPixel(int x, int y)
        {
            unsafe
            {
                IntPtr backBuffer = bitmap.BackBuffer;
                byte* pbuff = (byte*)backBuffer.ToPointer();
                return Color.FromArgb(pbuff[4 * x + (y * bitmap.BackBufferStride) + 2],
                    pbuff[4 * x + (y * bitmap.BackBufferStride) + 1],
                    pbuff[4 * x + (y * bitmap.BackBufferStride)]);
            }
        }
    }
}
