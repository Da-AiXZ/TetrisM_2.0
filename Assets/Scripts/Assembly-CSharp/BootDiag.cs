using System;
using System.Reflection;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BootDiag : MonoBehaviour
{
    private const string URL = "https://gale-details-mentioned-figures.trycloudflare.com/poll";
    private const float POLL_INTERVAL = 2f;
    private string _status = "INIT";
    private float _timer = 0f;
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
        StartCoroutine(PollLoop());
    }

    void OnGUI()
    {
        if (_guiStyle == null) { _guiStyle = new GUIStyle(GUI.skin.label); _guiStyle.fontSize = 18; }
        GUI.color = Color.green;
        GUI.Label(new Rect(10, Screen.height - 40, Screen.width - 20, 40),
            $"[CF Eye] {_status} | scene={SceneManager.GetActiveScene().name}", _guiStyle);
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
                req.timeout = 15;
                var op = req.SendWebRequest();
                float elapsed = 0f;
                while (!op.isDone && elapsed < 17f)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (req.result == UnityWebRequest.Result.Success)
                {
                    _status = "OK";
                    string resp = req.downloadHandler.text.Trim();
                    if (resp.StartsWith("CMD:"))
                        Exec(resp.Substring(4));
                }
                else
                {
                    _status = "ERR: " + (req.error ?? "timeout");
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

    void Exec(string cmd)
    {
        try
        {
            cmd = cmd.Trim();
            int sp = cmd.IndexOf(' ');
            string op = sp > 0 ? cmd.Substring(0, sp) : cmd;
            string arg = sp > 0 ? cmd.Substring(sp + 1) : "";

            switch (op.ToUpper())
            {
                case "EXEC":
                    int d = arg.LastIndexOf('.');
                    if (d < 0) return;
                    var t = Type.GetType(arg.Substring(0, d));
                    t?.GetMethod(arg.Substring(d + 1), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
                    break;
                case "SET":
                    int e = arg.IndexOf('=');
                    if (e < 0) return;
                    string fp = arg.Substring(0, e);
                    string v = arg.Substring(e + 1);
                    int ld = fp.LastIndexOf('.');
                    if (ld < 0) return;
                    var st = Type.GetType(fp.Substring(0, ld));
                    var fi = st?.GetField(fp.Substring(ld + 1), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (fi != null) fi.SetValue(null, Convert.ChangeType(v, fi.FieldType));
                    break;
            }
        }
        catch { }
    }
}
