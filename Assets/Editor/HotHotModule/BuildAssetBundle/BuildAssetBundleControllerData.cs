using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HotHot
{
    [Serializable]
    class BuildAssetBundleControllerData : ScriptableObject
    {

        public static readonly string path = "Assets/";
        public static readonly string fileName = "BuildAssetBundleControllerDate.asset";
        public static string filePath => Path.Combine(path, fileName);

        public Platform currentPlatform = Platform.Windows;
        public BuildAssetBundleOptions currentOptions = BuildAssetBundleOptions.None;
    }
}