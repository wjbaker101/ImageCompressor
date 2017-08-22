using System.Threading.Tasks;

namespace ImageCompressor.Main.Compressors
{
    public interface ICompressor
    {
        Task CompressImageAsync(string sourceFile, string destinationFile);

        Task ResizeAndCompressImageAsync(string sourceFile, string destinationFile, int width, int height);
    }
}
