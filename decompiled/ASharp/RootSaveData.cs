using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;
using I2.Loc;
using UnityEngine;

[Serializable]
public sealed class RootSaveData : ISerializable
{
	public const int RootSaveVersion = 1;

	public const string DefaultSaveFileExtension = ".dat";

	public const string AutotestSaveFileExtension = ".testdat";

	public const string BaseLevelSaveFileExtension = ".lvldat";

	public const string CustomRootFolder = "CustomScenarioSaves";

	public const string CustomScenariosFolder = "CustomScenarios";

	public const string LevelEditorStatesRootFolder = "LevelEditor";

	public const string LevelEditorCustomLVLDATFolder = "LevelEditorScenarios";

	public const string LevelEditorCustomYMLFolder = "YMLScenarios";

	public const string LevelEditorRulebaseYMLFolder = "SingleScenarioYML";

	public const string AutotestFolderName = "Autotests";

	public const string CampaignSavesFolder = "Campaign";

	public const string ScenarioSavesFolder = "SingleScenarios";

	public const string CheckpointSavesFolder = "Checkpoints";

	public const string CorruptedSavesFolder = "CorruptedSaves";

	public const string InvalidatedSavesFolder = "InvalidatedSaves";

	public const string BackupSavesFolder = "BackupSaves";

	public const string RootSaveFileName = "RootSave.dat";

	public const string GlobalDataSaveFileName = "GlobalData.dat";

	public const string AutoLogSaveFileName = "KronosAutoLog.dat";

	public const string ModsFolderName = "SteamMods";

	public const string ModdedRulesetsFolderName = "ModdedRulesets";

	public const string ModdingExportDir = "ModdingExport";

	public const string ModdingValidation = "ModValidation";

	public const string MultiplayerLocalization = "MultiplayerLocalization";

	public const string EditorPropertiesSaveFileName = "EditorProperties.json";

	public int Version;

	public string CurrentLanguage;

	public static Task<string> SaveRootFolder => PlatformLayer.Instance.NewPlatformLayer.GetSaveDataPathAsync();

	private static string _dataPath
	{
		get
		{
			if (!(SceneController.Instance == null))
			{
				return SceneController.Instance.ApplicationDataPath;
			}
			return Application.dataPath;
		}
	}

	public static Task<string> PlayerLogPath => FHTaskExtensions.CombineAsyncPaths(SaveData.Instance.PersistentDataPath, "Player.log");

	public static Task<string> TempPlayerLogPath => FHTaskExtensions.CombineAsyncPaths(SaveData.Instance.PersistentDataPath, "TempPlayer.log");

	public static Task<string> PlayerLogPathFromHost => FHTaskExtensions.CombineAsyncPaths(SaveData.Instance.PersistentDataPath, "HostPlayer.log");

	public static Task<string> ScreenCaptureImagePath => FHTaskExtensions.CombineAsyncPaths(SaveData.Instance.PersistentDataPath, "ErrorSceenCap.png");

	public static Task<string> RootSaveFile => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "RootSave.dat");

	public static string EditorPropertiesSaveFile => Path.GetFullPath(Path.Combine(_dataPath, "..", "UserSettings", "EditorProperties.json"));

	public static Task<string> LevelEditorCustomLevels => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "LevelEditor");

	public static Task<string> LevelEditorCustomLVLDAT => FHTaskExtensions.CombineAsyncPaths(LevelEditorCustomLevels, "LevelEditorScenarios");

	public static Task<string> LevelEditorCustomYML => FHTaskExtensions.CombineAsyncPaths(LevelEditorCustomLevels, "YMLScenarios");

	public static string RulebaseCustomLevelEditorScenarios => Path.Combine(YMLLoading.GetRulesetPath(), "Scenarios", "LevelEditorScenarios");

	public static string RulebaseCustomYMLScenarios => Path.Combine(YMLLoading.GetRulesetPath(), "Scenarios", "SingleScenarioYML");

	public static Task<string> MultiplayerLocalizationFolder => FHTaskExtensions.CombineAsyncPaths(SaveData.Instance.PersistentDataPath, "MultiplayerLocalization");

	public static Task<string> AutotestRuntimePath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "Autotests");

	public static string AutotestEditorPath
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(_dataPath, "..", "Autotests"));
			}
			return Path.GetFullPath(Path.Combine(_dataPath, "..", "..", "Autotests"));
		}
	}

	public Task<string> GlobalSaveFile => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "GlobalData.dat");

	public Task<string> PreviousGlobalSaveFile => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "Core.dat");

	public Task<string> CustomRootPath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "CustomScenarioSaves");

	public Task<string> JSONEventLogPath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "EventLog.JSON");

	public Task<string> CorruptedSavePath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "CorruptedSaves");

	public Task<string> InvalidatedSavePath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "InvalidatedSaves");

	public Task<string> PreDLCBackupSavePath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "BackupSaves");

	public Task<string> CampaignSavePath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "Campaign");

	public Task<string> SingleScenarioSavePath => FHTaskExtensions.CombineAsyncPaths(SaveRootFolder, "SingleScenarios");

	public Task<string> CurrentlyUsedSavePath => SaveData.Instance.Global.GameMode switch
	{
		EGameMode.Campaign => SaveData.Instance.Global.CurrentAdventureData.PartyMainSaveFile, 
		EGameMode.SingleScenario => SaveData.Instance.Global.CurrentAdventureData.PartyMainSaveFile, 
		_ => null, 
	};

	public static async Task<string> GetAutoTestPath()
	{
		if (Application.isEditor)
		{
			return AutotestEditorPath;
		}
		string text = await AutotestRuntimePath;
		if (!Directory.Exists(text))
		{
			Log.Info("Creating AutotestRuntimePath directory: " + text);
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public static Task<string> PlayerLogFromClientPath(string clientName)
	{
		return FHTaskExtensions.CombineAsyncPaths(SaveData.Instance.PersistentDataPath, "ClientPlayer-" + clientName + ".log");
	}

	public static async Task<IEnumerable<string>> PlayerLogFromMultipleClientPath(IEnumerable<string> clientNames)
	{
		string persistentDataPath = await SaveData.Instance.PersistentDataPath;
		return clientNames.Select((string clientName) => Path.Combine(persistentDataPath, "ClientPlayer-" + clientName + ".log"));
	}

	public static async void CleanClientPlayerLogs()
	{
		try
		{
			string[] files = Directory.GetFiles(await SaveData.Instance.PersistentDataPath, "ClientPlayer-*.log", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception trying to delete client player logs.");
		}
	}

	public static async Task<string[]> GetAllClientPlayerLogs()
	{
		List<string> clientLogs = new List<string>();
		string[] files = Directory.GetFiles(await SaveData.Instance.PersistentDataPath, "ClientPlayer-*.log", SearchOption.TopDirectoryOnly);
		foreach (string item in files)
		{
			clientLogs.Add(item);
		}
		return clientLogs.ToArray();
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Version", Version);
		info.AddValue("CurrentLanguage", CurrentLanguage);
	}

	public RootSaveData(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "Version"))
				{
					if (name == "CurrentLanguage")
					{
						CurrentLanguage = info.GetString("CurrentLanguage");
					}
				}
				else
				{
					Version = info.GetInt32("Version");
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize RootSaveData entry " + item.Name);
				throw ex;
			}
		}
		if (Version != 1)
		{
			Log.Info("Root Save data version has been bumped.  Current root data will be updated.");
			Version = 1;
			CurrentLanguage = LocalizationManager.CurrentLanguage;
		}
	}

	public Task<string> GetFilePathForParty(EGameMode gameMode, string partyName, string hostAccountID)
	{
		return gameMode switch
		{
			EGameMode.Campaign => FHTaskExtensions.CombineAsyncPaths(CampaignSavePath, partyName.Replace(" ", "_"), "HostSave_" + hostAccountID + ".dat"), 
			EGameMode.SingleScenario => FHTaskExtensions.CombineAsyncPaths(SingleScenarioSavePath, partyName.Replace(" ", "_"), "HostSave_" + hostAccountID + ".dat"), 
			_ => null, 
		};
	}

	public Task<string> GetDefaultFilePathForCustomLevel(string levelName, string workshopID = "")
	{
		if (workshopID.IsNullOrEmpty())
		{
			return SaveData.Instance.LevelEditorDataManager.LevelDataSaveFilePath(levelName);
		}
		return SaveData.Instance.LevelEditorDataManager.WorkshopLevelSaveFilePath(levelName, workshopID);
	}

	public RootSaveData()
	{
	}

	public static RootSaveData CreateRootSaveData()
	{
		Log.Info("Creating RootSaveData");
		return new RootSaveData
		{
			Version = 1,
			CurrentLanguage = LocalizationManager.CurrentLanguage
		};
	}
}
