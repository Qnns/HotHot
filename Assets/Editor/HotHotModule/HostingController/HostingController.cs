using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace HotHot
{
    class HostingController : EditorWindow
    {

        #region UI
        public static void ShowWindow()
        {
            GetWindow<HostingController>("Hosting Controller", typeof(VersionController));
            Init();
        }

        private void OnGUI()
        {
            GUILayout.Label(string.Format("upload process: {0} / {1}", uploadedFile, totalUploadFile));

            if (GUILayout.Button("Open entrance (HostingServer) "))
            {
                (uploader as IHostingEntrance)?.OpenEntrance();
            }
            if (GUILayout.Button("Clear root directory of hosting"))
            {
                if (EditorUtility.DisplayDialog("", "sure to clear root directory of hosting?", "sure", "cancel"))
                {
                    Delete("");
                }
            }
            if (GUILayout.Button("Upload"))
            {
                if (EditorUtility.DisplayDialog("", "Sure to only upload?", "sure", "cancel"))
                {
                    UploadOnly();
                }

            }
        }
        #endregion

        #region public method
        public static async Task<bool> Upload(Version<ABInfo> newVersion)
        {
            totalUploadFile = 0;
            uploadedFile = 0;
            allSuccessfully = true;

            //获取上传列表，只上传改动和新增
            var waitUploadList = newVersion.abInfoList.Where(abInfo => abInfo.address == newVersion.timestamp).ToList();
            //创建上传任务
            Debug.Log("start upload".AddPrefixLine4Long());
            totalUploadFile = waitUploadList.Count() + 2;
            var taskList = new List<Task>();
            foreach (ABInfo abInfo in waitUploadList)
            {
                var temp = uploader
                                .PutAsync(srcPath(abInfo.fileName), desPath(newVersion.timestamp, abInfo.fileName))
                                .ContinueWith(tcs => OnPutFinished(tcs, abInfo.fileName));
                taskList.Add(temp);
            }

            //版本文件夹里也存放version.v，可制作回滚功能
            taskList.Add(uploader
                                .PutAsync(srcPath(VersionStaticField.versionFileName), desPath(newVersion.timestamp, VersionStaticField.versionFileName))
                                .ContinueWith(tcs => OnPutFinished(tcs, VersionStaticField.versionFileName)));
            await Task.WhenAll(taskList.ToArray());
            //根目录存放最新版本，不加到taskList中是为了防止同时读取Version.v
            await uploader
                        .PutAsync(srcPath(VersionStaticField.versionFileName), VersionStaticField.versionFileName)
                        .ContinueWith(tcs => OnPutFinished(tcs, VersionStaticField.versionFileName));
            if (allSuccessfully)
            {
                Debug.Log("end upload".AddPrefixLine4Long());
                return true;
            }
            else
            {
                Debug.Log("upload failed".AddPrefixLine4Long().ToRed());
                return false;
            }

        }

        public static void Delete(string path)
        {
            if (!uploader.Delete(path))
                throw new Exception($"delete remote {path} failed");
        }
        #endregion

        #region private
        private static void Init()
        {
            uploader = new LocalHostingUploader();
        }
        private static IUploader uploader;
        private static int totalUploadFile;
        private static int uploadedFile;
        private static bool allSuccessfully;
        private static string srcPath(string fileName) => Path.Combine(VersionController.pathForGenerationVersion, fileName);
        private static string desPath(long timestamp, string fileName) => timestamp.ToString() + "/" + fileName;
        private static void OnPutFinished(Task<bool> tcs, string fileName)
        {
            if (tcs.Result)
            {
                uploadedFile++;
                Debug.Log($"{fileName,-18} upload successfully ({uploadedFile}/{totalUploadFile})");
            }
            else
            {
                allSuccessfully = false;
                Debug.Log($"{fileName,-18} upload failed");
            }
        }

        /// <summary>
        /// only upload
        /// </summary>
        private static async void UploadOnly()
        {
            var version = VersionMethod.ReadVersionFile(Path.Combine(VersionController.pathForGenerationVersion, VersionStaticField.versionFileName));
            await Upload(version);
        }
        #endregion
    }
}