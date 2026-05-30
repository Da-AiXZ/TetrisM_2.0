using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PhoneDebug : MonoBehaviour
{
	private static List<string> mLines = new List<string>();

	private static List<string> mWriteTxt = new List<string>();

	private string outpath;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		string path = DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
		outpath = Path.Combine(Application.persistentDataPath, path);
		if (File.Exists(outpath))
		{
			File.Delete(outpath);
		}
		File.Create(outpath);
		Application.logMessageReceived += HandleLog;
	}

	private void Start()
	{
		Debug.Log(outpath);
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Quit");
	}

	private void Update()
	{
		if (mWriteTxt.Count <= 0)
		{
			return;
		}
		string[] array = mWriteTxt.ToArray();
		using StreamWriter streamWriter = new StreamWriter(outpath, append: true, Encoding.UTF8);
		string[] array2 = array;
		foreach (string value in array2)
		{
			streamWriter.WriteLine(value);
		}
		mWriteTxt.Clear();
		streamWriter.Close();
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		mWriteTxt.Add(logString);
		mWriteTxt.Add(stackTrace);
		if (type == LogType.Log)
		{
			Log(logString);
		}
	}

	public static void Log(params object[] objs)
	{
		string text = "";
		for (int i = 0; i < objs.Length; i++)
		{
			text = ((i != 0) ? (text + ", " + objs[i].ToString()) : (text + objs[i].ToString()));
		}
		if (Application.isPlaying)
		{
			if (mLines.Count > 20)
			{
				mLines.RemoveAt(0);
			}
			mLines.Add(text);
		}
	}

	private void OnGUI()
	{
		GUI.color = Color.red;
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.normal.background = null;
		gUIStyle.normal.textColor = new Color(1f, 0f, 0f);
		gUIStyle.fontSize = 26;
		int i = 0;
		for (int count = mLines.Count; i < count; i++)
		{
			GUILayout.Label(mLines[i], gUIStyle);
		}
	}

	private void OnDestory()
	{
		Application.logMessageReceived -= HandleLog;
	}
}
