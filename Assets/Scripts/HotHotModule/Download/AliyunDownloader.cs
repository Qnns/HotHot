using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace HotHot
{
    class AliyunDownloader : IDownloader
    {
        private static readonly string bucketName = "";
        private static readonly string accessKeyId = "";
        private static readonly string accessKeySecret = "";
        private static readonly string endpoint = "";

        private static readonly AmazonS3Config config = new AmazonS3Config() { ServiceURL = endpoint };
        private static readonly AmazonS3Client client = new AmazonS3Client(accessKeyId, accessKeySecret, config);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task<(bool, bool)> IDownloader.ExistsAsync(string path)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return (true, true);
        }

        async Task<bool> IDownloader.GetAsync(string srcPath, string desPath)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = srcPath,
            };
            using GetObjectResponse response = await client.GetObjectAsync(request);
            try
            {
                // Save object to local file
                await response.WriteResponseStreamToFileAsync(desPath, false, CancellationToken.None);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                UnityEngine.Debug.Log(ex);
                return false;
            }
        }
    }
}