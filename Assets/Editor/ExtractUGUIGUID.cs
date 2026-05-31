using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ExtractUGUIGUID
{
    public static void Extract()
    {
        var go = new GameObject("_guid_extract_");
        go.AddComponent<Text>();
        go.AddComponent<Image>();
        go.AddComponent<Button>();
        go.AddComponent<RawImage>();
        
        var dir = "Assets/Editor/";
        var path = dir + "_guid_prefab.prefab";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        bool ok;
        PrefabUtility.SaveAsPrefabAsset(go, path, out ok);
        Object.DestroyImmediate(go);
        AssetDatabase.Refresh();
        
        if (ok)
        {
            var content = File.ReadAllText(path);
            var matches = System.Text.RegularExpressions.Regex.Matches(content, 
                @"m_Script:\s*\{fileID:\s*(\d+),\s*guid:\s*([a-f0-9]+),\s*type:\s*(\d+)\}");
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                Debug.Log($"[UGUI_GUID_FULL] fileID={m.Groups[1].Value} guid={m.Groups[2].Value} type={m.Groups[3].Value}");
            }
            AssetDatabase.DeleteAsset(path);
        }
        else
        {
            Debug.LogError("[UGUI_GUID] Failed to save prefab");
        }
    }
}
