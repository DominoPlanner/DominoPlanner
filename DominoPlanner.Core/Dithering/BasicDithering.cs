using System;
using System.Collections.Generic;
using System.Linq;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core.Dithering
{
    public class BasicDithering
    {
        protected IColorSpaceComparison comp;
        protected Lab[] labColors;
        protected DominoColor[] palette;
        protected int[] field;

        protected WriteableBitmap bitmap;
        public BasicDithering(IColorSpaceComparison comp, List<DominoColor> palette)
        {
            this.comp = comp;
            this.palette = palette.ToArray() ;
            labColors = palette.Select(p => p.labColor).ToArray();
        }
        public virtual int[] Dither(WriteableBitmap input)
        {
            using (input.GetBitmapContext())
            {
                field = new int[input.PixelWidth * input.PixelHeight];
                for (int x = 0; x < input.PixelWidth; x++)
                {
                    for (int y = 0; y < input.PixelHeight; y++)
                    {
                        System.Windows.Media.Color col = input.GetPixel(x, y);
                        Lab lab = col.ToLab();
                        int newp = Compare(lab, comp, labColors);
                        System.Windows.Media.Color newpixel = palette[newp].mediaColor;
                        field[input.PixelHeight * x + y] = newp;
                        DiffuseError(x, y, col.R - newpixel.R, col.G - newpixel.G, col.B - newpixel.B);
                    }
                }
            }
            return field;
        }

        protected virtual void DiffuseError(int x, int y, int v1, int v2, int v3)
        {
            // do nothing in default implementation
        }

        private static int Compare(Lab a, IColorSpaceComparison comp, Lab[] cols)
        {
            int Minimum = 0;

            double min = comp.Compare(a, cols[0]);
            double temp = Int32.MaxValue;
            for (int z = 1; z < cols.Length; z++)
            {
                temp = comp.Compare(cols[z], a);
                if (min > temp)
                {
                    min = temp;
                    Minimum = z;
                }
            }
            return Minimum;
        }
        protected void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            if (x >= bitmap.PixelWidth) x = bitmap.PixelWidth - 1;
            else if (x < 0) x = 0;
            if (y >= bitmap.PixelHeight) y = bitmap.PixelHeight - 1;
            else if (y < 0) y = 0;
            bitmap.SetPixel(x, y, r, g, b);

        }
        protected System.Windows.Media.Color GetPixel(int x, int y)
        {
            if (x >= bitmap.PixelWidth) x = bitmap.PixelWidth - 1;
            else if (x < 0) x = 0;
            if (y >= bitmap.PixelHeight) y = bitmap.PixelHeight - 1;
            else if (y < 0) y = 0;
            return bitmap.GetPixel(x, y);
        }
        protected byte Scale(int value)
        {
            return (byte)((value > 255) ? 255 : ((value < 0) ? 0 : value));
        }
    }
}
