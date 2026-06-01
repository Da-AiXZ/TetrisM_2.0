using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootDiag : MonoBehaviour
{
    private const string HOST = "80.225.252.235";
    private const int PORT = 443;
    private Thread _thread;
    private TcpClient _tcp;
    private NetworkStream _stream;
    private bool _connected = false;
    private string _status = "INIT";
    private float _diagTimer = 0f;
    private readonly ConcurrentQueue<string> _rxQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<string> _txQueue = new ConcurrentQueue<string>();
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
        while (_rxQueue.TryDequeue(out string cmd))
        {
            string result = Exec(cmd);
            if (!string.IsNullOrEmpty(result))
                _txQueue.Enqueue("RESULT:" + result);
        }

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
                    throw new Exception("Connect timeout");
                _tcp.EndConnect(ar);
                _stream = _tcp.GetStream();
                _connected = true;
                _status = "CONNECTED";

                string hello = $"HELLO: scene={SceneManager.GetActiveScene().name} device={SystemInfo.deviceModel}";
                byte[] hb = Encoding.UTF8.GetBytes(hello + "\n");
                _stream.Write(hb, 0, hb.Length);

                // Wait for server handshake (OK) within 3 seconds
                _stream.ReadTimeout = 3000;
                byte[] okBuf = new byte[32];
                int okN = _stream.Read(okBuf, 0, okBuf.Length);
                if (okN <= 0 || Encoding.UTF8.GetString(okBuf, 0, okN).Trim() != "OK")
                {
                    _status = "NO_HANDSHAKE";
                    throw new Exception("No handshake from server");
                }
                _status = "CONNECTED";

                byte[] buf = new byte[8192];
                while (_tcp.Connected)
                {
                    while (_txQueue.TryDequeue(out string tx))
                    {
                        byte[] txb = Encoding.UTF8.GetBytes(tx + "\n");
                        _stream.Write(txb, 0, txb.Length);
                    }

                    _stream.ReadTimeout = 500;
                    try
                    {
                        int n = _stream.Read(buf, 0, buf.Length);
                        if (n > 0)
                        {
                            string rx = Encoding.UTF8.GetString(buf, 0, n).Trim();
                            foreach (var line in rx.Split('\n'))
                            {
                                string t = line.Trim();
                                if (t.Length > 0 && t.StartsWith("CMD:"))
                                    _rxQueue.Enqueue(t.Substring(4));
                            }
                        }
                    }
                    catch (Exception) { }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                _status = "ERR: " + ex.Message;
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
