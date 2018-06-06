using System;
using System.Drawing;

namespace ColorMine.ColorSpaces.Conversions
{
    // Code lovingly copied from StackOverflow (and tweaked a bit)
    // Question/Answer: http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv/1626175#1626175
    // Submitter: Greg http://stackoverflow.com/users/12971/greg
    // License: http://creativecommons.org/licenses/by-sa/3.0/
    internal static class HsvConverter
    {
        internal static void ToColorSpace(IRgb color, IHsv item)
        {
            var max = Max(color.R, Max(color.G, color.B));
            var min = Min(color.R, Min(color.G, color.B));

            item.H = Color.FromArgb(255, (int)color.R, (int)color.G, (int)color.B).GetHue();
            item.S = (max <= 0) ? 0 : 1d - (1d * min / max);
            item.V = max / 255d;
        }

        internal static IRgb ToColor(IHsv item)
        {
            var range = Convert.ToInt32(Math.Floor(item.H / 60.0)) % 6;
            var f = item.H / 60.0 - Math.Floor(item.H / 60.0);

            var v = item.V * 255.0;
            var p = v * (1 - item.S);
            var q = v * (1 - f * item.S);
            var t = v * (1 - (1 - f) * item.S);

            switch (range)
            {
                case 0:
                    return NewRgb(v, t, p);
                case 1:
                    return NewRgb(q, v, p);
                case 2:
                    return NewRgb(p, v, t);
                case 3:
                    return NewRgb(p, q, v);
                case 4:
                    return NewRgb(t, p, v);
            }
            return NewRgb(v, p, q);
        }

        private static IRgb NewRgb(double r, double g, double b)
        {
            return new Rgb { R = r, G = g, B = b };
        }

        private static double Max(double a, double b)
        {
            return a > b ? a : b;
        }

        private static double Min(double a, double b)
        {
            return a < b ? a : b;
        }
    }
}