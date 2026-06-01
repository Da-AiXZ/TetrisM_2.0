using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Button : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	public float pressDurationTime = 1f;
	private UnityEngine.UI.Image _image;
	private Color _originalColor;

	private void Awake()
	{
		_image = GetComponent<UnityEngine.UI.Image>();
		if (_image != null) _originalColor = _image.color;
	}

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
		if (_image != null) _image.color = _originalColor * 0.6f;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isDown = false;
		if (_image != null) _image.color = _originalColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isDown = false;
		isPress = false;
		if (_image != null) _image.color = _originalColor;
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
