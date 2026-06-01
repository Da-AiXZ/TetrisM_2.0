using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchInputModule : BaseInputModule
{
	public static string lastDiag = "INIT";
	private PointerEventData pointerData;
	private List<RaycastResult> raycastResults = new List<RaycastResult>();
	private int processCount = 0;
	private int touchCount = 0;
	private int rayCount = 0;
	private string lastHit = "";
	private string lastTouchPos = "";

	public override void Process()
	{
		processCount++;
		if (Input.touchCount <= 0) return;
		touchCount = Input.touchCount;

		var touch = Input.GetTouch(0);
		lastTouchPos = $"{touch.position.x:F0},{touch.position.y:F0}";

		if (pointerData == null)
			pointerData = new PointerEventData(eventSystem);

		pointerData.Reset();
		pointerData.position = touch.position;

		// Direct hit test using RectangleContainsScreenPoint (handles CanvasScaler)
		raycastResults.Clear();
		var canvas = GameObject.FindObjectOfType<Canvas>();
		if (canvas != null)
		{
			var buttons = canvas.GetComponentsInChildren<Button>();
			foreach (var btn in buttons)
			{
				var rt = btn.transform as RectTransform;
				if (rt == null) continue;
				if (RectTransformUtility.RectangleContainsScreenPoint(rt, touch.position, canvas.worldCamera))
				{
					var rr = new RaycastResult();
					rr.gameObject = btn.gameObject;
					rr.module = null;
					raycastResults.Add(rr);
					break;
				}
			}
			rayCount = raycastResults.Count;
		}
		else
		{
			rayCount = -1;
		}

		if (raycastResults.Count > 0)
			lastHit = $"{raycastResults[0].gameObject.name}:{raycastResults[0].gameObject.layer}";
		else
			lastHit = "NONE";

		var firstTarget = raycastResults.Count > 0 ? raycastResults[0] : default(RaycastResult);

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

		lastDiag = GetDiag();
	}

	public string GetDiag()
	{
		var canvas = GameObject.FindObjectOfType<Canvas>();
		var buttons = canvas != null ? canvas.GetComponentsInChildren<Button>() : null;
		int bCnt = buttons != null ? buttons.Length : -1;
		var sb = new System.Text.StringBuilder();
		sb.Append($"timProc={processCount} timTouch={touchCount} timRay={rayCount} timHit={lastHit} timPos={lastTouchPos} bCnt={bCnt}");
		if (buttons != null)
		{
			for (int i = 0; i < buttons.Length && i < 8; i++)
			{
				var rt = buttons[i].transform as RectTransform;
				if (rt == null) continue;
				Vector3[] corners = new Vector3[4];
				rt.GetWorldCorners(corners);
				sb.Append($" btn{i}={buttons[i].name}({corners[0].x:F0},{corners[0].y:F0})-({corners[2].x:F0},{corners[2].y:F0})");
			}
		}
		return sb.ToString();
	}
}
