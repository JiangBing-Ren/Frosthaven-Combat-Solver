using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFSNet.Backend;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FFSNet;

public static class GHHostCallbacks
{
	private static List<string> m_ModCSVs;

	private static int s_FakePlayerLogSize = 0;

	private static readonly Dictionary<int, byte[]> s_ClientLogBuffers = new Dictionary<int, byte[]>();

	private static Dictionary<int, double> s_PendingClientLogs;

	private static Action<Dictionary<int, byte[]>> s_ClientLogsCallback;

	private static double s_ClientLogsHardDeadline;

	public const double PeerLogStartTimeout = 60.0;

	public const double PeerLogHardTimeout = 120.0;

	public static void Connected(NetworkBackendPlayer player)
	{
		if (player == FFSNetwork.CurrentPlayer)
		{
			return;
		}
		Log.NetworkInfo(player.Username + " connected.");
		ReproStepsLogger.LogEvent("PlayerConnected", $"Client {player}");
		if (!PlayerRegistry.JoiningPlayers.Contains(player))
		{
			if (PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w.IsClient).Count() > 0)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreJoining);
			}
			PlayerRegistry.JoiningPlayers.Add(player);
			PlayerRegistry.ConnectingUsers.RemoveAll((UserToken x) => player.SameUserToken(x));
		}
		PlayerRegistry.OnUserConnected?.Invoke(player);
		FFSNetwork.SendTokenToClient(GHNetworkCallbacks.GenerateGameToken(), player);
	}

	public static void Disconnected(NetworkBackendPlayer player, DisconnectionErrorToken disconnectToken)
	{
		Log.NetworkInfo("User disconnected.");
		if (!FFSNetwork.IsOnline || FFSNetwork.IsShuttingDown)
		{
			Log.NetworkInfo("Network is offline - skipping Disconnect cleanup.");
			return;
		}
		PlayerRegistry.ConnectingUsers.RemoveAll((UserToken x) => player.SameUserToken(x));
		if (PlayerRegistry.JoiningPlayers.Contains(player))
		{
			PlayerRegistry.JoiningPlayers.Remove(player);
			PlayerRegistry.OnJoiningUserDisconnected?.Invoke(player);
			if (!PlayerRegistry.OtherPlayersAreJoining())
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreFinishedJoining);
			}
		}
		PlayerRegistry.RemovePlayer(new PlayerID(player.PlayerID));
	}

	private static NetworkPlayer CreateJoiningPlayerAndSendPlayerTokens(NetworkBackendPlayer raisedBy)
	{
		NetworkPlayer networkPlayer = PlayerRegistry.CreateNetworkPlayer(raisedBy.GeneratePlayerToken(isPlayerHost: false, isLocalPlayer: false));
		foreach (NetworkPlayer player in PlayerRegistry.AllPlayers)
		{
			if (player != networkPlayer && player != PlayerRegistry.MyPlayer)
			{
				NetworkBackendPlayer client = FFSNetwork.Players.First((NetworkBackendPlayer p) => p.PlayerID == player.PlayerID.ID);
				FFSNetwork.SendTokenToClient(networkPlayer.PlayerToken, client);
			}
			bool isLocalPlayer = player.PlayerToken.IsLocalPlayer;
			player.PlayerToken.IsLocalPlayer = player == networkPlayer;
			FFSNetwork.SendTokenToClient(player.PlayerToken, raisedBy);
			player.PlayerToken.IsLocalPlayer = isLocalPlayer;
		}
		return networkPlayer;
	}

	private static void SendSlotAssignmentsTo(NetworkBackendPlayer raisedBy)
	{
		NetworkPlayer joinedPlayer = PlayerRegistry.GetPlayer(raisedBy.PlayerID);
		foreach (NetworkPlayer item in PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w != joinedPlayer))
		{
			foreach (PartySlotID assignedSlot in item.AssignedSlots)
			{
				Synchronizer.SendSideAction(GameActionType.AssignSlot, null, canBeUnreliable: false, sendToHostOnly: false, dataInt: item.PlayerID.ID, dataInt2: assignedSlot.Index, targetPlayerID: joinedPlayer.PlayerID.ID);
			}
		}
	}

	public static void OnEvent(PlayerEntityRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00023", "Received a PlayerEntityRequest but the event returns null.");
			return;
		}
		if (raisedBy == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00024", "Received a PlayerEntityRequest but the sending player does not exist anymore.");
			return;
		}
		NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID.ID == raisedBy.PlayerID);
		if (networkPlayer == null)
		{
			networkPlayer = CreateJoiningPlayerAndSendPlayerTokens(raisedBy);
		}
		FFSNetwork.SendTokenToClient(PlayerRegistry.GenerateSlotState(), raisedBy);
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer != networkPlayer)
			{
				allPlayer.SendAvatarDataToPlayer(networkPlayer);
			}
		}
	}

	public static void OnEvent(PlayerEntityInitializedEventToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00025", "Received a PlayerEntityInitializedEvent but the event returns null.");
		}
		else if (raisedBy == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00026", "Received a PlayerEntityInitializedEvent but the sending player does not exist anymore.");
		}
		else
		{
			SendSlotAssignmentsTo(raisedBy);
		}
	}

	public static void OnEvent(ControllableAssignmentRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00027", "Received a ControllableAssignmentRequest but the request returns null.");
			return;
		}
		if (raisedBy == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00028", "Received a ControllableAssignmentRequest but the requesting player does not exist anymore.");
			return;
		}
		if (ControllableRegistry.AllControllables.FirstOrDefault((NetworkControllable x) => x.Controller == null || x.Controller == PlayerRegistry.MyPlayer) == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00029", "Controllable was requested by the client but no controllables are left to be assigned.");
			return;
		}
		throw new InvalidOperationException("TODO");
	}

	public static void OnEvent(GameDataRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00010", "Received a GameDataRequest but the request returns null.");
			return;
		}
		if (raisedBy == null)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00011", "Received a GameDataRequest for " + ((DataActionType)request.DataActionID/*cast due to constrained. prefix*/).ToString() + " but the requesting player does not exist anymore.");
			return;
		}
		switch ((DataActionType)request.DataActionID)
		{
		case DataActionType.SendInitialGameState:
			PlatformLayer.Instance.StartCoroutine(SendInitialGameStateCoroutine(request, raisedBy));
			break;
		case DataActionType.SendCurrentGameState:
			PlatformLayer.Instance.StartCoroutine(SendCurrentGameStateCoroutine(request, raisedBy));
			break;
		case DataActionType.CompareScenarioStates:
			PlatformLayer.Instance.StartCoroutine(SendScenarioStateWhenReady(request, raisedBy));
			break;
		case DataActionType.SendPlayerLogFromHost:
			PlatformLayer.Instance.StartCoroutine(SendPlayerLogFromHostCoroutine(request, raisedBy));
			break;
		case DataActionType.SendStateDumpFromHost:
			FFSNetwork.StreamBytesToClient(raisedBy, Utility.CompressData(DesyncDetector.s_LastStateDump));
			break;
		case DataActionType.SendFHGameState:
		{
			byte[] data = FHGlue.BinarySerializeState(global::Singleton<FHMapChoreographer>.InstanceFast.GameServer.GameController.GameState, global::Singleton<FHMapChoreographer>.InstanceFast.DefRepo);
			data = Utility.CompressData(data);
			NetworkPlayer player = PlayerRegistry.GetPlayer(raisedBy.PlayerID);
			global::Singleton<FHMapChoreographer>.InstanceFast.OnHostSendingGameStateToClient(player);
			FFSNetwork.StreamBytesToClient(raisedBy, data);
			break;
		}
		default:
			Log.NetworkError("ERROR_MULTIPLAYER_00012", "Unprocessable DataActionType found: " + (DataActionType)request.DataActionID/*cast due to constrained. prefix*/);
			break;
		}
	}

	public static void OnEvent(GameDataEventToken evnt, NetworkBackendPlayer raisedBy)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a GameDataEvent but the event returns null."));
			return;
		}
		throw new InvalidOperationException("Not implemented in Fusion");
	}

	private static IEnumerator SendPlayerLogFromHostCoroutine(GameDataRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (s_FakePlayerLogSize > 0)
		{
			byte[] array = new byte[s_FakePlayerLogSize];
			System.Random random = new System.Random();
			for (int i = 0; i < s_FakePlayerLogSize; i++)
			{
				byte b = (byte)random.Next();
				array[i] = b;
			}
			byte[] array2 = Utility.CompressData(array);
			Log.Info($"Streaming fake {array2.Length} bytes");
			FFSNetwork.StreamBytesToClient(raisedBy, array2);
			yield break;
		}
		Task<string> playerLogPathTask = RootSaveData.PlayerLogPath;
		yield return new WaitForTask(playerLogPathTask);
		string playerLogPath = playerLogPathTask.Result;
		Task<string> tempPlayerLogPathTask = RootSaveData.TempPlayerLogPath;
		yield return new WaitForTask(tempPlayerLogPathTask);
		string tempPlayerLogPath = tempPlayerLogPathTask.Result;
		byte[] data = null;
		string error = null;
		int attempts = 1000;
		while (attempts > 0)
		{
			try
			{
				if (!File.Exists(playerLogPath))
				{
					error = "Unable to find Player log file at: " + playerLogPath;
					break;
				}
				File.Copy(playerLogPath, tempPlayerLogPath, overwrite: true);
				data = File.ReadAllBytes(tempPlayerLogPath);
				File.Delete(tempPlayerLogPath);
			}
			catch (IOException ex)
			{
				error = ex.Message;
				goto IL_01bf;
			}
			catch (Exception ex2)
			{
				error = ex2.Message;
			}
			break;
			IL_01bf:
			yield return null;
			int num = attempts - 1;
			attempts = num;
		}
		if (error != null)
		{
			Log.Error("[SEND HOST PLAYER LOG] " + error);
		}
		if (data == null)
		{
			if (error == null)
			{
				error = "Unable to send player log from Host";
			}
			data = Encoding.UTF8.GetBytes(error);
		}
		Task<byte[]> compressTask = Task.Run(() => Utility.CompressData(data));
		yield return new WaitForTask(compressTask);
		byte[] data2;
		if (compressTask.IsFaulted)
		{
			Log.Error("[SEND HOST PLAYER LOG] Compression failed: " + compressTask.Exception?.GetBaseException().Message);
			data2 = Utility.CompressData(Encoding.UTF8.GetBytes("Unable to compress player log on Host"));
		}
		else
		{
			data2 = compressTask.Result;
		}
		FFSNetwork.StreamBytesToClient(raisedBy, data2);
	}

	private static IEnumerator SendInitialGameStateCoroutine(GameDataRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null || raisedBy == null)
		{
			yield break;
		}
		while ((global::Singleton<AutoSaveProgress>.IsInitialized && global::Singleton<AutoSaveProgress>.Instance.IsShowing) || SaveData.Instance.Global.CurrentAdventureData.IsSaving)
		{
			yield return null;
		}
		FHSaveData fHSaveData = SaveData.Instance?.Global?.CurrentAdventureData?.FrosthavenSaveData;
		byte[] data = Array.Empty<byte>();
		if (fHSaveData != null)
		{
			using MemoryStream stream = new MemoryStream();
			Task task = fHSaveData.BinarySerialize(stream);
			yield return new WaitForTask(task);
			data = stream.ToArray();
		}
		Log.Info($"[SAVE] Sending initial game state to player {raisedBy}");
		FFSNetwork.StreamBytesToClient(raisedBy, Utility.CompressData(data));
	}

	private static IEnumerator SendCurrentGameStateCoroutine(GameDataRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request == null || raisedBy == null)
		{
			yield break;
		}
		byte[] data = Array.Empty<byte>();
		if (ScenarioManager.CurrentScenarioState != null)
		{
			FastReload.PrepareForSave();
			using MemoryStream stream = new MemoryStream();
			Task task = ScenarioManager.CurrentScenarioState.BinarySerialize(stream);
			yield return new WaitForTask(task);
			data = stream.ToArray();
		}
		Log.Info($"[SAVE] Sending current game state to player {raisedBy}");
		FFSNetwork.StreamBytesToClient(raisedBy, Utility.CompressData(data));
		if (PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID.ID == raisedBy.PlayerID) == null)
		{
			CreateJoiningPlayerAndSendPlayerTokens(raisedBy);
		}
		if (PlayerRegistry.JoiningPlayers.Contains(raisedBy))
		{
			PlayerRegistry.JoiningPlayers.Remove(raisedBy);
			if (!PlayerRegistry.OtherPlayersAreJoining())
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreFinishedJoining);
			}
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			FastReload.ResetOnClientJoined(raisedBy.PlayerID);
		}
		Synchronizer.SendSideAction(GameActionType.SetRegularSpeedMultiplier, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)(SaveData.Instance.Global.RegularSpeedMultiplier * 100f));
		DesyncDetector.RequestDelayedCheckOnHost();
	}

	public static IEnumerator SendScenarioStateWhenReady(GameDataRequestToken request, NetworkBackendPlayer raisedBy)
	{
		if (request != null && raisedBy != null)
		{
			while (!Choreographer.s_Choreographer.HostReadyToSendCompareState || Choreographer.s_Choreographer.HasPendingAnimations)
			{
				yield return null;
			}
			FFSNetwork.StreamBytesToClient(raisedBy, Utility.CompressData(Utility.ObjectToByteArray(ScenarioManager.CurrentScenarioState)));
		}
	}

	public static void RequestClientLogs(Action<Dictionary<int, byte[]>> onComplete)
	{
		s_PendingClientLogs = new Dictionary<int, double>();
		s_ClientLogBuffers.Clear();
		s_ClientLogsCallback = onComplete;
		double value = Time.unscaledTimeAsDouble + 60.0;
		s_ClientLogsHardDeadline = Time.unscaledTimeAsDouble + 120.0;
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer != PlayerRegistry.MyPlayer && allPlayer.IsClient)
			{
				s_PendingClientLogs[allPlayer.PlayerID.ID] = value;
			}
		}
		if (s_PendingClientLogs.Count == 0)
		{
			FireClientLogsCallback();
			return;
		}
		Synchronizer.RequestCustomDataFromClients(DataActionType.SendPlayerLogFromClient);
		PlatformLayer.Instance.StartCoroutine(ClientLogTimeoutCrt());
	}

	private static IEnumerator ClientLogTimeoutCrt()
	{
		while (s_PendingClientLogs != null && s_PendingClientLogs.Count > 0)
		{
			yield return null;
			if (s_PendingClientLogs == null)
			{
				break;
			}
			double unscaledTimeAsDouble = Time.unscaledTimeAsDouble;
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, double> s_PendingClientLog in s_PendingClientLogs)
			{
				if (unscaledTimeAsDouble >= s_PendingClientLog.Value)
				{
					list.Add(s_PendingClientLog.Key);
				}
			}
			foreach (int item in list)
			{
				s_ClientLogBuffers[item] = Encoding.UTF8.GetBytes($"<<< client {item} unresponsive — log collection timed out >>>");
				s_PendingClientLogs.Remove(item);
			}
			if (s_PendingClientLogs.Count == 0)
			{
				FireClientLogsCallback();
			}
		}
	}

	public static void OnStreamingDataFromClient(StreamingDataToken token, NetworkBackendPlayer sender)
	{
		if (s_PendingClientLogs == null)
		{
			return;
		}
		int playerID = sender.PlayerID;
		if (!s_PendingClientLogs.ContainsKey(playerID))
		{
			return;
		}
		if (token.IsStart)
		{
			s_ClientLogBuffers[playerID] = new byte[token.Total];
			s_PendingClientLogs[playerID] = s_ClientLogsHardDeadline;
		}
		if (!s_ClientLogBuffers.TryGetValue(playerID, out var value))
		{
			return;
		}
		Buffer.BlockCopy(token.Array, 0, value, token.Offset, token.Array.Length);
		if (token.IsEnd)
		{
			s_ClientLogBuffers[playerID] = Utility.DecompressData(value);
			s_PendingClientLogs.Remove(playerID);
			if (s_PendingClientLogs.Count == 0)
			{
				FireClientLogsCallback();
			}
		}
	}

	private static void FireClientLogsCallback()
	{
		Action<Dictionary<int, byte[]>> action = s_ClientLogsCallback;
		s_PendingClientLogs = null;
		s_ClientLogsCallback = null;
		action?.Invoke(new Dictionary<int, byte[]>(s_ClientLogBuffers));
		s_ClientLogBuffers.Clear();
	}
}
