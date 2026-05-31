using UnityEngine;
using UnityEngine.UI;

public class IOSDebug : MonoBehaviour
{
    private string _info = "";

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        var canvas = GameObject.Find("Canvas");
        var start = GameObject.Find("start");

        _info = "=== IOSDebug ===\n";
        _info += $"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n";
        _info += $"Canvas found: {canvas != null}\n";

        if (canvas != null)
        {
            _info += $"Canvas enabled: {canvas.activeSelf}\n";
            _info += $"Canvas children: {canvas.transform.childCount}\n";

            var c = canvas.GetComponent<Canvas>();
            _info += $"Canvas comp: {(c != null ? c.renderMode.ToString() : "MISSING")}\n";

            var texts = canvas.GetComponentsInChildren<Text>(true);
            _info += $"Text components: {texts.Length}\n";
            foreach (var t in texts)
            {
                _info += $"  Text '{t.name}' font={t.font?.name ?? "NULL"} size={t.fontSize} color={t.color} txt='{t.text}' en={t.enabled}\n";
                if (_info.Length > 800) break;
            }

            var imgs = canvas.GetComponentsInChildren<Image>(true);
            _info += $"Image components: {imgs.Length}\n";

            var raws = canvas.GetComponentsInChildren<RawImage>(true);
            _info += $"RawImage components: {raws.Length}\n";
        }

        _info += $"start found: {start != null}\n";
        if (start != null)
        {
            _info += $"start active: {start.activeSelf}\n";
            _info += $"start activeInHierarchy: {start.activeInHierarchy}\n";
        }

        _info += $"isStart: {MySystem.isStart}\n";
        _info += $"gameOver: {MySystem.gameOver}\n";
        _info += $"score: {MySystem.score}\n";
        _info += $"isPhone: {MySystem.isPhone}\n";

        var es = GameObject.Find("EventSystem");
        _info += $"EventSystem: {(es != null ? es.activeSelf.ToString() : "NULL")}\n";

        _info += $"Resolution: {Screen.width}x{Screen.height}\n";
        _info += $"Camera: {Camera.main?.name ?? "NULL"}\n";
    }

    void OnGUI()
    {
        GUI.color = Color.green;
        var style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(10, 10, 600, 900), _info, style);
    }
}
