using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
                        ExecRaw(resp.Substring(4));
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
        sb.Append($" touchSup={Input.touchSupported}");
        sb.Append($" dev={SystemInfo.deviceType}");
        sb.Append($" os={SystemInfo.operatingSystem}");
        
        var es = EventSystem.current;
        if (es != null)
        {
            sb.Append($" es={es.GetType().Name}");
            var im = es.currentInputModule;
            sb.Append($" im={(im != null ? im.GetType().Name : "null")}");
        }
        else
        {
            sb.Append(" es=null");
        }
        
        string[] btnNames = { "key1_down", "key1_right", "key1_left", "key3", "key2", "key1_up" };
        foreach (var name in btnNames)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    bool isDown = (bool)typeof(Button).GetField("isDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(btn);
                    bool isPress = (bool)typeof(Button).GetField("isPress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(btn);
                    sb.Append($" [{name}:D{isDown}P{isPress}]");
                }
            }
        }
        return sb.ToString();
    }

    void ExecRaw(string cmd)
    {
        cmd = cmd.Trim();
        // Hardcoded commands - no reflection needed
        if (cmd == "START")
        {
            MySystem.isStart = true;
            _status = "CMD:START";
            return;
        }
        if (cmd.StartsWith("BTN "))
        {
            if (int.TryParse(cmd.Substring(4), out int n))
            {
                MySystem.ButtonDown(n);
                _status = $"CMD:BTN {n}";
            }
            return;
        }
        if (cmd == "STOP")
        {
            MySystem.isStart = false;
            _status = "CMD:STOP";
            return;
        }";
            }
            return;
        }
        _status = "CMD:?";
    }
}
