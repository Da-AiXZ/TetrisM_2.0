using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootDiag : MonoBehaviour
{
    private const string HOST = "80.225.252.235";
    private const int PORT = 443;
    private TcpClient _tcp;
    private NetworkStream _stream;
    private Thread _thread;
    private readonly ConcurrentQueue<string> _rxQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<string> _txQueue = new ConcurrentQueue<string>();
    private float _diagTimer = 0f;
    private bool _connected;
    private string _status = "INIT";
    private GUIStyle _guiStyle;

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
        // Process incoming commands on main thread
        while (_rxQueue.TryDequeue(out string cmd))
        {
            string result = Exec(cmd);
            if (!string.IsNullOrEmpty(result))
                _txQueue.Enqueue("RESULT:" + result);
        }

        // Send diagnostics every 2 seconds
        _diagTimer += Time.deltaTime;
        if (_diagTimer >= 2f)
        {
            _diagTimer = 0f;
            _txQueue.Enqueue(Diag());
        }
    }

    void OnDestroy()
    {
        _thread?.Interrupt();
        try { _stream?.Close(); } catch { }
        try { _tcp?.Close(); } catch { }
    }

    void OnGUI()
    {
        if (_guiStyle == null) { _guiStyle = new GUIStyle(GUI.skin.label); _guiStyle.fontSize = 18; }
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, Screen.height - 40, Screen.width - 20, 40),
            $"[TCP Eye] {_status} | scene={SceneManager.GetActiveScene().name} | conn={_connected}", _guiStyle);
    }

    // ── Network (background thread) ──
    void NetworkLoop()
    {
        while (true)
        {
            try
            {
                _status = "CONNECTING";
                _tcp = new TcpClient();
                var ar = _tcp.BeginConnect(HOST, PORT, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(10000))
                    throw new Exception("Connect timeout (10s)");
                _tcp.EndConnect(ar);
                _stream = _tcp.GetStream();
                _connected = true;
                _status = "CONNECTED";

                // Send hello
                string hello = $"HELLO: scene={SceneManager.GetActiveScene().name} device={SystemInfo.deviceModel}";
                byte[] helloBytes = Encoding.UTF8.GetBytes(hello + "\n");
                _stream.Write(helloBytes, 0, helloBytes.Length);

                byte[] buf = new byte[8192];
                while (_tcp.Connected)
                {
                    // Send queued data
                    while (_txQueue.TryDequeue(out string tx))
                    {
                        byte[] txBytes = Encoding.UTF8.GetBytes(tx + "\n");
                        _stream.Write(txBytes, 0, txBytes.Length);
                    }

                    // Read with timeout
                    _stream.ReadTimeout = 500;
                    try
                    {
                        int n = _stream.Read(buf, 0, buf.Length);
                        if (n > 0)
                        {
                            string rx = Encoding.UTF8.GetString(buf, 0, n).Trim();
                            foreach (var line in rx.Split('\n'))
                            {
                                string trimmed = line.Trim();
                                if (trimmed.Length > 0 && trimmed.StartsWith("CMD:"))
                                    _rxQueue.Enqueue(trimmed.Substring(4));
                            }
                        }
                    }
                    catch (Exception) { /* timeout, ok */ }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                _status = $"ERR: {ex.Message}";
                _connected = false;
            }
            finally
            {
                try { _stream?.Close(); } catch { }
                try { _tcp?.Close(); } catch { }
                _connected = false;
            }
            _status = "RECONNECT_5s";
            Thread.Sleep(5000);
        }
    }

    // ── Diagnostics ──
    string Diag()
    {
        var sb = new StringBuilder("DATA:");
        sb.Append($" scene={SceneManager.GetActiveScene().name}");
        sb.Append($" roots={SceneManager.GetActiveScene().rootCount}");
        sb.Append($" isStart={MySystem.isStart}");
        sb.Append($" isPhone={MySystem.isPhone}");
        sb.Append($" touches={Input.touchCount}");

        var es = EventSystem.current;
        if (es != null)
        {
            var sim = es.GetComponent<StandaloneInputModule>();
            sb.Append($" ES=ON mod={(sim != null ? "Standalone" : "?")} enabled={es.enabled}");
        }
        else sb.Append(" ES=NULL");

        // Button states
        string[] btnNames = { "key1_down", "key1_right", "key1_left", "key3", "key2", "key1_up" };
        foreach (var name in btnNames)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                var btn = go.GetComponent<Button>();
                var img = go.GetComponent<RawImage>();
                var rt = go.GetComponent<RectTransform>();
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

    // ── Command execution (main thread) ──
    string Exec(string cmd)
    {
        try
        {
            var parts = cmd.Split(':');
            if (parts.Length < 2) return "BAD_CMD";
            var op = parts[0];
            var arg = cmd.Substring(op.Length + 1);

            if (op == "GET")
            {
                var dot = arg.LastIndexOf('.');
                if (dot < 0) return "BAD_GET";
                var goName = arg.Substring(0, dot);
                var field = arg.Substring(dot + 1);

                // Try static first
                var t = Type.GetType("MySystem, Assembly-CSharp");
                if (t != null)
                {
                    var fi = t.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (fi != null) return field + "=" + (fi.GetValue(null) ?? "null");
                    var pi = t.GetProperty(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (pi != null) return field + "=" + (pi.GetValue(null) ?? "null");
                }

                // Try GameObject
                var go = GameObject.Find(goName);
                if (go == null) return goName + "=NULL";
                if (field == "activeSelf") return "activeSelf=" + go.activeSelf;
                if (field == "layer") return "layer=" + go.layer;

                var comps = go.GetComponents<Component>();
                foreach (var c in comps)
                {
                    if (c == null) continue;
                    var fi2 = c.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fi2 != null) return c.GetType().Name + "." + field + "=" + (fi2.GetValue(c) ?? "null");
                    var pi2 = c.GetType().GetProperty(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (pi2 != null) return c.GetType().Name + "." + field + "=" + (pi2.GetValue(c) ?? "null");
                }
                return field + "=NOT_FOUND";
            }

            if (op == "EXEC")
            {
                var sp = arg.Split(' ');
                if (sp.Length < 2) return "BAD_EXEC";
                var t2 = Type.GetType("MySystem, Assembly-CSharp");
                if (t2 == null) return "TYPE_NOT_FOUND";
                var mi = t2.GetMethod(sp[0], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (mi == null) return "METHOD_NOT_FOUND";
                var p = mi.GetParameters();
                if (p.Length == 1 && p[0].ParameterType == typeof(int))
                    mi.Invoke(null, new object[] { int.Parse(sp[1]) });
                else
                    mi.Invoke(null, null);
                return "OK";
            }

            if (op == "SET")
            {
                var sp = arg.Split(' ');
                if (sp.Length < 2) return "BAD_SET";
                var dot = sp[0].LastIndexOf('.');
                var goName2 = sp[0].Substring(0, dot);
                var field2 = sp[0].Substring(dot + 1);
                var val = sp[1];

                var t3 = Type.GetType("MySystem, Assembly-CSharp");
                if (t3 != null)
                {
                    var fi = t3.GetField(field2, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (fi != null)
                    {
                        if (fi.FieldType == typeof(bool)) fi.SetValue(null, bool.Parse(val));
                        else if (fi.FieldType == typeof(int)) fi.SetValue(null, int.Parse(val));
                        else fi.SetValue(null, val);
                        return $"SET {field2}={val}";
                    }
                }
                return "SET_FAILED";
            }

            if (op == "FIND")
            {
                var go2 = GameObject.Find(arg);
                if (go2 == null) return "NULL";
                return go2.name + " active=" + go2.activeSelf + " layer=" + go2.layer;
            }

            if (op == "SCENE") return SceneManager.GetActiveScene().name + " roots=" + SceneManager.GetActiveScene().rootCount;
            if (op == "TOUCHES")
            {
                var sb = new StringBuilder($"count={Input.touchCount}");
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    sb.Append($" [{i}]:{t.phase}@{t.position}");
                }
                return sb.ToString();
            }
            if (op == "EVENTSYS")
            {
                var es2 = EventSystem.current;
                if (es2 == null) return "NULL";
                var sim2 = es2.GetComponent<StandaloneInputModule>();
                return $"enabled={es2.enabled} input={(sim2!=null?"Standalone":"?")}";
            }
            if (op == "CANVAS")
            {
                var cvs = FindObjectsOfType<Canvas>();
                var sb = new StringBuilder();
                foreach (var cv in cvs)
                    sb.Append($"[{cv.name} mode={cv.renderMode} enabled={cv.enabled} GR={cv.GetComponent<GraphicRaycaster>()!=null}] ");
                return sb.Length > 0 ? sb.ToString() : "NO_CANVAS";
            }

            return "UNKNOWN_OP: " + op;
        }
        catch (Exception ex)
        {
            return "ERR: " + ex.Message;
        }
    }
}
