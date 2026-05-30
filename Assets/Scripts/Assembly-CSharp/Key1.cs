using UnityEngine;

public class Key1 : MonoBehaviour
{
	public GameObject zero;

	public GameObject up;

	public GameObject left;

	public GameObject down;

	public GameObject right;

	private void Update()
	{
		switch (MySystem.buttonDown[0])
		{
		case 0:
			zero.SetActive(value: true);
			up.SetActive(value: false);
			down.SetActive(value: false);
			left.SetActive(value: false);
			right.SetActive(value: false);
			break;
		case 1:
			zero.SetActive(value: false);
			up.SetActive(value: true);
			down.SetActive(value: false);
			left.SetActive(value: false);
			right.SetActive(value: false);
			break;
		case 2:
			zero.SetActive(value: false);
			up.SetActive(value: false);
			down.SetActive(value: false);
			left.SetActive(value: true);
			right.SetActive(value: false);
			break;
		case 3:
			zero.SetActive(value: false);
			up.SetActive(value: false);
			down.SetActive(value: false);
			left.SetActive(value: false);
			right.SetActive(value: true);
			break;
		case 4:
			zero.SetActive(value: false);
			up.SetActive(value: false);
			down.SetActive(value: true);
			left.SetActive(value: false);
			right.SetActive(value: false);
			break;
		}
	}
}
