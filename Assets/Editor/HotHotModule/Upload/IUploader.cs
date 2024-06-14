using System.Threading.Tasks;

namespace HotHot
{
    interface IUploader
    {

        /// <summary>
        /// 获取HostingServer中版本，相当于获取版本号
        /// </summary>
        public bool Get(string srcPath, string desPath);

        public Task<bool> PutAsync(string srcPath, string desPath);
        public bool Delete(string path);
    }
}