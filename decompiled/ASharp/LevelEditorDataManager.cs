using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using GLOOM;
using Newtonsoft.Json;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;

[Serializable]
public class LevelEditorDataManager
{
	public enum ELevelFileSortType
	{
		Filename,
		DateModified,
		Extension
	}

	public const string cCustomLevelFolderPrefix = "CustomLevel_";

	public const string cWorkshopLevelFolderPrefix = "Workshop_";

	public const string cCustomLevelPreviewImageDir = "CustomLevelPreviewImages";

	public const string cCustomLevelPreviewImageFileName = "preview.png";

	public Dictionary<ulong, string> AvailableWorkshopLevels;

	public List<string> LevelEditorFileNames { get; private set; }

	public List<string> YMLFileNames { get; private set; }

	public List<FileInfo> AvailableFiles { get; private set; }

	public static string DefaultLevelThumbnailPath => Path.Combine(Application.streamingAssetsPath, "CustomLevelPreviewImages", "preview.png");

	public async Task<string> LevelDataSaveFilePath(string levelName, bool useCustom = false)
	{
		if (!useCustom)
		{
			return Path.Combine(await RootSaveData.LevelEditorCustomLVLDAT, "CustomLevel_" + levelName, levelName + ".lvldat");
		}
		string customLevelDataFolder = SaveData.Instance.Global.CustomLevelDataFolder;
		if (string.IsNullOrEmpty(customLevelDataFolder))
		{
			throw new Exception("Custom level data path is null or empty.");
		}
		return Path.Combine(customLevelDataFolder, levelName + ".lvldat");
	}

	public Task<string> WorkshopLevelSaveFilePath(string levelName, string workshopId)
	{
		return FHTaskExtensions.CombineAsyncPaths(RootSaveData.LevelEditorCustomLVLDAT, "Workshop_" + workshopId + "_" + levelName, levelName + ".lvldat");
	}

	public bool TrySetCustomLevelDataPath(string pathToSet, out string errorLog)
	{
		if (string.IsNullOrEmpty(pathToSet))
		{
			errorLog = "Cannot set Null path";
			return false;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(pathToSet);
		if (directoryInfo.Exists)
		{
			bool flag = true;
			AuthorizationRuleCollection accessRules = Directory.GetAccessControl(pathToSet).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
			if (directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
			{
				flag = false;
			}
			else
			{
				foreach (FileSystemAccessRule item in accessRules)
				{
					if (item.FileSystemRights.HasFlag(FileSystemRights.Write) && item.AccessControlType == AccessControlType.Deny)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				SaveData.Instance.Global.CustomLevelDataFolder = pathToSet;
				SaveData.Instance.SaveGlobalData();
				errorLog = string.Empty;
				return true;
			}
			errorLog = "Write access is denied to that folder, please choose another one.";
			return false;
		}
		errorLog = "Path does not exist, create it first";
		return false;
	}

	public LevelEditorDataManager()
	{
		LevelEditorFileNames = new List<string>();
		YMLFileNames = new List<string>();
		AvailableFiles = new List<FileInfo>();
		AvailableWorkshopLevels = new Dictionary<ulong, string>();
	}

	public async void DetermineAvailableFilesFromLoadFolder(bool useCustomLoadFolder = false)
	{
		LevelEditorFileNames.Clear();
		YMLFileNames.Clear();
		AvailableFiles.Clear();
		if (!useCustomLoadFolder)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(RootSaveData.RulebaseCustomLevelEditorScenarios);
			if (directoryInfo.Exists)
			{
				AddFiles(directoryInfo.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
			DirectoryInfo directoryInfo2 = new DirectoryInfo(RootSaveData.RulebaseCustomYMLScenarios);
			if (directoryInfo2.Exists)
			{
				AddFiles(directoryInfo2.GetFiles("*.yml", SearchOption.AllDirectories), isYML: true);
			}
			DirectoryInfo directoryInfo3 = new DirectoryInfo(await RootSaveData.LevelEditorCustomLVLDAT);
			if (directoryInfo3.Exists)
			{
				AddFiles(directoryInfo3.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
			DirectoryInfo directoryInfo4 = new DirectoryInfo(await RootSaveData.LevelEditorCustomYML);
			if (directoryInfo4.Exists)
			{
				AddFiles(directoryInfo4.GetFiles("*.yml", SearchOption.AllDirectories), isYML: true);
			}
		}
		else if (!string.IsNullOrEmpty(SaveData.Instance.Global.CustomLevelDataFolder))
		{
			DirectoryInfo directoryInfo5 = new DirectoryInfo(SaveData.Instance.Global.CustomLevelDataFolder);
			if (directoryInfo5.Exists)
			{
				AddFiles(directoryInfo5.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
		}
		_ = Application.isPlaying;
	}

	public void AddFiles(FileInfo[] files, bool isYML)
	{
		if (files.Length != 0)
		{
			AvailableFiles.AddRange(files);
			string[] array = files.Select((FileInfo s) => s.FullName).ToArray();
			try
			{
				Array.Sort(array, new AlphanumComparatorFast());
			}
			catch
			{
			}
			if (isYML)
			{
				YMLFileNames.AddRange(array);
			}
			else
			{
				LevelEditorFileNames.AddRange(array);
			}
		}
	}

	public void SortAvailableFiles(ELevelFileSortType sortType, bool ascending)
	{
		if (AvailableFiles == null || AvailableFiles.Count == 0)
		{
			return;
		}
		switch (sortType)
		{
		case ELevelFileSortType.Filename:
			AvailableFiles.Sort((FileInfo x, FileInfo y) => (!ascending) ? y.Name.CompareTo(x.Name) : x.Name.CompareTo(y.Name));
			break;
		case ELevelFileSortType.DateModified:
			AvailableFiles.Sort((FileInfo x, FileInfo y) => (!ascending) ? DateTime.Compare(y.LastWriteTime, x.LastWriteTime) : DateTime.Compare(x.LastWriteTime, y.LastWriteTime));
			break;
		case ELevelFileSortType.Extension:
			AvailableFiles.Sort((FileInfo x, FileInfo y) => (!ascending) ? y.Extension.CompareTo(x.Extension) : x.Extension.CompareTo(y.Extension));
			break;
		}
	}

	public async Task<(bool success, string path)> SaveLevelEditorBaseData(CCustomLevelData baseLevelData, string levelName, bool saveReadableJson = false, string filePathOverride = null)
	{
		baseLevelData.UpdateScenarioStateID();
		bool usingCustomLevelFolder = ((bool)SceneController.Instance && SaveData.Instance.Global.UseCustomLevelDataFolder) || filePathOverride != null;
		string levelSavePath = (string.IsNullOrEmpty(filePathOverride) ? (await LevelDataSaveFilePath(levelName, usingCustomLevelFolder)) : filePathOverride);
		string text = ((!saveReadableJson) ? string.Empty : (await LevelDataSaveFilePath("JSON_" + levelName, usingCustomLevelFolder)));
		string path = text;
		try
		{
			string directoryName = Path.GetDirectoryName(levelSavePath);
			if (directoryName != null)
			{
				Directory.CreateDirectory(directoryName);
			}
			using (FileStream fileStream = File.Open(levelSavePath, FileMode.Create, FileAccess.Write))
			{
				new BinaryFormatter().Serialize(fileStream, baseLevelData);
				fileStream.Close();
			}
			if ((bool)SceneController.Instance && !usingCustomLevelFolder)
			{
				string text2 = Path.Combine(Path.GetDirectoryName(levelSavePath), "preview.png");
				if (!File.Exists(text2) && File.Exists(DefaultLevelThumbnailPath))
				{
					File.Copy(DefaultLevelThumbnailPath, text2);
				}
			}
			if (saveReadableJson)
			{
				using FileStream serializationStream = File.Open(path, FileMode.Create, FileAccess.Write);
				string graph = JsonConvert.SerializeObject(baseLevelData, Formatting.Indented);
				new BinaryFormatter().Serialize(serializationStream, graph);
			}
			return (success: true, path: levelSavePath);
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred attempting to save LevelEditorBase data.");
			return (success: false, path: "");
		}
	}

	public async Task<CCustomLevelData> GetLevelDataForFile(FileInfo file)
	{
		string fullPath = Path.GetFullPath(file.FullName);
		if (File.Exists(fullPath))
		{
			if (file.Extension == ".lvldat")
			{
				try
				{
					CCustomLevelData result = null;
					using (FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read))
					{
						SaveData.Instance.CurrentFileBeingDeserialized = fullPath;
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						BinaryFormatter binaryFormatter2 = binaryFormatter;
						binaryFormatter2.Binder = new SerializationBinding(await SaveData.Instance.AssemblyInfoSaveDir);
						result = binaryFormatter.Deserialize(fs) as CCustomLevelData;
					}
					return result;
				}
				catch (Exception exception)
				{
					Log.Exception(exception, "Unable to get LevelData object from path.");
				}
			}
			else if (file.Extension == ".yml")
			{
				ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.Scenarios.SingleOrDefault((ScenarioDefinition s) => Path.GetFileName(s.FileName) == file.Name);
				if (scenarioDefinition != null)
				{
					return new CCustomLevelData(LocalizationManager.GetTranslation(scenarioDefinition.ID), scenarioDefinition);
				}
				Log.Error("Unable to find Custom Scenario " + file.Name);
			}
			else
			{
				Log.Error("Invlid file extension for custom file '" + file.Extension + "'");
			}
		}
		return null;
	}

	public object SaveLevelEditorBaseData(CCustomLevelData currentEditorLevelData, ReadOnlySpan<char> readOnlySpan, bool usingCustomLevelFolder)
	{
		throw new NotImplementedException();
	}

	public async Task<CCustomLevelData> GetLevelDataForFile(string filePath)
	{
		return await GetLevelDataForFile(new FileInfo(filePath));
	}
}
