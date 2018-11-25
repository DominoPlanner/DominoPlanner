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
