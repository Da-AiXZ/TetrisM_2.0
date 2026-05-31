#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class IOSBuildSetup : IPreprocessBuildWithReport
{
    public int callbackOrder => 10;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.iOS) return;
        
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/level1.unity", OpenSceneMode.Single);
        
        // Destroy old Canvas and create new one
        var oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null)
        {
            // Fix existing canvas instead of destroying
            var c = oldCanvas.GetComponent<Canvas>();
            if (c == null) c = oldCanvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100; // Ensure on top
            
            var scaler = oldCanvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = oldCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            var raycaster = oldCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null) oldCanvas.AddComponent<GraphicRaycaster>();
            
            Debug.Log($"IOSBuildSetup: Canvas fixed. Children: {oldCanvas.transform.childCount}");
            
            // Log all Text components
            var allText = oldCanvas.GetComponentsInChildren<Text>(true);
            Debug.Log($"IOSBuildSetup: Found {allText.Length} Text components");
            foreach (var txt in allText)
            {
                Debug.Log($"IOSBuildSetup:   Text '{txt.name}' active={txt.gameObject.activeInHierarchy} font={txt.font?.name} size={txt.fontSize} color={txt.color} text='{txt.text}'");
            }
            
            var allImages = oldCanvas.GetComponentsInChildren<Image>(true);
            Debug.Log($"IOSBuildSetup: Found {allImages.Length} Image components");
            
            var allRawImages = oldCanvas.GetComponentsInChildren<RawImage>(true);
            Debug.Log($"IOSBuildSetup: Found {allRawImages.Length} RawImage components");
        }
        else
        {
            Debug.LogError("IOSBuildSetup: No Canvas found!");
        }
        
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
