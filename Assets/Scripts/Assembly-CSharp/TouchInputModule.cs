using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchInputModule : BaseInputModule
{
	public static string lastDiag = "INIT";
{
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

		raycastResults.Clear();
		eventSystem.RaycastAll(pointerData, raycastResults);
		rayCount = raycastResults.Count;

		if (raycastResults.Count > 0)
		{
			lastHit = $"{raycastResults[0].gameObject.name}:{raycastResults[0].gameObject.layer}";
		}
		else
		{
			lastHit = "NONE";
		}

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
		return $"timProc={processCount} timTouch={touchCount} timRay={rayCount} timHit={lastHit} timPos={lastTouchPos}";
	}
}
