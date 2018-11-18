using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core.Dithering
{
    class JarvisJudiceNinkeDithering : BasicDithering
    {
        public JarvisJudiceNinkeDithering(IColorSpaceComparison comparison, List<DominoColor> palette, IterationInformation iterationInformation)
            : base(comparison, palette, iterationInformation)
        {
            maxDegreeOfParallelism = 1;
        }
        
        protected override void DiffuseError(int x, int y, int r, int g, int b)
        {
            //right pixels
            if (x + 1 < bitmap.Width)
            {
                SetPixel(x + 1, y, 
                        Scale((int)((GetPixel(x + 1, y).R) + (r * 7d / 48d))),
                        Scale((int)((GetPixel(x + 1, y).G) + (g * 7d / 48d))),
                        Scale((int)((GetPixel(x + 1, y).B) + (b * 7d / 48d))));
            }
            if (x + 2 < bitmap.Width)
            {
                SetPixel(x + 2, y, 
                        Scale((int)((GetPixel(x + 2, y).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x + 2, y).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x + 2, y).B) + (b * 5d / 48d))));
            }
            //first row
            if (y + 1 < bitmap.Height)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((GetPixel(x, y + 1).R) + (r * 7d / 48d))),
                        Scale((int)((GetPixel(x, y + 1).G) + (g * 7d / 48d))),
                        Scale((int)((GetPixel(x, y + 1).B) + (b * 7d / 48d))));
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 1, 
                        Scale((int)((GetPixel(x + 1, y + 1).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 1).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 1).B) + (b * 5d / 48d))));
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 1, 
                        Scale((int)((GetPixel(x + 2, y + 1).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 1).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 1).B) + (b * 3d / 48d))));
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 1, 
                        Scale((int)((GetPixel(x - 1, y + 1).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 1).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 1).B) + (b * 5d / 48d))));
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 1, 
                        Scale((int)((GetPixel(x - 2, y + 1).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 1).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 1).B) + (b * 3d / 48d))));
                }
            }
            //second row
            if (y + 2 < bitmap.Height)
            {
                SetPixel(x, y + 2, 
                        Scale((int)((GetPixel(x, y + 2).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x, y + 2).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x, y + 2).B) + (b * 5d / 48d))));
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 2, 
                        Scale((int)((GetPixel(x + 1, y + 2).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 2).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 2).B) + (b * 3d / 48d))));
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 2, 
                        Scale((int)((GetPixel(x + 2, y + 2).R) + (r * 1d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 2).G) + (g * 1d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 2).B) + (b * 1d / 48d))));
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 2, 
                        Scale((int)((GetPixel(x - 1, y + 2).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 2).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 2).B) + (b * 3d / 48d))));
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 2, 
                        Scale((int)((GetPixel(x - 2, y + 2).R) + (r * 1d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 2).G) + (g * 1d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 2).B) + (b * 1d / 48d))));
                }
            }
        }
    }
}
