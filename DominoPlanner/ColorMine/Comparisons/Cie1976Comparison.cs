using System;

namespace ColorMine.ColorSpaces.Comparisons
{
    /// <summary>
    /// Implements the CIE76 method of delta-e: http://en.wikipedia.org/wiki/Color_difference#CIE76
    /// </summary>
    public class Cie1976Comparison : IColorSpaceComparison
    {
        /// <summary>
        /// Calculates the CIE76 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIE76
        /// </summary>
        public double Compare(Lab lab1, Lab lab2)
        {
            var a = lab1;
            var b = lab2;

            var differences = Distance(a.L, b.L) + Distance(a.A, b.A) + Distance(a.B, b.B);
            return Math.Sqrt(differences);
        }

        private static double Distance(double a, double b)
        {
            return (a - b) * (a - b);
        }
    }
}