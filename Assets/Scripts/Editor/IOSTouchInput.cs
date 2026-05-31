using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class IOSTouchInput : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        string path = "Assets/Scripts/Assembly-CSharp/Title_2.cs";
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            string oldLine = "if (timmer >= time && Input.anyKey)";
            string newLine = "if (timmer >= time && (Input.anyKey || Input.touchCount > 0 || Input.GetMouseButtonDown(0)))";
            if (content.Contains(oldLine) && !content.Contains(newLine))
            {
                content = content.Replace(oldLine, newLine);
                File.WriteAllText(path, content);
                Debug.Log("IOSTouchInput: Patched Title_2.cs");
            }
        }
    }
}
