using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BootDiag : MonoBehaviour
{
    private const string URL = "https://80.225.252.235:8443/poll";
    private const float POLL_INTERVAL = 2f;
    private string _status = "INIT";
    private float _timer = 0f;
    private GUIStyle _guiStyle;
    private readonly List<string> _cmdQueue = new List<string>();

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        var go = new GameObject("BootDiag");
        DontDestroyOnLoad(go);
        go.AddComponent<BootDiag>();
    }

    void Start()
    {
        _status = "STARTING";
        StartCoroutine(PollLoop());
    }

    void Update()
    {
        lock (_cmdQueue)
        {
            foreach (var cmd in _cmdQueue)
            {
                string result = Exec(cmd);
                // Result will be sent in next poll
            }
            _cmdQueue.Clear();
        }
    }

    void OnGUI()
    {
        if (_guiStyle == null) { _guiStyle = new GUIStyle(GUI.skin.label); _guiStyle.fontSize = 18; }
        GUI.color = Color.green;
        GUI.Label(new Rect(10, Screen.height - 40, Screen.width - 20, 40),
            $"[HTTPS Eye] {_status} | scene={SceneManager.GetActiveScene().name}", _guiStyle);
    }

    IEnumerator PollLoop()
    {
        while (true)
        {
            _status = "POLLING";
            string diag = Diag();
            string fullUrl = URL + "?d=" + UnityWebRequest.EscapeURL(diag);

            using (var req = UnityWebRequest.Get(fullUrl))
            {
                req.timeout = 10;
                req.certificateHandler = new BypassCert();

                var op = req.SendWebRequest();
                float elapsed = 0f;
                while (!op.isDone && elapsed < 12f)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (req.result == UnityWebRequest.Result.Success)
                {
                    _status = "OK";
                    string resp = req.downloadHandler.text.Trim();
                    if (resp.StartsWith("CMD:"))
                    {
                        lock (_cmdQueue) { _cmdQueue.Add(resp.Substring(4)); }
                    }
                }
                else
                {
                    _status = "ERR: " + req.error;
                }
            }

            _timer = 0f;
            while (_timer < POLL_INTERVAL)
            {
                _timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    public class BypassCert : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] cert) => true;
    }

    string Diag()
    {
        var sb = new StringBuilder("DATA:");
        sb.Append($" scene={SceneManager.GetActiveScene().name}");
        sb.Append($" isStart={MySystem.isStart}");
        sb.Append($" touches={Input.touchCount}");
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
                case "GET": return GetField(arg);
                case "FIND":
                    var go = GameObject.Find(arg);
                    return go != null ? $"FOUND: {go.name} active={go.activeSelf}" : "NULL";
                case "EXEC": return ExecStatic(arg);
                case "SET": return SetStatic(arg);
                case "SCENE": return $"scene={SceneManager.GetActiveScene().name}";
                case "TOUCHES": return $"touches={Input.touchCount}";
                case "EVENTSYS":
                    var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                    return es != null ? $"ES: enabled={es.enabled}" : "ES: NULL";
                case "CANVAS":
                    var cv = FindObjectOfType<Canvas>();
                    if (cv == null) return "Canvas: NULL";
                    var gr = cv.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    return $"Canvas: render={cv.renderMode} GR={gr != null}";
                default: return $"UNKNOWN: {op}";
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
        int eq = arg.IndexOf('=');
        if (eq < 0) return "Invalid";
        string fieldPath = arg.Substring(0, eq);
        string value = arg.Substring(eq + 1);
        int dot = fieldPath.LastIndexOf('.');
        if (dot < 0) return "Invalid";
        string typeName = fieldPath.Substring(0, dot);
        string fieldName = fieldPath.Substring(dot + 1);
        var type = Type.GetType(typeName);
        if (type == null) return "Type not found";
        var fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (fi == null) return "Field not found";
        fi.SetValue(null, Convert.ChangeType(value, fi.FieldType));
        return "OK";
    }
}
