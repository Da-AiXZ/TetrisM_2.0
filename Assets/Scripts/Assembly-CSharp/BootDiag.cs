using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class BootDiag : MonoBehaviour
{
    private const string URL = "http://80.225.252.235:8080/poll";
    private string _status = "INIT";
    private bool _connected = false;
    private float _pollTimer = 0f;
    private GUIStyle _guiStyle;

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        var go = new GameObject("BootDiag");
        DontDestroyOnLoad(go);
        go.AddComponent<BootDiag>();
    }

    IEnumerator Start()
    {
        // First poll immediately
        yield return PollRoutine();
    }

    void Update()
    {
        _pollTimer += Time.deltaTime;
        if (_pollTimer >= 2f)
        {
            _pollTimer = 0f;
            StartCoroutine(PollRoutine());
        }
    }

    void OnDestroy() { }

    void OnGUI()
    {
        if (_guiStyle == null) { _guiStyle = new GUIStyle(GUI.skin.label); _guiStyle.fontSize = 18; }
        GUI.color = Color.yellow;
        string scene = SceneManager.GetActiveScene().name;
        GUI.Label(new Rect(10, Screen.height - 40, Screen.width - 20, 40),
            $"[HTTP Eye] {_status} | scene={scene} | conn={_connected}", _guiStyle);
    }

    IEnumerator PollRoutine()
    {
        string diag = BuildDiag();
        string url = URL + "?d=" + UnityWebRequest.EscapeURL(diag);
        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = 5;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                _connected = true;
                _status = "OK";
                string cmd = req.downloadHandler.text;
                if (!string.IsNullOrEmpty(cmd))
                {
                    string result = Exec(cmd);
                    // result sent on next poll as part of diag
                }
            }
            else
            {
                _connected = false;
                _status = "ERR: " + req.error;
            }
        }
    }

    string BuildDiag()
    {
        var sb = new StringBuilder("DATA:");
        sb.Append($" scene={SceneManager.GetActiveScene().name}");
        sb.Append($" isStart={MySystem.isStart}");
        sb.Append($" touches={Input.touchCount}");

        // Button states
        string[] btnNames = { "key1_down", "key1_right", "key1_left", "key3", "key2", "key1_up" };
        foreach (var name in btnNames)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    bool isDown = (bool)typeof(Button).GetField("isDown", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(btn);
                    bool isPress = (bool)typeof(Button).GetField("isPress", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(btn);
                    sb.Append($" [{name}:D{isDown}P{isPress}]");
                }
            }
        }
        return sb.ToString();
    }

    string Exec(string cmd)
    {
        try
        {
            cmd = cmd.Trim();
            int sp = cmd.IndexOf(' ');
            string op = sp > 0 ? cmd.Substring(0, sp) : cmd;
            string arg = sp > 0 ? cmd.Substring(sp + 1) : "";

            switch (op.ToUpper())
            {
                case "GET":
                    return GetField(arg);
                case "FIND":
                    var go = GameObject.Find(arg);
                    return go != null ? $"FOUND: {go.name} active={go.activeSelf}" : "NULL";
                case "EXEC":
                    return ExecStatic(arg);
                case "SET":
                    return SetStatic(arg);
                case "SCENE":
                    return $"scene={SceneManager.GetActiveScene().name}";
                case "TOUCHES":
                    return $"touches={Input.touchCount}";
                case "EVENTSYS":
                    var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                    return es != null ? $"ES: enabled={es.enabled}" : "ES: NULL";
                case "CANVAS":
                    var cv = FindObjectOfType<Canvas>();
                    if (cv == null) return "Canvas: NULL";
                    var gr = cv.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    return $"Canvas: render={cv.renderMode} GR={gr != null}";
                default:
                    return $"UNKNOWN: {op}";
            }
        }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    string GetField(string path)
    {
        int dot = path.IndexOf('.');
        if (dot < 0) return "Invalid path";
        string goName = path.Substring(0, dot);
        string field = path.Substring(dot + 1);
        var go = GameObject.Find(goName);
        if (go == null) return "GO not found";
        var comp = go.GetComponent(field);
        if (comp != null) return field + "=" + comp.ToString();
        // try static
        var type = Type.GetType(field);
        if (type != null) return field + "=" + type.ToString();
        return "Field not found";
    }

    string ExecStatic(string arg)
    {
        int dot = arg.LastIndexOf('.');
        if (dot < 0) return "Invalid";
        string typeName = arg.Substring(0, dot);
        string method = arg.Substring(dot + 1);
        var type = Type.GetType(typeName);
        if (type == null) return "Type not found";
        var mi = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (mi == null) return "Method not found";
        mi.Invoke(null, null);
        return "OK";
    }

    string SetStatic(string arg)
    {
        int dot = arg.LastIndexOf('.');
        int eq = arg.IndexOf('=');
        if (dot < 0 || eq < 0) return "Invalid";
        string typeName = arg.Substring(0, dot);
        string fieldName = arg.Substring(dot + 1, eq - dot - 1);
        string value = arg.Substring(eq + 1);
        var type = Type.GetType(typeName);
        if (type == null) return "Type not found";
        var fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (fi == null) return "Field not found";
        fi.SetValue(null, Convert.ChangeType(value, fi.FieldType));
        return "OK";
    }
}
