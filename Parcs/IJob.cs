namespace Parcs
{
    public interface IJob
    {
        int Number { get; }
        IPoint CreatePoint(int parentNumber);
        void AddFile(string fileName);
        string FileName { get; }
        void FinishJob();
        bool IsFinished { get; }
    }
}
