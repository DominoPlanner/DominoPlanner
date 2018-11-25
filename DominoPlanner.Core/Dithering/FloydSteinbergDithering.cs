using ColorMine.ColorSpaces.Comparisons;
using Emgu.CV;
using System.Collections.Generic;

namespace DominoPlanner.Core.Dithering
{
    public class FloydSteinbergDithering : Dithering
    {
        public FloydSteinbergDithering() 
            : base() {
            maxDegreeOfParallelism = 1;

        }
        public override void DiffuseError(int x, int y, int r, int g, int b, Image<Emgu.CV.Structure.Bgra, byte> bitmap)
        {
            if (x + 1 < bitmap.Width)
            {
                SetPixel(x + 1, y, 
                        Scale((int)((GetPixel(x + 1, y, bitmap).R) + (r * 7d / 16d))),
                        Scale((int)((GetPixel(x + 1, y, bitmap).G) + (g * 7d / 16d))),
                        Scale((int)((GetPixel(x + 1, y, bitmap).B) + (b * 7d / 16d))), bitmap);
            }
            if (y + 1 < bitmap.Height && x != 0)
            {
                SetPixel(x - 1, y + 1, 
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).R) + (r * 3d / 16d))),
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).G) + (g * 3d / 16d))),
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).B) + (b * 3d / 16d))), bitmap);
            }
            if (y + 1 < bitmap.Height)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((GetPixel(x, y + 1, bitmap).R) + (r * 5d / 16d))),
                        Scale((int)((GetPixel(x, y + 1, bitmap).G) + (g * 5d / 16d))),
                        Scale((int)((GetPixel(x, y + 1, bitmap).B) + (b * 5d / 16d))), bitmap);
            }
            if (x + 1 < bitmap.Width && y + 1 < bitmap.Height)
            {
                SetPixel(x + 1, y + 1, 
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).R) + (r * 3d / 16d))),
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).G) + (g * 3d / 16d))),
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).B) + (b * 3d / 16d))), bitmap);
            }
        }

    }
}
