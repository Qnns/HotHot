using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace HotHot
{
    class BuildDllController : EditorWindow
    {
        #region Asset
        private static BuildDllControllerData buildDllControllerDate;
        private static void InitAsset()
        {
            BuildDllControllerData date;
            if (File.Exists(BuildDllControllerData.filePath))
            {
                date = AssetDatabase.LoadAssetAtPath<BuildDllControllerData>(BuildDllControllerData.filePath);
            }
            else
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance<BuildDllControllerData>();
                AssetDatabase.CreateAsset(scriptableObject, BuildDllControllerData.filePath);
                date = scriptableObject as BuildDllControllerData;
            }
            buildDllControllerDate = date;
            currentCodeOptimization = date.codeOptimization;

        }
        #endregion

        #region UI

        private static CodeOptimization currentCodeOptimization;
        public static CodeOptimization CurrentCodeOptimization
        {
            get
            {
                return currentCodeOptimization;
            }
            set
            {
                buildDllControllerDate.codeOptimization = value;
                currentCodeOptimization = value;
            }
        }

        public static BuildDllController ShowWindow()
        {
            var ret = GetWindow<BuildDllController>("BuildDll Controller");
            InitAsset();
            return ret;
        }

        private void OnGUI()
        {
            CurrentCodeOptimization = (CodeOptimization)EditorGUILayout.EnumPopup("CodeOptimization", CurrentCodeOptimization);

            if (GUILayout.Button("Open directory (temp position of dll)"))
            {
                System.Diagnostics.Process.Start("explorer.exe", Path.GetFullPath(PathFor.tempDllOutput));
            }
            if (GUILayout.Button("Generate dlls"))
            {
                BuildDlls();
            }
        }

        #endregion

        #region public method
        public static bool BuildDlls()
        {
            if (!BuildModel(CurrentCodeOptimization))
                return false;
            if (!BuildHotfix(CurrentCodeOptimization))
                return false;
            return true;
        }
        #endregion

        #region private
        private static bool BuildModel(CodeOptimization codeOptimization)
        {
            List<string> codes;
            codes = new List<string>()
            {
                "Assets/Scripts/Codes/Model/",
                "Assets/Scripts/Codes/ModelView/",
            };

            if (!BuildDllController.BuildMuteAssembly("Model", codes, Array.Empty<string>(), codeOptimization))
                return false;

            FileHelper.Copy(Path.Combine(PathFor.tempDllOutput, $"Model.dll"), Path.Combine(PathFor.codeBundles, $"Model.dll.bytes"));
            FileHelper.Copy(Path.Combine(PathFor.tempDllOutput, $"Model.pdb"), Path.Combine(PathFor.codeBundles, $"Model.pdb.bytes"));

            return true;
        }

        private static bool BuildHotfix(CodeOptimization codeOptimization)
        {
            string[] logicFiles = Directory.GetFiles(PathFor.tempDllOutput, "Hotfix_*");
            foreach (string file in logicFiles)
            {
                File.Delete(file);
            }

            System.Random random = new System.Random();
            int randomNum = random.Next(100000000, 999999999);
            string logicFile = $"Hotfix_{randomNum}";

            List<string> codes;
            codes = new List<string>()
            {
                "Assets/Scripts/Codes/Hotfix/",
                "Assets/Scripts/Codes/HotfixView/",
            };


            if (!BuildDllController.BuildMuteAssembly("Hotfix", codes, Array.Empty<string>(), codeOptimization))
                return false;

            FileHelper.Copy(Path.Combine(PathFor.tempDllOutput, "Hotfix.dll"), Path.Combine(PathFor.codeBundles, $"Hotfix.dll.bytes"));
            FileHelper.Copy(Path.Combine(PathFor.tempDllOutput, "Hotfix.pdb"), Path.Combine(PathFor.codeBundles, $"Hotfix.pdb.bytes"));
            FileHelper.Copy(Path.Combine(PathFor.tempDllOutput, "Hotfix.dll"), Path.Combine(PathFor.tempDllOutput, $"{logicFile}.dll"));
            FileHelper.Copy(Path.Combine(PathFor.tempDllOutput, "Hotfix.pdb"), Path.Combine(PathFor.tempDllOutput, $"{logicFile}.pdb"));

            return true;
        }

        private static bool BuildMuteAssembly(string assemblyName, List<string> CodeDirectorys, string[] additionalReferences, CodeOptimization codeOptimization)
        {
            FileHelper.MakeSureDirectoryExists(PathFor.tempDllOutput);

            List<string> scripts = new List<string>();
            for (int i = 0; i < CodeDirectorys.Count; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(CodeDirectorys[i]);
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    scripts.Add(fileInfos[j].FullName);
                }
            }

            string dllPath = Path.Combine(PathFor.tempDllOutput, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(PathFor.tempDllOutput, $"{assemblyName}.pdb");
            File.Delete(dllPath);
            File.Delete(pdbPath);

            bool isSuccessfully = true;


            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(dllPath, scripts.ToArray());

            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            assemblyBuilder.compilerOptions.AllowUnsafeCode = true;
            assemblyBuilder.compilerOptions.CodeOptimization = codeOptimization;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            assemblyBuilder.additionalReferences = additionalReferences;
            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;
            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;
            assemblyBuilder.buildTargetGroup = buildTargetGroup;
            assemblyBuilder.buildFinished += (assemblyPath, compilerMessages) =>
            {
                int errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Warnings: {0} - Errors: {1}".AddPrefixLine4Short(), warningCount, errorCount);

                if (warningCount > 0)
                {
                    Debug.LogFormat("有{0}个Warning!!!".AddPrefixLine4Short().ToRed(), warningCount);
                }

                if (errorCount > 0)
                {
                    for (int i = 0; i < compilerMessages.Length; i++)
                    {
                        if (compilerMessages[i].type == CompilerMessageType.Error)
                        {
                            string filename = Path.GetFullPath(compilerMessages[i].file);
                            Debug.Log(
                                $"{compilerMessages[i].message} (at <a href=\"file:///{filename}/\" line=\"{compilerMessages[i].line}\">{Path.GetFileName(filename)}</a>)".AddPrefixLine4Short().ToRed());
                        }
                    }
                }

                isSuccessfully = (errorCount > 0 || warningCount > 0) ? false : true;
            };

            Debug.Log("start build dll".AddPrefixLine4Long());
            if (!assemblyBuilder.Build())
            {
                Debug.Log("build dll failed".AddPrefixLine4Long());
                return false;
            }

            while (EditorApplication.isCompiling)
            {
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1);
            }

            if (isSuccessfully)
            {
                Debug.Log("end build dll".AddPrefixLine4Long());
                return true;
            }
            else
            {
                Debug.Log("build dll failed".AddPrefixLine4Long().ToRed());
                return false;
            }
        }

        #endregion
    }
}