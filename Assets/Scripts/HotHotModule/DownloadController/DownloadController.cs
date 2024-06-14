using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HotHot
{
    class DownloadController
    {
        private static IDownloader downloader;
        private static Version<ABInfo> localVersion;
        private static bool hasLocalVersion;
        private static Version<ABInfo> remoteVersion;
        private static bool hasRemoteVersion;

        public static void Init()
        {
            downloader = new LocalHostingDownloader();
        }

        private static bool SearchLocalVersion()
        {
            Debug.Log("start search local version".AddPrefixLine4Short());
            var outsideVersionFilePath = PathFor.outsideRes4VersionFile;
            var insideVersionFilePath = PathFor.insideRes4VersionFile;
            try
            {
                if (File.Exists(outsideVersionFilePath))
                {
                    localVersion = VersionMethod.ReadVersionFile(outsideVersionFilePath);
                    hasLocalVersion = true;
                }
                else if (File.Exists(insideVersionFilePath))
                {
                    localVersion = VersionMethod.ReadVersionFile(insideVersionFilePath);
                    hasLocalVersion = true;
                }
                else
                {
                    hasLocalVersion = false;
                }
                Debug.Log("end search local version".AddPrefixLine4Short());
                return true;
            }
            catch
            {
                Debug.Log("search local version failed".AddPrefixLine4Short().ToRed());
                return false;
            }
        }

        private static async Task<bool> SearchRemoteVersion()
        {
            Debug.Log("start search remote version".AddPrefixLine4Short());
            var path = PathFor.outsideRes4TempVersionFile;

            bool requestSuccessfully;
            bool isExists;
            (requestSuccessfully, isExists) = await downloader.ExistsAsync(VersionStaticField.versionFileName);

            if (!requestSuccessfully)
            {
                Debug.Log("search remote version fail".AddPrefixLine4Short().ToRed());
                return false;
            }
            if (isExists)
            {
                if (await downloader.GetAsync(VersionStaticField.versionFileName, path))
                {
                    remoteVersion = VersionMethod.ReadVersionFile(path);
                    hasRemoteVersion = true;
                }
                else
                {
                    Debug.Log("search remote version fail".AddPrefixLine4Short().ToRed());
                    return false;
                }

            }
            else
            {
                hasRemoteVersion = false;
            }

            Debug.Log("end search remote version".AddPrefixLine4Short());
            return true;

        }

        private static bool CheckIsUpdate()
        {
            Debug.Log("start check update".AddPrefixLine4Short());
            if (!hasRemoteVersion)
            {
                //远端没有版本。无需更新
                Debug.Log("远端没有版本。无需更新".AddPrefixLine4Short());
                Debug.Log("end check update".AddPrefixLine4Short());
                return false;
            }

            if (!hasLocalVersion)
            {
                //本地没有版本，远端有版本。需要更新
                Debug.Log("本地没有版本，远端有版本。需要更新".AddPrefixLine4Short());
                Debug.Log("end check update".AddPrefixLine4Short());
                return true;
            }

            if (localVersion.timestamp < remoteVersion.timestamp)
            {
                //本地版本落后远端版本。需要更新
                Debug.Log("本地版本落后远端版本。需要更新".AddPrefixLine4Short());
                Debug.Log("end check update".AddPrefixLine4Short());
                return true;
            }
            else
            {
                //本地版本等于远端版本。无需更新
                Debug.Log("本地版本等于远端版本。无需更新".AddPrefixLine4Short());
                Debug.Log("end check update".AddPrefixLine4Short());
                return false;
            }

        }

        /// <summary>
        /// 下载更新到临时文件夹
        /// </summary>
        private static async Task<bool> Download()
        {
            Debug.Log("start download files".AddPrefixLine4Short());
            var path = PathFor.oustideRes4TempHotfixRes;
            FileHelper.CleanDirectory(path);
            FileHelper.MakeSureDirectoryExists(path);

            List<ABInfo> waitDownloadList;
            if (!hasLocalVersion)
            {
                waitDownloadList = remoteVersion.abInfoList;
            }
            else
            {
                var changesList = VersionMethod.FetchChangesList(localVersion.ToRawVersion(), remoteVersion.ToRawVersion());
                waitDownloadList = changesList.Select(rawABInfo => (ABInfo)rawABInfo).ToList();
            }

            bool allSuccessfully = true;
            List<Task> taskList = new List<Task>();
            foreach (ABInfo abInfo in waitDownloadList)
            {
                var srcPath = abInfo.address.ToString() + "/" + abInfo.fileName;
                var desPath = Path.Combine(path, abInfo.fileName);
                taskList.Add(downloader.GetAsync(srcPath, desPath).ContinueWith(task => { if (!task.Result) allSuccessfully = false; }));
            }
            await Task.WhenAll(taskList);
            if (allSuccessfully)
            {
                Debug.Log("end download files".AddPrefixLine4Short());
                return true;
            }
            else
            {
                Debug.Log("download files failed".AddPrefixLine4Short().ToRed());
                return false;
            }
        }

        private static bool CheckMD5()
        {
            Debug.Log("start check md5".AddPrefixLine4Short());
            var fileList = Directory.GetFiles(PathFor.oustideRes4TempHotfixRes);
            var version = VersionMethod.ReadVersionFile(PathFor.outsideRes4TempVersionFile);
            Dictionary<string, string> md5Of = new Dictionary<string, string>();
            version.abInfoList.ForEach(abInfo => md5Of[abInfo.fileName] = abInfo.md5);
            var temp = fileList
                                .Where(filePath => MD5Helper.FileMD5(filePath) != md5Of[Path.GetFileName(filePath)])
                                .Select(filePath => $"{Path.GetFileName(filePath)} md5校验不通过".AddPrefixLine4Short())
                                .ToList();
            if (temp.Count() > 0)
            {
                //存在md5校验不通过的文件
                temp.ForEach(Debug.Log);
                Debug.Log("check md5 failed".AddPrefixLine4Short().ToRed());
                return false;
            }
            Debug.Log("end check md5".AddPrefixLine4Short());
            return true;
        }

        private static bool MoveHotfixFiles()
        {
            Debug.Log("start move hotfix files".AddPrefixLine4Short());
            var hotfixRes = PathFor.outsideRes4HotfixRes;
            var tempHotfixRes = PathFor.oustideRes4TempHotfixRes;
            try
            {
                FileHelper.MakeSureDirectoryExists(hotfixRes);
                var pathList = Directory.GetFiles(tempHotfixRes);
                foreach (string path in pathList)
                {
                    var desPath = Path.Combine(hotfixRes, Path.GetFileName(path));
                    FileHelper.Copy(path, desPath);
                }
                Debug.Log("end move hotfix files".AddPrefixLine4Short());
                return true;
            }
            catch
            {
                Debug.Log("move hotfix files failed".AddPrefixLine4Short().ToRed());
                return false;
            }
        }

        private static bool MoveVersionFile()
        {
            Debug.Log("start move version file".AddPrefixLine4Short());
            try
            {
                FileHelper.Copy(PathFor.outsideRes4TempVersionFile, PathFor.outsideRes4VersionFile);
                Debug.Log("end move version file".AddPrefixLine4Short());
                return true;
            }
            catch
            {
                Debug.Log("move version file failed".AddPrefixLine4Short().ToRed());
                return false;
            }
        }


        public static async Task<bool> Hotfix()
        {
            Debug.Log("start Hotfix".AddPrefixLine4Long().ToGreen());

            if (!SearchLocalVersion())
            {
                Debug.Log("hotfix fail".AddPrefixLine4Long().ToRed());
                return false;
            }

            if (!await SearchRemoteVersion())
            {
                Debug.Log("hotfix fail".AddPrefixLine4Long().ToRed());
                return false;
            }


            if (!CheckIsUpdate())
            {
                Debug.Log("end Hotfix".AddPrefixLine4Long().ToGreen());
                return true;
            }

            if (!await Download())
            {
                Debug.Log("hotfix fail".AddPrefixLine4Long().ToRed());
                return false;
            }

            if (!CheckMD5())
            {
                Debug.Log("hotfix fail".AddPrefixLine4Long().ToRed());
                return false;
            }

            //把更新复制到PathFor.outsideRes4HotfixRes
            if (!MoveHotfixFiles())
            {
                Debug.Log("hotfix fail".AddPrefixLine4Long().ToRed());
                return false;
            }

            if (!MoveVersionFile())
            {
                Debug.Log("hotfix fail".AddPrefixLine4Long().ToRed());
                return false;
            }


            Debug.Log("end Hotfix".AddPrefixLine4Long().ToGreen());
            return true;
        }

    }

}

