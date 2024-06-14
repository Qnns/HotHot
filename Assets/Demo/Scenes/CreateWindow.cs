using System;
using System.IO;
using HotHot;
using UnityEngine;

public class CreateWindow : MonoBehaviour
{
    // Start is called before the first frame update
    private CodeLoader codeLoader;
    void Start()
    {
        DownloadController.Init();
        codeLoader = new CodeLoader();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            DownloadController.Hotfix();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Create();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            LoadCode();
        }
    }

    private void Create()
    {
        var path = Path.Combine(Application.persistentDataPath, "HotfixRes", "window.unity3d");
        if (File.Exists(path))
        {
            var asset = AssetBundle.LoadFromFile(path);
            var prefab = (GameObject)asset.LoadAsset("window");
            var gameObject = GameObject.Instantiate(prefab);
            gameObject.GetComponent<Canvas>().worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            Debug.Log("find window.unity3d");
        }
        else
        {
            Debug.Log("cant find window.unity3d");
        }
    }

    private void LoadCode()
    {
        codeLoader.Start();
        var hotfix = codeLoader.hotfix;
        var methodInfo = hotfix.GetType("TestHotHotSystem").GetMethod("DoSomething");
        Action demoFunc = (Action)Delegate.CreateDelegate(typeof(Action), methodInfo);
        demoFunc();
    }
}
