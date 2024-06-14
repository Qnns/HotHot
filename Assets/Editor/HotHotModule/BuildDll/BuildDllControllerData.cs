using System;
using System.IO;
using UnityEditor.Compilation;
using UnityEngine;

namespace HotHot
{
    [Serializable]
    class BuildDllControllerData : ScriptableObject
    {

        public static readonly string path = "Assets/";
        public static readonly string fileName = "BuildDllControllerDate.asset";
        public static string filePath => Path.Combine(path, fileName);
        public CodeOptimization codeOptimization = CodeOptimization.None;

    }
}