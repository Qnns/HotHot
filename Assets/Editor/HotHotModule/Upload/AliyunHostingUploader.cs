using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace HotHot
{

    public class AliyunHostingUploader : IUploader, IHostingEntrance
    {
        private static readonly string bucketName = "";
        private static readonly string accessKeyId = "";
        private static readonly string accessKeySecret = "";
        private static readonly string endpoint = "";

        private static readonly AmazonS3Config config = new AmazonS3Config() { ServiceURL = endpoint };
        private static readonly AmazonS3Client client = new AmazonS3Client(accessKeyId, accessKeySecret, config);

        public void OpenEntrance()
        {
            throw new NotImplementedException();
        }

        bool IUploader.Delete(string path)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = path,
                };
                Task.WaitAll(client.DeleteObjectAsync(deleteObjectRequest));
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return false;
            }
        }

        bool IUploader.Get(string srcPath, string desPath)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = srcPath,
            };
            using GetObjectResponse response = client.GetObjectAsync(request).Result;
            try
            {
                // Save object to local file
                Task.WaitAll(response.WriteResponseStreamToFileAsync(desPath, false, CancellationToken.None));
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return false;
            }
        }

        async Task<bool> IUploader.PutAsync(string srcPath, string desPath)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = desPath,
                FilePath = srcPath,
            };

            var response = await client.PutObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}


