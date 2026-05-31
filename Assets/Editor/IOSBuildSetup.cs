using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class IOSBuildSetup
{
    public static void Configure()
    {
        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Disabled);
        Debug.Log("[IOSBuildSetup] iOS configured.");

        // Attach IOSDebug to level0 and level1
        foreach (var scenePath in new[] { "Assets/Scenes/level0.unity", "Assets/Scenes/level1.unity" })
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var mySystem = GameObject.Find("mySystem");
            if (mySystem == null)
            {
                var mainCam = GameObject.Find("Main Camera");
                if (mainCam != null)
                {
                    var debug = mainCam.GetComponent<IOSDebug>();
                    if (debug == null) mainCam.AddComponent<IOSDebug>();
                    Debug.Log($"[IOSBuildSetup] IOSDebug added to Main Camera in {scenePath}");
                }
            }
            else
            {
                var debug = mySystem.GetComponent<IOSDebug>();
                if (debug == null) mySystem.AddComponent<IOSDebug>();
                Debug.Log($"[IOSBuildSetup] IOSDebug added to mySystem in {scenePath}");
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
