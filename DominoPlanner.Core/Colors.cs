using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    static class ColorExtension
    {
        private static Lab ToLab(this Bgr color)
        {
            double r = color.Red / 255,
                g = color.Green / 255,
                b = color.Blue / 255,
                x, y, z;

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            // Umrechnung bei Tageslicht
            x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3) : (7.787 * x) + 16 / 116;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3) : (7.787 * y) + 16 / 116;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3) : (7.787 * z) + 16 / 116;

            return new Lab((116 * y) - 16, (500 * (x - y)), 200 * (y - z));
        }
        public static Lab ToLab(this System.Windows.Media.Color color)
        {
            return new Bgr(color.B, color.G, color.R).ToLab();
        }
        public static Lab ToLab(this Bgra color)
        {
            return new Bgr(color.Blue, color.Green, color.Red).ToLab();
        }
    }
    public interface IColorComparison
    {
        double Distance(Lab lab1, Lab lab2);

        ColorComparisonMode colorComparisonMode { get; }
    }
    public class CieDe2000Comparison : IColorComparison
    {
        ColorComparisonMode IColorComparison.colorComparisonMode
        {
            get
            {
                return ColorComparisonMode.Cie2000;
            }
        }

        public double Distance(Lab lab1, Lab lab2)
        {
            //Set weighting factors to 1
            double k_L = 1.0d;
            double k_C = 1.0d;
            double k_H = 1.0d;


            //Change Color Space to L*a*b:
            //DateTime t1 = DateTime.Now;
            //  MessageBox.Show("" + (DateTime.Now.Ticks - t1.Ticks));

            //Calculate Cprime1, Cprime2, Cabbar
            double c_star_1_ab = Math.Sqrt(lab1.Y * lab1.Y + lab1.Z * lab1.Z);
            double c_star_2_ab = Math.Sqrt(lab2.Y * lab2.Y + lab2.Z * lab2.Z);
            double c_star_average_ab = (c_star_1_ab + c_star_2_ab) / 2;

            double c_star_average_ab_pot7 = c_star_average_ab * c_star_average_ab * c_star_average_ab;
            c_star_average_ab_pot7 *= c_star_average_ab_pot7 * c_star_average_ab;

            double G = 0.5 * (1 - Math.Sqrt(c_star_average_ab_pot7 / (c_star_average_ab_pot7 + 6103515625))); //25^7


            double a1_prime = (1 + G) * lab1.Y;
            double a2_prime = (1 + G) * lab2.Y;

            double C_prime_1 = Math.Sqrt(a1_prime * a1_prime + lab1.Z * lab1.Z);
            double C_prime_2 = Math.Sqrt(a2_prime * a2_prime + lab2.Z * lab2.Z);
            //Angles in Degree.
            double h_prime_1 = ((Math.Atan2(lab1.Z, a1_prime) * 180d / Math.PI) + 360) % 360d;
            double h_prime_2 = ((Math.Atan2(lab2.Z, a2_prime) * 180d / Math.PI) + 360) % 360d;

            double delta_L_prime = lab2.X - lab1.X;
            double delta_C_prime = C_prime_2 - C_prime_1;

            double h_bar = Math.Abs(h_prime_1 - h_prime_2);
            double delta_h_prime;
            if (C_prime_1 * C_prime_2 == 0) delta_h_prime = 0;
            else
            {
                if (h_bar <= 180d)
                {
                    delta_h_prime = h_prime_2 - h_prime_1;
                }
                else if (h_bar > 180d && h_prime_2 <= h_prime_1)
                {
                    delta_h_prime = h_prime_2 - h_prime_1 + 360.0;
                }
                else
                {
                    delta_h_prime = h_prime_2 - h_prime_1 - 360.0;
                }
            }
            double delta_H_prime = 2 * Math.Sqrt(C_prime_1 * C_prime_2) * Math.Sin(delta_h_prime * Math.PI / 360);

            // Calculate CIEDE2000
            double L_prime_average = (lab1.X + lab2.X) / 2d;
            double C_prime_average = (C_prime_1 + C_prime_2) / 2d;

            //Calculate h_prime_average

            double h_prime_average;
            if (C_prime_1 * C_prime_2 == 0) h_prime_average = 0;
            else
            {
                if (h_bar <= 180d)
                {
                    h_prime_average = (h_prime_1 + h_prime_2) / 2;
                }
                else if (h_bar > 180d && (h_prime_1 + h_prime_2) < 360d)
                {
                    h_prime_average = (h_prime_1 + h_prime_2 + 360d) / 2;
                }
                else
                {
                    h_prime_average = (h_prime_1 + h_prime_2 - 360d) / 2;
                }
            }
            double L_prime_average_minus_50_square = (L_prime_average - 50);
            L_prime_average_minus_50_square *= L_prime_average_minus_50_square;

            double S_L = 1 + ((.015d * L_prime_average_minus_50_square) / Math.Sqrt(20 + L_prime_average_minus_50_square));

            double S_C = 1 + .045 * C_prime_average;
            double T = 1
                - .17 * Math.Cos(DegToRad(h_prime_average - 30))
                + .24 * Math.Cos(DegToRad(h_prime_average * 2))
                + .32 * Math.Cos(DegToRad(h_prime_average * 3 + 6))
                - .2 * Math.Cos(DegToRad(h_prime_average * 4 - 63));
            double S_H = 1 + .015 * T * C_prime_average;
            double h_prime_average_minus_275_div_25_square = (h_prime_average - 275) / (25);
            h_prime_average_minus_275_div_25_square *= h_prime_average_minus_275_div_25_square;
            double delta_theta = 30 * Math.Exp(-h_prime_average_minus_275_div_25_square);

            double C_prime_average_pot_7 = C_prime_average * C_prime_average * C_prime_average;
            C_prime_average_pot_7 *= C_prime_average_pot_7 * C_prime_average;
            double R_C = 2 * Math.Sqrt(C_prime_average_pot_7 / (C_prime_average_pot_7 + 6103515625));

            double R_T = -Math.Sin(DegToRad(2 * delta_theta)) * R_C;

            double delta_L_prime_div_k_L_S_L = delta_L_prime / (S_L * k_L);
            double delta_C_prime_div_k_C_S_C = delta_C_prime / (S_C * k_C);
            double delta_H_prime_div_k_H_S_H = delta_H_prime / (S_H * k_H);

            double CIEDE2000 = Math.Sqrt(
                delta_L_prime_div_k_L_S_L * delta_L_prime_div_k_L_S_L
                + delta_C_prime_div_k_C_S_C * delta_C_prime_div_k_C_S_C
                + delta_H_prime_div_k_H_S_H * delta_H_prime_div_k_H_S_H
                + R_T * delta_C_prime_div_k_C_S_C * delta_H_prime_div_k_H_S_H
                );
            return CIEDE2000;
        }
        private double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
    public class CmcComparison : IColorComparison
    {
        ColorComparisonMode IColorComparison.colorComparisonMode
        {
            get
            {
                return ColorComparisonMode.CmcComparison;
            }
        }

        private readonly double _lightness = 2;
        private readonly double _chroma = 1;
        public double Distance(Emgu.CV.Structure.Lab lab1, Emgu.CV.Structure.Lab lab2)
        {
            var deltaL = lab1.X - lab2.X;
            var h = Math.Atan2(lab1.Z, lab1.Y);

            var c1 = Math.Sqrt(lab1.Y * lab1.Y + lab1.Z * lab1.Z);
            var c2 = Math.Sqrt(lab2.Y * lab2.Y + lab2.Z * lab2.Z);
            var deltaC = c1 - c2;

            var deltaH = Math.Sqrt(
                (lab1.Y - lab2.Y) * (lab1.Y - lab2.Y) +
                (lab1.Z - lab2.Z) * (lab1.Z - lab2.Z) -
                deltaC * deltaC);

            var c1_4 = c1 * c1;
            c1_4 *= c1_4;
            var t = 164 <= h || h >= 345
                        ? .56 + Math.Abs(.2 * Math.Cos(h + 168.0))
                        : .36 + Math.Abs(.4 * Math.Cos(h + 35.0));
            var f = Math.Sqrt(c1_4 / (c1_4 + 1900.0));

            var sL = lab1.X < 16 ? .511 : (.040975 * lab1.X) / (1.0 + .01765 * lab1.X);
            var sC = (.0638 * c1) / (1 + .0131 * c1) + .638;
            var sH = sC * (f * t + 1 - f);

            var differences = DistanceDivided(deltaL, _lightness * sL) +
                              DistanceDivided(deltaC, _chroma * sC) +
                              DistanceDivided(deltaH, sH);

            return Math.Sqrt(differences);
        }
        private static double DistanceDivided(double a, double dividend)
        {
            var adiv = a / dividend;
            return adiv * adiv;
        }
    }
    public class Cie94Comparison : IColorComparison
    {
        ColorComparisonMode IColorComparison.colorComparisonMode
        {
            get
            {
                return ColorComparisonMode.Cie94;
            }
        }

        double Kl = 1.0;
        double K1 = .045;
        double K2 = .015;
        public double Distance(Lab lab1, Lab lab2)
        {

            var deltaL = lab1.X - lab2.X;
            var deltaA = lab1.Y - lab2.Y;
            var deltaB = lab1.Z - lab2.Z;

            var c1 = Math.Sqrt(lab1.Y * lab1.Y + lab1.Z * lab1.Z);
            var c2 = Math.Sqrt(lab2.Y * lab2.Y + lab2.Z * lab2.Z);
            var deltaC = c1 - c2;

            var deltaH = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
            deltaH = deltaH < 0 ? 0 : Math.Sqrt(deltaH);

            const double sl = 1.0;
            const double kc = 1.0;
            const double kh = 1.0;

            var sc = 1.0 + K1 * c1;
            var sh = 1.0 + K2 * c1;

            var deltaLKlsl = deltaL / (Kl * sl);
            var deltaCkcsc = deltaC / (kc * sc);
            var deltaHkhsh = deltaH / (kh * sh);
            var i = deltaLKlsl * deltaLKlsl + deltaCkcsc * deltaCkcsc + deltaHkhsh * deltaHkhsh;
            return i < 0 ? 0 : Math.Sqrt(i);
        }
    }
    public class Cie1976Comparison : IColorComparison
    {
        ColorComparisonMode IColorComparison.colorComparisonMode {
            get
            {
                return ColorComparisonMode.Cie76;
            }
        }

        public double Distance(Lab lab1, Lab lab2)
        {

            var differences = Distance(lab1.X, lab2.X) + Distance(lab1.Y, lab2.Y) + Distance(lab1.Z, lab2.Z);
            return Math.Sqrt(differences);
        }

        private static double Distance(double a, double b)
        {
            return (a - b) * (a - b);
        }
    }

}
