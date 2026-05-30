using UnityEngine;

public class FallDown : MonoBehaviour
{
	public bool isShow = true;

	public int toward;

	public int type;

	public int x;

	public int y;

	public FallDown_2 a;

	public FallDown_2 b;

	public FallDown_2 c;

	public FallDown_2 d;

	public int id;

	public bool canNext;

	public GameObject next;

	private int timmer;

	private int timmer2;

	private Vector2 v;

	private void Start()
	{
		MySystem.isHold = true;
		if (Random.Range(1, 3) == 1)
		{
			while (id == 0 || id == 4 || id == 7 || id == 12 || id == 13 || id == 14 || id == 57)
			{
				id = Random.Range(1, 23);
				if (id >= 18)
				{
					id += 36;
				}
			}
		}
		else
		{
			while (id == 0 || id == 4 || id == 7 || id == 13 || id == 14 || id == 49)
			{
				id = Random.Range(18, 53);
			}
		}
		if ((bool)GameObject.Find("FallDown") && GameObject.Find("FallDown").GetComponent<FallDown>().id == 1)
		{
			int num = Random.Range(1, 100);
			if (num < 30)
			{
				id = 56;
			}
			else if (num < 60)
			{
				id = 2;
			}
			else if (num < 90)
			{
				id = 15;
			}
		}
		a.id = id;
		b.id = id;
		c.id = id;
		d.id = id;
		if (!MySystem.hadGod)
		{
			if (Random.Range(1, 100) == 3)
			{
				a.id = 12;
				MySystem.hadGod = true;
			}
			else if (Random.Range(1, 100) == 3)
			{
				b.id = 12;
				MySystem.hadGod = true;
			}
			else if (Random.Range(1, 100) == 3)
			{
				c.id = 12;
				MySystem.hadGod = true;
			}
			else if (Random.Range(1, 100) == 3)
			{
				d.id = 12;
				MySystem.hadGod = true;
			}
		}
		x = 5;
		y = 21;
		a.position = new Vector2(0f, 0f);
		a.toward = 0;
		a.type = 0;
		switch (type)
		{
		case 0:
			b.position = new Vector2(0f, 2f);
			b.toward = 0;
			b.type = 2;
			c.position = new Vector2(0f, 1f);
			c.toward = 0;
			c.type = 1;
			d.position = new Vector2(0f, -1f);
			d.toward = 2;
			d.type = 1;
			break;
		case 1:
			b.position = new Vector2(0f, 1f);
			b.toward = 0;
			b.type = 1;
			c.position = new Vector2(1f, 1f);
			c.toward = 0;
			c.type = 3;
			d.position = new Vector2(1f, 0f);
			d.toward = 1;
			d.type = 1;
			break;
		case 2:
			b.position = new Vector2(-1f, 0f);
			b.toward = 3;
			b.type = 1;
			c.position = new Vector2(1f, 0f);
			c.toward = 1;
			c.type = 1;
			d.position = new Vector2(0f, -1f);
			d.toward = 2;
			d.type = 1;
			break;
		case 3:
			b.position = new Vector2(0f, 1f);
			b.toward = 0;
			b.type = 1;
			c.position = new Vector2(0f, -1f);
			c.toward = 2;
			c.type = 1;
			d.position = new Vector2(1f, -1f);
			d.toward = 1;
			d.type = 3;
			break;
		case 4:
			b.position = new Vector2(0f, 1f);
			b.toward = 0;
			b.type = 1;
			c.position = new Vector2(-1f, -1f);
			c.toward = 2;
			c.type = 3;
			d.position = new Vector2(0f, -1f);
			d.toward = 2;
			d.type = 1;
			break;
		case 5:
			b.position = new Vector2(1f, 0f);
			b.toward = 1;
			b.type = 1;
			c.position = new Vector2(-1f, -1f);
			c.toward = 2;
			c.type = 3;
			d.position = new Vector2(0f, -1f);
			d.toward = 2;
			d.type = 1;
			break;
		case 6:
			b.position = new Vector2(-1f, 0f);
			b.toward = 3;
			b.type = 1;
			c.position = new Vector2(0f, -1f);
			c.toward = 2;
			c.type = 1;
			d.position = new Vector2(1f, -1f);
			d.toward = 1;
			d.type = 3;
			break;
		}
		a._Reset();
		b._Reset();
		c._Reset();
		d._Reset();
	}

	private void Update()
	{
		if (isShow)
		{
			base.name = "Showing";
			switch (type)
			{
			case 0:
				base.transform.position = new Vector3(2f, 0.8f, 0f);
				break;
			case 1:
				base.transform.position = new Vector3(1.8f, 0.8f, 0f);
				break;
			case 2:
				base.transform.position = new Vector3(2f, 1.2f, 0f);
				break;
			case 3:
				base.transform.position = new Vector3(1.8f, 1f, 0f);
				break;
			case 4:
				base.transform.position = new Vector3(2.2f, 1f, 0f);
				break;
			case 5:
				base.transform.position = new Vector3(2f, 1.2f, 0f);
				break;
			case 6:
				base.transform.position = new Vector3(2f, 1.2f, 0f);
				break;
			}
		}
		else if (!MySystem.isBack)
		{
			base.name = "FallDown";
			if ((Input.GetKeyDown(MySystem.keys.w) || Input.GetKeyDown(MySystem.keys.up) || MySystem.buttonDown[1] == 1) && !MySystem.hadTurn)
			{
				Turn();
				MySystem.hadTurn = true;
			}
			if (!Input.GetKeyDown(MySystem.keys.w) && !Input.GetKeyDown(MySystem.keys.up) && MySystem.buttonDown[1] != 1 && MySystem.hadTurn)
			{
				MySystem.hadTurn = false;
			}
			if ((Input.GetKeyDown(MySystem.keys.space) || MySystem.buttonDown[2] == 1) && !MySystem.hadDown)
			{
				FallToEnd();
				MySystem.hadDown = true;
			}
			if (!Input.GetKeyDown(MySystem.keys.space) && MySystem.buttonDown[2] != 1 && MySystem.hadDown)
			{
				MySystem.hadDown = false;
			}
			base.transform.position = new Vector3(Mathf.SmoothDamp(base.transform.position.x, -2.8f + (float)x * 0.4f, ref v.x, 0.1f), Mathf.SmoothDamp(base.transform.position.y, -3.8f + (float)y * 0.4f, ref v.y, 0.1f), 0f);
		}
	}

	private void FixedUpdate()
	{
		if (isShow || MySystem.isBack)
		{
			return;
		}
		timmer++;
		if (timmer >= MySystem.speed)
		{
			timmer = 0;
			Fall();
		}
		if ((Input.GetKey(MySystem.keys.s) || Input.GetKey(MySystem.keys.down) || MySystem.buttonDown[0] == 4) && timmer >= 5)
		{
			timmer = 0;
			Fall();
		}
		int num = 0;
		if (Input.GetKey(MySystem.keys.a) || Input.GetKey(MySystem.keys.left) || MySystem.buttonDown[0] == 2)
		{
			num = 1;
		}
		if (Input.GetKey(MySystem.keys.d) || Input.GetKey(MySystem.keys.right) || MySystem.buttonDown[0] == 3)
		{
			num = ((num != 1) ? 2 : 0);
		}
		if (num == 0)
		{
			timmer2 = 100;
		}
		else
		{
			timmer2++;
		}
		if (timmer2 >= 10 && num != 0)
		{
			timmer2 = 0;
			switch (num)
			{
			case 1:
				Left();
				break;
			case 2:
				Right();
				break;
			}
		}
	}

	public void EndShow()
	{
		base.transform.position = new Vector3(0f, 100f, 0f);
	}

	public void Turn()
	{
		switch (Random.Range(1, 5))
		{
		case 1:
			MySystem.sound2.PlayOneShot(MySystem.turn1);
			break;
		case 2:
			MySystem.sound2.PlayOneShot(MySystem.turn2);
			break;
		case 3:
			MySystem.sound2.PlayOneShot(MySystem.turn3);
			break;
		case 4:
			MySystem.sound2.PlayOneShot(MySystem.turn4);
			break;
		}
		if (a.CanTurn() && b.CanTurn() && c.CanTurn() && d.CanTurn())
		{
			a.Turn();
			b.Turn();
			c.Turn();
			d.Turn();
		}
		toward++;
		if (toward > 3)
		{
			toward = 0;
		}
	}

	public void Fall()
	{
		if (a.CanFall() && b.CanFall() && c.CanFall() && d.CanFall())
		{
			y--;
		}
		else
		{
			Down();
		}
	}

	public void Left()
	{
		if (a.CanLeft() && b.CanLeft() && c.CanLeft() && d.CanLeft())
		{
			x--;
		}
	}

	public void Right()
	{
		if (a.CanRight() && b.CanRight() && c.CanRight() && d.CanRight())
		{
			x++;
		}
	}

	public void Down()
	{
		a.Down();
		b.Down();
		c.Down();
		d.Down();
		if (canNext)
		{
			Object.Instantiate(next);
		}
		GameObject.Find("Main Camera").GetComponent<MyCamera>().Shake(0f, 0.1f);
		MySystem.isHold = false;
		if (id == 1 || id == 3 || id == 4 || id == 10 || id == 15)
		{
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
		}
		else if (id == 8)
		{
			switch (Random.Range(1, 5))
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
		}
		else if (id == 11 || id == 19 || id == 20)
		{
			switch (Random.Range(1, 5))
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.wood1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.wood2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.wood3);
				break;
			case 4:
				MySystem.sound.PlayOneShot(MySystem.wood4);
				break;
			}
		}
		else if ((id % 2 == 0 && id >= 26 && id <= 34) || (id % 2 == 1 && id >= 37 && id <= 41) || (id >= 42 && id <= 48) || id == 54)
		{
			switch (Random.Range(1, 5))
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.cloth1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.cloth2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.cloth3);
				break;
			case 4:
				MySystem.sound.PlayOneShot(MySystem.cloth4);
				break;
			}
		}
		else if (id == 56)
		{
			switch (Random.Range(1, 4))
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.lava1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.lava2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.lava3);
				break;
			}
		}
		else if (id == 58)
		{
			switch (Random.Range(1, 4))
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.water1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.water2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.water3);
				break;
			}
		}
		else
		{
			switch (Random.Range(1, 5))
			{
			case 1:
				MySystem.sound.PlayOneShot(MySystem.stone1);
				break;
			case 2:
				MySystem.sound.PlayOneShot(MySystem.stone2);
				break;
			case 3:
				MySystem.sound.PlayOneShot(MySystem.stone3);
				break;
			case 4:
				MySystem.sound.PlayOneShot(MySystem.stone4);
				break;
			}
		}
		Object.Destroy(base.gameObject);
	}

	public void FallToEnd()
	{
		while (a.CanFall() && b.CanFall() && c.CanFall() && d.CanFall())
		{
			y--;
		}
		Down();
		GameObject.Find("Main Camera").GetComponent<MyCamera>().Shake(0f, 0.2f);
	}

	public void Destroy()
	{
		Object.Destroy(base.gameObject);
	}
}
