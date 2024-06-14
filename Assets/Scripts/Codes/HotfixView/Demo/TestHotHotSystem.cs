using UnityEngine;

public class TestHotHotSystem
{

    public static void DoSomething()
    {
        GameObject.Find("window").GetComponent<Transform>().localPosition += new Vector3(0, 200, 0);

    }
}