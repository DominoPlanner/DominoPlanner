using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core.ColorMine.Comparisons
{
    public abstract class ColorDetectionMode
    {
        // singleton
        private static CieDe2000Comparison comp1;
        public static IColorSpaceComparison CieDe2000Comparison
        {
            get
            {
                if (comp1 == null)
                {
                    comp1 = new CieDe2000Comparison();
                }
                return comp1;
            }
        }
        private static CmcComparison comp2;
        public static IColorSpaceComparison CmcComparison
        {
            get
            {
                if (comp2 == null)
                {
                    comp2 = new CmcComparison();
                }
                return comp2;
            }
        }
        private static Cie1976Comparison comp3;
        public static IColorSpaceComparison Cie1976Comparison
        {
            get
            {
                if (comp3 == null)
                {
                    comp3 = new Cie1976Comparison();
                }
                return comp3;
            }
        }
        private static Cie94Comparison comp4;
        public static IColorSpaceComparison Cie94Comparison
        {
            get
            {
                if (comp4 == null)
                {
                    comp4 = new Cie94Comparison();
                }
                return comp4;
            }
        }
    }
}
