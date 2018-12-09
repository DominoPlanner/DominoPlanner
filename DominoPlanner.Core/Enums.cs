namespace DominoPlanner.Core
{
    public enum DitherMode
    {
        NoDithering,
        FloydSteinberg,
        JarvisJudiceNinke,
        Stucki
    }
    public enum ColorComparisonMode
    {
        Cie76,
        CmcComparison,
        Cie94,
        Cie2000
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
