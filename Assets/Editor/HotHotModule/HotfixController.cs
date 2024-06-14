using UnityEditor;
using UnityEngine;

namespace HotHot
{
    class HotfixController : EditorWindow
    {

        #region UI
        [MenuItem("Build Tools/HotfixController")]
        private static void ShowWindow()
        {
            BuildDllController.ShowWindow();
            BuildAssetBundleController.ShowWindow();
            VersionController.ShowWindow();
            HostingController.ShowWindow();
            GetWindow<HotfixController>("Hoftix Controller", typeof(HostingController));
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Build dll, Build ab, Generate version, then Upload"))
            {
                if (EditorUtility.DisplayDialog("", "Sure to upload hotfix?", "sure", "cancel"))
                {
                    UploadHoftix();
                }

            }
        }
        #endregion

        #region public method
        /// <summary>
        /// build dll, build ab, generate version, then upload
        /// </summary>
        private static async void UploadHoftix()
        {
            if (!BuildDllController.BuildDlls())
                return;

            if (!BuildAssetBundleController.BuildAssetBundle())
                return;

            if (!VersionController.GenerateVersionFile(out Version<ABInfo> newVersion))
                return;

            if (!await HostingController.Upload(newVersion))
                return;
        }
        #endregion
    }
}