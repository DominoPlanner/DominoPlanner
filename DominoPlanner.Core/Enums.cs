using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    public enum DitherMode
    {
        NoDithering,
        FloydSteinberg,
        JarvisJudiceNinke,
        Stucki
    }
    public enum Orientation
    {
        Horizontal,
        Vertical
            
    }
    public enum SummaryMode
    {
        None, Small, Large
    }
    public enum ColorMode
    {
        Normal, Intelligent, Inverted, Fixed
    }
    public enum AverageMode
    {
        Corner,
        Average
    }
}
