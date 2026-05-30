using UnityEngine;

public class White : MonoBehaviour
{
	public SpriteRenderer s;

	private float a = 1f;

	private void FixedUpdate()
	{
		a -= 0.1f;
		if (a >= 0f)
		{
			s.color = new Color(1f, 1f, 1f, a);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
