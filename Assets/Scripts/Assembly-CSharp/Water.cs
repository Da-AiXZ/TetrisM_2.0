using System;
using UnityEngine;

public class Water : MonoBehaviour
{
	public static int[,] showingWater = new int[10, 320];

	public static int[,] water = new int[10, 320];

	public static GameObject waterKids;

	private int tick;

	public Vector2Int enter;

	private void FixedUpdate()
	{
		tick++;
		if (Input.GetKey("o") && Input.GetKey("p") && Input.GetKey("i"))
		{
			water[enter.x, enter.y] = 1;
			MySystem.score = -2333333;
		}
		if (tick >= 1)
		{
			tick = 0;
			Tick();
		}
	}

	private void Tick()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = DateTime.Now.Second;
		while (num < 10)
		{
			if ((water[num, num2] == 1 || water[num, num2] == 2) && MySystem.blocks[num, num2 / 16] != 0 && MySystem.blocks[num, num2 / 16] != 56 && MySystem.blocks[num, num2 / 16] != 58)
			{
				num3 = 1;
				water[num, num2] = 0;
				if (num4 % 2 == 0)
				{
					while (true)
					{
						if (water[num, num2 + num3] != 1 && water[num, num2 + num3] != 2 && (MySystem.blocks[num, (num2 + num3) / 16] == 0 || MySystem.blocks[num, (num2 + num3) / 16] == 56 || MySystem.blocks[num, (num2 + num3) / 16] == 58))
						{
							water[num, num2 + num3] = 1;
							break;
						}
						if (num > 0 && water[num - 1, num2 + num3] != 1 && water[num - 1, num2 + num3] != 2 && (MySystem.blocks[num - 1, (num2 + num3) / 16] == 0 || MySystem.blocks[num - 1, (num2 + num3) / 16] == 56 || MySystem.blocks[num - 1, (num2 + num3) / 16] == 58))
						{
							water[num - 1, num2 + num3] = 1;
							break;
						}
						if (num < 9 && water[num + 1, num2 + num3] != 1 && water[num + 1, num2 + num3] != 2 && (MySystem.blocks[num + 1, (num2 + num3) / 16] == 0 || MySystem.blocks[num + 1, (num2 + num3) / 16] == 56 || MySystem.blocks[num + 1, (num2 + num3) / 16] == 58))
						{
							water[num + 1, num2 + num3] = 1;
							break;
						}
						num3++;
						if (num2 + num3 > 319)
						{
							MySystem.gameOver = true;
							break;
						}
					}
				}
				else
				{
					while (true)
					{
						if (water[num, num2 + num3] != 1 && water[num, num2 + num3] != 2 && (MySystem.blocks[num, (num2 + num3) / 16] == 0 || MySystem.blocks[num, (num2 + num3) / 16] == 56 || MySystem.blocks[num, (num2 + num3) / 16] == 58))
						{
							water[num, num2 + num3] = 1;
							break;
						}
						if (num < 9 && water[num + 1, num2 + num3] != 1 && water[num + 1, num2 + num3] != 2 && (MySystem.blocks[num + 1, (num2 + num3) / 16] == 0 || MySystem.blocks[num + 1, (num2 + num3) / 16] == 56 || MySystem.blocks[num + 1, (num2 + num3) / 16] == 58))
						{
							water[num + 1, num2 + num3] = 1;
							break;
						}
						if (num > 0 && water[num - 1, num2 + num3] != 1 && water[num - 1, num2 + num3] != 2 && (MySystem.blocks[num - 1, (num2 + num3) / 16] == 0 || MySystem.blocks[num - 1, (num2 + num3) / 16] == 56 || MySystem.blocks[num - 1, (num2 + num3) / 16] == 58))
						{
							water[num - 1, num2 + num3] = 1;
							break;
						}
						num3++;
						if (num2 + num3 > 319)
						{
							MySystem.gameOver = true;
							break;
						}
					}
				}
			}
			num4++;
			num2++;
			if (num2 >= 320)
			{
				num2 = 0;
				num++;
			}
		}
		bool flag = false;
		num = 0;
		num2 = 0;
		while (num < 10)
		{
			if (num2 < 319)
			{
				if ((water[num, num2 + 1] == 1 || water[num, num2 + 1] == 2) && water[num, num2] == 0 && (MySystem.blocks[num, num2 / 16] == 0 || MySystem.blocks[num, num2 / 16] == 56 || MySystem.blocks[num, num2 / 16] == 58) && !flag)
				{
					water[num, num2] = 1;
					flag = true;
				}
				else if (water[num, num2 + 1] == 0 && flag)
				{
					water[num, num2] = 0;
					flag = false;
				}
			}
			num2++;
			if (num2 >= 320)
			{
				if (flag)
				{
					water[num, 319] = 0;
				}
				num2 = 0;
				num++;
			}
		}
		num = 0;
		num2 = 0;
		int num5 = -1;
		num4 = DateTime.Now.Second;
		while (num < 10)
		{
			if (num2 > 0)
			{
				if (num5 == -1 && water[num, num2 - 1] == 0 && (water[num, num2] == 1 || water[num, num2] == 2) && (MySystem.blocks[num, (num2 - 1) / 16] == 0 || MySystem.blocks[num, (num2 - 1) / 16] == 56 || MySystem.blocks[num, (num2 - 1) / 16] == 58))
				{
					num5 = num2;
				}
				if (num5 != -1 && water[num, num2] == 0)
				{
					num5 = -1;
				}
				if (num4 % 2 == 1)
				{
					if (num > 0 && num5 == -1 && (water[num, num2] == 1 || water[num, num2] == 2) && water[num - 1, num2] != 1 && water[num - 1, num2] != 2 && (MySystem.blocks[num - 1, num2 / 16] == 0 || MySystem.blocks[num - 1, num2 / 16] == 56 || MySystem.blocks[num - 1, num2 / 16] == 58))
					{
						water[num, num2] = 0;
						water[num - 1, num2] = 1;
					}
				}
				else if (num < 9 && num5 == -1 && (water[num, num2] == 1 || water[num, num2] == 2) && water[num + 1, num2] != 1 && water[num + 1, num2] != 2 && (MySystem.blocks[num + 1, num2 / 16] == 0 || MySystem.blocks[num + 1, num2 / 16] == 56 || MySystem.blocks[num + 1, num2 / 16] == 58))
				{
					water[num, num2] = 0;
					water[num + 1, num2] = 1;
				}
			}
			num4++;
			num2++;
			if (num2 >= 320)
			{
				num2 = 0;
				num++;
			}
		}
		num = 0;
		num2 = 0;
		int num6 = 0;
		bool flag2 = true;
		while (num < 10)
		{
			if (water[num, num2 * 16 + num6] != 1)
			{
				flag2 = false;
			}
			num6++;
			if (num6 >= 16)
			{
				if (flag2)
				{
					MySystem.blocks[num, num2] = 58;
					Add(num, num2);
				}
				else if (MySystem.blocks[num, num2] == 58)
				{
					MySystem.blocks[num, num2] = 0;
					Add(num, num2);
				}
				flag2 = true;
				num6 = 0;
				num2++;
			}
			if (num2 >= 20)
			{
				num2 = 0;
				num++;
			}
		}
		ResetShowingWater();
	}

	public static void Reset_()
	{
		waterKids = GameObject.Find("WaterKids");
		int num = 0;
		int num2 = 0;
		while (num < 10)
		{
			water[num, num2] = 0;
			num2++;
			if (num2 >= 320)
			{
				num2 = 0;
				num++;
			}
		}
		ResetShowingWater();
	}

	public static void DeleteWater(int x, int y)
	{
		for (int i = 0; i < 16; i++)
		{
			water[x, i + y * 16] = 0;
		}
		Add(x, y);
		ResetShowingWater();
	}

	public static void AddWater(int x, int y)
	{
		for (int i = 0; i < 16; i++)
		{
			water[x, i + y * 16] = 1;
		}
		Add(x, y);
		ResetShowingWater();
	}

	public static void ResetShowingWater()
	{
		int num = 0;
		int num2 = 0;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		int num6 = 0;
		while (num < 10)
		{
			if (showingWater[num, num2] == 1 && num3 == -1)
			{
				num3 = num2;
			}
			if (showingWater[num, num2] == 0 && num3 != -1)
			{
				num4 = num3;
				num3 = -1;
			}
			if (showingWater[num, num2] == 1)
			{
				if (water[num, num2] != 1 && num3 != -1)
				{
					GameObject.Find("Water" + num + "," + num3).GetComponent<WaterKid>().Wei(num2 - num3);
				}
			}
			else if (showingWater[num, num2] == 0)
			{
				if (water[num, num2] == 1)
				{
					if (num5 == -1)
					{
						num5 = num2;
						num6++;
					}
					else
					{
						num6++;
					}
				}
				if (num2 < 319 && (water[num, num2 + 1] == 0 || showingWater[num, num2 + 1] == 1) && num5 != -1)
				{
					if (num5 > 0)
					{
						if (showingWater[num, num5 - 1] == 1)
						{
							WaterKid component = GameObject.Find("Water" + num + "," + num4).GetComponent<WaterKid>();
							component.n += num6;
							component.Hello();
							num5 = num4;
						}
						else
						{
							GameObject obj = UnityEngine.Object.Instantiate(MySystem.waterKids);
							obj.transform.SetParent(waterKids.transform);
							WaterKid component2 = obj.GetComponent<WaterKid>();
							component2.n = num6;
							component2.x = num;
							component2.y = num5;
							component2.Hello();
							num3 = num5;
						}
					}
					else
					{
						GameObject obj2 = UnityEngine.Object.Instantiate(MySystem.waterKids);
						obj2.transform.SetParent(waterKids.transform);
						WaterKid component3 = obj2.GetComponent<WaterKid>();
						component3.n = num6;
						component3.x = num;
						component3.y = num5;
						component3.Hello();
						num3 = num5;
					}
					if (num2 < 320 && showingWater[num, num2 + 1] == 1)
					{
						WaterKid component4 = GameObject.Find("Water" + num + "," + (num2 + 1)).GetComponent<WaterKid>();
						WaterKid component5 = GameObject.Find("Water" + num + "," + num5).GetComponent<WaterKid>();
						component5.n += component4.n;
						component4.GoodBye();
						component5.Hello();
						num3 = num5;
					}
					num5 = -1;
					num6 = 0;
				}
			}
			num2++;
			if (num2 >= 320)
			{
				num3 = -1;
				num2 = 0;
				num++;
			}
		}
	}

	public static void Add(int x, int y)
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
}
