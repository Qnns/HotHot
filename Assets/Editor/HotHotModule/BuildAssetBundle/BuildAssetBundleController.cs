using System.IO;
using UnityEditor;
using UnityEngine;

namespace HotHot
{
    class BuildAssetBundleController : EditorWindow
    {
        #region Asset
        private static BuildAssetBundleControllerData buildControllerDate;
        private static void InitAsset()
        {
            BuildAssetBundleControllerData date;
            if (File.Exists(BuildAssetBundleControllerData.filePath))
            {
                date = AssetDatabase.LoadAssetAtPath<BuildAssetBundleControllerData>(BuildAssetBundleControllerData.filePath);
            }
            else
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance<BuildAssetBundleControllerData>();
                AssetDatabase.CreateAsset(scriptableObject, BuildAssetBundleControllerData.filePath);
                date = scriptableObject as BuildAssetBundleControllerData;
            }
            buildControllerDate = date;
            currentPlatform = date.currentPlatform;
            currentOptions = date.currentOptions;
        }

        #endregion

        #region UI
        private static Platform currentPlatform;
        public static Platform CurrentPlatform
        {
            get
            {
                return currentPlatform;
            }
            private set
            {
                buildControllerDate.currentPlatform = value;
                currentPlatform = value;
            }
        }

        private static BuildAssetBundleOptions currentOptions;
        public static BuildAssetBundleOptions CurrentOptions
        {
            get
            {
                return currentOptions;
            }
            private set
            {
                buildControllerDate.currentOptions = value;
                currentOptions = value;
            }
        }

        public static BuildAssetBundleController ShowWindow()
        {
            var ret = GetWindow<BuildAssetBundleController>("BuildAssetBundle Controller", typeof(BuildDllController));
            InitAsset();
            return ret;
        }

        private void OnGUI()
        {
            CurrentPlatform = (Platform)EditorGUILayout.EnumPopup("Platform", CurrentPlatform);
            CurrentOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("BuildOptions", CurrentOptions);

            if (GUILayout.Button("Open directory (position of AssetBundle generation) "))
            {
                System.Diagnostics.Process.Start("explorer.exe", Path.GetFullPath(pathForBuildAB));
            }

            if (GUILayout.Button("Build AssetBundle"))
            {
                BuildAssetBundle();
            }
        }

        #endregion

        #region public method
        public static bool BuildAssetBundle()
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
            string programName = "HotHot";
            string exeName = programName;
            switch (CurrentPlatform)
            {
                case Platform.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    break;
                case Platform.Android:
                    buildTarget = BuildTarget.Android;
                    exeName += ".apk";
                    break;
                case Platform.IOS:
                    buildTarget = BuildTarget.iOS;
                    break;
                case Platform.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    break;

                case Platform.Linux:
                    buildTarget = BuildTarget.StandaloneLinux64;
                    break;
            }
            Debug.Log("start build ab".AddPrefixLine4Long());
            try
            {
                FileHelper.CleanDirectory(pathForBuildAB);
                BuildPipeline.BuildAssetBundles(pathForBuildAB, currentOptions, buildTarget);
                Debug.Log("end build ab".AddPrefixLine4Long());
                return true;
            }
            catch
            {
                Debug.Log("build ab failed".AddPrefixLine4Long().ToRed());
                return false;
            }
        }

        #endregion

        #region public field

        public static string pathForBuildAB => string.Format(PathFor.buildFormat, currentPlatform.ToString());

        #endregion
    }
}