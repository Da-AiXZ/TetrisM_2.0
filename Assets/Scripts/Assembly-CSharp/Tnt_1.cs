using UnityEngine;

public class Tnt_1 : MonoBehaviour
{
	private float timmer;

	private int count;

	private bool exp;

	public GameObject r;

	public GameObject w;

	public Rigidbody2D rig;

	private void Start()
	{
		rig.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(3f, 4f));
		MySystem.sound.PlayOneShot(MySystem.fuse);
		base.name = "Tnt_1";
	}

	private void Update()
	{
		if (!exp)
		{
			timmer += Time.deltaTime;
			if ((double)timmer < 0.3)
			{
				r.SetActive(value: true);
				w.SetActive(value: false);
			}
			else if ((double)timmer < 0.6)
			{
				r.SetActive(value: false);
				w.SetActive(value: true);
				if (count >= 7)
				{
					exp = true;
				}
			}
			else
			{
				timmer = 0f;
				count++;
			}
			return;
		}
		timmer += Time.deltaTime;
		w.transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime);
		if (timmer > 0.5f)
		{
			MyCamera.shake = 0.1f;
			switch (Random.Range(1, 5))
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.exp1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.exp2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.exp3);
				break;
			case 4:
				MySystem.sound.PlayOneShot(MySystem.exp4);
				break;
			}
			Object.Instantiate(MySystem.tnt_2, base.transform.position, Quaternion.Euler(0f, 0f, 0f));
			Object.Destroy(base.gameObject);
		}
	}
}
