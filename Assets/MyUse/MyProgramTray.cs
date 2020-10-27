using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProgramTray : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
	Tray tray;
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		tray = new Tray();
		tray.InitTray();
	}
	private void OnApplicationQuit()
	{
		tray?.Dispose();
		tray = null;
	}
#endif
}
