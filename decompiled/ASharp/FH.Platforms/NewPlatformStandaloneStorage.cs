using System.Collections;
using System.IO;
using UnityEngine;

namespace FH.Platforms;

public class NewPlatformStandaloneStorage : INewPlatformStorage
{
	public string _persistentDataPath;

	public bool IsReady { get; private set; }

	public string GetPersistentDataPath()
	{
		if (!string.IsNullOrEmpty(EditorProperties.Instance.CustomEditorDataPath))
		{
			return EditorProperties.Instance.CustomEditorDataPath;
		}
		return _persistentDataPath;
	}

	public string GetSaveDataPath()
	{
		if (Application.isEditor)
		{
			string name = Directory.GetParent(Application.dataPath).Name;
			return Path.Combine(GetPersistentDataPath(), "Unity/" + name);
		}
		return Path.Combine(GetPersistentDataPath(), "Local");
	}

	public IEnumerator InitCrt()
	{
		_persistentDataPath = Application.persistentDataPath;
		IsReady = true;
		yield break;
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
