using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BootDiag : MonoBehaviour
{
    private const string URL = "https://gale-details-mentioned-figures.trycloudflare.com/poll";
    private const float POLL_INTERVAL = 2f;
    private string _status = "INIT";
    private float _timer = 0f;
    private GUIStyle _guiStyle;
    private int _frameCount = 0;

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

    void Update()
    {
        _frameCount++;
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
                    {
                        string result = GlobalCmd.Exec(resp.Substring(4));
                        _status = result;
                    }
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
        sb.Append($" {TouchInputModule.lastDiag}");
        sb.Append($" score={MySystem.score}");

        // Canvas and scaler
        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
                sb.Append($" sclM={scaler.matchWidthOrHeight:F3} sclR=({scaler.referenceResolution.x:F0},{scaler.referenceResolution.y:F0})");
            sb.Append($" camSz={canvas.worldCamera?.orthographicSize:F1}");
        }

        // FallDown count
        var allFd = UnityEngine.Object.FindObjectsOfType<FallDown>();
        sb.Append($" fdCnt={allFd.Length}");

        // BlocksSprite
        int bsLen = (MySystem.BlocksSprite != null) ? MySystem.BlocksSprite.Length : -1;
        int bsOk = 0;
        if (MySystem.BlocksSprite != null)
            for (int i = 0; i < MySystem.BlocksSprite.Length; i++)
                if (MySystem.BlocksSprite[i] != null) bsOk++;
        sb.Append($" bsLen={bsLen} bsOk={bsOk}");

        return sb.ToString();
    }
}
