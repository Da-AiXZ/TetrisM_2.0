using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Button : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	public float pressDurationTime = 1f;

	public bool responseOnceByPress;

	public float doubleClickIntervalTime = 0.5f;

	public UnityEvent onDoubleClick;

	public UnityEvent onPress;

	public UnityEvent onClick;

	private bool isDown;

	private bool isPress;

	private float downTime;

	private float clickIntervalTime;

	private int clickTimes;

	private void Update()
	{
		if (isDown && MySystem.isPhone)
		{
			if (responseOnceByPress && isPress)
			{
				return;
			}
			downTime += Time.deltaTime;
			if (downTime > pressDurationTime)
			{
				isPress = true;
				onPress.Invoke();
			}
		}
		if (clickTimes < 1 || !MySystem.isPhone)
		{
			return;
		}
		clickIntervalTime += Time.deltaTime;
		if (clickIntervalTime >= doubleClickIntervalTime)
		{
			if (clickTimes >= 2)
			{
				onDoubleClick.Invoke();
			}
			else
			{
				onClick.Invoke();
			}
			clickTimes = 0;
			clickIntervalTime = 0f;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isDown = true;
		downTime = 0f;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isDown = false;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isDown = false;
		isPress = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!isPress)
		{
			clickTimes++;
		}
		else
		{
			isPress = false;
		}
	}
}
