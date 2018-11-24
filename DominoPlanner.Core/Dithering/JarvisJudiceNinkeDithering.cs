using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner.Core;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core.Dithering
{
    public class JarvisJudiceNinkeDithering : Dithering
    {
        public JarvisJudiceNinkeDithering()
        {
            maxDegreeOfParallelism = 1;
        }
        
        public override void DiffuseError(int x, int y, int r, int g, int b, Image<Emgu.CV.Structure.Bgr, byte> bitmap)
        {
            //right pixels
            if (x + 1 < bitmap.Width)
            {
                SetPixel(x + 1, y, 
                        Scale((int)((GetPixel(x + 1, y, bitmap).R) + (r * 7d / 48d))),
                        Scale((int)((GetPixel(x + 1, y, bitmap).G) + (g * 7d / 48d))),
                        Scale((int)((GetPixel(x + 1, y, bitmap).B) + (b * 7d / 48d))),bitmap);
            }
            if (x + 2 < bitmap.Width)
            {
                SetPixel(x + 2, y, 
                        Scale((int)((GetPixel(x + 2, y, bitmap).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x + 2, y, bitmap).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x + 2, y, bitmap).B) + (b * 5d / 48d))),bitmap);
            }
            //first row
            if (y + 1 < bitmap.Height)
            {
                SetPixel(x, y + 1, 
                        Scale((int)((GetPixel(x, y + 1, bitmap).R) + (r * 7d / 48d))),
                        Scale((int)((GetPixel(x, y + 1, bitmap).G) + (g * 7d / 48d))),
                        Scale((int)((GetPixel(x, y + 1, bitmap).B) + (b * 7d / 48d))),bitmap);
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 1, 
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 1, bitmap).B) + (b * 5d / 48d))),bitmap);
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 1, 
                        Scale((int)((GetPixel(x + 2, y + 1, bitmap).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 1, bitmap).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 1, bitmap).B) + (b * 3d / 48d))),bitmap);
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 1, 
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 1, bitmap).B) + (b * 5d / 48d))),bitmap);
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 1, 
                        Scale((int)((GetPixel(x - 2, y + 1, bitmap).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 1, bitmap).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 1, bitmap).B) + (b * 3d / 48d))),bitmap);
                }
            }
            //second row
            if (y + 2 < bitmap.Height)
            {
                SetPixel(x, y + 2, 
                        Scale((int)((GetPixel(x, y + 2, bitmap).R) + (r * 5d / 48d))),
                        Scale((int)((GetPixel(x, y + 2, bitmap).G) + (g * 5d / 48d))),
                        Scale((int)((GetPixel(x, y + 2, bitmap).B) + (b * 5d / 48d))),bitmap);
                if (x + 1 < bitmap.Width)
                {
                    SetPixel(x + 1, y + 2, 
                        Scale((int)((GetPixel(x + 1, y + 2, bitmap).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 2, bitmap).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x + 1, y + 2, bitmap).B) + (b * 3d / 48d))),bitmap);
                }
                if (x + 2 < bitmap.Width)
                {
                    SetPixel(x + 2, y + 2, 
                        Scale((int)((GetPixel(x + 2, y + 2, bitmap).R) + (r * 1d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 2, bitmap).G) + (g * 1d / 48d))),
                        Scale((int)((GetPixel(x + 2, y + 2, bitmap).B) + (b * 1d / 48d))),bitmap);
                }
                if (x != 0)
                {
                    SetPixel(x - 1, y + 2, 
                        Scale((int)((GetPixel(x - 1, y + 2, bitmap).R) + (r * 3d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 2, bitmap).G) + (g * 3d / 48d))),
                        Scale((int)((GetPixel(x - 1, y + 2, bitmap).B) + (b * 3d / 48d))),bitmap);
                }
                if (x != 1)
                {
                    SetPixel(x - 2, y + 2, 
                        Scale((int)((GetPixel(x - 2, y + 2, bitmap).R) + (r * 1d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 2, bitmap).G) + (g * 1d / 48d))),
                        Scale((int)((GetPixel(x - 2, y + 2, bitmap).B) + (b * 1d / 48d))),bitmap);
                }
            }
        }
    }
}
