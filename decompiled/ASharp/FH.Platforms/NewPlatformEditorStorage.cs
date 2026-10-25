using System.Collections;
using System.IO;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformEditorStorage : INewPlatformStorage
{
	private string _persistentDataPath => Application.persistentDataPath;

	public bool IsReady => true;

	internal NewPlatformEditorStorage()
	{
	}

	public IEnumerator InitCrt()
	{
		yield break;
	}

	public string GetPersistentDataPath()
	{
		return _persistentDataPath;
	}

	private string GetUserID()
	{
		return "0";
	}

	public string GetSaveDataPath()
	{
		string name = Directory.GetParent(Application.dataPath).Name;
		return Path.Combine(GetPersistentDataPath(), "Unity/" + name);
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
