using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Gloomhaven;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using SteamDataAttribution.Internal;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GlobalData : ISerializable
{
	public delegate void SpeedChangedEventHandler();

	public static readonly char[] UnsupportedCharacters = new char[15]
	{
		'_', '/', '*', '.', '"', '\\', '[', ']', ':', ';',
		'|', ',', '<', '>', '?'
	};

	public int partnerID;

	public int MasterVolume;

	public int MusicVolume;

	public int AmbientVolume;

	public int SFXVolume;

	public int StoryVolume = 80;

	public bool MuteStoryVoiceOver;

	public int UIVolume = 80;

	public bool MuteAudioInBackground = true;

	public bool DisabledCombatLog;

	public List<CombatLogFilter> DisabledCombatLogFilters = new List<CombatLogFilter>();

	public string QualityLevel;

	public GraphicProfile CustomQualityProfile;

	public int TargetFrameRate = 60;

	public bool EnableSecondClickHexToConfirm;

	public bool DisableHoverOutlines;

	public bool DisableAbilityFocusOutlines;

	public bool EpicLogin;

	public bool AllowAutomaticTacticalCameraRotation;

	public bool AllowAutomaticTacticalCameraMovement = true;

	private bool m_ChooseAdvantageModifiers;

	private bool m_ConfirmAttackAbilityBeforeFinishing;

	public bool IntroCinematicSeen;

	public int LastSeenPatchNotesVersion;

	public string LastStartVerion = "";

	private float m_RegularSpeedMultiplier = 1f;

	private float m_MultiplayerRegularSpeedMultiplier = 1f;

	private bool m_SpeedUpHoldRequest;

	private bool m_SpeedUpAllowed;

	public bool ShowSpeedUpButton = true;

	public bool EnableFastUndo = true;

	public const string DefaultNetworkRegion = "eu";

	public string NetworkRegion = "eu";

	public bool MPEndOfTurnCompare;

	public StatsDataStorage m_StatsDataStorage = new StatsDataStorage();

	public string CurrentHostAccountID;

	private string _resumeCampaignName;

	[FormerlySerializedAs("ResumeSingleScenarioName")]
	private string _resumeSingleScenarioName;

	public List<PartyAdventureData> AllSingleScenarios;

	public bool SteamDataSuiteAttributionDone;

	[NonSerialized]
	public CCustomLevelData CurrentCustomLevelData;

	public bool CurrentlyPlayingCustomLevel;

	[NonSerialized]
	public string CurrentFrontEndTutorialID;

	[NonSerialized]
	public string CurrentFrontEndTutorialFilename;

	[NonSerialized]
	public CCustomLevelData CurrentEditorLevelData;

	[NonSerialized]
	public AutoTestData CurrentEditorAutoTestData;

	[NonSerialized]
	public AutoTestData CurrentAutoTestDataCopy;

	[NonSerialized]
	public AutoTestData LastLoadedAutoTestData;

	public const int ResetDataVersion = 4;

	private int m_ResetDataVersion;

	private List<PartyAdventureData> _campaignCache = new List<PartyAdventureData>();

	internal volatile bool InvalidateCache = true;

	private SemaphoreSlim _loadSavesLock = new SemaphoreSlim(1, 1);

	public List<string> CompletedTutorialIDs { get; set; }

	public bool DebugSpeedMode { get; set; }

	public bool ChooseAdvantageModifiers
	{
		get
		{
			return m_ChooseAdvantageModifiers;
		}
		set
		{
			if (Application.isPlaying && m_ChooseAdvantageModifiers != value)
			{
				m_ChooseAdvantageModifiers = value;
				SaveData.Instance?.SaveGlobalData();
			}
		}
	}

	public bool ConfirmAttackAbilityBeforeFinishing
	{
		get
		{
			return m_ConfirmAttackAbilityBeforeFinishing;
		}
		set
		{
			if (Application.isPlaying && m_ConfirmAttackAbilityBeforeFinishing != value)
			{
				m_ConfirmAttackAbilityBeforeFinishing = value;
				SaveData.Instance?.SaveGlobalData();
			}
		}
	}

	public float RegularSpeedMultiplier
	{
		get
		{
			return m_RegularSpeedMultiplier;
		}
		set
		{
			if (m_RegularSpeedMultiplier != value)
			{
				m_RegularSpeedMultiplier = value;
				ReapplySpeedUp();
				SaveData.Instance.SaveGlobalData();
			}
		}
	}

	public float MultiplayerRegularSpeedMultiplier
	{
		get
		{
			return m_MultiplayerRegularSpeedMultiplier;
		}
		set
		{
			m_MultiplayerRegularSpeedMultiplier = value;
			ReapplySpeedUp();
		}
	}

	public string LastAutotestFolder { get; set; }

	public bool UseCustomLevelDataFolder { get; set; }

	public string CustomLevelDataFolder { get; set; }

	public EGameMode GameMode { get; set; }

	public PartyAdventureData CurrentAdventureData { get; set; }

	public DLCRegistry.EDLCKey DLCInstalledRecord { get; private set; }

	public static List<string> CompletedCharacters => new List<string>
	{
		"BruteID", "CragheartID", "ScoundrelID", "SpellweaverID", "TinkererID", "MindthiefID", "ElementalistID", "SoothsingerID", "SunkeeperID", "BerserkerID",
		"NightshroudID", "BeastTyrantID", "SummonerID", "QuartermasterID"
	};

	public EGameState CurrentGameState
	{
		get
		{
			if (GameMode == EGameMode.Campaign)
			{
				if (AdventureState.MapState == null)
				{
					return EGameState.None;
				}
				if (AdventureState.MapState.IsInScenarioPhase)
				{
					return EGameState.Scenario;
				}
				return EGameState.Map;
			}
			if (GameMode == EGameMode.LevelEditor && CurrentEditorLevelData != null)
			{
				return EGameState.Scenario;
			}
			if (GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.Autotest || GameMode == EGameMode.FrontEndTutorial)
			{
				return EGameState.Scenario;
			}
			return EGameState.None;
		}
	}

	public event SpeedChangedEventHandler GameSpeedChanged;

	public void InvokeGameSpeedChanged()
	{
		GameSpeedChanged?.Invoke();
	}

	public void SetSpeedUpHoldRequest(bool active)
	{
		if (!DebugSpeedMode)
		{
			m_SpeedUpHoldRequest = active;
			ReapplySpeedUp();
		}
	}

	public void AllowSpeedUp()
	{
		if (!DebugSpeedMode)
		{
			m_SpeedUpAllowed = true;
			ReapplySpeedUp();
		}
	}

	public void DisallowSpeedUp()
	{
		if (!DebugSpeedMode)
		{
			m_SpeedUpAllowed = false;
			ReapplySpeedUp();
		}
	}

	private void ReapplySpeedUp()
	{
		if (m_SpeedUpAllowed)
		{
			float num = (m_SpeedUpHoldRequest ? Math.Min(m_RegularSpeedMultiplier * 3f, 10f) : m_RegularSpeedMultiplier);
			if (FFSNetwork.IsOnline && !FFSNetwork.IsHost)
			{
				num = m_MultiplayerRegularSpeedMultiplier;
			}
			if (TimeManager.DefaultTimeScale != num)
			{
				TimeManager.DefaultTimeScale = num;
				GameSpeedChanged?.Invoke();
			}
		}
		else if (TimeManager.DefaultTimeScale != 1f)
		{
			TimeManager.DefaultTimeScale = 1f;
			GameSpeedChanged?.Invoke();
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PartnerID", partnerID);
		info.AddValue("MasterVolume", MasterVolume);
		info.AddValue("MusicVolume", MusicVolume);
		info.AddValue("AmbientVolume", AmbientVolume);
		info.AddValue("SFXVolume", SFXVolume);
		info.AddValue("StoryVolume", StoryVolume);
		info.AddValue("MuteStoryVoiceOver", MuteStoryVoiceOver);
		info.AddValue("MuteAudioInBackground", MuteAudioInBackground);
		info.AddValue("MultiplayerPingVolume", UIVolume);
		info.AddValue("DisabledCombatLogFilters", DisabledCombatLogFilters);
		info.AddValue("DisabledCombatLog", DisabledCombatLog);
		info.AddValue("CustomQualityProfile", CustomQualityProfile);
		info.AddValue("QualityLevel", QualityLevel);
		info.AddValue("TargetFrameRate", TargetFrameRate);
		info.AddValue("EnableSecondClickHexToConfirm", EnableSecondClickHexToConfirm);
		info.AddValue("DisableAbilityFocusOutlines", DisableAbilityFocusOutlines);
		info.AddValue("EpicLogin", EpicLogin);
		info.AddValue("DisableHoverOutlines", DisableHoverOutlines);
		info.AddValue("AllowAutomaticTacticalCameraRotation", AllowAutomaticTacticalCameraRotation);
		info.AddValue("AllowAutomaticTacticalCameraMovement", AllowAutomaticTacticalCameraMovement);
		info.AddValue("StatsDataStorage", m_StatsDataStorage);
		info.AddValue("ResetDataVersion", m_ResetDataVersion);
		info.AddValue("SteamDataSuiteAttributionDone", SteamDataSuiteAttributionDone);
		info.AddValue("ResumeCampaignName", _resumeCampaignName);
		info.AddValue("ResumeSingleScenarioName", _resumeSingleScenarioName);
		info.AddValue("UseCustomLevelDataFolder", UseCustomLevelDataFolder);
		info.AddValue("CustomLevelDataFolder", CustomLevelDataFolder);
		info.AddValue("MPEndOfTurnCompare", MPEndOfTurnCompare);
		info.AddValue("m_RegularSpeedMultiplier", m_RegularSpeedMultiplier);
		info.AddValue("ShowSpeedUpButton", ShowSpeedUpButton);
		info.AddValue("EnableFastUndo", EnableFastUndo);
		info.AddValue("NetworkRegion", NetworkRegion);
		info.AddValue("CompletedTutorialIDs", CompletedTutorialIDs);
		info.AddValue("DLCInstalledRecord", DLCInstalledRecord);
		info.AddValue("LastStartVerion", LastStartVerion);
		info.AddValue("ChooseAdvantageModifiers", ChooseAdvantageModifiers);
		info.AddValue("ConfirmAttackAbilityBeforeFinishing", ConfirmAttackAbilityBeforeFinishing);
		info.AddValue("IntroCinematicSeen", IntroCinematicSeen);
	}

	private GlobalData(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PartnerID":
					partnerID = info.GetInt32("PartnerID");
					break;
				case "MasterVolume":
					MasterVolume = info.GetInt32("MasterVolume");
					break;
				case "MusicVolume":
					MusicVolume = info.GetInt32("MusicVolume");
					break;
				case "AmbientVolume":
					AmbientVolume = info.GetInt32("AmbientVolume");
					break;
				case "SFXVolume":
					SFXVolume = info.GetInt32("SFXVolume");
					break;
				case "StoryVolume":
					StoryVolume = info.GetInt32("StoryVolume");
					break;
				case "MuteStoryVoiceOver":
					MuteStoryVoiceOver = info.GetBoolean("MuteStoryVoiceOver");
					break;
				case "MultiplayerPingVolume":
					UIVolume = info.GetInt32("MultiplayerPingVolume");
					break;
				case "MuteAudioInBackground":
					MuteAudioInBackground = info.GetBoolean("MuteAudioInBackground");
					break;
				case "DisabledCombatLogFilters":
					DisabledCombatLogFilters = ((List<CombatLogFilter>)info.GetValue("DisabledCombatLogFilters", typeof(List<CombatLogFilter>))) ?? new List<CombatLogFilter>();
					break;
				case "DisabledCombatLog":
					DisabledCombatLog = info.GetBoolean("DisabledCombatLog");
					break;
				case "CustomQualityProfile":
					CustomQualityProfile = (GraphicProfile)info.GetValue("CustomQualityProfile", typeof(GraphicProfile));
					break;
				case "QualityLevel":
					QualityLevel = info.GetString("QualityLevel");
					break;
				case "TargetFrameRate":
					TargetFrameRate = info.GetInt32("TargetFrameRate");
					break;
				case "EnableSecondClickHexToConfirm":
					EnableSecondClickHexToConfirm = info.GetBoolean("EnableSecondClickHexToConfirm");
					break;
				case "DisableAbilityFocusOutlines":
					DisableAbilityFocusOutlines = info.GetBoolean("DisableAbilityFocusOutlines");
					break;
				case "EpicLogin":
					EpicLogin = info.GetBoolean("EpicLogin");
					break;
				case "DisableHoverOutlines":
					DisableHoverOutlines = info.GetBoolean("DisableHoverOutlines");
					break;
				case "AllowAutomaticTacticalCameraRotation":
					AllowAutomaticTacticalCameraRotation = info.GetBoolean("AllowAutomaticTacticalCameraRotation");
					break;
				case "AllowAutomaticTacticalCameraMovement":
					AllowAutomaticTacticalCameraMovement = info.GetBoolean("AllowAutomaticTacticalCameraMovement");
					break;
				case "StatsDataStorage":
					m_StatsDataStorage = (StatsDataStorage)info.GetValue("StatsDataStorage", typeof(StatsDataStorage));
					break;
				case "ResetDataVersion":
					m_ResetDataVersion = info.GetInt32("ResetDataVersion");
					break;
				case "SteamDataSuiteAttributionDone":
					SteamDataSuiteAttributionDone = info.GetBoolean("SteamDataSuiteAttributionDone");
					break;
				case "ResumeCampaignName":
					_resumeCampaignName = info.GetString("ResumeCampaignName");
					break;
				case "ResumeSingleScenarioName":
					_resumeSingleScenarioName = info.GetString("ResumeSingleScenarioName");
					break;
				case "AllSingleScenarios":
					AllSingleScenarios = (List<PartyAdventureData>)info.GetValue("AllSingleScenarios", typeof(List<PartyAdventureData>));
					break;
				case "LastAutotestFolder":
					LastAutotestFolder = info.GetString("LastAutotestFolder");
					break;
				case "UseCustomLevelDataFolder":
					UseCustomLevelDataFolder = info.GetBoolean("UseCustomLevelDataFolder");
					break;
				case "CustomLevelDataFolder":
					CustomLevelDataFolder = info.GetString("CustomLevelDataFolder");
					break;
				case "MPEndOfTurnCompare":
					MPEndOfTurnCompare = info.GetBoolean("MPEndOfTurnCompare");
					break;
				case "m_RegularSpeedMultiplier":
					m_RegularSpeedMultiplier = Math.Clamp((float)info.GetDouble("m_RegularSpeedMultiplier"), 1f, 10f);
					break;
				case "ChooseAdvantageModifiers":
					ChooseAdvantageModifiers = info.GetBoolean("ChooseAdvantageModifiers");
					break;
				case "ConfirmAttackAbilityBeforeFinishing":
					ConfirmAttackAbilityBeforeFinishing = info.GetBoolean("ConfirmAttackAbilityBeforeFinishing");
					break;
				case "ShowSpeedUpButton":
					ShowSpeedUpButton = info.GetBoolean("ShowSpeedUpButton");
					break;
				case "EnableFastUndo":
					EnableFastUndo = info.GetBoolean("EnableFastUndo");
					FastReload.FastUndoEnabled = EnableFastUndo;
					break;
				case "NetworkRegion":
					NetworkRegion = info.GetString("NetworkRegion");
					break;
				case "CompletedTutorialIDs":
					CompletedTutorialIDs = (List<string>)info.GetValue("CompletedTutorialIDs", typeof(List<string>));
					break;
				case "DLCInstalledRecord":
					DLCInstalledRecord = (DLCRegistry.EDLCKey)info.GetValue("DLCInstalledRecord", typeof(DLCRegistry.EDLCKey));
					break;
				case "LastSeenPatchNotesVersion":
					LastSeenPatchNotesVersion = info.GetInt32("LastSeenPatchNotesVersion");
					break;
				case "LastStartVerion":
					LastStartVerion = info.GetString("LastStartVerion");
					break;
				case "IntroCinematicSeen":
					IntroCinematicSeen = info.GetBoolean("IntroCinematicSeen");
					break;
				case "AllCampaigns":
					break;
				case "ResumeAdventureName":
					break;
				case "ResumeModdedAdventureName":
					break;
				case "AllAdventures":
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize GlobalData entry " + item.Name);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal async void OnDeserialized(StreamingContext context)
	{
		if (!SteamDataSuiteAttributionDone)
		{
			SteamDataSuiteAttributionDone = true;
		}
		if (AllSingleScenarios == null)
		{
			AllSingleScenarios = new List<PartyAdventureData>();
		}
		if (CompletedTutorialIDs == null)
		{
			CompletedTutorialIDs = new List<string>();
		}
	}

	public async Task<PartyAdventureData> GetResumeCampaign()
	{
		if (string.IsNullOrEmpty(_resumeCampaignName))
		{
			return null;
		}
		return (from t in await LoadSavesFromDisk()
			where t.GameMode == EGameMode.Campaign && t.PartyName == _resumeCampaignName
			orderby t.LastSavedTimeStamp descending
			select t).FirstOrDefault();
	}

	public void SetResumeCampaign(PartyAdventureData campaign)
	{
		_resumeCampaignName = campaign?.PartyName ?? string.Empty;
	}

	public void SetResumeSingleScenario(PartyAdventureData campaign)
	{
		_resumeSingleScenarioName = campaign?.PartyName ?? string.Empty;
	}

	public async Task<PartyAdventureData> GetResumeSingleScenario()
	{
		if (string.IsNullOrEmpty(_resumeSingleScenarioName))
		{
			return null;
		}
		await LoadSavesFromDisk();
		return AllSingleScenarios.OrderByDescending((PartyAdventureData t) => t.LastSavedTimeStamp).FirstOrDefault((PartyAdventureData t) => t.PartyName == _resumeSingleScenarioName);
	}

	public async Task<List<PartyAdventureData>> LoadSavesFromDisk()
	{
		await _loadSavesLock.WaitAsync();
		try
		{
			if (InvalidateCache)
			{
				_campaignCache.Clear();
				AllSingleScenarios.Clear();
				Stopwatch sw = new Stopwatch();
				sw.Start();
				List<string> savePaths = new List<string>();
				List<string> list = savePaths;
				list.Add(await SaveData.Instance.RootData.CampaignSavePath);
				list = savePaths;
				list.Add(await SaveData.Instance.RootData.SingleScenarioSavePath);
				List<Task<PartyAdventureData>> allTasks = new List<Task<PartyAdventureData>>();
				foreach (string item in savePaths)
				{
					if (Directory.Exists(item))
					{
						Task<PartyAdventureData>[] collection = (from file in Directory.EnumerateFiles(item, "*.dat", SearchOption.AllDirectories).ToList()
							select LoadPartyAdventureData(file)).ToArray();
						allTasks.AddRange(collection);
					}
				}
				await Task.Run(() =>
				{
					Task[] tasks = allTasks.ToArray();
					Task.WaitAll(tasks);
				});
				List<PartyAdventureData> list2 = (from t in allTasks
					where t.Result != null
					select t.Result).ToList();
				_campaignCache.AddRange(list2.Where((PartyAdventureData r) => r.GameMode == EGameMode.Campaign));
				AllSingleScenarios.AddRange(list2.Where((PartyAdventureData r) => r.GameMode == EGameMode.SingleScenario));
				sw.Stop();
				Log.Info($"[SAVE] Loading {list2.Count} saves from disk took {sw.Elapsed.TotalSeconds} seconds");
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception while loading saves from disk");
		}
		finally
		{
			_loadSavesLock.Release();
		}
		AllSingleScenarios = AllSingleScenarios.OrderByDescending((PartyAdventureData p) => p.LastSavedTimeStamp).ToList();
		InvalidateCache = false;
		return _campaignCache;
	}

	private async Task<PartyAdventureData> LoadPartyAdventureData(string file)
	{
		PartyAdventureData data = null;
		try
		{
			data = await PartyAdventureData.LoadSaveInfoFromFile(file);
			if (data != null && data.FromGameRevision < PartyAdventureData.InvalidateSavesBeforeRevision)
			{
				await PartyAdventureData.MoveGameStateToInvalidatedSaveFolderInternal(data, file);
				return null;
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred while attempting to load the save data");
		}
		if (data == null)
		{
			await PartyAdventureData.MoveGameStateToCorruptedSaveFolderInternal(Path.GetFileNameWithoutExtension(file), file);
			return null;
		}
		return data;
	}

	public async Task DeserializeGameStateAndValidateOnLoad(EGameMode gameMode, PartyAdventureData partyData)
	{
		try
		{
			if (partyData.FrosthavenSaveData == null)
			{
				await partyData.Load(gameMode, isJoiningMPClient: false);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Unable to update MapStateFile");
		}
	}

	public GlobalData()
	{
		MasterVolume = 80;
		MusicVolume = 80;
		AmbientVolume = 80;
		SFXVolume = 80;
		StoryVolume = 80;
		MuteStoryVoiceOver = false;
		UIVolume = 80;
		MuteAudioInBackground = true;
		DisabledCombatLogFilters = new List<CombatLogFilter>();
		DisabledCombatLog = false;
		AllowAutomaticTacticalCameraRotation = false;
		AllowAutomaticTacticalCameraMovement = true;
		CurrentCustomLevelData = null;
		CurrentAutoTestDataCopy = null;
		GameMode = EGameMode.MainMenu;
		SteamDataSuiteAttributionDone = false;
		AllSingleScenarios = new List<PartyAdventureData>();
		m_ResetDataVersion = 4;
		QualityLevel = string.Empty;
		TargetFrameRate = 60;
		CompletedTutorialIDs = new List<string>();
	}

	public void Save()
	{
	}

	public void Load()
	{
		if (!SteamDataSuiteAttributionDone)
		{
			SteamDataAttribution.Internal.SteamDataAttribution.OnRuntimeMethodLoad();
		}
	}

	public void CloseCustomLevel()
	{
		CurrentlyPlayingCustomLevel = false;
		CurrentCustomLevelData = null;
	}

	public bool IsCorrectVersion()
	{
		if (m_ResetDataVersion < 4)
		{
			return false;
		}
		return true;
	}

	public async Task<PartyAdventureData> FindMostRecentSave()
	{
		_ = 1;
		try
		{
			List<PartyAdventureData> saves = new List<PartyAdventureData>();
			PartyAdventureData partyAdventureData = await GetResumeCampaign();
			if (partyAdventureData != null)
			{
				saves.Add(partyAdventureData);
			}
			PartyAdventureData partyAdventureData2 = await GetResumeSingleScenario();
			if (partyAdventureData2 != null)
			{
				saves.Add(partyAdventureData2);
			}
			return saves.OrderByDescending((PartyAdventureData o) => o.LastSavedTimeStamp).FirstOrDefault();
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during FindMostRecentSave.");
			return null;
		}
	}

	public void ChangeDLCInstalledState(DLCRegistry.EDLCKey dlcToChange, bool stateToSet)
	{
		if (stateToSet)
		{
			DLCInstalledRecord |= dlcToChange;
		}
		else
		{
			DLCInstalledRecord &= ~dlcToChange;
		}
	}
}
