using System;
using System.Collections.Generic;
using System.Linq;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DominoPlanner.Core.Dithering
{
    public class BasicDithering
    {
        protected IColorSpaceComparison comp;
        protected Lab[] labColors;
        protected DominoColor[] palette;
        protected int[] field;
        protected IterationInformation IterationInformation;
        protected double iterationWeight;
        // Die Dithering-Verfahren müssen sequenziell ausgeführt werden.
        protected int maxDegreeOfParallelism = 1;
        protected Image<Emgu.CV.Structure.Bgr, Byte> bitmap;
        
        public BasicDithering(IColorSpaceComparison comp, List<DominoColor> palette, IterationInformation iterationInformation)
        {
            this.comp = comp;
            this.palette = palette.ToArray() ;
            labColors = palette.Select(p => p.labColor).ToArray();
            this.IterationInformation = iterationInformation;
        }
        public virtual int[] Dither(Mat input)
        {
            Console.WriteLine("Debug Flag");
            IterationInformation.weights = Enumerable.Repeat(1.0, palette.Length).ToArray();
            if (IterationInformation is IterativeColorRestriction)
            {
                if (palette.Sum(color => color.count) < input.Width * input.Height)
                    throw new InvalidOperationException("Gesamtsteineanzahl ist größer als vorhandene Anzahl, kann nicht konvergieren");
            }
            field = new int[input.Width * input.Height];
            // tatsächlich genutzte Farben auslesen
            for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
            {
                IterationInformation.numberofiterations = iter;
                Console.WriteLine($"Iteration {iter}");
                bitmap = input.ToImage<Emgu.CV.Structure.Bgr, Byte>();
                Parallel.For(0, input.Width, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism }, (x) =>
                //for (int x = 0; x < input.Width; x++)
                {
                    for (int y = input.Height - 1; y >= 0; y--)
                    {
                        Rgb rgb = new Rgb()
                        {
                            R = bitmap.Data[y, x, 2],
                            G = bitmap.Data[y, x, 1],
                            B = bitmap.Data[y, x, 0]
                        };
                        Lab lab = rgb.To<Lab>();
                        int newp = Compare(lab);
                        System.Windows.Media.Color newpixel = palette[newp].mediaColor;
                        field[input.Height * x + y] = newp;
                        DiffuseError(x, y, (int)(rgb.R) - newpixel.R, (int)(rgb.G) - newpixel.G, (int)(rgb.B) - newpixel.B);
                    }
                    });
                //}
                IterationInformation.EvaluateSolution(palette, field);
                if (IterationInformation.colorRestrictionsFulfilled != false) break;
            }
            bitmap.Dispose();
            return field;
        }

        protected virtual void DiffuseError(int x, int y, int v1, int v2, int v3)
        {
            // do nothing in default implementation
        }

        private int Compare(Lab a)
        {
            int Minimum = 0;

            double min = Int32.MaxValue;
            double temp = Int32.MaxValue;
            for (int z = labColors.Length - 1; z >= 0; z--)
            {
                temp = comp.Compare(labColors[z], a) * IterationInformation.weights[z];
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
            if (x >= bitmap.Width) x = bitmap.Width - 1;
            else if (x < 0) x = 0;
            if (y >= bitmap.Height) y = bitmap.Height - 1;
            else if (y < 0) y = 0;
            bitmap.Data[y, x, 1] = g;
            bitmap.Data[y, x, 0] = b;
            bitmap.Data[y, x, 2] = r;

        }
        protected System.Windows.Media.Color GetPixel(int x, int y)
        {
            if (x >= bitmap.Width) x = bitmap.Width - 1;
            else if (x < 0) x = 0;
            if (y >= bitmap.Height) y = bitmap.Height - 1;
            else if (y < 0) y = 0;
            return new System.Windows.Media.Color() {
                R = bitmap.Data[y, x, 2],
                G = bitmap.Data[y, x, 1],
                B = bitmap.Data[y, x, 0]
            };
        }
        protected byte Scale(int value)
        {
            return (byte)((value > 255) ? 255 : ((value < 0) ? 0 : value));
        }
    }
}
