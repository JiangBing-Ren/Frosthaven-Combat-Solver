using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AStar;
using FFSNet;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using Libs.GameConsole;
using Libs.Utils.Profiler;
using ReportIssueGui;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class FastReload
{
	public class Checkpoint
	{
		public string name;

		public ScenarioState scenarioState;

		public string importantStateDump;

		public string fullStateDump;

		public override string ToString()
		{
			return name;
		}
	}

	public static bool FastUndoEnabled = false;

	public static bool WaitingForHostUndo = false;

	[StateDump(Skip = true)]
	public static bool IsFastLoading = false;

	private static Coroutine WaitingForCoroutine;

	public static Action OnFastUndoEnabledChanged;

	public static bool TriggerAutoSaveGame = false;

	public static bool s_FastRedoEnabled = false;

	public static bool FastRedoEnabled = false;

	[ConsoleVariable(Alias = "enable_fast_restart_round", Persistent = true)]
	public static bool FastRestartRoundEnabled = false;

	[ConsoleVariable(Alias = "reset_card_hands_on_fast_reload", Persistent = false)]
	public static bool ResetCardHands = false;

	[ConsoleVariable(Alias = "report_fast_load_desyncs", Persistent = false)]
	public static int ReportFastLoadDesyncs = 2;

	public static List<Checkpoint> History = new List<Checkpoint>();

	public static int IndexInHistory = -1;

	public static ScenarioState InitialStartRoundState;

	private static Checkpoint QuickSaveCheckpoint;

	private static int s_AutoUndoCounter;

	private static int s_AutoRedoCounter;

	public static bool AutoUnodRedoOnEveryAutoSave;

	private static bool s_ignoreNextAutoSave = false;

	public static bool IsSavingMultiThreaded = false;

	private static List<NetworkPlayer> m_WaitForFastUndoList = new List<NetworkPlayer>();

	private static bool s_IgnoreNextLoadAfterNormalLoad;

	[ConsoleVariable(Alias = "enable_fast_undo", Persistent = false)]
	public static bool setting_FastUndoEnabled
	{
		get
		{
			if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
			{
				return FastUndoEnabled;
			}
			return SaveData.Instance?.Global?.EnableFastUndo == true;
		}
		set
		{
			if (SaveData.Instance?.Global == null)
			{
				return;
			}
			if (SaveData.Instance.Global.EnableFastUndo != value)
			{
				SaveData.Instance.Global.EnableFastUndo = value;
				SaveData.Instance.SaveGlobalData();
			}
			OnFastUndoEnabledChanged?.Invoke();
			if (FastUndoEnabled != value)
			{
				FastUndoEnabled = value;
				if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
				{
					SceneController.RestartRoundMode dataInt = (value ? SceneController.RestartRoundMode.EnableFastUndo : SceneController.RestartRoundMode.DisableFastUndo);
					Synchronizer.SendSideAction(GameActionType.RestartRound, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)dataInt);
				}
				if (ScenarioManager.CurrentScenarioState != null)
				{
					ResetToStartRoundState();
					AutoSave();
				}
			}
		}
	}

	public static bool WaitingForUndo
	{
		get
		{
			if (WaitingForCoroutine == null && !ScenarioManager.IsLoading)
			{
				return IsFastLoading;
			}
			return true;
		}
	}

	[ConsoleVariable(Alias = "enable_fast_redo", Persistent = false)]
	public static bool setting_FastRedoEnabled
	{
		get
		{
			return s_FastRedoEnabled;
		}
		set
		{
			s_FastRedoEnabled = value;
			FastRedoEnabled = value;
		}
	}

	public static string CurrentHistoryID
	{
		get
		{
			if (History != null && IndexInHistory >= 0 && IndexInHistory < History.Count)
			{
				return History[IndexInHistory].name;
			}
			return null;
		}
	}

	public static bool CanSave(bool isCanLoadCheck = false)
	{
		if (PhaseManager.CurrentPhase is CPhasePlayerExhausted)
		{
			return false;
		}
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return false;
		}
		if (ScenarioManager.IsFastReloading)
		{
			return false;
		}
		if (GameState.ThreadIsSleeping & isCanLoadCheck)
		{
			return false;
		}
		if (Choreographer.IsInNoPartyCampaign())
		{
			return false;
		}
		if (GameState.PendingOnLongRestBonuses != null && GameState.PendingOnLongRestBonuses.Count > 0)
		{
			return false;
		}
		if (GameState.PendingOnShortRestBonuses != null && GameState.PendingOnShortRestBonuses.Count > 0)
		{
			return false;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ShortRest)
		{
			if (GameState.s_ShortRestingPlayer == null)
			{
				return false;
			}
			if (GameState.s_ShortRestingPlayer.CharacterClass.HasShortRested)
			{
				return false;
			}
			if (GameState.s_ShortRestCardToBurnID == 0 && !ShortRest.Instant)
			{
				return false;
			}
			if (ScenarioManager.CurrentScenarioState.FreeShortRestButMuddle)
			{
				return false;
			}
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection)
		{
			if (!(GameState.TurnActor is CPlayerActor cPlayerActor))
			{
				return false;
			}
			if (!isCanLoadCheck)
			{
				if (GameState.IsPerformingCardOutOfPhase)
				{
					return false;
				}
				if (cPlayerActor.IsTakingExtraTurn)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static bool CanLoad()
	{
		if (DesyncDetector.IsCheckingHostHash())
		{
			return false;
		}
		return CanSave(isCanLoadCheck: true);
	}

	public static bool CanUndo()
	{
		if (IndexInHistory <= 0)
		{
			return false;
		}
		if (!CanLoad())
		{
			return false;
		}
		return true;
	}

	public static bool CanRedo()
	{
		if (IndexInHistory + 1 >= History.Count)
		{
			return false;
		}
		if (!CanLoad())
		{
			return false;
		}
		return true;
	}

	public static void Reset()
	{
		History.Clear();
		IndexInHistory = -1;
		InitialStartRoundState = null;
		UpdateUndoRedoButtons();
	}

	public static void ResetToStartRoundState(ScenarioState scenarioState = null)
	{
		Reset();
		if (FastUndoEnabled)
		{
			if (scenarioState == null)
			{
				scenarioState = GetDefaultStartRoundState();
			}
			InitialStartRoundState = scenarioState;
		}
	}

	public static void ResetOnClientJoined(int playerID)
	{
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendSideAction(GameActionType.AutoSaveOnClientJoined, null, canBeUnreliable: false, sendToHostOnly: false, 0, playerID);
		}
		else if (PlayerRegistry.MyPlayer.PlayerID.ID == playerID)
		{
			return;
		}
		ResetToStartRoundState();
	}

	public static async Task RestartRound()
	{
		IndexInHistory = GetStartRoundIndex();
		if (IndexInHistory >= 0)
		{
			await Load(History[IndexInHistory]);
			UpdateUndoRedoButtons();
		}
		else
		{
			OnRestartRound(isFullRestart: false);
			await Load(GetDefaultStartRoundState());
			UpdateUndoRedoButtons();
		}
	}

	public static void AutoSaveCheckPointMultiThreaded(bool forceSave = false, string importantStateDump = null, string fullStateDump = null, CPlayerActor playerAboutToShortRest = null, Action onDone = null)
	{
		if (ScenarioRuleClient.IsProcessingOrMessagesQueued || FFSNetwork.IsOnline)
		{
			AutoSave(forceSave, importantStateDump, fullStateDump, playerAboutToShortRest);
		}
		else
		{
			CoroutineHelper.RunCoroutine(AutoSaveCheckPointMultiThreadedCoroutine(forceSave, importantStateDump, fullStateDump, playerAboutToShortRest, onDone));
		}
	}

	private static IEnumerator AutoSaveCheckPointMultiThreadedCoroutine(bool forceSave = false, string importantStateDump = null, string fullStateDump = null, CPlayerActor playerAboutToShortRest = null, Action onDone = null)
	{
		UpdateInitialStartRoundState();
		if (s_ignoreNextAutoSave)
		{
			s_ignoreNextAutoSave = false;
			UpdateUndoRedoButtons();
		}
		else if (!CanSave())
		{
			if (forceSave)
			{
				Log.Error($"Cannot fast auto-save, current phase: {PhaseManager.PhaseType}");
			}
			UpdateUndoRedoButtons();
		}
		else
		{
			if (!forceSave && !FastUndoEnabled)
			{
				yield break;
			}
			string checkpointName = GenerateCheckpointName(playerAboutToShortRest);
			if (!forceSave && IndexInHistory >= 0 && IndexInHistory < History.Count && History[IndexInHistory].name == checkpointName)
			{
				UpdateUndoRedoButtons();
				yield break;
			}
			bool savedCheckpoint = false;
			Checkpoint checkPoint = null;
			bool wasBlocking = Choreographer.s_Choreographer.BlockClientMessageProcessing;
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: true);
			PrepareForSave();
			IsSavingMultiThreaded = true;
			ScenarioRuleClient.ExecuteSRLCommand(null, null, async () =>
			{
				checkPoint = await SaveCheckpoint(checkpointName, importantStateDump, fullStateDump);
				savedCheckpoint = true;
			}, "FAST_AUTO_SAVE");
			while (!savedCheckpoint)
			{
				yield return new WaitForEndOfFrame();
			}
			IsSavingMultiThreaded = false;
			if (checkPoint == null)
			{
				Choreographer.s_Choreographer.SetBlockClientMessageProcessing(wasBlocking);
				yield break;
			}
			GenerateStateDump(checkPoint, importantStateDump, fullStateDump);
			Log.Info($"Fast Auto Save at {IndexInHistory} / {History.Count - 1}");
			IndexInHistory++;
			if (IndexInHistory < History.Count)
			{
				History.RemoveRange(IndexInHistory, History.Count - IndexInHistory);
			}
			History.Add(checkPoint);
			UpdateInitialStartRoundState();
			UpdateUndoRedoButtons();
			if (playerAboutToShortRest == null)
			{
				if (TriggerAutoSaveGame)
				{
					TriggerAutoSaveGame = false;
					yield return SaveData.Instance.AutoSaveCurrentAdventureData();
				}
				if (AutoUnodRedoOnEveryAutoSave)
				{
					RequestAutoUndoRedo(1, 1);
				}
			}
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(wasBlocking);
			onDone?.Invoke();
		}
	}

	public static async Task AutoSave(bool forceSave = false, string importantStateDump = null, string fullStateDump = null, CPlayerActor playerAboutToShortRest = null)
	{
		UpdateInitialStartRoundState();
		if (s_ignoreNextAutoSave)
		{
			s_ignoreNextAutoSave = false;
			UpdateUndoRedoButtons();
		}
		else if (!CanSave())
		{
			if (forceSave)
			{
				Log.Error($"Cannot fast auto-save, current phase: {PhaseManager.PhaseType}");
			}
			UpdateUndoRedoButtons();
		}
		else
		{
			if (!forceSave && !FastUndoEnabled)
			{
				return;
			}
			string text = GenerateCheckpointName(playerAboutToShortRest);
			if (!forceSave && IndexInHistory >= 0 && IndexInHistory < History.Count && History[IndexInHistory].name == text)
			{
				UpdateUndoRedoButtons();
				return;
			}
			Checkpoint checkpoint = await SaveCheckpoint(text, importantStateDump, fullStateDump);
			if (checkpoint == null)
			{
				return;
			}
			Log.Info($"Fast Auto Save at {IndexInHistory} / {History.Count - 1}");
			IndexInHistory++;
			if (IndexInHistory < History.Count)
			{
				History.RemoveRange(IndexInHistory, History.Count - IndexInHistory);
			}
			History.Add(checkpoint);
			UpdateInitialStartRoundState();
			UpdateUndoRedoButtons();
			if (playerAboutToShortRest == null)
			{
				if (TriggerAutoSaveGame)
				{
					TriggerAutoSaveGame = false;
					await SaveData.Instance.AutoSaveCurrentAdventureData();
				}
				if (AutoUnodRedoOnEveryAutoSave)
				{
					RequestAutoUndoRedo(1, 1);
				}
			}
		}
	}

	public static void AutoSaveOnInstantShortRest(CPlayerActor playerAboutToShortRest)
	{
		if (FastUndoEnabled)
		{
			AutoSave(forceSave: false, null, null, playerAboutToShortRest).Wait();
		}
	}

	private static bool CanUndoNow()
	{
		if (Choreographer.s_Choreographer.m_MessageQueue.Count > 0 || ScenarioRuleClient.IsProcessingOrMessagesQueued || ActionProcessor.ActionQueue.Count > 0)
		{
			return false;
		}
		return CanUndo();
	}

	private static IEnumerator WaitForMessagesToFinish(Action onFinish)
	{
		ProcessingUI.Show(show: true);
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (!CanUndoNow());
		WaitingForCoroutine = null;
		ProcessingUI.Show(show: false);
		onFinish();
	}

	private static IEnumerator WaitForClientsToBeReadyForFastUndo(Action onFinish)
	{
		ProcessingUI.Show(show: true);
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (PlayerRegistry.PlayersWeShouldWaitFor.Any((NetworkPlayer x) => m_WaitForFastUndoList.Find((NetworkPlayer y) => y.PlayerID == x.PlayerID) == null));
		WaitingForCoroutine = null;
		ProcessingUI.Show(show: false);
		onFinish();
	}

	public static void ClientReadyForFastUndo(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			if (!m_WaitForFastUndoList.Contains(player))
			{
				m_WaitForFastUndoList.Add(player);
				Log.SimpleInfo("Host acknowledged client " + player.Username + "( ID: " + player.PlayerID.ToString() + " ) ready for fast undo.");
			}
			else
			{
				Log.Error("Player " + player.PlayerID.ToString() + " is already added to m_WaitForFastUndoList");
			}
		}
		else
		{
			Log.NetworkWarning("Error determining the player who reached fast undo ready check. NetworkPlayer returns null.");
		}
	}

	public static async Task ClientTryConfirmReadyForFastUndo(GameAction a, bool tryWaitLocally = true)
	{
		if (tryWaitLocally && !CanUndoNow())
		{
			WaitingForCoroutine = CoroutineHelper.instance.StartCoroutine(WaitForMessagesToFinish(() =>
			{
				ClientTryConfirmReadyForFastUndo(a, tryWaitLocally: false);
			}));
		}
		else
		{
			Synchronizer.SendSideAction(GameActionType.FastUndoClientReady, null, canBeUnreliable: false, sendToHostOnly: true);
		}
	}

	public static void WaitForClientsToConfirmReadyForFastUndo()
	{
		m_WaitForFastUndoList.Clear();
		if (!m_WaitForFastUndoList.Contains(PlayerRegistry.MyPlayer))
		{
			m_WaitForFastUndoList.Add(PlayerRegistry.MyPlayer);
			WaitingForCoroutine = CoroutineHelper.instance.StartCoroutine(WaitForClientsToBeReadyForFastUndo(() =>
			{
				m_WaitForFastUndoList.Clear();
				Undo(force: false, tryWaitLocally: false, tryWaitForClients: false);
			}));
			Synchronizer.SendSideAction(GameActionType.FastUndoCheckClientReady);
			Log.SimpleInfo("Player " + PlayerRegistry.MyPlayer.Username + " (ID: " + PlayerRegistry.MyPlayer.PlayerID.ToString() + ") trying to fast undo, starting WaitForClientsToBeReadyForFastUndo coroutine to acknowledge clients.");
		}
	}

	public static async Task Undo(bool force = false, bool tryWaitLocally = true, bool tryWaitForClients = true)
	{
		if ((!force && !CanUndo()) || IndexInHistory <= 0)
		{
			return;
		}
		if (tryWaitLocally && !CanUndoNow())
		{
			WaitingForCoroutine = CoroutineHelper.instance.StartCoroutine(WaitForMessagesToFinish(() =>
			{
				Undo(force: false, tryWaitLocally: false, tryWaitForClients);
			}));
			return;
		}
		_ = History[IndexInHistory];
		Checkpoint checkpoint = History[IndexInHistory - 1];
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsClient)
			{
				WaitingForHostUndo = true;
				ProcessingUI.Show(show: true);
				UpdateUndoRedoButtons(refreshView: false);
				Synchronizer.SendSideAction(GameActionType.FastUndoToCheckpoint, null, canBeUnreliable: false, sendToHostOnly: true, 0, 0, 0, dataBool: false, checkpoint.name);
				return;
			}
			if (tryWaitForClients)
			{
				WaitForClientsToConfirmReadyForFastUndo();
				return;
			}
		}
		await UndoToCheckpoint(checkpoint.name);
	}

	public static Checkpoint FindLastCheckPointWithID(string id)
	{
		return History.LastOrDefault((Checkpoint x) => x.name.Equals(id));
	}

	public static bool HasLastCheckpoint(string id)
	{
		return FindLastCheckPointWithID(id) != null;
	}

	public static async Task UndoToCheckpoint(string id, bool force = false)
	{
		if (!force && !CanUndo())
		{
			return;
		}
		Checkpoint newCheckpoint = FindLastCheckPointWithID(id);
		if (newCheckpoint == null)
		{
			return;
		}
		Checkpoint checkpoint = History[IndexInHistory];
		Log.Info($"Fast Undo at {checkpoint} {IndexInHistory} / {History.Count - 1} to {newCheckpoint}");
		IndexInHistory = History.IndexOf(newCheckpoint);
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsHost)
			{
				Synchronizer.SendSideAction(GameActionType.FastUndoToCheckpoint, null, canBeUnreliable: false, sendToHostOnly: false, 0, 0, 0, dataBool: false, newCheckpoint.name);
				await Task.Yield();
			}
			if (ActionProcessor.ActionQueue.Count > 0)
			{
				List<GameAction> list = ActionProcessor.ActionQueue.ToList();
				ActionProcessor.ActionQueue.Clear();
				foreach (GameAction item in list)
				{
					if (!(item.SupplementaryDataToken is StartRoundCardsToken startRoundCardsToken) || !(startRoundCardsToken.FastReloadID != CurrentHistoryID))
					{
						ActionProcessor.ActionQueue.Enqueue(item);
					}
				}
			}
		}
		ProcessingUI.Show(show: false);
		WaitingForHostUndo = false;
		await Load(newCheckpoint);
		UpdateUndoRedoButtons();
	}

	public static async Task Redo(bool force = false)
	{
		if ((force || CanRedo()) && IndexInHistory < History.Count - 1)
		{
			Log.Info($"Fast Redo at {IndexInHistory} / {History.Count - 1}");
			IndexInHistory++;
			await Load(History[IndexInHistory]);
			UpdateUndoRedoButtons();
		}
	}

	public static void RequestAutoUndoRedo(int undoCount, int redoCount)
	{
		int num = ((IndexInHistory >= 0) ? IndexInHistory : 0);
		if (undoCount < 0 || undoCount > num)
		{
			s_AutoUndoCounter = num;
		}
		else
		{
			s_AutoUndoCounter = undoCount;
		}
		if (redoCount <= -2)
		{
			redoCount = s_AutoUndoCounter;
		}
		int num2 = History.Count - 1 - IndexInHistory + s_AutoUndoCounter;
		if (redoCount < 0 || redoCount > num2)
		{
			s_AutoRedoCounter = num2;
		}
		else
		{
			s_AutoRedoCounter = redoCount;
		}
	}

	public static void CheckAutoUndoRedo()
	{
		if (s_AutoUndoCounter > 0)
		{
			if (CanUndo())
			{
				s_AutoUndoCounter--;
				Undo();
			}
		}
		else if (s_AutoRedoCounter > 0 && CanRedo())
		{
			s_AutoRedoCounter--;
			Redo();
		}
	}

	public static void StopAutoUndoRedo()
	{
		s_AutoUndoCounter = (s_AutoRedoCounter = 0);
	}

	public static void UndoAction()
	{
		Undo();
	}

	public static void RedoAction()
	{
		if (FFSNetwork.IsOnline)
		{
			Synchronizer.SendSideAction(GameActionType.RestartRound, null, canBeUnreliable: false, sendToHostOnly: false, 0, 4);
		}
		Redo();
	}

	private static bool IsStartRoundState(ScenarioState scenarioState)
	{
		if (scenarioState?.SavedGameState == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(scenarioState.SavedGameState.TurnActorGUID))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(scenarioState.SavedGameState.ShortRestingPlayerGUID))
		{
			return false;
		}
		return true;
	}

	private static bool IsStartRoundState(ScenarioState scenarioState, int roundNumber)
	{
		if (!IsStartRoundState(scenarioState))
		{
			return false;
		}
		if (scenarioState.SavedGameState.RoundCount != roundNumber)
		{
			return false;
		}
		return true;
	}

	private static void UpdateInitialStartRoundState()
	{
		if (InitialStartRoundState == null)
		{
			ScenarioState defaultStartRoundState = GetDefaultStartRoundState();
			if (IsStartRoundState(defaultStartRoundState))
			{
				InitialStartRoundState = defaultStartRoundState;
			}
			else if (History.Count != 0)
			{
				InitialStartRoundState = History[0].scenarioState;
			}
		}
	}

	private static void UpdateDefaultStrartRoundState()
	{
		if (GameState.RoundCount > 0 && !IsStartRoundState(GetDefaultStartRoundState(), GameState.RoundCount))
		{
			int startRoundIndex = GetStartRoundIndex();
			if (startRoundIndex >= 0)
			{
				SetDefaultStartRoundState(History[startRoundIndex].scenarioState);
			}
			else if (IsStartRoundState(InitialStartRoundState, GameState.RoundCount))
			{
				SetDefaultStartRoundState(InitialStartRoundState);
			}
			else
			{
				Log.Error($"FastReload: Could not find start round state for round {GameState.RoundCount}");
			}
		}
	}

	public static ScenarioState GetDefaultStartRoundState()
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			return SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState?.CurrentState;
		}
		return SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentSingleScenario?.ScenarioState;
	}

	public static void SetDefaultStartRoundState(ScenarioState scenarioState)
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState?.UpdateAutoSaveState(scenarioState);
		}
		else
		{
			SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario.ScenarioState = scenarioState;
		}
	}

	private static int GetStartRoundIndex()
	{
		if (GameState.RoundCount <= 0)
		{
			return -1;
		}
		if (IsStartRoundState(InitialStartRoundState, GameState.RoundCount))
		{
			if (History.Count > 0 && History[0].scenarioState == InitialStartRoundState)
			{
				return 0;
			}
			return -1;
		}
		for (int i = 0; i < History.Count; i++)
		{
			if (IsStartRoundState(History[i].scenarioState, GameState.RoundCount))
			{
				return i;
			}
		}
		return -1;
	}

	public static void OnRestartRound(bool isFullRestart)
	{
		if (isFullRestart)
		{
			IndexInHistory = GetStartRoundIndex();
			s_ignoreNextAutoSave = true;
			s_IgnoreNextLoadAfterNormalLoad = true;
		}
	}

	public static async Task<string> QuickSavePath()
	{
		return Path.Combine(Path.GetDirectoryName(await SaveData.Instance.RootData.CurrentlyUsedSavePath), "FastSave.bin");
	}

	public static async Task<string> HistorySavePath()
	{
		return Path.Combine(Path.GetDirectoryName(await SaveData.Instance.RootData.CurrentlyUsedSavePath), "FastSave");
	}

	public static async Task QuickSave(bool saveToDisk)
	{
		string saveToFilePath = null;
		if (saveToDisk)
		{
			saveToFilePath = await QuickSavePath();
		}
		QuickSaveCheckpoint = await SaveCheckpoint("quick_save", null, null, saveToFilePath);
	}

	public static async Task QuickLoad(bool loadFromDisk)
	{
		if (loadFromDisk)
		{
			Checkpoint checkpoint = await LoadCheckpointFromFile("quick_save", await QuickSavePath());
			if (checkpoint == null)
			{
				return;
			}
			QuickSaveCheckpoint = checkpoint;
		}
		if (QuickSaveCheckpoint != null)
		{
			await Load(QuickSaveCheckpoint);
		}
	}

	public static async Task SaveAllHistory()
	{
		string hsPath = await HistorySavePath();
		try
		{
			Stopwatch timer = Stopwatch.StartNew();
			DirectoryInfo directoryInfo = new DirectoryInfo(hsPath);
			if (directoryInfo.Exists)
			{
				FileInfo[] files = directoryInfo.GetFiles();
				for (int i = 0; i < files.Length; i++)
				{
					files[i].Delete();
				}
			}
			if (History.Count == 0)
			{
				if (directoryInfo.Exists)
				{
					directoryInfo.Delete();
				}
				return;
			}
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			int i2 = 0;
			while (i2 < History.Count)
			{
				Checkpoint checkpoint = History[i2];
				string filename = $"{hsPath}/{i2:D3}-{checkpoint.name}.bin";
				using (MemoryStream stream = new MemoryStream())
				{
					await checkpoint.scenarioState.BinarySerialize(stream);
					await File.WriteAllBytesAsync(filename, stream.ToArray());
				}
				int i = i2 + 1;
				i2 = i;
			}
			Log.Trace($"FastReload: Saved {History.Count} checkpoints in {timer.ElapsedMilliseconds}ms");
		}
		catch (Exception ex)
		{
			Log.Error("Error saving FastReload history to " + hsPath + ": " + ex.Message);
		}
	}

	public static async Task LoadAllHistory()
	{
		Reset();
		string hsPath = await HistorySavePath();
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(hsPath);
			if (!directoryInfo.Exists)
			{
				return;
			}
			Stopwatch timer = Stopwatch.StartNew();
			FileInfo[] files = directoryInfo.GetFiles();
			Array.Sort(files, (FileInfo a, FileInfo b) => a.Name.CompareTo(b.Name));
			string curCPName = GenerateCheckpointName();
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				string name = Path.GetFileNameWithoutExtension(fileInfo.Name);
				int num2 = name.IndexOf('-');
				if (num2 >= 0)
				{
					name = name.Substring(num2 + 1);
					Checkpoint item = await LoadCheckpointFromFile(name, fileInfo.FullName);
					if (name == curCPName)
					{
						IndexInHistory = History.Count;
					}
					History.Add(item);
				}
			}
			if (IndexInHistory < 0)
			{
				IndexInHistory = GetStartRoundIndex();
			}
			UpdateUndoRedoButtons();
			Log.Trace($"FastReload: Loaded {History.Count} checkpoints in {timer.ElapsedMilliseconds}ms");
		}
		catch (Exception ex)
		{
			Log.Error("Error loading FastReload history from " + hsPath + ": " + ex.Message);
		}
	}

	public static bool IsMyTurn()
	{
		if (!FFSNetwork.IsOnline)
		{
			return true;
		}
		switch (PhaseManager.PhaseType)
		{
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
			return FFSNetwork.IsHost;
		case CPhase.PhaseType.ShortRest:
			if (GameState.s_ShortRestingPlayer != null)
			{
				return GameState.s_ShortRestingPlayer.IsUnderMyControl();
			}
			return false;
		case CPhase.PhaseType.ActionSelection:
			if (GameState.TurnActor is CPlayerActor actor2)
			{
				return actor2.IsUnderMyControl();
			}
			return false;
		case CPhase.PhaseType.InfuserMechanicSelection:
			if (GameState.PendingInfusionPhase?.TriggerActor is CPlayerActor actor)
			{
				return actor.IsUnderMyControl();
			}
			return false;
		default:
			return false;
		}
	}

	private static void FixButton(FHButton btn, string text, string tooltipText)
	{
		TextMeshProUGUI textMeshProUGUI = btn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
		if (textMeshProUGUI != null)
		{
			textMeshProUGUI.text = text;
		}
		UITextTooltipTarget component = btn.GetComponent<UITextTooltipTarget>();
		if (component != null)
		{
			component.tooltipText = tooltipText;
		}
		Transform transform = btn.transform.Find("HotkeyIcon");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: false);
		}
	}

	public static void UpdateUndoRedoButtons(bool refreshView = true)
	{
		if (!(Choreographer.s_Choreographer == null))
		{
			if (!Choreographer.s_Choreographer.m_FastUndoButton.transform.parent.gameObject.activeSelf)
			{
				Choreographer.s_Choreographer.m_FastUndoButton.transform.parent.gameObject.SetActive(value: true);
				Choreographer.s_Choreographer.m_FastUndoButton.onClick.AddListener(UndoAction);
				Choreographer.s_Choreographer.m_FastRedoButton.onClick.AddListener(RedoAction);
				FixButton(Choreographer.s_Choreographer.m_FastUndoButton, "FastUndo", "GUI_UNDO_ACTION");
				FixButton(Choreographer.s_Choreographer.m_FastRedoButton, "FastRedo", "GUI_REDO_ACTION");
			}
			if (FastUndoEnabled && CanUndo() && !WaitingForHostUndo)
			{
				Choreographer.s_Choreographer.m_FastUndoButton.gameObject.SetActive(value: true);
				Choreographer.s_Choreographer.m_FastUndoButton.interactable = IsMyTurn();
				UIUtility.BringToFront(Choreographer.s_Choreographer.m_FastUndoButton.gameObject);
			}
			else
			{
				Choreographer.s_Choreographer.m_FastUndoButton.gameObject.SetActive(value: false);
			}
			if (FastRedoEnabled && CanRedo())
			{
				Choreographer.s_Choreographer.m_FastRedoButton.gameObject.SetActive(value: true);
				Choreographer.s_Choreographer.m_FastRedoButton.interactable = IsMyTurn();
				UIUtility.BringToFront(Choreographer.s_Choreographer.m_FastRedoButton.gameObject);
			}
			else
			{
				Choreographer.s_Choreographer.m_FastRedoButton.gameObject.SetActive(value: false);
			}
			if (refreshView)
			{
				Choreographer.s_Choreographer.RefreshViewController();
			}
		}
	}

	public static void ProxyFastUndoEnabled(bool enabled)
	{
		FastUndoEnabled = enabled;
		OnFastUndoEnabledChanged?.Invoke();
		if (ScenarioManager.CurrentScenarioState != null)
		{
			ResetToStartRoundState();
			AutoSave();
		}
	}

	public static void PrepareForSave()
	{
		if (!ScenarioRuleClient.IsWorkerThread)
		{
			Singleton<LevelEventsController>.Instance.TickEvents();
			GameState.CleanUpBeforeSave();
			Choreographer.s_Choreographer?.CleanUpBeforeSave();
			ScenarioManager.CurrentScenarioState.Update();
		}
	}

	public static async Task<ScenarioState> SaveScenarioState(string saveToFilePath = null)
	{
		if (!CanSave())
		{
			Log.Error($"Cannot fast save, current phase: {PhaseManager.PhaseType}");
			return null;
		}
		Stopwatch timer = Stopwatch.StartNew();
		PrepareForSave();
		ScenarioState state;
		if (string.IsNullOrEmpty(saveToFilePath))
		{
			state = await ScenarioManager.CurrentScenarioState.CopyScenarioState();
		}
		else
		{
			using MemoryStream stream = new MemoryStream();
			state = await ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>(stream);
			try
			{
				await File.WriteAllBytesAsync(saveToFilePath, stream.ToArray());
			}
			catch (Exception ex)
			{
				Log.Error("Error writing " + saveToFilePath + ": " + ex.Message);
			}
		}
		Log.Trace($"Fast Save took {timer.ElapsedMilliseconds}ms");
		return state;
	}

	private static async Task<ScenarioState> LoadScenarioStateFromFile(string filePath)
	{
		_ = 1;
		try
		{
			using MemoryStream stream = new MemoryStream(await File.ReadAllBytesAsync(filePath));
			return await GloomUtility.BinaryDeserialize<ScenarioState>(stream);
		}
		catch (Exception ex)
		{
			Log.Error("Error reading " + filePath + ": " + ex.Message);
			return null;
		}
	}

	private static async Task<Checkpoint> LoadCheckpointFromFile(string checkpointName, string filePath)
	{
		ScenarioState scenarioState = await LoadScenarioStateFromFile(filePath);
		if (scenarioState == null)
		{
			return null;
		}
		return new Checkpoint
		{
			name = checkpointName,
			scenarioState = scenarioState
		};
	}

	private static async Task<Checkpoint> SaveCheckpoint(string name, string importantStateDump = null, string fullStateDump = null, string saveToFilePath = null)
	{
		ScenarioState scenarioState = await SaveScenarioState(saveToFilePath);
		if (scenarioState == null)
		{
			return null;
		}
		Checkpoint checkpoint = new Checkpoint();
		checkpoint.name = name;
		checkpoint.scenarioState = scenarioState;
		if (!ScenarioRuleClient.IsWorkerThread)
		{
			GenerateStateDump(checkpoint, importantStateDump, fullStateDump);
		}
		return checkpoint;
	}

	public static void GenerateStateDump(Checkpoint checkpoint, string importantStateDump = null, string fullStateDump = null)
	{
		if (importantStateDump == null)
		{
			StateDumper.NormalizeUseableForFastLoad = true;
			importantStateDump = StateDumper.GenerateImportantStateDump();
			StateDumper.NormalizeUseableForFastLoad = false;
		}
		checkpoint.importantStateDump = StripImportantStateDump(importantStateDump);
		if (fullStateDump == null && ReportFastLoadDesyncs >= 2)
		{
			StateDumper.NormalizeUseableForFastLoad = true;
			fullStateDump = StateDumper.GenerateFullStateDump();
			StateDumper.NormalizeUseableForFastLoad = false;
		}
		checkpoint.fullStateDump = StripFullStateDump(fullStateDump);
	}

	private static string StripFullStateDump(string stateDump)
	{
		if (stateDump == null)
		{
			return null;
		}
		int num = stateDump.IndexOf("Controllables:", StringComparison.Ordinal);
		if (num < 0)
		{
			return stateDump;
		}
		int num2 = stateDump.IndexOf("Scenario.PlayerActors:", StringComparison.Ordinal);
		if (num2 < 0)
		{
			return stateDump;
		}
		return stateDump.Remove(num, num2 - num);
	}

	private static string StripImportantStateDump(string stateDump)
	{
		if (stateDump == null)
		{
			return null;
		}
		int num = stateDump.IndexOf("Players:", StringComparison.Ordinal);
		if (num < 0)
		{
			return stateDump;
		}
		int num2 = stateDump.IndexOf("Round:", StringComparison.Ordinal);
		if (num2 < 0)
		{
			return stateDump;
		}
		return stateDump.Remove(num, num2 - num);
	}

	private static string GenerateCheckpointName(CPlayerActor playerAboutToShortRest = null)
	{
		string text = string.Empty;
		CPlayerActor cPlayerActor = playerAboutToShortRest ?? GameState.s_ShortRestingPlayer;
		if (cPlayerActor != null || PhaseManager.CurrentPhase is CPhaseShortRest || (PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest && !GameState.IsPerformingCardOutOfPhase))
		{
			int num = 0;
			foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
			{
				if (allPlayer.CharacterClass.HasShortRested)
				{
					num++;
				}
			}
			if (cPlayerActor != null)
			{
				text = $"R{GameState.RoundCount}_BSR_{num}";
			}
			else if (num > 0)
			{
				text = $"R{GameState.RoundCount}_ASR_{num - 1}";
			}
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		text = $"R{GameState.RoundCount}";
		if (GameState.TurnActor == null)
		{
			return text;
		}
		text = text + "_" + GameState.TurnActor.Class.ID + "_";
		return text + GameState.GetPlayedActionsCount();
	}

	public static async Task Load(int checkpointIndex)
	{
		if (checkpointIndex < 0 || checkpointIndex >= History.Count)
		{
			Log.Error($"Invalid FastLoad checkPoint index: {checkpointIndex}");
			return;
		}
		IndexInHistory = checkpointIndex;
		await Load(History[IndexInHistory]);
		UpdateUndoRedoButtons();
	}

	private static async Task Load(Checkpoint checkpoint)
	{
		if (checkpoint?.scenarioState == null)
		{
			return;
		}
		await Load(checkpoint.scenarioState);
		if (ReportFastLoadDesyncs <= 0)
		{
			return;
		}
		StateDumper.NormalizeUseableForFastLoad = true;
		string stateDump = StateDumper.GenerateImportantStateDump();
		StateDumper.NormalizeUseableForFastLoad = false;
		stateDump = StripImportantStateDump(stateDump);
		bool flag = stateDump != checkpoint.importantStateDump;
		string text = checkpoint.fullStateDump;
		if (!string.IsNullOrEmpty(checkpoint.fullStateDump))
		{
			StateDumper.NormalizeUseableForFastLoad = true;
			text = StateDumper.GenerateFullStateDump();
			StateDumper.NormalizeUseableForFastLoad = false;
			text = StripFullStateDump(text);
		}
		if (checkpoint.importantStateDump == null)
		{
			checkpoint.importantStateDump = stateDump;
			checkpoint.fullStateDump = text;
			return;
		}
		bool flag2 = text != checkpoint.fullStateDump;
		if (flag || (flag2 && ReportFastLoadDesyncs >= 2))
		{
			string title = null;
			string description = null;
			bool majorDesync;
			if (flag)
			{
				StateDump.AnalyzeDiff(checkpoint.importantStateDump, stateDump, out title, out description, out majorDesync);
				title = "MAJOR FastLoad desync: " + title;
			}
			else
			{
				StateDump.AnalyzeDiff(checkpoint.fullStateDump, text, out title, out description, out majorDesync);
				title = "Minor FastLoad desync: " + title;
			}
			Log.Error(title + "\n" + description);
			string text2 = StateDumper.CombineStateDumps(checkpoint.importantStateDump, checkpoint.fullStateDump);
			string text3 = StateDumper.CombineStateDumps(stateDump, text);
			StateDumper.SaveStateDump(text2, "desync_saved");
			StateDumper.SaveStateDump(text3, "desync_loaded");
			if (DesyncDetector.ReportUIEnabled)
			{
				ReportIssueWindow.ShowDesync(title, description, Encoding.UTF8.GetBytes(text2), Encoding.UTF8.GetBytes(text3), null, null, null);
			}
		}
	}

	public static async Task Load(ScenarioState savedState, bool showUI = true)
	{
		IsFastLoading = true;
		if (showUI)
		{
			ProcessingUI.Show(show: true);
		}
		await Task.Yield();
		DesyncDetector.Reset();
		Stopwatch timer = Stopwatch.StartNew();
		Stopwatch stepTimer = Stopwatch.StartNew();
		int stepCount = 0;
		ScenarioManager.OldFastReloadState = ScenarioManager.CurrentScenarioState;
		try
		{
			Choreographer.s_Choreographer.ResetDelayedDrops();
			if (EnvironmentLoader.IsInstanceValid)
			{
				ProceduralMapTile[] array = (from a in EnvironmentLoader.Instance.GetMaps()
					select a.GetComponent<ProceduralMapTile>()).ToArray();
				for (int num = 0; num < array.Length; num++)
				{
					array[num].ResetBeforeLoad();
				}
			}
			ScenarioManager.NewFastReloadState = await savedState.CopyScenarioState();
			logStep("Deep Copy ScenarioState");
			DestroyModifiedProps(ScenarioManager.NewFastReloadState);
			logStep("DestroyModifiedProps");
			List<CObjectProp> persistedProps = ScenarioManager.CurrentScenarioState.Props.ToList();
			Singleton<CardsHandManager>.Instance.Hide();
			foreach (CardsHandUI item in Singleton<CardsHandManager>.Instance.cardHandsUI)
			{
				item.itemsUI.SetActor(null, subscribeToEvents: false);
			}
			if (ResetCardHands)
			{
				Singleton<CardsHandManager>.Instance.Reset();
				logStep("CardsHandManager.Reset");
			}
			UIManager.Instance?.dialogPopup?.Hide();
			Singleton<UIUseItemsBar>.Instance.Hide();
			Singleton<UIActiveBonusBar>.Instance.Hide();
			Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			logStep("Close Popups");
			Singleton<UIReadyToggle>.Instance?.Reset();
			CardsActionControlller.s_Instance?.Reset();
			logStep("CardsActionControlller.Reset");
			ScenarioManager.Scenario.Reset(ScenarioManager.NewFastReloadState.Maps);
			logStep("ScenarioManager.Reset");
			Choreographer.s_Choreographer.Reset();
			logStep("Choreographer.Reset");
			ClientScenarioManager.Create(ScenarioManager.NewFastReloadState, firstLoad: false);
			logStep("ClientScenarioManager.Create");
			if (SaveData.Instance.Global.CurrentlyPlayingCustomLevel && SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType == ELevelPartyChoiceType.LoadAdventureParty && SaveData.Instance.Global.CurrentCustomLevelData.EnemyLevelsScaleToPartyLevel)
			{
				ScenarioManager.Scenario.EnemyMaxHealthBasedOnPartyLevel = SaveData.Instance.Global.CurrentCustomLevelData.EnemyMaxHealthBasedOnPartyLevel;
				ScenarioManager.Scenario.SetEnemyHealthToMaxOnPlay = SaveData.Instance.Global.CurrentCustomLevelData.SetEnemyHealthToMaxOnPlay;
			}
			ReapplyPersistedPropsPassability(persistedProps);
			logStep("ReapplyPersistedPropsPassability");
			Choreographer.s_Choreographer.m_CurrentState = ScenarioManager.NewFastReloadState;
			MF.EnableYieldIfOverBudget = false;
			await Choreographer.s_Choreographer.ScenarioCreateClientBoardAndCharacters(null);
			MF.EnableYieldIfOverBudget = true;
			logStep("ScenarioCreateClientBoardAndCharacters");
			GameState.Start();
			logStep("GameState.Start");
			UnityGameEditorRuntime.CacheDataFromCampaign();
			UnityGameEditorRuntime.ApplyMapData();
			logStep("UnityGameEditorRuntime.ApplyMapData");
			UnityGameEditorRuntime.InitObjectivesUI();
			GameState.OnAfterLoad();
			Singleton<CardsHandManager>.InstanceFast?.OnAfterLoad();
			Choreographer.s_Choreographer.OnAfterLoad();
			InfusionBoardUI.Instance?.OnAfterLoad();
			logStep("OnAfterLoad");
			UpdateDefaultStrartRoundState();
			Choreographer.s_Choreographer.IsRestarting = false;
			if (GameState.s_ShortRestingPlayer != null)
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.ShortRest);
			}
			else if (GameState.TurnActor != null)
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
			}
			else
			{
				PhaseManager.SetNextPhase(CPhase.PhaseType.SelectAbilityCardsOrLongRest);
				Choreographer.s_Choreographer.PlayerToSelectAbilityCardsOrLongRest = false;
			}
			Singleton<UIReadyToggle>.Instance?.ToggleVisibility(FFSNetwork.IsOnline && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest);
			logStep("PhaseManager.NextStep");
			Choreographer.s_Choreographer.TickUpdate(ignoreTimeQuota: true);
			logStep("Choreographer.Update");
			foreach (UnityGameEditorObject item2 in UnityGameEditorObject.s_all.ToList())
			{
				if (!(item2 == null))
				{
					item2.Start();
				}
			}
			ProceduralMapTile.RepopulateRevealedRooms(savedState);
			Singleton<LevelEventsController>.Instance.Init();
			Singleton<LevelEventsController>.Instance.RemoveAlreadyPlayedMessages();
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
			logStep("WorldspaceStarHexDisplay.RefreshCurrentState");
			InitiativeTrack.Refresh(forceSelectFirstActor: true);
			logStep("InitiativeTrack.Refresh");
			StatsDataStorage.ScrapeEventLogAndCheckBattleGoals(EResult.InProgress, initial: false, endScenario: false, SEventLog.CacheUnprocessedEvents(), ScenarioManager.CurrentScenarioState.RoundNumber);
			SEventLog.ClearUnprocessedEventsCache();
			logStep("Refreshing battle goals/masteries");
			ScenarioScriptFrontend.OnFastLoad();
			foreach (CardsHandUI item3 in Singleton<CardsHandManager>.Instance.cardHandsUI)
			{
				if (!item3.PlayerActor.IsDead)
				{
					item3.itemsUI.SetActor(item3.PlayerActor);
				}
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception);
		}
		ScenarioManager.OldFastReloadState = null;
		ScenarioManager.NewFastReloadState = null;
		Choreographer.s_Choreographer.FaceNearestEnemies();
		if (showUI)
		{
			ProcessingUI.Show(show: false);
		}
		Log.Trace($"Fast Load took {timer.ElapsedMilliseconds}ms");
		IsFastLoading = false;
		void logStep(string stepName)
		{
			Log.Trace($"FastLoad: {stepName}[{++stepCount}] step took {stepTimer.ElapsedMilliseconds}ms");
			stepTimer.Restart();
		}
	}

	public static bool HasPendingFastAutoSaveToLoad(ScenarioState savedState)
	{
		if (s_IgnoreNextLoadAfterNormalLoad)
		{
			return false;
		}
		if (savedState == null)
		{
			return false;
		}
		if (FFSNetwork.IsOnline)
		{
			return false;
		}
		if (!FastUndoEnabled)
		{
			return false;
		}
		return true;
	}

	public static bool HasPendingFastAutoSaveToLoad()
	{
		return HasPendingFastAutoSaveToLoad(SaveData.Instance.Global.CurrentAdventureData?.FastReloadState);
	}

	public static IEnumerator LoadAfterNormalLoad(ScenarioState savedState)
	{
		using (new MiniProfilerScope("FastReload.LoadAfterNormalLoad"))
		{
			UpdateInitialStartRoundState();
			if (!HasPendingFastAutoSaveToLoad(savedState))
			{
				s_IgnoreNextLoadAfterNormalLoad = false;
				yield break;
			}
			s_ignoreNextAutoSave = true;
			yield return null;
			Choreographer.s_Choreographer.TickUpdate(ignoreTimeQuota: true);
			yield return null;
			s_ignoreNextAutoSave = false;
			Task t = Load(savedState, showUI: false);
			while (!t.IsCompleted)
			{
				yield return null;
			}
			Checkpoint checkpoint = new Checkpoint();
			checkpoint.name = GenerateCheckpointName();
			checkpoint.scenarioState = savedState;
			History.Clear();
			History.Add(checkpoint);
			IndexInHistory = 0;
			UpdateInitialStartRoundState();
			UpdateUndoRedoButtons();
		}
	}

	private static void DestroyPropVisuals(CObjectProp prop)
	{
		if (prop is CObjectDoor)
		{
			return;
		}
		GameObject gameObject = Choreographer.FindPropGameObject(prop);
		if (!(gameObject == null))
		{
			Choreographer.TryPlugTerrainHole(prop, gameObject);
			gameObject.GetComponent<UnityGameEditorObject>()?.ReleasePathing();
			EnvironmentLoader.Instance.BeforePropDestroyed(gameObject, prop);
			UnityEngine.Object.Destroy(gameObject);
			if (gameObject != null)
			{
				gameObject.name = "(destroyed) " + gameObject.name;
			}
		}
	}

	private static void DestroyModifiedProps(ScenarioState savedState)
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		List<CObjectProp> list = new List<CObjectProp>();
		foreach (CSpawner spawner in currentScenarioState.Spawners)
		{
			CObjectProp prop = spawner.Prop;
			if (prop != null)
			{
				list.Add(prop);
			}
		}
		foreach (CObjectProp currentProp in currentScenarioState.Props)
		{
			CObjectProp savedProp = savedState.Props.SingleOrDefault((CObjectProp x) => x.PropGuid == currentProp.PropGuid);
			if (IsModifiedProp(currentProp, savedProp))
			{
				list.Add(currentProp);
			}
		}
		foreach (CObjectProp item in list)
		{
			item.DestroyProp(0f, sendMessageToClient: false);
			DestroyPropVisuals(item);
		}
		foreach (CObjectProp activatedProp in ScenarioManager.CurrentScenarioState.ActivatedProps)
		{
			DestroyPropVisuals(activatedProp);
		}
	}

	private static bool IsModifiedProp(CObjectProp currentProp, CObjectProp savedProp)
	{
		if (savedProp == null)
		{
			return true;
		}
		if (currentProp.PrefabName != savedProp.PrefabName)
		{
			return true;
		}
		if (!currentProp.ArrayIndex.Equals(savedProp.ArrayIndex))
		{
			return true;
		}
		if (currentProp.Activated != savedProp.Activated)
		{
			return true;
		}
		if (currentProp is CObjectObstacle cObjectObstacle && savedProp is CObjectObstacle cObjectObstacle2 && !cObjectObstacle.PathingBlockers.SequenceEqual(cObjectObstacle2.PathingBlockers))
		{
			return true;
		}
		return false;
	}

	private static void ReapplyPersistedPropsPassability(List<CObjectProp> persistedProps)
	{
		foreach (CObjectProp persistedProp in persistedProps)
		{
			ReapplyPersistedPropPassability(persistedProp);
		}
	}

	private static void ReapplyPersistedPropPassability(CObjectProp prop)
	{
		if (prop is CObjectObstacle cObjectObstacle)
		{
			if (cObjectObstacle.PathingBlockers == null)
			{
				return;
			}
			{
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					ScenarioManager.PathFinder.GetNode(new Point(pathingBlocker)).Blocked = true;
				}
				return;
			}
		}
		if (prop is CObjectDoor cObjectDoor)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(new Point(cObjectDoor.ArrayIndex));
			node.IsBridge = true;
			node.IsBridgeOpen = cObjectDoor.Activated;
		}
	}
}
