using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeensManagement : MonoBehaviour
{
	private string fold;

	private string last;

	private string toDay;

	private string host;

	private int now;

	private int endH;

	private int endM;

	private bool hasConnect;

	private void End()
	{
		SceneManager.LoadScene(2);
	}

	private void End2()
	{
		WFileS("/Limit", toDay, "0:0\n");
		endM = 0;
		endH = 0;
	}

	private void resetEnd()
	{
		string text = RFileS("/Limit", toDay, 256);
		if (text == "error")
		{
			WFileS("/Limit", toDay, "25:61\n");
			endM = 61;
			endH = 25;
		}
		else
		{
			string[] array = text.Split(':', '\n');
			int.TryParse(array[0], out endH);
			int.TryParse(array[1], out endM);
		}
	}

	private void Start()
	{
		fold = Application.persistentDataPath.Remove(Application.persistentDataPath.LastIndexOf('/'));
		fold += "/TeenManagement_test_0.1";
		toDay = "/" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + ".time";
		last = RFileS("/Time", toDay, 65536);
		if (last == "error")
		{
			last = "";
		}
		else
		{
			last = last.Remove(last.LastIndexOf("\n")) + "\n";
		}
		last = last + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ",TetrisM:open;\n";
		WFileS("/Time", toDay, last);
		now = 233;
		host = string.Empty;
		IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily.ToString() == "InterNetwork")
			{
				host = iPAddress.ToString();
			}
		}
		new Thread(StartServer).Start();
		resetEnd();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (now != DateTime.Now.Minute)
		{
			WFileS("/Time", toDay, last + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ",TetrisM:close;\n");
			now = DateTime.Now.Minute;
		}
		if ((DateTime.Now.Hour > endH || (DateTime.Now.Minute >= endM && DateTime.Now.Hour == endH)) && !MySystem.isStart && hasConnect)
		{
			End();
		}
	}

	private void StartServer()
	{
		int port = 23300;
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
		socket.Listen(2);
		while (true)
		{
			try
			{
				Socket parameter = socket.Accept();
				new Thread(Receive).Start(parameter);
				Debug.Log("接受新连接");
			}
			catch
			{
			}
		}
	}

	private void Receive(object socket)
	{
		Socket socket2 = (Socket)socket;
		while (true)
		{
			try
			{
				byte[] array = new byte[65536];
				socket2.Receive(array);
				string[] array2 = Encoding.UTF8.GetString(array).Split(",");
				if (array2[0] == "检查")
				{
					socket2.Send(Encoding.UTF8.GetBytes(RFileS("/Time", "/" + array2[1] + ".time", 65536)));
				}
				else if (array2[0] == "设置")
				{
					WFileS("/Limit", toDay, array2[1] + "\n");
					resetEnd();
					socket2.Send(Encoding.UTF8.GetBytes("OK."));
				}
				else if (array2[0] == "强制结束")
				{
					End2();
					socket2.Send(Encoding.UTF8.GetBytes("OK."));
				}
				else
				{
					socket2.Send(Encoding.UTF8.GetBytes("?."));
				}
				hasConnect = true;
			}
			catch
			{
			}
		}
	}

	private void WFileS(string folder, string file, string inof)
	{
		if (!Directory.Exists(fold + folder))
		{
			Directory.CreateDirectory(fold + folder);
		}
		if (!File.Exists(fold + folder + file))
		{
			FileStream fileStream = File.Create(fold + folder + file);
			fileStream.Close();
			fileStream.Dispose();
		}
		byte[] bytes = Encoding.UTF8.GetBytes(inof);
		FileStream fileStream2 = File.OpenWrite(fold + folder + file);
		fileStream2.Write(bytes, 0, bytes.Length);
		fileStream2.Close();
		fileStream2.Dispose();
	}

	private string RFileS(string folder, string file, int length)
	{
		if (!Directory.Exists(fold + folder))
		{
			return "error";
		}
		if (!File.Exists(fold + folder + file))
		{
			return "error";
		}
		FileStream fileStream = File.OpenRead(fold + folder + file);
		byte[] array = new byte[length + 1];
		fileStream.Read(array, 0, length);
		fileStream.Close();
		fileStream.Dispose();
		return Encoding.UTF8.GetString(array);
	}
}
