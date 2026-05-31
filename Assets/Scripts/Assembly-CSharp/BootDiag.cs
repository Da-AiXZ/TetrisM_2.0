using UnityEngine;
using UnityEngine.SceneManagement;

public class BootDiag : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBoot()
    {
        var go = new GameObject("BootDiag");
        DontDestroyOnLoad(go);
        go.AddComponent<BootDiag>();
    }

    void OnGUI()
    {
        GUI.color = Color.yellow;
        var cam = Camera.main;
        GUI.Label(new Rect(10, 130, 700, 25), "[BootDiag] scene=" + SceneManager.GetActiveScene().name + " roots=" + SceneManager.GetActiveScene().rootCount);
        GUI.Label(new Rect(10, 155, 700, 25), "[BootDiag] Camera.main=" + (cam != null ? cam.name + " size=" + cam.orthographicSize : "NULL"));
        var cvs = FindObjectsOfType<Canvas>();
        GUI.Label(new Rect(10, 180, 700, 25), "[BootDiag] Canvases=" + cvs.Length + " " + (cvs.Length > 0 ? cvs[0].name + " mode=" + cvs[0].renderMode + " enabled=" + cvs[0].enabled : ""));
    }
}
