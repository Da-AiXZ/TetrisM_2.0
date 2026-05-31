using UnityEngine;

public class Title : MonoBehaviour
{
	public Vector2 position;

	public float waitTime;

	public float speed;

	public Rigidbody2D r;

	public bool move;

	public bool isDown;

	public bool isMother;

	private Vector2 v;

	private bool isOnPosition;

	private float timemer;

	private float movePosition;

	private void Start()
	{
		if (!isMother)
		{
			r.simulated = false;
		}
		else
		{
			MySystem.StartLoadScene("level1");
		}
	}

	private void Update()
	{
		if (!isMother)
		{
			timemer += Time.deltaTime;
			if (timemer >= waitTime && !isOnPosition)
			{
				base.transform.position = new Vector3(Mathf.SmoothDamp(base.transform.position.x, position.x, ref v.x, speed), Mathf.SmoothDamp(base.transform.position.y, position.y, ref v.y, speed), 0f);
				if (Mathf.Abs(base.transform.position.x - position.x) + Mathf.Abs(base.transform.position.y - position.y) <= 0.01f)
				{
					base.transform.position = position;
					isOnPosition = true;
				}
			}
			else
			{
				if (!isOnPosition)
				{
					return;
				}
				if (!GameObject.Find("Title").GetComponent<Title>().isDown)
				{
					if (move)
					{
						if ((double)(base.transform.position.y - position.y) > 0.059)
						{
							movePosition = position.y - 0.06f;
							v.y = 0f;
						}
						else if ((double)(base.transform.position.y - position.y) < -0.059)
						{
							movePosition = position.y + 0.06f;
							v.y = 0f;
						}
						else if (base.transform.position.y - position.y == 0f)
						{
							movePosition = position.y - 0.06f;
							v.y = 0f;
						}
						base.transform.position = new Vector3(position.x, Mathf.SmoothDamp(base.transform.position.y, movePosition, ref v.y, 1f), 0f);
					}
				}
				else
				{
					r.simulated = true;
					r.velocity = new Vector2(Random.Range(-3f, 3f), Random.Range(5f, 10f));
					r.angularVelocity = Random.Range(-50f, 50f);
					Object.Destroy(this);
				}
			}
		}
		else if (isDown)
		{
			timemer += Time.deltaTime;
			if ((double)timemer > 1.3)
			{
				MySystem.LoadScene();
				Object.Destroy(this);
			}
		}
	}
}
