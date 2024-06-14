using System.IO;
using System.Threading.Tasks;

namespace HotHot
{

    class LocalHostingUploader : IUploader, IHostingEntrance
    {
        private static string rootPath = "../Local Hosting/";
        private static string GetRealPath(string srcPath) => Path.Combine(rootPath, srcPath);

        bool IUploader.Get(string srcPath, string desPath)
        {
            var dir = Path.GetDirectoryName(desPath);
            var realSrcPath = GetRealPath(srcPath);
            try
            {
                FileHelper.MakeSureDirectoryExists(dir);
                FileHelper.Copy(realSrcPath, desPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        async Task<bool> IUploader.PutAsync(string srcPath, string desPath)
        {
            var realDesPath = GetRealPath(desPath);
            var dir = Path.GetDirectoryName(realDesPath);
            FileHelper.MakeSureDirectoryExists(dir);

            try
            {
                using (FileStream sourceStream = File.Open(srcPath, FileMode.Open))
                {
                    using (FileStream destinationStream = File.Create(Path.Combine(rootPath, desPath)))
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

        bool IUploader.Delete(string path)
        {
            var realPath = GetRealPath(path);
            try
            {
                FileHelper.CleanDirectory(realPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        void IHostingEntrance.OpenEntrance()
        {
            var path = Path.GetFullPath(rootPath);
            FileHelper.MakeSureDirectoryExists(path);
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }
}