using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchInputModule : BaseInputModule
{
	public static string lastDiag = "INIT";
	public static int hitStrategy = 0; // 0=RectContains 1=ScreenCorners 2=WorldCorners 3=GraphicRaycaster
	
	private PointerEventData pointerData;
	private List<RaycastResult> raycastResults = new List<RaycastResult>();
	private int processCount = 0;
	private int touchCount = 0;
	private int rayCount = 0;
	private string lastHit = "";
	private string lastTouchPos = "";
	private string stratInfo = "";

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

		raycastResults.Clear();
		var canvas = GameObject.FindObjectOfType<Canvas>();
		if (canvas != null)
		{
			var buttons = canvas.GetComponentsInChildren<Button>();
			if (hitStrategy == 0)
				HitTest_RectContains(buttons, touch, canvas);
			else if (hitStrategy == 1)
				HitTest_ScreenCorners(buttons, touch, canvas);
			else if (hitStrategy == 2)
				HitTest_WorldCorners(buttons, touch);
			else if (hitStrategy == 3)
				HitTest_GraphicRaycaster(buttons, touch, canvas);
			rayCount = raycastResults.Count;
		}
		else
		{
			rayCount = -1;
		}

		if (raycastResults.Count > 0)
			lastHit = $"{raycastResults[0].gameObject.name}";
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

	void HitTest_RectContains(Button[] buttons, Touch touch, Canvas canvas)
	{
		stratInfo = "RectContains";
		foreach (var btn in buttons)
		{
			var rt = btn.transform as RectTransform;
			if (rt == null) continue;
			Vector2 localPoint;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, touch.position, canvas.worldCamera, out localPoint))
			{
				if (rt.rect.Contains(localPoint))
				{
					var rr = new RaycastResult();
					rr.gameObject = btn.gameObject;
					rr.module = null;
					raycastResults.Add(rr);
					return;
				}
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
			{
				var rr = new RaycastResult();
				rr.gameObject = btn.gameObject;
				rr.module = null;
				raycastResults.Add(rr);
				return;
			}
		}
	}

	void HitTest_WorldCorners(Button[] buttons, Touch touch)
	{
		stratInfo = "WorldCorners";
		foreach (var btn in buttons)
		{
			var rt = btn.transform as RectTransform;
			if (rt == null) continue;
			Vector3[] corners = new Vector3[4];
			rt.GetWorldCorners(corners);
			// World corners are in world space. For Screen Space - Camera canvas,
			// world space IS screen pixel space (assuming camera at z=0, ortho).
			float xMin = corners[0].x, yMin = corners[0].y;
			float xMax = corners[2].x, yMax = corners[2].y;
			if (touch.position.x >= xMin && touch.position.x <= xMax &&
			    touch.position.y >= yMin && touch.position.y <= yMax)
			{
				var rr = new RaycastResult();
				rr.gameObject = btn.gameObject;
				rr.module = null;
				raycastResults.Add(rr);
				return;
			}
		}
	}

	void HitTest_GraphicRaycaster(Button[] buttons, Touch touch, Canvas canvas)
	{
		stratInfo = "GraphicRaycaster";
		var raycaster = canvas.GetComponent<GraphicRaycaster>();
		if (raycaster == null) { stratInfo = "GraphicRaycaster(NULL)"; return; }
		
		var eventSystem = EventSystem.current;
		if (eventSystem == null) { stratInfo = "GraphicRaycaster(ES_NULL)"; return; }
		
		var ped = new PointerEventData(eventSystem);
		ped.position = touch.position;
		var results = new List<RaycastResult>();
		raycaster.Raycast(ped, results);
		if (results.Count > 0)
		{
			raycastResults.Add(results[0]);
		}
	}

	public string GetDiag()
	{
		var canvas = GameObject.FindObjectOfType<Canvas>();
		var buttons = canvas != null ? canvas.GetComponentsInChildren<Button>() : null;
		int bCnt = buttons != null ? buttons.Length : -1;
		var sb = new System.Text.StringBuilder();
		sb.Append($"timProc={processCount} timTouch={touchCount} timRay={rayCount} timHit={lastHit} timPos={lastTouchPos} strat={hitStrategy}:{stratInfo} bCnt={bCnt}");
		if (buttons != null && buttons.Length > 0)
		{
			// Show first3 button world corners
			for (int i = 0; i < buttons.Length && i < 3; i++)
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
