using System;
using System.IO;
using UnityEngine;

namespace HotHot
{
    [Serializable]
    class VersionControllerData : ScriptableObject
    {
        public static readonly string path = "Assets/";
        public static readonly string fileName = "VersionControllerDate.asset";

        public static string filePath => Path.Combine(path, fileName);

        public string currentVersionNumber = "v1.0.0";
        public int versionNumberHigh = 1;
        public int versionNumberMid = 0;
        public int versionNumberLow = 0;

    }
}