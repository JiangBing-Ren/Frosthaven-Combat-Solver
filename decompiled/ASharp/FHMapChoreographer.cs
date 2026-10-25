using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EPOOutline;
using FFSNet;
using FFSNet.Backend;
using FH.YML;
using FHGUI;
using FHGUI.Common;
using FHGUI.Services;
using FHGUI.States;
using FHRL;
using Libs.GameConsole;
using Libs.Utils;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Achievments;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class FHMapChoreographer : Singleton<FHMapChoreographer>
{
	private class DesyncDetection
	{
		public float Countdown;

		public bool Desynced;
	}

	private class Standalone
	{
	}

	public struct GameStateRecievingState
	{
		public float Progress;

		public bool InProgress;

		public MultiplayerGameState State;
	}

	public FHMapView View;

	private static bool _autoBattle;

	private bool _endGame;

	private bool _movingToScenarioMap;

	private GameStateRecievingState _gameStateRecievingOpState;

	private FHScenarioResult _scenarioResultToProcess;

	private DesyncDetection _desync = new DesyncDetection();

	private Standalone _standalone = new Standalone();

	private bool _loading;

	public bool TrigerAutoSave;

	private List<NetworkPlayer> _sendingStateToPlayers = new List<NetworkPlayer>();

	private CampaignState _cmdPrevState;

	public FHMultiplayerClient GameClient { get; private set; }

	public FHMultiplayerServer GameServer { get; private set; }

	public CampaignState GameState => GameClient.GameController.CampaignController.State;

	public CampaignController GameController => GameClient.GameController.CampaignController;

	public DefRepository DefRepo { get; private set; }

	public bool IsStandaloneScene
	{
		get
		{
			if (SceneController.Instance == null)
			{
				return Application.isEditor;
			}
			return false;
		}
	}

	public bool IsMultiplayerEnabled
	{
		get
		{
			if (GameServer == null)
			{
				return FFSNetwork.IsOnline;
			}
			return true;
		}
	}

	public bool AutoBattle
	{
		get
		{
			if (!IsStandaloneScene)
			{
				return _autoBattle;
			}
			return true;
		}
	}

	public bool AutoStartFirstScenario
	{
		get
		{
			if (IsStandaloneScene)
			{
				return FetchDevPref("AutoStartFirstScenario", defaultValue: true);
			}
			return true;
		}
	}

	public bool AutoCreateParty
	{
		get
		{
			if (IsStandaloneScene)
			{
				return FetchDevPref("AutoCreateParty", defaultValue: true);
			}
			return false;
		}
	}

	public bool SkipIntro
	{
		get
		{
			if (IsStandaloneScene)
			{
				return FetchDevPref("SkipIntro", defaultValue: true);
			}
			return false;
		}
	}

	public bool UsePlaceholderMaps
	{
		get
		{
			if (IsStandaloneScene)
			{
				return FetchDevPref("UsePlaceholderMaps", defaultValue: true);
			}
			return false;
		}
	}

	public GameStateRecievingState GameStateRecievingOpOpState => _gameStateRecievingOpState;

	public bool IsTutorial => GameState.GameMode.Name == "Tutorial";

	public bool IsInRoadToFrosthaven => GameState.GameMode.Name == "RoadToFrosthaven";

	private static BuildingState SelectedBuilding
	{
		get
		{
			if (!(Singleton<FHMapChoreographer>.Instance.View.CurrentState is OutpostVillageMapState outpostVillageMapState))
			{
				return null;
			}
			BuildingTypeID? selectedBuildingID = outpostVillageMapState.SelectedBuilding;
			if (!selectedBuildingID.HasValue)
			{
				return null;
			}
			return Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Buildings.SingleOrDefault((BuildingState x) => x.ID == selectedBuildingID.Value);
		}
	}

	[ConsoleVariable(Alias = "target_framerate", Persistent = false)]
	public static int TargetFrameRate
	{
		get
		{
			return Application.targetFrameRate;
		}
		set
		{
			Application.targetFrameRate = value;
		}
	}

	protected override void AwakeInit()
	{
		if (IsStandaloneScene)
		{
			GameConsoleWindow.Create();
			UnityEngine.Object.DestroyImmediate(UnityEngine.Object.FindObjectOfType<UIMapEscMenu>(includeInactive: true).gameObject);
			UnityEngine.Object.FindObjectsByType<UIAudioEffects>(FindObjectsSortMode.None).ForEach((UIAudioEffects o) =>
			{
				UnityEngine.Object.Destroy(o);
			});
		}
		else
		{
			UnityEngine.Object.FindObjectOfType<UIMapEscMenu>(includeInactive: true).gameObject.SetActive(value: true);
		}
		GameConsole.Context.Add(this);
		if (IsStandaloneScene)
		{
			GameConsole.Context.Add(_standalone);
		}
		GameConsole.Engine.CommandPerprocessor = NetworkCommandPreprocessor;
		CommandExecEngine engine = GameConsole.Engine;
		engine.CommandPostprocessor = (Action<IConsole, ICommand>)Delegate.Combine(engine.CommandPostprocessor, new Action<IConsole, ICommand>(NetworkCommandPostprocessor));
		DefRepo = YMLDefRepository.Repo.DefRepository;
		GameClient = new FHMultiplayerClient(DefRepo);
		if (SaveData.Instance == null)
		{
			CampaignState state = FHSaveData.NewGameState(YMLDefRepository.Repo);
			GameClient.ReplaceState(state, null);
		}
		else
		{
			MultiplayerGameState state2 = FHGlue.GameStateFromSave();
			GameClient.ReplaceState(state2);
			_scenarioResultToProcess = SaveData.Instance?.Global?.CurrentAdventureData?.ScenarioResult;
		}
		if (IsStandaloneScene)
		{
			if (SkipIntro)
			{
				SkipIntroduction(create_party: false);
			}
			if (AutoCreateParty && GameState.AllCharacters.Count == 0)
			{
				CreateDefaultParty();
			}
		}
		StartCoroutine(LoadSequence());
	}

	private IEnumerator LoadSequence()
	{
		_loading = true;
		View.ShowLoadingScreen();
		yield return StartMultiplayerCrt();
		yield return View.OutpostSceneController.Init(View.GameState);
		View.InitUI();
		yield return View.WaitUntilScreenIsReady();
		while (_scenarioResultToProcess != null)
		{
			yield return null;
		}
		View.HideLoadingScreen();
		_loading = false;
		if (IsStandaloneScene && AutoCreateParty && SkipIntro)
		{
			Singleton<FHMapChoreographer>.Instance.View.Choreographer.ExecuteCommand(FHRPC.Campaign.NextPhase);
		}
	}

	private bool NetworkCommandPreprocessor(IConsole console, ICommand command)
	{
		if (command is CommandCallCommand commandCallCommand)
		{
			NetworkConsoleCommandAttribute customAttribute = commandCallCommand.Command.MethodInfo.GetCustomAttribute<NetworkConsoleCommandAttribute>();
			if (customAttribute != null)
			{
				if (customAttribute.SingleplayerOnly)
				{
					if (FFSNetwork.IsClient || GameServer != null)
					{
						console.WriteLineWithStyle("Command '" + commandCallCommand.Command.Name + "' can be executed only in singleplayer!", CommandOutputStyle.Error);
						return false;
					}
				}
				else if (FFSNetwork.IsClient)
				{
					console.WriteLineWithStyle("In multiplayer, command '" + commandCallCommand.Command.Name + "' can be executed only on host!", CommandOutputStyle.Error);
					return false;
				}
				_cmdPrevState = GameClient.CloneCurrentGameState();
			}
		}
		return true;
	}

	private void NetworkCommandPostprocessor(IConsole console, ICommand command)
	{
		if (!(command is CommandCallCommand commandCallCommand))
		{
			return;
		}
		NetworkConsoleCommandAttribute customAttribute = commandCallCommand.Command.MethodInfo.GetCustomAttribute<NetworkConsoleCommandAttribute>();
		if (customAttribute != null)
		{
			View.RefreshView();
			CampaignController.Result.ResultStats oldState = new CampaignController.Result.ResultStats(_cmdPrevState, DefRepo);
			CampaignController.Result.ResultStats newState = new CampaignController.Result.ResultStats(GameClient.GameState.CampaignState, DefRepo);
			ProcessStateChangeForAchievementStats(oldState, newState, null);
			if (GameServer != null && !customAttribute.SingleplayerOnly && !GameServer.IsStateSameAsServer(GameClient.GameState))
			{
				GameServer.ReplaceState(GameClient.GameState);
				SendGameState();
			}
		}
	}

	private IEnumerator StartMultiplayerCrt()
	{
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		GameAction.ExecuteActionHandler = OnGameAction;
		bool loadingJoiningFromClient = PlayerRegistry.LoadingInFromJoiningClient;
		bool waitForPlayers = PlayerRegistry.WaitForOtherPlayers;
		if (!PlayerRegistry.Initialized)
		{
			yield return PlayerRegistry.InitializeCrt();
			PlayerRegistry.LoadingInFromJoiningClient = loadingJoiningFromClient;
			PlayerRegistry.WaitForOtherPlayers = waitForPlayers;
		}
		PlayerRegistry.OnSlotOwnershipChanged = (SlotOwnershipChangedEvent)Delegate.Combine(PlayerRegistry.OnSlotOwnershipChanged, new SlotOwnershipChangedEvent(OnSlotOwnershipChanged));
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Combine(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnterRoom));
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningPlayerLeft));
		ControllableRegistry.OnControllableObjectChanged = (ControllableObjectChangedEvent)Delegate.Combine(ControllableRegistry.OnControllableObjectChanged, new ControllableObjectChangedEvent(OnControllableObjectChanged));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		GameClient.Init();
		if (IsStandaloneScene)
		{
			yield break;
		}
		FFSNetwork.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		FFSNetwork.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
		new Stopwatch();
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsHost)
			{
				GameServer = new FHMultiplayerServer(GameClient.GameState, DefRepo);
				GHNetworkCallbacks.CheckForSavePointReached();
				Synchronizer.SendSideAction(GameActionType.GameLoadedAndHostReady);
				SceneController.Instance.GameLoadedAndHostReady = true;
			}
			else
			{
				if (!PlayerRegistry.LoadingInFromJoiningClient)
				{
					Log.Info("Client waiting for Host to finish loading map scene before continuing MultiplayerStartup");
					while (!SceneController.Instance.GameLoadedAndHostReady)
					{
						yield return null;
					}
				}
				else
				{
					RequestGameState();
					while (_gameStateRecievingOpState.InProgress)
					{
						yield return null;
					}
				}
				try
				{
					if (!SceneController.Instance.GameLoadedAndClientReady)
					{
						Synchronizer.SendSideAction(GameActionType.GameLoadedAndClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
						SceneController.Instance.GameLoadedAndClientReady = true;
					}
					if (!PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment)
					{
						PlayerRegistry.MyPlayer.PlayerReadyForAssignment = true;
						PlayerRegistry.MyPlayer.SentPlayerReadyForAssignment = true;
						Synchronizer.SendSideAction(GameActionType.ReadyForAssignment);
					}
					PlayerRegistry.LoadingInFromJoiningClient = false;
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "An exception occurred within the MultiplayerStartup function");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
				}
			}
			if (FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayers && PlayerRegistry.MyPlayer.IsParticipant)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyLoadingFinished);
				PlayerRegistry.NotifyLoadingFinished(PlayerRegistry.MyPlayer);
				if (PlayerRegistry.WaitForOtherPlayers)
				{
					yield return SceneController.Instance.WaitForPlayers();
				}
			}
		}
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
		Singleton<FHMapChoreographer>.Instance.View.ResetView();
	}

	private void EndMultiplayer()
	{
		GameServer?.Dispose();
		PlayerRegistry.OnSlotOwnershipChanged = (SlotOwnershipChangedEvent)Delegate.Remove(PlayerRegistry.OnSlotOwnershipChanged, new SlotOwnershipChangedEvent(OnSlotOwnershipChanged));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		GameAction.ExecuteActionHandler = null;
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnUserEnterRoom = (UserEnterEvent)Delegate.Remove(PlayerRegistry.OnUserEnterRoom, new UserEnterEvent(OnUserEnterRoom));
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningPlayerLeft));
		ControllableRegistry.OnControllableObjectChanged = (ControllableObjectChangedEvent)Delegate.Remove(ControllableRegistry.OnControllableObjectChanged, new ControllableObjectChangedEvent(OnControllableObjectChanged));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		if (!IsStandaloneScene)
		{
			FFSNetwork.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		}
	}

	private void OnSwitchedToSinglePlayer()
	{
		if (_endGame)
		{
			return;
		}
		PlayerRegistry.HostReinitialize();
		GameClient.Init();
		GameServer?.Dispose();
		GameServer = null;
		if (YMLDefRepository.Repo.DefRepository == null)
		{
			return;
		}
		GameClient.Init();
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
		CharacterState[] array = GameState.AllCharacters.ToArray();
		foreach (CharacterState characterState in array)
		{
			if (characterState != null && characterState.PartyState == CharacterPartyStateType.CreationProcess && !characterState.CreationFlowPassed)
			{
				ExecuteCommand(FHRPC.Campaign.Character.CancelCreateNewCharacter, characterState.ID);
			}
		}
		View.ResetView();
	}

	private void OnSwitchedToMultiplayer()
	{
		PlayerRegistry.HostReinitialize();
		GameClient.Init();
		GameServer = new FHMultiplayerServer(GameClient.GameState, DefRepo);
		if (Singleton<UIMultiplayerEscSubmenu>.Instance != null)
		{
			Singleton<UIMultiplayerEscSubmenu>.Instance.Refresh();
		}
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
		View.ResetView();
	}

	private void OnSlotOwnershipChanged(PartySlotID slotid, NetworkPlayer player)
	{
		View.RefreshView();
	}

	private void OnControllableObjectChanged(NetworkControllable controllable, IControllable oldControllableObject, IControllable newControllableObject)
	{
		if (!FFSNetwork.IsStartingUp)
		{
			Log.NetworkInfo("MAP: OnControllableOwnershipChanged");
			NetworkControllable controllable2 = ControllableRegistry.GetControllable(controllable.ID);
			NetworkPlayer newController = PlayerRegistry.AllPlayers.First((NetworkPlayer p) => p.PlayerID == controllable.Controller.PlayerID);
			View?.UpdateCharacterController(controllable2, newController, null);
		}
	}

	private void OnControllableOwnershipChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		if (!FFSNetwork.IsStartingUp)
		{
			Log.NetworkInfo("MAP: OnControllableOwnershipChanged");
			View?.RefreshView();
			View?.UpdateCharacterController(controllable, newController, oldController);
		}
	}

	private void OnJoiningPlayerLeft(NetworkBackendPlayer connection)
	{
		View?.RefreshView();
	}

	private void OnUserEnterRoom(UserToken userToken)
	{
		View?.RefreshView();
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		if (FFSNetwork.IsHost)
		{
			_sendingStateToPlayers.Remove(player);
			RefreshSendingGameStateBlocking();
		}
		View?.RefreshView();
		View?.CancelVotes();
	}

	private void OnPlayerJoined(NetworkPlayer player)
	{
		if (FFSNetwork.IsOnline && Singleton<UINotificationManager>.InstanceFast != null)
		{
			UIMultiplayerNotifications.ShowPlayerJoined(player);
		}
		View?.RefreshView();
		View?.CancelVotes();
	}

	private void OnPlayerConnected(NetworkBackendPlayer connection)
	{
		View?.RefreshView();
		View?.CancelVotes();
	}

	public static void OutOfSceneCommand(GameAction action)
	{
		MultiplayerCommand command = ((FHMultiplayerCommandToken)action.SupplementaryDataToken).Command;
		Log.Error($"Out of FHMapChoreographer command: ({command.PlayerID}) {command.Name}");
	}

	private void OnGameAction(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		if (!OnGameActionRecieved(action))
		{
			GameAction.DefaultActionHandle(action, ref actionValid, ref executionFinished);
		}
	}

	private bool OnGameActionRecieved(GameAction action)
	{
		Log.NetworkInfo($"OnGameAction: {action.ActionType}");
		if (action.ActionType == GameActionType.EnterScenario)
		{
			MoveToScenarioMap();
			return true;
		}
		if (action.ActionType == GameActionType.SyncFHCampaignState)
		{
			RequestGameState();
			return true;
		}
		if (action.ActionType == GameActionType.FHMultiplayerCommand)
		{
			if (GameServer != null)
			{
				GameServer.EnqueueCommand(((FHMultiplayerCommandToken)action.SupplementaryDataToken).Command);
			}
			else if (GameClient != null)
			{
				GameClient.EnqueueCommand(((FHMultiplayerCommandToken)action.SupplementaryDataToken).Command);
			}
			return true;
		}
		if (action.ActionType == GameActionType.FHCampaignStateSent)
		{
			NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
			_sendingStateToPlayers.Remove(player);
			RefreshSendingGameStateBlocking();
			return true;
		}
		return false;
	}

	private void RefreshSendingGameStateBlocking()
	{
		if (!FFSNetwork.IsHost)
		{
			throw new InvalidOperationException("Only host can call 'RefreshSendingGameStateBlocking'");
		}
		if (_sendingStateToPlayers.Any())
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MapHQ);
		}
		else
		{
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
		}
	}

	private void RequestGameState()
	{
		if (!FFSNetwork.IsClient)
		{
			throw new Exception("Only client can request game state");
		}
		_gameStateRecievingOpState.InProgress = true;
		_gameStateRecievingOpState.State = null;
		_gameStateRecievingOpState.Progress = 0f;
		GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.RecievingFHGameState;
		Synchronizer.RequestCustomData(DataActionType.SendFHGameState);
	}

	private void SendGameState()
	{
		if (FFSNetwork.IsOnline)
		{
			if (!FFSNetwork.IsHost)
			{
				throw new Exception("Only server can send game state");
			}
			Synchronizer.SendGameAction(GameActionType.SyncFHCampaignState);
		}
	}

	public void OnHostSendingGameStateToClient(NetworkPlayer toPlayer)
	{
		_sendingStateToPlayers.AddNotContained(toPlayer);
		RefreshSendingGameStateBlocking();
	}

	public void OnClientRecievingGameState(float progress)
	{
		ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MapHQ);
		_gameStateRecievingOpState = new GameStateRecievingState
		{
			InProgress = true,
			Progress = progress
		};
		View?.ShowRecievingGameState(progress);
	}

	public void OnClientRecievingGameStateAborted()
	{
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
		_gameStateRecievingOpState = new GameStateRecievingState
		{
			InProgress = false,
			Progress = 0f
		};
		View?.HideRecievingGameState();
	}

	public void OnClientGameStateRecieved(MultiplayerGameState state)
	{
		state.CampaignState.ResolveDefs(DefRepo);
		GameClient.ReplaceState(state);
		Log.Info("New game state recieved");
		View?.OnReceivingGameState();
		View?.HideRecievingGameState();
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapHQ);
		Synchronizer.SendSideAction(GameActionType.FHCampaignStateSent, null, canBeUnreliable: false, sendToHostOnly: true);
		_gameStateRecievingOpState = new GameStateRecievingState
		{
			State = state,
			Progress = 1f,
			InProgress = false
		};
	}

	private void ClearScenarioResultToProcess()
	{
		if (SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState != null)
		{
			SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState = null;
		}
		_scenarioResultToProcess = null;
		if (SaveData.Instance?.Global?.CurrentAdventureData?.ScenarioResult != null)
		{
			SaveData.Instance.Global.CurrentAdventureData.ScenarioResult = null;
		}
	}

	private void CreateDefaultParty(int partySize = 2)
	{
		GameClient.CreateDefaultParty(partySize);
	}

	private void SkipIntroduction(bool create_party = true)
	{
		if (GameState.IsInPrologue)
		{
			FHRL.ScenarioState scenarioState = GameState.UnlockedScenarios.FirstOrDefault((FHRL.ScenarioState s) => s.ScenarioID.ID == 1);
			if (scenarioState != null)
			{
				scenarioState.Completed = true;
			}
		}
		if (create_party && GameState.AllCharacters.Count == 0)
		{
			CreateDefaultParty();
		}
		GameState.IsSetupPhase = false;
	}

	public FHMultiplayerClient.Promise ExecuteCommand(string name, params object[] parameters)
	{
		(MultiplayerCommand, FHMultiplayerClient.Promise) tuple = GameClient.ExecuteCommand(name, parameters);
		if (GameServer != null)
		{
			GameServer.EnqueueCommand(tuple.Item1);
		}
		return tuple.Item2;
	}

	public void RecalcAttackModifierDeck(CharacterState characterState)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Find(characterState.ClassID.ID);
		characterState.AttackModifierIDs.Clear();
		List<AttackModifierYMLData> applyToThisList = new List<AttackModifierYMLData>(cCharacterClass.AttackModifierCardsPool);
		List<CharacterPerk> list = new List<CharacterPerk>();
		foreach (PerkDef perk in characterState.Perks)
		{
			list.Add(new CharacterPerk(perk.ID.ID, characterState.ClassID.ID)
			{
				IsActive = true
			});
		}
		CCharacterClass.ApplyPerksToList(list.Select((CharacterPerk p) => p.Perk).ToList(), ref applyToThisList);
		List<string> source = applyToThisList.Select((AttackModifierYMLData modifier) => modifier.Name).ToList();
		characterState.AttackModifierIDs.Clear();
		characterState.AttackModifierIDs.AddRange(source.Select((string id) => new CharacterAttackModifierID(id)));
	}

	public void RecalcAttackModifierDecks()
	{
		foreach (CharacterState allCharacter in GameState.AllCharacters)
		{
			RecalcAttackModifierDeck(allCharacter);
		}
	}

	public void ProcessScenarioResult(FHScenarioResult result)
	{
		result.HasBeenProcessed = true;
		ScenarioResult scenarioResult = new ScenarioResult
		{
			ScenarioWon = (result.eResult == EResult.Win),
			isRestarting = (result.eResult == EResult.Restart),
			InspirationChange = result.InspirationChange,
			ConclusionIndex = (uint)result.ConclusionIndex,
			ConclusionParameters = result.ConclusionParameters.ToList(),
			Effects = result.Effects.ToList(),
			RandomItemLooted = result.RandomItemLooted,
			LootedRandomSideScenarioID = result.LootedRandomSideScenarioID,
			LootedScenarioID = result.LootedScenarioID,
			LootedRandomItemBlueprintID = result.LootedRandomItemBlueprintID,
			TriggeredCampaignSections = result.TriggeredCampaignSections.ToList(),
			MoraleSpent = result.MoraleSpent,
			LootedChestTreasureTableIDs = result.LootedTreasureTableIDs.ToList()
		};
		foreach (FHScenarioResult.Character rc in result.Characters)
		{
			CharacterState characterState = GameState.PartyCharacters.FirstOrDefault((CharacterState c) => c.ClassID.ID == rc.ClassID);
			if (characterState != null)
			{
				List<ItemDef> source = DefRepo.Items.Values.Where((ItemDef i) => rc.NewItems.Contains((int)i.ID.ID)).ToList();
				scenarioResult.Characters.Add(new ScenarioResult.CharacterResult
				{
					Character = characterState.ClassDef,
					GoldChange = rc.GoldChange,
					PerkChecks = rc.PerkChecks,
					ResourceChange = rc.ResourceChange,
					ExperienceChange = rc.ExperienceChange,
					ExperienceFromAbilityCards = rc.ExperienceChangeFromAbilityCards,
					NewItemIDs = source.Select((ItemDef s) => s.ID).ToList(),
					BattleGoalCompleted = rc.BattleGoalCompleted,
					MasteriesCompleted = rc.MasteryGoalsCompletedIDs.Select((string x) => new MasteryID(x)).ToList(),
					MonstersKilled = rc.KillStats,
					ResourceChangeFromLootCards = rc.ResourceChangeFromLootCards,
					CapturedPets = rc.CapturedPets.ToList(),
					Exhausted = rc.Exhausted,
					LootCardTypesDrawn = rc.LootCardTypesDrawn
				});
			}
		}
		CompleteScenario(GameClient, scenarioResult);
	}

	private void CompleteScenario(FHMultiplayerClient client, ScenarioResult result)
	{
		if (GameClient.IsPartyLeader)
		{
			ExecuteCommand(FHRPC.Campaign.Scenario.CompleteActiveScenario, result);
		}
	}

	private void OnScenarioCompleted(CampaignEvent.ScenarioCompleted eventData)
	{
		if (eventData.Success)
		{
			ListAchievementStat completedScenarios = GlobalStatRepository.Instance.CompletedScenarios;
			uint iD = eventData.GetScenario(GameState).Def.ID.ID;
			completedScenarios.NewValue(iD.ToString());
		}
		GlobalStatRepository.Instance.ScenariosPlayed.Increment();
		ScenarioStatRepository.ResetInstance();
		GameClient.SyncCharacters();
		ClearScenarioResultToProcess();
	}

	private void MoveToScenarioMap()
	{
		_movingToScenarioMap = true;
		if (AutoBattle)
		{
			return;
		}
		GameClient.SyncCharacters();
		try
		{
			FHGlue.SetCurrentScenario(GameClient.GameController.CampaignController);
		}
		catch (Exception ex)
		{
			GameClient.GameState.CampaignState.ActiveScenario.HasEnteredScenario = false;
			throw ex;
		}
		try
		{
			PlayerRegistry.StartWaitingForPlayers();
			if (FFSNetwork.IsOnline)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.MapLoadoutScreen);
			}
			SceneController.Instance.CampaignScenarioStart();
		}
		catch (Exception ex2)
		{
			Log.Exception(ex2, "Exception while processing EnterScenario.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
		}
	}

	private void AutoCompleteScenario()
	{
		CompleteScenario(GameClient, new ScenarioResult
		{
			ScenarioWon = true,
			isRestarting = false,
			Characters = GameClient.GameState.CampaignState.PartyCharacters.Select((CharacterState c) => new ScenarioResult.CharacterResult
			{
				Character = c.ClassDef,
				BattleGoalCompleted = GameClient.GameState.CampaignState.ActiveScenario.GetCharacterSettings(c.ClassID).SelectedBattleGoal.HasValue,
				GoldChange = 1
			}).ToList()
		});
		_movingToScenarioMap = false;
	}

	private void PlayCampaignEventSounds(MultiplayerGameController.Result result)
	{
		if (result.CampaignResult.HasEvent<CampaignEvent.StartScenario>())
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.questStartAudioItem);
		}
	}

	private void ProcessGameResult(MultiplayerGameController.Result result, bool supressDialogues = false)
	{
		if (result.Action == MultiplayerGameController.Action.PlayerReadyChanged && GameClient.IsPartyLeader && GameClient.GameState.ReadyCheckID.IsNOTNullOrEmpty() && GameClient.GameState.Players.FirstOrDefault((MultiplayerGameState.Player p) => !p.Ready && p.IsParticipant) == null)
		{
			ExecuteCommand("FinishReadyCheck", GameClient.PlayerID, GameClient.GameState.ReadyCheckID);
		}
		if (result.CampaignResult != null)
		{
			if (GameClient.IsPartyLeader && !GameClient.GameState.ReadyCheckID.IsNullOrEmpty() && (result.Action == MultiplayerGameController.Action.PartySlotChanged || result.CampaignResult.HasEvent<CampaignEvent.CharacterCreationStarted>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterCreationCancelled>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterCreationCompleted>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterAddedToParty>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterRemovedFromParty>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterDeleted>()))
			{
				ExecuteCommand("CancelReadyCheck", GameClient.PlayerID);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.CharacterCreationCompleted>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterRetired>() || result.CampaignResult.HasEvent<CampaignEvent.BuildingBuilding>() || result.CampaignResult.HasEvent<CampaignEvent.BuildingUpgrading>() || result.CampaignResult.HasEvent<CampaignEvent.BuildingRebuilding>() || (result.CampaignResult.HasEvent<CampaignEvent.TravelStateChanged>() && result.CampaignResult.EndStats.ActiveScenario != null && result.CampaignResult.EndStats.PartyTravelState == CampaignState.PartyTravelStateType.TravelingToScenario) || (result.CampaignResult.HasEvent<CampaignEvent.StartScenario>() && result.CampaignResult.EndStats.ActiveScenario != null && (result.CampaignResult.EndStats.ActiveScenario.NoIntroOutro || (result.CampaignResult.EndStats.IsInPrologue && string.IsNullOrEmpty(result.CampaignResult.EndStats.PartyMarkerLocation)))))
			{
				TrigerAutoSave = true;
			}
			PlayCampaignEventSounds(result);
			foreach (CampaignEvent.ScenarioCompleted item in result.CampaignResult.Events.OfType<CampaignEvent.ScenarioCompleted>())
			{
				OnScenarioCompleted(item);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.CharacterCreationCompleted>() || result.CampaignResult.HasEvent<CampaignEvent.CharacterPerkMarksChanged>())
			{
				RecalcAttackModifierDecks();
			}
			ProcessStateChangeForAchievementStats(result);
			foreach (CampaignEvent @event in result.CampaignResult.Events)
			{
				Log.Info($"[GAME] Applied result: {@event}");
			}
		}
		View.GetService<BattleBriefingViewService>()?.ProcessBriefingChangeSound(result);
		if (supressDialogues)
		{
			GameClient.GameState.CampaignState.BlockShowUnresolvedActions = false;
		}
		else
		{
			View.ProcessGameResult(result);
		}
	}

	private void ProcessStateChangeForAchievementStats(MultiplayerGameController.Result result)
	{
		CampaignController.Result.ResultStats beginStats = result.CampaignResult.BeginStats;
		CampaignController.Result.ResultStats endStats = result.CampaignResult.EndStats;
		ProcessStateChangeForAchievementStats(beginStats, endStats, result);
	}

	private void ProcessStateChangeForAchievementStats(CampaignController.Result.ResultStats oldState, CampaignController.Result.ResultStats newState, MultiplayerGameController.Result result)
	{
		if (result?.CampaignResult != null && result.CampaignResult.HasEvent<CampaignEvent.CampaignStickerAdded>())
		{
			View?.OutpostSceneController?.OutpostMapScene?.RefreshStickerVisuals(newState.Stickers);
		}
		IEnumerable<CampaignController.Result.ResultStats.CharacterStat> source = oldState.AllCharacters.Where((CampaignController.Result.ResultStats.CharacterStat c) => c.PartyState != CharacterPartyStateType.CreationProcess);
		int num = (int)newState.AllCharacters.Where((CampaignController.Result.ResultStats.CharacterStat c) => c.PartyState != CharacterPartyStateType.CreationProcess).Sum((CampaignController.Result.ResultStats.CharacterStat s) => s.Gold) - (int)source.Sum((CampaignController.Result.ResultStats.CharacterStat s) => s.Gold);
		if (num > 0)
		{
			CampaignStatRepository.Instance.GoldCollected.Increment(num);
		}
		if (newState.Calendar.Week != oldState.Calendar.Week || (result != null && result.CampaignResult.HasEvent<CampaignEvent.OutpostEventResolved>()))
		{
			AchievementsTracker.CheckAchievements();
		}
		if (result?.CampaignResult?.Events == null)
		{
			return;
		}
		foreach (CampaignEvent @event in result.CampaignResult.Events)
		{
			UIContextualTutorialModule.OnEvent(@event);
		}
	}

	protected override void AwakenedDestroy()
	{
		AssetBundleManager.ClearImages();
		GameConsole.Context.Remove(this);
		GameConsole.Engine.CommandPerprocessor = null;
		CommandExecEngine engine = GameConsole.Engine;
		engine.CommandPostprocessor = (Action<IConsole, ICommand>)Delegate.Remove(engine.CommandPostprocessor, new Action<IConsole, ICommand>(NetworkCommandPostprocessor));
		EndMultiplayer();
		View.OutpostSceneController.Unload();
	}

	public async Task EndGame()
	{
		_endGame = true;
		View.ResetView();
		View.SetState(new InitViewState());
	}

	private void Start()
	{
		View.ResetView();
	}

	private async void Update()
	{
		if (_scenarioResultToProcess != null && !_scenarioResultToProcess.HasBeenProcessed)
		{
			ProcessScenarioResult(_scenarioResultToProcess);
		}
		if (GameServer != null)
		{
			GameServer.Update();
			MultiplayerCommand command;
			while (GameServer.TryDequeueExecutedCommand(out command))
			{
				GameClient.EnqueueCommand(command);
			}
		}
		GameClient.Update();
		if (GameClient.StateReplaced)
		{
			View.RefreshView();
		}
		GameClient.UpdateCleanup();
		MultiplayerGameController.Result r;
		while (GameClient.TryGetUnprocessedResult(out r))
		{
			ProcessGameResult(r);
		}
		if (TrigerAutoSave && !IsTutorial)
		{
			await AutoSave();
		}
		if (!_movingToScenarioMap && _scenarioResultToProcess == null && (!_loading || IsTutorial) && (GameState.ActiveScenario?.HasEnteredScenario ?? false))
		{
			if (AutoBattle)
			{
				AutoCompleteScenario();
			}
			else if (!FFSNetwork.IsOnline || (FFSNetwork.IsHost && GameServer.EnterScenarioTriggered))
			{
				MoveToScenarioMap();
			}
		}
		GHNetworkCallbacks.CheckForSavePointReached();
	}

	public async Task AutoSave()
	{
		TrigerAutoSave = false;
		if (SaveData.Instance != null)
		{
			await SaveData.Instance.AutoSaveCurrentAdventureData();
		}
	}

	public void OnDifficultyChanged(EAdventureDifficulty difficulty)
	{
		SaveData.Instance.Global.CurrentAdventureData.ChangeDifficulty(difficulty);
		View.RefreshView();
	}

	private static void ProcessResultFromCommand(CampaignController.Result result, bool suppressDialogues = false)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.EventLog.ProcessGameResult(result);
		MultiplayerGameController.Result result2 = new MultiplayerGameController.Result(result, Singleton<FHMapChoreographer>.Instance.GameClient.PlayerID);
		Singleton<FHMapChoreographer>.Instance.ProcessGameResult(result2, suppressDialogues);
		if (suppressDialogues)
		{
			Singleton<FHMapChoreographer>.Instance.View.GetService<OutpostNotificationViewService>().ProcessGameResult(result2);
		}
	}

	internal static bool FetchDevPref(string prefName, bool defaultValue = false)
	{
		return PlayerPrefs.GetInt("OUTPOST_DEV_" + prefName, defaultValue ? 1 : 0) != 0;
	}

	internal static bool StoreDevPref(string prefName, bool value)
	{
		PlayerPrefs.SetInt("OUTPOST_DEV_" + prefName, value ? 1 : 0);
		return value;
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "finish_donation", RequiredContext = typeof(FHMapChoreographer))]
	public static void FinishDonation(IConsole console)
	{
		BuildingState selectedBuilding = SelectedBuilding;
		if (selectedBuilding == null)
		{
			console.WriteLineWithStyle("No valid building selected", CommandOutputStyle.Error);
			return;
		}
		if (!selectedBuilding.IsWaitingForDonations)
		{
			console.WriteLineWithStyle($"No valid donation for {selectedBuilding.ID}", CommandOutputStyle.Error);
			return;
		}
		BuildingTypeDef def = selectedBuilding.Def;
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.FinalizeBuildingDonaions(def));
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "cancel_construction", RequiredContext = typeof(FHMapChoreographer))]
	public static void CancelConstruction(IConsole console)
	{
		BuildingState selectedBuilding = SelectedBuilding;
		if (selectedBuilding == null)
		{
			console.WriteLineWithStyle("No valid building selected", CommandOutputStyle.Error);
			return;
		}
		if (!selectedBuilding.IsWaitingForDonations)
		{
			console.WriteLineWithStyle($"No valid donation for {selectedBuilding.ID}", CommandOutputStyle.Error);
			return;
		}
		BuildingTypeDef def = selectedBuilding.Def;
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.CancelBuildingConstruction(def));
	}

	[ConsoleCommand(Command = "show_donate_progress", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowDonationProgress(IConsole console)
	{
		BuildingState selectedBuilding = SelectedBuilding;
		if (selectedBuilding == null)
		{
			console.WriteLineWithStyle("No valid building selected", CommandOutputStyle.Error);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		OutpostResources totalMaterialsDonated = selectedBuilding.donationDistribution.TotalMaterialsDonated;
		int totalGoldDonated = selectedBuilding.donationDistribution.TotalGoldDonated;
		stringBuilder.AppendLine($"Donation progress for {selectedBuilding.Def.Name}, {selectedBuilding.ConstructionState}");
		if (selectedBuilding.donationDistribution.GoldCost > 0)
		{
			stringBuilder.AppendLine($"{totalGoldDonated}/{selectedBuilding.donationDistribution.GoldCost} Gold");
		}
		if (selectedBuilding.donationDistribution.MaterialCost.Lumber > 0)
		{
			stringBuilder.AppendLine($"{totalMaterialsDonated.Lumber}/{selectedBuilding.donationDistribution.MaterialCost.Lumber} Lumber");
		}
		if (selectedBuilding.donationDistribution.MaterialCost.Metal > 0)
		{
			stringBuilder.AppendLine($"{totalMaterialsDonated.Metal}/{selectedBuilding.donationDistribution.MaterialCost.Metal} Metal");
		}
		if (selectedBuilding.donationDistribution.MaterialCost.Hide > 0)
		{
			stringBuilder.AppendLine($"{totalMaterialsDonated.Hide}/{selectedBuilding.donationDistribution.MaterialCost.Hide} Hide");
		}
		if (selectedBuilding.donationDistribution.Inspiration != 0)
		{
			stringBuilder.AppendLine($"{selectedBuilding.donationDistribution.Inspiration} inspiration");
		}
		console.WriteLineWithStyle(stringBuilder.ToString(), CommandOutputStyle.Info);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_pet")]
	public static void AddPet(IConsole console, uint id)
	{
		if (ScenarioManager.CurrentScenarioState != null)
		{
			ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.PetCards.SingleOrDefault((ItemCardYMLData x) => x.ID == id);
			if (itemCardYMLData == null)
			{
				console.WriteLineWithStyle($"No pet with {id} exists in defs", CommandOutputStyle.Error);
				return;
			}
			ScenarioManager.CurrentScenarioState.Pets.Add(itemCardYMLData.GetPet);
			console.WriteLineWithStyle("Added " + itemCardYMLData.Name + " to pets", CommandOutputStyle.Info);
		}
		if (Singleton<FHMapChoreographer>.Instance == null)
		{
			return;
		}
		PetID key = new PetID(id);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.Pets.TryGetValue(key, out var value))
		{
			console.WriteLineWithStyle($"No pet with {id} exists in defs", CommandOutputStyle.Error);
			return;
		}
		try
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.PetController.AddPet(value));
		}
		catch (Exception ex)
		{
			console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_all_pets")]
	public static void AddAllPets(IConsole console)
	{
		if (Singleton<FHMapChoreographer>.Instance == null)
		{
			return;
		}
		foreach (ItemCardYMLData petCard in ScenarioRuleClient.SRLYML.PetCards)
		{
			if (ScenarioManager.CurrentScenarioState != null && !ScenarioManager.CurrentScenarioState.Pets.Contains(petCard.GetPet))
			{
				ScenarioManager.CurrentScenarioState.Pets.Add(petCard.GetPet);
			}
			PetID key = new PetID((uint)petCard.ID);
			if (Singleton<FHMapChoreographer>.Instance.DefRepo.Pets.TryGetValue(key, out var value))
			{
				int petCapacity = Singleton<FHMapChoreographer>.Instance.GameController.PetController.GetPetCapacity();
				if (Singleton<FHMapChoreographer>.Instance.GameController.State.Outpost.Pets.Count >= petCapacity)
				{
					console.WriteLineWithStyle($"Pet capacity reached! ({petCapacity})", CommandOutputStyle.Warning);
					break;
				}
				try
				{
					ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.PetController.AddPet(value));
				}
				catch (Exception ex)
				{
					console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
				}
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "del_pet", RequiredContext = typeof(FHMapChoreographer))]
	public static void DelPett(IConsole console, uint id)
	{
		PetID key = new PetID(id);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.Pets.TryGetValue(key, out var value))
		{
			console.WriteLineWithStyle($"No pet with {id} exists in defs", CommandOutputStyle.Error);
			return;
		}
		try
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.PetController.DelPet(value));
		}
		catch (Exception ex)
		{
			console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "bring_pet", RequiredContext = typeof(FHMapChoreographer))]
	public static void BringPet(IConsole console, uint id)
	{
		if (Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario == null)
		{
			console.WriteLineWithStyle("No active scenario", CommandOutputStyle.Error);
			return;
		}
		PetID key = new PetID(id);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.Pets.TryGetValue(key, out var value))
		{
			console.WriteLineWithStyle($"No pet with {id} exists in defs", CommandOutputStyle.Error);
			return;
		}
		try
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.PetController.BringPetToScenario(value));
		}
		catch (Exception ex)
		{
			console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "remove_pet", RequiredContext = typeof(FHMapChoreographer))]
	public static void RemovePet(IConsole console, uint id)
	{
		if (Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario == null)
		{
			console.WriteLineWithStyle("No active scenario", CommandOutputStyle.Error);
			return;
		}
		PetID key = new PetID(id);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.Pets.TryGetValue(key, out var value))
		{
			console.WriteLineWithStyle($"No pet with {id} exists in defs", CommandOutputStyle.Error);
			return;
		}
		try
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.PetController.RemovePetFromScenario(value));
		}
		catch (Exception ex)
		{
			console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_town_guard_perk", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddTownGuardPerk(IConsole console, uint id, string campaignSection = null)
	{
		TownGuardPerkID key = new TownGuardPerkID(id);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardPerkDefs.ContainsKey(key))
		{
			console.WriteLineWithStyle($"No perk with id {id} exists", CommandOutputStyle.Error);
			return;
		}
		TownGuardPerkDef def = Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardPerkDefs[key];
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.AddTownGuardPerk(def, free: true));
	}

	[ConsoleCommand(Command = "show_job_postings", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowJobPostings(IConsole console)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Job postings:");
		foreach (CampaignSectionID jobPosting in Singleton<FHMapChoreographer>.Instance.GameState.Outpost.JobPostings)
		{
			bool flag = Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.IsJobPostingAccepted(jobPosting);
			bool flag2 = Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.IsJobPostingCompleted(jobPosting);
			stringBuilder.AppendLine($"ID: {jobPosting} - accepted: {flag}, completed: {flag2}");
		}
		console.Write(stringBuilder.ToString());
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_job_posting", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddJobPosting(IConsole console, string campaignSection = null)
	{
		CampaignSectionID campaignSectionID = new CampaignSectionID(campaignSection);
		if (!Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.IsValidJobPostingSectionToAdd(campaignSectionID))
		{
			console.WriteLineWithStyle($"Job posting {campaignSectionID} is not valid to add", CommandOutputStyle.Error);
		}
		else
		{
			Singleton<FHMapChoreographer>.Instance.GameState.Outpost.JobPostings.Add(campaignSectionID);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "accept_job_posting", RequiredContext = typeof(FHMapChoreographer))]
	public static void AcceptJobPosting(IConsole console, string campaignSection = null)
	{
		CampaignSectionID campaignSectionID = new CampaignSectionID(campaignSection);
		if (!Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.IsValidJobPostingSectionToAccept(campaignSectionID))
		{
			if (!Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.IsValidJobPostingSectionToAdd(campaignSectionID))
			{
				console.WriteLineWithStyle($"Job posting {campaignSectionID} is not valid to accept", CommandOutputStyle.Error);
				return;
			}
			AddJobPosting(console, campaignSection);
		}
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.AcceptJobPosting(campaignSectionID));
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_all_town_guard_perks", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddAllTownGuardPerks(IConsole console)
	{
		foreach (TownGuardPerkDef value in Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardPerkDefs.Values)
		{
			int availableTownGuardPerkCount = Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.GetAvailableTownGuardPerkCount(value);
			for (int i = 0; i < availableTownGuardPerkCount; i++)
			{
				ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.AddTownGuardPerk(value), suppressDialogues: true);
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_town_guard_points", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveTownGuardPerk(IConsole console, int amount = 1)
	{
		uint townGuardPerksPoints = (uint)Mathf.Max(0, (int)Singleton<FHMapChoreographer>.Instance.GameState.Outpost.TownGuardPerksPoints + amount);
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.TownGuardPerksPoints = townGuardPerksPoints;
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_town_guard_checks", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveTownGuardChecks(IConsole console, int amount = 1)
	{
		OutpostState outpost = Singleton<FHMapChoreographer>.Instance.GameState.Outpost;
		CampaignSettings settings = Singleton<FHMapChoreographer>.Instance.GameController.Settings;
		int num = Math.Max((int)outpost.TownGuardPerkChecks + amount, 0);
		outpost.TownGuardPerkChecks = (uint)(num % settings.TownGuardPerkChecksForPerk);
		outpost.TownGuardPerksPoints += (uint)num / settings.TownGuardPerkChecksForPerk;
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_town_guard_modifier", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveTownGuardModifier(IConsole console, string param)
	{
		DefRepository defRepo = Singleton<FHMapChoreographer>.Instance.DefRepo;
		string mod = param;
		TownGuardModType type = TownGuardModType.Modifier;
		TownGuardAttackModifierID townGuardAttackModifierID = new TownGuardAttackModifierID(mod);
		if (!defRepo.TownGuardAttackModifierDefs.ContainsKey(townGuardAttackModifierID))
		{
			if (!int.TryParse(mod, out var _))
			{
				Enum.TryParse<TownGuardModType>(mod, ignoreCase: true, out type);
			}
			townGuardAttackModifierID = defRepo.TownGuardAttackModifierDefs.SingleOrDefault((KeyValuePair<TownGuardAttackModifierID, TownGuardAttackModifierDef> x) => x.Value.Type == type && (type != TownGuardModType.Modifier || mod == x.Value.MathModifier)).Key;
		}
		if (townGuardAttackModifierID != default(TownGuardAttackModifierID))
		{
			Singleton<FHMapChoreographer>.Instance.GameState.Outpost.TownGuardDeckIDs.Insert(0, townGuardAttackModifierID);
		}
		else
		{
			console.WriteLineWithStyle("No town guard modifier " + param + " exists", CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_challenge")]
	public static void AddChallenge(IConsole console, string id_or_key)
	{
		AddOrSetChallenge(console, id_or_key, add: true);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_challenge")]
	public static void SetChallenge(IConsole console, string id_or_key)
	{
		AddOrSetChallenge(console, id_or_key, add: false);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "empty_challenge_deck")]
	public static void EmptyChallengeDeck(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.CompletedChallengeIDs.AddRange(Singleton<FHMapChoreographer>.Instance.GameState.ChallengeDeckIDs);
		Singleton<FHMapChoreographer>.Instance.GameState.ChallengeDeckIDs.Clear();
		if (Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario != null)
		{
			Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario.DrawnChallengeIDs.Clear();
			Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario.SelectedChallengeIDs.Clear();
		}
		Singleton<FHMapChoreographer>.Instance.View.RefreshView();
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "show_challenge_deck")]
	public static void ShowChallengeDeck(IConsole console)
	{
		string text = "Challenges deck:";
		if (!Singleton<FHMapChoreographer>.Instance.GameState.ChallengeDeck.Any())
		{
			text += "Empty";
		}
		foreach (ChallengeDef item in Singleton<FHMapChoreographer>.Instance.GameState.ChallengeDeck)
		{
			text = text + "\n" + item.NameLocKey;
		}
		console.WriteLineWithStyle(text, CommandOutputStyle.Default);
	}

	private static void AddOrSetChallenge(IConsole console, string id_or_key, bool add)
	{
		if (!uint.TryParse(id_or_key, out var id))
		{
			string keyToLower = id_or_key.ToLower();
			CChallenge cChallenge = CChallenge.Challenges.Values.SingleOrDefault((CChallenge x) => x.Name.ToLower().Contains(keyToLower));
			if (cChallenge != null)
			{
				id = cChallenge.ID;
			}
			else
			{
				id = 0u;
			}
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			if (!CChallenge.Challenges.ContainsKey(id))
			{
				console.WriteLineWithStyle("No challenge with " + id_or_key + " id exists", CommandOutputStyle.Error);
				return;
			}
			if (ScenarioManager.CurrentScenarioState.Challenges.Any((CChallenge x) => x.ID == id))
			{
				console.WriteLineWithStyle($"{id} already present in challenges", CommandOutputStyle.Error);
				return;
			}
			ScenarioManager.CurrentScenarioState.AddChallenge(id);
			CChallenge.CheckTrigger(CChallenge.Trigger.OnStartScenario);
			console.WriteLineWithStyle($"Added {CChallenge.Challenges[id]} to challenges", CommandOutputStyle.Info);
		}
		if (Singleton<FHMapChoreographer>.Instance == null)
		{
			return;
		}
		if (Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario == null)
		{
			console.WriteLineWithStyle("No active scenario", CommandOutputStyle.Error);
			return;
		}
		ChallengeID challengeID = new ChallengeID(id);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.Challenges.ContainsKey(challengeID))
		{
			console.WriteLineWithStyle($"No challenge with {id} exists", CommandOutputStyle.Error);
			return;
		}
		if (!add)
		{
			Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario.DrawnChallengeIDs.Clear();
			Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario.SelectedChallengeIDs.Clear();
		}
		Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario.DrawnChallengeIDs.Add(challengeID);
		Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario.SelectedChallengeIDs.Add(challengeID);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "maximum_power", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveAllUnloockAll(IConsole console)
	{
		GiveAndMaxParty(console);
		GiveAllResources(console);
		BuildAllBuildings(console);
		UnlockAllScenarios(console);
		UnlockAllItems(console);
		UnlockAllRecipies(console);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "maximum_party", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveAndMaxParty(IConsole console)
	{
		foreach (CharacterClassDef def in Singleton<FHMapChoreographer>.Instance.DefRepo.CharacterClasses.Values)
		{
			if (def.OnlyValidInLevels == null)
			{
				Singleton<FHMapChoreographer>.Instance.GameState.UnlockedClassIDs.AddNotContained(def.ID);
				if (!Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters.Any((CharacterState c) => c.ClassDef == def))
				{
					CharacterState characterState = new CharacterState(def, Singleton<FHMapChoreographer>.Instance.DefRepo, CharacterID.NewID());
					characterState.Name = def.Name;
					characterState.PartyState = CharacterPartyStateType.Removed;
					Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters.Add(characterState);
				}
			}
		}
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.Cheat_AddExperience(allCharacter, (int)Singleton<FHMapChoreographer>.Instance.GameState.GameMode.CharacterExperienceTable.Last());
			allCharacter.PerkIDs.Clear();
			allCharacter.PerkIDs.AddRange(allCharacter.ClassDef.Perks);
			allCharacter.UnusedCardIDs.Clear();
			allCharacter.UnusedCardIDs.AddRange(allCharacter.PassedCards.Select((ClassCardDef s) => s.ID));
			allCharacter.CardUnlocks = 0u;
			allCharacter.SelectedCardIDs.Clear();
			allCharacter.SelectedCardIDs.AddRange(allCharacter.UnusedCardIDs.Take((int)allCharacter.ClassDef.MaxCards));
			allCharacter.UnusedCardIDs.RemoveRange(0, (int)allCharacter.ClassDef.MaxCards);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_all_classes", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockAllClasses(IConsole console)
	{
		foreach (CharacterClassDef value in Singleton<FHMapChoreographer>.Instance.DefRepo.CharacterClasses.Values)
		{
			if (value.OnlyValidInLevels == null)
			{
				Singleton<FHMapChoreographer>.Instance.GameState.UnlockedClassIDs.AddNotContained(value.ID);
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_class", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockClass(IConsole console, string classId)
	{
		CharacterClassDef def = Singleton<FHMapChoreographer>.Instance.DefRepo.CharacterClasses.Values.FirstOrDefault((CharacterClassDef characterClassDef) => characterClassDef.ID.ID.Contains(classId, StringComparison.InvariantCultureIgnoreCase));
		if (def == null)
		{
			console.WriteLineWithStyle(classId + " is not a valid Hero class id", CommandOutputStyle.Default);
			return;
		}
		if (Singleton<FHMapChoreographer>.Instance.GameState.UnlockedClassIDs.Any((CharacterClassID id) => id == def.ID))
		{
			console.WriteLineWithStyle(classId + " is already unlocked", CommandOutputStyle.Default);
			return;
		}
		Singleton<FHMapChoreographer>.Instance.GameState.UnlockedClassIDs.AddNotContained(def.ID);
		console.WriteLineWithStyle(def.ID.ID + " unlocked", CommandOutputStyle.Default);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_puzzle", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockAllClasses(IConsole console, string puzzleKey)
	{
		PuzzleID puzzleID = new PuzzleID(puzzleKey);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.Puzzles.TryGetValue(puzzleID, out var def))
		{
			console.WriteLineWithStyle($"{puzzleID} is not a valid puzzle", CommandOutputStyle.Warning);
		}
		else if (Singleton<FHMapChoreographer>.Instance.GameController.PuzzleController.CompletedPuzzles.Any((PuzzleState x) => x.Def == def))
		{
			console.WriteLineWithStyle($"{puzzleID} is already completed", CommandOutputStyle.Warning);
		}
		else if (Singleton<FHMapChoreographer>.Instance.GameController.PuzzleController.UnlockedPuzzles.Any((PuzzleState x) => x.Def == def))
		{
			console.WriteLineWithStyle($"{puzzleID} is already completed", CommandOutputStyle.Warning);
		}
		else
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.UnlockPuzzle, puzzleKey)));
		}
	}

	[ConsoleCommand(Command = "show_puzzles", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowPuzzleProgress(IConsole console)
	{
		console.WriteLine("Completed puzzles: ");
		foreach (PuzzleState completedPuzzle in Singleton<FHMapChoreographer>.Instance.GameController.PuzzleController.CompletedPuzzles)
		{
			console.WriteLine($" - {completedPuzzle.Def.Name} solved in {Singleton<FHMapChoreographer>.Instance.GameController.PuzzleController.CalcPuzzleSolveDuration(completedPuzzle)} weeks");
		}
		console.WriteLine("Incomplete puzzles: ");
		foreach (PuzzleState incompletePuzzle in Singleton<FHMapChoreographer>.Instance.GameController.PuzzleController.IncompletePuzzles)
		{
			console.Write(incompletePuzzle.Def.Name + ", ");
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "solve_puzzle", RequiredContext = typeof(FHMapChoreographer))]
	public static void SolvePuzzle(IConsole console, string puzzleKey = null, bool cheat = false)
	{
		PuzzleState puzzleState = null;
		puzzleState = (string.IsNullOrEmpty(puzzleKey) ? Singleton<FHMapChoreographer>.Instance.View.GameController.PuzzleController.IncompletePuzzles.LastOrDefault() : Singleton<FHMapChoreographer>.Instance.View.GameController.PuzzleController.IncompletePuzzles.SingleOrDefault((PuzzleState x) => x.ID.ID.Contains(puzzleKey, StringComparison.InvariantCultureIgnoreCase)));
		if (puzzleState != null)
		{
			if (!cheat && !Singleton<FHMapChoreographer>.Instance.View.GameController.PuzzleController.CanSolvePuzzle(puzzleState))
			{
				console.WriteLine($"{puzzleState.Def} is not ready to be solved yet");
			}
			else
			{
				ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.View.GameController.PuzzleController.SolvePuzzle(puzzleState.Def, cheat));
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_recipies", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockAllRecipies(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.DiscoveredBrewableItemIDs.Clear();
		List<ItemDef> list = Singleton<FHMapChoreographer>.Instance.DefRepo.Items.Values.Where((ItemDef i) => i.CanBeBrewed).ToList();
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.DiscoveredBrewableItemIDs.AddRange(list.Select((ItemDef i) => i.ID));
		foreach (ItemDef def in list)
		{
			if (def.IsUnlimitedQuantity && Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.All((ItemID id) => id != def.ID))
			{
				Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Add(def.ID);
				continue;
			}
			for (int num = Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Count((ItemID i) => i == def.ID); num < def.Quantity; num++)
			{
				Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Add(def.ID);
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "show_me_the_money", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveAllResources(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Resources += new OutpostResources
		{
			Arrowvine = 100,
			Axenut = 100,
			Snowthistle = 100,
			Flamefruit = 100,
			Corpsecap = 100,
			Rockroot = 100
		};
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Morale = 10;
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.ProsperityLevel = 9u;
		Singleton<FHMapChoreographer>.Instance.GameState.Inspiration = 15u;
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			allCharacter.Gold += 100u;
			allCharacter.Resources += new OutpostResources
			{
				Hide = 100,
				Metal = 100,
				Lumber = 100
			};
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "reset_resources", RequiredContext = typeof(FHMapChoreographer))]
	public static void ResetAllResources(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Resources -= Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Resources;
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Morale = 0;
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			allCharacter.Gold = 0u;
			allCharacter.Resources -= allCharacter.Resources;
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_material", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveMaterial(IConsole console, OutpostMaterialType material, int amount)
	{
		OutpostResources outpostResources = new OutpostResources(material, amount);
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			allCharacter.Resources += outpostResources;
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_herb", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveHerb(IConsole console, OutpostHerbType herb, int amount)
	{
		OutpostResources outpostResources = new OutpostResources(herb, amount);
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Resources += outpostResources;
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_perk", RequiredContext = typeof(FHMapChoreographer))]
	public static void GivePerk(IConsole console, int amount = 1)
	{
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			allCharacter.PerkMarks = (uint)Mathf.Max(0, (int)allCharacter.PerkMarks + amount);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_perk_checks", RequiredContext = typeof(FHMapChoreographer))]
	public static void GivePerkChecks(IConsole console, int amount = 1)
	{
		CampaignSettings settings = Singleton<FHMapChoreographer>.Instance.GameController.Settings;
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			int num = Math.Max((int)allCharacter.PerkChecks + amount, 0);
			allCharacter.PerkChecks = (uint)(num % settings.PerkChecksPerMark);
			allCharacter.PerkMarks += (uint)num / settings.PerkChecksPerMark;
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_gold", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveGold(IConsole console, int amount)
	{
		foreach (CharacterState allCharacter in Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters)
		{
			allCharacter.Gold = (uint)Mathf.Max(0, (int)allCharacter.Gold + amount);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "build_all", RequiredContext = typeof(FHMapChoreographer))]
	public static void BuildAllBuildings(IConsole console, int targetLevel = -1)
	{
		if (!Singleton<FHMapChoreographer>.Instance.GameState.PartyCharacters.Any())
		{
			console.WriteLineWithStyle("At least one Hero must be present in the party to execute this command", CommandOutputStyle.Warning);
			return;
		}
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.FreeConstruct = true;
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.InstantConstruct = true;
		try
		{
			foreach (BuildingTypeDef def in Singleton<FHMapChoreographer>.Instance.DefRepo.Buildings.Values)
			{
				BuildingState buildingState = Singleton<FHMapChoreographer>.Instance.GameController.State.Outpost.Buildings.FirstOrDefault((BuildingState b) => b.Def == def);
				if (!Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Buildings.Any((BuildingState b) => b.Def == def))
				{
					ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.BuildBuilding(def, null), suppressDialogues: true);
					buildingState = Singleton<FHMapChoreographer>.Instance.GameController.State.Outpost.Buildings.First((BuildingState b) => b.Def == def);
				}
				if (buildingState.Wrecked)
				{
					ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.Rebuild(def, null), suppressDialogues: true);
				}
				int num = ((targetLevel == -1) ? ((int)buildingState.Def.Levels) : Mathf.Min(targetLevel, (int)buildingState.Def.Levels));
				for (int num2 = (int)(buildingState.Level + 1); num2 <= num; num2++)
				{
					ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.UpgradeBuilding(buildingState, null, null), suppressDialogues: true);
				}
			}
		}
		catch (Exception ex)
		{
			console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
		}
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.FreeConstruct = false;
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.InstantConstruct = false;
	}

	[ConsoleCommand(Command = "show_all_loot_cards")]
	public static void ShowAllLootCards(IConsole console)
	{
		if (Singleton<FHMapChoreographer>.Instance != null)
		{
			foreach (LootCardState allLootCard in Singleton<FHMapChoreographer>.Instance.GameState.AllLootCards)
			{
				console.WriteLine(allLootCard.ToString());
			}
			return;
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			LootCard[] allCards = CLootDeck.AllCards;
			for (int i = 0; i < allCards.Length; i++)
			{
				LootCard lootCard = allCards[i];
				console.WriteLine(lootCard.ToString());
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "build", RequiredContext = typeof(FHMapChoreographer))]
	public static void BuildBuilding(IConsole console, string id, int level = 1)
	{
		if (!Singleton<FHMapChoreographer>.Instance.GameState.PartyCharacters.Any())
		{
			console.WriteLineWithStyle("At least one Hero must be present in the party to execute this command", CommandOutputStyle.Warning);
			return;
		}
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.FreeConstruct = true;
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.InstantConstruct = true;
		BuildingTypeDef def = Singleton<FHMapChoreographer>.Instance.DefRepo.Buildings.FirstOrDefault((KeyValuePair<BuildingTypeID, BuildingTypeDef> b) => b.Value.Name.ToLower().Contains(id.ToLower())).Value;
		BuildingState buildingState = Singleton<FHMapChoreographer>.Instance.GameController.State.Outpost.Buildings.SingleOrDefault((BuildingState b) => b.Def == def);
		try
		{
			if (buildingState == null)
			{
				ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.BuildBuilding(def, null), suppressDialogues: true);
				buildingState = Singleton<FHMapChoreographer>.Instance.GameController.State.Outpost.Buildings.First((BuildingState b) => b.Def == def);
			}
			if (buildingState.Wrecked)
			{
				ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.Rebuild(def, null), suppressDialogues: true);
			}
			for (int num = (int)(buildingState.Level + 1); num <= level; num++)
			{
				ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.BuildingsController.UpgradeBuilding(buildingState, null, null), suppressDialogues: true);
			}
		}
		catch (Exception ex)
		{
			console.WriteLineWithStyle(ex.ToString(), CommandOutputStyle.Error);
		}
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.FreeConstruct = false;
		Singleton<FHMapChoreographer>.Instance.GameController.Settings.InstantConstruct = false;
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "wreck_all", RequiredContext = typeof(FHMapChoreographer))]
	public static void WreckAllBuildings(IConsole console)
	{
		foreach (BuildingState item in Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Buildings.Where((BuildingState b) => b.CanBeDamaged))
		{
			item.Wrecked = true;
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "wreck", RequiredContext = typeof(FHMapChoreographer))]
	public static void WreckBuilding(IConsole console, string id)
	{
		BuildingTypeDef def = Singleton<FHMapChoreographer>.Instance.DefRepo.Buildings.FirstOrDefault((KeyValuePair<BuildingTypeID, BuildingTypeDef> b) => b.Value.Name.ToLower().Contains(id.ToLower())).Value;
		if (def == null)
		{
			console.WriteLine("Invalid building type " + id);
			return;
		}
		if (def.CannotBeDamaged)
		{
			console.WriteLine(id + " can not be damages");
			return;
		}
		foreach (BuildingState item in Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Buildings.Where((BuildingState b) => b.Def == def))
		{
			item.Wrecked = true;
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "scenario_unlock_all", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockAllScenarios(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Scenarios.Values.ForEach((ScenarioDef def) =>
		{
			if (!Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Any((FHRL.ScenarioState s) => s.Def == def))
			{
				Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Add(new FHRL.ScenarioState(def, Singleton<FHMapChoreographer>.Instance.GameController.DefRepository));
			}
		});
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "scenario_unlock", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockScenario(IConsole console, int id)
	{
		Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Scenarios.Values.Where((ScenarioDef i) => i.ID.ID == (uint)id).ForEach((ScenarioDef def) =>
		{
			if (!Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Any((FHRL.ScenarioState s) => s.Def == def))
			{
				Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Add(new FHRL.ScenarioState(def, Singleton<FHMapChoreographer>.Instance.GameController.DefRepository));
			}
		});
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "auto_battle", RequiredContext = typeof(FHMapChoreographer))]
	public static void ToggleAutoBattle(IConsole console)
	{
		_autoBattle = !_autoBattle;
		console.WriteLine($"AutoBattle set to {_autoBattle}");
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "items_unlock_all", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockAllItems(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffects(Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items.Keys.Select((ItemID s) => new CampaignEffect(CampaignEffectType.AddItemsToShop, (int)s.ID)));
		Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.BuyItems, 1));
		Singleton<FHMapChoreographer>.Instance.GameController.State.AdditionalActiveEffects.Add(new CampaignEffect(CampaignEffectType.BuyItems, 1));
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "section_activate", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockAllItems(IConsole console, string sectionID)
	{
		if (Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.CampaignSections.Values.FirstOrDefault((CampaignSectionDef i) => i.ID.ID == sectionID) == null)
		{
			throw new Exception("Section with ID '" + sectionID + "' not found");
		}
		Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.ActivateCampaignSection, sectionID));
	}

	[ConsoleCommand(Command = "refresh_view", RequiredContext = typeof(FHMapChoreographer))]
	public static void RefreshView(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.View.RefreshView();
	}

	[ConsoleCommand(Command = "reset_view", RequiredContext = typeof(FHMapChoreographer))]
	public static void ResetView(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.View.ResetView();
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "create_party", RequiredContext = typeof(FHMapChoreographer))]
	public static void CreateDefaultParty_Cheat(IConsole console, int partySize = 2)
	{
		Singleton<FHMapChoreographer>.Instance.CreateDefaultParty(partySize);
		Singleton<FHMapChoreographer>.Instance.View.RefreshView();
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_exp", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveExperience(IConsole console, int experience)
	{
		foreach (CharacterState partyCharacter in Singleton<FHMapChoreographer>.Instance.GameState.PartyCharacters)
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.Cheat_AddExperience(partyCharacter, experience));
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "reset_masteries", RequiredContext = typeof(FHMapChoreographer))]
	public static void ResetMasteries(IConsole console)
	{
		foreach (CharacterState partyCharacter in Singleton<FHMapChoreographer>.Instance.GameState.PartyCharacters)
		{
			partyCharacter.CompletedMasteryIDs.Clear();
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "complete_masteries", RequiredContext = typeof(FHMapChoreographer))]
	public static void CompleteMasteries(IConsole console)
	{
		foreach (CharacterState partyCharacter in Singleton<FHMapChoreographer>.Instance.GameState.PartyCharacters)
		{
			foreach (MasteryDef item in Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.GetUncompletedMasteries(partyCharacter).ToList())
			{
				partyCharacter.CompletedMasteryIDs.Add(item.ID);
				Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.AddPerkMarks(partyCharacter, item.PerkMarksReward);
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "next_week", RequiredContext = typeof(FHMapChoreographer))]
	public static void NextWeek(IConsole console)
	{
		if (Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Phase != OutpostPhase.Downtime)
		{
			console.WriteLineWithStyle("Trying to advance week outside of the downtime phase", CommandOutputStyle.Error);
			return;
		}
		Singleton<FHMapChoreographer>.Instance.GameState.NextScenarioModifiers = new NextScenarioGlobalModifiers();
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.NextPhase());
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_week", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetWeek(IConsole console, uint week)
	{
		if (Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Phase != OutpostPhase.Downtime)
		{
			console.WriteLineWithStyle("Trying to advance week outside of the downtime phase", CommandOutputStyle.Error);
			return;
		}
		Singleton<FHMapChoreographer>.Instance.GameState.NextScenarioModifiers = new NextScenarioGlobalModifiers();
		Singleton<FHMapChoreographer>.Instance.View.GameState.Outpost.Week = week - 1;
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.NextPhase());
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_item", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveItem(IConsole console, int itemId)
	{
		ItemDef itemDef = Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items.Values.FirstOrDefault((ItemDef i) => i.ID.ID == (uint)itemId);
		if (itemDef == null)
		{
			itemDef = Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items.Values.FirstOrDefault((ItemDef i) => i.ScenarioItemID == (uint)itemId);
		}
		if (itemDef == null)
		{
			console.WriteLineWithStyle($"No item with ID {itemId} found", CommandOutputStyle.Error);
			return;
		}
		foreach (CharacterState partyCharacter in Singleton<FHMapChoreographer>.Instance.GameController.State.PartyCharacters)
		{
			if (Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.CanHaveItem(partyCharacter, itemDef) && Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.HasFreeItemSlot(partyCharacter, itemDef))
			{
				partyCharacter.EquipedItemIDs.Add(itemDef.ID);
				console.Write("Equipped ");
				console.WriteWithStyle(itemDef.Name, CommandOutputStyle.Highlight);
				console.Write(" to ");
				console.WriteWithStyle(partyCharacter.Name, CommandOutputStyle.Highlight);
				return;
			}
		}
		console.WriteLineWithStyle($"No valid character found to equip {itemId}", CommandOutputStyle.Error);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "give_all_items", RequiredContext = typeof(FHMapChoreographer))]
	public static void GiveAllItems(IConsole console)
	{
		foreach (CharacterState partyCharacter in Singleton<FHMapChoreographer>.Instance.GameController.State.PartyCharacters)
		{
			foreach (ItemDef value in Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items.Values)
			{
				if (Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.CanHaveItem(partyCharacter, value))
				{
					if (Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.HasFreeItemSlot(partyCharacter, value))
					{
						partyCharacter.EquipedItemIDs.Add(value.ID);
					}
					else
					{
						partyCharacter.TuckedItemIDs.Add(value.ID);
					}
				}
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_blueprint", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockBlueprint(IConsole console, int itemId)
	{
		ItemDef itemDef = Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items.Values.FirstOrDefault((ItemDef i) => i.ID.ID == (uint)itemId);
		if (itemDef == null)
		{
			itemDef = Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items.Values.FirstOrDefault((ItemDef i) => i.ScenarioItemID == (uint)itemId);
		}
		if (itemDef == null)
		{
			console.WriteLineWithStyle($"No item with ID {itemId} found", CommandOutputStyle.Error);
		}
		else if (!itemDef.CanBeCrafted)
		{
			console.WriteLineWithStyle($"{itemId} is not a blueprint item", CommandOutputStyle.Error);
		}
		else if (!Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Contains(itemDef.ID))
		{
			for (int num = 0; num < itemDef.Quantity; num++)
			{
				Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Add(itemDef.ID);
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_blueprints", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockBlueprint(IConsole console)
	{
		foreach (KeyValuePair<ItemID, ItemDef> item in Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.Items)
		{
			ItemDef value = item.Value;
			if (value.CanBeCrafted && !Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Contains(value.ID))
			{
				for (int i = 0; i < value.Quantity; i++)
				{
					Singleton<FHMapChoreographer>.Instance.GameState.Outpost.CraftableItemIDs.Add(value.ID);
				}
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_soldiers", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddSoldiers(IConsole console, int amount)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Soldiers = (uint)Math.Max(0, (int)Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Soldiers + amount);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_inspiration", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddInspiration(IConsole console, int amount)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Inspiration = (uint)Math.Max(0, (int)Singleton<FHMapChoreographer>.Instance.GameState.Inspiration + amount);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "add_morale", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddMorale(IConsole console, int amount)
	{
		if (amount != 0)
		{
			ProcessResultFromCommand((amount > 0) ? Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.GainMorale, amount)) : Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.LoseMorale, -amount)));
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_prosperity", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetProsperityLevel(IConsole console, int level)
	{
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.ProsperityLevel = (uint)Math.Max(0, level);
	}

	[ConsoleCommand(Command = "add_prosperity", RequiredContext = typeof(FHMapChoreographer))]
	public static void AddProsperityLevel(IConsole console, int prosperity)
	{
		ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.OutpostController.Cheat_AddProspoerity(prosperity));
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_benchmark", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockBenchmarkScenarios(IConsole console)
	{
		int[] array = new int[3] { 66, 98, 666 };
		foreach (int id in array)
		{
			if (Singleton<FHMapChoreographer>.Instance.DefRepo.Scenarios.TryGetValue(new ScenarioID((uint)id), out var def) && !Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Any((FHRL.ScenarioState s) => s.Def == def))
			{
				Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Add(new FHRL.ScenarioState(def, Singleton<FHMapChoreographer>.Instance.DefRepo));
			}
		}
	}

	[ConsoleCommand(Command = "net_sync_state", RequiredContext = typeof(FHMapChoreographer))]
	public static void SyncState(IConsole console)
	{
		if (!FFSNetwork.IsOnline)
		{
			console.WriteLineWithStyle("This command can be executed only in multiplayer", CommandOutputStyle.Error);
		}
		else if (FFSNetwork.IsHost)
		{
			Singleton<FHMapChoreographer>.Instance.GameServer.ReplaceState(Singleton<FHMapChoreographer>.Instance.GameClient.GameState);
			Singleton<FHMapChoreographer>.Instance.SendGameState();
		}
		else
		{
			Singleton<FHMapChoreographer>.Instance.RequestGameState();
		}
	}

	[ConsoleCommand(Command = "town_guard_deck", RequiredContext = typeof(FHMapChoreographer))]
	public static void PrintTownGuardDeck(IConsole console)
	{
		int num = 0;
		foreach (TownGuardAttackModifierDef item in Singleton<FHMapChoreographer>.Instance.GameState.Outpost.TownGuardDeck)
		{
			CommandOutputStyle style = CommandOutputStyle.Default;
			if (num < 2)
			{
				style = CommandOutputStyle.Highlight;
			}
			if (item.Type == TownGuardModType.Modifier)
			{
				string mathModifier = item.MathModifier;
				console.WriteWithStyle(mathModifier ?? "", style);
			}
			else
			{
				console.WriteWithStyle($"{item.Type}", style);
			}
			if (item.Rolling)
			{
				console.WriteWithStyle(" R", style);
			}
			foreach (CampaignEffect effect in item.Effects)
			{
				console.WriteWithStyle($" {effect}", style);
			}
			console.WriteWithStyle(", ", style);
			num++;
		}
	}

	[ConsoleCommand(Command = "list_pesonal_quests", RequiredContext = typeof(FHMapChoreographer))]
	public static void PrintPersonalQuests(IConsole console)
	{
		foreach (PersonalQuestDef d in Singleton<FHMapChoreographer>.Instance.DefRepo.PersonalQuests.Values)
		{
			CharacterState characterState = Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.AllCharacters.FirstOrDefault((CharacterState c) => c.PersonalQuestDef == d);
			MultiplayerGameState.Player player = Singleton<FHMapChoreographer>.Instance.GameClient.GameState.Players.FirstOrDefault((MultiplayerGameState.Player p) => p.DrawnPersonalQuests.Contains(d.ID));
			string arg = ((characterState == null) ? "In deck" : $"In character '{characterState.ClassID}'");
			if (player != null)
			{
				arg = $"In player '{PlayerRegistry.GetPlayer(player.ID)}'";
			}
			console.WriteLine($"'{d.ID}, {d.Name}', {arg}");
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "event_trigger", RequiredContext = typeof(FHMapChoreographer))]
	public static void TriggerEvent(IConsole console, string eventID)
	{
		CampaignSectionDef value2;
		if (Singleton<FHMapChoreographer>.Instance.DefRepo.OutpostEvents.TryGetValue(new OutpostEventID(eventID), out var _))
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.EnableEvent, eventID)));
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.ActivateEvent, eventID)));
			OutpostEventState eventToResolve = Singleton<FHMapChoreographer>.Instance.GameController.OutpostEventController.EventToResolve;
			if (eventToResolve != null)
			{
				eventToResolve.DontWaitForTravel = true;
			}
		}
		else if (Singleton<FHMapChoreographer>.Instance.DefRepo.CampaignSections.TryGetValue(new CampaignSectionID(eventID), out value2))
		{
			Singleton<FHMapChoreographer>.Instance.GameState.ActivatedCampaignSectionIDs.Remove(value2.ID);
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.ApplyEffect(new CampaignEffect(CampaignEffectType.ActivateCampaignSection, eventID)));
		}
		else
		{
			console.WriteLineWithStyle("Event with id '" + eventID + "' not found", CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_next_event", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetNextEvent(IConsole console, string eventID)
	{
		OutpostEventID outpostEventID = new OutpostEventID(eventID);
		if (Singleton<FHMapChoreographer>.Instance.DefRepo.OutpostEvents.TryGetValue(outpostEventID, out var value))
		{
			List<OutpostEventID> eventDeck = Singleton<FHMapChoreographer>.Instance.GameController.OutpostEventController.GetEventDeck(value);
			if (eventDeck != null)
			{
				eventDeck.Remove(outpostEventID);
				eventDeck.Insert(0, outpostEventID);
				console.WriteLine("Newly ordered events: " + string.Join(", ", eventDeck));
			}
			else
			{
				console.WriteLineWithStyle("Event '" + eventID + "' is invalid", CommandOutputStyle.Error);
			}
		}
		else
		{
			console.WriteLineWithStyle("Event with id '" + eventID + "' not found", CommandOutputStyle.Error);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_next_town_guard_modifier", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetNextTownGuardModifier(IConsole console, string modifierID)
	{
		TownGuardAttackModifierID? townGuardAttackModifierID = new TownGuardAttackModifierID(modifierID);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardAttackModifierDefs.ContainsKey(townGuardAttackModifierID.Value))
		{
			townGuardAttackModifierID = Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardAttackModifierDefs.Values.SingleOrDefault((TownGuardAttackModifierDef x) => x.MathModifier == modifierID)?.ID;
			if (!townGuardAttackModifierID.HasValue)
			{
				string toLower = modifierID.ToLower();
				townGuardAttackModifierID = Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardAttackModifierDefs.Values.SingleOrDefault((TownGuardAttackModifierDef x) => x.Type.ToString().ToLower().Contains(toLower))?.ID;
			}
		}
		if (townGuardAttackModifierID.HasValue && Singleton<FHMapChoreographer>.Instance.DefRepo.TownGuardAttackModifierDefs.TryGetValue(townGuardAttackModifierID.Value, out var _))
		{
			List<TownGuardAttackModifierID> townGuardDeckIDs = Singleton<FHMapChoreographer>.Instance.GameState.Outpost.TownGuardDeckIDs;
			if (townGuardDeckIDs != null)
			{
				townGuardDeckIDs.Remove(townGuardAttackModifierID.Value);
				townGuardDeckIDs.Insert(0, townGuardAttackModifierID.Value);
				PrintTownGuardDeck(console);
			}
		}
		else
		{
			console.WriteLineWithStyle("Modifier with id or value '" + modifierID + "' not found", CommandOutputStyle.Error);
		}
	}

	[ConsoleCommand(Command = "event_list_deck", RequiredContext = typeof(FHMapChoreographer))]
	public static void ListEventDecks(IConsole console, string deck = null)
	{
		if (!string.IsNullOrEmpty(deck))
		{
			deck = deck.ToUpper();
			switch (deck)
			{
			case "SO":
				console.WriteLine("Summer oupost events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.SummerOutpostEventDeckIDs));
				break;
			case "SR":
				console.WriteLine("Summer road events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.SummerRoadEventDeckIDs));
				break;
			case "WO":
				console.WriteLine("Winter oupost events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.WinterOutpostEventDeckIDs));
				break;
			case "WR":
				console.WriteLine("Winter road events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.WinterRoadEventDeckIDs));
				break;
			case "B":
				console.WriteLine("Boat events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.BoatEventDeckIDs));
				break;
			default:
				console.WriteLine("Invalid deck: " + deck);
				break;
			}
		}
		else
		{
			console.WriteLine("Summer oupost events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.SummerOutpostEventDeckIDs));
			console.WriteLine("Winter oupost events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.WinterOutpostEventDeckIDs));
			console.WriteLine("Summer road events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.SummerRoadEventDeckIDs));
			console.WriteLine("Winter road events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.WinterRoadEventDeckIDs));
			console.WriteLine("Boat events: " + string.Join(", ", Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.BoatEventDeckIDs));
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "scenario_level_modifier", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetScenarioLevelModifier(IConsole console, int mod)
	{
		Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Settings.ScenarioLevelModifier = mod;
		console.WriteLine($"Scenario level modifier set to {Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Settings.ScenarioLevelModifier}");
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "effect_apply", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetScenarioLevelModifier(IConsole console, CampaignEffectType type, string param1 = null, string param2 = null, string param3 = null)
	{
		Singleton<FHMapChoreographer>.Instance.GameClient.GameController.CampaignController.ApplyEffect(new CampaignEffect(type, param1, param2, param3));
	}

	[ConsoleCommand(Command = "stickers_info", RequiredContext = typeof(FHMapChoreographer))]
	public static void PrintStickers(IConsole console)
	{
		console.WriteLine(string.Join(", ", from s in Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Stickers
			orderby s.ID
			select s.ID));
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_next_random_item", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetNextRandomItem(IConsole console, int itemId)
	{
		ItemDef itemDef = Singleton<FHMapChoreographer>.Instance.DefRepo.Items[new ItemID((uint)itemId)];
		Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.RandomItemDeckIDs.Insert(0, itemDef.ID);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_next_personal_quest", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetNextPersonalQuest(IConsole console, string personalQuestID)
	{
		PersonalQuestID key = new PersonalQuestID(personalQuestID);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.PersonalQuests.TryGetValue(key, out var value))
		{
			value = Singleton<FHMapChoreographer>.Instance.DefRepo.PersonalQuests.Values.FirstOrDefault((PersonalQuestDef v) => v.Name.StartsWith(personalQuestID));
		}
		if (value == null)
		{
			console.WriteLineWithStyle("Inavlid personal quest id: '" + personalQuestID + "'", CommandOutputStyle.Error);
			return;
		}
		console.WriteLine($"Personal quest '{value.ID}' set as first");
		Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.PersonalQuestDeckIDs.Remove(value.ID);
		Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.PersonalQuestDeckIDs.Insert(0, value.ID);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_buildings", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockBuildings(IConsole console)
	{
		foreach (KeyValuePair<BuildingTypeID, BuildingTypeDef> building in Singleton<FHMapChoreographer>.Instance.DefRepo.Buildings)
		{
			if (!Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.AvailableBuildingToConstructIDs.Contains(building.Key))
			{
				Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.AvailableBuildingToConstructIDs.Add(building.Key);
			}
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "unlock_building", RequiredContext = typeof(FHMapChoreographer))]
	public static void UnlockBuildings(IConsole console, int id)
	{
		BuildingTypeDef building = Singleton<FHMapChoreographer>.Instance.DefRepo.Buildings.FirstOrDefault((KeyValuePair<BuildingTypeID, BuildingTypeDef> b) => b.Key.ID == id).Value;
		if (building != null && !Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Buildings.Any((BuildingState b) => b.ID == building.ID) && !Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.AvailableBuildingToConstructIDs.Contains(building.ID))
		{
			Singleton<FHMapChoreographer>.Instance.GameClient.GameState.CampaignState.Outpost.AvailableBuildingToConstructIDs.Add(building.ID);
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_battle_goal", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetBattleGoal(IConsole console, string battleGoalIDKey, string classID = null)
	{
		BattleGoalID key = new BattleGoalID(battleGoalIDKey);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.BattleGoals.TryGetValue(key, out var value))
		{
			value = Singleton<FHMapChoreographer>.Instance.DefRepo.BattleGoals.Values.FirstOrDefault((BattleGoalDef v) => v.ID.ID.ToLower().Contains(battleGoalIDKey.ToLower()));
			if (value == null)
			{
				console.WriteLineWithStyle("Invalid battle goal id: '" + battleGoalIDKey + "'", CommandOutputStyle.Error);
				return;
			}
		}
		if (Singleton<FHMapChoreographer>.Instance.GameState?.ActiveScenario == null)
		{
			console.WriteLineWithStyle("No active scenario'", CommandOutputStyle.Error);
			return;
		}
		if (string.IsNullOrEmpty(classID) && Singleton<FHMapChoreographer>.Instance.View?.SelectedCharacter != null)
		{
			classID = Singleton<FHMapChoreographer>.Instance.View.SelectedCharacter.ClassID.ID;
		}
		CharacterState characterState = Singleton<FHMapChoreographer>.Instance.GameState.PartyCharacters.SingleOrDefault((CharacterState x) => x.ClassID.ID.ToLower().Contains(classID.ToLower()));
		if (characterState == null)
		{
			console.WriteLineWithStyle("No selected character'", CommandOutputStyle.Error);
		}
		else
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.ScenarioController.SelectCharacterBattleGoal(characterState.ID, value, overrideDrawn: true));
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "set_personal_quest", RequiredContext = typeof(FHMapChoreographer))]
	public static void SetPersonalQuest(IConsole console, string personalQuestID)
	{
		PersonalQuestID key = new PersonalQuestID(personalQuestID);
		if (!Singleton<FHMapChoreographer>.Instance.DefRepo.PersonalQuests.TryGetValue(key, out var value))
		{
			value = Singleton<FHMapChoreographer>.Instance.DefRepo.PersonalQuests.Values.FirstOrDefault((PersonalQuestDef v) => v.Name.StartsWith(personalQuestID));
		}
		if (value == null)
		{
			console.WriteLineWithStyle("Invalid personal quest id: '" + personalQuestID + "'", CommandOutputStyle.Error);
			return;
		}
		CharacterState characterState = Singleton<FHMapChoreographer>.Instance.View?.SelectedCharacter;
		if (characterState == null)
		{
			console.WriteLineWithStyle("No selected character", CommandOutputStyle.Error);
		}
		else
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.SelectPersonalQuest(characterState.ID, value, isRetiring: false, ignoreGameState: true));
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "complete_personal_quest", RequiredContext = typeof(FHMapChoreographer))]
	public static void CompletePersonalQuest(IConsole console)
	{
		CharacterState characterState = Singleton<FHMapChoreographer>.Instance.View?.SelectedCharacter;
		if (characterState == null)
		{
			console.WriteLineWithStyle("No selected character", CommandOutputStyle.Error);
		}
		else
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.Cheat_CompletePersonalQuest(characterState));
		}
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "increment_personal_quest", RequiredContext = typeof(FHMapChoreographer))]
	public static void IncrementPersonalQuest(IConsole console, string incType = null, uint amount = 1u)
	{
		CharacterState characterState = Singleton<FHMapChoreographer>.Instance.View?.SelectedCharacter;
		if (characterState == null)
		{
			console.WriteLineWithStyle("No selected character", CommandOutputStyle.Error);
			return;
		}
		if (characterState.PersonalQuest == null)
		{
			console.WriteLineWithStyle("No personal quest", CommandOutputStyle.Error);
			return;
		}
		if (!Enum.TryParse<PersonalQuestDef.IncrementType>(incType, out var result))
		{
			result = characterState.PersonalQuestDef.WhenToIncrement().FirstOrDefault();
		}
		if (result != PersonalQuestDef.IncrementType.None)
		{
			Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.Cheat_IncrementPersonalQuest(characterState, result, amount);
		}
	}

	[ConsoleCommand(Command = "show_personal_quest_progress", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowPersonalQuestProgress(IConsole console)
	{
		CharacterState characterState = Singleton<FHMapChoreographer>.Instance.View?.SelectedCharacter;
		if (characterState?.PersonalQuest == null)
		{
			console.WriteLineWithStyle("No selected character with personal quest", CommandOutputStyle.Error);
			return;
		}
		string text = $"{characterState.PersonalQuestDef.ID} progress: \n" + characterState.PersonalQuestDef.DumpProgressInfo(characterState.PersonalQuest, characterState, Singleton<FHMapChoreographer>.Instance.GameController);
		console.WriteLineWithStyle(text, CommandOutputStyle.Default);
	}

	[ConsoleCommand(Command = "show_personal_quest_deck", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowPersonalQuestDeck(IConsole console)
	{
		string text = "Personal quest deck:";
		foreach (PersonalQuestDef item in Singleton<FHMapChoreographer>.Instance.View.GameState.PersonalQuestDeck)
		{
			text += $"\n{item.ID}";
		}
		console.WriteLineWithStyle(text, CommandOutputStyle.Default);
	}

	[ConsoleCommand(Command = "show_random_blueprint_deck", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowRandomBlueprintDeck(IConsole console)
	{
		string text = "Random blueprint deck:";
		foreach (ItemDef item in Singleton<FHMapChoreographer>.Instance.View.GameState.RandomItemBlueprintDeck)
		{
			text += $"\n{item.ID}";
		}
		console.WriteLineWithStyle(text, CommandOutputStyle.Default);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "skip_prologue", RequiredContext = typeof(FHMapChoreographer))]
	public static void SkipPrologue(IConsole console)
	{
		if (!Singleton<FHMapChoreographer>.Instance.GameState.IsInPrologue)
		{
			console.WriteWithStyle("Game is not in prologue state", CommandOutputStyle.Error);
			return;
		}
		if (!Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters.Any())
		{
			CreateDefaultParty_Cheat(console);
		}
		Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.FinishCreateCharacters();
		foreach (CharacterState removedCharacter in Singleton<FHMapChoreographer>.Instance.GameController.State.RemovedCharacters)
		{
			Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.AddCharacterFromParty(removedCharacter);
		}
		Singleton<FHMapChoreographer>.Instance.GameState.IsSetupPhase = false;
		if (Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario != null)
		{
			Singleton<FHMapChoreographer>.Instance.GameState.ActiveScenario = null;
		}
		Singleton<FHMapChoreographer>.Instance.GameController.State.UnlockedScenarios.All((FHRL.ScenarioState s) => s.Completed = true);
		Singleton<FHMapChoreographer>.Instance.GameState.UnlockedScenarios.Add(new FHRL.ScenarioState(new ScenarioID(2u), Singleton<FHMapChoreographer>.Instance.DefRepo));
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Morale = 5;
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Week = 2u;
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Phase = OutpostPhase.Downtime;
		OutpostState outpost = Singleton<FHMapChoreographer>.Instance.GameState.Outpost;
		if (outpost.Trading == null)
		{
			outpost.Trading = new TradingState();
		}
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Trading.Reset();
		Singleton<FHMapChoreographer>.Instance.GameState.Outpost.Trading.BuyItems += Singleton<FHMapChoreographer>.Instance.GameState.Settings.BuyItemPerTurn;
		Singleton<FHMapChoreographer>.Instance.View.ResetView();
	}

	[MultiplayerCommand(MultiplayerCommandExecType.Everybody)]
	[ConsoleCommand(Command = "create_character", RequiredContext = typeof(FHMapChoreographer))]
	public static void CreateCharacter(IConsole console, string className = null)
	{
		foreach (CharacterClassDef def in Singleton<FHMapChoreographer>.Instance.GameController.DefRepository.CharacterClasses.Values.ToList())
		{
			if (className != null && !def.Name.ToLower().StartsWith(className.ToLower()))
			{
				continue;
			}
			string validCharacterName = Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.GetValidCharacterName(def.Name);
			if (Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.CanCreateCharacter(def, validCharacterName))
			{
				Singleton<FHMapChoreographer>.Instance.GameClient.GameController.StartCreateNewCharacterWithoutSlot(def, CharacterID.NewID(), validCharacterName);
				Singleton<FHMapChoreographer>.Instance.GameController.CharacterController.FinishCreateCharacters();
				Singleton<FHMapChoreographer>.Instance.GameState.AllCharacters.First((CharacterState c) => c.ClassDef == def);
				console.WriteLine("Created character " + def.Name);
				Singleton<FHMapChoreographer>.Instance.View.RefreshView();
				return;
			}
		}
		console.WriteLineWithStyle("No character class def to create new character from", CommandOutputStyle.Error);
	}

	[ConsoleCommand(Command = "show_end_of_demo")]
	public static void ShowEndOfDemoPopup(IConsole console)
	{
		Singleton<FHMapChoreographer>.Instance.View.PushState(new DemoMarketingPopupViewState(), FHMapViewLayer.LayerType.Modal);
	}

	[NetworkConsoleCommand]
	[ConsoleCommand(Command = "choose_modifier", RequiredContext = typeof(FHMapChoreographer))]
	public static void ChooseAttackModifier(IConsole console, string modID)
	{
		AttackController attackController = Singleton<FHMapChoreographer>.Instance.View.GameController.AttackController;
		List<TownGuardAttackModifierDef> list = attackController?.GetTownGuardAttackModifiersToChoose()?.ToList();
		if (list == null || list.Count != 2)
		{
			console.WriteLine("Invalid cards to choose from");
			return;
		}
		TownGuardAttackModifierDef townGuardAttackModifierDef = list.FirstOrDefault((TownGuardAttackModifierDef x) => x.ID.ID.Equals(modID, StringComparison.InvariantCultureIgnoreCase));
		if (townGuardAttackModifierDef != null)
		{
			ProcessResultFromCommand(attackController.ResolveChooseAttackModifier(townGuardAttackModifierDef));
		}
	}

	[ConsoleCommand(Command = "show_choose_modifiers", RequiredContext = typeof(FHMapChoreographer))]
	public static void ShowChooseAttackModifiers(IConsole console)
	{
		List<TownGuardAttackModifierDef> list = Singleton<FHMapChoreographer>.Instance.View.GameController.AttackController?.GetTownGuardAttackModifiersToChoose()?.ToList();
		if (list == null || list.Count != 2)
		{
			console.WriteLine("Invalid cards to choose from");
			return;
		}
		console.WriteLine("Modifiers to choose: ");
		foreach (TownGuardAttackModifierDef item in list)
		{
			console.WriteLine($"{item}");
		}
	}

	[ConsoleCommand(Command = "render_stage_toggle", RequiredContext = typeof(FHMapChoreographer))]
	public static void RenderStageToggle(IConsole console)
	{
		Outliner outliner = UnityEngine.Object.FindObjectOfType<Outliner>();
		if (outliner == null)
		{
			console.WriteLine("Outliner component not found");
		}
		else
		{
			outliner.RenderStage = ((outliner.RenderStage != RenderStage.AfterTransparents) ? RenderStage.AfterTransparents : RenderStage.BeforeTransparents);
		}
	}

	[ConsoleCommand(Command = "buy_lumber_with_inspiration", RequiredContext = typeof(FHMapChoreographer))]
	public static void BuyLumberWithInspiration(IConsole console, int amount = 1)
	{
		BuyMaterialWithInspiration(console, OutpostMaterialType.Lumber, amount);
	}

	[ConsoleCommand(Command = "buy_metal_with_inspiration", RequiredContext = typeof(FHMapChoreographer))]
	public static void BuyMetalWithInspiration(IConsole console, int amount = 1)
	{
		BuyMaterialWithInspiration(console, OutpostMaterialType.Metal, amount);
	}

	[ConsoleCommand(Command = "buy_hide_with_inspiration", RequiredContext = typeof(FHMapChoreographer))]
	public static void BuyHideWithInspiration(IConsole console, int amount = 1)
	{
		BuyMaterialWithInspiration(console, OutpostMaterialType.Hide, amount);
	}

	private static void BuyMaterialWithInspiration(IConsole console, OutpostMaterialType type, int amount = 1)
	{
		CharacterState characterState = Singleton<FHMapChoreographer>.Instance.View?.SelectedCharacter;
		if (characterState == null)
		{
			console.WriteLineWithStyle("No selected character", CommandOutputStyle.Error);
		}
		else if (!Singleton<FHMapChoreographer>.Instance.GameController.TradingController.CanBuyMaterialWithInspiration(type, amount))
		{
			console.WriteLineWithStyle("Not enough inspiration", CommandOutputStyle.Error);
		}
		else
		{
			ProcessResultFromCommand(Singleton<FHMapChoreographer>.Instance.GameController.TradingController.BuyMaterialWithInspiration(characterState, type, amount));
		}
	}
}
