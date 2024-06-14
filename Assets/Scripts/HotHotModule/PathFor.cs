using System.IO;
using UnityEngine;

namespace HotHot
{
    public static class PathFor
    {
        public static readonly string buildFormat = "../Release/{0}/StreamingAssets/";
        public static readonly string generateVersionFormat = buildFormat;

        public static readonly string codeBundles = "Assets/Bundles/Code/";

        public static readonly string tempDllOutput = "./Temp/Dll";

        //上传时从托管OSS下载最新的version.v用于比对
        public static readonly string storeHostingVersion = "./Temp/Hosting Version/Version.v";

        /// <summary>
        ///应用程序  外部  资源路径
        /// </summary>
        public static string outsideRes => Application.persistentDataPath;


        /// <summary>
        /// 应用程序  内部  资源路径
        /// </summary>
        public static string insideRes => Application.streamingAssetsPath;


        /// <summary>
        /// 应用程序内部资源路径存放路径(www/webrequest专用)
        /// </summary>
        public static string insideRes4Web
        {
            get
            {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                return $"file://{Application.streamingAssetsPath}";
#else
                return Application.streamingAssetsPath;
#endif

            }
        }

        //upload download
        public static string oustideRes4TempHotfixRes => Path.Combine(outsideRes, "TempHotfixRes/");
        public static string outsideRes4HotfixRes => Path.Combine(outsideRes, "HotfixRes/");
        public static string outsideRes4TempVersionFile => Path.Combine(outsideRes, "TempVersionFile/", VersionStaticField.versionFileName);
        public static string outsideRes4VersionFile => Path.Combine(outsideRes, "VersionFile/", VersionStaticField.versionFileName);
        public static string insideRes4VersionFile => Path.Combine(insideRes, "VersionFile/", VersionStaticField.versionFileName);



    }
}