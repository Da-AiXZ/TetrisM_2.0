using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.Linq;

public class UIFixer : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
    {
        var allGOs = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
            .Select(t => t.gameObject);

        int fixedCount = 0;
        foreach (var go in allGOs)
        {
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null)
                {
                    // Missing component found - try to identify from serialized data
                    var so = new SerializedObject(go);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.name == "m_Script" && prop.propertyType == SerializedPropertyType.Generic)
                        {
                            // We can't easily read the original GUID here
                            // Instead, look at other properties to guess the type
                            Debug.Log($"[UIFixer] Found missing component on {go.name}, path: {prop.propertyPath}");
                        }
                    }
                    so.Dispose();
                }
            }
        }
        Debug.Log($"[UIFixer] Processed scene {scene.name}, found {fixedCount} missing UI components");
    }
}
