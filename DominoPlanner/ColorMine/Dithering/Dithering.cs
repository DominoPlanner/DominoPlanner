using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System.Windows.Forms;
using DominoPlanner;
using System.Windows.Media.Imaging;

namespace Dominorechner_V2.ColorMine
{
    public abstract class Dithering
    {
        public abstract int[,] Dither(WriteableBitmap input, IColorSpaceComparison comparison, float treshold = Int32.MaxValue);
        public static int Compare(Lab a, IColorSpaceComparison comp, List<Lab> cols, float threshold = Int32.MaxValue)
        {
            int Minimum = 0;

            double min = comp.Compare(a, cols[0]);
            double temp = Int32.MaxValue;
            for (int z = 1; z < cols.Count; z++)
            {
                temp = comp.Compare(cols[z], a);
                if (min > temp)
                {
                    min = temp;
                    Minimum = z;
                }
            }
            
            //if (min > threshold) MessageBox.Show("Der Grenzwert von " + threshold + "wurde überschritten (" + min + "). \nFarbwert im Originalbild: (" + original.R + "; " + original.G + "; " + original.B + "), "
            //    + "\nNächste Farbe: Farbname: " + farben.Einstellung[Minimum].Farbname + ", Farbwert: (" + farben.Einstellung[Minimum].RGBwert.R + "; " + farben.Einstellung[Minimum].RGBwert.G + "; " +
            //    farben.Einstellung[Minimum].RGBwert.B + ").");
            return Minimum;
            
            
        }
    }
}
