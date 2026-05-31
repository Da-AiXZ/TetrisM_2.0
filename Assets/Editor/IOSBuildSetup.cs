using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class IOSBuildSetup : IProcessSceneWithReport
{
    public int callbackOrder => 0;
    
    static IOSBuildSetup()
    {
        // Output UGUI GUIDs at editor load time
        var textGO = new GameObject("_guidprobe_");
        var t = textGO.AddComponent<Text>();
        var i = textGO.AddComponent<Image>();
        var b = textGO.AddComponent<Button>();
        var ri = textGO.AddComponent<RawImage>();
        
        // Get GUIDs from serialized object
        var so = new SerializedObject(t);
        var scriptProp = so.FindProperty("m_Script");
        if (scriptProp != null)
        {
            var fileID = scriptProp.FindPropertyRelative("m_FileID");
            var guid = scriptProp.FindPropertyRelative("m_GUID");
            Debug.Log($"[UGUI_GUID] Text: fileID={fileID?.intValue}, guid={guid?.stringValue}");
        }
        so.Dispose();
        
        so = new SerializedObject(i);
        scriptProp = so.FindProperty("m_Script");
        if (scriptProp != null)
        {
            var guid = scriptProp.FindPropertyRelative("m_GUID");
            Debug.Log($"[UGUI_GUID] Image: guid={guid?.stringValue}");
        }
        so.Dispose();
        
        so = new SerializedObject(b);
        scriptProp = so.FindProperty("m_Script");
        if (scriptProp != null)
        {
            var guid = scriptProp.FindPropertyRelative("m_GUID");
            Debug.Log($"[UGUI_GUID] Button: guid={guid?.stringValue}");
        }
        so.Dispose();
        
        so = new SerializedObject(ri);
        scriptProp = so.FindProperty("m_Script");
        if (scriptProp != null)
        {
            var guid = scriptProp.FindPropertyRelative("m_GUID");
            Debug.Log($"[UGUI_GUID] RawImage: guid={guid?.stringValue}");
        }
        so.Dispose();
        
        Object.DestroyImmediate(textGO);
        Debug.Log("[UGUI_GUID] Probe complete");
    }
    
    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
    {
        Debug.Log($"[IOSBuildSetup] Processing scene: {scene.name}");
    }

    public static void Configure()
    {
        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Disabled);
        Debug.Log("[IOSBuildSetup] iOS configured.");
    }
}
