using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformPS5Storage : INewPlatformStorage
{
	private readonly NewPlatformPS5User _user;

	private bool _isReady;

	private string _persistentDataPath;

	public bool IsReady => _isReady;

	internal NewPlatformPS5Storage(NewPlatformPS5User user)
	{
		_user = user;
	}

	public IEnumerator InitCrt()
	{
		_isReady = true;
		_persistentDataPath = Application.persistentDataPath;
		yield break;
	}

	public string GetPersistentDataPath()
	{
		return _persistentDataPath;
	}

	private string GetUserID()
	{
		if (!IsReady)
		{
			throw new Exception("PS5 is not ready to provide a user!");
		}
		return _user?.PlatformPlayerID ?? "0";
	}

	public string GetSaveDataPath()
	{
		return Path.Combine(GetPersistentDataPath(), GetUserID());
	}

	public bool LoadFromFile(string filename, out object data)
	{
		return RuntimeTools.LoadFromFile(Path.Combine(GetSaveDataPath(), filename), out data);
	}

	public bool SaveToFile(string filename, object data)
	{
		return RuntimeTools.SaveToFile(GetSaveDataPath(), filename, data);
	}
}
