using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HotHot
{

    class VersionController : EditorWindow
    {
        #region Asset
        /// <summary>
        /// 存储VersionController类的部分字段
        /// </summary>
        private static VersionControllerData versionControllerDate;

        private static void InitAsset()
        {
            VersionControllerData date;
            if (File.Exists(VersionControllerData.filePath))
            {
                date = AssetDatabase.LoadAssetAtPath<VersionControllerData>(VersionControllerData.filePath);
            }
            else
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance<VersionControllerData>();
                AssetDatabase.CreateAsset(scriptableObject, VersionControllerData.filePath);
                date = scriptableObject as VersionControllerData;
            }
            versionControllerDate = date;
            currentVersionNumber = date.currentVersionNumber;
            versionNumberHigh = date.versionNumberHigh;
            versionNumberMid = date.versionNumberMid;
            versionNumberLow = date.versionNumberLow;

        }
        #endregion

        #region UI
        private static string currentVersionNumber;
        public static string CurrentVersionNumber
        {
            get
            {
                return currentVersionNumber;
            }
            private set
            {
                versionControllerDate.currentVersionNumber = value;
                currentVersionNumber = value;
            }
        }

        public static VersionController ShowWindow()
        {
            var ret = GetWindow<VersionController>("Version Controller", typeof(BuildAssetBundleController));
            InitAsset();
            Init();
            return ret;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("VersionNumber", CurrentVersionNumber);

            if (GUILayout.Button("Open directory (position of version generation) "))
            {
                System.Diagnostics.Process.Start("explorer.exe", Path.GetFullPath(pathForGenerationVersion));

            }

            if (GUILayout.Button("Generate version"))
            {
                GenerateVersionFile(out Version<ABInfo> newVersion);
            }
            if (GUILayout.Button("Reset version number"))
            {
                if (EditorUtility.DisplayDialog("", "Sure to reset version number?", "sure", "cancel"))
                {
                    RestartVersionNumber();
                }
            }

        }
        #endregion

        #region public Method
        public static bool GenerateVersionFile(out Version<ABInfo> newVersion)
        {
            Debug.Log("start generate version".AddPrefixLine4Long());
            if (CurrentVersionNumber == "v1.0.0")
            {
                newVersion = GenerateVersionFile4First();
                Debug.Log("end generate version".AddPrefixLine4Long());
                return true;
            }
            else
            {
                if (!uploader.Get(VersionStaticField.versionFileName, PathFor.storeHostingVersion))
                {
                    newVersion = null;
                    Debug.Log("fetch remote version failed".AddPrefixLine4Short().ToRed());
                    Debug.Log("generate version failed".AddPrefixLine4Long().ToRed());
                    return false;
                }
                newVersion = GenerateVersionFile4NotFirst(PathFor.storeHostingVersion);
                Debug.Log("end generate version".AddPrefixLine4Long());
                return true;
            }
        }
        #endregion

        #region public Field
        public static string pathForGenerationVersion => string.Format(PathFor.generateVersionFormat, currentPlatform.ToString());
        #endregion

        #region private
        /// <summary>
        /// 用于生成版本
        /// </summary>
        private static Version<ABInfo> GenerateVersionFile4First()
        {
            var rawVersion = GenerateRawVersion();
            var version = rawVersion.ToVersion();
            VersionMethod.WriteVersionFile(pathForGenerationVersion, version);
            return version;
        }

        /// <summary>
        /// 用于生成版本（非第一个版本）
        /// </summary>
        /// <param name="oldVersionPath"></param>
        private static Version<ABInfo> GenerateVersionFile4NotFirst(string oldVersionPath)
        {
            var oldVersion = VersionMethod.ReadVersionFile(oldVersionPath);
            var newVersion = GenerateRawVersion().ToVersion();
            var unchangesList = VersionMethod.FetchUnchangesList(oldVersion.ToRawVersion(), newVersion.ToRawVersion());
            foreach (ABInfo abInfo in newVersion.abInfoList)
            {
                if (unchangesList.Exists(unchange => unchange.fileName == abInfo.fileName && unchange.md5 == abInfo.md5))
                    abInfo.address = oldVersion.abInfoList.Find(oldABInfo => oldABInfo.fileName == abInfo.fileName && oldABInfo.md5 == abInfo.md5).address;//未改变的项使用原有地址
            }
            VersionMethod.WriteVersionFile(pathForGenerationVersion, newVersion);
            return newVersion;
        }

        private static void Init()
        {
            uploader = new LocalHostingUploader();
        }
        private static IUploader uploader;
        private static Platform currentPlatform => BuildAssetBundleController.CurrentPlatform;
        private static readonly string formatVersion = "v{0}.{1}.{2}";
        private static int versionNumberHigh;
        private static int VersionNumberHigh
        {
            get
            {
                return versionNumberHigh;
            }
            set
            {
                versionControllerDate.versionNumberHigh = value;
                versionNumberHigh = value;
            }
        }
        private static int versionNumberMid;
        private static int VersionNumberMid
        {
            get
            {
                return versionNumberMid;
            }
            set
            {
                versionControllerDate.versionNumberMid = value;
                versionNumberMid = value;
            }
        }
        private static int versionNumberLow;
        private static int VersionNumberLow
        {
            get
            {
                return versionNumberLow;
            }
            set
            {
                versionControllerDate.versionNumberLow = value;
                versionNumberLow = value;
            }
        }

        private void RestartVersionNumber()
        {
            VersionNumberHigh = 1;
            VersionNumberMid = 0;
            VersionNumberLow = 0;
            CurrentVersionNumber = string.Format(formatVersion, 1, 0, 0);

        }

        private static long GenerateTimestamp()
        {
            var temp = DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss");
            return Convert.ToInt64(temp);
        }

        private static Version<RawABInfo> GenerateRawVersion()
        {

            //找到所有ab包生成版本
            var fileList = FileHelper.GetAllFiles(pathForGenerationVersion, "*.unity3d");
            var timestamp = GenerateTimestamp();
            List<RawABInfo> rawABInfoList = new List<RawABInfo>();
            foreach (string filePath in fileList)
            {
                var rawABInfo = new RawABInfo();
                rawABInfo.fileName = Path.GetFileName(filePath);
                rawABInfo.md5 = MD5Helper.FileMD5(filePath);
                rawABInfoList.Add(rawABInfo);
            }
            var rawVersion = new Version<RawABInfo>();
            rawVersion.number = CurrentVersionNumber;
            rawVersion.timestamp = timestamp;
            rawVersion.abInfoList = rawABInfoList;
            //上述代码已经使用了版本号，所以使用下一个版本号，版本号+1
            CurrentVersionNumber = string.Format(formatVersion, VersionNumberHigh, VersionNumberMid, ++VersionNumberLow);
            return rawVersion;
        }

        #endregion
    }

}