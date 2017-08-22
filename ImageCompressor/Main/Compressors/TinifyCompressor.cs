using System.Threading.Tasks;
using TinifyAPI;

namespace ImageCompressor.Main.Compressors
{
    public class TinifyCompressor : ICompressor
    {
        private string ApiKey { get; set; }

        public TinifyCompressor(string key)
        {
            ApiKey = key;
        }

        public async Task CompressImageAsync(string sourceFile, string destinationFile)
        {
            Tinify.Key = this.ApiKey;

            var source = Tinify.FromFile(sourceFile);

            await source.ToFile(destinationFile);
        }

        public async Task ResizeAndCompressImageAsync(string sourceFile, string destinationFile, int width, int height)
        {
            Tinify.Key = this.ApiKey;

            var source = Tinify.FromFile(sourceFile);

            var resized = source.Resize(new
            {
                method = "cover",
                width = width,
                height = height
            });

            await resized.ToFile(destinationFile);
        }
    }
}
