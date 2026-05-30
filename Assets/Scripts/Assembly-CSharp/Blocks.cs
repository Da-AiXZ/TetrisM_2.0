using UnityEngine;

public class Blocks : MonoBehaviour
{
	public int id;

	public int x;

	public int y;

	private float timmer15;

	private bool isOn15;

	private int[,] before12 = new int[10, 20];

	private bool isBack12;

	private int timmer12;

	private int bx12;

	private int by12;

	private int xx12;

	private int yy12;

	private void Update()
	{
		if (timmer15 > 0f && isOn15)
		{
			timmer15 -= Time.deltaTime;
			if (timmer15 <= 0f)
			{
				isOn15 = false;
				MySystem.blocks[x, y] = 15;
				Add15();
			}
		}
	}

	public void Fall(int yy, int total)
	{
		int i = 1;
		bool flag = true;
		for (; y - i >= yy; i++)
		{
			if (MySystem.blocks[x, y - i] == 9 || id == 9)
			{
				flag = false;
			}
		}
		if (flag)
		{
			MySystem.blocks[x, y] = 0;
			y -= total;
			if (y <= 19)
			{
				MySystem.ticks[x, y] = true;
			}
			_Reset();
		}
	}

	public void Tick()
	{
		switch (id)
		{
		case 1:
			if (Check(x + 1, y, 2) || Check(x - 1, y, 2) || Check(x, y + 1, 2) || Check(x, y - 1, 2) || Check(x + 1, y, 56) || Check(x - 1, y, 56) || Check(x, y + 1, 56) || Check(x, y - 1, 56))
			{
				id = 0;
				Object.Instantiate(MySystem.tnt, base.transform.position, Quaternion.Euler(0f, 0f, 0f));
				_Reset15();
			}
			break;
		case 3:
			if (Check(x, y + 1, 0))
			{
				id = 4;
				_Reset();
			}
			break;
		case 4:
			if (!Check(x, y + 1, 0))
			{
				id = 3;
				_Reset();
			}
			break;
		case 6:
			if (Check(x + 1, y, 2) || Check(x - 1, y, 2) || Check(x, y + 1, 2) || Check(x, y - 1, 2))
			{
				id = 7;
				_Reset15();
			}
			break;
		case 7:
			if (!Check(x + 1, y, 2) && !Check(x - 1, y, 2) && !Check(x, y + 1, 2) && !Check(x, y - 1, 2))
			{
				id = 6;
				_Reset15();
			}
			break;
		case 8:
		{
			int num = 0;
			while (Check(x, y - 1, 0))
			{
				Fall(y - 1, 1);
				num = Random.Range(1, 5);
			}
			switch (num)
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.sand1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.sand2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.sand3);
				break;
			case 4:
				MySystem.sound.PlayOneShot(MySystem.sand4);
				break;
			}
			break;
		}
		case 11:
			if (Check(x + 1, y, 0))
			{
				GameObject obj = Object.Instantiate(MySystem.block);
				obj.GetComponent<Blocks>().id = 10;
				obj.GetComponent<Blocks>().x = x + 1;
				obj.GetComponent<Blocks>().y = y;
				obj.GetComponent<Blocks>()._Reset10();
			}
			if (Check(x - 1, y, 0))
			{
				GameObject obj2 = Object.Instantiate(MySystem.block);
				obj2.GetComponent<Blocks>().id = 10;
				obj2.GetComponent<Blocks>().x = x - 1;
				obj2.GetComponent<Blocks>().y = y;
				obj2.GetComponent<Blocks>()._Reset10();
			}
			if (Check(x, y + 1, 0))
			{
				GameObject obj3 = Object.Instantiate(MySystem.block);
				obj3.GetComponent<Blocks>().id = 10;
				obj3.GetComponent<Blocks>().x = x;
				obj3.GetComponent<Blocks>().y = y + 1;
				obj3.GetComponent<Blocks>()._Reset10();
			}
			if (Check(x, y - 1, 0))
			{
				GameObject obj4 = Object.Instantiate(MySystem.block);
				obj4.GetComponent<Blocks>().id = 10;
				obj4.GetComponent<Blocks>().x = x;
				obj4.GetComponent<Blocks>().y = y - 1;
				obj4.GetComponent<Blocks>()._Reset10();
			}
			break;
		case 12:
		{
			id = 13;
			int num2 = 0;
			int num3 = 0;
			while (num2 < 10)
			{
				before12[num2, num3] = MySystem.blocks[num2, num3];
				num3++;
				if (num3 > 19)
				{
					num3 = 0;
					num2++;
				}
			}
			bx12 = x;
			by12 = y;
			_Reset();
			break;
		}
		case 15:
			isOn15 = true;
			timmer15 = 0.5f;
			MySystem.blocks[x, y] = 2;
			Add15();
			break;
		case 16:
			if (Check(x, y - 1, 54) && Check(x, y - 2, 54))
			{
				GameObject.Find(x + "," + (y - 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y - 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + "," + (y - 2)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y - 2)).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.snowMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x, y + 1, 54) && Check(x, y + 2, 54))
			{
				GameObject.Find(x + "," + (y + 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y + 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + "," + (y + 2)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y + 2)).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.snowMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x - 1, y, 54) && Check(x - 2, y, 54))
			{
				GameObject.Find(x - 1 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 1 + "," + y).GetComponent<Blocks>()._Reset();
				GameObject.Find(x - 2 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 2 + "," + y).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.snowMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x + 1, y, 54) && Check(x + 2, y, 54))
			{
				GameObject.Find(x + 1 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 1 + "," + y).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + 2 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 2 + "," + y).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.snowMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x, y - 1, 55) && Check(x, y - 2, 55) && Check(x - 1, y - 1, 55) && Check(x + 1, y - 1, 55))
			{
				GameObject.Find(x + "," + (y - 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y - 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + 1 + "," + (y - 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 1 + "," + (y - 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x - 1 + "," + (y - 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 1 + "," + (y - 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + "," + (y - 2)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y - 2)).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.ironMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x, y + 1, 55) && Check(x, y + 2, 55) && Check(x - 1, y + 1, 55) && Check(x + 1, y + 1, 55))
			{
				GameObject.Find(x + "," + (y + 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y + 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + 1 + "," + (y + 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 1 + "," + (y + 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x - 1 + "," + (y + 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 1 + "," + (y + 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + "," + (y + 2)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + "," + (y + 2)).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.ironMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x + 1, y, 55) && Check(x + 2, y, 55) && Check(x + 1, y + 1, 55) && Check(x + 1, y - 1, 55))
			{
				GameObject.Find(x + 1 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 1 + "," + y).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + 2 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 2 + "," + y).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + 1 + "," + (y + 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 1 + "," + (y + 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x + 1 + "," + (y - 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x + 1 + "," + (y - 1)).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.ironMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			else if (Check(x - 1, y, 55) && Check(x - 2, y, 55) && Check(x - 1, y + 1, 55) && Check(x - 1, y - 1, 55))
			{
				GameObject.Find(x - 1 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 1 + "," + y).GetComponent<Blocks>()._Reset();
				GameObject.Find(x - 2 + "," + y).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 2 + "," + y).GetComponent<Blocks>()._Reset();
				GameObject.Find(x - 1 + "," + (y + 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 1 + "," + (y + 1)).GetComponent<Blocks>()._Reset();
				GameObject.Find(x - 1 + "," + (y - 1)).GetComponent<Blocks>().id = 0;
				GameObject.Find(x - 1 + "," + (y - 1)).GetComponent<Blocks>()._Reset();
				id = 0;
				_Reset();
				Object.Instantiate(MySystem.ironMan, base.transform.position, Quaternion.Euler(Vector3.zero));
			}
			break;
		case 17:
			if (Check(x + 1, y, 58) || Check(x - 1, y, 58) || Check(x, y + 1, 58) || Check(x, y - 1, 58))
			{
				id = 49;
				_Reset();
			}
			break;
		case 49:
			if (Check(x + 1, y, 56) || Check(x - 1, y, 56) || Check(x, y + 1, 56) || Check(x, y - 1, 56))
			{
				id = 17;
				_Reset();
			}
			break;
		}
	}

	public void _Reset10()
	{
		Add();
		base.transform.SetParent(MySystem.BlockMother.transform);
		MySystem.blocks[x, y] = id;
		base.name = x + "," + y;
		base.gameObject.GetComponent<SpriteRenderer>().sprite = MySystem.BlocksSprite[id];
		switch (Random.Range(1, 5))
		{
		case 1:
			MySystem.sound.PlayOneShot(MySystem.grass1);
			break;
		case 2:
			MySystem.sound.PlayOneShot(MySystem.grass2);
			break;
		case 3:
			MySystem.sound.PlayOneShot(MySystem.grass3);
			break;
		case 4:
			MySystem.sound.PlayOneShot(MySystem.grass4);
			break;
		}
		base.transform.position = new Vector2(-2.8f + (float)x * 0.4f, -3.8f + (float)y * 0.4f);
	}

	public void _Reset()
	{
		Add();
		MySystem.blocks[x, y] = id;
		base.name = x + "," + y;
		base.gameObject.GetComponent<SpriteRenderer>().sprite = MySystem.BlocksSprite[id];
		if (base.gameObject.GetComponent<MoveTo>() == null)
		{
			base.gameObject.AddComponent<MoveTo>().position = new Vector2(-2.8f + (float)x * 0.4f, -3.8f + (float)y * 0.4f);
		}
		else
		{
			GetComponent<MoveTo>().position = new Vector2(-2.8f + (float)x * 0.4f, -3.8f + (float)y * 0.4f);
		}
		if (id == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void _Reset15()
	{
		Add15();
		MySystem.blocks[x, y] = id;
		base.name = x + "," + y;
		base.gameObject.GetComponent<SpriteRenderer>().sprite = MySystem.BlocksSprite[id];
		if (base.gameObject.GetComponent<MoveTo>() == null)
		{
			base.gameObject.AddComponent<MoveTo>().position = new Vector2(-2.8f + (float)x * 0.4f, -3.8f + (float)y * 0.4f);
		}
		else
		{
			GetComponent<MoveTo>().position = new Vector2(-2.8f + (float)x * 0.4f, -3.8f + (float)y * 0.4f);
		}
		if (id == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private bool Check(int x, int y, int id)
	{
		if (x >= 0 && x < 10 && y >= 0 && y < 20 && MySystem.blocks[x, y] == id)
		{
			return true;
		}
		return false;
	}

	private void Add()
	{
		if (x + 1 >= 0 && x + 1 < 10 && y >= 0 && y < 20 && MySystem.blocks[x + 1, y] != 0)
		{
			MySystem.ticks[x + 1, y] = true;
		}
		if (x - 1 >= 0 && x - 1 < 10 && y >= 0 && y < 20 && MySystem.blocks[x - 1, y] != 0)
		{
			MySystem.ticks[x - 1, y] = true;
		}
		if (x >= 0 && x < 10 && y + 1 >= 0 && y + 1 < 20 && MySystem.blocks[x, y + 1] != 0)
		{
			MySystem.ticks[x, y + 1] = true;
		}
		if (x >= 0 && x < 10 && y - 1 >= 0 && y - 1 < 20 && MySystem.blocks[x, y - 1] != 0)
		{
			MySystem.ticks[x, y - 1] = true;
		}
	}

	private void Add15()
	{
		if (x + 1 >= 0 && x + 1 < 10 && y >= 0 && y < 20 && MySystem.blocks[x + 1, y] != 0 && MySystem.blocks[x + 1, y] != 15 && MySystem.blocks[x + 1, y] != 2)
		{
			MySystem.ticks[x + 1, y] = true;
		}
		if (x - 1 >= 0 && x - 1 < 10 && y >= 0 && y < 20 && MySystem.blocks[x - 1, y] != 0 && MySystem.blocks[x - 1, y] != 15 && MySystem.blocks[x - 1, y] != 2)
		{
			MySystem.ticks[x - 1, y] = true;
		}
		if (x >= 0 && x < 10 && y + 1 >= 0 && y + 1 < 20 && MySystem.blocks[x, y + 1] != 0 && MySystem.blocks[x, y + 1] != 15 && MySystem.blocks[x, y + 1] != 2)
		{
			MySystem.ticks[x, y + 1] = true;
		}
		if (x >= 0 && x < 10 && y - 1 >= 0 && y - 1 < 20 && MySystem.blocks[x, y - 1] != 0 && MySystem.blocks[x, y - 1] != 15 && MySystem.blocks[x, y - 1] != 2)
		{
			MySystem.ticks[x, y - 1] = true;
		}
	}

	public void Back12()
	{
		if (base.gameObject.GetComponent<MoveTo>() == null)
		{
			base.gameObject.AddComponent<MoveTo>().position = new Vector2(-1f, 0f);
		}
		else
		{
			GetComponent<MoveTo>().position = new Vector2(-1f, 0f);
		}
		isBack12 = true;
		MySystem.isBack = true;
		timmer12 = 0;
		base.name = "god";
		base.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "UI";
	}

	public void Back()
	{
		if (base.gameObject.GetComponent<MoveTo>() == null)
		{
			base.gameObject.AddComponent<MoveTo>().position = new Vector2(-1f, 0f);
		}
		else
		{
			GetComponent<MoveTo>().position = new Vector2(-1f, 0f);
		}
	}

	private void FixedUpdate()
	{
		if (!isBack12)
		{
			return;
		}
		timmer12++;
		if (timmer12 == 20)
		{
			int num = 0;
			int num2 = 0;
			while (num < 10)
			{
				if ((bool)GameObject.Find(num + "," + num2))
				{
					GameObject.Find(num + "," + num2).GetComponent<Blocks>().Back();
				}
				num2++;
				if (num2 > 19)
				{
					num2 = 0;
					num++;
				}
			}
			Water.Reset_();
			Lava.Reset_();
		}
		if (timmer12 == 50)
		{
			int num3 = 0;
			int num4 = 0;
			while (num3 < 10)
			{
				if ((bool)GameObject.Find(num3 + "," + num4))
				{
					Object.DestroyImmediate(GameObject.Find(num3 + "," + num4));
				}
				num4++;
				if (num4 > 19)
				{
					num4 = 0;
					num3++;
				}
			}
			num3 = 0;
			num4 = 0;
			while (num3 < 10)
			{
				MySystem.blocks[num3, num4] = before12[num3, num4];
				num4++;
				if (num4 > 19)
				{
					num4 = 0;
					num3++;
				}
			}
			xx12 = 0;
			yy12 = 0;
		}
		if (timmer12 >= 50 && xx12 < 10)
		{
			bool flag = true;
			while (flag)
			{
				if (MySystem.blocks[xx12, yy12] != 0 && MySystem.blocks[xx12, yy12] != 12 && MySystem.blocks[xx12, yy12] != 13)
				{
					GameObject obj = Object.Instantiate(MySystem.block, new Vector3(-1f, 0f), Quaternion.Euler(Vector3.zero));
					obj.name = xx12 + "," + yy12;
					obj.GetComponent<Blocks>().id = MySystem.blocks[xx12, yy12];
					obj.GetComponent<Blocks>().x = xx12;
					obj.GetComponent<Blocks>().y = yy12;
					obj.GetComponent<Blocks>()._Reset();
					obj.transform.SetParent(MySystem.BlockMother.transform);
					flag = false;
				}
				xx12++;
				if (xx12 > 9)
				{
					xx12 = 0;
					yy12++;
					if (yy12 >= 20)
					{
						flag = false;
					}
				}
			}
		}
		if (yy12 >= 20)
		{
			isBack12 = false;
			MySystem.isBack = false;
			id = 14;
			x = bx12;
			y = by12;
			_Reset();
			MySystem.hadGod = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.transform.tag == "tnt" && id != 9 && id != 12 && id != 13 && id != 14 && id != 56 && id != 57 && id != 58)
		{
			if (id == 1)
			{
				id = 0;
				MySystem.score += 10;
				Object.Instantiate(MySystem.tnt, base.transform.position, Quaternion.Euler(0f, 0f, 0f));
				_Reset();
			}
			else
			{
				id = 0;
				_Reset();
			}
		}
	}
}
