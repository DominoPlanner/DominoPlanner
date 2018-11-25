using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core.Dithering
{
    public class StuckiDithering : Dithering
    {
        public StuckiDithering()
        {
            maxDegreeOfParallelism = 1;
        }

        public override void DiffuseError(int x, int y, int r, int g, int b, Image<Emgu.CV.Structure.Bgra, byte> bitmap)
        {
            //right pixels
            if (x + 1 < bitmap.Width)
            {
                SetPixel(x + 1, y, 
                        Scale((int)(GetPixel(x + 1, y, bitmap).R + (r * 8d / 42d))),
                        Scale((int)(GetPixel(x + 1, y, bitmap).G + (g * 8d / 42d))),
                        Scale((int)(GetPixel(x + 1, y, bitmap).B + (b * 8d / 42d))),bitmap);
            }
            if (x + 2 < bitmap.Width)
            {
                SetPixel(x + 2, y,
                        Scale((int)((GetPixel(x + 2, y, bitmap).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x + 2, y, bitmap).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x + 2, y, bitmap).B) + (b * 4d / 42d))),bitmap);
            }
            //first row
            if (y + 1 < bitmap.Height)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((GetPixel(x, y + 1, bitmap).R) + (r * 8d / 42d))),
                        Scale((int)((GetPixel(x, y + 1, bitmap).G) + (g * 8d / 42d))),
                        Scale((int)((GetPixel(x, y + 1, bitmap).B) + (b * 8d / 42d))),bitmap);
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 1, 
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).B) + (b * 4d / 42d))),bitmap);
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 1,
                        Scale((int)((GetPixel(x + 2, y + 1, bitmap).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 1, bitmap).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 1, bitmap).B) + (b * 2d / 42d))),bitmap);
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 1, 
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).B) + (b * 4d / 42d))),bitmap);
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 1, 
                        Scale((int)((GetPixel(x - 2, y + 1, bitmap).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 1, bitmap).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 1, bitmap).B) + (b * 2d / 42d))),bitmap);
                }
            }
            //second row
            if (y + 2 < bitmap.Height)
            {
                SetPixel(x, y + 2, 
                        Scale((int)((GetPixel(x, y + 2, bitmap).R) + (r * 4d / 42d))),
                        Scale((int)((GetPixel(x, y + 2, bitmap).G) + (g * 4d / 42d))),
                        Scale((int)((GetPixel(x, y + 2, bitmap).B) + (b * 4d / 42d))),bitmap);
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 2, 
                        Scale((int)((GetPixel(x + 1, y + 2, bitmap).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 2, bitmap).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x + 1, y + 2, bitmap).B) + (b * 2d / 42d))),bitmap);
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 2, 
                        Scale((int)((GetPixel(x + 2, y + 2, bitmap).R) + (r * 1d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 2, bitmap).G) + (g * 1d / 42d))),
                        Scale((int)((GetPixel(x + 2, y + 2, bitmap).B) + (b * 1d / 42d))),bitmap);
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 2, 
                        Scale((int)((GetPixel(x - 1, y + 2, bitmap).R) + (r * 2d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 2, bitmap).G) + (g * 2d / 42d))),
                        Scale((int)((GetPixel(x - 1, y + 2, bitmap).B) + (b * 2d / 42d))),bitmap);
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 2,
                        Scale((int)((GetPixel(x - 2, y + 2, bitmap).R) + (r * 1d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 2, bitmap).G) + (g * 1d / 42d))),
                        Scale((int)((GetPixel(x - 2, y + 2, bitmap).B) + (b * 1d / 42d))),bitmap);
                }
            }
        }
    }
}
