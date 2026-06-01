using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Reflection;

public class BootDiag : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBoot()
    {
        var go = new GameObject("BootDiag");
        DontDestroyOnLoad(go);
        go.AddComponent<BootDiag>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var cvs = Object.FindObjectsOfType<Canvas>();
        foreach (var cv in cvs)
        {
            if (cv.GetComponent<GraphicRaycaster>() == null)
                cv.gameObject.AddComponent<GraphicRaycaster>();
        }
        var allMono = Object.FindObjectsOfType<MonoBehaviour>();
        foreach (var m in allMono)
        {
            if (m == null || m.GetType().Name != "Button") continue;
            var ri = m.GetComponent<RawImage>();
            if (ri != null && ri.color.a < 0.1f)
            {
                ri.color = new Color(ri.color.r, ri.color.g, ri.color.b, 1f);
                ri.raycastTarget = true;
            }
            // Also ensure Image if no RawImage
            var img = m.GetComponent<Image>();
            if (img != null)
                img.raycastTarget = true;
        }
    }

    private static FieldInfo _fiIsDown;
    private static FieldInfo _fiIsPress;

    void OnGUI()
    {
        GUI.color = Color.yellow;
        int y = 130;
        var cam = Camera.main;
        GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] scene=" + SceneManager.GetActiveScene().name + " roots=" + SceneManager.GetActiveScene().rootCount); y+=22;
        GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] Camera.main=" + (cam != null ? cam.name + " size=" + cam.orthographicSize : "NULL")); y+=22;
        
        var cvs = Object.FindObjectsOfType<Canvas>();
        bool hasGR = cvs.Length > 0 && cvs[0].GetComponent<GraphicRaycaster>() != null;
        GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] Canvases=" + cvs.Length + " mode=" + (cvs.Length > 0 ? cvs[0].renderMode.ToString() : "?") + " GR=" + hasGR); y+=22;

        var es = Object.FindObjectOfType<EventSystem>();
        var inputMod = es != null ? es.GetComponent<StandaloneInputModule>() : null;
        GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] EventSystem=" + (es != null ? (es.enabled ? "ON" : "DISABLED") : "NULL") + " InputMod=" + (inputMod != null ? "Standalone" : "NULL")); y+=22;

        var allMono = Object.FindObjectsOfType<MonoBehaviour>();
        var topCanvas = cvs.Length > 0 ? cvs[0].transform : null;

        var sb = new System.Text.StringBuilder("[BootDiag] Buttons:");
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
            var ri = m.GetComponent<RawImage>();
            var rt = m.GetComponent<RectTransform>();
            
            // Check parent chain to Canvas
            bool isChildOfCanvas = false;
            if (topCanvas != null)
            {
                Transform t = m.transform;
                while (t != null) { if (t == topCanvas) { isChildOfCanvas = true; break; } t = t.parent; }
            }

            sb.Append("\n  ").Append(m.name).Append(" ");
            sb.Append(isD ? "D" : "-").Append(isP ? "P" : "-");
            sb.Append(ri != null ? "R" : "?");
            sb.Append(ri != null && ri.color.a > 0.5f ? "v" : "h");
            sb.Append(" child=").Append(isChildOfCanvas ? "Y" : "N");
            sb.Append(" sz=").Append(rt != null ? rt.rect.width.ToString("F0") + "x" + rt.rect.height.ToString("F0") : "?");
        }
        if (btnCount == 0) sb.Append(" NONE");
        GUI.Label(new Rect(10, y, 700, 25 * (btnCount + 1)), sb.ToString());
        y += 25 * (btnCount + 1) + 5;

        var k1 = Object.FindObjectOfType<Key1>();
        if (k1 != null)
            GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] Key1: zero=" + (k1.zero != null) + " up=" + (k1.up != null) + " down=" + (k1.down != null) + " left=" + (k1.left != null) + " right=" + (k1.right != null));
        else
            GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] Key1: NULL");
        y+=22;

        var k2 = Object.FindObjectOfType<Key2>();
        if (k2 != null)
            GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] Key2: up=" + (k2.up != null) + " down=" + (k2.down != null));
        else
            GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] Key2: NULL");
        
        y+=22;
        // REPL status
        GUI.Label(new Rect(10, y, 700, 25), "[BootDiag] REPL: polling " + (Time.frameCount % 2 == 0 ? "." : " "));
    }
}
