using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ExtractUGUIGUID
{
    public static void Extract()
    {
        ExtractOne("Text", (go) => go.AddComponent<Text>());
        ExtractOne("Image", (go) => go.AddComponent<Image>());
        ExtractOne("RawImage", (go) => go.AddComponent<RawImage>());
        ExtractOne("CanvasScaler", (go) => go.AddComponent<CanvasScaler>());
        ExtractOne("GraphicRaycaster", (go) => go.AddComponent<GraphicRaycaster>());
    }
    static void ExtractOne(string name, System.Action<GameObject> add)
    {
        var go = new GameObject("_p_" + name); add(go);
        var p = "Assets/Editor/_p_" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, p, out bool ok);
        Object.DestroyImmediate(go); AssetDatabase.Refresh();
        if (ok) { foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(p), @"m_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]+)")) Debug.Log($"[PROBE] {name}: fid={m.Groups[1].Value} g={m.Groups[2].Value}"); AssetDatabase.DeleteAsset(p); }
    }
}
