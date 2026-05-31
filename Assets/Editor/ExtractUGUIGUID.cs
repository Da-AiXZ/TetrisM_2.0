using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ExtractUGUIGUID
{
    public static void Extract()
    {
        var dir = "Assets/Editor/";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // Create separate prefabs for each component type
        ExtractOne("Text", (go) => go.AddComponent<Text>());
        ExtractOne("Image", (go) => go.AddComponent<Image>());
        ExtractOne("RawImage", (go) => go.AddComponent<RawImage>());
        // Skip Button - project has its own Button class
    }

    static void ExtractOne(string name, System.Action<GameObject> addComp)
    {
        var go = new GameObject("_probe_" + name);
        addComp(go);
        var path = "Assets/Editor/_probe_" + name + ".prefab";
        bool ok;
        PrefabUtility.SaveAsPrefabAsset(go, path, out ok);
        Object.DestroyImmediate(go);
        AssetDatabase.Refresh();

        if (ok)
        {
            var content = File.ReadAllText(path);
            var matches = System.Text.RegularExpressions.Regex.Matches(content,
                @"m_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]+),\s*type:\s*(\d+)\}");
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                Debug.Log($"[UGUI_PROBE] {name}: fileID={m.Groups[1].Value} guid={m.Groups[2].Value} type={m.Groups[3].Value}");
            }
            AssetDatabase.DeleteAsset(path);
        }
        else
        {
            Debug.LogError($"[UGUI_PROBE] Failed to save prefab for {name}");
        }
    }
}
