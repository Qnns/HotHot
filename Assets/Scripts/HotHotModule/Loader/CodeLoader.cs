using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HotHot
{
    public class CodeLoader
    {
        public Assembly model;
        public Assembly hotfix;

        private bool enableCodes = false;
        private bool isEditor = false;

        public void Start()
        {
            if (enableCodes)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly ass in assemblies)
                {
                    string name = ass.GetName().Name;
                    if (name == "Unity.Model.Codes")
                    {
                        this.model = ass;
                    }
                }
            }
            else
            {
                byte[] assBytes;
                byte[] pdbBytes;
                if (!isEditor)
                {
                    var path = Path.Combine(Application.persistentDataPath, "HotfixRes", "code.unity3d");

                    var asset = AssetBundle.LoadFromFile(path);
                    var dll = asset.LoadAsset("Model.dll");
                    var pdb = asset.LoadAsset("Model.pdb");
                    asset.Unload(false);

                    assBytes = ((TextAsset)dll).bytes;
                    pdbBytes = ((TextAsset)pdb).bytes;

                }
                else
                {
                    assBytes = File.ReadAllBytes(Path.Combine(PathFor.tempDllOutput, "Model.dll"));
                    pdbBytes = File.ReadAllBytes(Path.Combine(PathFor.tempDllOutput, "Model.pdb"));
                }

                this.model = Assembly.Load(assBytes, pdbBytes);
                this.LoadHotfix();
            }

        }

        // 热重载调用该方法
        public void LoadHotfix()
        {
            byte[] assBytes;
            byte[] pdbBytes;
            if (!isEditor)
            {

                var path = Path.Combine(Application.persistentDataPath, "HotfixRes", "code.unity3d");

                var asset = AssetBundle.LoadFromFile(path);
                var dll = asset.LoadAsset("Hotfix.dll");
                var pdb = asset.LoadAsset("Hotfix.pdb");
                asset.Unload(false);

                assBytes = ((TextAsset)dll).bytes;
                pdbBytes = ((TextAsset)pdb).bytes;
            }
            else
            {
                // Unity在这里搞了个优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
                string[] logicFiles = Directory.GetFiles(PathFor.tempDllOutput, "Hotfix_*.dll");
                if (logicFiles.Length != 1)
                {
                    throw new Exception("Logic dll count != 1");
                }
                string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
                assBytes = File.ReadAllBytes(Path.Combine(PathFor.tempDllOutput, $"{logicName}.dll"));
                pdbBytes = File.ReadAllBytes(Path.Combine(PathFor.tempDllOutput, $"{logicName}.pdb"));
            }

            hotfix = Assembly.Load(assBytes, pdbBytes);
        }
    }
}