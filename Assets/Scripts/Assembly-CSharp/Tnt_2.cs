using UnityEngine;

public class Tnt_2 : MonoBehaviour
{
	private void Start()
	{
		base.transform.localPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		base.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
		float num = Random.Range(1f, 3f);
		base.transform.localScale = new Vector3(num, num);
	}
}
