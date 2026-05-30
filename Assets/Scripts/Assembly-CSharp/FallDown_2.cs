using UnityEngine;

public class FallDown_2 : MonoBehaviour
{
	public FallDown mother;

	public Vector2 position;

	public int x;

	public int y;

	public int toward;

	public int type;

	public int id;

	public SpriteRenderer spriteR;

	public GameObject white;

	private Vector2 v;

	public void _Reset()
	{
		x = (int)position.x;
		y = (int)position.y;
		spriteR.sprite = MySystem.BlocksSprite[id];
		if (id == 58)
		{
			Object.Instantiate(Resources.Load("Water") as GameObject, base.transform);
		}
	}

	private void Update()
	{
		base.transform.localPosition = new Vector3(Mathf.SmoothDamp(base.transform.localPosition.x, (float)x * 0.4f, ref v.x, 0.1f), Mathf.SmoothDamp(base.transform.localPosition.y, (float)y * 0.4f, ref v.y, 0.1f), 0f);
	}

	public bool CanTurn()
	{
		int num = x;
		int num2 = y;
		switch (type)
		{
		case 0:
			return true;
		case 1:
			switch (toward)
			{
			case 0:
				num++;
				num2--;
				break;
			case 1:
				num--;
				num2--;
				break;
			case 2:
				num--;
				num2++;
				break;
			case 3:
				num++;
				num2++;
				break;
			}
			if (mother.x + num < 10 && mother.x + num >= 0 && mother.y + num2 < 20 && mother.y + num2 >= 0)
			{
				if (MySystem.blocks[mother.x + num, mother.y + num2] != 0 && MySystem.blocks[mother.x + num, mother.y + num2] != 56 && MySystem.blocks[mother.x + num, mother.y + num2] != 58)
				{
					return false;
				}
				return true;
			}
			if (mother.x + num < 10 && mother.x + num >= 0 && mother.y + num2 >= 19)
			{
				return true;
			}
			return false;
		case 2:
			switch (toward)
			{
			case 0:
				num += 2;
				num2 -= 2;
				break;
			case 1:
				num -= 2;
				num2 -= 2;
				break;
			case 2:
				num -= 2;
				num2 += 2;
				break;
			case 3:
				num += 2;
				num2 += 2;
				break;
			}
			if (mother.x + num < 10 && mother.x + num >= 0 && mother.y + num2 < 20 && mother.y + num2 >= 0)
			{
				if (MySystem.blocks[mother.x + num, mother.y + num2] != 0 && MySystem.blocks[mother.x + num, mother.y + num2] != 56 && MySystem.blocks[mother.x + num, mother.y + num2] != 58)
				{
					return false;
				}
				return true;
			}
			if (mother.x + num < 10 && mother.x + num >= 0 && mother.y + num2 >= 19)
			{
				return true;
			}
			return false;
		case 3:
			switch (toward)
			{
			case 0:
				num2 -= 2;
				break;
			case 1:
				num -= 2;
				break;
			case 2:
				num2 += 2;
				break;
			case 3:
				num += 2;
				break;
			}
			if (mother.x + num < 10 && mother.x + num >= 0 && mother.y + num2 < 20 && mother.y + num2 >= 0)
			{
				if (MySystem.blocks[mother.x + num, mother.y + num2] != 0 && MySystem.blocks[mother.x + num, mother.y + num2] != 56 && MySystem.blocks[mother.x + num, mother.y + num2] != 58)
				{
					return false;
				}
				return true;
			}
			if (mother.x + num < 10 && mother.x + num >= 0 && mother.y + num2 >= 19)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public void Turn()
	{
		switch (type)
		{
		case 1:
			switch (toward)
			{
			case 0:
				x++;
				y--;
				break;
			case 1:
				x--;
				y--;
				break;
			case 2:
				x--;
				y++;
				break;
			case 3:
				x++;
				y++;
				break;
			}
			break;
		case 2:
			switch (toward)
			{
			case 0:
				x += 2;
				y -= 2;
				break;
			case 1:
				x -= 2;
				y -= 2;
				break;
			case 2:
				x -= 2;
				y += 2;
				break;
			case 3:
				x += 2;
				y += 2;
				break;
			}
			break;
		case 3:
			switch (toward)
			{
			case 0:
				y -= 2;
				break;
			case 1:
				x -= 2;
				break;
			case 2:
				y += 2;
				break;
			case 3:
				x += 2;
				break;
			}
			break;
		}
		toward++;
		if (toward > 3)
		{
			toward = 0;
		}
	}

	public bool CanLeft()
	{
		if (mother.x + x - 1 < 10 && mother.x + x - 1 >= 0 && mother.y + y < 25 && mother.y + y >= 0)
		{
			if (MySystem.blocks[mother.x + x - 1, mother.y + y] != 0 && MySystem.blocks[mother.x + x - 1, mother.y + y] != 58 && MySystem.blocks[mother.x + x - 1, mother.y + y] != 56)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CanRight()
	{
		if (mother.x + x + 1 < 10 && mother.x + x + 1 >= 0 && mother.y + y < 25 && mother.y + y >= 0)
		{
			if (MySystem.blocks[mother.x + x + 1, mother.y + y] != 0 && MySystem.blocks[mother.x + x + 1, mother.y + y] != 56 && MySystem.blocks[mother.x + x + 1, mother.y + y] != 58)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CanFall()
	{
		if (mother.x + x < 10 && mother.x + x >= 0 && mother.y + y - 1 < 20 && mother.y + y - 1 >= 0)
		{
			if (MySystem.blocks[mother.x + x, mother.y + y - 1] != 0)
			{
				if (MySystem.blocks[mother.x + x, mother.y + y - 1] == 5)
				{
					switch (Random.Range(1, 4))
					{
					case 1:
						MySystem.sound.PlayOneShot(MySystem.glass1);
						break;
					case 2:
						MySystem.sound.PlayOneShot(MySystem.glass2);
						break;
					case 3:
						MySystem.sound.PlayOneShot(MySystem.glass3);
						break;
					}
					GameObject.Find(mother.x + x + "," + (mother.y + y - 1)).GetComponent<Blocks>().id = 0;
					GameObject.Find(mother.x + x + "," + (mother.y + y - 1)).GetComponent<Blocks>()._Reset();
					return true;
				}
				if (MySystem.blocks[mother.x + x, mother.y + y - 1] == 56 || MySystem.blocks[mother.x + x, mother.y + y - 1] == 58)
				{
					return true;
				}
				return false;
			}
			return true;
		}
		if (mother.x + x < 10 && mother.x + x >= 0 && mother.y + y - 1 >= 20)
		{
			return true;
		}
		return false;
	}

	public void Down()
	{
		if (y + mother.y > 19)
		{
			MySystem.gameOver = true;
			MySystem.isStart = false;
			mother.Destroy();
			Object.Destroy(base.gameObject);
		}
		else if (id != 56 && id != 58)
		{
			white.SetActive(value: true);
			base.transform.SetParent(GameObject.Find("BlockMother").transform);
			base.transform.name = x + mother.x + "," + (y + mother.y);
			MySystem.blocks[x + mother.x, y + mother.y] = id;
			base.transform.position = new Vector3(-2.8f + (float)(x + mother.x) * 0.4f, -3.8f + (float)(y + mother.y) * 0.4f);
			MoveTo moveTo = base.gameObject.AddComponent<MoveTo>();
			moveTo.position = new Vector2(-2.8f + (float)(x + mother.x) * 0.4f, -3.8f + (float)(y + mother.y) * 0.4f);
			moveTo.v = v;
			Blocks blocks = base.gameObject.AddComponent<Blocks>();
			blocks.x = x + mother.x;
			blocks.y = y + mother.y;
			blocks.id = id;
			blocks._Reset();
			MySystem.ticks[x + mother.x, y + mother.y] = true;
			Object.Destroy(this);
		}
		else if (id == 56)
		{
			Lava.AddWater(mother.x + x, mother.y + y);
			Object.Destroy(base.gameObject);
		}
		else if (id == 58)
		{
			Water.AddWater(mother.x + x, mother.y + y);
			Object.Destroy(base.gameObject);
		}
	}
}
