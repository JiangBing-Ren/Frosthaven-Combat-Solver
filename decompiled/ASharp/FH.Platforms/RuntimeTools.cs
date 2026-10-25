using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

namespace FH.Platforms;

public static class RuntimeTools
{
	public static IEnumerable<Scene> GetCurrentActiveScenes()
	{
		int count = SceneManager.sceneCount;
		for (int i = 0; i < count; i++)
		{
			yield return SceneManager.GetSceneAt(i);
		}
	}

	internal static bool LoadFromFile(string filePath, out object data)
	{
		data = null;
		if (!File.Exists(filePath))
		{
			return false;
		}
		try
		{
			using (FileStream serializationStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				data = binaryFormatter.Deserialize(serializationStream);
			}
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Failed to load data from " + filePath + ".");
			return false;
		}
	}

	internal static bool SaveToFile(string directory, string filename, object data)
	{
		string text = Path.Combine(directory, filename);
		try
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			using (FileStream serializationStream = File.Open(text, FileMode.Create, FileAccess.Write))
			{
				new BinaryFormatter().Serialize(serializationStream, data);
			}
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, $"An exception occurred attempting to save data: {data} in {text}.");
			return false;
		}
	}
}
