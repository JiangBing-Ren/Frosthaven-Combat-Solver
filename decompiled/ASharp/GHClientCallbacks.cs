using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFSNet;
using FFSNet.Backend;
using FHRL;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;

public static class GHClientCallbacks
{
	public enum EStreamMode
	{
		NotSet,
		ReceivingCompareState,
		ReceivingInitialGameState,
		ReceivingCurrentGameState,
		ReceivingPlayerLog,
		ReceivingPlayerLogForReport,
		ReceivingStateDump,
		RecievingFHGameState
	}

	private static EStreamMode m_CurrentStreamMode;

	public static Action<byte[]> OnHostPlayerLogForReportReceived;

	private static GameToken gameData;

	private static string targetSaveFilePath;

	private static FHSaveData initialSaveData;

	public static EStreamMode CurrentStreamMode
	{
		get
		{
			return m_CurrentStreamMode;
		}
		set
		{
			if (m_CurrentStreamMode != value)
			{
				Log.NetworkInfo($"Setting current stream mode from {m_CurrentStreamMode} to {value}\n{Environment.StackTrace}");
				m_CurrentStreamMode = value;
			}
		}
	}

	public static bool IsStreamInProgress { get; private set; }

	public static async void Connected(GameToken gameToken)
	{
		gameData = gameToken;
		Log.NetworkInfo("Connected. My Connection: ");
		CurrentStreamMode = EStreamMode.NotSet;
		IsStreamInProgress = false;
		if (gameData == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00006", "Invalid session data provided. Disconnecting from the session.");
			FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.GameConnectionFailed, ConnectionErrorCode.InvalidSessionData);
			FFSNetwork.Shutdown();
			return;
		}
		Choreographer.SetChooseAdvantageModifiers(gameToken.ChooseAdvantageModifiers, force: true);
		Choreographer.SetConfirmAttackAbilityFinished(gameToken.ConfirmAttackAbilityBeforeFinishing, force: true);
		SaveData.Instance.Global.MultiplayerRegularSpeedMultiplier = gameToken.RegularSpeedMultiplier;
		FastReload.FastUndoEnabled = gameToken.FastUndoEnabled;
		FastReload.FastRedoEnabled = gameToken.FastRedoEnabled;
		EGameMode gameMode = (EGameMode)gameData.GameModeID;
		Log.NetworkInfo("Receiving game data from an on-going session.");
		Log.NetworkInfo("Game Mode : " + gameMode);
		Log.NetworkInfo("Save Name: " + gameData.SaveName);
		Log.NetworkInfo("Fast Undo: " + (FastReload.FastUndoEnabled ? "Enabled" : "Disabled"));
		if (!PlayerRegistry.Initialized)
		{
			await PlayerRegistry.InitializeTask();
		}
		if (gameMode.In(EGameMode.Campaign, EGameMode.SingleScenario))
		{
			targetSaveFilePath = await SaveData.Instance.RootData.GetFilePathForParty((EGameMode)gameData.GameModeID, gameData.SaveName, gameData.HostAccountID);
			Log.NetworkInfo("Target file Path: " + targetSaveFilePath);
			if (targetSaveFilePath.IsNullOrEmpty())
			{
				Log.NetworkError("ERROR_MULTIPLAYER_00007", "Could not process the target save file path. Disconnecting from the session.");
				FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.GameConnectionFailed, ConnectionErrorCode.InvalidFilePath);
				FFSNetwork.Shutdown();
			}
			else if (gameData.WaitUntilSavePoint)
			{
				Log.Info("Stopped WaitUntilSavePoint");
				FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.ChangeConnectionState, ConnectionState.WaitUntilSavePoint);
			}
			else
			{
				ProceedToDownloadSaveFile();
			}
		}
		else
		{
			Log.Error($"Trying to launch multiplayer save in game mode {gameMode}");
			TryLaunchMultiplayerSave(gameData);
		}
	}

	public static void Disconnected(DisconnectionErrorToken disconnectToken)
	{
		IsStreamInProgress = false;
		CurrentStreamMode = EStreamMode.NotSet;
		string text = string.Empty;
		DisconnectionErrorCode disconnectionErrorCode;
		if (disconnectToken != null)
		{
			disconnectionErrorCode = (DisconnectionErrorCode)disconnectToken.ErrorCode;
			text = ((DisconnectionErrorCode)disconnectToken.ErrorCode/*cast due to constrained. prefix*/).ToString();
		}
		else
		{
			disconnectionErrorCode = DisconnectionErrorCode.None;
		}
		switch (disconnectionErrorCode)
		{
		case DisconnectionErrorCode.HostEndedSession:
			Log.NetworkInfo("Host ended the session.");
			return;
		case DisconnectionErrorCode.Desynchronization:
			Log.NetworkInfo("Disconnected by the host. Host desynchronized.");
			return;
		case DisconnectionErrorCode.NetworkBackendError:
			Log.NetworkInfo("Disconnected by network backend: network backend error.");
			return;
		}
		if (text == "Disconnected")
		{
			Log.NetworkInfo("Session ended.");
		}
		else
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00038", "Disconnected from the session. Reason: " + text);
		}
	}

	public static void OnEvent(PlayerLeftToken evnt, NetworkBackendPlayer raisdBy)
	{
		PlayerRegistry.RemovePlayer(new PlayerID(evnt.PlayerID));
	}

	public static void SessionConnected()
	{
		Log.NetworkInfo("Connected to session: ");
	}

	public static void SessionConnectFailed(ConnectionErrorCode error)
	{
		Log.NetworkError("ERROR_MULTIPLAYER_00017", $"Failed to connect to session: {error} ");
		if (FFSNetwork.Manager.AutoShutdownUponJoiningFailed)
		{
			FFSNetwork.Shutdown();
		}
	}

	public static void OnEvent(SlotStateToken evnt, NetworkBackendPlayer raisdBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a SlotStateToken but the event returns null."));
		}
		else
		{
			PlayerRegistry.ApplySlotState(evnt);
		}
	}

	public static void OnEvent(PlayerToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a PlayerToken but the event returns null."));
		}
		else
		{
			PlayerRegistry.CreateNetworkPlayer(evnt);
		}
	}

	public static void OnEvent(ControllableAssignmentEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableAssignmentEvent but the event returns null."));
			return;
		}
		throw new InvalidOperationException("TODO");
	}

	public static void OnEvent(ControllableReleaseEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableReleaseEvent but the event returns null."));
			return;
		}
		PlayerRegistry.GetPlayer(evnt.PlayerID);
		throw new InvalidOperationException("TODO");
	}

	public static void OnEvent(ControllableDestructionEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a ControllableReleaseEvent but the event returns null."));
		}
		else
		{
			ControllableRegistry.DestroyControllable(evnt.ControllableID);
		}
	}

	public static void OnEvent(SavePointReachedEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a SavePointReachedEvent but the event returns null."));
		}
		else if (UIMultiplayerJoinSessionWindow.s_This != null && UIMultiplayerJoinSessionWindow.s_This.CurrentConnectionState == ConnectionState.WaitUntilSavePoint)
		{
			FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.ChangeConnectionState, ConnectionState.SavePointReached);
			ProceedToDownloadSaveFile(autoFailSaveComparison: true);
		}
	}

	private static void ProceedToDownloadSaveFile(bool autoFailSaveComparison = false)
	{
		Log.NetworkInfo("Waiting for the host to start streaming the save file.");
		FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.ChangeConnectionState, ConnectionState.DownloadingNewSave);
		CurrentStreamMode = EStreamMode.ReceivingInitialGameState;
		Synchronizer.RequestCustomData(DataActionType.SendInitialGameState);
	}

	public static async void OnEvent(GameDataRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00010", "Received a GameDataRequest but the request returns null.");
		}
		else if (raisedBy == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00011", "Received a GameDataRequest for " + ((DataActionType)request.DataActionID/*cast due to constrained. prefix*/).ToString() + " but the requesting player does not exist anymore.");
		}
		else if (request.DataActionID == 5)
		{
			string text = await RootSaveData.PlayerLogPath;
			if (File.Exists(text))
			{
				byte[] playerLogBytes = GetBytesFromFilePath(text);
				Synchronizer.StreamCompressedCustomData(await Task.Run(() => FFSNet.Utility.CompressData(playerLogBytes)));
			}
		}
		else
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00012", "Unprocessable DataActionType found: " + (DataActionType)request.DataActionID/*cast due to constrained. prefix*/);
		}
	}

	private static byte[] GetBytesFromFilePath(string pathToOutputLog)
	{
		using FileStream fileStream = File.Open(pathToOutputLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using MemoryStream memoryStream = new MemoryStream();
		fileStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private static async Task LaunchMultiplayerFromCurrentState(byte[] stateData)
	{
		if (stateData != null && stateData.Length != 0)
		{
			using MemoryStream stream = new MemoryStream(stateData);
			UnityGameEditorRuntime.s_OverrideCurrentScenarioState = await GloomUtility.BinaryDeserialize<ScenarioRuleLibrary.ScenarioState>(stream);
			UnityGameEditorRuntime.s_SkipNextAutoSave = true;
		}
		TryLaunchMultiplayerSave(gameData);
	}

	private static bool CompareScenarioStates(ScenarioRuleLibrary.ScenarioState receivedScenarioState)
	{
		try
		{
			List<Tuple<int, string>> list = ScenarioRuleLibrary.ScenarioState.CompareStates(ScenarioManager.CurrentScenarioState, receivedScenarioState, isMPCompare: true);
			if (list.Count == 0)
			{
				Log.NetworkInfo("SCENARIO STATE COMPARISONS PASSED.");
				return true;
			}
			Log.NetworkError("", "Scenario state mismatches:");
			foreach (Tuple<int, string> item in list)
			{
				Log.NetworkError("", "Error # " + item.Item1 + ": " + item.Item2);
			}
			Log.NetworkWarning("SCENARIO STATE MISMATCHES LOGGED.");
			return false;
		}
		catch (Exception exception)
		{
			Log.NetworkException(exception, "Exception trying to do scenario state comparison");
			return false;
		}
	}

	private static void TryLaunchMultiplayerSave(GameToken gameData)
	{
		try
		{
			Log.NetworkInfo("Host enabled DLC flag on the save file received: " + (DLCRegistry.EDLCKey)gameData.DLCFlag/*cast due to constrained. prefix*/);
			switch ((EGameMode)gameData.GameModeID)
			{
			case EGameMode.Campaign:
			{
				Log.NetworkInfo("Continuing campaign mode: " + gameData.SaveName);
				FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.GameConnectionEstablished, gameData);
				PartyAdventureData partyAdventureData2 = PartyAdventureData.GenerateDataFromFHSave(initialSaveData, targetSaveFilePath, DateTime.Now, gameData.SaveName, new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostUsername), EGameMode.Campaign, (ERulesFlag)gameData.CampaignRules, gameData.IsIronMan);
				if (partyAdventureData2.AdventureMapState?.CurrentMapScenarioState?.CurrentState != null)
				{
					FastReload.ResetToStartRoundState(partyAdventureData2.AdventureMapState.CurrentMapScenarioState.CurrentState);
				}
				SaveData.Instance.LoadCampaignMode(partyAdventureData2, isJoiningMPClient: true);
				break;
			}
			case EGameMode.SingleScenario:
			{
				Log.NetworkInfo("Continuing custom level: " + gameData.SaveName);
				FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.GameConnectionEstablished, gameData);
				PartyAdventureData partyAdventureData = PartyAdventureData.GenerateDataFromFHSave(initialSaveData, targetSaveFilePath, DateTime.Now, gameData.SaveName, new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostUsername), EGameMode.SingleScenario, (ERulesFlag)gameData.CampaignRules, gameData.IsIronMan);
				SaveData.Instance.Global.CurrentAdventureData = partyAdventureData;
				SaveData.Instance.Global.CurrentHostAccountID = partyAdventureData.Owner.PlatformAccountID;
				if (partyAdventureData.AdventureMapState?.CurrentSingleScenario?.ScenarioState != null)
				{
					FastReload.ResetToStartRoundState(partyAdventureData.AdventureMapState.CurrentSingleScenario.ScenarioState);
				}
				SceneController.Instance.LoadSingleScenario(partyAdventureData);
				break;
			}
			default:
				Log.NetworkError("ERROR_MULTIPLAYER_00008", "Error launching multiplayer game. Unsuitable or unfinished game mode found: " + (EGameMode)gameData.GameModeID/*cast due to constrained. prefix*/);
				FFSNetwork.QueueNetworkEvent(NetworkBackendEvent.Type.GameConnectionFailed, ConnectionErrorCode.InvalidGameMode);
				FFSNetwork.Shutdown();
				break;
			}
		}
		catch (Exception ex)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00009", "Exception trying to launch multiplayer save:\n" + ex);
			FFSNetwork.Shutdown();
		}
		initialSaveData = null;
	}

	public static void StreamDataStarted()
	{
		Log.Info("Starting Data Stream");
		IsStreamInProgress = true;
		if (CurrentStreamMode == EStreamMode.RecievingFHGameState)
		{
			Singleton<FHMapChoreographer>.InstanceFast?.OnClientRecievingGameState(0f);
		}
	}

	public static void StreamDataProgress(float progress)
	{
		Log.Info("Stream Progress: " + progress);
	}

	public static void StreamDataAborted()
	{
		IsStreamInProgress = false;
		Log.NetworkError("ERROR_MULTIPLAYER_00009", "Data stream aborted");
		FFSNetwork.Shutdown();
		if (CurrentStreamMode == EStreamMode.RecievingFHGameState)
		{
			Singleton<FHMapChoreographer>.InstanceFast?.OnClientRecievingGameStateAborted();
		}
	}

	public static async void StreamDataReceived(byte[] data)
	{
		Log.Info("Stream completed");
		IsStreamInProgress = false;
		switch (CurrentStreamMode)
		{
		case EStreamMode.ReceivingPlayerLog:
		{
			string path = await RootSaveData.PlayerLogPathFromHost;
			CurrentStreamMode = EStreamMode.NotSet;
			byte[] array = FFSNet.Utility.DecompressData(data);
			File.WriteAllBytes(path, array);
			DesyncDetector.OnHostPlayerLogReceived(array);
			break;
		}
		case EStreamMode.ReceivingPlayerLogForReport:
		{
			CurrentStreamMode = EStreamMode.NotSet;
			byte[] obj = FFSNet.Utility.DecompressData(data);
			Action<byte[]> onHostPlayerLogForReportReceived = OnHostPlayerLogForReportReceived;
			OnHostPlayerLogForReportReceived = null;
			onHostPlayerLogForReportReceived?.Invoke(obj);
			break;
		}
		case EStreamMode.ReceivingStateDump:
			CurrentStreamMode = EStreamMode.NotSet;
			DesyncDetector.OnHostStateDumpReceived(FFSNet.Utility.DecompressData(data));
			break;
		case EStreamMode.ReceivingCompareState:
		{
			CurrentStreamMode = EStreamMode.NotSet;
			bool flag = CompareScenarioStates((ScenarioRuleLibrary.ScenarioState)FFSNet.Utility.ByteArrayToObject(FFSNet.Utility.DecompressData(data)));
			if (SceneController.Instance.IsLoading)
			{
				Synchronizer.SendSideAction(GameActionType.EndOfRoundCompareClientFinished, null, canBeUnreliable: false, sendToHostOnly: true);
				DebugMenu.Instance.DisplayDebugText(1, "State comparison: " + (flag ? "PASSED" : "FAILED"));
				if (!flag)
				{
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MULTIPLAYER_00037", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
				}
			}
			break;
		}
		case EStreamMode.ReceivingInitialGameState:
		{
			Log.NetworkInfo("Waiting for the host to start streaming current game state.");
			byte[] buffer = FFSNet.Utility.DecompressData(data);
			using MemoryStream stream = new MemoryStream(buffer);
			initialSaveData = await GloomUtility.BinaryDeserialize<FHSaveData>(stream);
			CurrentStreamMode = EStreamMode.ReceivingCurrentGameState;
			Synchronizer.RequestCustomData(DataActionType.SendCurrentGameState);
			break;
		}
		case EStreamMode.ReceivingCurrentGameState:
			CurrentStreamMode = EStreamMode.NotSet;
			LaunchMultiplayerFromCurrentState(FFSNet.Utility.DecompressData(data));
			break;
		case EStreamMode.RecievingFHGameState:
		{
			CurrentStreamMode = EStreamMode.NotSet;
			MultiplayerGameState state = FHGlue.BinaryDeserialzieState(FFSNet.Utility.DecompressData(data), Singleton<FHMapChoreographer>.InstanceFast.DefRepo);
			Singleton<FHMapChoreographer>.InstanceFast.OnClientGameStateRecieved(state);
			break;
		}
		default:
			CurrentStreamMode = EStreamMode.NotSet;
			Log.NetworkError("ERROR_MULTIPLAYER_00009", "Invalid data stream mode set");
			if (Singleton<FHMapChoreographer>.InstanceFast == null && Choreographer.s_Choreographer == null)
			{
				FFSNetwork.Shutdown();
			}
			break;
		}
	}
}
