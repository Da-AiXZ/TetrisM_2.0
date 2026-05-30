using UnityEngine;

public class WaterKid : MonoBehaviour
{
	public int x;

	public int y;

	public int n;

	public SpriteRenderer sprite;

	public void Hello()
	{
		base.name = "Water" + x + "," + y;
		sprite.size = new Vector2(0.16f, 0.01f * (float)n);
		base.transform.position = new Vector2(-2.8f + (float)x * 0.4f, -4f + (float)y * 0.025f + 0.025f * (float)(n / 2));
		for (int i = 0; i < n; i++)
		{
			if (y + i < 320)
			{
				Water.showingWater[x, y + i] = 1;
			}
		}
	}

	public void GoodBye()
	{
		for (int i = 0; i < n; i++)
		{
			Water.showingWater[x, y + i] = 0;
		}
		Object.Destroy(base.gameObject);
	}

	public void Wei(int nn)
	{
		if (nn == 0)
		{
			Object.Destroy(base.gameObject);
		}
		for (int i = 0; i < n; i++)
		{
			Water.showingWater[x, y + i] = 0;
		}
		n = nn;
		Hello();
	}
}
