using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ExtractUGUIGUID
{
    public static void Extract()
    {
        // Test each component separately
        TestComponent<Text>("Text");
        TestComponent<Image>("Image");
        TestComponent<RawImage>("RawImage");
        // Button is ambiguous - skip
    }
    
    static void TestComponent<T>(string name) where T : Component
    {
        var go = new GameObject("_probe_" + name);
        var comp = go.AddComponent<T>();
        var dir = "Assets/Editor/";
        var path = dir + "_probe_" + name + ".prefab";
        bool ok;
        PrefabUtility.SaveAsPrefabAsset(go, path, out ok);
        Object.DestroyImmediate(go);
        AssetDatabase.Refresh();
        
        if (ok)
        {
            var content = File.ReadAllText(path);
            var match = System.Text.RegularExpressions.Regex.Match(content, 
                @"m_Script:\s*\{fileID:\s*([\d-]+),\s*guid:\s*([a-f0-9]+),\s*type:\s*(\d+)\}");
            if (match.Success)
                Debug.Log($"[GUID_{name}] fileID={match.Groups[1].Value} guid={match.Groups[2].Value}");
            else
                Debug.LogWarning($"[GUID_{name}] No m_Script found in prefab");
            AssetDatabase.DeleteAsset(path);
        }
        else
        {
            Debug.LogError($"[GUID_{name}] Failed to save prefab");
        }
    }
}
