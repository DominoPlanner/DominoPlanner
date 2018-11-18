using ColorMine.ColorSpaces.Comparisons;
using System.Collections.Generic;

namespace DominoPlanner.Core.Dithering
{
    class FloydSteinbergDithering : BasicDithering
    {
        public FloydSteinbergDithering(IColorSpaceComparison comparison, List<DominoColor> palette, IterationInformation iterationInformation) 
            : base(comparison, palette, iterationInformation) {
            maxDegreeOfParallelism = 1;

        }
        protected override void DiffuseError(int x, int y, int r, int g, int b)
        {
            if (x + 1 < bitmap.Width)
            {
                SetPixel(x + 1, y, 
                        Scale((int)((GetPixel(x + 1, y).R) + (r * 7d / 16d))),
                        Scale((int)((GetPixel(x + 1, y).G) + (g * 7d / 16d))),
                        Scale((int)((GetPixel(x + 1, y).B) + (b * 7d / 16d))));
            }
            if (y + 1 < bitmap.Height && x != 0)
            {
                SetPixel(x - 1, y + 1, 
                        Scale((int)((GetPixel(x - 1, y + 1).R) + (r * 3d / 16d))),
                        Scale((int)((GetPixel(x - 1, y + 1).G) + (g * 3d / 16d))),
                        Scale((int)((GetPixel(x - 1, y + 1).B) + (b * 3d / 16d))));
            }
            if (y + 1 < bitmap.Height)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((GetPixel(x, y + 1).R) + (r * 5d / 16d))),
                        Scale((int)((GetPixel(x, y + 1).G) + (g * 5d / 16d))),
                        Scale((int)((GetPixel(x, y + 1).B) + (b * 5d / 16d))));
            }
            if (x + 1 < bitmap.Width && y + 1 < bitmap.Height)
            {
                SetPixel(x + 1, y + 1, 
                        Scale((int)((GetPixel(x + 1, y + 1).R) + (r * 3d / 16d))),
                        Scale((int)((GetPixel(x + 1, y + 1).G) + (g * 3d / 16d))),
                        Scale((int)((GetPixel(x + 1, y + 1).B) + (b * 3d / 16d))));
            }
        }

    }
}
