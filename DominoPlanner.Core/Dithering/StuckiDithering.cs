using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core.Dithering
{
    class StuckiDithering : BasicDithering
    {
        public StuckiDithering(IColorSpaceComparison comparison, List<DominoColor> palette) : base(comparison, palette)
        {

        }

        protected override void DiffuseError(int x, int y, int r, int g, int b)
        {
            //right pixels
            if (x + 1 < bitmap.PixelWidth)
            {
                SetPixel(x + 1, y, 
                        Scale((int)(GetPixel(x + 1, y).R + (r * 8d / 42d))),
                        Scale((int)(GetPixel(x + 1, y).G + (g * 8d / 42d))),
                        Scale((int)(GetPixel(x + 1, y).B + (b * 8d / 42d))));
            }
            if (x + 2 < bitmap.PixelWidth)
            {
                SetPixel(x + 2, y,
                        Scale((int)((GetPixel(x + 2, y).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x + 2, y).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x + 2, y).B) + (b * 4d / 42d))));
            }
            //first row
            if (y + 1 < bitmap.PixelHeight)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((GetPixel(x, y + 1).R) + (r * 8d / 42d))),
                        Scale((int)((GetPixel(x, y + 1).G) + (g * 8d / 42d))),
                        Scale((int)((GetPixel(x, y + 1).B) + (b * 8d / 42d))));
                if (x + 1 < bitmap.PixelWidth)
                {
                    SetPixel(x + 1, y + 1, 
                        Scale((int)((GetPixel(x + 1, y + 1).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 1).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 1).B) + (b * 4d / 42d))));
                }
                if (x + 2 < bitmap.PixelWidth)
                {
                    SetPixel(x + 2, y + 1,
                        Scale((int)((GetPixel(x + 2, y + 1).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 1).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 1).B) + (b * 2d / 42d))));
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 1, 
                        Scale((int)((GetPixel(x - 1, y + 1).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 1).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 1).B) + (b * 4d / 42d))));
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 1, 
                        Scale((int)((GetPixel(x - 2, y + 1).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 1).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 1).B) + (b * 2d / 42d))));
                }
            }
            //second row
            if (y + 2 < bitmap.PixelHeight)
            {
                SetPixel(x, y + 2, 
                        Scale((int)((GetPixel(x, y + 2).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x, y + 2).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x, y + 2).B) + (b * 4d / 42d))));
                if (x + 1 < bitmap.PixelWidth)
                {
                    SetPixel(x + 1, y + 2, 
                        Scale((int)((GetPixel(x + 1, y + 2).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 2).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 2).B) + (b * 2d / 42d))));
                }
                if (x + 2 < bitmap.PixelWidth)
                {
                    SetPixel(x + 2, y + 2, 
                        Scale((int)((GetPixel(x + 2, y + 2).R) + (r * 1d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 2).G) + (g * 1d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 2).B) + (b * 1d / 42d))));
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 2, 
                        Scale((int)((GetPixel(x - 1, y + 2).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 2).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 2).B) + (b * 2d / 42d))));
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 2,
                        Scale((int)((GetPixel(x - 2, y + 2).R) + (r * 1d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 2).G) + (g * 1d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 2).B) + (b * 1d / 42d))));
                }
            }
        }
    }
}
