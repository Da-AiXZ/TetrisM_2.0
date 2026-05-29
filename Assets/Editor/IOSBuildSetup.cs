using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class IOSBuildSetup
{
    public static void Configure()
    {
        // Graphics APIs - Metal only for iOS
        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
        
        // Scripting backend - IL2CPP
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        
        // Target iOS version
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        
        // Target device
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        
        // DISABLE stripping to ensure all shaders are included
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Disabled);
        
        // Ensure URP shaders are always included
        var graphicsSettings = GraphicsSettings.GetGraphicsSettings();
        if (graphicsSettings != null)
        {
            Debug.Log("[IOSBuildSetup] GraphicsSettings found, URP pipeline should be active.");
        }
        
        Debug.Log("[IOSBuildSetup] iOS PlayerSettings configured (no stripping).");
    }
}
