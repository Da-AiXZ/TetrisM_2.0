using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootDiag : MonoBehaviour
{
    private const string PROXY_HOST = "192.168.190.110";
    private const int PROXY_PORT = 1082;
    private const string TARGET = "http://80.225.252.235:8088/poll";
    private const float POLL_INTERVAL = 2f;
    
    private string _status = "INIT";
    private float _timer = 0f;
    private GUIStyle _guiStyle;
    private Thread _thread;
    private volatile bool _running = true;
    private readonly List<string> _cmdQueue = new List<string>();
    private readonly object _lock = new object();
    private volatile string _cachedDiag = "";

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        var go = new GameObject("BootDiag");
        DontDestroyOnLoad(go);
        go.AddComponent<BootDiag>();
    }

    void Start()
    {
        _thread = new Thread(NetworkLoop);
        _thread.IsBackground = true;
        _thread.Start();
    }

    void Update()
    {
        _cachedDiag = Diag();
        lock (_lock)
        {
            foreach (var cmd in _cmdQueue)
                Exec(cmd);
            _cmdQueue.Clear();
        }
    }

    void OnDestroy()
    {
        _running = false;
        _thread?.Interrupt();
    }

    void OnGUI()
    {
        if (_guiStyle == null) { _guiStyle = new GUIStyle(GUI.skin.label); _guiStyle.fontSize = 18; }
        GUI.color = Color.green;
        GUI.Label(new Rect(10, Screen.height - 40, Screen.width - 20, 40),
            $"[Proxy Eye] {_status} | scene={SceneManager.GetActiveScene().name}", _guiStyle);
    }

    void NetworkLoop()
    {
        while (_running)
        {
            try
            {
                _status = "CONNECTING";
                using (var tcp = new TcpClient())
                {
                    var ar = tcp.BeginConnect(PROXY_HOST, PROXY_PORT, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(5000))
                        throw new Exception("Proxy timeout");
                    tcp.EndConnect(ar);
                    var stream = tcp.GetStream();
                    stream.ReadTimeout = 8000;
                    _status = "CONNECTED";

                    string diag = _cachedDiag;
                    string req = $"GET {TARGET}?d={Uri.EscapeDataString(diag)} HTTP/1.1\r\n" +
                                 $"Host: 80.225.252.235:8088\r\n" +
                                 "Connection: close\r\n\r\n";
                    byte[] reqBytes = Encoding.UTF8.GetBytes(req);
                    stream.Write(reqBytes, 0, reqBytes.Length);

                    // Read response
                    var sb = new StringBuilder();
                    byte[] buf = new byte[4096];
                    int total = 0;
                    while (total < 65536)
                    {
                        try
                        {
                            int n = stream.Read(buf, 0, buf.Length);
                            if (n <= 0) break;
                            sb.Append(Encoding.UTF8.GetString(buf, 0, n));
                            total += n;
                        }
                        catch { break; }
                    }

                    string resp = sb.ToString();
                    int bodyStart = resp.IndexOf("\r\n\r\n");
                    string body = bodyStart >= 0 ? resp.Substring(bodyStart + 4).Trim() : resp.Trim();
                    
                    if (!string.IsNullOrEmpty(body))
                    {
                        _status = "OK";
                        if (body.StartsWith("CMD:"))
                        {
                            lock (_lock) { _cmdQueue.Add(body.Substring(4)); }
                        }
                    }
                    else
                    {
                        _status = "EMPTY";
                    }
                }
            }
            catch (Exception ex)
            {
                _status = "ERR: " + ex.Message;
            }

            if (_running)
            {
                _status = "WAIT_" + (int)POLL_INTERVAL + "s";
                Thread.Sleep((int)(POLL_INTERVAL * 1000));
            }
        }
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
                    return go != null ? $"FOUND: {go.name}" : "NULL";
                case "EXEC": return ExecStatic(arg);
                case "SET": return SetStatic(arg);
                case "TOUCHES": return $"touches={Input.touchCount}";
                case "EVENTSYS":
                    var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                    return es != null ? $"ES: enabled={es.enabled}" : "ES: NULL";
                case "CANVAS":
                    var cv = FindObjectOfType<Canvas>();
                    return cv != null ? $"Canvas: render={cv.renderMode}" : "Canvas: NULL";
                default: return "UNKNOWN";
            }
        }
        catch (Exception ex) { return $"ERR: {ex.Message}"; }
    }

    string GetField(string path)
    {
        int dot = path.IndexOf('.');
        if (dot < 0) return "Invalid";
        string goName = path.Substring(0, dot);
        string field = path.Substring(dot + 1);
        var go = GameObject.Find(goName);
        if (go == null) return "GO null";
        var comp = go.GetComponent(field);
        return comp != null ? comp.ToString() : "comp null";
    }

    string ExecStatic(string arg)
    {
        int dot = arg.LastIndexOf('.');
        if (dot < 0) return "Invalid";
        var type = Type.GetType(arg.Substring(0, dot));
        if (type == null) return "Type null";
        var mi = type.GetMethod(arg.Substring(dot + 1), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (mi == null) return "Method null";
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
        var type = Type.GetType(fieldPath.Substring(0, dot));
        var fi = type.GetField(fieldPath.Substring(dot + 1), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        fi.SetValue(null, Convert.ChangeType(value, fi.FieldType));
        return "OK";
    }
}
