using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIFixer : MonoBehaviour
{
	void Start()
	{
		Invoke("Fix", 0.5f);
	}

	void Fix()
	{
		var canvas = GameObject.FindObjectOfType<Canvas>();
		if (canvas == null) return;

		var buttons = canvas.GetComponentsInChildren<Button>(true);
		foreach (var btn in buttons)
		{
			var rt = btn.transform as RectTransform;
			if (rt == null) continue;

			// Ensure Image component
			var img = btn.GetComponent<Image>();
			if (img == null)
			{
				img = btn.gameObject.AddComponent<Image>();
				img.color = new Color(1, 1, 1, 0.3f); // semi-transparent for debugging
			}
			img.raycastTarget = true;

			// Reset anchors to stretch
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.pivot = new Vector2(0.5f, 0.5f);

			// Rebuild layout based on button name
			string name = btn.name.ToLower();

			if (name.Contains("key1") && name.Contains("down"))
			{
				// A button / Start — bottom center, large
				rt.anchorMin = new Vector2(0.3f, 0.05f);
				rt.anchorMax = new Vector2(0.7f, 0.25f);
				Debug.Log($"UIFixer: {btn.name} -> A-button bottom center");
			}
			else if (name.Contains("key1") && name.Contains("left"))
			{
				// Left arrow — bottom-left
				rt.anchorMin = new Vector2(0.05f, 0.15f);
				rt.anchorMax = new Vector2(0.22f, 0.35f);
				Debug.Log($"UIFixer: {btn.name} -> left arrow");
			}
			else if (name.Contains("key1") && name.Contains("right"))
			{
				// Right arrow — bottom-left area
				rt.anchorMin = new Vector2(0.24f, 0.15f);
				rt.anchorMax = new Vector2(0.41f, 0.35f);
				Debug.Log($"UIFixer: {btn.name} -> right arrow");
			}
			else if (name.Contains("key1") && name.Contains("up"))
			{
				// Up arrow — above left/right
				rt.anchorMin = new Vector2(0.145f, 0.36f);
				rt.anchorMax = new Vector2(0.315f, 0.56f);
				Debug.Log($"UIFixer: {btn.name} -> up arrow");
			}
			else if (name.Contains("key2"))
			{
				// B button — bottom-right
				rt.anchorMin = new Vector2(0.75f, 0.10f);
				rt.anchorMax = new Vector2(0.95f, 0.35f);
				Debug.Log($"UIFixer: {btn.name} -> B-button right");
			}
			else if (name.Contains("key3"))
			{
				// C button or third action button
				rt.anchorMin = new Vector2(0.55f, 0.10f);
				rt.anchorMax = new Vector2(0.73f, 0.35f);
				Debug.Log($"UIFixer: {btn.name} -> key3 mid-right");
			}
			else
			{
				// Generic fallback
				rt.anchorMin = new Vector2(0.4f, 0.4f);
				rt.anchorMax = new Vector2(0.6f, 0.6f);
			}

			// Zero out offset (anchors-only positioning)
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
		}

		Canvas.ForceUpdateCanvases();
		Debug.Log($"UIFixer: fixed {buttons.Length} buttons");
	}
}
