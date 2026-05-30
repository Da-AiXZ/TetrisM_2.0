using UnityEngine;

public class DestoryTimmer : MonoBehaviour
{
	public float time;

	private float timmer;

	private void Update()
	{
		timmer += Time.deltaTime;
		if (timmer > time)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
