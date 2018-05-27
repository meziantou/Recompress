namespace Recompress
{
    public interface ICompressor
    {
        bool CanProcess(string path);
        void Process(string path);
    }
}
