namespace DominoPlanner.Core
{
    public enum DitherMode
    {
        NoDithering = 0,
        FloydSteinberg = 1,
        JarvisJudiceNinke = 2,
        Stucki = 3
    }
    public enum ColorComparisonMode
    {
        Cie76 = 0,
        CmcComparison = 1,
        Cie94 = 2,
        Cie2000 = 3
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
