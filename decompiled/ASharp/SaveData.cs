using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using FH.YML;
using I2.Loc;
using Libs.GameConsole;
using Libs.Threading;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Client;
using UnityEngine;

public class SaveData : MonoBehaviour
{
	private enum TakingScreenshotState
	{
		None,
		InProgress,
		Completed
	}

	private const string c_CLA_AutoTest = "-autotest";

	private const string c_CLA_LoadMostRecentSave = "-loadmostrecentsave";

	private const string c_CLA_Debug = "-debug";

	private const string c_CLA_AutoupdateAutotests = "-autoupdate";

	[NonSerialized]
	public GlobalData Global;

	private SemaphoreSlim _savingLock = new SemaphoreSlim(1);

	[NonSerialized]
	public volatile bool IsSavingData;

	[NonSerialized]
	public volatile bool IsSavingThreadActive;

	private string _autoTestResultsLog;

	[NonSerialized]
	private PartyAdventureData m_TempPartyData;

	private bool m_TempIsMPClient;

	private RenderTexture _rtSmall;

	private Texture2D _screenshotTexture;

	private Texture2D _smallTexture;

	private byte[] _smallTextureEncoded;

	private bool _takeScreenshotSuccess;

	private volatile bool _takeScreenshotDone = true;

	private volatile TakingScreenshotState _takeScreenshotState;

	private volatile bool _takeScreenshotRequested;

	public static SaveData Instance { get; private set; }

	public RootSaveData RootData { get; private set; }

	public Task<string> PersistentDataPath => PlatformLayer.Instance.NewPlatformLayer.GetPersistentDataPathAsync();

	public bool LoadMostRecentSave { get; set; }

	public string AutoTestResultsLog
	{
		get
		{
			return _autoTestResultsLog ?? Path.Combine(Application.dataPath, "autotest.log");
		}
		private set
		{
			_autoTestResultsLog = value;
		}
	}

	public string AutoTestFolder { get; private set; }

	public bool IsModdingEnabled { get; private set; }

	public string CurrentFileBeingDeserialized { get; set; }

	public Task<string> AssemblyInfoSaveDir => FHTaskExtensions.CombineAsyncPaths(RootSaveData.SaveRootFolder, "SaveFileAssemblyInfo");

	public bool AutoupdateAutotests { get; private set; }

	public AutoTestDataManager AutoTestDataManager { get; private set; }

	public LevelEditorDataManager LevelEditorDataManager { get; private set; }

	public event Action GlobalDataChanged;

	public IEnumerator LoadRulebase()
	{
		Log.Info("Loading Rulebase");
		Task<string> persistentDataPathTask = Instance.PersistentDataPath;
		yield return new WaitForTask(persistentDataPathTask);
		string result = persistentDataPathTask.Result;
		yield return SceneController.Instance.InitialiseGloomhavenCoroutine(result, YMLLoading.RulebaseFolder);
		yield return null;
		Task loadTask = LoadGlobalData();
		yield return new WaitForTask(loadTask);
		if (loadTask.HasFailed(out var exception))
		{
			HandleException(exception);
			yield break;
		}
		try
		{
			Instance.InitialiseDataManagers();
		}
		catch (Exception exception2)
		{
			HandleException(exception2);
			yield break;
		}
		yield return null;
		static void HandleException(Exception ex)
		{
			Log.Exception(ex, "An exception occurred while loading rulebase at startup");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public void InitOnAwake()
	{
		Instance = this;
		ParseCommandLineArgs();
		CreateAssemblyInfoSaveDir();
		try
		{
			PlatformLayer.Instance.NewPlatformLayer.WhenPlatformAvailable(() =>
			{
				LoadRootData();
			});
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while loading rulebase at startup");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	private async Task CreateAssemblyInfoSaveDir()
	{
	}

	public IEnumerator StartInitCrt()
	{
		while (!PersistentData.s_Instance.IsDataLoaded || Global == null)
		{
			yield return null;
		}
		while (!AnalyticsWrapper.IsReady)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		AnalyticsWrapper.InitHeader();
		AnalyticsWrapper.LogAppBoot();
	}

	public void ApplicationQuitDeinit()
	{
		Log.Info("BRYNN: SaveData OnApplicationQuit Start" + DateTime.Now);
		GlobalData global = Global;
		if (global != null && global.CurrentGameState == EGameState.Scenario)
		{
			Instance.EndScenario(EResult.InProgressAppKill);
		}
		Log.Info("BRYNN: SaveData OnApplicationQuit Start" + DateTime.Now);
	}

	private void Awake()
	{
		InitOnAwake();
	}

	private IEnumerator Start()
	{
		yield return StartInitCrt();
	}

	private void OnApplicationQuit()
	{
		ApplicationQuitDeinit();
	}

	public async Task StartNewCampaignMode(string partyName, EAdventureDifficulty difficulty, ERulesFlag rules, string campaignID)
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), partyName, difficulty, new SaveOwner(), EGameMode.Campaign, rules, campaignID);
		Global.SetResumeCampaign(partyAdventureData);
		Global.CurrentAdventureData = partyAdventureData;
		Global.CurrentHostAccountID = partyAdventureData.Owner.PlatformAccountID;
		await ResumeNewCampaignMode(isJoiningMPClient: false);
	}

	public async Task ResumeNewCampaignMode(bool isJoiningMPClient)
	{
		if (Global.CurrentAdventureData != null)
		{
			if (Global.CurrentAdventureData.FrosthavenSaveData == null)
			{
				await Global.CurrentAdventureData.Load(EGameMode.Campaign, isJoiningMPClient);
			}
			Global.GameMode = EGameMode.Campaign;
			ScenarioManager.Mode = ScenarioManager.EDLLMode.Campaign;
			Global.CurrentAdventureData.ResolveFHGameState(YMLDefRepository.Repo, recreateMapParty: false);
			await SaveGlobalData();
			return;
		}
		throw new Exception("No Adventure data was found for Adventure party " + Global?.CurrentAdventureData?.PartyName);
	}

	public void LoadCampaignMode(PartyAdventureData partyData, bool isJoiningMPClient)
	{
		if (!FFSNetwork.IsOnline && PlatformLayer.UserData.PlatformAccountID != partyData.Owner.PlatformAccountID)
		{
			m_TempPartyData = partyData;
			m_TempIsMPClient = isJoiningMPClient;
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("CREATE_NEW_LOCAL_SAVE_BUTTON", CreateNewSaveFromMPHostSaveCampaign, InputFHManager.Controls.Generic.AlternativeChoice),
				new ErrorMessage.LabelAction("CANCEL_BUTTON", SceneController.Instance.LoadMainMenu, InputFHManager.Controls.Generic.Back)
			};
			SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("CREATE_NEW_SAVE_MESSAGE_TITLE", "CREATE_NEW_LOCAL_SAVE", Environment.StackTrace, buttons);
		}
		else
		{
			if (!isJoiningMPClient)
			{
				Global.SetResumeCampaign(partyData);
				partyData.FrosthavenSaveData = null;
			}
			Global.CurrentAdventureData = partyData;
			SceneController.Instance.CampaignResume(partyData, isJoiningMPClient);
		}
	}

	public void CreateNewSaveFromMPHostSaveCampaign()
	{
		CreateNewSaveFromMPHostSave(EGameMode.Campaign);
	}

	private async Task CreateNewSaveFromMPHostSave(EGameMode mode)
	{
		Global.CurrentHostAccountID = (m_TempIsMPClient ? m_TempPartyData.Owner.PlatformAccountID : PlatformLayer.UserData.PlatformAccountID);
		PartyAdventureData newData = await m_TempPartyData.DeepCopySerializableObject<PartyAdventureData>();
		if (mode == EGameMode.Campaign)
		{
			Global.SetResumeCampaign(newData);
		}
		Global.CurrentAdventureData = newData;
		await newData.Load(mode, isJoiningMPClient: false);
		PartyAdventureData partyAdventureData = newData;
		partyAdventureData.OverwriteAdventureMapStateFilePath(await newData.PartyMainSaveFile);
		newData.Owner = new SaveOwner();
		newData.AdventureMapState.SetSaveOwner(newData.Owner.ConvertToMapStateSaveOwner());
		await newData.UpdateSavePaths();
		await SaveGlobalData();
		if (mode == EGameMode.Campaign)
		{
			await ResumeNewCampaignMode(isJoiningMPClient: false);
		}
		m_TempPartyData = null;
		m_TempIsMPClient = false;
		if (mode == EGameMode.Campaign)
		{
			SceneController.Instance.CampaignResume(newData, isJoiningMPClient: false);
		}
	}

	public async Task CopySaveToBackupBeforeEnablingDLCOnIt(PartyAdventureData partyData, DLCRegistry.EDLCKey dlcToAdd)
	{
		await partyData.CopyGametateFileToBackupFolder(partyData.GameMode);
		partyData.AdventureMapState?.SetDLCEnabled(dlcToAdd);
		await partyData.ManualSave(partyData.SaveFileName);
	}

	public async Task StartLevelEditor()
	{
		if (Global.CurrentEditorLevelData == null)
		{
			Global.CurrentEditorLevelData = new CCustomLevelData(string.Empty, new ScenarioState());
		}
		Instance.Global.GameMode = EGameMode.LevelEditor;
		await SaveGlobalData();
	}

	public void StartCustomLevel()
	{
		ScenarioManager.Mode = ScenarioManager.EDLLMode.CustomScenario;
		Global.GameMode = EGameMode.SingleScenario;
		if (Global.CurrentAdventureData?.FrosthavenSaveData == null)
		{
			Global.CurrentAdventureData?.Load(EGameMode.SingleScenario, isJoiningMPClient: false);
		}
	}

	public void StartFrontEndTutorialLevel()
	{
		ScenarioManager.Mode = ScenarioManager.EDLLMode.CustomScenario;
		Global.GameMode = EGameMode.FrontEndTutorial;
		if (Global.CurrentAdventureData?.FrosthavenSaveData == null)
		{
			Global.CurrentAdventureData.Load(EGameMode.FrontEndTutorial, isJoiningMPClient: false);
		}
	}

	public void StartAutoTest(AutoTestData autoTest)
	{
		AutoTestController.s_Instance.AutotestStarted = true;
		Global.CurrentlyPlayingCustomLevel = false;
		Global.CurrentCustomLevelData = autoTest;
		Global.GameMode = EGameMode.Autotest;
		ScenarioManager.Mode = ScenarioManager.EDLLMode.CustomScenario;
	}

	public IEnumerator EndScenarioThreaded(EResult result)
	{
		IsSavingThreadActive = true;
		IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-SaveData => EndScenarioSaveThread()", () =>
		{
			EndScenarioSaveThread(result);
		});
		yield return threadHandle.WaitForCompletionCrt();
	}

	public async void EndScenarioSaveThread(EResult result)
	{
		if (AutoTestController.s_AutoTestCurrentlyLoaded || (Global.CurrentAdventureData == null && !Global.CurrentlyPlayingCustomLevel))
		{
			IsSavingThreadActive = false;
			return;
		}
		if (Global.CurrentAdventureData.PartyName != "Tutorial" && Global.GameMode == EGameMode.Campaign)
		{
			await AutoSaveCurrentAdventureData();
		}
		if (Global.CurrentAdventureData != null)
		{
			Global.CurrentAdventureData.FastReloadState = null;
		}
		IsSavingThreadActive = false;
	}

	public async Task EndScenario(EResult result)
	{
		if (AutoTestController.s_AutoTestCurrentlyLoaded || (Global.CurrentAdventureData == null && !Global.CurrentlyPlayingCustomLevel))
		{
			return;
		}
		_ = Global.GameMode;
		bool currentlyPlayingCustomLevel = Global.CurrentlyPlayingCustomLevel;
		switch (Global.GameMode)
		{
		case EGameMode.Campaign:
			if (currentlyPlayingCustomLevel)
			{
				Global.CloseCustomLevel();
			}
			if (result != EResult.InProgress && result != EResult.InProgressAppKill)
			{
				Global.CurrentAdventureData.EndCurrentScenario(result);
			}
			switch (result)
			{
			case EResult.Win:
				Global.CurrentAdventureData.WinScenario();
				break;
			case EResult.Lose:
			case EResult.Resign:
			case EResult.Restart:
				Global.CurrentAdventureData.LoseScenario();
				break;
			case EResult.InProgress:
			case EResult.InProgressAppKill:
				Global.GameMode = EGameMode.MainMenu;
				break;
			}
			break;
		case EGameMode.SingleScenario:
			Global.CloseCustomLevel();
			Global.GameMode = EGameMode.MainMenu;
			break;
		case EGameMode.FrontEndTutorial:
			Global.CloseCustomLevel();
			if (result == EResult.Win && !Global.CompletedTutorialIDs.Contains(Global.CurrentFrontEndTutorialID))
			{
				Global.CompletedTutorialIDs.Add(Global.CurrentFrontEndTutorialID);
			}
			if (result != EResult.Lose && result != EResult.Resign)
			{
				Global.CurrentFrontEndTutorialID = null;
				Global.CurrentFrontEndTutorialFilename = null;
			}
			break;
		case EGameMode.Autotest:
			Global.CloseCustomLevel();
			Global.GameMode = EGameMode.MainMenu;
			break;
		default:
			Log.Error("Invalid GameMode.  Unable to end run.");
			Global.GameMode = EGameMode.MainMenu;
			break;
		}
		SEventLog.ClearEventLog();
	}

	public async Task LoadRootData()
	{
		Log.Info("Loading RootSaveData");
		string text = await RootSaveData.RootSaveFile;
		if (File.Exists(text))
		{
			try
			{
				using FileStream fs = File.Open(text, FileMode.Open, FileAccess.Read);
				Instance.CurrentFileBeingDeserialized = text;
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				BinaryFormatter binaryFormatter2 = binaryFormatter;
				binaryFormatter2.Binder = new SerializationBinding(await AssemblyInfoSaveDir);
				RootData = binaryFormatter.Deserialize(fs) as RootSaveData;
			}
			catch (Exception exception)
			{
				Log.Exception(exception, "Unable to load root data.  It will be recreated.");
			}
		}
		if (RootData == null)
		{
			string text2 = await PlatformLayer.Instance.NewPlatformLayer.GetLocale();
			string supportedLanguage = LocalizationManager.GetSupportedLanguage(text2);
			string languageCode = LocalizationManager.GetLanguageCode(supportedLanguage);
			LocalizationManager.SetLanguageAndCode(supportedLanguage, languageCode);
			Debug.Log("Applying default locale: " + text2 + " | " + supportedLanguage + " | " + languageCode);
			RootData = RootSaveData.CreateRootSaveData();
		}
		await SaveRootData();
	}

	public async Task SaveRootData()
	{
		Log.Info("Saving RootSaveData");
		if (Singleton<AutoSaveProgress>.IsInitialized)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		string path = await RootSaveData.RootSaveFile;
		try
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using FileStream serializationStream = File.Open(path, FileMode.Create, FileAccess.Write);
			new BinaryFormatter().Serialize(serializationStream, RootData);
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred attempting to save.");
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.IsInitialized)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
	}

	public async Task<List<string>> FindPartyDataFileNames(EGameMode gameMode)
	{
		try
		{
			if (gameMode != EGameMode.Campaign)
			{
				Log.Error("Invalid Game mode sent to FindPartyDataFileNames");
				return new List<string>();
			}
			string path = await RootData.CampaignSavePath;
			if (Directory.Exists(path))
			{
				string[] directories = Directory.GetDirectories(path);
				if (directories.Length != 0)
				{
					List<string> list = new List<string>();
					string[] array = directories;
					foreach (string text in array)
					{
						string[] array2 = new string[1] { gameMode.ToString() + "_" };
						if (text.Contains(array2[0]))
						{
							string[] array3 = text.Split(array2, 2, StringSplitOptions.RemoveEmptyEntries);
							if (array3.Length < 2)
							{
								Log.Warning("Unexpected party entry");
							}
							else if (!list.Contains(array3[1]))
							{
								list.Add(array3[1]);
							}
							else
							{
								Log.Error("Two party files with the same party name exist");
							}
						}
					}
					return list;
				}
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred in FindPartyDataFileNames.");
		}
		return new List<string>();
	}

	public bool CanDelete(PartyAdventureData save)
	{
		return _savingLock.CurrentCount > 0;
	}

	public async Task DeleteCampaign(string partyName)
	{
		try
		{
			PartyAdventureData partyAdventureData = (await Global.LoadSavesFromDisk()).FirstOrDefault((PartyAdventureData c) => c.PartyName == partyName && c.GameMode == EGameMode.Campaign);
			if (partyAdventureData != null)
			{
				string path = await partyAdventureData.PartySaveDir;
				Global.InvalidateCache = true;
				if (Directory.Exists(path))
				{
					Directory.Delete(path, recursive: true);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred attempting to delete capaign.");
			List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
			list.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, InputFHManager.Controls.Generic.AlternativeChoice));
			SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00010", ex.StackTrace, list, ex.Message);
		}
	}

	public async Task DeletePartyData(PartyAdventureData partyData, EGameMode mode)
	{
		_ = 2;
		try
		{
			if (!CanDelete(partyData))
			{
				throw new Exception("Cannot delete current save");
			}
			if (mode != EGameMode.Campaign)
			{
				Log.Error("Invalid mode set for DeletePartyData");
				return;
			}
			string partySaveDir = await partyData.PartySaveDir;
			string partySaveFile = await partyData.PartyMainSaveFile;
			string path = await partyData.ScreenShotFile;
			if (File.Exists(partySaveFile))
			{
				File.Delete(partySaveFile);
			}
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			Global.InvalidateCache = true;
			if (Directory.Exists(partySaveDir) && !Directory.EnumerateFiles(partySaveDir, "*", SearchOption.AllDirectories).Any())
			{
				Directory.Delete(partySaveDir, recursive: true);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred attempting to delete party data.");
			List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
			list.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, InputFHManager.Controls.Generic.AlternativeChoice));
			SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00010", ex.StackTrace, list, ex.Message);
		}
	}

	public async Task LoadGlobalData()
	{
		Log.Info("Loading Global Data");
		bool messageDisplayed = false;
		bool globalDataReset = false;
		string globalSaveFile = await RootData.GlobalSaveFile;
		string previousGlobalSaveFile = await RootData.PreviousGlobalSaveFile;
		if (File.Exists(globalSaveFile))
		{
			try
			{
				bool shouldLoad = true;
				using (FileStream globalSaveFs = File.Open(globalSaveFile, FileMode.Open, FileAccess.Read))
				{
					Instance.CurrentFileBeingDeserialized = globalSaveFile;
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					BinaryFormatter binaryFormatter2 = binaryFormatter;
					binaryFormatter2.Binder = new SerializationBinding(await AssemblyInfoSaveDir);
					BinaryFormatter binaryFormatter3 = binaryFormatter;
					bool flag = false;
					try
					{
						Global = binaryFormatter3.Deserialize(globalSaveFs) as GlobalData;
					}
					catch (SerializationException ex)
					{
						if (!ex.Message.Contains("The ObjectManager found an invalid number of fixups"))
						{
							throw ex;
						}
						flag = true;
					}
					if (flag || !Global.IsCorrectVersion())
					{
						Log.Info("Reset Global Data version has been incremented - resetting global data");
						shouldLoad = false;
						if (AutoTestController.GameBootedForAutoTests)
						{
							ResetGlobalData();
						}
						else if (Application.isPlaying)
						{
							List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
							list.Add(new ErrorMessage.LabelAction("GUI_OK", () =>
							{
								ResetGlobalData();
							}, InputFHManager.Controls.Generic.AlternativeChoice));
							list.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, InputFHManager.Controls.Generic.Confirm));
							SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessage("ERROR_SAVEDATA_00006", list, "ERROR_SAVEDATA_00018", Environment.StackTrace, null, showErrorReportButton: false);
							messageDisplayed = true;
						}
					}
				}
				if (!messageDisplayed & shouldLoad)
				{
					Global.Load();
					Global.GameMode = EGameMode.MainMenu;
				}
			}
			catch (Exception ex2)
			{
				try
				{
					Log.Exception(ex2, "Unable to load file data.");
					if (ex2.Message.Contains("Binary stream '0' does not contain a valid BinaryHeader"))
					{
						if (File.Exists(globalSaveFile))
						{
							File.Delete(globalSaveFile);
						}
						if (File.Exists(previousGlobalSaveFile))
						{
							File.Delete(previousGlobalSaveFile);
						}
						await ResetGlobalData();
						return;
					}
					if (AutoTestController.GameBootedForAutoTests)
					{
						await ResetGlobalData();
						return;
					}
					List<ErrorMessage.LabelAction> list2 = new List<ErrorMessage.LabelAction>();
					list2.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SAVEFILE_BUTTON", () =>
					{
						ResetGlobalData();
					}, InputFHManager.Controls.Generic.AlternativeChoice));
					list2.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, InputFHManager.Controls.Generic.Confirm));
					SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessage("ERROR_SAVEDATA_00007", list2, "ERROR_SAVEDATA_00019", ex2.StackTrace, ex2.Message);
					messageDisplayed = true;
				}
				catch (Exception)
				{
					Log.Exception(ex2, "Exception during Exception handling!");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00007", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
					return;
				}
			}
		}
		else if (File.Exists(previousGlobalSaveFile))
		{
			try
			{
				using (FileStream globalSaveFs = File.Open(previousGlobalSaveFile, FileMode.Open, FileAccess.Read))
				{
					Instance.CurrentFileBeingDeserialized = previousGlobalSaveFile;
					BinaryFormatter binaryFormatter2 = new BinaryFormatter();
					BinaryFormatter binaryFormatter = binaryFormatter2;
					binaryFormatter.Binder = new SerializationBinding(await AssemblyInfoSaveDir);
					Global = binaryFormatter2.Deserialize(globalSaveFs) as GlobalData;
					Global.Load();
					Global.GameMode = EGameMode.MainMenu;
				}
				File.Delete(previousGlobalSaveFile);
				Task saveTask = SaveGlobalData();
				await saveTask;
				if (saveTask.HasFailed(out var exception))
				{
					throw exception;
				}
			}
			catch (Exception ex2)
			{
				try
				{
					Log.Exception(ex2, "Unable to load file data.");
					if (ex2.Message.Contains("Binary stream '0' does not contain a valid BinaryHeader"))
					{
						if (File.Exists(globalSaveFile))
						{
							File.Delete(globalSaveFile);
						}
						if (File.Exists(previousGlobalSaveFile))
						{
							File.Delete(previousGlobalSaveFile);
						}
						await ResetGlobalData();
						return;
					}
					if (AutoTestController.GameBootedForAutoTests)
					{
						await ResetGlobalData();
						return;
					}
					List<ErrorMessage.LabelAction> list3 = new List<ErrorMessage.LabelAction>();
					list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SAVEFILE_BUTTON", () =>
					{
						ResetGlobalData();
					}, InputFHManager.Controls.Generic.AlternativeChoice));
					list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, InputFHManager.Controls.Generic.Confirm));
					SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00007", ex2.StackTrace, list3, ex2.Message);
					messageDisplayed = true;
				}
				catch (Exception)
				{
					Log.Exception(ex2, "Exception during Exception handling!");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00007", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
					return;
				}
			}
		}
		else
		{
			await ResetGlobalData();
			globalDataReset = true;
		}
		if (!messageDisplayed && !globalDataReset)
		{
			await SaveGlobalData();
		}
		await Global.LoadSavesFromDisk();
	}

	public async Task ResetGlobalData()
	{
		Log.Info("Reset Global Data");
		Global = new GlobalData();
		Global.Load();
		Global.GameMode = EGameMode.MainMenu;
		await SaveGlobalData();
	}

	public async Task SaveGlobalData()
	{
		Log.Info("Saving Global Data");
		if (Global == null)
		{
			return;
		}
		GlobalDataChanged?.Invoke();
		if (Singleton<AutoSaveProgress>.IsInitialized)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		string globalSaveFile = await RootData.GlobalSaveFile;
		string assemblyInfoSaveDir = await Instance.AssemblyInfoSaveDir;
		try
		{
			Global.Save();
			bool flag = false;
			int num = 0;
			while (!flag && num < 3)
			{
				string directoryName = Path.GetDirectoryName(globalSaveFile);
				if (!Directory.Exists(directoryName))
				{
					Log.Info("Creating GlobalSaveFile directory: " + directoryName);
					Directory.CreateDirectory(directoryName);
				}
				try
				{
					using (FileStream serializationStream = File.Open(globalSaveFile, FileMode.Create, FileAccess.Write))
					{
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						binaryFormatter.Binder = new SerializationBinding(assemblyInfoSaveDir);
						binaryFormatter.Serialize(serializationStream, Global);
					}
					flag = true;
				}
				catch (Exception exception)
				{
					Log.Exception(exception);
					num++;
				}
			}
			if (!flag)
			{
				string tempFileName = Path.GetTempFileName();
				try
				{
					using (FileStream serializationStream2 = File.Open(tempFileName, FileMode.Create, FileAccess.Write))
					{
						new BinaryFormatter().Serialize(serializationStream2, Global);
					}
					flag = true;
				}
				catch (Exception exception2)
				{
					Log.Exception(exception2);
				}
				if (flag)
				{
					if (File.Exists(globalSaveFile))
					{
						File.Delete(globalSaveFile);
					}
					File.Move(tempFileName, globalSaveFile);
				}
			}
			if (!flag)
			{
				throw new Exception("Unable to save GlobalData data to " + globalSaveFile + ".  Something prevented saving to this location");
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred attempting to save global data.");
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("GUI_ERROR_RETRY", () =>
				{
					SaveGlobalData();
				}, InputFHManager.Controls.Generic.AlternativeChoice),
				new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, InputFHManager.Controls.Generic.Confirm)
			};
			SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00017", ex.StackTrace, buttons, ex.Message);
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.IsInitialized)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
	}

	public async Task AutoSaveCurrentAdventureData()
	{
		await SaveCurrentAdventureData(PartyAdventureData.SaveType.AutoSave);
	}

	public async Task QuickSaveCurrentAdventureData()
	{
		await SaveCurrentAdventureData(PartyAdventureData.SaveType.QuickSave);
	}

	public async Task ManualSaveCurrentAdventureData(string newFileName = null)
	{
		await SaveCurrentAdventureData(PartyAdventureData.SaveType.Manual, newFileName);
	}

	private async Task SaveCurrentAdventureData(PartyAdventureData.SaveType type, string newFileName = null)
	{
		if (AutoTestController.s_AutoTestCurrentlyLoaded || Global.GameMode == EGameMode.MainMenu)
		{
			return;
		}
		Log.SimpleInfo("Saving current adventure data");
		try
		{
			if (Global.CurrentAdventureData == null)
			{
				Log.Error("Current party adventure data was null when trying to save");
				return;
			}
			if (!(await _savingLock.WaitAsync(TimeSpan.FromSeconds(10.0))))
			{
				throw new Exception("Waiting for anoter save to finish timed out!");
			}
			if (Singleton<AutoSaveProgress>.IsInitialized)
			{
				Singleton<AutoSaveProgress>.Instance.ShowProgress();
			}
			_ = Global.CurrentAdventureData;
			bool flag = false;
			switch (type)
			{
			case PartyAdventureData.SaveType.Manual:
				flag = await Global.CurrentAdventureData.ManualSave(newFileName);
				break;
			case PartyAdventureData.SaveType.AutoSave:
				flag = await Global.CurrentAdventureData.AutoSave();
				break;
			case PartyAdventureData.SaveType.QuickSave:
				flag = await Global.CurrentAdventureData.QuickSave();
				break;
			}
			if (flag)
			{
				await SaveScreenshot(await Global.CurrentAdventureData.ScreenShotFile);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred attempting to save current party adventure data.");
		}
		finally
		{
			_savingLock.Release();
		}
	}

	public void InitialiseDataManagers()
	{
		AutoTestDataManager = new AutoTestDataManager();
		LevelEditorDataManager = new LevelEditorDataManager();
	}

	public async Task StartNewCustomLevel(CCustomLevelData levelData, PartyAdventureData customParty)
	{
		Global.CurrentEditorAutoTestData = null;
		Global.CurrentAutoTestDataCopy = null;
		Global.CurrentCustomLevelData = levelData;
		Global.CurrentlyPlayingCustomLevel = true;
		Global.CurrentAdventureData = customParty;
		Global.CurrentHostAccountID = customParty.Owner.PlatformAccountID;
		Global.AllSingleScenarios.Add(customParty);
		Global.CurrentHostAccountID = customParty.Owner.PlatformAccountID;
		await Instance.SaveGlobalData();
		levelData.ScenarioState.OverridePlayersInState(customParty.AdventureMapState.MapParty.ExportPlayerStates(levelData.ScenarioState));
		customParty.AdventureMapState.CurrentSingleScenario = levelData;
	}

	public async Task LoadCustomLevelFromData(CCustomLevelData levelData, LevelEditorController.ELevelEditorState forLevelEditorState = LevelEditorController.ELevelEditorState.Idle)
	{
		try
		{
			if (levelData.ScenarioState.ScenarioType != EScenarioType.Custom)
			{
				levelData.ScenarioState.ScenarioType = EScenarioType.Custom;
				Log.Error("Custom level data was set to YML not Custom! Please re-save this file");
			}
			Global.CurrentEditorAutoTestData = null;
			Global.CurrentAutoTestDataCopy = null;
			Global.CurrentCustomLevelData = levelData;
			switch (forLevelEditorState)
			{
			case LevelEditorController.ELevelEditorState.Idle:
				Global.CurrentEditorLevelData = null;
				Global.CurrentlyPlayingCustomLevel = true;
				break;
			case LevelEditorController.ELevelEditorState.Editing:
				Global.CurrentEditorLevelData = levelData;
				Global.CurrentlyPlayingCustomLevel = false;
				break;
			case LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty:
			case LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel:
			{
				GlobalData global = Global;
				global.CurrentEditorLevelData = await levelData.DeepCopySerializableObject<CCustomLevelData>();
				Global.CurrentlyPlayingCustomLevel = true;
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Unable to load custom scenario.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public async Task LoadCustomLevelDataFromFile(string filePath, LevelEditorController.ELevelEditorState forLevelEditorState = LevelEditorController.ELevelEditorState.Idle)
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		try
		{
			PartyAdventureData data = await PartyAdventureData.LoadSaveInfoFromFile(filePath);
			if (data != null)
			{
				await data.Load(EGameMode.SingleScenario, isJoiningMPClient: false);
				CCustomLevelData currentSingleScenario = data.FrosthavenSaveData.MapState.CurrentSingleScenario;
				await LoadCustomLevelFromData(currentSingleScenario, forLevelEditorState);
				return;
			}
			using FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
			Instance.CurrentFileBeingDeserialized = filePath;
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			BinaryFormatter binaryFormatter2 = binaryFormatter;
			binaryFormatter2.Binder = new SerializationBinding(await AssemblyInfoSaveDir);
			object obj = binaryFormatter.Deserialize(fs);
			CCustomLevelData levelData = obj as CCustomLevelData;
			if (obj is FHSaveData fHSaveData)
			{
				levelData = fHSaveData.MapState.CurrentSingleScenario;
			}
			await LoadCustomLevelFromData(levelData, forLevelEditorState);
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Unable to load custom scenario.\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public async Task LoadAutoTestFromData(AutoTestData autoTest, FileInfo autoTestFile, bool loadIntoEditor = false)
	{
		_ = 1;
		try
		{
			if (autoTest.ScenarioState.ScenarioType != EScenarioType.Custom)
			{
				autoTest.ScenarioState.ScenarioType = EScenarioType.Custom;
				Log.Error("Custom level data was set to YML not Custom! Please re-save this file");
			}
			if (autoTest != null)
			{
				if (autoTest.ScenarioState != null)
				{
					foreach (EnemyState allEnemyState in autoTest.ScenarioState.AllEnemyStates)
					{
						if (allEnemyState.ConfigPerPartySize != null && allEnemyState.ConfigPerPartySize.Count > 0 && allEnemyState.ConfigPerPartySize.ContainsKey(1) && allEnemyState.ConfigPerPartySize.ContainsKey(2))
						{
							allEnemyState.ConfigPerPartySize[1] = allEnemyState.ConfigPerPartySize[2];
						}
					}
				}
				if (autoTest.ExpectedResultingScenarioState != null)
				{
					foreach (EnemyState allEnemyState2 in autoTest.ExpectedResultingScenarioState.AllEnemyStates)
					{
						if (allEnemyState2.ConfigPerPartySize != null && allEnemyState2.ConfigPerPartySize.Count > 0 && allEnemyState2.ConfigPerPartySize.ContainsKey(1) && allEnemyState2.ConfigPerPartySize.ContainsKey(2))
						{
							allEnemyState2.ConfigPerPartySize[1] = allEnemyState2.ConfigPerPartySize[2];
						}
					}
				}
			}
			AutoTestDataManager.CurrentlyRunningAutotestFile = autoTestFile;
			GlobalData global = Global;
			global.CurrentAutoTestDataCopy = await autoTest.DeepCopySerializableObject<AutoTestData>();
			Global.CurrentAutoTestDataCopy.ScenarioState.StateNeedsUpdatesSaved = autoTest.ScenarioState.StateNeedsUpdatesSaved;
			global = Global;
			global.LastLoadedAutoTestData = await autoTest.DeepCopySerializableObject<AutoTestData>();
			Global.LastLoadedAutoTestData.ScenarioState.StateNeedsUpdatesSaved = autoTest.ScenarioState.StateNeedsUpdatesSaved;
			if (loadIntoEditor)
			{
				Global.CurrentEditorAutoTestData = autoTest;
				Global.CurrentEditorLevelData = autoTest;
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Unable to load Autotest.");
		}
	}

	public void ParseCommandLineArgs()
	{
		try
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				Log.Info("COMMAND LINE ARG " + i + ": " + commandLineArgs[i]);
				if (commandLineArgs[i].ToLower() == "-autotest")
				{
					if (i + 2 >= commandLineArgs.Length)
					{
						Log.Error("Bad arguments for Autotests - it requires an additional argument: event log to run.");
					}
					else
					{
						AutoTestFolder = commandLineArgs[i + 1];
						AutoTestResultsLog = commandLineArgs[i + 2];
						AutoTestController.GameBootedForAutoTests = true;
						i += 2;
					}
				}
				if (commandLineArgs[i].ToLower() == "-loadmostrecentsave")
				{
					LoadMostRecentSave = true;
				}
				if (commandLineArgs[i].ToLower() == "-debug")
				{
					Main.s_DevMode = true;
				}
				if (commandLineArgs[i].ToLower() == "-autoupdate")
				{
					AutoupdateAutotests = true;
				}
			}
			IsModdingEnabled = true;
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An error occurrs attempting to parse the command line arguments.");
			Application.Quit(1);
		}
	}

	public static string NextAvailableFilename(string path)
	{
		string text = "_({0})";
		if (!File.Exists(path))
		{
			return path;
		}
		if (Path.HasExtension(path))
		{
			return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), text));
		}
		return GetNextFilename(path + text);
	}

	private static string GetNextFilename(string pattern)
	{
		string text = string.Format(pattern, 1);
		if (text == pattern)
		{
			throw new ArgumentException("The pattern must include an index place-holder", "pattern");
		}
		if (!File.Exists(text))
		{
			return text;
		}
		int num = 1;
		int num2 = 2;
		while (File.Exists(string.Format(pattern, num2)))
		{
			num = num2;
			num2 *= 2;
		}
		while (num2 != num + 1)
		{
			int num3 = (num2 + num) / 2;
			if (File.Exists(string.Format(pattern, num3)))
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		return string.Format(pattern, num2);
	}

	public void Update()
	{
		if (_takeScreenshotRequested)
		{
			if (_takeScreenshotState == TakingScreenshotState.None)
			{
				StartCoroutine(TakeScreenshotCrt());
			}
			_takeScreenshotRequested = false;
		}
	}

	public async Task SaveScreenshot(string path)
	{
		_takeScreenshotRequested = true;
		while (_takeScreenshotState != TakingScreenshotState.Completed)
		{
			await Task.Yield();
		}
		if (_takeScreenshotSuccess && _smallTextureEncoded != null)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllBytes(path, _smallTextureEncoded);
		}
		_takeScreenshotState = TakingScreenshotState.None;
	}

	private IEnumerator TakeScreenshotCrt()
	{
		_takeScreenshotState = TakingScreenshotState.InProgress;
		yield return new WaitForEndOfFrame();
		int width = Screen.width;
		int height = Screen.height;
		if (!_rtSmall)
		{
			_rtSmall = RenderTexture.GetTemporary(640, 360, 0);
		}
		if (!_screenshotTexture)
		{
			_screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);
		}
		if (!_smallTexture)
		{
			_smallTexture = new Texture2D(640, 360, TextureFormat.RGB24, mipChain: false);
		}
		_screenshotTexture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		_screenshotTexture.Apply();
		yield return new WaitForEndOfFrame();
		Graphics.Blit(_screenshotTexture, _rtSmall);
		yield return new WaitForEndOfFrame();
		try
		{
			RenderTexture.active = _rtSmall;
			_smallTexture.ReadPixels(new Rect(0f, 0f, _rtSmall.width, _rtSmall.height), 0, 0);
			_smallTexture.Apply();
			RenderTexture.active = null;
		}
		catch (Exception)
		{
			_takeScreenshotSuccess = false;
		}
		_takeScreenshotSuccess = true;
		yield return new WaitForEndOfFrame();
		if (_takeScreenshotSuccess)
		{
			_smallTextureEncoded = _smallTexture.EncodeToPNG();
		}
		else
		{
			_smallTextureEncoded = null;
		}
		_takeScreenshotState = TakingScreenshotState.Completed;
	}

	public bool CanSave()
	{
		if (_savingLock.CurrentCount == 0)
		{
			return false;
		}
		if (Global.CurrentAdventureData == null)
		{
			return false;
		}
		if (Global.CurrentAdventureData.IsSaving)
		{
			return false;
		}
		if (Singleton<FHMapChoreographer>.InstanceFast != null)
		{
			if (Singleton<FHMapChoreographer>.InstanceFast.GameState?.Outpost?.ActiveEvent?.Attack != null)
			{
				return false;
			}
			return true;
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			if (FastReload.FastUndoEnabled)
			{
				return FastReload.CanSave();
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanLoad()
	{
		if (_savingLock.CurrentCount == 0)
		{
			return false;
		}
		if (FFSNetwork.IsOnline)
		{
			return false;
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			return FastReload.CanLoad();
		}
		return true;
	}

	[ConsoleCommand(Command = "delete")]
	public static void Delete(IConsole console, string newFileName = null)
	{
		Instance.StartCoroutine(DeleteCommandCrt(console, newFileName));
	}

	public static IEnumerator DeleteCommandCrt(IConsole console, string fileName = null)
	{
		(console as CommandExecution)?.OpenStream();
		PartyAdventureData save;
		if (fileName == null)
		{
			Task<PartyAdventureData> mrs = Instance.Global.FindMostRecentSave();
			yield return new WaitForTask(mrs);
			save = mrs.Result;
		}
		else
		{
			Task<List<PartyAdventureData>> t = Instance.Global.LoadSavesFromDisk();
			yield return new WaitForTask(t);
			List<PartyAdventureData> source = t.Result;
			if (Instance.Global.CurrentAdventureData != null)
			{
				source = source.Where((PartyAdventureData partyAdventureData) => partyAdventureData.PartyName == Instance.Global.CurrentAdventureData.PartyName).ToList();
			}
			save = source.FirstOrDefault((PartyAdventureData partyAdventureData) => partyAdventureData.SaveFileName.ToLowerInvariant() == fileName.ToLowerInvariant());
		}
		if (save != null)
		{
			if (!Instance.CanDelete(save))
			{
				console.WriteLineWithStyle("Cannot delete save", CommandOutputStyle.Error);
			}
			else
			{
				Task<string> t2 = save.PartyMainSaveFile;
				yield return new WaitForTask(t2);
				console.WriteLine("Deleting save '" + t2.Result + "'");
				Task task = Instance.DeletePartyData(save, EGameMode.Campaign);
				yield return new WaitForTask(task);
			}
		}
		else
		{
			console.WriteLineWithStyle("Save '" + fileName + "' not found", CommandOutputStyle.Error);
		}
		(console as CommandExecution)?.CloseStream();
	}

	[ConsoleCommand(Command = "save")]
	public static void Save(IConsole console, string newFileName = null)
	{
		if (!Instance.CanSave())
		{
			console.WriteLineWithStyle("Cannot save right now", CommandOutputStyle.Error);
		}
		else
		{
			Instance.StartCoroutine(SaveCommandCrt(console, newFileName));
		}
	}

	private static IEnumerator SaveCommandCrt(IConsole console, string newFileName = null)
	{
		(console as CommandExecution)?.OpenStream();
		Task<List<PartyAdventureData>> t0 = Instance.Global.LoadSavesFromDisk();
		while (!t0.IsCompleted)
		{
			yield return null;
		}
		PartyAdventureData.SaveType type = (string.IsNullOrEmpty(newFileName) ? PartyAdventureData.SaveType.QuickSave : PartyAdventureData.SaveType.Manual);
		Task t1 = Instance.SaveCurrentAdventureData(type, newFileName);
		while (!t1.IsCompleted)
		{
			yield return null;
		}
		Task<string> partyMainSaveFile = Instance.Global.CurrentAdventureData.PartyMainSaveFile;
		partyMainSaveFile.Wait();
		console.WriteLine("Saved '" + partyMainSaveFile.Result + "'");
		(console as CommandExecution)?.CloseStream();
	}

	[ConsoleCommand(Command = "load")]
	public static void Load(IConsole console, string fileName = null)
	{
		if (!Instance.CanLoad())
		{
			console.WriteLineWithStyle("Cannot load right now", CommandOutputStyle.Error);
		}
		else
		{
			Instance.StartCoroutine(LoadCommandCrt(console, fileName));
		}
	}

	public static IEnumerator LoadCommandCrt(IConsole console, string fileName = null)
	{
		(console as CommandExecution)?.OpenStream();
		PartyAdventureData partyAdventureData;
		if (fileName == null)
		{
			Task<PartyAdventureData> mrs = Instance.Global.FindMostRecentSave();
			yield return new WaitForTask(mrs);
			partyAdventureData = mrs.Result;
		}
		else
		{
			Task<List<PartyAdventureData>> t = Instance.Global.LoadSavesFromDisk();
			while (!t.IsCompleted)
			{
				yield return null;
			}
			List<PartyAdventureData> source = t.Result;
			if (Instance.Global.CurrentAdventureData != null)
			{
				source = source.Where((PartyAdventureData partyAdventureData2) => partyAdventureData2.PartyName == Instance.Global.CurrentAdventureData.PartyName).ToList();
			}
			partyAdventureData = source.FirstOrDefault((PartyAdventureData partyAdventureData2) => partyAdventureData2.SaveFileName.ToLowerInvariant() == fileName.ToLowerInvariant());
		}
		if (partyAdventureData != null)
		{
			Task<string> partyMainSaveFile = partyAdventureData.PartyMainSaveFile;
			partyMainSaveFile.Wait();
			console.WriteLine("Loading save '" + partyMainSaveFile.Result + "'");
			Instance.LoadCampaignMode(partyAdventureData, isJoiningMPClient: false);
		}
		else
		{
			console.WriteLineWithStyle("Save '" + fileName + "' not found", CommandOutputStyle.Error);
		}
		(console as CommandExecution)?.CloseStream();
	}

	[ConsoleCommand(Command = "dbg_create_many_save")]
	public static void CreateManySaves(IConsole console, string prefix = null, int saves = 100)
	{
		Instance.StartCoroutine(CreateManySavesCrt(console, prefix, saves));
	}

	private static IEnumerator CreateManySavesCrt(IConsole console, string prefix, int saves)
	{
		(console as CommandExecution)?.OpenStream();
		int i = 0;
		while (i < saves)
		{
			string newFileName = $"{prefix ?? string.Empty}_{i}";
			console.WriteLine($"{i + 1}/{saves}");
			yield return new WaitForTask(Instance.ManualSaveCurrentAdventureData(newFileName));
			int num = i + 1;
			i = num;
		}
		(console as CommandExecution)?.CloseStream();
	}
}
