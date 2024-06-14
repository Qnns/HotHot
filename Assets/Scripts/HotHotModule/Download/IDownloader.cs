using System.Threading.Tasks;

namespace HotHot
{
    interface IDownloader
    {
        public Task<bool> GetAsync(string srcPath, string desPath);

        /// <summary>
        /// first bool: 请求是否成功；second bool: 结果
        /// </summary>
        public Task<(bool, bool)> ExistsAsync(string path);

    }
}