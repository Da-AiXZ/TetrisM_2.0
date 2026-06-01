using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Reflection;

public class BootDiag : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBoot()
    {
        var go = new GameObject("BootDiag");
        DontDestroyOnLoad(go);
        go.AddComponent<BootDiag>();
    }

    private static FieldInfo _fiIsDown;
    private static FieldInfo _fiIsPress;

    void OnGUI()
    {
        GUI.color = Color.yellow;
        var cam = Camera.main;
        GUI.Label(new Rect(10, 130, 700, 25), "[BootDiag] scene=" + SceneManager.GetActiveScene().name + " roots=" + SceneManager.GetActiveScene().rootCount);
        GUI.Label(new Rect(10, 155, 700, 25), "[BootDiag] Camera.main=" + (cam != null ? cam.name + " size=" + cam.orthographicSize : "NULL"));
        var cvs = FindObjectsOfType<Canvas>();
        GUI.Label(new Rect(10, 180, 700, 25), "[BootDiag] Canvases=" + cvs.Length + " " + (cvs.Length > 0 ? cvs[0].name + " mode=" + cvs[0].renderMode + " enabled=" + cvs[0].enabled : ""));

        // EventSystem check
        var es = FindObjectOfType<EventSystem>();
        GUI.Label(new Rect(10, 205, 700, 25), "[BootDiag] EventSystem=" + (es != null ? (es.enabled ? "ON" : "DISABLED") : "NULL"));

        // Button state by reflection
        var allMono = FindObjectsOfType<MonoBehaviour>();
        System.Text.StringBuilder sb = new System.Text.StringBuilder("[BootDiag] Buttons: ");
        int btnCount = 0;
        foreach (var m in allMono)
        {
            if (m == null || m.GetType().Name != "Button") continue;
            btnCount++;
            if (_fiIsDown == null)
            {
                _fiIsDown = m.GetType().GetField("isDown", BindingFlags.NonPublic | BindingFlags.Instance);
                _fiIsPress = m.GetType().GetField("isPress", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            bool isD = _fiIsDown != null && (bool)_fiIsDown.GetValue(m);
            bool isP = _fiIsPress != null && (bool)_fiIsPress.GetValue(m);
            sb.Append(m.name).Append("(");
            sb.Append(isD ? "D" : "-");
            sb.Append(isP ? "P" : "-");
            sb.Append(") ");
        }
        if (btnCount == 0) sb.Append("NONE");
        GUI.Label(new Rect(10, 230, 700, 25), sb.ToString());

        // Key1 check
        var k1 = FindObjectOfType<Key1>();
        if (k1 != null)
            GUI.Label(new Rect(10, 255, 700, 25), "[BootDiag] Key1: zero=" + (k1.zero != null) + " up=" + (k1.up != null) + " down=" + (k1.down != null) + " left=" + (k1.left != null) + " right=" + (k1.right != null));
        else
            GUI.Label(new Rect(10, 255, 700, 25), "[BootDiag] Key1: NULL");

        var k2 = FindObjectOfType<Key2>();
        if (k2 != null)
            GUI.Label(new Rect(10, 280, 700, 25), "[BootDiag] Key2: up=" + (k2.up != null) + " down=" + (k2.down != null));
        else
            GUI.Label(new Rect(10, 280, 700, 25), "[BootDiag] Key2: NULL");
    }
}
