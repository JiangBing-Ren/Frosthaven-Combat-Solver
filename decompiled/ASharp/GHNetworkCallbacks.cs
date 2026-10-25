using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FFSNet.Backend;
using FHRL;
using Libs.Build;
using ScenarioRuleLibrary;
using UnityEngine;

public static class GHNetworkCallbacks
{
	public static bool s_ClientsWaitingForSavePoint;

	public static void StartDone()
	{
		if (FFSNetwork.IsHost)
		{
			foreach (NetworkControllable allControllable in ControllableRegistry.AllControllables)
			{
				if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
				{
					continue;
				}
				if (allControllable.ControllableObject is CharacterManager { CharacterActor: var characterActor })
				{
					CAbilityCard initiativeAbilityCard = ((CPlayerActor)characterActor).CharacterClass.InitiativeAbilityCard;
					List<AbilityCardUI> selectedCards = Singleton<CardsHandManager>.Instance.GetSelectedCards(characterActor.ID);
					if (initiativeAbilityCard != null && selectedCards != null && selectedCards.Count == characterActor.MaxRoundCardsCount)
					{
						string text = "Checking initiative\n";
						text += $"Initiative Card ID: {initiativeAbilityCard.ID} Initiative CardInstanceID: {initiativeAbilityCard.CardInstanceID}\n";
						for (int i = 0; i < selectedCards.Count; i++)
						{
							text += string.Format("SelectedCards[{0}] ID: {1} SelectedCards[{2}] CardInstanceID: {3}\n", i, selectedCards[i].IsLongRest ? "LongRest" : selectedCards[i].AbilityCard.ID.ToString(), i, selectedCards[i].IsLongRest ? "LongRest" : selectedCards[i].AbilityCard.CardInstanceID.ToString());
						}
						Log.NetworkInfo(text);
						if (selectedCards[selectedCards.Count - 1].AbilityCard.ID != initiativeAbilityCard.ID)
						{
							selectedCards.Reverse();
						}
					}
					if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
					{
						Log.NetworkInfo("Round Cards set: " + string.Join(", ", selectedCards.Select((AbilityCardUI s) => s.CardName)));
					}
				}
				else
				{
					Log.Error("Invalid type for controllable.ControllableObject.  Type: " + allControllable.ControllableObject.GetType().ToString());
				}
			}
		}
		if (FFSNetwork.IsHost)
		{
			InitializeGameState();
			{
				foreach (PartySlotID slot in PlayerRegistry.GetSlots())
				{
					PlayerRegistry.AssignPlayerToSlot(PlayerRegistry.MyPlayer, slot);
				}
				return;
			}
		}
		if (FFSNetwork.IsClient)
		{
			PlayerRegistry.MyPlayerInitialized.AddListener(InitializeGameState);
		}
	}

	public static void ShutdownBegin(string disconnectReason)
	{
		if (FFSNetwork.IsHost && !FFSNetwork.HasDesynchronized && !string.IsNullOrEmpty(disconnectReason))
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00038", "Disconnected from the session. Reason: " + disconnectReason);
		}
		Debug.Log("FFSNet shutdown started. Resetting variables.");
		ActionProcessor.Shutdown();
		if (FFSNetwork.IsClient && !SceneController.Instance.IsLoading && SaveData.Instance.Global.CurrentGameState != EGameState.None && !SceneController.Instance.GlobalErrorMessage.ShowingMessage)
		{
			Singleton<ESCMenu>.Instance.LoadMainMenu(skipConfirmation: true);
		}
	}

	private static bool CheckVersions(string clientName, string tokenVersion)
	{
		if (tokenVersion == RuntimeBuildInfo.BuildVersion)
		{
			return true;
		}
		Log.NetworkWarning("Connected client '" + clientName + "' version '" + tokenVersion + "' does not match host version " + RuntimeBuildInfo.BuildVersion);
		return false;
	}

	public static void ConnectRequest(NetworkBackendConnectRequest request)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			request.Refuse(new ConnectionErrorToken(ConnectionErrorCode.SessionShuttingDown));
			return;
		}
		if (request.UserToken == null)
		{
			request.Refuse(new ConnectionErrorToken(ConnectionErrorCode.InvalidUserData));
			return;
		}
		if (!CheckVersions(request.UserToken.Username, request.UserToken.GameVersion))
		{
			request.Refuse(new ConnectionErrorToken(ConnectionErrorCode.DifferentVersion));
			return;
		}
		if (!PassesGHPlayerLimitationChecks())
		{
			request.Refuse(new ConnectionErrorToken(ConnectionErrorCode.SessionFull));
			return;
		}
		if (!PassesBasicConnectionTests(request.UserToken, string.Empty, out var error))
		{
			request.Refuse(error);
			return;
		}
		if (!CheckVersions(request.UserToken.Username, request.UserToken.GameVersion))
		{
			request.Refuse(new ConnectionErrorToken(ConnectionErrorCode.DifferentVersion));
		}
		PlayerRegistry.ConnectingUsers.Add(request.UserToken);
		PlayerRegistry.OnUserEnterRoom?.Invoke(request.UserToken);
		GameToken gameToken = GenerateGameToken();
		if (gameToken == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00013", "Error accepting a connection. Playing non-multiplayer game mode (" + SaveData.Instance.Global.GameMode.ToString() + "). Disconnecting.");
			error = new ConnectionErrorToken(ConnectionErrorCode.SessionNotFound);
			FFSNetwork.Shutdown();
			PlayerRegistry.ConnectingUsers.RemoveAll((UserToken x) => x.Username == request.UserToken.Username && x.PlatformPlayerID == request.UserToken.PlatformPlayerID);
			return;
		}
		Log.Info(request.UserToken.Username + " User connecting at phase " + ActionProcessor.CurrentPhase.ToString() + " has to wait? " + gameToken.WaitUntilSavePoint + ", connecting " + string.Join(",", PlayerRegistry.ConnectingUsers.Select((UserToken it) => it.Username)));
		request.Accept();
	}

	public static bool ShouldWaitForSavePoint()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast != null)
		{
			if (Singleton<FHMapChoreographer>.InstanceFast.View != null && !Singleton<FHMapChoreographer>.InstanceFast.View.ShouldAllowPlayersToJoin())
			{
				return true;
			}
			return false;
		}
		if (FastReload.WaitingForUndo)
		{
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
		{
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ShortRest)
		{
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			return !DesyncDetector.IsAtCheckpointPhase(forJoining: true);
		}
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			if (SceneController.Instance.SelectingPersonalQuest)
			{
				return true;
			}
			if (SceneController.Instance.BusyProcessingResults)
			{
				return true;
			}
			if (SceneController.Instance.RetiringCharacter)
			{
				return true;
			}
			if (SceneController.Instance.CheckingLockedContent)
			{
				return true;
			}
		}
		if (!ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.StartOfRound))
		{
			return true;
		}
		return false;
	}

	public static void CheckForSavePointReached()
	{
		if (s_ClientsWaitingForSavePoint && !ShouldWaitForSavePoint())
		{
			Synchronizer.NotifyJoiningPlayersAboutReachingSavePoint();
		}
	}

	public static GameToken GenerateGameToken()
	{
		string empty = string.Empty;
		bool flag = ShouldWaitForSavePoint();
		if (flag)
		{
			s_ClientsWaitingForSavePoint = true;
		}
		switch (SaveData.Instance.Global.GameMode)
		{
		case EGameMode.Campaign:
			empty = SaveData.Instance.Global.CurrentAdventureData.PartyName;
			break;
		case EGameMode.SingleScenario:
			empty = SaveData.Instance.Global.CurrentCustomLevelData.Name;
			break;
		default:
			return null;
		}
		return new GameToken((int)SaveData.Instance.Global.GameMode, empty, null, PlatformLayer.UserData.PlatformPlayerID, PlatformLayer.UserData.PlatformAccountID, PlatformLayer.UserData.UserName, SaveData.Instance.Global.RegularSpeedMultiplier, SaveData.Instance.Global.ChooseAdvantageModifiers, SaveData.Instance.Global.ConfirmAttackAbilityBeforeFinishing, 0u, (int)SaveData.Instance.Global.CurrentAdventureData.Rules, SaveData.Instance.Global.CurrentAdventureData.IsIronMan, flag);
	}

	private static bool PassesGHPlayerLimitationChecks()
	{
		Log.Info("Starting PassesGHPlayerLimitationChecks");
		if (SaveData.Instance == null || SaveData.Instance.Global == null)
		{
			return false;
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario || Singleton<FHMapChoreographer>.InstanceFast != null)
		{
			uint num = SaveData.Instance.Global.CurrentAdventureData?.FrosthavenCampaign?.Settings.MaxPartySize ?? 4;
			bool flag = PlayerRegistry.AllPlayers.Count + PlayerRegistry.JoiningPlayers.Count <= num;
			Log.Info("PassesGHPlayerLimitationChecks " + (flag ? "Passed" : ("Failed!  AllPlayers.Count: " + PlayerRegistry.AllPlayers.Count + "  JoiningPlayers: " + PlayerRegistry.JoiningPlayers.Count + "  maxCharacters: " + num)));
			return flag;
		}
		Log.Info("PassesGHPlayerLimitationChecks Failed.  FHMapChoreographer Instance is null.");
		return false;
	}

	private static void InitializeGameState()
	{
	}

	private static void SaveLatestProcessedActionIDToSaveFile()
	{
	}

	private static bool PassesBasicConnectionTests(UserToken userToken, string correctPassword, out ConnectionErrorToken error)
	{
		error = null;
		if (PlayerRegistry.AllPlayers.Count >= FFSNetwork.Manager.MaxPlayers)
		{
			Debug.Log("PassesBasicConnectionTests Failed! AllPlayers: " + PlayerRegistry.AllPlayers.Count + "  MaxPlayers: " + FFSNetwork.Manager.MaxPlayers);
			error = new ConnectionErrorToken(ConnectionErrorCode.SessionFull);
			return false;
		}
		if (!userToken.ServerPassword.Equals(correctPassword))
		{
			error = new ConnectionErrorToken(ConnectionErrorCode.IncorrectPassword);
			return false;
		}
		if (userToken.Username.In("TEMPACCOXXX", "Bl0ckedH4xor"))
		{
			error = new ConnectionErrorToken(ConnectionErrorCode.UserBlocked);
			return false;
		}
		return true;
	}

	public static void OnEvent(PlayerAvatarStreamingDataToken evnt, NetworkBackendPlayer raisedBy)
	{
		PlayerRegistry.GetPlayer(evnt.PlayerID)?.AvatarStreaming(evnt);
	}

	public static void OnEvent(NetworkActionEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			if (!FFSNetwork.IsOnline)
			{
				Log.NetworkInfo("Received a NetworkActionEvent but FFSNetwork is not online.");
			}
			if (FFSNetwork.HasDesynchronized)
			{
				Log.NetworkInfo("Received a NetworkActionEvent but FFSNetwork has desynchronized.");
			}
		}
		else if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a NetworkActionEvent but the event returns null."));
		}
		else if (evnt.Token == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a NetworkActionEvent but the token provided returns null."));
		}
		else
		{
			Log.NetworkInfo("Received a NetworkAction event: " + (GameActionType)((NetworkAction)evnt.Token).ActionTypeID/*cast due to constrained. prefix*/);
			ActionProcessor.ProcessSideAction(new GameAction(evnt));
		}
	}

	public static void OnEvent(GameActionEventClassIDToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			return;
		}
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a GameActionEventClassID but the event returns null."));
			return;
		}
		NetworkPlayer player = PlayerRegistry.GetPlayer(evnt.PlayerID);
		if (player != null && player != PlayerRegistry.MyPlayer)
		{
			player.LastActionID = evnt.ActionID;
		}
		DesyncDetector.ClearPendingHostHash();
		DesyncDetector.DelaySyncChecksOnHost();
		GameAction gameAction = new GameAction(evnt);
		Log.NetworkInfo($"[Core] Received GameActionEventClassID GameAction {gameAction}");
		ActionProcessor.QueueUpAction(gameAction);
	}

	public static void OnEvent(GameActionEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			return;
		}
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a GameActionEvent but the event returns null."));
			return;
		}
		NetworkPlayer player = PlayerRegistry.GetPlayer(evnt.PlayerID);
		if (player != null && player != PlayerRegistry.MyPlayer)
		{
			player.LastActionID = evnt.ActionID;
		}
		DesyncDetector.ClearPendingHostHash();
		DesyncDetector.DelaySyncChecksOnHost();
		GameAction gameAction = new GameAction(evnt);
		Log.NetworkInfo($"[Core] Received GameAction {gameAction}");
		ActionProcessor.QueueUpAction(gameAction);
	}
}
