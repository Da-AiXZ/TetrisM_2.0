using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GetGUID
{
    [MenuItem("Tools/GetGUID")]
    static void Run()
    {
        var go = new GameObject("TestText");
        var t = go.AddComponent<Text>();
        t.text = "Hello";
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(go.scene, "/tmp/guid_test.unity");
        EditorApplication.Exit(0);
    }
}
