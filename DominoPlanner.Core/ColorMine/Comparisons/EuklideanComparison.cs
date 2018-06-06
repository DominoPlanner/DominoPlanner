using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColorMine.ColorSpaces.Comparisons;

namespace ColorMine.ColorSpaces.Comparisons
{
    internal class EuklideanComparison : IColorSpaceComparison
    {
        public double Compare(Lab lab1, Lab lab2)
        {
            var a =lab1.To<Rgb>();
            var b = lab2.To<Rgb>();

            var differences = Distance(a.R, b.R) + Distance(a.G, b.G) + Distance(a.B, b.B);
            return Math.Sqrt(differences);
        }

        private static double Distance(double a, double b)
        {
            return (a - b) * (a - b);
        }
    }
}
