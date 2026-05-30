using UnityEngine;

public class Key2 : MonoBehaviour
{
	public GameObject up;

	public GameObject down;

	private void Update()
	{
		if (MySystem.is2)
		{
			up.SetActive(value: false);
			down.SetActive(value: true);
		}
		else
		{
			up.SetActive(value: true);
			down.SetActive(value: false);
		}
	}
}
