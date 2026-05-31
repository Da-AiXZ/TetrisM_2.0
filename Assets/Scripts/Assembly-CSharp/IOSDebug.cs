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
            
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
                _info += $"Scaler: {scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight}\n";

            var texts = canvas.GetComponentsInChildren<Text>(true);
            _info += $"Text components: {texts.Length}\n";
            foreach (var t in texts)
            {
                _info += $"  Text '{t.name}' font={t.font?.name ?? "NULL"} size={t.fontSize} color={t.color} txt='{t.text}' en={t.enabled}\n";
                if (_info.Length > 1000) break;
            }

            var imgs = canvas.GetComponentsInChildren<Image>(true);
            _info += $"Image components: {imgs.Length}\n";
            foreach (var img in imgs)
            {
                var rt = img.GetComponent<RectTransform>();
                _info += $"  Img '{img.name}' sprite={img.sprite?.name ?? "NULL"} pos={rt.anchoredPosition} size={rt.sizeDelta} en={img.enabled}\n";
                if (_info.Length > 1200) break;
            }

            var raws = canvas.GetComponentsInChildren<RawImage>(true);
            _info += $"RawImage components: {raws.Length}\n";
            
            // Check each child for null components
            _info += $"Child details:\n";
            foreach (Transform child in canvas.transform)
            {
                var comps = child.GetComponents<Component>();
                int nullCount = 0;
                foreach (var comp in comps) if (comp == null) nullCount++;
                var rt = child.GetComponent<RectTransform>();
                _info += $"  {child.name}: comps={comps.Length} nullComps={nullCount} pos={rt.anchoredPosition} size={rt.sizeDelta}\n";
                if (_info.Length > 1800) break;
            }
        }

        _info += $"start found: {start != null}\n";
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
        style.fontSize = 28;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(5, 5, 800, 2000), _info, style);
    }
}
