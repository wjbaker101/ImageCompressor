using System.IO;
using System.Threading.Tasks;
using TinifyAPI;

namespace ImageCompressor.Main
{
    public class Compressor
    {
        private readonly string API_KEY = "Yk8o9Lni6nYdv3vmc7j5cpAalBufDc_l";

        public Compressor()
        {
            Tinify.Key = API_KEY;
        }

        public long GetCompressionCount()
        {
            return Tinify.CompressionCount;
        }

        public async Task CompressImageAsync(string fileLocation, string destinationFile)
        {
            var source = Tinify.FromFile(fileLocation);

            await source.ToFile(destinationFile);
        }

        public async Task ResizeImageAsync(string fileLocation, string destinationFile, dynamic options)
        {
            var source = Tinify.FromFile(fileLocation);

            var resized = source.Resize(new
            {
                method = options.method,
                width = options.width,
                height = options.height
            });

            await resized.ToFile(destinationFile);
        }

        #region THREAD-SAFE SINGLETON CODE

        private static Compressor instance = null;
        private static readonly object padlock = new object();

        public static Compressor Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Compressor();
                    }
                    return instance;
                }
            }
        }

        #endregion
    }
}
