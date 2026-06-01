using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySystem : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct keys
	{
		public static string w = "w";

		public static string a = "a";

		public static string s = "s";

		public static string d = "d";

		public static string up = "up";

		public static string left = "left";

		public static string down = "down";

		public static string right = "right";

		public static string space = "space";
	}

	public static bool isPhone = true;

	private static AsyncOperation scene;

	public static int score = 0;

	public static int bestScore = 0;

	public static int[,] blocks = new int[10, 25];

	public static int speed = 50;

	public static Sprite[] BlocksSprite;

	public static GameObject BlockMother;

	public static GameObject start;

	public static GameObject tnt_2;

	public static GameObject tnt;

	public static GameObject self;

	public static GameObject block;

	public static GameObject snowMan;

	public static GameObject ironMan;

	public static bool isStart = false;

	public static bool isHold = false;

	public static bool gameOver = false;

	public static bool[] types = new bool[7] { true, true, true, true, true, true, true };

	public static bool[,] ticks = new bool[10, 20];

	public static GameObject FallDowns;

	public static GameObject waterKids;

	public static GameObject lavaKids;

	public static int[] buttonDown = new int[3];

	public static int[] buttonDownb = new int[3];

	public static bool hadDown = false;

	public static bool hadTurn = false;

	public static bool hadGod;

	public static bool is1;

	public static bool is2;

	public static bool isBack = false;

	public static AudioSource sound;

	public static AudioSource sound2;

	public static AudioClip exp1;

	public static AudioClip exp2;

	public static AudioClip exp3;

	public static AudioClip exp4;

	public static AudioClip fuse;

	public static AudioClip cloth1;

	public static AudioClip cloth2;

	public static AudioClip cloth3;

	public static AudioClip cloth4;

	public static AudioClip grass1;

	public static AudioClip grass2;

	public static AudioClip grass3;

	public static AudioClip grass4;

	public static AudioClip gravel1;

	public static AudioClip gravel2;

	public static AudioClip gravel3;

	public static AudioClip gravel4;

	public static AudioClip sand1;

	public static AudioClip sand2;

	public static AudioClip sand3;

	public static AudioClip sand4;

	public static AudioClip stone1;

	public static AudioClip stone2;

	public static AudioClip stone3;

	public static AudioClip stone4;

	public static AudioClip wood1;

	public static AudioClip wood2;

	public static AudioClip wood3;

	public static AudioClip wood4;

	public static AudioClip water1;

	public static AudioClip water2;

	public static AudioClip water3;

	public static AudioClip lava1;

	public static AudioClip lava2;

	public static AudioClip lava3;

	public static AudioClip waterFall1;

	public static AudioClip waterFall2;

	public static AudioClip waterFall3;

	public static AudioClip lavaPop;

	public static AudioClip lavaFall;

	public static AudioClip glass1;

	public static AudioClip glass2;

	public static AudioClip glass3;

	public static AudioClip levelup1;

	public static AudioClip levelup2;

	public static AudioClip turn1;

	public static AudioClip turn2;

	public static AudioClip turn3;

	public static AudioClip turn4;

	public static AudioClip fizz;

	private int rx;

	private int ry;

	private void FixedUpdate()
	{
		if (isBack)
		{
			return;
		}
		if (blocks[rx, ry] != 0 && blocks[rx, ry] != 56 && blocks[rx, ry] != 58)
		{
			if ((bool)GameObject.Find(rx + "," + ry))
			{
				if (GameObject.Find(rx + "," + ry).GetComponent<Blocks>().id != 15 || blocks[rx, ry] != 2)
				{
					GameObject.Find(rx + "," + ry).GetComponent<Blocks>().id = blocks[rx, ry];
				}
				GameObject.Find(rx + "," + ry).GetComponent<Blocks>()._Reset();
			}
			else
			{
				GameObject obj = Object.Instantiate(block);
				obj.transform.position = new Vector2(-2.8f + (float)rx * 0.4f, -3.8f + (float)ry * 0.4f);
				obj.transform.SetParent(BlockMother.transform);
				Blocks component = obj.GetComponent<Blocks>();
				component.x = rx;
				component.y = ry;
				component.id = blocks[rx, ry];
				component._Reset();
			}
		}
		else if ((bool)GameObject.Find(rx + "," + ry))
		{
			Object.Destroy(GameObject.Find(rx + "," + ry));
		}
		ry++;
		if (ry >= 20)
		{
			ry = 0;
			rx++;
			if (rx >= 10)
			{
				rx = 0;
			}
		}
	}

	private static System.Collections.Generic.Queue<string> logQueue = new System.Collections.Generic.Queue<string>();
	private static bool logSending = false;
	
	public static void SendLog(string msg)
	{
		lock (logQueue) { logQueue.Enqueue("[" + System.DateTime.Now.ToString("HH:mm:ss") + "] " + msg); }
		if (!logSending) { logSending = true; FlushLogs(); }
	}
	
	private static async void FlushLogs()
	{
		while (true)
		{
			string batch = null;
			lock (logQueue)
			{
				if (logQueue.Count > 0)
				{
					var sb = new System.Text.StringBuilder();
					while (logQueue.Count > 0 && sb.Length < 4000)
						sb.AppendLine(logQueue.Dequeue());
					batch = sb.ToString();
				}
			}
			if (batch != null)
			{
				try
				{
					var www = UnityEngine.Networking.UnityWebRequest.Post("http://80.225.252.235:9999/log", batch, "text/plain");
					www.timeout = 3;
					var op = www.SendWebRequest();
					while (!op.isDone) await System.Threading.Tasks.Task.Yield();
					www.Dispose();
				}
				catch { }
			}
			else { logSending = false; break; }
		}
	}

	private void Start()
	{
		Reset_();
		// REPL moved to BootDiag.cs
	}

	private void StartRepl()
	{
		StartCoroutine(ReplLoop());
	}

	private System.Collections.IEnumerator ReplLoop()
	{
		yield return new WaitForSeconds(1f);
		string nextMsg = "REPL READY: " + SystemInfo.deviceModel + " scene=" + SceneManager.GetActiveScene().name;
		while (true)
		{
			string url = "http://80.225.252.235:9998/";
			WWWForm form = new WWWForm();
			form.AddField("d", nextMsg);
			using (var req = UnityWebRequest.Post(url, form))
			{
				req.timeout = 8;
				yield return req.SendWebRequest();
				if (req.result == UnityWebRequest.Result.Success)
				{
					string cmd = req.downloadHandler.text.Trim();
					if (cmd == "NOP" || string.IsNullOrEmpty(cmd))
						nextMsg = "ACK";
					else
					{
						try { nextMsg = ExecRepl(cmd); }
						catch (System.Exception ex) { nextMsg = "ERR: " + ex.Message; }
					}
				}
				else
					nextMsg = "NET_ERR: " + req.error;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private bool _guiReady = false;
	private void OnGUI()
	{
		GUI.color = Color.green;
		GUI.Label(new Rect(10, 10, 600, 30), "MySystem ALIVE | isStart=" + isStart + " | scene=" + SceneManager.GetActiveScene().name);
		GUI.Label(new Rect(10, 40, 600, 30), "isPhone=" + isPhone + " | gameOver=" + gameOver + " | isBack=" + isBack);
		var c = Camera.main;
		GUI.Label(new Rect(10, 70, 600, 30), "Camera=" + (c != null ? c.name + " pos=" + c.transform.position : "NULL"));
		var cv = UnityEngine.Object.FindObjectOfType<Canvas>();
		GUI.Label(new Rect(10, 100, 600, 30), "Canvas=" + (cv != null ? cv.name + " mode=" + cv.renderMode : "NULL"));
		if (!_guiReady) { _guiReady = true; SendLog("OnGUI first frame OK"); }
	}

	private string ExecRepl(string cmd)
	{
		var parts = cmd.Split(':');
		var op = parts[0];
		var arg = parts.Length > 1 ? cmd.Substring(op.Length + 1) : "";
		if (op == "FIND")
		{
			var go = GameObject.Find(arg);
			if (go == null) return "NULL";
			return go.name + " active=" + go.activeSelf + " layer=" + go.layer;
		}
		if (op == "CAM")
		{
			var c = Camera.main;
			if (c == null) return "Camera.main=NULL";
			return string.Format("cam={0} pos={1} size={2} clearFlags={3}",
				c.name, c.transform.position, c.orthographicSize, c.clearFlags);
		}
		if (op == "CANVAS")
		{
			var cvs = UnityEngine.Object.FindObjectsOfType<Canvas>();
			var sb = new StringBuilder();
			foreach (var cv in cvs)
				sb.AppendFormat("[{0} mode={1} enabled={2} sorting={3}] ",
					cv.name, cv.renderMode, cv.enabled, cv.sortingOrder);
			return sb.Length > 0 ? sb.ToString() : "NO_CANVAS";
		}
		if (op == "GET")
		{
			var dot = arg.LastIndexOf('.');
			if (dot < 0) return "BAD_GET";
			var goName = arg.Substring(0, dot);
			var field = arg.Substring(dot + 1);
			var go = GameObject.Find(goName);
			if (go == null) return goName + "=NULL";
			var comps = go.GetComponents<Component>();
			foreach (var c in comps)
			{
				if (c == null) continue;
				var fi = c.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				if (fi != null) return c.GetType().Name + "." + field + "=" + (fi.GetValue(c) ?? "null");
				var pi = c.GetType().GetProperty(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				if (pi != null) return c.GetType().Name + "." + field + "=" + (pi.GetValue(c) ?? "null");
			}
			return field + "=NOT_FOUND";
		}
		if (op == "SCENE")
		{
			return SceneManager.GetActiveScene().name + " rootCount=" + SceneManager.GetActiveScene().rootCount;
		}
		if (op == "ROOTS")
		{
			var roots = SceneManager.GetActiveScene().GetRootGameObjects();
			var sb = new StringBuilder();
			foreach (var r in roots)
				sb.Append(r.name).Append("|");
			return sb.ToString();
		}
		return "UNKNOWN_OP: " + op;
	}

	public static void ResetBlocks()
	{
		int num = 0;
		int num2 = 0;
		while (num < 10)
		{
			if (blocks[num, num2] != 0 && blocks[num, num2] != 56 && blocks[num, num2] != 58)
			{
				if ((bool)GameObject.Find(num + "," + num2))
				{
					if (GameObject.Find(num + "," + num2).GetComponent<Blocks>().id != 15 || blocks[num, num2] != 2)
					{
						GameObject.Find(num + "," + num2).GetComponent<Blocks>().id = blocks[num, num2];
					}
					GameObject.Find(num + "," + num2).GetComponent<Blocks>()._Reset();
				}
				else
				{
					GameObject obj = Object.Instantiate(block);
					obj.transform.position = new Vector2(-2.8f + (float)num * 0.4f, -3.8f + (float)num2 * 0.4f);
					obj.transform.SetParent(BlockMother.transform);
					Blocks component = obj.GetComponent<Blocks>();
					component.x = num;
					component.y = num2;
					component.id = blocks[num, num2];
					component._Reset();
				}
			}
			else if ((bool)GameObject.Find(num + "," + num2))
			{
				Object.Destroy(GameObject.Find(num + "," + num2));
			}
			num2++;
			if (num2 >= 20)
			{
				num2 = 0;
				num++;
			}
		}
	}

	private int frameCount = 0;

	private void Update()
	{
		if (frameCount == 0) SendLog("Update frame0, isStart=" + isStart + " gameOver=" + gameOver);
		if (frameCount == 60) SendLog("Update frame60, isStart=" + isStart + " gameOver=" + gameOver + " isBack=" + isBack);
		frameCount++;
		Summon(isFirst: false);
		Tick();
		if (Input.GetKey("joystick button 0"))
		{
			Debug.Log(1);
		}
		if (Input.GetKey("o") && Input.GetKey("p"))
		{
			if (Input.GetKeyDown("r"))
			{
				Reset_();
				score = -2333333;
			}
			if (Input.GetKeyDown("s"))
			{
				isStart = true;
				score = -2333333;
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// iOS: Quit not allowed
		}
		switch (buttonDownb[0])
		{
		case 0:
			buttonDown[0] = 0;
			break;
		case 1:
			buttonDown[0] = 1;
			break;
		case 2:
			buttonDown[0] = 2;
			break;
		case 3:
			buttonDown[0] = 3;
			break;
		case 4:
			buttonDown[0] = 4;
			break;
		}
		if (buttonDownb[1] == 1 && !is1)
		{
			buttonDown[1] = 1;
			is1 = true;
		}
		else if (buttonDownb[1] == 1 && is1)
		{
			buttonDown[1] = 0;
		}
		else if (buttonDownb[1] != 1 && is1)
		{
			is1 = false;
		}
		if (buttonDownb[2] == 1 && !is2)
		{
			buttonDown[2] = 1;
			is2 = true;
		}
		else if (buttonDownb[2] == 1 && is2)
		{
			buttonDown[2] = 0;
		}
		else if (buttonDownb[2] != 1 && is2)
		{
			is2 = false;
		}
		buttonDownb = new int[3];
		if (!isStart)
		{
			start.SetActive(value: true);
			if (Input.GetKeyDown(keys.space) || buttonDown[2] == 1)
			{
				Summon(isFirst: true);
				isStart = true;
				hadDown = true;
				start.SetActive(value: false);
			}
		}
		if (gameOver)
		{
			start.SetActive(value: true);
			Reset_();
		}
	}

	public static void Reset_()
	{
		SendLog("Reset_ begin");
		self = GameObject.Find("mySystem");
		SendLog("Reset_ self=" + (self != null ? self.name : "NULL"));
		while ((bool)self.GetComponent<AudioSource>())
		{
			Object.DestroyImmediate(self.GetComponent<AudioSource>());
		}
		sound = self.AddComponent<AudioSource>();
		sound2 = self.AddComponent<AudioSource>();
		sound2.volume = 0.2f;
		exp1 = Resources.Load("explode1") as AudioClip;
		exp2 = Resources.Load("explode2") as AudioClip;
		exp3 = Resources.Load("explode3") as AudioClip;
		exp4 = Resources.Load("explode4") as AudioClip;
		fuse = Resources.Load("fuse") as AudioClip;
		cloth1 = Resources.Load("cloth1") as AudioClip;
		cloth2 = Resources.Load("cloth2") as AudioClip;
		cloth3 = Resources.Load("cloth3") as AudioClip;
		cloth4 = Resources.Load("cloth4") as AudioClip;
		grass1 = Resources.Load("grass1") as AudioClip;
		grass2 = Resources.Load("grass2") as AudioClip;
		grass3 = Resources.Load("grass3") as AudioClip;
		grass4 = Resources.Load("grass4") as AudioClip;
		gravel1 = Resources.Load("gravel1") as AudioClip;
		gravel2 = Resources.Load("gravel2") as AudioClip;
		gravel3 = Resources.Load("gravel3") as AudioClip;
		gravel4 = Resources.Load("gravel4") as AudioClip;
		sand1 = Resources.Load("sand1") as AudioClip;
		sand2 = Resources.Load("sand2") as AudioClip;
		sand3 = Resources.Load("sand3") as AudioClip;
		sand4 = Resources.Load("sand4") as AudioClip;
		stone1 = Resources.Load("stone1") as AudioClip;
		stone2 = Resources.Load("stone2") as AudioClip;
		stone3 = Resources.Load("stone3") as AudioClip;
		stone4 = Resources.Load("stone4") as AudioClip;
		wood1 = Resources.Load("wood1") as AudioClip;
		wood2 = Resources.Load("wood2") as AudioClip;
		wood3 = Resources.Load("wood3") as AudioClip;
		wood4 = Resources.Load("wood4") as AudioClip;
		lava1 = Resources.Load("empty_lava1") as AudioClip;
		lava2 = Resources.Load("empty_lava2") as AudioClip;
		lava3 = Resources.Load("empty_lava3") as AudioClip;
		lavaPop = Resources.Load("lavapop") as AudioClip;
		lavaFall = Resources.Load("lava") as AudioClip;
		water1 = Resources.Load("empty1") as AudioClip;
		water2 = Resources.Load("empty2") as AudioClip;
		water3 = Resources.Load("empty3") as AudioClip;
		waterFall1 = Resources.Load("water1") as AudioClip;
		waterFall2 = Resources.Load("water2") as AudioClip;
		waterFall3 = Resources.Load("water3") as AudioClip;
		glass1 = Resources.Load("Glass_break1") as AudioClip;
		glass2 = Resources.Load("Glass_break2") as AudioClip;
		glass3 = Resources.Load("Glass_break3") as AudioClip;
		levelup1 = Resources.Load("levelup1") as AudioClip;
		levelup2 = Resources.Load("levelup2") as AudioClip;
		turn1 = Resources.Load("Weak_attack1") as AudioClip;
		turn2 = Resources.Load("Weak_attack2") as AudioClip;
		turn3 = Resources.Load("Weak_attack3") as AudioClip;
		turn4 = Resources.Load("Weak_attack4") as AudioClip;
		fizz = Resources.Load("Fizz") as AudioClip;
		isStart = false;
		isHold = false;
		start = GameObject.Find("start");
		gameOver = false;
		SendLog("Reset_ start=" + (start != null ? "OK" : "NULL"));
		FallDowns = Resources.Load("FallDowns") as GameObject;
		SendLog("Reset_ FallDowns=" + (FallDowns != null ? "OK" : "NULL"));
		block = Resources.Load("Block") as GameObject;
		SendLog("Reset_ Block=" + (block != null ? "OK" : "NULL"));
		tnt = Resources.Load("Tnt_1") as GameObject;
		tnt_2 = Resources.Load("Tnt_2") as GameObject;
		snowMan = Resources.Load("snowMan_1") as GameObject;
		ironMan = Resources.Load("ironMan_1") as GameObject;
		waterKids = Resources.Load("waterKids") as GameObject;
		lavaKids = Resources.Load("lavaKids") as GameObject;
		types = new bool[7] { true, true, true, true, true, true, true };
		// AssetRipper sprite sheet fix: create sprites at runtime from texture
		BlocksSprite = new Sprite[112];
		Texture2D blocksTex = Resources.Load<Texture2D>("Blocks");
		if (blocksTex != null)
		{
			int[,] rects = new int[,] {
				{ 15, 17, 102, 16, 16 },
				{ 16, 34, 102, 16, 16 },
				{ 17, 51, 102, 16, 16 },
				{ 18, 68, 102, 16, 16 },
				{ 19, 85, 102, 16, 16 },
				{ 20, 102, 102, 16, 16 },
				{ 21, 119, 102, 16, 16 },
				{ 22, 136, 102, 16, 16 },
				{ 23, 153, 102, 16, 16 },
				{ 24, 170, 102, 16, 16 },
				{ 25, 187, 102, 16, 16 },
				{ 26, 204, 102, 16, 16 },
				{ 27, 221, 102, 16, 16 },
				{ 42, 0, 68, 16, 16 },
				{ 43, 17, 68, 16, 16 },
				{ 44, 34, 68, 16, 16 },
				{ 45, 51, 68, 16, 16 },
				{ 46, 68, 68, 16, 16 },
				{ 47, 85, 68, 16, 16 },
				{ 49, 119, 68, 16, 16 },
				{ 50, 136, 68, 16, 16 },
				{ 51, 153, 68, 16, 16 },
				{ 53, 187, 68, 16, 16 },
				{ 54, 204, 68, 16, 16 },
				{ 55, 221, 68, 16, 16 },
				{ 70, 0, 34, 16, 16 },
				{ 71, 17, 34, 16, 16 },
				{ 72, 34, 34, 16, 16 },
				{ 73, 51, 34, 16, 16 },
				{ 74, 68, 34, 16, 16 },
				{ 75, 85, 34, 16, 16 },
				{ 76, 102, 34, 16, 16 },
				{ 77, 119, 34, 16, 16 },
				{ 78, 136, 34, 16, 16 },
				{ 79, 153, 34, 16, 16 },
				{ 80, 170, 34, 16, 16 },
				{ 81, 187, 34, 16, 16 },
				{ 82, 204, 34, 16, 16 },
				{ 83, 221, 34, 16, 16 },
				{ 98, 0, 0, 16, 16 },
				{ 99, 17, 0, 16, 16 },
				{ 100, 34, 0, 16, 16 },
				{ 101, 51, 0, 16, 16 },
				{ 102, 68, 0, 16, 16 },
				{ 103, 85, 0, 16, 16 },
				{ 104, 102, 0, 16, 16 },
				{ 105, 119, 0, 16, 16 },
				{ 106, 136, 0, 16, 16 },
				{ 107, 153, 0, 16, 16 },
				{ 108, 170, 0, 16, 16 },
				{ 109, 187, 0, 16, 16 },
				{ 110, 204, 0, 16, 16 },
				{ 111, 221, 0, 16, 16 }
			};
			for (int i = 0; i < rects.GetLength(0); i++)
			{
				int sid = rects[i,0];
				if (sid < BlocksSprite.Length)
					BlocksSprite[sid] = Sprite.Create(blocksTex,
						new Rect(rects[i,1], blocksTex.height - rects[i,2] - rects[i,4], rects[i,3], rects[i,4]),
						new Vector2(0.5f, 0.5f), 100f);
			}
			// Fill missing IDs with nearest available sprite
			for (int i = 0; i < BlocksSprite.Length; i++)
			{
				if (BlocksSprite[i] != null) continue;
				// Find nearestnon-null sprite
				for (int d = 1; d < 20; d++)
				{
					if (i + d < BlocksSprite.Length && BlocksSprite[i + d] != null) { BlocksSprite[i] = BlocksSprite[i + d]; break; }
					if (i - d >=0 && BlocksSprite[i - d] != null) { BlocksSprite[i] = BlocksSprite[i - d]; break; }
				}
			}
		}
		string text = RFileS("/Score", "/BestScore.cup", 1024);
		buttonDownb = new int[3];
		buttonDown = new int[3];
		Water.Reset_();
		Lava.Reset_();
		if (text != "error")
		{
			bestScore = int.Parse(text);
		}
		else
		{
			WFileS("/Score", "/BestScore.cup", "0");
		}
		if (score > bestScore)
		{
			WFileS("/Score", "/BestScore.cup", score.ToString());
			bestScore = score;
		}
		score = 0;
		GameObject.Find("Canvas").GetComponent<Score>()._Reset();
		string text2 = RFileS("/key/", "key.txt", 1024);
		Physics.simulationMode = SimulationMode.Script;
		if (text2 != "error")
		{
			string[] array = text2.Split(',');
			keys.w = array[1];
			keys.a = array[4];
			keys.s = array[10];
			keys.d = array[7];
			keys.up = array[2];
			keys.left = array[5];
			keys.down = array[11];
			keys.right = array[8];
			keys.space = array[13];
		}
		else
		{
			WFileS("/key/", "key.txt", "转向,w,up,左移,a,left,右移,d,right,下降,s,down,下落,space,\n如果要改动 请不要改变任何逗号 仅第一行有实际作用 中文仅作为提示作用 使用英文标点\n键盘代码见https://blog.csdn.net/KJHKAHDKH/article/details/116360027 删除此文件自动恢复");
		}
		int num = 0;
		int num2 = 0;
		int[,] array2 = blocks;
		int upperBound = array2.GetUpperBound(0);
		int upperBound2 = array2.GetUpperBound(1);
		for (int i = array2.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = array2.GetLowerBound(1); j <= upperBound2; j++)
			{
				_ = array2[i, j];
				blocks[num, num2] = 0;
				num2++;
				if (num2 > 24)
				{
					num2 = 0;
					num++;
					if (num > 9)
					{
						goto end_IL_06df;
					}
				}
			}
			continue;
			end_IL_06df:
			break;
		}
		num = 0;
		num2 = 0;
		bool[,] array3 = ticks;
		upperBound2 = array3.GetUpperBound(0);
		upperBound = array3.GetUpperBound(1);
		for (int i = array3.GetLowerBound(0); i <= upperBound2; i++)
		{
			for (int j = array3.GetLowerBound(1); j <= upperBound; j++)
			{
				_ = array3[i, j];
				ticks[num, num2] = false;
				num2++;
				if (num2 > 19)
				{
					num2 = 0;
					num++;
					if (num > 9)
					{
						goto end_IL_075b;
					}
				}
			}
			continue;
			end_IL_075b:
			break;
		}
		while ((bool)GameObject.Find("Showing"))
		{
			Object.DestroyImmediate(GameObject.Find("Showing"));
		}
		while ((bool)GameObject.Find("Tnt_1"))
		{
			Object.DestroyImmediate(GameObject.Find("Tnt_1"));
		}
		while ((bool)GameObject.Find("FallDowns"))
		{
			Object.DestroyImmediate(GameObject.Find("FallDowns"));
		}
		while ((bool)GameObject.Find("BlockMother"))
		{
			Object.DestroyImmediate(GameObject.Find("BlockMother"));
		}
		BlockMother = new GameObject();
		BlockMother.name = "BlockMother";
	}

	public static void ButtonDown(int inp)
	{
		switch (inp)
		{
		case 1:
			buttonDownb[0] = 1;
			break;
		case 2:
			buttonDownb[0] = 2;
			break;
		case 3:
			buttonDownb[0] = 3;
			break;
		case 4:
			buttonDownb[0] = 4;
			break;
		case 5:
			buttonDownb[1] = 1;
			break;
		case 6:
			buttonDownb[2] = 1;
			break;
		}
	}

	public static void Summon(bool isFirst)
	{
		if (isStart && !isHold)
		{
			GameObject.Find("Showing").GetComponent<FallDown>().isShow = false;
			GameObject.Find("Showing").GetComponent<FallDown>().EndShow();
			int num = 0;
			bool[] array = types;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					num++;
				}
			}
			if (num == 0)
			{
				types = new bool[7] { true, true, true, true, true, true, true };
			}
			num = Random.Range(0, 7);
			while (!types[num])
			{
				num = Random.Range(0, 7);
			}
			types[num] = false;
			Object.Instantiate(FallDowns).GetComponent<FallDown>().type = num;
		}
		else
		{
			if (!isFirst)
			{
				return;
			}
			int num2 = 0;
			bool[] array = types;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					num2++;
				}
			}
			if (num2 == 0)
			{
				types = new bool[7] { true, true, true, true, true, true, true };
			}
			num2 = Random.Range(0, 7);
			while (!types[num2])
			{
				num2 = Random.Range(0, 7);
			}
			types[num2] = false;
			GameObject obj = Object.Instantiate(FallDowns);
			obj.GetComponent<FallDown>().type = num2;
			obj.GetComponent<FallDown>().isShow = false;
			obj.GetComponent<FallDown>().EndShow();
			num2 = 0;
			array = types;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					num2++;
				}
			}
			if (num2 == 0)
			{
				types = new bool[7] { true, true, true, true, true, true, true };
			}
			num2 = Random.Range(0, 7);
			while (!types[num2])
			{
				num2 = Random.Range(0, 7);
			}
			types[num2] = false;
			Object.Instantiate(FallDowns).GetComponent<FallDown>().type = num2;
		}
	}

	public static void Tick()
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		while (num2 < 20)
		{
			if (ticks[num, num2] && !isBack)
			{
				BGMM.FakeUpdate();
				flag = true;
				if (blocks[num, num2] != 0 && blocks[num, num2] != 56 && blocks[num, num2] != 58)
				{
					GameObject.Find(num + "," + num2).GetComponent<Blocks>().Tick();
				}
			}
			ticks[num, num2] = false;
			num++;
			if (num > 9)
			{
				num = 0;
				num2++;
			}
		}
		if (flag)
		{
			Check();
		}
	}

	public static void Check()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool[] array = new bool[20];
		int[,] array2 = blocks;
		int upperBound = array2.GetUpperBound(0);
		int upperBound2 = array2.GetUpperBound(1);
		for (int i = array2.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = array2.GetLowerBound(1); j <= upperBound2; j++)
			{
				_ = array2[i, j];
				if (blocks[num, num2] != 0)
				{
					num3++;
				}
				num++;
				if (num > 9)
				{
					if (num3 == 10)
					{
						array[num2] = true;
					}
					num3 = 0;
					num = 0;
					num2++;
					if (num2 > 19)
					{
						goto end_IL_0090;
					}
				}
			}
			continue;
			end_IL_0090:
			break;
		}
		num3 = 0;
		num2 = 0;
		bool[] array3 = array;
		foreach (bool num4 in array3)
		{
			num = 0;
			if (num4)
			{
				num3++;
				for (; num < 10; num++)
				{
					if (blocks[num, num2] != 13)
					{
						if (blocks[num, num2] == 56)
						{
							Lava.DeleteWater(num, num2);
						}
						else if (blocks[num, num2] == 58)
						{
							Water.DeleteWater(num, num2);
						}
						else
						{
							Object.Destroy(GameObject.Find(num + "," + num2));
						}
					}
					else if (!isBack)
					{
						GameObject.Find(num + "," + num2).GetComponent<Blocks>().Back12();
					}
					blocks[num, num2] = 0;
				}
			}
			else if (num3 > 0)
			{
				FallAbove(num2 - 1, num3);
				if (num3 > 2)
				{
					sound.PlayOneShot(levelup2);
				}
				else
				{
					sound.PlayOneShot(levelup1);
				}
				score += (int)Mathf.Pow(2f, num3) * 100;
				Score.beLing = 0.9f;
				num3 = 0;
			}
			num2++;
		}
		if (num3 > 0)
		{
			FallAbove(0, num3);
			score += (int)Mathf.Pow(2f, num3) * 100;
			Score.beLing = 0.9f;
		}
	}

	public static void FallAbove(int y, int total)
	{
		GameObject.Find("Main Camera").GetComponent<MyCamera>().Shake(0f, 0.4f);
		int num = 0;
		while (y + num < 20)
		{
			num++;
			for (int i = 0; i < 10; i++)
			{
				if (blocks[i, y + num] != 0 && blocks[i, y + num] != 56 && blocks[i, y + num] != 58)
				{
					GameObject.Find(i + "," + (y + num)).GetComponent<Blocks>().Fall(y, total);
				}
			}
		}
		ResetBlocks();
	}

	public static void StartLoadScene(string name)
	{
		scene = SceneManager.LoadSceneAsync(name);
		scene.allowSceneActivation = false;
	}

	public static void LoadScene()
	{
		scene.allowSceneActivation = true;
	}

	public static void WFileS(string folder, string file, string inof)
	{
		if (!Directory.Exists(Application.persistentDataPath + folder))
		{
			Directory.CreateDirectory(Application.persistentDataPath + folder);
		}
		if (!File.Exists(Application.persistentDataPath + folder + file))
		{
			FileStream fileStream = File.Create(Application.persistentDataPath + folder + file);
			fileStream.Close();
			fileStream.Dispose();
		}
		byte[] bytes = Encoding.UTF8.GetBytes(inof);
		FileStream fileStream2 = File.OpenWrite(Application.persistentDataPath + folder + file);
		fileStream2.Write(bytes, 0, bytes.Length);
		fileStream2.Close();
		fileStream2.Dispose();
	}

	public static string RFileS(string folder, string file, int length)
	{
		if (!Directory.Exists(Application.persistentDataPath + folder))
		{
			return "error";
		}
		if (!File.Exists(Application.persistentDataPath + folder + file))
		{
			return "error";
		}
		FileStream fileStream = File.OpenRead(Application.persistentDataPath + folder + file);
		byte[] array = new byte[length + 1];
		fileStream.Read(array, 0, length);
		fileStream.Close();
		fileStream.Dispose();
		return Encoding.UTF8.GetString(array);
	}
}