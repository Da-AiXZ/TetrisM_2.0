using UnityEditor;
using UnityEngine;

public class IOSBuildSetup
{
    public static void Configure()
    {
        // Graphics APIs - Metal only
        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { UnityEngine.Rendering.GraphicsDeviceType.Metal });
        
        // Scripting backend - IL2CPP
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        
        // Target iOS version
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        
        // Target device
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        
        // Strip engine code
        PlayerSettings.stripEngineCode = true;
        
        // Managed stripping
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Medium);
        
        Debug.Log("[IOSBuildSetup] iOS PlayerSettings configured.");
    }
}
