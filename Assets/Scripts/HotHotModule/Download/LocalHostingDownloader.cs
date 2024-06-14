using System.IO;
using System.Threading.Tasks;

namespace HotHot
{
    class LocalHostingDownloader : IDownloader
    {
        private static string rootPath = "../Local Hosting/";
        private static string GetRealPath(string srcPath) => Path.Combine(rootPath, srcPath);
        async Task<bool> IDownloader.GetAsync(string srcPath, string desPath)
        {
            var dir = Path.GetDirectoryName(desPath);
            FileHelper.MakeSureDirectoryExists(dir);

            try
            {
                using (FileStream sourceStream = File.Open(GetRealPath(srcPath), FileMode.Open))
                {
                    using (FileStream destinationStream = File.Create(desPath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<(bool, bool)> IDownloader.ExistsAsync(string path)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            bool isExists;
            try
            {
                isExists = File.Exists(GetRealPath(path));
                return (true, isExists);
            }
            catch
            {
                isExists = default;
                return (false, isExists);
            }

        }
    }
}