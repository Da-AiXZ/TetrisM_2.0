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
        sb.Append($" b0={MySystem.buttonDown[0]} b1={MySystem.buttonDown[1]} b2={MySystem.buttonDown[2]}");
        sb.Append($" score={MySystem.score}");
        sb.Append($" fdRes={(Resources.Load("FallDowns") != null ? "OK" : "NULL")}");
        
        // Find all FallDown instances
        var allFd = GameObject.FindObjectsOfType<FallDown>();
        sb.Append($" fdCnt={allFd.Length}");
        if (allFd.Length > 0)
        {
            var fd = allFd[0];
            sb.Append($" fdPos=({fd.transform.position.x:F1},{fd.transform.position.y:F1},{fd.transform.position.z:F1})");
            sb.Append($" fdScl=({fd.transform.localScale.x:F2},{fd.transform.localScale.y:F2},{fd.transform.localScale.z:F2})");
            var sr = fd.GetComponent<SpriteRenderer>();
            sb.Append($" fdSr={(sr != null ? "OK" : "NULL")}");
            if (sr != null) sb.Append($" fdCol=({sr.color.r:F2},{sr.color.g:F2},{sr.color.b:F2},{sr.color.a:F2})");
        }
        
        // Camera
        var cam = Camera.main;
        if (cam != null)
            sb.Append($" camSz={cam.orthographicSize:F1} camPos=({cam.transform.position.x:F1},{cam.transform.position.y:F1})");
        
        // Score display objects (Score component looks for "Score" and "Best" Text)
        var scGo = GameObject.Find("Score");
        var scTxt = (scGo != null) ? scGo.GetComponent<UnityEngine.UI.Text>() : null;
        sb.Append($" scGo={(scGo != null ? "OK" : "NULL")}");
        if (scTxt != null) sb.Append($" scTxt=OK scTxtAct={scGo.activeSelf} scTxtEn={scTxt.enabled}");
        
        var bestGo = GameObject.Find("Best");
        sb.Append($" bestGo={(bestGo != null ? "OK" : "NULL")}");
        
        // BlocksSprite stats
        int bsLen = (MySystem.BlocksSprite != null) ? MySystem.BlocksSprite.Length : -1;
        int bsNonNull =0;
        if (MySystem.BlocksSprite != null) { for (int i =0; i < MySystem.BlocksSprite.Length; i++) if (MySystem.BlocksSprite[i] != null) bsNonNull++; }
        sb.Append($" bsLen={bsLen} bsOk={bsNonNull}");
        
        // Check FallDown_2 sprite
        var fd2s = GameObject.FindObjectsOfType<FallDown_2>();
        if (fd2s.Length > 0)
        {
            var fd2 = fd2s[0];
            sb.Append($" fd2Sr={(fd2.spriteR != null ? "OK" : "NULL")}");
            if (fd2.spriteR != null) sb.Append($" fd2Spr={(fd2.spriteR.sprite != null ? "OK" : "NULL")} fd2Id={fd2.id}");
        }
        
        return sb.ToString();
    }

    void ExecRaw(string cmd)
    {
        cmd = cmd.Trim();
        if (cmd == "START") { MySystem.isStart = false; MySystem.buttonDown[2] = 0; MySystem.ButtonDown(6); _status = "CMD:START"; return; }
        if (cmd == "STOP") { MySystem.isStart = false; MySystem.buttonDown[2] = 0; _status = "CMD:STOP"; return; }
        if (cmd.StartsWith("BTN ")) { if (int.TryParse(cmd.Substring(4), out int n)) { MySystem.ButtonDown(n); _status = $"CMD:BTN{n}"; } return; }
        _status = "CMD:?";
    }
}
