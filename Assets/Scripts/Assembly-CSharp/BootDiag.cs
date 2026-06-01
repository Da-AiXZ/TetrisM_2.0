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
        sb.Append($" touches={Input.touchCount}"); sb.Append($" {TouchInputModule.lastDiag}");
        sb.Append($" b0={MySystem.buttonDown[0]} b1={MySystem.buttonDown[1]} b2={MySystem.buttonDown[2]}"); var scaler = UnityEngine.Object.FindObjectOfType<UnityEngine.UI.CanvasScaler>(); if (scaler != null) sb.Append($" sclMode={scaler.screenMatchMode} sclMWH={scaler.matchWidthOrHeight:F3} sclRef=({scaler.referenceResolution.x:F0},{scaler.referenceResolution.y:F0})"); else sb.Append(" scl=NULL");
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
        if (MySystem.BlocksSprite != null) {
            string sr(int i) {
                if (i >= MySystem.BlocksSprite.Length || MySystem.BlocksSprite[i] == null) return "NULL";
                var r = MySystem.BlocksSprite[i].rect;
                return $"({r.x:F0},{r.y:F0},{r.width:F0},{r.height:F0})";
            }
            sb.Append($" spR[15]={sr(15)} spR[16]={sr(16)} spR[52]={sr(52)} spR[70]={sr(70)}");
            // Check sprite texture reference
            if (MySystem.BlocksSprite[15] != null)
                sb.Append($" spTex[15]={(MySystem.BlocksSprite[15].texture != null ? "OK" : "NULL")}");
            // Check fd2 sprite texture
            var fd2all = GameObject.FindObjectsOfType<FallDown_2>();
            if (fd2all.Length >0 && fd2all[0].spriteR != null && fd2all[0].spriteR.sprite != null)
                sb.Append($" fd2Tex={(fd2all[0].spriteR.sprite.texture != null ? "OK" : "NULL")}"); for (int i =0; i < MySystem.BlocksSprite.Length; i++) if (MySystem.BlocksSprite[i] != null) bsNonNull++; }
        sb.Append($" bsLen={bsLen} bsOk={bsNonNull}");
        // Sample first few sprite names
        if (MySystem.BlocksSprite != null) {
            string sr(int i) {
                if (i >= MySystem.BlocksSprite.Length || MySystem.BlocksSprite[i] == null) return "NULL";
                var r = MySystem.BlocksSprite[i].rect;
                return $"({r.x:F0},{r.y:F0},{r.width:F0},{r.height:F0})";
            }
            sb.Append($" spR[15]={sr(15)} spR[16]={sr(16)} spR[52]={sr(52)} spR[70]={sr(70)}");
            // Check sprite texture reference
            if (MySystem.BlocksSprite[15] != null)
                sb.Append($" spTex[15]={(MySystem.BlocksSprite[15].texture != null ? "OK" : "NULL")}");
            // Check fd2 sprite texture
            var fd2all = GameObject.FindObjectsOfType<FallDown_2>();
            if (fd2all.Length >0 && fd2all[0].spriteR != null && fd2all[0].spriteR.sprite != null)
                sb.Append($" fd2Tex={(fd2all[0].spriteR.sprite.texture != null ? "OK" : "NULL")}");
            string sn(int i) => (i < MySystem.BlocksSprite.Length && MySystem.BlocksSprite[i] != null) ? MySystem.BlocksSprite[i].name : "NULL";
            sb.Append($" sp[0]={sn(0)} sp[1]={sn(1)} sp[15]={sn(15)} sp[52]={sn(52)}");
        }
        
        // Check all FallDown_2 sprites
        var fd2s = GameObject.FindObjectsOfType<FallDown_2>();
        sb.Append($" fd2Cnt={fd2s.Length}");
        for (int j =0; j < System.Math.Min(fd2s.Length,4); j++)
        {
            var fd2 = fd2s[j];
            string spName = "?";
            if (fd2.spriteR != null && fd2.spriteR.sprite != null)
                spName = fd2.spriteR.sprite.name;
            sb.Append($" [fd2_{j}:id={fd2.id} sp={spName}]");
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
