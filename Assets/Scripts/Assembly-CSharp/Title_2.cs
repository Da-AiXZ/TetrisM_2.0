using UnityEngine;

public class Title_2 : MonoBehaviour
{
	public SpriteRenderer s;

	public float time;

	private float timmer;

	private float a;

	private void Start()
	{
		s.color = new Color(1f, 1f, 1f, 0f);
	}

	private void FixedUpdate()
	{
		timmer += Time.deltaTime;
		if (a < 1f && timmer >= time && !GameObject.Find("Title").GetComponent<Title>().isDown)
		{
			a += 0.01f;
			s.color = new Color(1f, 1f, 1f, a);
		}
		else if (a > 0f && GameObject.Find("Title").GetComponent<Title>().isDown)
		{
			a -= 0.05f;
			if (a < 0f)
			{
				a = 0f;
			}
			s.color = new Color(1f, 1f, 1f, a);
		}
		if (timmer >= time && (Input.anyKey || Input.touchCount > 0 || Input.GetMouseButtonDown(0)))
		{
			GameObject.Find("Title").GetComponent<Title>().isDown = true;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
