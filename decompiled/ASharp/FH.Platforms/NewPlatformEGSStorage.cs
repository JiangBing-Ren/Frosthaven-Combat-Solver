using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformEGSStorage : INewPlatformStorage
{
	private readonly NewPlatformEGSUser _user;

	private bool _isReady;

	private string _persistentDataPath;

	public bool IsReady => _isReady;

	internal NewPlatformEGSStorage(NewPlatformEGSUser user)
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
			throw new Exception("EGS is not ready to provide a user!");
		}
		return _user.PlatformPlayerID;
	}

	public string GetSaveDataPath()
	{
		throw new Exception("Wrong platform storage");
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
