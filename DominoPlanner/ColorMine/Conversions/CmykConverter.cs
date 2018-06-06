using System;
using System.Windows.Media;
using ColorMine.ColorSpaces.Conversions.Utility;

namespace ColorMine.ColorSpaces.Conversions
{
    public static class CmykConverter
    {
        public static void ToColorSpace(IRgb color, ICmyk item)
        {
            var cmy = new Cmy();
            cmy.Initialize(color);

            var k = 1.0;
            if (cmy.C < k)
                k = cmy.C;
            if (cmy.M < k)
                k = cmy.M;
            if (cmy.Y < k)
                k = cmy.Y;
            item.K = k;

            if (k.BasicallyEqualTo(1))
            {
                item.C = 0;
                item.M = 0;
                item.Y = 0;
            }
            else
            {
                item.C = (cmy.C - k) / (1 - k);
                item.M = (cmy.M - k) / (1 - k);
                item.Y = (cmy.Y - k) / (1 - k);
            }
        }

        public static void ToColorSpace(IRgb color, ICmyk item, Uri cmykProfile)
        {
            if (cmykProfile == null)
            {
                ToColorSpace(color, item);
                return;
            }
            
            var cmyk = CmykProfileConverter.TranslateColor(color, cmykProfile);
            item.C = cmyk.C;
            item.M = cmyk.M;
            item.Y = cmyk.Y;
            item.K = cmyk.K;
        }

        public static void ToColorSpace(IRgb color, ICmyk item, Uri cmykProfile, Uri rgbProfile)
        {
            if (rgbProfile == null)
            {
                ToColorSpace(color, item, cmykProfile);
                return;
            }

            var cmyk = CmykProfileConverter.TranslateColor(color, cmykProfile, rgbProfile);
            item.C = cmyk.C;
            item.M = cmyk.M;
            item.Y = cmyk.Y;
            item.K = cmyk.K;
        }

        public static IRgb ToColor(ICmyk item)
        {
            var cmy = new Cmy
                {
                    C = (item.C * (1 - item.K) + item.K),
                    M = (item.M * (1 - item.K) + item.K),
                    Y = (item.Y * (1 - item.K) + item.K)
                };

            return cmy.ToRgb();
        }

        public static IRgb ToColor(ICmyk item, Uri profile)
        {
            var points = new[] { (float)item.C, (float)item.M, (float)item.Y, (float)item.K };
            var color = Color.FromValues(points, profile);
            return new Rgb
                {
                    R = color.R,
                    G = color.G,
                    B = color.B
                };
        }
    }
}