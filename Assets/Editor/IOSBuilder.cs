using UnityEditor;
using System.Linq;
using UnityEngine;

public class IOSBuilder
{
    public static void Build()
    {
        IOSBuildSetup.Configure();
        
        // Get scenes from Build Settings
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        
        Debug.Log($"[IOSBuilder] Building {scenes.Length} scenes for iOS...");
        
        BuildPipeline.BuildPlayer(scenes, "build/iOS", BuildTarget.iOS, BuildOptions.None);
        
        Debug.Log("[IOSBuilder] Build completed!");
    }
}
