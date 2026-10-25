using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;

[Serializable]
public class AutoTestDataManager
{
	private string currentAutotestPath;

	[NonSerialized]
	public List<AutoTestData> LoadedAutoTests = new List<AutoTestData>();

	public List<FileInfo> AutoTestFiles { get; private set; }

	public string CurrentlyRunningAutotestPath { get; set; }

	public FileInfo CurrentlyRunningAutotestFile { get; set; }

	public async Task<string> GetCurrentAutotestPath()
	{
		if (currentAutotestPath == null)
		{
			currentAutotestPath = await RootSaveData.GetAutoTestPath();
		}
		return currentAutotestPath;
	}

	public void SetCurrentAutotestPath(string newPath)
	{
		SaveData.Instance.Global.LastAutotestFolder = newPath;
		currentAutotestPath = newPath;
	}

	public async Task<string> AutoTestSaveFolder(string fileName)
	{
		return Path.Combine(await GetCurrentAutotestPath(), fileName + ".testdat");
	}

	public AutoTestDataManager()
	{
		AutoTestFiles = new List<FileInfo>();
	}

	public async Task DetermineAvailableFilesFromLoadFolder()
	{
		string path = await GetCurrentAutotestPath();
		if (Directory.Exists(path))
		{
			AutoTestFiles.Clear();
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			AutoTestFiles = (from o in directoryInfo.GetFiles("*.testdat", SearchOption.AllDirectories)
				orderby Path.GetFileName(o.Name)
				select o).ToList();
		}
	}

	public async Task LoadAllAutoTestDataFromFile()
	{
		LoadedAutoTests.Clear();
		AutoTestFiles.Clear();
		string text = SaveData.Instance.AutoTestFolder;
		if (string.IsNullOrEmpty(text))
		{
			text = await GetCurrentAutotestPath();
		}
		string[] files = Directory.GetFiles(text, "*.testdat", SearchOption.AllDirectories);
		if (files.Length == 0)
		{
			throw new Exception("No autotest files found in " + text);
		}
		Array.Sort(files, new AlphanumComparatorFast());
		string[] array = files;
		foreach (string f in array)
		{
			using (FileStream fs = File.Open(f, FileMode.Open, FileAccess.Read))
			{
				SaveData.Instance.CurrentFileBeingDeserialized = f;
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				BinaryFormatter binaryFormatter2 = binaryFormatter;
				binaryFormatter2.Binder = new SerializationBinding(await SaveData.Instance.AssemblyInfoSaveDir);
				AutoTestData item = binaryFormatter.Deserialize(fs) as AutoTestData;
				LoadedAutoTests.Add(item);
				fs.Close();
			}
			AutoTestFiles.Add(new FileInfo(f));
		}
	}

	public async Task<AutoTestData> GetAutotestDataFromFile(FileInfo file)
	{
		try
		{
			using FileStream fs = File.Open(file.FullName, FileMode.Open, FileAccess.Read);
			SaveData.Instance.CurrentFileBeingDeserialized = file.FullName;
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			BinaryFormatter binaryFormatter2 = binaryFormatter;
			binaryFormatter2.Binder = new SerializationBinding(await SaveData.Instance.AssemblyInfoSaveDir);
			AutoTestData result = binaryFormatter.Deserialize(fs) as AutoTestData;
			fs.Close();
			return result;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Unable to load autotest file data.\n");
		}
		return null;
	}

	public async Task<bool> SaveAutoTestData(AutoTestData autoTestData, FileInfo file, bool saveReadableJson = false)
	{
		try
		{
			string fullName = file.FullName;
			string directoryName = Path.GetDirectoryName(fullName);
			if (!Directory.Exists(directoryName))
			{
				Log.Info("Creating auto test data directory: " + fullName);
				Directory.CreateDirectory(directoryName);
			}
			using (FileStream fileStream = File.Open(fullName, FileMode.Create, FileAccess.Write))
			{
				new BinaryFormatter().Serialize(fileStream, autoTestData);
				fileStream.Close();
			}
			if (saveReadableJson)
			{
				using FileStream serializationStream = File.Open(await AutoTestSaveFolder("JSON_" + Path.GetFileNameWithoutExtension(file.FullName) + ".json"), FileMode.Create, FileAccess.Write);
				string graph = JsonConvert.SerializeObject(autoTestData, Formatting.Indented);
				new BinaryFormatter().Serialize(serializationStream, graph);
			}
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred attempting to save Autotest data.\n");
			return false;
		}
	}
}
