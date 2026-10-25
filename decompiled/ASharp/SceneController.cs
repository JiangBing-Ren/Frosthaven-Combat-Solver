using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AStar;
using Assets.Script.GUI.MainMenu.Modding;
using Chronos;
using ClockStone;
using FFSNet;
using FH;
using FH.ApparanceBaking;
using FH.GraphicalUserInterface;
using FH.GraphicalUserInterface.MainMenu;
using FH.Misc;
using FH.Tools;
using FH.YML;
using FHGUI;
using FHGUI.States;
using FHRL;
using GLOOM;
using GLOOM.MainMenu;
using Libs.GameConsole;
using Libs.Penumbra;
using Libs.Threading;
using Libs.Utils.Profiler;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Achievments;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class SceneController : MonoBehaviour
{
	public enum RestartRoundMode
	{
		Default,
		FullRestart,
		FastRestart,
		FastUndo,
		FastRedo,
		EnableFastUndo,
		DisableFastUndo
	}

	private enum LaunchType
	{
		MainMenu,
		Editor,
		SandboxNew,
		SandboxResume,
		CampaignNew,
		CampaignResume,
		ScenarioPreset,
		ScenarioWithParty,
		JoinMultiplayer
	}

	private const string InitialLoadingScreen = "InitialLoadingScreen";

	private const string MainMenuScene = "MainMenu";

	private const string ScenarioScene = "Game";

	private const string EmptyTransitionScene = "EmptyTransitionScene";

	public static SceneController Instance;

	[NonSerialized]
	public Thread MainThread;

	public GameObject MainSceneCamera;

	public LoadingScreen LoadingScreenInstance;

	public GameObject GlobalCanvas;

	public GameObject ErrorMessagePrefab;

	private List<string> scenesInBuild;

	private bool ymlChecksumsMatch;

	private Scene currentlyLoadedScene;

	private bool mostRecentSaveLoaded;

	private AudioObject _currentMusic;

	private AudioObject _outpostAmbient;

	private AudioObject _worldMapAmbient;

	private string _applicationDataPath;

	private bool _cardsFailedLoading;

	public KronosProfilingScaffolding LoadingProfiler = new KronosProfilingScaffolding();

	private string SBeg = "{{{ ";

	private string SEnd = "}}} ";

	[ConsoleVariable(Alias = "launch_scenario_with_party")]
	public static string LaunchScenarioWithParty_kvar;

	private static string _quickJoinMultipayerSessionID;

	private static string _quickLaunchLevel;

	private static LaunchType _launchType;

	private string FHCampaignMapScene => "FHCampaignMap";

	public bool ReloadMainMenu { get; set; }

	public ErrorMessage GlobalErrorMessage { get; set; }

	public bool IsLoading { get; private set; }

	public bool ScenarioIsLoading { get; set; }

	public Scene GetCurrentScene => currentlyLoadedScene;

	public YMLLoading YML { get; set; }

	public ModdingService ModService { get; private set; }

	public bool GameLoadedAndHostReady { get; set; }

	public bool GameLoadedAndClientReady { get; set; }

	public bool ModLoadingCompleted { get; set; }

	public float GameSpeedIncreaseAmount { get; set; }

	public bool SelectingPersonalQuest { get; set; }

	public bool BusyProcessingResults { get; set; }

	public bool RetiringCharacter { get; set; }

	public bool CheckingLockedContent { get; set; }

	public List<string> MainMenuVideos { get; set; }

	public HashSet<int> MainMenuVideoHashSet { get; set; }

	public bool InitialLoadingComplete { get; private set; }

	public string ApplicationDataPath
	{
		get
		{
			if (!string.IsNullOrEmpty(_applicationDataPath))
			{
				return _applicationDataPath;
			}
			return Application.dataPath;
		}
	}

	public string GetCurrentSceneName
	{
		get
		{
			_ = currentlyLoadedScene;
			return currentlyLoadedScene.name;
		}
	}

	public event BasicEventHandler OnSaveDataLoaded;

	private void Awake()
	{
		Instance = this;
		PanicReset.EnsureInstance();
		if (Thread.CurrentThread.Name == null)
		{
			Thread.CurrentThread.Name = "GH-Main";
		}
		_applicationDataPath = Application.dataPath;
		ReloadMainMenu = false;
		ScenarioIsLoading = false;
		MainThread = Thread.CurrentThread;
		ModService = new ModdingService();
		GameSpeedIncreaseAmount = 3f;
		try
		{
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				MainMenuVideos = Directory.GetFiles(Path.Combine(Application.dataPath, "Resources", "Data", "StreamingAssets", "Movies", "Ambient"), "*.webm").ToList();
			}
			else
			{
				MainMenuVideos = Directory.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies", "Ambient"), "*.webm").ToList();
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception while trying to find Main Menu movie files.\n");
			MainMenuVideos = new List<string>();
		}
		MainMenuVideoHashSet = new HashSet<int>();
		ModLoadingCompleted = false;
		if (GlobalCanvas != null)
		{
			GlobalCanvas.SetActive(value: true);
		}
		GlobalErrorMessage = UnityEngine.Object.Instantiate(ErrorMessagePrefab, GlobalCanvas.transform).GetComponent<ErrorMessage>();
		GlobalErrorMessage.Hide();
		ShowLoadingScreen();
	}

	private IEnumerator Start()
	{
		Log.Info("Start SceneController");
		BudgetCoroutineRunner budgetCoroutineRunner = new BudgetCoroutineRunner("InitializeSceneController", Init());
		yield return budgetCoroutineRunner.RunCoroutine();
	}

	public IEnumerator Init()
	{
		YML = new YMLLoading();
		yield return SaveData.Instance.LoadRulebase();
		if (Application.isPlaying)
		{
			while ((bool)GlobalErrorMessage && GlobalErrorMessage.ShowingMessage)
			{
				yield return null;
			}
			if ((bool)LoadingScreenInstance)
			{
				LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.LoadingWithProgress);
				yield return null;
			}
		}
		Log.Info("Creating Custom Level directories");
		Task<string> levelEditorCustomLvlDatTask = RootSaveData.LevelEditorCustomLVLDAT;
		yield return new WaitForTask(levelEditorCustomLvlDatTask);
		if (!Directory.Exists(levelEditorCustomLvlDatTask.Result))
		{
			Directory.CreateDirectory(levelEditorCustomLvlDatTask.Result);
		}
		Task<string> levelEditorCustomYMLTask = RootSaveData.LevelEditorCustomYML;
		yield return new WaitForTask(levelEditorCustomYMLTask);
		if (!Directory.Exists(levelEditorCustomYMLTask.Result))
		{
			Directory.CreateDirectory(levelEditorCustomYMLTask.Result);
		}
		yield return PersistentData.s_Instance.InitGlobalData();
		if (Application.isPlaying)
		{
			StartCoroutine(StartAudio());
		}
		try
		{
			OnSaveDataLoaded?.Invoke();
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while loading main menu scene at startup\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			yield break;
		}
		if ((bool)LoadingScreenInstance)
		{
			while (GlobalErrorMessage.ShowingMessage)
			{
				yield return null;
			}
			LoadingScreenInstance.UpdateProgressBar(100f);
			yield return null;
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			yield return null;
		}
		try
		{
			scenesInBuild = new List<string>();
			for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string scenePathByBuildIndex = SceneUtility.GetScenePathByBuildIndex(i);
				scenePathByBuildIndex.LastIndexOf("/");
				scenesInBuild.Add(Path.GetFileNameWithoutExtension(scenePathByBuildIndex));
			}
			Log.Info(new StringBuilder(scenesInBuild.Count).AppendLine("Scenes in build:").AppendJoin('\n', scenesInBuild).ToString());
		}
		catch (Exception ex2)
		{
			Log.Exception(ex2, "An exception occurred while loading main menu scene at startup\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
			yield break;
		}
		if (SaveData.Instance.LoadMostRecentSave)
		{
			yield return LoadMostRecentSave();
			if (mostRecentSaveLoaded)
			{
				mostRecentSaveLoaded = false;
				yield break;
			}
		}
		SaveData.Instance.Global.MPEndOfTurnCompare = false;
		if (Application.isPlaying && GlobalErrorMessage.ShowingMessage)
		{
			yield break;
		}
		FFSNetwork.OnDesyncDetected = ShowDesyncDetectionError;
		if (!string.IsNullOrEmpty(LaunchScenarioWithParty_kvar))
		{
			_quickLaunchLevel = LaunchScenarioWithParty_kvar;
			_launchType = LaunchType.ScenarioWithParty;
		}
		LaunchType launchType = _launchType;
		if ((uint)(launchType - 1) <= 7u)
		{
			while (!TelemetryAndEulaPolicy.IsEulaAccepted)
			{
				TelemetryAndEulaPolicyUI.ShowTelemetryAndEulaPolicy();
				yield return null;
			}
		}
		switch (_launchType)
		{
		case LaunchType.Editor:
		{
			Task<CCustomLevelData> dataTask = GetCustomLevelData(_quickLaunchLevel);
			yield return new WaitForTask(dataTask);
			if (dataTask.HasFailed(out var exception3))
			{
				HandleQuickLaunchError(exception3.Message);
				yield break;
			}
			CCustomLevelData result3 = dataTask.Result;
			if (result3 != null)
			{
				yield return StartEditLevel(result3);
			}
			break;
		}
		case LaunchType.SandboxNew:
			LoadMainMenu();
			InitialLoadingComplete = true;
			while (MainMenuUIManager.Instance == null || !MainMenuUIManager.Instance.Initialised)
			{
				yield return null;
			}
			ShowLoadingScreen();
			yield return null;
			yield return YML.LoadSingleScenariosAsyncCrt(ScenarioManager.EDLLMode.Campaign);
			if (!YML.LastLoadResult)
			{
				HandleQuickLaunchError("Unable to load YML for DLL Mode: Campaign");
				yield break;
			}
			DisableLoadingScreen();
			UnityEngine.Object.FindObjectOfType<CustomPartySetup>(includeInactive: true).gameObject.SetActive(value: true);
			CustomPartySetup.Instance.ShowForSandbox();
			break;
		case LaunchType.SandboxResume:
		{
			if (!SaveData.Instance.Global.AllSingleScenarios.Exists((PartyAdventureData s) => s.PartyName == "SandboxScenario" && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID))
			{
				HandleQuickLaunchError("Resume data for sandbox does not exist");
				yield break;
			}
			yield return YML.LoadSingleScenariosAsyncCrt(ScenarioManager.EDLLMode.Campaign);
			if (!YML.LastLoadResult)
			{
				HandleQuickLaunchError("Unable to load YML for DLL Mode: Campaign");
				yield break;
			}
			PartyAdventureData resumeSingleScenario = SaveData.Instance.Global.AllSingleScenarios.FirstOrDefault((PartyAdventureData s) => s.PartyName == "SandboxScenario");
			SaveData.Instance.Global.SetResumeSingleScenario(resumeSingleScenario);
			SaveData.Instance.Global.CurrentHostAccountID = PlatformLayer.UserData.PlatformAccountID;
			ResumeSingleScenario();
			break;
		}
		case LaunchType.CampaignNew:
			StartNewCampaign();
			break;
		case LaunchType.CampaignResume:
		{
			Task<PartyAdventureData> campaignsTask = SaveData.Instance.Global.GetResumeCampaign();
			yield return new WaitForTask(campaignsTask);
			PartyAdventureData resumeData = campaignsTask.Result;
			if (resumeData == null)
			{
				HandleQuickLaunchError("Resume data for campaign does not exist");
				yield break;
			}
			Task task = SaveData.Instance.Global.DeserializeGameStateAndValidateOnLoad(EGameMode.Campaign, resumeData);
			yield return new WaitForTask(task);
			SaveData.Instance.LoadCampaignMode(resumeData, isJoiningMPClient: false);
			break;
		}
		case LaunchType.ScenarioPreset:
		{
			Task<CCustomLevelData> dataTask = GetCustomLevelData(_quickLaunchLevel);
			yield return new WaitForTask(dataTask);
			if (dataTask.HasFailed(out var exception2))
			{
				HandleQuickLaunchError(exception2.Message);
				yield break;
			}
			CCustomLevelData result2 = dataTask.Result;
			yield return StartCoroutine(StartScenarioWithPresetCharacters(result2));
			break;
		}
		case LaunchType.ScenarioWithParty:
		{
			Task<CCustomLevelData> dataTask = GetCustomLevelData(_quickLaunchLevel);
			yield return new WaitForTask(dataTask);
			if (dataTask.HasFailed(out var exception))
			{
				HandleQuickLaunchError(exception.Message);
				yield break;
			}
			CCustomLevelData result = dataTask.Result;
			yield return StartCoroutine(StartScenarioWithParty(result));
			break;
		}
		case LaunchType.JoinMultiplayer:
			StartCoroutine(LoadMainMenuCrt(_quickJoinMultipayerSessionID));
			break;
		default:
			try
			{
				if (AutoTestController.GameBootedForAutoTests)
				{
					AutoTestController.s_Instance.StartAutoTests(AutoTestController.OnAutoTestCompleteAndQuit);
				}
				else if (Application.isPlaying)
				{
					LoadMainMenu();
				}
			}
			catch (Exception ex3)
			{
				Log.Exception(ex3, "An exception occurred while loading main menu scene at startup\n");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex3.StackTrace, Application.Quit, ex3.Message);
				yield break;
			}
			break;
		}
		InitialLoadingComplete = true;
		void StartNewCampaign()
		{
			GameData gameData = new GameData
			{
				GameName = Guid.NewGuid().ToString(),
				Difficulty = EAdventureDifficulty.Normal,
				Rules = ERulesFlag.None
			};
			NewCampaignStart(gameData.GameName, gameData.Difficulty, gameData.Rules);
		}
	}

	[ConsoleCommand(Command = "play", Description = " <scenario> <party|preset> Starts a scenario with party or preset characters")]
	public static async void PlayConsoleCommand(IConsole console, string scenarioName, string mode = "party")
	{
		CCustomLevelData cCustomLevelData;
		try
		{
			cCustomLevelData = await GetCustomLevelData(scenarioName);
		}
		catch (Exception ex)
		{
			throw new ConsoleCommandException(ex.Message);
		}
		SceneController instance = Instance;
		if (instance.IsLoading)
		{
			throw new ConsoleCommandException("Cannot start a scenario while loading");
		}
		console.WriteLine("Staring scenario '" + cCustomLevelData.Name + "'");
		if (mode == "preset")
		{
			instance.StartCoroutine(instance.StartScenarioWithPresetCharacters(cCustomLevelData));
		}
		else
		{
			instance.StartCoroutine(instance.StartScenarioWithParty(cCustomLevelData));
		}
	}

	[ConsoleCommand(Command = "copy_scenario")]
	public static async void CopyScenario(IConsole console, string scenarioID, bool copyObjects = true, int xOffs = 0, int yOffs = 0)
	{
		if (ScenarioManager.CurrentScenarioState == null || !GameState.IsInLevelEditor)
		{
			return;
		}
		CCustomLevelData levelData;
		try
		{
			levelData = await GetCustomLevelData(scenarioID);
		}
		catch (Exception ex)
		{
			throw new ConsoleCommandException(ex.Message);
		}
		ScenarioRuleLibrary.ScenarioState css = ScenarioManager.CurrentScenarioState;
		css.ScenarioModifiers.Clear();
		css.WinObjectives.Clear();
		css.LoseObjectives.Clear();
		CCustomLevelData currentLevelData = SaveData.Instance.Global.CurrentEditorLevelData;
		foreach (CScenarioModifier scenarioModifier in levelData.ScenarioState.ScenarioModifiers)
		{
			CScenarioModifier item = await scenarioModifier.DeepCopySerializableObject<CScenarioModifier>();
			css.ScenarioModifiers.Add(item);
		}
		foreach (CObjective winObjective in levelData.ScenarioState.WinObjectives)
		{
			CObjective item2 = await winObjective.DeepCopySerializableObject<CObjective>();
			css.WinObjectives.Add(item2);
		}
		foreach (CObjective loseObjective in levelData.ScenarioState.LoseObjectives)
		{
			CObjective item3 = await loseObjective.DeepCopySerializableObject<CObjective>();
			css.LoseObjectives.Add(item3);
		}
		if (copyObjects)
		{
			for (int num = css.Props.Count - 1; num >= 0; num--)
			{
				if (!(css.Props[num] is CObjectDoor))
				{
					css.Props[num].DestroyProp();
				}
			}
			foreach (CObjectProp prop in levelData.ScenarioState.Props)
			{
				if (prop is CObjectDoor)
				{
					continue;
				}
				TileIndex arrayIndex = prop.ArrayIndex;
				Point newPos = new Point(arrayIndex.X + xOffs, arrayIndex.Y + yOffs);
				CTile tile = ScenarioManager.GetTile(newPos);
				if (tile != null)
				{
					CObjectProp cObjectProp = await prop.DeepCopySerializableObject<CObjectProp>();
					cObjectProp.SetLocation(new TileIndex(newPos.X, newPos.Y), null, prop.Rotation);
					cObjectProp.StartingMapGuid = tile.m_HexMap.MapGuid;
					tile.SpawnProp(cObjectProp);
					CApparanceOverrideDetails apparanceOverrideDetailsForGUID = levelData.GetApparanceOverrideDetailsForGUID(prop.PropGuid);
					if (apparanceOverrideDetailsForGUID != null)
					{
						CApparanceOverrideDetails item4 = await apparanceOverrideDetailsForGUID.DeepCopySerializableObject<CApparanceOverrideDetails>();
						currentLevelData.ApparanceOverrideList.Add(item4);
					}
				}
			}
			for (int num2 = css.Spawners.Count - 1; num2 >= 0; num2--)
			{
				css.Spawners[num2].HideSpawnerProp(forced: true);
				css.Spawners.RemoveAt(num2);
			}
			foreach (CSpawner spawner in levelData.ScenarioState.Spawners)
			{
				TileIndex arrayIndex2 = spawner.ArrayIndex;
				Point newPos = new Point(arrayIndex2.X + xOffs, arrayIndex2.Y + yOffs);
				CTile tile = ScenarioManager.GetTile(newPos);
				if (tile != null)
				{
					CSpawner cSpawner = await spawner.DeepCopySerializableObject<CSpawner>();
					cSpawner.SetLocation(new TileIndex(newPos.X, newPos.Y));
					cSpawner.StartingMapGuid = tile.m_HexMap.MapGuid;
					tile.SpawnSpawner(cSpawner);
				}
			}
			CopyMonsterList(css, levelData, "Enemy", xOffs, yOffs);
			CopyMonsterList(css, levelData, "Enemy2", xOffs, yOffs);
			CopyMonsterList(css, levelData, "Neutral", xOffs, yOffs);
			CopyMonsterList(css, levelData, "Ally", xOffs, yOffs);
		}
		if (currentLevelData != null)
		{
			currentLevelData.LevelMessages.Clear();
			currentLevelData.LevelEvents.Clear();
			foreach (CLevelMessage levelMessage in levelData.LevelMessages)
			{
				CLevelMessage item5 = await levelMessage.DeepCopySerializableObject<CLevelMessage>();
				currentLevelData.LevelMessages.Add(item5);
			}
			foreach (CLevelEvent levelEvent in levelData.LevelEvents)
			{
				CLevelEvent item6 = await levelEvent.DeepCopySerializableObject<CLevelEvent>();
				currentLevelData.LevelEvents.Add(item6);
			}
		}
		css.LootTable = levelData.ScenarioState.LootTable.ToArray();
	}

	private static async void CopyMonsterList(ScenarioRuleLibrary.ScenarioState css, CCustomLevelData levelData, string monsterType, int xOffs = 0, int yOffs = 0)
	{
		List<EnemyState> list = null;
		List<EnemyState> list2 = null;
		switch (monsterType)
		{
		case "Enemy":
			list = levelData.ScenarioState.Monsters;
			list2 = css.Monsters;
			break;
		case "Enemy2":
			list = levelData.ScenarioState.Enemy2Monsters;
			list2 = css.Enemy2Monsters;
			break;
		case "Neutral":
			list = levelData.ScenarioState.NeutralMonsters;
			list2 = css.NeutralMonsters;
			break;
		case "Ally":
			list = levelData.ScenarioState.AllyMonsters;
			list2 = css.AllyMonsters;
			break;
		}
		if (list == null || list2 == null)
		{
			return;
		}
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			if (list2[num]?.Actor is CEnemyActor enemyToRemove)
			{
				LevelEditorController.RemoveMonster(enemyToRemove);
				list2.RemoveAt(num);
			}
		}
		foreach (EnemyState item in list)
		{
			TileIndex location = item.Location;
			Point newPos = new Point(location.X + xOffs, location.Y + yOffs);
			CTile tile = ScenarioManager.GetTile(newPos);
			if (tile != null)
			{
				EnemyState enemyState = await item.DeepCopySerializableObject<EnemyState>();
				enemyState.Location = new TileIndex(newPos);
				enemyState.StartingMapGuid = tile.m_HexMap.MapGuid;
				LevelEditorController.SpawnMonsterAtLocation(enemyState);
			}
		}
	}

	private IEnumerator StartScenarioWithParty(CCustomLevelData levelData)
	{
		ShowLoadingScreen();
		if (Choreographer.s_Choreographer != null)
		{
			yield return StartCoroutine(EndScenarioSafely());
		}
		SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
		Task<CCustomLevelData> copyTask = levelData.DeepCopySerializableObject<CCustomLevelData>();
		yield return new WaitForTask(copyTask);
		SaveData.Instance.Global.CurrentEditorLevelData = copyTask.Result;
		LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Campaign;
		LoadCustomLevelDataService loadCustomLevelDataService = new LoadCustomLevelDataService(SaveData.Instance.Global.CurrentEditorLevelData);
		Task<List<IGameSaveData>> task = loadCustomLevelDataService.GetSaves();
		yield return new WaitForTask(task);
		List<IGameSaveData> result = task.Result;
		PartyGameSaveData sgd = (PartyGameSaveData)result.OrderByDescending((IGameSaveData gameSaveData) => gameSaveData.LastSavedTimeStamp).FirstOrDefault((IGameSaveData gameSaveData) => gameSaveData.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
		if (true && sgd != null)
		{
			yield return YML.LoadSingleScenariosAsyncCrt(ScenarioManager.EDLLMode.Campaign);
			if (!YML.LastLoadResult)
			{
				HandleQuickLaunchError("Unable to load YML for DLL Mode: Campaign");
				yield break;
			}
			PartyAdventureData partyData = sgd.PartyAdventureData;
			Task task2 = partyData.Load(partyData.GameMode, isJoiningMPClient: false);
			yield return new WaitForTask(task2);
			AdventureState.UpdateMapState(partyData.AdventureMapState);
			ScenarioManager.SetHouseRules(partyData.AdventureMapState.HouseRulesSetting);
			partyData.AdventureMapState.MapParty.OnMapStateAdventureStarted();
			LevelEditorController.s_Instance.SetLevelEditorState(LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty);
			Task task3 = SaveData.Instance.StartNewCustomLevel(SaveData.Instance.Global.CurrentEditorLevelData, sgd.PartyAdventureData);
			yield return new WaitForTask(task3);
			Instance.LoadSingleScenario(partyData);
		}
		else
		{
			if (sgd != null)
			{
				MainMenuUIAutomation.SaveFile = sgd.GameName;
			}
			MainMenuUIAutomation.LevelEditorAutoSelectMode = true;
			MainMenuUIManager.SetLoadingCompleteCallback(OnMainMenuLoadedToLoadPartyCallback);
			Instance.LoadMainMenu();
		}
	}

	private IEnumerator StartScenarioWithPresetCharacters(CCustomLevelData levelData)
	{
		ShowLoadingScreen();
		if (Choreographer.s_Choreographer != null)
		{
			yield return StartCoroutine(EndScenarioSafely());
		}
		SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
		Task<CCustomLevelData> copyTask = levelData.DeepCopySerializableObject<CCustomLevelData>();
		yield return new WaitForTask(copyTask);
		SaveData.Instance.Global.CurrentCustomLevelData = copyTask.Result;
		yield return YML.LoadSingleScenariosAsyncCrt(ScenarioManager.EDLLMode.Campaign);
		if (!YML.LastLoadResult)
		{
			HandleQuickLaunchError("Unable to load YML for DLL Mode: Campaign");
		}
		else
		{
			LoadCustomLevel(SaveData.Instance.Global.CurrentCustomLevelData);
		}
	}

	public IEnumerator LoadAbilityCards(IList<string> characterIds)
	{
		List<CAbilityCard> list = CharacterClassManager.AllAbilityCards.ToList();
		if (characterIds != null)
		{
			list = list.Where((CAbilityCard ac) => characterIds.Contains(ac.ClassID)).ToList();
		}
		Stopwatch sw = new Stopwatch();
		sw.Start();
		foreach (CAbilityCard item in list)
		{
			if (LoadAbilityCard(item))
			{
				if (sw.ElapsedMilliseconds > 66)
				{
					yield return null;
					sw.Restart();
				}
				continue;
			}
			yield break;
		}
	}

	public bool LoadAbilityCard(CAbilityCard abilityCard)
	{
		if (ObjectPool.HasAbilityCard(abilityCard.ID))
		{
			return true;
		}
		bool flag = false;
		AbilityCardUI abilityCardUI;
		GameObject newAbilityCard;
		if (false)
		{
			GameObject gameObject = PrefabLoaderCache.MiscCards.Load($"AbilityCard_{abilityCard.ID}");
			if (gameObject != null)
			{
				abilityCardUI = UnityEngine.Object.Instantiate(gameObject.GetComponent<AbilityCardUI>(), ObjectPool.instance.transform);
				newAbilityCard = abilityCardUI.gameObject;
				flag = true;
				ObjectPool.CreateCardPool(abilityCard.ID, ObjectPool.ECardType.Ability, newAbilityCard, 1);
			}
			else
			{
				Log.Warning($"Ability card 'AbilityCard_{abilityCard.ID}' was not found in card cache, will be generated runtime.");
			}
		}
		if (!flag)
		{
			try
			{
				flag = PersistentData.CreateAbilityCard(abilityCard.GetAbilityCardYML, out newAbilityCard, out abilityCardUI);
			}
			catch (Exception ex)
			{
				_cardsFailedLoading = true;
				Log.Exception(ex, "An exception occurred while loading Persistent Data");
				Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
				return false;
			}
			if (!flag)
			{
				_cardsFailedLoading = true;
				Log.Error("Failed to create ability card ID: " + abilityCard.ID + " CardInstanceID: " + abilityCard.CardInstanceID);
				Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
				return false;
			}
		}
		return true;
	}

	private void OnMainMenuLoadedToLoadPartyCallback()
	{
		MainMenuUIManager.Instance.ModeSelectionScreen.SetModeSuccessfullyLoadedCallback(OnModeSuccessfullyLoadedCallback);
		MainMenuUIManager.Instance.ModeSelectionScreen.ShowForLevelEditor();
	}

	private async void OnModeSuccessfullyLoadedCallback()
	{
		if (!string.IsNullOrWhiteSpace(MainMenuUIAutomation.SaveFile))
		{
			LoadCustomLevelDataService service = new LoadCustomLevelDataService(SaveData.Instance.Global.CurrentEditorLevelData);
			IGameSaveData gameSaveData = (await service.GetSaves()).FirstOrDefault((IGameSaveData s) => s.GameName == MainMenuUIAutomation.SaveFile);
			if (gameSaveData != null)
			{
				await service.LoadGame(gameSaveData);
				return;
			}
		}
		MainMenuUIManager.Instance.LevelEditorMainMenuScreen.OpenLoadPartyDialogForLevelData(SaveData.Instance.Global.CurrentEditorLevelData);
	}

	public void OnDestroy()
	{
		if (YML != null)
		{
			YML.Close();
		}
	}

	public IEnumerator ReloadGlobalCardAssets()
	{
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.LoadingWithProgress);
		ShowLoadingScreen();
		BudgetCoroutineRunner budgetCoroutineRunner = new BudgetCoroutineRunner("Init Global Data", PersistentData.s_Instance.InitGlobalData());
		yield return budgetCoroutineRunner.RunCoroutine();
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
		DisableLoadingScreen();
	}

	private IEnumerator StartAudio()
	{
		Dictionary<string, int> bossRoomsMap = null;
		while (!AudioController.IsMusicEnabled())
		{
			yield return null;
		}
		Dictionary<string, string[]> scenariosMusicMap = new Dictionary<string, string[]>
		{
			["AlgoxMusic"] = new string[21]
			{
				"FS_001", "FS_002", "FS_003", "FS_004", "FS_010", "FS_011", "FS_018", "FS_019", "FS_028", "FS_029",
				"FS_030", "FS_038", "FS_039", "FS_040", "FS_055", "FS_089", "FS_093", "FS_101", "FS_115", "FS_116",
				"FS_129"
			},
			["UnfetteredMusic"] = new string[24]
			{
				"FS_012", "FS_016", "FS_020", "FS_025", "FS_026", "FS_031", "FS_035", "FS_036", "FS_037", "FS_043",
				"FS_044", "FS_051", "FS_058", "FS_059", "FS_080", "FS_087", "FS_088", "FS_094", "FS_096", "FS_097",
				"FS_098", "FS_109", "FS_125", "FS_134"
			},
			["LurkerAbaelMusic"] = new string[22]
			{
				"FS_007", "FS_014", "FS_022", "FS_042", "FS_045", "FS_049", "FS_050", "FS_053", "FS_054", "FS_060",
				"FS_071", "FS_073", "FS_074", "FS_075", "FS_076", "FS_077", "FS_078", "FS_083", "FS_091", "FS_092",
				"FS_118", "FS_131"
			},
			["RadiantForestMusic"] = new string[6] { "FS_032", "FS_033", "FS_047", "FS_069", "FS_070", "FS_119" },
			["ChaosAncientMusic"] = new string[13]
			{
				"FS_005", "FS_009", "FS_015", "FS_017", "FS_023", "FS_024", "FS_027", "FS_034", "FS_041", "FS_056",
				"FS_067", "FS_068", "FS_095"
			},
			["IceLevelsMusic"] = new string[20]
			{
				"FS_006", "FS_013", "FS_021", "FS_046", "FS_048", "FS_052", "FS_055", "FS_057", "FS_065", "FS_072",
				"FS_079", "FS_082", "FS_085", "FS_086", "FS_090", "FS_108", "FS_111", "FS_114", "FS_122", "FS_126"
			},
			["FinalArcMusic"] = new string[5] { "FS_008", "FS_061", "FS_062", "FS_063", "FS_064" }
		};
		string[] _musicPlaylists = new string[4]
		{
			GlobalSettings.DefInstance.m_MainMenuMusicPlaylist,
			GlobalSettings.DefInstance.m_OutpostWinterMusicPlaylist,
			GlobalSettings.DefInstance.m_OutpostSummerMusicPlaylist,
			GlobalSettings.DefInstance.m_QuestBriefingMusicPlaylist
		};
		int currentMusicIndex = -1;
		float currentMusicHalfMute = 1f;
		string scenarioMusicName = null;
		while (true)
		{
			if (scenarioMusicName != null && IsLoading)
			{
				scenarioMusicName = null;
				currentMusicIndex = -1;
			}
			int num = -1;
			if (MainMenuUIManager.Instance != null && MainMenuUIManager.Instance.isActiveAndEnabled)
			{
				num = 0;
			}
			else if (Singleton<FHMapChoreographer>.InstanceFast != null && Singleton<FHMapChoreographer>.InstanceFast.View != null)
			{
				num = currentMusicIndex;
				FHMapViewState currentState = Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState;
				bool flag = currentState != null && currentState.Category == FHMapViewState.StateCategory.WorldMap;
				FHMapViewState currentState2 = Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState;
				bool flag2 = currentState2 != null && currentState2.Category == FHMapViewState.StateCategory.Outpost;
				FHMapViewState currentState3 = Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState;
				bool flag3 = currentState3 != null && currentState3.Category == FHMapViewState.StateCategory.Briefing;
				FHMapViewState currentState4 = Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState;
				bool flag4 = currentState4 != null && currentState4.Category == FHMapViewState.StateCategory.Narration;
				bool flag5 = Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState != null && Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState is EventViewState;
				if ((bool)_currentMusic)
				{
					float num2 = 0.01f * (float)SaveData.Instance.Global.MasterVolume;
					currentMusicHalfMute = Mathf.Lerp(currentMusicHalfMute, 0.01f * (float)SaveData.Instance.Global.MusicVolume * (flag5 ? 0.36f : 1f), 3f * Time.deltaTime);
					_currentMusic.volume = currentMusicHalfMute * num2;
				}
				if (flag | flag2)
				{
					num = ((!Singleton<FHMapChoreographer>.InstanceFast.GameState.Outpost.Calendar.IsSummer) ? 1 : 2);
					if (!(_outpostAmbient == null))
					{
					}
				}
				else if (flag3)
				{
					num = 3;
				}
				else if (flag4)
				{
					num = -2;
				}
			}
			else if (Choreographer.s_Choreographer != null)
			{
				if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
				{
					yield return new WaitUntil(() => TransitionManager.s_Instance == null || TransitionManager.s_Instance.TransitionDone);
				}
				if (bossRoomsMap == null)
				{
					bossRoomsMap = new Dictionary<string, int>();
					GlobalSettings.BossRoomDef[] bossRooms = GlobalSettings.DefInstance.BossRooms;
					for (int num3 = 0; num3 < bossRooms.Length; num3++)
					{
						GlobalSettings.BossRoomDef bossRoomDef = bossRooms[num3];
						bossRoomsMap[bossRoomDef.LevelId] = bossRoomDef.RoomIndex;
					}
				}
				if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.ScenarioEnded)
				{
					num = -2;
				}
				else
				{
					num = 4;
					if (ScenarioManager.CurrentScenarioState != null && bossRoomsMap.ContainsKey(ScenarioManager.CurrentScenarioState.BakingID) && ScenarioManager.CurrentScenarioState.Maps[bossRoomsMap[ScenarioManager.CurrentScenarioState.BakingID]].Revealed)
					{
						num = 5;
					}
				}
			}
			if (num != currentMusicIndex && !IsLoading)
			{
				if ((currentMusicIndex != 1 && currentMusicIndex != 2) || num != 1)
				{
				}
				if (num > -1)
				{
					if ((bool)_currentMusic)
					{
						_currentMusic.StopPlaylist(1f);
					}
					if (num == 4 || num == 5)
					{
						if (ScenarioManager.CurrentScenarioState != null)
						{
							if (scenarioMusicName == null)
							{
								scenarioMusicName = "ScenarioMusic";
								foreach (KeyValuePair<string, string[]> item in scenariosMusicMap)
								{
									if (item.Value.Any((string levelIndexStr) => ScenarioManager.CurrentScenarioState.BakingID.StartsWith(levelIndexStr)))
									{
										scenarioMusicName = item.Key;
										break;
									}
								}
							}
							string text = scenarioMusicName;
							if (num == 5)
							{
								text += "_Boss";
							}
							_currentMusic = AudioController.PlayMusicPlaylist(text);
						}
					}
					else
					{
						if (num == 6)
						{
							_currentMusic = AudioController.PlayMusicPlaylist("ScenarioMusic");
						}
						else
						{
							_currentMusic = AudioController.PlayMusicPlaylist(_musicPlaylists[num]);
						}
						if ((bool)_currentMusic)
						{
							_currentMusic.HandleNextPlaylistTrack((AudioObject o) =>
							{
								_currentMusic = o;
							});
						}
					}
					if ((bool)_currentMusic)
					{
						_currentMusic.HandleNextPlaylistTrack((AudioObject o) =>
						{
							_currentMusic = o;
						});
					}
				}
				else if (num == -2 && (bool)_currentMusic)
				{
					_currentMusic.StopPlaylist(1f);
				}
				currentMusicIndex = num;
			}
			yield return null;
		}
	}

	public IEnumerator InitialiseGloomhavenCoroutine(string applicationPersistenDataPath, string rulebaseDataRoot)
	{
		try
		{
			SharedClient.Initialise(applicationPersistenDataPath, rulebaseDataRoot);
			ScenarioRuleClient.Initialise();
			MapRuleLibraryClient.Instance.Initialise();
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while initialising the DLLs\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
		if ((bool)LoadingScreenInstance)
		{
			LoadingScreenInstance.UpdateProgressBar(6f);
		}
		yield return null;
		while (!PlatformLayer.Instance.NewPlatformLayer.IsAvailableAndReady)
		{
			yield return null;
		}
		yield return YML.InitAsyncCrt();
		if (!YML.InitResult)
		{
			Log.Error("Failed to Initialise YMLLoading class!");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		if ((bool)LoadingScreenInstance)
		{
			LoadingScreenInstance.UpdateProgressBar(7f);
			yield return null;
		}
		IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-SceneController.InitialiseGloomhavenCoroutine() => YML.LoadAll()", () =>
		{
			YML.LoadAll();
		});
		yield return threadHandle.WaitForCompletionCrt();
		if ((bool)LoadingScreenInstance)
		{
			LoadingScreenInstance.UpdateProgressBar(10f);
			yield return null;
		}
		if (!Instance.YML.LastLoadResult)
		{
			Log.Error("Unable to load All YML");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		try
		{
			ScenarioRuleClient.LoadData();
		}
		catch (Exception ex2)
		{
			Log.Exception(ex2, "An exception occurred while initialising the DLLs\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
		}
		if (YMLDefRepository.Repo.DefRepository == null)
		{
			yield return YMLDefRepository.Repo.GenerateDefRepositoryCrt();
		}
		if ((bool)LoadingScreenInstance)
		{
			LoadingScreenInstance.UpdateProgressBar(12f);
		}
	}

	public void UnloadInitialLoadingScreen()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).name == "InitialLoadingScreen")
			{
				SceneManager.UnloadSceneAsync("InitialLoadingScreen");
				break;
			}
		}
	}

	public void ExitApplication()
	{
		Application.Quit();
	}

	private IEnumerator LoadSceneCoroutine(string sceneName)
	{
		using (new MiniProfilerScope("SceneController.LoadSceneCoroutine"))
		{
			ShowLoadingScreen();
			ObjectPool.RecyclePendingObjects();
			if (currentlyLoadedScene.IsValid() && currentlyLoadedScene.isLoaded)
			{
				if (Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.m_ProcGenScene.IsValid() && Choreographer.s_Choreographer.m_ProcGenScene.isLoaded)
				{
					try
					{
						SceneManager.UnloadSceneAsync(Choreographer.s_Choreographer.m_ProcGenScene);
					}
					catch (Exception ex)
					{
						Log.Exception(ex, "An exception occurred while unloading the ProcGen scene\n");
						GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
						yield break;
					}
				}
				MainSceneCamera.SetActive(value: true);
				AsyncOperation asyncUnload;
				try
				{
					asyncUnload = SceneManager.UnloadSceneAsync(currentlyLoadedScene);
				}
				catch (Exception ex2)
				{
					Log.Exception(ex2, "An exception occurred while unloading the current scene\n");
					GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
					yield break;
				}
				WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
				while (!asyncUnload.isDone)
				{
					yield return waitForEndOfFrameDelay;
				}
			}
			if (Application.isPlaying)
			{
				yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			}
			else
			{
				yield return null;
			}
			if (MainSceneCamera.activeInHierarchy && PersistentData.s_Instance.IsDataLoaded && sceneName != "EmptyTransitionScene")
			{
				MainSceneCamera.SetActive(value: false);
			}
			currentlyLoadedScene = SceneManager.GetSceneByName(sceneName);
			if (SharedClient.CriticalYMLFailure != string.Empty)
			{
				GlobalErrorMessage.ShowMessageDefaultTitle(SharedClient.CriticalYMLFailure, "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			}
			else if (ReloadMainMenu)
			{
				ReloadMainMenu = false;
				LoadMainMenu();
			}
			else
			{
				ObjectPool.instance.IsRestarting = false;
			}
		}
	}

	public void ShowLoadingScreen()
	{
		if (!IsLoading)
		{
			IsLoading = true;
			LoadingProfiler.Stopwatch = new PenumbraStopwatch("Loading");
			MiniProfiler instance = MiniProfiler.Instance;
			instance.onBeginScope = (MiniProfiler.OnBeginScopeFunc)Delegate.Combine(instance.onBeginScope, new MiniProfiler.OnBeginScopeFunc(OnBeginScope));
			MiniProfiler instance2 = MiniProfiler.Instance;
			instance2.onEndScope = (MiniProfiler.OnEndScopeFunc)Delegate.Combine(instance2.onEndScope, new MiniProfiler.OnEndScopeFunc(OnEndScope));
			MiniProfiler.Instance.BeginScope("Loading Screen");
			Singleton<InputFHManager>.Instance.InputContext.PushOrSetActionsMask(this, Array.Empty<InputAction>());
			Log.Info("Showing loading screen.");
			try
			{
				ReproStepsLogger.LogEvent("LoadingScreen", "Shown");
				LoadingScreenInstance.gameObject.SetActive(value: true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "An exception occurred while showing loading screen\n");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			}
		}
	}

	public void DisableLoadingScreen()
	{
		if (IsLoading && !YML.IsUnloading)
		{
			MainMenuUIManager instance = MainMenuUIManager.Instance;
			if ((object)instance == null || !instance.ModeSelectionScreen.BusyLoading)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Resources.UnloadUnusedAssets();
				_ = EnvironmentLoader.Instance != null;
				MiniProfiler.Instance.EndScope("Loading Screen");
				MiniProfiler instance2 = MiniProfiler.Instance;
				instance2.onBeginScope = (MiniProfiler.OnBeginScopeFunc)Delegate.Remove(instance2.onBeginScope, new MiniProfiler.OnBeginScopeFunc(OnBeginScope));
				MiniProfiler instance3 = MiniProfiler.Instance;
				instance3.onEndScope = (MiniProfiler.OnEndScopeFunc)Delegate.Remove(instance3.onEndScope, new MiniProfiler.OnEndScopeFunc(OnEndScope));
				Singleton<InputFHManager>.Instance.InputContext.PopActionsMask(this);
				IsLoading = false;
				DumpLoadTimes();
				LoadingProfiler.Stopwatch?.Clear();
				LoadingProfiler.Stopwatch?.Dispose();
				LoadingProfiler.Stopwatch = null;
				Log.Info("Disabling loading screen.");
				LoadingScreenInstance.gameObject.SetActive(value: false);
				ReproStepsLogger.LogEvent("LoadingScreen", "Hidden");
			}
		}
		ProcessingUI.Show(show: false);
	}

	private void OnBeginScope(string name)
	{
		LoadingProfiler.Stopwatch.Time(SBeg + name);
	}

	private void OnEndScope(string name, long elapsedMS)
	{
		LoadingProfiler.Stopwatch.Time($"{SEnd}{name}    [{elapsedMS / 1000}.{elapsedMS % 1000:D03}s]");
	}

	public void DumpLoadTimes()
	{
		if (LoadingProfiler.Stopwatch == null)
		{
			return;
		}
		PenubmraStopwatchTiming[] timings = LoadingProfiler.Stopwatch.GetTimings();
		if (timings.Length == 0)
		{
			return;
		}
		TimeSpan time = timings[0].Time;
		TimeSpan timeSpan = time;
		TimeSpan time2 = timings[^1].Time;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Total Loading time: {time2:mm\\:ss\\.fff}");
		for (int i = 0; i < timings.Length; i++)
		{
			PenubmraStopwatchTiming penubmraStopwatchTiming = timings[i];
			if (i + 1 < timings.Length && penubmraStopwatchTiming.Name.StartsWith(SBeg))
			{
				ReadOnlySpan<char> readOnlySpan = penubmraStopwatchTiming.Name;
				ReadOnlySpan<char> other = readOnlySpan.Slice(SBeg.Length);
				PenubmraStopwatchTiming penubmraStopwatchTiming2 = timings[i + 1];
				if (penubmraStopwatchTiming2.Name.StartsWith(SEnd) && penubmraStopwatchTiming2.Name.Length >= SEnd.Length + other.Length)
				{
					readOnlySpan = penubmraStopwatchTiming2.Name;
					if (readOnlySpan.Slice(SEnd.Length, other.Length).SequenceEqual(other))
					{
						stringBuilder.AppendLine($"[{penubmraStopwatchTiming.Time - time:mm\\:ss\\.fff}][+{penubmraStopwatchTiming.Time - timeSpan:ss\\.fff}] {other.ToString()}{penubmraStopwatchTiming2.Name.Substring(SEnd.Length + other.Length)}");
						timeSpan = penubmraStopwatchTiming2.Time;
						i++;
						continue;
					}
				}
			}
			stringBuilder.AppendLine($"[{penubmraStopwatchTiming.Time - time:mm\\:ss\\.fff}][+{penubmraStopwatchTiming.Time - timeSpan:ss\\.fff}] {penubmraStopwatchTiming.Name}");
			timeSpan = penubmraStopwatchTiming.Time;
		}
		Log.Info(stringBuilder.ToString());
	}

	public void LoadMainMenu()
	{
		StartCoroutine(LoadMainMenuCrt(null));
	}

	public void LoadMainMenu(string joinSessionID)
	{
		StartCoroutine(LoadMainMenuCrt(joinSessionID));
	}

	private IEnumerator LoadMainMenuCrt(string joinSessionID)
	{
		try
		{
			ScenarioManager.Unload();
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			Timekeeper.instance.m_GlobalClock.localTimeScale = 1f;
			ScenarioRuleClient.ToggleMessageProcessing(process: false);
			ObjectPool.RecyclePendingObjects();
			if (AutoTestController.s_Instance != null && AutoTestController.s_Instance.CurrentOverlay != null)
			{
				UnityEngine.Object.Destroy(AutoTestController.s_Instance.CurrentOverlay.gameObject);
			}
			ScenarioManager.Mode = ScenarioManager.EDLLMode.None;
			Log.SimpleInfo("User exit to Main Menu");
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while loading main menu\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00006", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
		if (SaveData.Instance.Global.GameMode != EGameMode.MainMenu)
		{
			SaveData.Instance.Global.GameMode = EGameMode.MainMenu;
		}
		try
		{
			GameLoadedAndClientReady = false;
			GameLoadedAndHostReady = false;
			CampaignStatRepository.ResetInstance();
			ScenarioStatRepository.ResetInstance();
			ControllableRegistry.Reset();
			PlayerRegistry.Reset();
			FFSNetwork.Shutdown(FFSNetwork.IsHost ? new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession) : null);
			if (!AutoTestController.IsBulkEvaluating() && ObjectPool.instance != null && !ObjectPool.KeepMonsterCardsLoaded)
			{
				ObjectPool.ClearAllMonsterCards();
			}
			SaveData.Instance.Global.CurrentAdventureData = null;
			StartCoroutine(LoadMainMenuCoroutine(joinSessionID));
		}
		catch (Exception ex2)
		{
			Log.Exception(ex2, "An exception occurred while loading main menu\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00006", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
		}
		yield break;
	}

	private IEnumerator LoadMainMenuCoroutine(string joinSessionID = null)
	{
		yield return new WaitWhile(() => FFSNetwork.IsShuttingDown);
		yield return LoadSceneCoroutine("MainMenu");
		AdventureState.End();
		AssetBundleManager.Instance.UnloadNotRequiredBundles();
		yield return null;
		while (true)
		{
			MainMenuUIManager instance = MainMenuUIManager.Instance;
			if ((object)instance != null && instance.Initialised)
			{
				break;
			}
			yield return null;
		}
		if (!SaveData.Instance.Global.IntroCinematicSeen)
		{
			MainMenuUIManager.Instance.ShowIntroCinematic();
		}
		DisableLoadingScreen();
		while (MainMenuUIManager.Instance.IntroCinematicIsPlaying)
		{
			yield return null;
		}
		if (!TelemetryAndEulaPolicy.IsEulaAccepted)
		{
			yield return new WaitForSecondsRealtime(0.5f);
		}
		while (!TelemetryAndEulaPolicy.IsEulaAccepted)
		{
			TelemetryAndEulaPolicyUI.ShowTelemetryAndEulaPolicy();
			yield return null;
		}
		PlatformLayer.Instance.NewPlatformLayer.CheckPendingSessionAndJoin();
		if (!string.IsNullOrEmpty(joinSessionID))
		{
			UnityEngine.Object.FindObjectOfType<UIMultiplayerJoinSessionWindow>(includeInactive: true).gameObject.SetActive(value: true);
			yield return null;
			UIMultiplayerJoinSessionWindow.s_This.inviteCodeInput.text = joinSessionID;
			UIMultiplayerJoinSessionWindow.s_This.JoinSession(joinSessionID);
		}
	}

	public void OnFinishedLoadToFrontEndTutorialMenu()
	{
		GameObject gameObject = GameObject.Find("Tutorial");
		if (gameObject != null)
		{
			gameObject.GetComponentInChildren<UIMenuOption>().Select();
		}
	}

	public IEnumerator OnStartScenarioCallback(AWLaunchMethod launchMethod, bool restart = false)
	{
		using (new MiniProfilerScope("SceneController.OnStartScenarioCallback"))
		{
			if (SaveData.Instance.Global.GameMode == EGameMode.MainMenu)
			{
				yield break;
			}
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				IEnumerator enumerator = null;
				try
				{
					enumerator = Instance.LoadScenarioAssetBundlesAsync(null, AdventureState.MapState);
				}
				catch (Exception ex)
				{
					HandleException(ex);
				}
				yield return enumerator;
				try
				{
					if (launchMethod == AWLaunchMethod.resume)
					{
						StatsDataTracker.Instance.m_IsCampaignModeRunning = true;
					}
				}
				catch (Exception ex2)
				{
					HandleException(ex2);
				}
				try
				{
					foreach (Tuple<string, CBattleGoalState> allActiveBattleGoal in SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.AllActiveBattleGoals)
					{
						allActiveBattleGoal.Item2.BattleGoalConditionState?.Init(allActiveBattleGoal.Item2.BattleGoal);
					}
				}
				catch (Exception ex3)
				{
					HandleException(ex3);
				}
				Log.SimpleInfo((launchMethod == AWLaunchMethod.resume) ? "User resumed scenario" : "User started scenario");
				Choreographer.s_Choreographer.Play(restart);
			}
			else
			{
				yield return OnCustomLevelLoadedCallback(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario);
			}
		}
		void HandleException(Exception ex4)
		{
			Log.Exception(ex4, "An exception occurred while starting scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex4.StackTrace, LoadMainMenu, ex4.Message);
		}
	}

	public void RestartScenario(RestartRoundMode mode = RestartRoundMode.Default, bool fromNetwork = false)
	{
		switch (mode)
		{
		case RestartRoundMode.Default:
		case RestartRoundMode.FullRestart:
		case RestartRoundMode.FastRestart:
			Choreographer.s_Choreographer.IsRestarting = true;
			StartCoroutine(RestartScenarioCoroutine(mode, fromNetwork));
			break;
		case RestartRoundMode.FastUndo:
			FastReload.Undo();
			break;
		case RestartRoundMode.FastRedo:
			FastReload.Redo();
			break;
		default:
			Log.Error($"Unknown mode passed to SceneController.RestartScenario: {mode}");
			break;
		}
	}

	public void RestartScenarioFromInitial()
	{
		ScenarioManager.CurrentScenarioState.ScenarioSetup.HasBeenRestarted = true;
		Choreographer.s_Choreographer.IsRestarting = true;
		Choreographer.s_Choreographer.EndGame(EResult.Restart);
		StartCoroutine(OncampaignSceneLoadedAfterRestartCoroutine());
	}

	private IEnumerator OncampaignSceneLoadedAfterRestartCoroutine()
	{
		do
		{
			yield return null;
		}
		while (IsLoading);
		Singleton<FHMapChoreographer>.InstanceFast.GameClient.GameState.CampaignState.ActiveScenario.HasEnteredScenario = false;
		FHMapView view = Singleton<FHMapChoreographer>.InstanceFast.View;
		CharacterState characterState = view.PlayerCharacters.FirstOrDefault();
		if (characterState != null)
		{
			view.SetSelectedCharacter(characterState, -1);
		}
		view.SetState(new BattleGoalSelectionViewState(isFromSummary: false));
	}

	public void RegenerateAndRestartScenario(Action onFinish = null, bool fullRestart = true)
	{
		Choreographer.s_Choreographer.IsRestarting = true;
		if (Choreographer.IsInNoPartyCampaign())
		{
			fullRestart = false;
		}
		StartCoroutine(RegenerateAndRestartScenarioCoroutine(onFinish, fullRestart));
	}

	private IEnumerator RestartScenarioCoroutine(RestartRoundMode mode, bool fromNetwork)
	{
		bool flag = false;
		switch (mode)
		{
		case RestartRoundMode.Default:
			flag = FastReload.FastRestartRoundEnabled;
			break;
		case RestartRoundMode.FullRestart:
			flag = false;
			break;
		case RestartRoundMode.FastRestart:
			flag = true;
			break;
		}
		if (FFSNetwork.IsOnline && !fromNetwork)
		{
			RestartRoundMode dataInt = ((!flag) ? RestartRoundMode.FullRestart : RestartRoundMode.FastRestart);
			if (Singleton<UIResultsManager>.Instance.IsShown)
			{
				Synchronizer.SendSideAction(GameActionType.RestartRound, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)dataInt);
			}
			else
			{
				Synchronizer.SendSideAction(GameActionType.RestartRoundMessage, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)dataInt);
			}
		}
		if (flag)
		{
			yield return new WaitForTask(FastReload.RestartRound());
			yield break;
		}
		ObjectPool.instance.IsRestarting = true;
		FastReload.OnRestartRound(isFullRestart: true);
		ShowLoadingScreen();
		yield return EndScenarioSafely();
		ActionProcessor.OnRestartRound();
		ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.StartOfRound);
		Choreographer.s_Choreographer.IsRestarting = false;
		yield return LoadSceneCoroutine("Game");
		yield return OnStartScenarioCallback(AWLaunchMethod.resume);
	}

	private IEnumerator RegenerateAndRestartScenarioCoroutine(Action onFinish = null, bool fullRestart = true)
	{
		ShowLoadingScreen();
		yield return EndScenarioSafely();
		Choreographer.s_Choreographer.IsRestarting = false;
		FHScenarioSetup fHScenarioSetup = AdventureState.MapState.CurrentMapScenarioState?.InitialState?.ScenarioSetup;
		fHScenarioSetup.HasBeenRestarted = true;
		if (fullRestart)
		{
			Choreographer.s_Choreographer.EndGame(EResult.Restart);
			AdventureState.MapState.CurrentMapScenarioState = null;
			yield return OncampaignSceneLoadedAfterRestartCoroutine();
		}
		else
		{
			AdventureState.MapState.CurrentMapScenarioState.RegenerateMapScenario();
			AdventureState.MapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
			AdventureState.MapState.EnterScenario(fHScenarioSetup);
			yield return LoadSceneCoroutine("Game");
			yield return OnStartScenarioCallback(AWLaunchMethod.resume, fullRestart);
		}
		onFinish?.Invoke();
	}

	public IEnumerator EndScenarioSafely()
	{
		Log.SimpleInfo("Entered EndScenarioSafely coroutine");
		if (ScenarioManager.CurrentScenarioState != null)
		{
			ScenarioManager.CurrentScenarioState.ScenarioEnded = true;
		}
		DesyncDetector.Reset();
		if (Choreographer.s_Choreographer == null)
		{
			Log.SimpleInfo("Exited EndScenarioSafely coroutine early as Choreographer instance was null");
			yield break;
		}
		if (GameState.ThreadIsSleeping || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse)
		{
			GameState.s_ThreadAboutToAbort = true;
		}
		yield return new WaitForSeconds(0.5f);
		if (ScenarioRuleClient.IsMessageProcessingBlocked)
		{
			ScenarioRuleClient.ToggleMessageProcessing(process: true);
		}
		Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
		Log.SimpleInfo("EndScenarioSafely coroutine about to pause until pathfinder is not locked + choreographer and SRL are done processing messages");
		WaitForSeconds waitForSecondsDelay = new WaitForSeconds(0.5f);
		float timeSinceLastDebugLog = 0f;
		while (CPathFinder.Locked || Choreographer.s_Choreographer.m_MessageQueue.Count > 0 || ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.s_ThreadAboutToAbort || (FFSNetwork.IsOnline && ActionProcessor.IsProcessing))
		{
			while (GameState.ThreadIsSleeping || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse)
			{
				GameState.s_ThreadAboutToAbort = true;
				yield return null;
			}
			timeSinceLastDebugLog += 0.5f;
			if (timeSinceLastDebugLog >= 3f)
			{
				timeSinceLastDebugLog = 0f;
				Log.SimpleInfo("EndScenarioSafely coroutine softlock reporting: \nPathfinder is locked: " + CPathFinder.Locked + "\nChoreographer Message Queue Count: " + Choreographer.s_Choreographer.m_MessageQueue.Count + "\nScenarioRuleClient Is Processing or Messages Queued: " + ScenarioRuleClient.IsProcessingOrMessagesQueued + "\nGameState Thread About to Abort: " + GameState.s_ThreadAboutToAbort + "\nDeath Dissolves in Progress Count: " + DeathDissolve.s_DeathDissolvesInProgress.Count);
			}
			if (AutoTestController.s_AutoTestCurrentlyLoaded)
			{
				AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
			}
			yield return waitForSecondsDelay;
		}
		Log.SimpleInfo("EndScenarioSafely coroutine about to pause until player is done selecting damage response if needed");
		while (GameState.ThreadIsSleeping || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse)
		{
			GameState.s_ThreadAboutToAbort = true;
			yield return null;
		}
		ScenarioRuleClient.Stop();
		Log.SimpleInfo("Exited EndScenarioSafely coroutine");
	}

	public void NewCampaignStart(string partyName, EAdventureDifficulty difficulty, ERulesFlag rules, string campaignID = "Frosthaven")
	{
		StartCoroutine(NewCampaignStartCoroutine(partyName, difficulty, rules, campaignID));
	}

	private IEnumerator NewCampaignStartCoroutine(string partyName, EAdventureDifficulty difficulty, ERulesFlag rules, string campaignID)
	{
		ShowLoadingScreen();
		yield return null;
		bool isTutorial = false;
		if (partyName == "Tutorial")
		{
			isTutorial = true;
			Task deleteOldTutorialTask = SaveData.Instance.DeleteCampaign(partyName);
			yield return new WaitForTask(deleteOldTutorialTask);
			if (deleteOldTutorialTask.HasFailed(out var exception))
			{
				ErrorMessage(exception);
			}
		}
		if (!Instance.YML.LastLoadResult)
		{
			Log.Error("Unable to load Campaign YML");
			Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, Instance.LoadMainMenu);
			yield break;
		}
		PersistentData.s_Instance.FailedLoading = false;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Log.Error("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		Task<List<PartyAdventureData>> task = SaveData.Instance.Global.LoadSavesFromDisk();
		yield return new WaitForTask(task);
		Task startTask = SaveData.Instance.StartNewCampaignMode(partyName, difficulty, rules, campaignID);
		yield return new WaitForTask(startTask);
		if (startTask.HasFailed(out var exception2))
		{
			ErrorMessage(exception2);
		}
		try
		{
			AdventureState.StartAdventure(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState);
			LoadAdventureAssetBundles();
		}
		catch (Exception ex)
		{
			ErrorMessage(ex);
		}
		if (!isTutorial)
		{
			Task deleteOldTutorialTask = SaveData.Instance.AutoSaveCurrentAdventureData();
			yield return new WaitForTask(deleteOldTutorialTask);
			if (deleteOldTutorialTask.HasFailed(out exception2))
			{
				ErrorMessage(exception2);
			}
		}
		string sceneName;
		IEnumerator afterSceneLoaded;
		if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.IsInScenarioPhase)
		{
			CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState;
			if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
				SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
			if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
			{
				AdventureState.MapState.EnterScenario();
			}
			sceneName = "Game";
			afterSceneLoaded = OnCampaignScenarioLoadedCoroutine(AWLaunchMethod.create);
		}
		else
		{
			sceneName = FHCampaignMapScene;
			afterSceneLoaded = OnCampaignMapLoadedCoroutine();
		}
		Coroutine coroutine = StartCoroutine(LoadSceneCoroutine(sceneName));
		StatsDataTracker.Instance.TrackRunSessionID(SaveData.Instance.Global.CurrentAdventureData.RunSessionID);
		AnalyticsWrapper.LogRunStart(AWGameMode.campaign, AWLaunchMethod.create, SaveData.Instance.Global.CurrentAdventureData, PlayerRegistry.AllPlayers.Count > 1);
		Log.SimpleInfo("User started new Campaign adventure");
		yield return coroutine;
		yield return StartCoroutine(afterSceneLoaded);
		void ErrorMessage(Exception ex2)
		{
			Log.Exception(ex2, "An exception occurred while starting the campaign map\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, LoadMainMenu, ex2.Message);
		}
	}

	public void CampaignResume(PartyAdventureData partyData, bool isJoiningMPClient, bool newLoad = true)
	{
		StartCoroutine(CampaignResumeCoroutine(partyData, isJoiningMPClient, newLoad));
	}

	private IEnumerator CampaignResumeCoroutine(PartyAdventureData partyData, bool isJoiningMPClient, bool newLoad = true)
	{
		using (new MiniProfilerScope("SceneController.CampaignResumeCoroutine"))
		{
			if ((bool)Singleton<FHMapChoreographer>.InstanceFast)
			{
				yield return new WaitForTask(Singleton<FHMapChoreographer>.InstanceFast.EndGame());
			}
			ShowLoadingScreen();
			yield return null;
			if ((bool)Choreographer.s_Choreographer)
			{
				yield return EndScenarioSafely();
				yield return UnityGameEditorRuntime.UnloadScenario();
			}
			if (!FFSNetwork.IsOnline)
			{
				GameLoadedAndClientReady = false;
				GameLoadedAndHostReady = false;
				CampaignStatRepository.ResetInstance();
				ScenarioStatRepository.ResetInstance();
				ControllableRegistry.Reset();
				PlayerRegistry.Reset();
				ScenarioRuleClient.Reset();
			}
			if (!PlayerRegistry.Initialized)
			{
				yield return PlayerRegistry.InitializeCrt();
			}
			PlayerRegistry.LoadingInFromJoiningClient = isJoiningMPClient;
			yield return LoadSceneCoroutine("EmptyTransitionScene");
			AssetBundleManager.Instance.UnloadNotRequiredBundles();
			PersistentData.s_Instance.FailedLoading = false;
			yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
			if (PersistentData.s_Instance.FailedLoading)
			{
				Log.Error("Unable to load Monster Card YML\n");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
				yield break;
			}
			AWLaunchMethod launchMethod = AWLaunchMethod.resume;
			if (SaveData.Instance.Global.CurrentAdventureData != null)
			{
				Task task = SaveData.Instance.ResumeNewCampaignMode(isJoiningMPClient);
				yield return new WaitForTask(task);
				LoadAdventureAssetBundles();
				AdventureState.StartAdventure(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState);
				UnityGameEditorRuntime.s_SkipNextSelectCardSyncCheck = false;
				if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.IsInScenarioPhase)
				{
					bool flag = false;
					try
					{
						if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState != null)
						{
							SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
							CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState;
							if (currentMapScenarioState.MatchSessionID == null && (!FFSNetwork.IsOnline || FFSNetwork.IsHost))
							{
								AdventureState.MapState.EnterScenario();
							}
							if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState != null)
							{
								if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
								{
									SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
									SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
								}
								if (FFSNetwork.IsOnline && !FFSNetwork.IsHost)
								{
									launchMethod = AWLaunchMethod.join;
								}
							}
							else
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
					catch (Exception ex)
					{
						HandleException(ex);
					}
					if (flag)
					{
						Log.Error("Unable to resume Adventure game. CurrentMapScenarioState is null");
						GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00022", "GUI_ERROR_RETURN_TO_MAP_BUTTON", Environment.StackTrace, () =>
						{
							StartCoroutine(ReturnToMapAction());
						});
					}
					else
					{
						UnityGameEditorRuntime.s_SkipNextSelectCardSyncCheck = FFSNetwork.IsOnline && FFSNetwork.IsClient;
						yield return StartCoroutine(LoadSceneCoroutine("Game"));
						yield return StartCoroutine(OnCampaignScenarioLoadedCoroutine(launchMethod));
					}
				}
				else
				{
					yield return StartCoroutine(LoadSceneCoroutine(FHCampaignMapScene));
					yield return OnCampaignMapLoadedCoroutine(isJoiningMPClient);
				}
			}
			else
			{
				if (isJoiningMPClient && !FFSNetwork.IsOnline)
				{
					yield break;
				}
				Log.Error("Unable to resume Campaign game.  Save data is null.");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, MoveGameStateFileToCorruptedSaveFolder);
			}
			if ((SaveData.Instance.Global.m_StatsDataStorage.m_RunSessionID == null) & newLoad)
			{
				if (isJoiningMPClient)
				{
					launchMethod = AWLaunchMethod.join;
				}
				AnalyticsWrapper.LogRunStart(AWGameMode.campaign, launchMethod, SaveData.Instance.Global.CurrentAdventureData, PlayerRegistry.AllPlayers.Count > 1);
				Log.SimpleInfo((launchMethod == AWLaunchMethod.join) ? "User joined a Campaign adventure" : "User resumed a Campaign adventure");
			}
		}
		void HandleException(Exception ex2)
		{
			Log.Exception(ex2, "Unable to resume Adventure game\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, MoveGameStateFileToCorruptedSaveFolder, ex2.Message);
		}
		async void MoveGameStateFileToCorruptedSaveFolder()
		{
			await SaveData.Instance.Global.CurrentAdventureData.MoveGameStateFileToCorruptedSaveFolder(EGameMode.Campaign);
			LoadMainMenu();
		}
		IEnumerator ReturnToMapAction()
		{
			Task task2 = SaveData.Instance.AutoSaveCurrentAdventureData();
			yield return new WaitForTask(task2);
			IEnumerator enumerator = null;
			try
			{
				enumerator = LoadSceneCoroutine(FHCampaignMapScene);
			}
			catch (Exception ex2)
			{
				HandleException(ex2);
			}
			yield return enumerator;
			yield return OnCampaignMapLoadedCoroutine(isJoiningMPClient);
		}
	}

	private IEnumerator OnCampaignMapLoadedCoroutine(bool isMPClientJoining = false)
	{
		Log.Info("OnCampaignMapLoadedCoroutine");
		yield return new WaitWhile(() => AssetBundleManager.Instance.ScenarioBundlesLoading);
		try
		{
			AdventureState.MapState.OnMapLoaded(isMPClientJoining);
		}
		catch (Exception ex)
		{
			HandleException(ex);
		}
		yield return new WaitWhile(() => MapRuleLibraryClient.Instance.MessageQueueLength > 0);
		try
		{
			StatsDataTracker.Instance.m_IsCampaignModeRunning = true;
		}
		catch (Exception ex2)
		{
			HandleException(ex2);
		}
		void HandleException(Exception ex3)
		{
			Log.Exception(ex3, "Unable to load Campaign Map\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, LoadMainMenu, ex3.Message);
		}
	}

	private void OnCampaignScenarioLoaded(AWLaunchMethod launchMethod)
	{
	}

	private IEnumerator OnCampaignScenarioLoadedCoroutine(AWLaunchMethod launchMethod)
	{
		using (new MiniProfilerScope("SceneController.OnCampaignScenarioLoadedCoroutine"))
		{
			Log.Info("OnCampaignScenarioLoadedCoroutine");
			yield return new WaitWhile(() => MapRuleLibraryClient.Instance.MessageQueueLength > 0 || AdventureState.MapState.AdventureStateStarting);
			StatsDataTracker.Instance.m_IsCampaignModeRunning = true;
			yield return OnStartScenarioCallback(launchMethod);
		}
	}

	public void CampaignRestart()
	{
		StartCoroutine(CampaignRestartCoroutine());
	}

	private IEnumerator CampaignRestartCoroutine()
	{
		try
		{
			SaveData.Instance.Global.CurrentAdventureData.RetryScenario();
			if (SaveData.Instance.Global.CurrentAdventureData == null || !SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.IsInScenarioPhase)
			{
				Log.Error("Unable to restart Adventure game scenario.  Save data is null.");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00021", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
				yield break;
			}
			CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState;
			if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Unable to restart Adventure game scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			yield break;
		}
		yield return LoadSceneCoroutine("Game");
		yield return OnStartScenarioCallback(AWLaunchMethod.create, restart: true);
	}

	public void CampaignStartFHScenario(CMapScenarioState mapScenarioState)
	{
		StartCoroutine(CampaignStartFHScenarioCoroutine(mapScenarioState));
	}

	private IEnumerator CampaignStartFHScenarioCoroutine(CMapScenarioState mapScenarioState)
	{
		try
		{
			if (mapScenarioState.LoadInitialState && mapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				SaveData.Instance.Global.CurrentCustomLevelData = mapScenarioState.CustomLevelData;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while starting campaign scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			yield break;
		}
		yield return LoadSceneCoroutine("Game");
		yield return OnStartScenarioCallback((FFSNetwork.IsOnline && !FFSNetwork.IsHost) ? AWLaunchMethod.join : AWLaunchMethod.create);
	}

	public void CampaignScenarioStart()
	{
		StartCoroutine(CampaignScenarioStartCoroutine());
	}

	private IEnumerator CampaignScenarioStartCoroutine()
	{
		try
		{
			CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState;
			if (currentMapScenarioState.LoadInitialState && currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while starting campaign scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			yield break;
		}
		yield return LoadSceneCoroutine("Game");
		yield return OnStartScenarioCallback((FFSNetwork.IsOnline && !FFSNetwork.IsHost) ? AWLaunchMethod.join : AWLaunchMethod.create);
	}

	public void ResumeSingleScenario()
	{
		StartCoroutine(ResumeSingleScenarioCoroutine());
	}

	private IEnumerator ResumeSingleScenarioCoroutine()
	{
		ShowLoadingScreen();
		yield return null;
		if (!PlayerRegistry.Initialized)
		{
			yield return PlayerRegistry.InitializeCrt();
		}
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Log.Error("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		Task<PartyAdventureData> getResumeTask = SaveData.Instance.Global.GetResumeSingleScenario();
		yield return new WaitForTask(getResumeTask);
		PartyAdventureData resumeSingleScenarioSave = getResumeTask.Result;
		if (resumeSingleScenarioSave != null)
		{
			Task task = resumeSingleScenarioSave.Load(EGameMode.SingleScenario, isJoiningMPClient: false);
			yield return new WaitForTask(task);
			SaveData.Instance.Global.CurrentAdventureData = resumeSingleScenarioSave;
		}
		if (resumeSingleScenarioSave?.AdventureMapState?.CurrentSingleScenario?.ScenarioState != null)
		{
			SaveData.Instance.Global.CurrentCustomLevelData = resumeSingleScenarioSave.AdventureMapState.CurrentSingleScenario;
			SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			AdventureState.UpdateMapState(resumeSingleScenarioSave.AdventureMapState);
			SaveData.Instance.StartCustomLevel();
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			yield return LoadSceneCoroutine("Game");
			yield return OnCustomLevelLoadedCallback(resumeSingleScenarioSave.AdventureMapState.CurrentSingleScenario);
		}
		else
		{
			Log.ErrorWithReport("Unable to find resume state for scenario\n");
			SaveData.Instance.Global.CurrentAdventureData = null;
		}
	}

	public void LoadSingleScenario(PartyAdventureData partyData)
	{
		StartCoroutine(LoadSingleScenarioCoroutine(partyData));
	}

	private IEnumerator LoadSingleScenarioCoroutine(PartyAdventureData partyData)
	{
		ShowLoadingScreen();
		yield return null;
		UnityGameEditorRuntime.s_SkipNextSelectCardSyncCheck = false;
		if (!PlayerRegistry.Initialized)
		{
			yield return PlayerRegistry.InitializeCrt();
		}
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Log.Error("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			SaveData.Instance.StartCustomLevel();
			if (AdventureState.MapState == null)
			{
				LoadAdventureAssetBundles();
				AdventureState.StartAdventure(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState);
			}
			partyData.ResolveFHGameState(YMLDefRepository.Repo, recreateMapParty: false);
			ScenarioStatRepository.ResetInstance();
			if (partyData.AdventureMapState.CurrentSingleScenario.ScenarioState == null)
			{
				if (partyData.AdventureMapState.CurrentSingleScenario.YMLFile == null || partyData.AdventureMapState.CurrentSingleScenario.YMLFile.Length <= 0)
				{
					throw new Exception("Custom Level Scenario State is null");
				}
				ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.Scenarios.SingleOrDefault((ScenarioDefinition s) => s.FileName == partyData.AdventureMapState.CurrentSingleScenario.YMLFile);
				if (scenarioDefinition == null)
				{
					throw new Exception("Unable to find scenario with filename " + partyData.AdventureMapState.CurrentSingleScenario.YMLFile);
				}
				partyData.AdventureMapState.CurrentSingleScenario.ScenarioState = CMapScenarioState.CreateNewScenario(scenarioDefinition, SharedClient.GlobalRNG.Next(), partyData.AdventureMapState.MapParty.PartyLevel, scenarioDefinition.FileName, LocalizationManager.GetTranslation(scenarioDefinition.ID), (scenarioDefinition.Description != string.Empty) ? LocalizationManager.GetTranslation(scenarioDefinition.Description) : string.Empty, partyData.AdventureMapState.MapParty.SelectedCharacters.Count(), new SharedLibrary.Random(), out var _, out var _, partyData.AdventureMapState.MapParty.ThreatLevel);
			}
			if (SaveData.Instance.Global.CurrentCustomLevelData == null)
			{
				SaveData.Instance.Global.CurrentCustomLevelData = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while loading a custom map\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			yield break;
		}
		UnityGameEditorRuntime.s_SkipNextSelectCardSyncCheck = FFSNetwork.IsOnline && FFSNetwork.IsClient;
		yield return LoadSceneCoroutine("Game");
		yield return OnCustomLevelLoadedCallback(partyData.AdventureMapState.CurrentSingleScenario);
	}

	public void LoadCustomLevel(CCustomLevelData customLevel, bool isFrontEndTutorial = false)
	{
		StartCoroutine(LoadCustomLevelCoroutine(customLevel, isFrontEndTutorial));
	}

	public IEnumerator LoadCustomLevelCoroutine(CCustomLevelData customLevel, bool isFrontEndTutorial = false, KronosProfilingScaffolding profiling = null)
	{
		EGameMode gameMode = (isFrontEndTutorial ? EGameMode.FrontEndTutorial : EGameMode.SingleScenario);
		ShowLoadingScreen();
		yield return null;
		ControllableRegistry.Reset();
		PlayerRegistry.Reset();
		if (!PlayerRegistry.Initialized)
		{
			yield return PlayerRegistry.InitializeCrt();
		}
		yield return PersistentData.s_Instance.InitMonsterCards();
		if (PersistentData.s_Instance.FailedLoading)
		{
			Log.Error("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), "Custom Party", EAdventureDifficulty.Normal, new SaveOwner(), gameMode, ERulesFlag.None);
		partyAdventureData.AdventureMapState.CurrentSingleScenario = customLevel;
		SaveData.Instance.Global.CurrentAdventureData = partyAdventureData;
		SaveData.Instance.Global.CurrentHostAccountID = partyAdventureData.Owner.PlatformAccountID;
		if (isFrontEndTutorial)
		{
			SaveData.Instance.StartFrontEndTutorialLevel();
		}
		else
		{
			SaveData.Instance.StartCustomLevel();
		}
		UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
		yield return LoadSceneCoroutine("Game");
		yield return OnCustomLevelLoadedCallback(customLevel, profiling);
	}

	public IEnumerator OnCustomLevelLoadedCallback(CCustomLevelData customLevel, KronosProfilingScaffolding profiling = null)
	{
		if (customLevel.ScenarioState != null && customLevel.ScenarioState.Players.Count > 0)
		{
			IEnumerator enumerator;
			try
			{
				enumerator = Instance.LoadScenarioAssetBundlesAsync(customLevel.ScenarioState);
			}
			catch (Exception ex)
			{
				HandleException(ex);
				yield break;
			}
			yield return enumerator;
			Log.Info("Load scenario \"" + customLevel.Name + "\"");
			Task task;
			try
			{
				task = UnityGameEditorRuntime.LoadScenario(customLevel.ScenarioState, (CMapParty)null, profiling);
			}
			catch (Exception ex2)
			{
				HandleException(ex2);
				yield break;
			}
			yield return new WaitForTask(task);
			profiling?.Stopwatch?.Time("Scene loaded");
			yield break;
		}
		GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00018", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
		throw new Exception("ERROR_SCENE_00018; Missing ScenarioState or no players are in the ScenarioState!");
		void HandleException(Exception ex3)
		{
			Log.Exception(ex3, "An exception occurred while starting scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, LoadMainMenu, ex3.Message);
		}
	}

	public void LevelEditorStart(ScenarioRuleLibrary.ScenarioState scenarioState = null)
	{
		StartCoroutine(LevelEditorStartCoroutine(scenarioState));
	}

	private IEnumerator LevelEditorStartCoroutine(ScenarioRuleLibrary.ScenarioState scenarioState = null)
	{
		ShowLoadingScreen();
		yield return null;
		AdventureState.End();
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Log.Error("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		Task startEditorTask = SaveData.Instance.StartLevelEditor();
		yield return new WaitForTask(startEditorTask);
		if (startEditorTask.HasFailed(out var exception))
		{
			HandleException(exception);
		}
		IEnumerator enumerator = null;
		try
		{
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			enumerator = LoadSceneCoroutine("Game");
		}
		catch (Exception ex)
		{
			HandleException(ex);
		}
		yield return enumerator;
		yield return OnStartLevelEditorCallback(scenarioState);
		void HandleException(Exception ex2)
		{
			Log.Exception(ex2, "An exception occurred while starting the level editor\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, LoadMainMenu, ex2.Message);
		}
	}

	private IEnumerator OnStartLevelEditorCallback(ScenarioRuleLibrary.ScenarioState scenarioState = null)
	{
		if (scenarioState != null)
		{
			IEnumerator enumerator = null;
			try
			{
				enumerator = Instance.LoadScenarioAssetBundlesAsync();
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			yield return enumerator;
			try
			{
				SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState = scenarioState;
			}
			catch (Exception ex2)
			{
				HandleException(ex2);
			}
		}
		try
		{
			LevelEditorController.s_Instance.StartLevelEditor();
		}
		catch (Exception ex3)
		{
			HandleException(ex3);
		}
		void HandleException(Exception ex4)
		{
			Log.Exception(ex4, "An exception occurred while starting scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex4.StackTrace, LoadMainMenu, ex4.Message);
		}
	}

	public void AutoTestStart(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false, bool skipMonsterCardGeneration = false)
	{
		if (autoTest == null)
		{
			Log.Error("Cannot load null AutoTest");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00024", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
		}
		else
		{
			StartCoroutine(AutoTestStartCoroutine(autoTest, autoTestState, withoutAutotestFunctionality, skipMonsterCardGeneration));
		}
	}

	private IEnumerator AutoTestStartCoroutine(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality, bool skipMonsterCardGeneration)
	{
		ShowLoadingScreen();
		yield return null;
		IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-SceneController.AutoTestStartCoroutine() => YML.LoadGuildMaster()", () =>
		{
			YML.LoadCampaign();
		});
		yield return threadHandle.WaitForCompletionCrt();
		if (!Instance.YML.LastLoadResult)
		{
			Log.Error("Unable to load Guildmaster YML");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		if (!skipMonsterCardGeneration)
		{
			yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
			if (PersistentData.s_Instance.FailedLoading)
			{
				Log.Error("Unable to load Monster Card YML\n");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
				yield break;
			}
		}
		IEnumerator enumerator = null;
		try
		{
			SaveData.Instance.StartAutoTest(autoTest);
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			enumerator = LoadSceneCoroutine("Game");
		}
		catch (Exception ex)
		{
			HandleException(ex);
		}
		yield return enumerator;
		IEnumerator enumerator2 = null;
		try
		{
			enumerator2 = OnStartAutoTestCallback(autoTest, autoTestState, withoutAutotestFunctionality);
		}
		catch (Exception ex2)
		{
			HandleException(ex2);
		}
		yield return enumerator2;
		void HandleException(Exception ex3)
		{
			Log.Exception(ex3, "An exception occurred while starting the Autotest\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, LoadMainMenu, ex3.Message);
		}
	}

	public IEnumerator BulkRunAutoTestStart(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false)
	{
		try
		{
			ShowLoadingScreen();
			SaveData.Instance.StartAutoTest(autoTest);
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred while starting the Autotest\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
		yield return LoadSceneCoroutine("Game");
		yield return OnStartAutoTestCallback(autoTest, autoTestState, withoutAutotestFunctionality);
	}

	private IEnumerator OnStartAutoTestCallback(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false)
	{
		IEnumerator enumerator = null;
		try
		{
			enumerator = Instance.LoadScenarioAssetBundlesAsync(autoTest.ScenarioState);
		}
		catch (Exception ex)
		{
			HandleException(ex);
		}
		yield return enumerator;
		try
		{
			AutoTestController.s_Instance.LoadIntoAutoTest(autoTest, autoTestState, withoutAutotestFunctionality);
		}
		catch (Exception ex2)
		{
			HandleException(ex2);
		}
		void HandleException(Exception ex3)
		{
			Log.Exception(ex3, "An exception occurred while starting scenario\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("Could not start scenario, game will now return to Main Menu", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, LoadMainMenu, ex3.Message);
		}
	}

	private void ShowDesyncDetectionError(Exception ex)
	{
		bool flag = ex?.Message == "ERROR_MULTIPLAYER_00037";
		List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
		if (flag)
		{
			list.Add(new ErrorMessage.LabelAction("GUI_RESTART_ROUND", () =>
			{
				UIScenarioEscMenu.ScenarioInstance?.QueueRestartRound(() =>
				{
					ShowDesyncDetectionError(ex);
				});
			}, InputFHManager.Controls.Generic.AlternativeChoice));
		}
		list.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", () =>
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.None)
			{
				UIManager.LoadMainMenu(null, () =>
				{
					Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
				});
			}
			else
			{
				GlobalErrorMessage.CloseErrorMessage();
			}
		}, InputFHManager.Controls.Generic.Confirm));
		string errorMessageKey = "ERROR_MULTIPLAYER_00022";
		if (flag)
		{
			errorMessageKey = ex.Message;
		}
		GlobalErrorMessage.ShowMultiChoiceMessage(errorMessageKey, list, "GUI_ERROR_TITLE", ex?.StackTrace, ex?.Message, showErrorReportButton: true, trackDebug: false, allowMessageBox: false);
		Instance.GlobalErrorMessage.SuppressShowingErrorMessages = true;
	}

	public IEnumerator WaitForPlayers()
	{
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.WaitingForPlayers);
		while (!PlayerRegistry.CheckLoadingFinished())
		{
			yield return null;
		}
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
	}

	public void PlayerReadyForAssignment(GameAction action)
	{
		NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == new PlayerID(action.PlayerID));
		if (networkPlayer == null)
		{
			return;
		}
		networkPlayer.PlayerReadyForAssignment = true;
		MultiplayerGameController multiplayerGameController = Singleton<FHMapChoreographer>.InstanceFast?.GameClient?.GameController;
		if (multiplayerGameController != null)
		{
			FHMultiplayerUtils.SyncPlayers(multiplayerGameController);
		}
		MultiplayerGameController multiplayerGameController2 = Singleton<FHMapChoreographer>.InstanceFast?.GameServer?.GameController;
		if (multiplayerGameController2 != null)
		{
			FHMultiplayerUtils.SyncPlayers(multiplayerGameController2);
		}
		if (FFSNetwork.IsHost)
		{
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
			{
				Singleton<UIScenarioMultiplayerController>.InstanceFast?.RefreshWaitingNotifications();
				return;
			}
			_ = SaveData.Instance.Global.CurrentGameState;
			_ = 1;
		}
	}

	public IEnumerator LoadMostRecentSave()
	{
		mostRecentSaveLoaded = false;
		Task<PartyAdventureData> task = SaveData.Instance.Global.FindMostRecentSave();
		yield return new WaitForTask(task);
		PartyAdventureData result = task.Result;
		if (result == null)
		{
			yield break;
		}
		switch (result.GameMode)
		{
		case EGameMode.Campaign:
			CampaignResume(result, isJoiningMPClient: false);
			mostRecentSaveLoaded = true;
			break;
		case EGameMode.SingleScenario:
		case EGameMode.FrontEndTutorial:
			yield return YML.LoadCampaignAsyncCrt();
			if (!Instance.YML.LastLoadResult)
			{
				Log.Error("Unable to load Guildmaster YML");
				break;
			}
			ResumeSingleScenario();
			mostRecentSaveLoaded = true;
			break;
		}
	}

	public void LoadAdventureAssetBundles()
	{
	}

	public void LoadScenarioAssetBundles(ScenarioRuleLibrary.ScenarioState scenarioBeingLoaded = null, CMapState mapStateBeingLoaded = null)
	{
		AssetBundleManager.Instance.LoadAllScenarioAssetsBeforeAction(scenarioBeingLoaded, mapStateBeingLoaded);
	}

	public IEnumerator LoadScenarioAssetBundlesAsync(ScenarioRuleLibrary.ScenarioState scenarioBeingLoaded = null, CMapState mapStateBeingLoaded = null)
	{
		AssetBundleManager.Instance.LoadAllScenarioAssetsBeforeAction(scenarioBeingLoaded, mapStateBeingLoaded);
		yield return new WaitWhile(() => AssetBundleManager.Instance.ScenarioBundlesLoading);
	}

	private void HandleQuickLaunchError(string message)
	{
		List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
		{
			new ErrorMessage.LabelAction("CANCEL_BUTTON", LoadMainMenu, InputFHManager.Controls.Generic.Back)
		};
		GlobalErrorMessage.ShowGenericDebugMessage("Quick Launch Error", message, buttons);
	}

	private static async Task<CCustomLevelData> GetCustomLevelData(string query)
	{
		SaveData.Instance.LevelEditorDataManager.DetermineAvailableFilesFromLoadFolder(SaveData.Instance.Global.UseCustomLevelDataFolder);
		string actualLevelName = query.ToLowerInvariant();
		FileInfo fi = SaveData.Instance.LevelEditorDataManager.AvailableFiles.FirstOrDefault((FileInfo fileInfo) => fileInfo.Name.ToLowerInvariant().Contains(actualLevelName));
		if (fi != null)
		{
			return (await SaveData.Instance.LevelEditorDataManager.GetLevelDataForFile(fi.FullName)) ?? throw new ApplicationException("Could not load level data for '" + fi.FullName + "'");
		}
		ScenarioDef scenarioDef = YMLDefRepository.Repo.FHScenarios.Items.FirstOrDefault((ScenarioDef x) => x.LevelID.ToLowerInvariant().Contains(actualLevelName));
		if (scenarioDef != null)
		{
			return ScenarioRuleClient.SRLYML.GetCustomLevel(scenarioDef.LevelID) ?? throw new ApplicationException("Could not load level data for '" + scenarioDef.LevelID + "'");
		}
		throw new ApplicationException("Cannot find level that contains '" + query + "' in its name");
	}

	[ConsoleCommand(Command = "edit", Description = " <scenario> - Opens the scenario in the level editor")]
	public static async void EditCommand(IConsole console, string scenario)
	{
		CCustomLevelData cCustomLevelData;
		try
		{
			cCustomLevelData = await GetCustomLevelData(scenario);
		}
		catch (Exception ex)
		{
			throw new ConsoleCommandException(ex.Message);
		}
		SceneController instance = Instance;
		if (instance.IsLoading)
		{
			throw new ConsoleCommandException("Cannot edit a scenario while loading");
		}
		console.WriteLine("Opening for edit level '" + cCustomLevelData.Name + "'");
		instance.StartCoroutine(StartEditLevel(cCustomLevelData));
	}

	private static IEnumerator StartEditLevel(CCustomLevelData levelData)
	{
		Task task = SaveData.Instance.LoadCustomLevelFromData(levelData, LevelEditorController.ELevelEditorState.Editing);
		yield return new WaitForTask(task);
		if ((bool)MainMenuUIManager.Instance)
		{
			MainMenuUIManager.Instance.mainMenu.Hide();
		}
		SceneController sc = Instance;
		sc.ShowLoadingScreen();
		yield return 0.1f;
		LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Campaign;
		sc.YML.LoadSingleScenarios(LevelEditorController.s_Instance.LastLoadedRuleset);
		if (sc.YML.LastLoadResult)
		{
			sc.DisableLoadingScreen();
		}
		else
		{
			Log.GUIError("Unable to load YML for DLL Mode: " + LevelEditorController.s_Instance.LastLoadedRuleset);
			sc.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, sc.LoadMainMenu);
		}
		sc.LevelEditorStart();
	}

	[ConsoleCommand(Command = "test", Description = " <auto test> - Runs the auto tests")]
	public static async void Testommand(IConsole console, string autoTest)
	{
		await SaveData.Instance.AutoTestDataManager.DetermineAvailableFilesFromLoadFolder();
		FileInfo autoTestFile = SaveData.Instance.AutoTestDataManager.AutoTestFiles.FirstOrDefault((FileInfo f) => f.Name.ToLowerInvariant().Contains(autoTest.ToLowerInvariant()));
		if (autoTestFile == null)
		{
			throw new ConsoleCommandException("Cannot find auto test file that contains '" + autoTest + "' in its name");
		}
		SaveData instance = SaveData.Instance;
		await instance.LoadAutoTestFromData(await SaveData.Instance.AutoTestDataManager.GetAutotestDataFromFile(autoTestFile), autoTestFile);
		Instance.AutoTestStart(SaveData.Instance.Global.CurrentAutoTestDataCopy, EAutoTestControllerState.PlayFromMenu);
	}
}
