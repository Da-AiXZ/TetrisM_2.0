using UnityEngine;

public class MyCamera : MonoBehaviour
{
	private Vector2 v;

	private bool isOnPosition;

	private Vector3 position = new Vector3(0f, 0f, -10f);

	public static float shake;

	private void Update()
	{
		position.z = -10f;
		if (!isOnPosition)
		{
			base.transform.position = new Vector3(0f, Mathf.SmoothDamp(base.transform.position.y, 0f, ref v.y, 0.5f), -10f);
			if ((double)Mathf.Abs(base.transform.position.y) < 0.001)
			{
				isOnPosition = true;
				base.transform.position = new Vector3(0f, 0f, -10f);
			}
		}
		else
		{
			base.transform.position = new Vector3(Mathf.SmoothDamp(base.transform.position.x, position.x, ref v.x, 0.5f), Mathf.SmoothDamp(base.transform.position.y, position.y, ref v.y, 0.5f), -10f);
			if (Mathf.Abs(base.transform.position.x - position.x) + Mathf.Abs(base.transform.position.y - position.y) <= 0.001f)
			{
				base.transform.position = position;
			}
		}
	}

	private void FixedUpdate()
	{
		if (shake > 0f)
		{
			base.transform.position = new Vector3(position.x + Random.Range(-0.2f, 0.2f), position.y + Random.Range(-0.2f, 0.2f), -10f);
			shake -= Time.deltaTime;
		}
	}

	public void Shake(float x, float y)
	{
		base.transform.position = new Vector3(x, y, -10f);
	}
}
