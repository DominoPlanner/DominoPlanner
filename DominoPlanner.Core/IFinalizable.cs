namespace DominoPlanner.Core
{
    public interface IFinalizable<T> where T : DominoTransfer
    {
        T FinalizeObject();
    }
}