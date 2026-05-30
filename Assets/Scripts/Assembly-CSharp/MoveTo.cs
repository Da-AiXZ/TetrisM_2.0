using UnityEngine;

public class MoveTo : MonoBehaviour
{
	public Vector2 position;

	public Vector2 v;

	private void Update()
	{
		base.transform.position = new Vector3(Mathf.SmoothDamp(base.transform.position.x, position.x, ref v.x, 0.1f), Mathf.SmoothDamp(base.transform.position.y, position.y, ref v.y, 0.1f), 0f);
		if (Mathf.Abs(base.transform.position.x - position.x) + Mathf.Abs(base.transform.position.y - position.y) <= 0.001f)
		{
			base.transform.position = position;
			Object.Destroy(this);
		}
	}
}
