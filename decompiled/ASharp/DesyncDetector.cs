using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FFSNet;
using Libs.GameConsole;
using ReportIssueGui;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public static class DesyncDetector
{
	public enum EReportState
	{
		None,
		DownloadingStateDump,
		DownloadingHostPlayerLog,
		FillingInReport,
		Disconnected
	}

	[ConsoleVariable(Alias = "report_multiplayer_desyncs", Persistent = false)]
	public static int ReportMultiplayerDesyncs = 2;

	public static List<StateHashToken> s_StateHashHistory = new List<StateHashToken>();

	public static byte[] s_LastStateDump;

	private static StateHashToken s_PendingHostHash = null;

	private static Stopwatch s_PendingTimer = null;

	public static string s_DiffTitle;

	public static string s_DiffDescription;

	public static byte[] s_ClientStateDump;

	public static byte[] s_HostStateDump;

	public static byte[] s_ClientFirstStateDump;

	public static byte[] s_HostFirstStateDump;

	public static byte[] s_HostPlayerLog;

	public static byte[] s_WaitingForHostPlayerLog = Encoding.UTF8.GetBytes("<<< waiting for host player log >>>");

	public static byte[] s_DisconnectedHostPlayerLog = Encoding.UTF8.GetBytes("<<< host disconnected >>>");

	private static int CurrentActionID;

	public static bool s_IgnoreNextHash;

	private static bool s_DelayedSyncCheckRequested;

	private static double s_DelayedSyncChecksTime;

	private static int s_lastFrameUIPreventedDesyncCheck;

	private static List<(NetworkPlayer player, EReportState state)> s_playersReportingDesync = new List<(NetworkPlayer, EReportState)>();

	private static bool s_reportingDesyncUIShown = false;

	private static CActor CurrentActor
	{
		get
		{
			if (PhaseManager.PhaseType != CPhase.PhaseType.ShortRest)
			{
				return GameState.TurnActor;
			}
			return GameState.s_ShortRestingPlayer;
		}
	}

	private static ECharacter CurrentPlayerID
	{
		get
		{
			if (!(CurrentActor is CPlayerActor cPlayerActor))
			{
				return ECharacter.None;
			}
			return cPlayerActor.CharacterClass.CharacterModel;
		}
	}

	public static bool ReportUIEnabled => ReportIssueWindow.ForceExtendedVersion;

	public static void Reset(bool full = true)
	{
		s_DiffTitle = null;
		s_DiffDescription = null;
		s_ClientStateDump = null;
		s_HostStateDump = null;
		s_ClientFirstStateDump = null;
		s_HostFirstStateDump = null;
		s_HostPlayerLog = null;
		s_LastStateDump = null;
		if (full)
		{
			s_StateHashHistory.Clear();
		}
	}

	public static void CheckForDesync(bool isAtCheckpoint = true)
	{
		if (isAtCheckpoint)
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				CurrentActionID = 0;
				s_StateHashHistory.Clear();
			}
			else
			{
				CurrentActionID++;
				s_DelayedSyncCheckRequested = false;
			}
		}
		if (!IsAtCheckpointPhase(forJoining: false))
		{
			DelaySyncChecksOnHost();
			if (isAtCheckpoint)
			{
				RequestDelayedCheckOnHost();
			}
			if (isAtCheckpoint)
			{
				FastReload.AutoSaveCheckPointMultiThreaded();
			}
			return;
		}
		if (!FFSNetwork.IsOnline)
		{
			if (isAtCheckpoint)
			{
				FastReload.AutoSaveCheckPointMultiThreaded();
			}
			return;
		}
		StateHashToken token = null;
		if (isAtCheckpoint && (FFSNetwork.IsHost || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest))
		{
			token = GenerateStateDumpAndFastAutoSave();
		}
		if (!FFSNetwork.IsHost)
		{
			CheckPendingHostHash();
		}
		else if (!isAtCheckpoint)
		{
			TryDelayedCheck();
		}
		else
		{
			SendHostHash(token);
		}
	}

	public static void DelaySyncChecksOnHost(float after = 0.5f)
	{
		if (FFSNetwork.IsHost)
		{
			double num = Time.unscaledTimeAsDouble + (double)after;
			if (s_DelayedSyncChecksTime < num)
			{
				s_DelayedSyncChecksTime = num;
			}
		}
	}

	public static void RequestDelayedCheckOnHost(bool aggressive = false)
	{
		if (FFSNetwork.IsHost && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			s_DelayedSyncCheckRequested = true;
		}
	}

	public static bool IsUIPreventingDesyncCheck()
	{
		if (ActionProcessor.CurrentPhase != ActionPhaseType.StartOfRound)
		{
			return true;
		}
		Choreographer s_Choreographer = Choreographer.s_Choreographer;
		if ((object)s_Choreographer != null && s_Choreographer.readyButton?.IsInteractable == true)
		{
			return true;
		}
		CardsHandManager instance = Singleton<CardsHandManager>.Instance;
		if ((object)instance == null || instance.CurrentHand?.currentMode != CardHandMode.CardsSelection)
		{
			return true;
		}
		DialogPopup dialogPopup = UIManager.Instance?.dialogPopup;
		if (dialogPopup != null && dialogPopup.isActiveAndEnabled)
		{
			return true;
		}
		return false;
	}

	public static bool IsAtCheckpointPhase(bool forJoining)
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return false;
		}
		if (ScenarioManager.IsFastReloading & forJoining)
		{
			return false;
		}
		if (ScenarioManager.CurrentScenarioState.ScenarioEnded)
		{
			return false;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			if (ScenarioRuleClient.IsProcessingOrMessagesQueued)
			{
				return false;
			}
			if (Choreographer.IsProcessingOrMessagesQueued)
			{
				return false;
			}
			if (ActionProcessor.IsProcessing)
			{
				return false;
			}
			if (ActionProcessor.ReadyToProcessNextAction && ActionProcessor.ActionQueue.Count > 0)
			{
				return false;
			}
			int frameCount = Time.frameCount;
			if (IsUIPreventingDesyncCheck())
			{
				s_lastFrameUIPreventedDesyncCheck = frameCount;
			}
			if (frameCount - s_lastFrameUIPreventedDesyncCheck < 5)
			{
				return false;
			}
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ShortRest && !ScenarioManager.CurrentScenarioState.FreeShortRest && !ScenarioManager.CurrentScenarioState.FreeShortRestButMuddle)
		{
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			return true;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection)
		{
			return GameState.TurnActor is CPlayerActor;
		}
		return false;
	}

	private static void SendHostHash(StateHashToken token)
	{
		if (token == null)
		{
			return;
		}
		DelaySyncChecksOnHost();
		Log.Info($"Sending host hash: {token}");
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			Synchronizer.SendSideAction(GameActionType.StateHash, token);
			return;
		}
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer != PlayerRegistry.MyPlayer)
			{
				StateHashToken actionDataToken = new StateHashToken(token, allPlayer.LastActionID);
				FFSNetwork.SendTokenToClient(new NetworkActionEventToken
				{
					Token = new NetworkAction(GameActionType.StateHash, PlayerRegistry.MyPlayer, actionDataToken)
				}, allPlayer.PlayerID);
			}
		}
	}

	private static void TryDelayedCheck()
	{
		if (s_DelayedSyncCheckRequested)
		{
			if (ReportMultiplayerDesyncs <= 0)
			{
				s_DelayedSyncCheckRequested = false;
			}
			else if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				s_DelayedSyncCheckRequested = false;
			}
			else if (!(Time.unscaledTimeAsDouble < s_DelayedSyncChecksTime))
			{
				StateHashToken token = GenerateStateDumpAndFastAutoSave();
				s_DelayedSyncCheckRequested = false;
				SendHostHash(token);
			}
		}
	}

	public static void ProxyRequestSyncCheck(GameAction action)
	{
		RequestDelayedCheckOnHost();
	}

	public static void OnStateHashReceived(GameAction action)
	{
		if (!(action.SupplementaryDataToken is StateHashToken arg))
		{
			Log.Error($"OnStateHashReceived: action token is not StateHashToken: {action.SupplementaryDataToken}");
			return;
		}
		Log.Info($"Received host hash: {arg}");
		if (ReportMultiplayerDesyncs <= 0)
		{
			Log.Info($"Host hash ignored, because ReportMultiplayerDesyncs is {ReportMultiplayerDesyncs}");
			return;
		}
		s_PendingHostHash = arg;
		s_PendingTimer = null;
	}

	public static void ClearPendingHostHash()
	{
		if (s_PendingHostHash != null)
		{
			Log.Info($"Discarding pending host hash: {s_PendingHostHash}");
			s_PendingHostHash = null;
			s_PendingTimer = null;
		}
	}

	public static bool IsCheckingHostHash()
	{
		return s_PendingTimer != null;
	}

	private static void CheckPendingHostHash()
	{
		if (s_PendingHostHash == null)
		{
			return;
		}
		if (ReportMultiplayerDesyncs <= 0)
		{
			s_PendingHostHash = null;
		}
		else
		{
			if (s_ClientStateDump != null)
			{
				return;
			}
			bool flag;
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				if (s_PendingHostHash.lastGameActionID != 0 && s_PendingHostHash.lastGameActionID != PlayerRegistry.MyPlayer.LastActionID)
				{
					Log.Info($"Skipped host hash because of last GameAction ID mismatch ({s_PendingHostHash.lastGameActionID} / {PlayerRegistry.MyPlayer.LastActionID}): {s_PendingHostHash}");
					Synchronizer.SendSideAction(GameActionType.RequestSyncCheck, null, canBeUnreliable: false, sendToHostOnly: true);
					s_PendingHostHash = null;
					s_PendingTimer = null;
					return;
				}
				flag = s_PendingHostHash.round == GameState.RoundCount && s_PendingHostHash.phaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
			}
			else
			{
				StateHashToken stateHashToken = s_StateHashHistory.LastOrDefault();
				flag = s_PendingHostHash.IsSameCheckpoint(stateHashToken);
				if (flag && !IsCurrentState(stateHashToken))
				{
					if (stateHashToken.importantStateHash != s_PendingHostHash.importantStateHash || stateHashToken.fullStateHash != s_PendingHostHash.fullStateHash)
					{
						Log.Error($"Skipped a desync-ed out-of-date host hash: {s_PendingHostHash} <-> {stateHashToken}");
					}
					else
					{
						Log.Info($"Skipped out-of-date host hash: {s_PendingHostHash}");
					}
					s_PendingHostHash = null;
					s_PendingTimer = null;
					return;
				}
			}
			if (!flag)
			{
				for (int num = s_StateHashHistory.Count - 2; num >= 0; num--)
				{
					StateHashToken stateHashToken2 = s_StateHashHistory[num];
					if (stateHashToken2.IsSameCheckpoint(s_PendingHostHash))
					{
						if (stateHashToken2.importantStateHash != s_PendingHostHash.importantStateHash || stateHashToken2.fullStateHash != s_PendingHostHash.fullStateHash)
						{
							Log.Error($"Skipped a desync-ed out-of-date host hash: {s_PendingHostHash} <-> {stateHashToken2}");
						}
						else
						{
							Log.Info($"Skipped out-of-date host hash: {s_PendingHostHash}");
						}
						s_PendingHostHash = null;
						s_PendingTimer = null;
						return;
					}
				}
				if (s_PendingTimer == null)
				{
					Log.Info($"Delaying host hash: {s_PendingHostHash}");
					s_PendingTimer = Stopwatch.StartNew();
				}
				else if (s_PendingTimer.ElapsedMilliseconds >= 5000)
				{
					s_PendingHostHash = null;
					s_PendingTimer = null;
				}
			}
			else
			{
				StateHashToken hostHash = s_PendingHostHash;
				s_PendingHostHash = null;
				s_PendingTimer = null;
				ClientSyncCheck(hostHash);
				s_StateHashHistory.Clear();
				FastReload.UpdateUndoRedoButtons();
			}
		}
	}

	private static void ClientSyncCheck(StateHashToken hostHash)
	{
		Log.Info($"Processing host hash: {hostHash}");
		if (s_IgnoreNextHash)
		{
			s_IgnoreNextHash = false;
			Log.Info($"Ignored host hash {hostHash} because of an event between receiving and processing it");
			Synchronizer.SendSideAction(GameActionType.RequestSyncCheck, null, canBeUnreliable: false, sendToHostOnly: true);
			return;
		}
		StateHashToken stateHashToken = GenerateStateDumpAndFastAutoSave();
		if (stateHashToken == null)
		{
			return;
		}
		bool flag = hostHash.importantStateHash != stateHashToken.importantStateHash;
		bool flag2 = hostHash.fullStateHash != stateHashToken.fullStateHash;
		if (!flag && !flag2)
		{
			if (s_ClientStateDump != null)
			{
				if (ReportUIEnabled)
				{
					ReportIssueWindow.Hide();
				}
				Reset(full: false);
				if (ReportUIEnabled)
				{
					SendClientReportingDesyncState(EReportState.None);
				}
			}
			return;
		}
		s_ClientStateDump = s_LastStateDump;
		bool flag3 = ReportUIEnabled;
		if (!flag)
		{
			if (ReportMultiplayerDesyncs < 2)
			{
				s_ClientStateDump = null;
				return;
			}
			if (ReportMultiplayerDesyncs == 2)
			{
				if (s_ClientFirstStateDump != null)
				{
					s_ClientStateDump = null;
					return;
				}
				flag3 = false;
				s_ClientFirstStateDump = s_ClientStateDump;
			}
		}
		string text = (flag ? "MAJOR" : "Minor");
		Log.Error($"{text} desync detected!\nhost hash: {hostHash}\nclient hash: {stateHashToken}");
		if (flag3)
		{
			s_HostPlayerLog = s_WaitingForHostPlayerLog;
			ReportIssueWindow.ShowDesync(text + " multiplayer desync: <<< waiting for host state dump >>>", "<<< waiting for host state dump >>>", null, null, null, null, s_HostPlayerLog);
			SendClientReportingDesyncState(EReportState.DownloadingStateDump);
		}
		GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingStateDump;
		Synchronizer.RequestCustomData(DataActionType.SendStateDumpFromHost);
	}

	public static void OnHostStateDumpReceived(byte[] hostStateDump)
	{
		if (s_ClientStateDump == null)
		{
			return;
		}
		string text = Encoding.UTF8.GetString(hostStateDump);
		string text2 = Encoding.UTF8.GetString(s_ClientStateDump);
		if (!StateDump.AnalyzeDiff(text, text2, out s_DiffTitle, out s_DiffDescription, out var majorDesync, StateDumper.DelimiterLine))
		{
			if (ReportUIEnabled)
			{
				ReportIssueWindow.Hide();
				Reset(full: false);
				SendClientReportingDesyncState(EReportState.None);
			}
			return;
		}
		string text3 = (majorDesync ? "MAJOR" : "Minor");
		ReproStepsLogger.LogEvent(text3 + " multiplayer desync", s_DiffTitle);
		s_DiffTitle = text3 + " multiplayer desync: " + s_DiffTitle;
		Log.Error(s_DiffTitle + "\n" + s_DiffDescription);
		StateDumper.SaveStateDump(text, "host-" + text3);
		StateDumper.SaveStateDump(text2, "client-" + text3);
		if (!majorDesync)
		{
			if (ReportMultiplayerDesyncs < 2)
			{
				s_ClientStateDump = null;
				return;
			}
			if (ReportMultiplayerDesyncs == 2)
			{
				if (s_HostFirstStateDump != null)
				{
					s_ClientStateDump = null;
					return;
				}
				s_HostFirstStateDump = hostStateDump;
				s_ClientStateDump = null;
				return;
			}
		}
		if (ReportUIEnabled && GetClientReportingDesyncState(PlayerRegistry.MyPlayer) == EReportState.None)
		{
			Reset(full: false);
			return;
		}
		s_HostStateDump = hostStateDump;
		if (ReportUIEnabled)
		{
			ReportIssueWindow.UpdateDesyncInfo(s_DiffTitle, "<<<waiting for host player log>>>\n\n" + s_DiffDescription, s_HostStateDump, s_ClientStateDump, s_HostFirstStateDump, s_ClientFirstStateDump, s_HostPlayerLog);
			SendClientReportingDesyncState(EReportState.DownloadingHostPlayerLog);
		}
		GHClientCallbacks.CurrentStreamMode = GHClientCallbacks.EStreamMode.ReceivingPlayerLog;
		Synchronizer.RequestCustomData(DataActionType.SendPlayerLogFromHost);
	}

	public static void OnHostPlayerLogReceived(byte[] hostPlayerLog)
	{
		if (s_HostStateDump == null)
		{
			return;
		}
		if (!ReportUIEnabled)
		{
			if (ReportMultiplayerDesyncs <= 0)
			{
				Reset(full: false);
				return;
			}
			s_HostPlayerLog = hostPlayerLog;
			FFSNetwork.HandleDesync(new Exception("ERROR_MULTIPLAYER_00037"));
			return;
		}
		if (GetClientReportingDesyncState(PlayerRegistry.MyPlayer) != EReportState.None)
		{
			SendClientReportingDesyncState(EReportState.FillingInReport);
			ReportIssueWindow.UpdateDesyncInfo(s_DiffTitle, s_DiffDescription, s_HostStateDump, s_ClientStateDump, s_HostFirstStateDump, s_ClientFirstStateDump, hostPlayerLog);
		}
		Reset(full: false);
	}

	public static void FillInErrorReport()
	{
		if (s_DiffTitle != null)
		{
			ReportIssueWindow.UpdateDesyncInfo(s_DiffTitle, s_DiffDescription, s_HostStateDump, s_ClientStateDump, s_HostFirstStateDump, s_ClientFirstStateDump, s_HostPlayerLog);
		}
	}

	private static bool IsCurrentState(StateHashToken hashState)
	{
		if (hashState != null && hashState.phaseType == PhaseManager.PhaseType && hashState.playerID == CurrentPlayerID)
		{
			return hashState.actionID == CurrentActionID;
		}
		return false;
	}

	private static StateHashToken GenerateStateDumpAndFastAutoSave()
	{
		if (!FFSNetwork.IsOnline)
		{
			FastReload.AutoSaveCheckPointMultiThreaded();
			return null;
		}
		if (ReportMultiplayerDesyncs <= 0)
		{
			Log.Info($"Host hash not generated, because ReportMultiplayerDesyncs is {ReportMultiplayerDesyncs}");
			FastReload.AutoSaveCheckPointMultiThreaded();
			return null;
		}
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			StateHashToken stateHashToken = s_StateHashHistory.LastOrDefault();
			if (IsCurrentState(stateHashToken))
			{
				return stateHashToken;
			}
		}
		FastReload.PrepareForSave();
		FastReload.Checkpoint checkpoint = ((FastReload.History.Count > 0 && FastReload.IndexInHistory >= 0 && FastReload.IndexInHistory < FastReload.History.Count) ? FastReload.History[FastReload.IndexInHistory] : null);
		FastReload.AutoSave();
		string text = null;
		string text2 = null;
		FastReload.Checkpoint checkpoint2 = ((FastReload.History.Count > 0 && FastReload.IndexInHistory < FastReload.History.Count) ? FastReload.History[FastReload.IndexInHistory] : null);
		if (checkpoint2 != null && checkpoint2 != checkpoint)
		{
			text = checkpoint2.importantStateDump;
			text2 = ((ReportMultiplayerDesyncs < 2) ? null : checkpoint2.fullStateDump);
		}
		else
		{
			text = StateDumper.GenerateImportantStateDump();
			text2 = ((ReportMultiplayerDesyncs < 2) ? null : StateDumper.GenerateFullStateDump());
		}
		if (text2 != null)
		{
			int num = text2.IndexOf("s_Choreographer", StringComparison.Ordinal);
			if (num >= 0)
			{
				text2 = text2.Substring(0, num);
			}
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		string importantStateHash = GenerateStateHash(bytes);
		if (text2 != null)
		{
			string s = StateDumper.CombineStateDumps(text, text2);
			s_LastStateDump = Encoding.UTF8.GetBytes(s);
		}
		else
		{
			s_LastStateDump = bytes;
		}
		string fullStateHash = ((text2 == null) ? null : GenerateStateHash(s_LastStateDump));
		StateHashToken stateHashToken2 = new StateHashToken(importantStateHash, fullStateHash, GameState.RoundCount, PhaseManager.PhaseType, CurrentPlayerID, CurrentActionID);
		Log.Info($"Generated state hash: {stateHashToken2}");
		s_StateHashHistory.Add(stateHashToken2);
		return stateHashToken2;
	}

	private static string GenerateStateHash(byte[] stateDump)
	{
		if (stateDump == null)
		{
			return null;
		}
		using MD5 mD = MD5.Create();
		return BitConverter.ToString(mD.ComputeHash(stateDump)).Replace("-", "");
	}

	private static EReportState GetClientReportingDesyncState(NetworkPlayer player)
	{
		for (int i = 0; i < s_playersReportingDesync.Count; i++)
		{
			var (networkPlayer, result) = s_playersReportingDesync[i];
			if (networkPlayer == player)
			{
				return result;
			}
		}
		return EReportState.None;
	}

	private static void SetClientReportingDesyncState(NetworkPlayer player, EReportState state)
	{
		if (player == null)
		{
			return;
		}
		for (int i = 0; i < s_playersReportingDesync.Count; i++)
		{
			NetworkPlayer item = s_playersReportingDesync[i].player;
			if (item == player)
			{
				if (state == EReportState.None)
				{
					s_playersReportingDesync.RemoveAt(i);
				}
				else
				{
					s_playersReportingDesync[i] = (player, state);
				}
				return;
			}
		}
		s_playersReportingDesync.Add((player, state));
	}

	public static void SendClientReportingDesyncState(EReportState state)
	{
		Synchronizer.SendSideAction(GameActionType.ClientReportingDesyncState, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)state);
		OnClientReportingDesyncState(PlayerRegistry.MyPlayer, state);
	}

	public static void OnClientReportingDesyncState(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			OnClientReportingDesyncState(player, (EReportState)action.DataInt);
		}
	}

	public static void OnClientReportingDesyncState(NetworkPlayer player, EReportState state)
	{
		SetClientReportingDesyncState(player, state);
		UpdateClientReportingDesyncUI();
	}

	public static void OnPlayerDisconnected(NetworkPlayer player)
	{
		if (player == PlayerRegistry.MyPlayer || player == PlayerRegistry.HostPlayer)
		{
			switch (GetClientReportingDesyncState(PlayerRegistry.MyPlayer))
			{
			case EReportState.DownloadingStateDump:
				s_HostPlayerLog = s_DisconnectedHostPlayerLog;
				ReportIssueWindow.UpdateDesyncInfo("Multiplayer desync: <<< host disconnected >>>", "Host disconnected before state dump was received!\nNo useful information gathered!\nPlease manually attach the host player log!", s_HostStateDump, s_ClientStateDump, s_HostFirstStateDump, s_ClientFirstStateDump, s_HostPlayerLog);
				break;
			case EReportState.DownloadingHostPlayerLog:
				s_HostPlayerLog = s_DisconnectedHostPlayerLog;
				ReportIssueWindow.UpdateDesyncInfo(s_DiffTitle, "Host disconnected before player log was received!\nPlease manually attach the host player log!\n\n" + s_DiffDescription, s_HostStateDump, s_ClientStateDump, s_HostFirstStateDump, s_ClientFirstStateDump, s_HostPlayerLog);
				break;
			}
		}
		SetClientReportingDesyncState(player, EReportState.Disconnected);
		UpdateClientReportingDesyncUI();
	}

	public static void OnReportIssueUIHidden()
	{
		if (GetClientReportingDesyncState(PlayerRegistry.MyPlayer) != EReportState.None)
		{
			SendClientReportingDesyncState(EReportState.None);
			Reset(full: false);
		}
	}

	private static void OnClientReportingDesyncUIHidden()
	{
		if (s_reportingDesyncUIShown)
		{
			s_reportingDesyncUIShown = false;
		}
		s_playersReportingDesync.Clear();
	}

	private static void UpdateClientReportingDesyncUI()
	{
		string text = "";
		bool flag = false;
		bool flag2 = false;
		foreach (var (networkPlayer, eReportState) in s_playersReportingDesync)
		{
			if (eReportState != EReportState.None)
			{
				if (!networkPlayer.IsClient || networkPlayer == PlayerRegistry.MyPlayer)
				{
					flag = false;
					break;
				}
				text += $"{networkPlayer.Username}: {eReportState}\n";
				if (eReportState == EReportState.DownloadingStateDump || eReportState == EReportState.DownloadingHostPlayerLog)
				{
					flag = true;
					flag2 = true;
				}
				else
				{
					flag = s_reportingDesyncUIShown;
				}
			}
		}
		if (!flag)
		{
			if (s_reportingDesyncUIShown)
			{
				Singleton<UIConfirmationBoxManager>.Instance.Hide();
			}
			return;
		}
		if (!s_reportingDesyncUIShown)
		{
			s_reportingDesyncUIShown = true;
		}
		UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
		string explanation = text;
		UnityAction onFinished = OnClientReportingDesyncUIHidden;
		instance.ShowGenericCancelConfirmation("Desync Detected", explanation, null, "GUI_CANCEL", onFinished);
		Singleton<UIConfirmationBoxManager>.Instance.SetButtonsInteraction(!flag2);
	}
}
