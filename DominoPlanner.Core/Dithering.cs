using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Threading.Tasks;
using System.Diagnostics;
using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    public class Dithering
    {
        public bool resetColors = false;
        internal int[,] weights;
        internal int divisor;
        internal int[] startindizes;
        internal int matrix_width;
        internal int matrix_height;
        internal int start_first_row;
        // Die Dithering-Verfahren müssen sequenziell ausgeführt werden.
        // Die Basisimplementierung kann aber multicore laufen
        public int maxDegreeOfParallelism = -1;
        public Dithering()
        {
            matrix_width = 1;
            matrix_height = 1;
            start_first_row = 1;
            GetDiscreteArray();
            
        }
        
        public void AddToPixel(ref Bgra color, int r, int g, int b)
        {
            color.Red = Saturate(color.Red + r);
            color.Blue = Saturate(color.Blue + b);
            color.Green = Saturate(color.Green + g);
        }
        double Saturate(double input)
        {
            if (input > 255)
                return 255;
            else if (input < 0)
                return 0;
            return input;
        }
        public virtual double Weight(double x, double y) => 0;

        public void GetDiscreteArray()
        {
            weights = new int[matrix_height, matrix_width];
            
            startindizes = new int[matrix_height];
            startindizes[0] = start_first_row;
            for (int j = 0; j < matrix_height; j++)
            {
                for (int i = startindizes[j]; i< matrix_width; i++)
                {
                    weights[j, i] = (int)Weight(i - startindizes[0] + 1, j);
                }
            }
            divisor = weights.Cast<int>().Sum();
        }
    }
    public class FloydSteinbergDithering : Dithering
    {
        public override double Weight(double x, double y)
        {
            if (x >= 0) return 11 - 4 * x - 6 * y;
            else return 11 + 2 * x - 6 * y;
        }
        public FloydSteinbergDithering()
        {
            matrix_width = 3;
            matrix_height = 2;
            start_first_row = 2;
            GetDiscreteArray();
            resetColors = true;
            maxDegreeOfParallelism = 1;
        }
    }
    public class JarvisJudiceNinkeDithering : Dithering
    {
        public override double Weight(double x, double y)
        {
            if (x >= 0) return 9 - 2 * x - 2 * y;
            else return 9 + 2 * x - 2 * y;
        }
        public JarvisJudiceNinkeDithering()
        {
            matrix_width = 5;
            matrix_height = 3;
            start_first_row = 3;
            resetColors = true;
            maxDegreeOfParallelism = 1;
            GetDiscreteArray();
        }
    }
    public class StuckiDithering : Dithering
    {
        public override double Weight(double x, double y)
        {
            if (x >= 0) return Math.Pow(2, 2 - x) * Math.Pow(2, 2 - y);
            else return Math.Pow(2, 2 + x) * Math.Pow(2, 2 - y);
        }
        public StuckiDithering()
        {
            matrix_width = 5;
            matrix_height = 3;
            start_first_row = 3;
            resetColors = true;
            maxDegreeOfParallelism = 1;
            GetDiscreteArray();
        }
    }
}
