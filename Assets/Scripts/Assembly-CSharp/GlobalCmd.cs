using UnityEngine;
using UnityEngine.UI;

public static class GlobalCmd
{
	public static int diagLevel = 0;
	public static int diagFreq = 60;

	public static string Exec(string cmd)
	{
		cmd = cmd.Trim();

		// ── TOUCH ──
		if (cmd.StartsWith("TOUCH:STRAT"))
		{
			int s; if (int.TryParse(cmd.Substring(cmd.LastIndexOf(' ') + 1), out s) && s >= 0 && s <= 3)
			{ TouchInputModule.hitStrategy = s; return $"OK TOUCH:STRAT={s}"; }
			return "ERR TOUCH:STRAT 0-3";
		}
		if (cmd == "TOUCH:DIAG 0") { TouchInputModule.touchDiag = 0; return "OK TOUCH:DIAG=0 off"; }
		if (cmd == "TOUCH:DIAG 1") { TouchInputModule.touchDiag = 1; return "OK TOUCH:DIAG=1 brief"; }
		if (cmd == "TOUCH:DIAG 2") { TouchInputModule.touchDiag = 2; return "OK TOUCH:DIAG=2 full"; }

		// ── GAME ──
		if (cmd == "GAME:START") { MySystem.isStart = false; MySystem.buttonDown[2] = 0; MySystem.ButtonDown(6); return "OK GAME:START"; }
		if (cmd == "GAME:STOP") { MySystem.isStart = false; MySystem.buttonDown[2] = 0; return "OK GAME:STOP"; }
		if (cmd.StartsWith("GAME:BTN"))
		{
			int n; if (int.TryParse(cmd.Substring(cmd.LastIndexOf(' ') + 1), out n))
			{ MySystem.ButtonDown(n); return $"OK GAME:BTN={n}"; }
			return "ERR GAME:BTN N";
		}

		// ── CANVAS ──
		if (cmd.StartsWith("CANVAS:SCALE"))
		{
			float v; if (float.TryParse(cmd.Substring(cmd.LastIndexOf(' ') + 1),
				System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
			{
				v = Mathf.Clamp01(v);
				var canvas = GameObject.FindObjectOfType<Canvas>();
				if (canvas != null)
				{
					var scaler = canvas.GetComponent<CanvasScaler>();
					if (scaler != null) { scaler.matchWidthOrHeight = v; return $"OK CANVAS:SCALE={v:F3}"; }
					return "ERR no CanvasScaler";
				}
				return "ERR no Canvas";
			}
			return "ERR CANVAS:SCALE 0.00-1.00";
		}
		if (cmd == "CANVAS:DUMP")
		{
			var canvas = GameObject.FindObjectOfType<Canvas>();
			if (canvas == null) return "ERR no Canvas";
			var buttons = canvas.GetComponentsInChildren<Button>();
			var sb = new System.Text.StringBuilder("CANVAS:DUMP ");
			for (int i = 0; i < buttons.Length; i++)
			{
				var rt = buttons[i].transform as RectTransform;
				if (rt == null) continue;
				Vector3[] c = new Vector3[4]; rt.GetWorldCorners(c);
				var img = buttons[i].GetComponent<Image>();
				bool rc = img != null && img.raycastTarget;
				float a = img != null ? img.color.a : 0;
				sb.Append($"{buttons[i].name} rc={rc} a={a:F2} rect=({c[0].x:F0},{c[0].y:F0})-({c[2].x:F0},{c[2].y:F0}) | ");
			}
			return sb.ToString();
		}
		if (cmd == "CANVAS:FORCE") { Canvas.ForceUpdateCanvases(); return "OK CANVAS:FORCE"; }
		if (cmd == "CANVAS:INFO")
		{
			var canvas = GameObject.FindObjectOfType<Canvas>();
			if (canvas == null) return "ERR no Canvas";
			var scaler = canvas.GetComponent<CanvasScaler>();
			var gr = canvas.GetComponent<GraphicRaycaster>();
			return $"CANVAS:INFO mode={canvas.renderMode} cam={canvas.worldCamera?.name} scaler={(scaler!=null?$"ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight:F3}":"NULL")} graycaster={(gr!=null?"OK":"NULL")}";
		}

		// ── DIAG ──
		if (cmd.StartsWith("DIAG:FREQ"))
		{
			int f; if (int.TryParse(cmd.Substring(cmd.LastIndexOf(' ') + 1), out f) && f >= 1)
			{ diagFreq = f; return $"OK DIAG:FREQ={f}"; }
			return "ERR DIAG:FREQ N";
		}
		if (cmd == "DIAG:LEVEL 0") { diagLevel = 0; return "OK DIAG:LEVEL=0"; }
		if (cmd == "DIAG:LEVEL 1") { diagLevel = 1; return "OK DIAG:LEVEL=1"; }
		if (cmd == "DIAG:LEVEL 2") { diagLevel = 2; return "OK DIAG:LEVEL=2"; }
		if (cmd == "DIAG:FULL") { return TouchInputModule.GetFullDiag(); }

		// ── RENDER ──
		if (cmd == "RENDER:CAM")
		{
			var cams = Camera.allCameras;
			var sb = new System.Text.StringBuilder("RENDER:CAM ");
			foreach (var cam in cams)
				sb.Append($"{cam.name} ortho={cam.orthographic} size={cam.orthographicSize:F1} pos=({cam.transform.position.x:F1},{cam.transform.position.y:F1},{cam.transform.position.z:F1}) | ");
			return sb.ToString();
		}
		if (cmd == "RENDER:SPRITE")
		{
			var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
			return $"RENDER:SPRITE count={sprites.Length}";
		}

		// ── BTNCOLOR (direct button manipulation) ──
		if (cmd.StartsWith("BTNCOLOR"))
		{
			var parts = cmd.Split(' ');
			if (parts.Length >= 5)
			{
				float r, g, b, a;
				if (float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out r) &&
					float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out g) &&
					float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out b) &&
					float.TryParse(parts[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a))
				{
					var canvas = GameObject.FindObjectOfType<Canvas>();
					if (canvas != null)
					{
						var buttons = canvas.GetComponentsInChildren<Button>();
						int n = 0;
						foreach (var btn in buttons)
						{
							var img = btn.GetComponent<Image>();
							if (img != null) { img.color = new Color(r, g, b, a); n++; }
						}
						return $"OK BTNCOLOR ({r:F2},{g:F2},{b:F2},{a:F2}) x{n}";
					}
					return "ERR no Canvas";
				}
			}
			return "ERR BTNCOLOR R G B A";
		}

		// ── HELP ──
		if (cmd == "HELP")
		{
			return "TOUCH:STRAT0-3|TOUCH:DIAG0-2|GAME:START|GAME:STOP|GAME:BTN N|CANVAS:SCALE0.00-1.00|CANVAS:DUMP|CANVAS:FORCE|CANVAS:INFO|DIAG:FREQ N|DIAG:LEVEL0-2|DIAG:FULL|RENDER:CAM|BTNCOLOR R G B A";
		}

		return $"UNKNOWN:{cmd} (try HELP)";
	}
}
