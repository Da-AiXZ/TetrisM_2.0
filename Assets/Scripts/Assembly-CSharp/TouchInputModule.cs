using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TouchInputModule : BaseInputModule
{
	public static string lastDiag = "INIT";
	public static int hitStrategy = 2;
	public static int touchDiag = 1;

	private PointerEventData pointerData;
	private List<RaycastResult> raycastResults = new List<RaycastResult>();
	private int processCount = 0;
	private int touchCount = 0;
	private int frameCount = 0;
	private int rayCount = 0;
	private string lastHit = "";
	private string lastTouchPos = "";
	private string stratInfo = "";
	private string touchPhase = "";

	public override void Process()
	{
		processCount++;
		frameCount++;

		if (Input.touchCount <= 0)
		{
			if (touchDiag >= 2 && frameCount % 120 == 0)
				lastDiag = $"TIM:{processCount} TOUCH:0 (idle)";
			return;
		}

		touchCount = Input.touchCount;
		var touch = Input.GetTouch(0);
		lastTouchPos = $"{touch.position.x:F0},{touch.position.y:F0}";
		touchPhase = touch.phase.ToString();

		if (pointerData == null)
			pointerData = new PointerEventData(eventSystem);

		pointerData.Reset();
		pointerData.position = touch.position;

		raycastResults.Clear();
		var canvas = GameObject.FindObjectOfType<Canvas>();
		if (canvas != null)
		{
			var buttons = canvas.GetComponentsInChildren<Button>();
			switch (hitStrategy)
			{
				case 0: HitTest_RectContains(buttons, touch, canvas); break;
				case 1: HitTest_ScreenCorners(buttons, touch, canvas); break;
				case 2: HitTest_WorldCorners(buttons, touch); break;
				case 3: HitTest_GraphicRaycaster(buttons, touch, canvas); break;
			}
			rayCount = raycastResults.Count;
		}
		else
		{
			rayCount = -1;
			stratInfo = "NoCanvas";
		}

		if (raycastResults.Count > 0)
		{
			lastHit = raycastResults[0].gameObject.name;
			var firstTarget = raycastResults[0];

			if (touch.phase == TouchPhase.Began)
			{
				pointerData.pointerPressRaycast = firstTarget;
				var handler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(firstTarget.gameObject);
				if (handler != null)
				{
					ExecuteEvents.Execute(handler, pointerData, ExecuteEvents.pointerDownHandler);
					pointerData.pointerPress = handler;
				}
			}
			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				if (pointerData.pointerPress != null)
				{
					ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
					ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
				}
				pointerData.pointerPress = null;
			}
			else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
			{
				var newHandler = (firstTarget.gameObject != null)
					? ExecuteEvents.GetEventHandler<IPointerDownHandler>(firstTarget.gameObject)
					: null;
				if (pointerData.pointerPress != null && newHandler != pointerData.pointerPress)
				{
					ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerExitHandler);
					pointerData.pointerPress = null;
				}
			}
		}
		else
		{
			lastHit = "NONE";
		}

		// Report at configured frequency
		if (touchDiag >= 1 && frameCount % GlobalCmd.diagFreq == 0)
			lastDiag = GetFullDiag();
	}

	void HitTest_RectContains(Button[] buttons, Touch touch, Canvas canvas)
	{
		stratInfo = "RectContains";
		foreach (var btn in buttons)
		{
			var rt = btn.transform as RectTransform;
			if (rt == null) continue;
			Vector2 lp;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, touch.position, canvas.worldCamera, out lp))
			{
				if (rt.rect.Contains(lp))
				{ AddHit(btn); return; }
			}
		}
	}

	void HitTest_ScreenCorners(Button[] buttons, Touch touch, Canvas canvas)
	{
		stratInfo = "ScreenCorners";
		foreach (var btn in buttons)
		{
			var rt = btn.transform as RectTransform;
			if (rt == null) continue;
			if (RectTransformUtility.RectangleContainsScreenPoint(rt, touch.position, canvas.worldCamera))
			{ AddHit(btn); return; }
		}
	}

	void HitTest_WorldCorners(Button[] buttons, Touch touch)
	{
		stratInfo = "WorldCorners";
		foreach (var btn in buttons)
		{
			var rt = btn.transform as RectTransform;
			if (rt == null) continue;
			Vector3[] c = new Vector3[4];
			rt.GetWorldCorners(c);
			if (touch.position.x >= c[0].x && touch.position.x <= c[2].x &&
			    touch.position.y >= c[0].y && touch.position.y <= c[2].y)
			{ AddHit(btn); return; }
		}
	}

	void HitTest_GraphicRaycaster(Button[] buttons, Touch touch, Canvas canvas)
	{
		var raycaster = canvas.GetComponent<GraphicRaycaster>();
		if (raycaster == null) { stratInfo = "GR(NULL)"; return; }
		var es = EventSystem.current;
		if (es == null) { stratInfo = "GR(ES=NULL)"; return; }
		stratInfo = "GR";
		var ped = new PointerEventData(es);
		ped.position = touch.position;
		var results = new List<RaycastResult>();
		raycaster.Raycast(ped, results);
		if (results.Count > 0)
			raycastResults.Add(results[0]);
	}

	void AddHit(Button btn)
	{
		var rr = new RaycastResult();
		rr.gameObject = btn.gameObject;
		rr.module = null;
		raycastResults.Add(rr);
	}

	public static string GetFullDiag()
	{
		var inst = FindObjectOfType<TouchInputModule>();
		if (inst == null) return "TIM:NULL";

		var canvas = FindObjectOfType<Canvas>();
		var buttons = canvas != null ? canvas.GetComponentsInChildren<Button>() : null;
		int bCnt = buttons != null ? buttons.Length : 0;

		var sb = new System.Text.StringBuilder();
		sb.Append($"T:{inst.processCount} tc={inst.touchCount} strat={TouchInputModule.hitStrategy}:{inst.stratInfo}");
		sb.Append($" hit={inst.lastHit} ray={inst.rayCount}");
		sb.Append($" pos=({inst.lastTouchPos}) ph={inst.touchPhase}");

		if (canvas != null)
		{
			var scaler = canvas.GetComponent<CanvasScaler>();
			if (scaler != null)
				sb.Append($" sclM={scaler.matchWidthOrHeight:F3} sclR=({scaler.referenceResolution.x:F0},{scaler.referenceResolution.y:F0})");
			sb.Append($" camSz={canvas.worldCamera?.orthographicSize:F1}");
			var gr = canvas.GetComponent<GraphicRaycaster>();
			sb.Append($" gr={(gr!=null?"OK":"NULL")}");
		}
		else sb.Append(" cnv=NULL");

		sb.Append($" btn={bCnt}");

		if (buttons != null && bCnt > 0)
		{
			for (int i = 0; i < bCnt && i < 6; i++)
			{
				var rt = buttons[i].transform as RectTransform;
				if (rt == null) continue;
				Vector3[] c = new Vector3[4];
				rt.GetWorldCorners(c);
				var img = buttons[i].GetComponent<Image>();
				float a = img != null ? img.color.a : -1;
				bool rc = img != null && img.raycastTarget;
				sb.Append($" [{buttons[i].name}] rc={rc} a={a:F2} W=({c[0].x:F0},{c[0].y:F0})-({c[2].x:F0},{c[2].y:F0})");
			}
		}

		return sb.ToString();
	}
}
