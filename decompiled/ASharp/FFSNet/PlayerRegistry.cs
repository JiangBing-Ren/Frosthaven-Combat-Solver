using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFSNet.Backend;
using FHRL;
using Libs.GameConsole;
using UnityEngine;
using UnityEngine.Events;

namespace FFSNet;

public static class PlayerRegistry
{
	public static List<NetworkPlayer> AllPlayers = new List<NetworkPlayer>();

	private static bool m_WaitForOtherPlayers;

	public static List<NetworkPlayer> PlayersFinishedLoading = new List<NetworkPlayer>();

	public static UnityEvent MyPlayerInitialized = new UnityEvent();

	public static Action MyControllablesChanged;

	private static NetworkPlayer myPlayer;

	private static NetworkPlayer hostPlayer;

	private static NetworkPlayer[] _slotAssignment = new NetworkPlayer[4];

	private static NetworkControllable[] _controllableAssigment = new NetworkControllable[4];

	public static List<NetworkPlayer> Participants => AllPlayers.FindAll((NetworkPlayer x) => x.IsParticipant);

	public static List<NetworkPlayer> PlayersWeShouldWaitFor => AllPlayers.FindAll((NetworkPlayer x) => !IsPlayerJoining(x));

	public static List<NetworkBackendPlayer> JoiningPlayers { get; private set; }

	public static List<UserToken> ConnectingUsers { get; private set; }

	public static bool OtherClientsAreJoining { get; private set; }

	public static bool WaitForOtherPlayers
	{
		get
		{
			return m_WaitForOtherPlayers;
		}
		set
		{
			m_WaitForOtherPlayers = value;
			Log.NetworkInfo("WaitForOtherPlayers set to: " + m_WaitForOtherPlayers);
		}
	}

	public static bool LoadingInFromJoiningClient { get; set; }

	public static NetworkPlayer MyPlayer
	{
		get
		{
			return myPlayer;
		}
		private set
		{
			myPlayer = value;
			if (value != null && MyPlayerInitialized != null)
			{
				Log.NetworkInfo("Invoking MyPlayerInitilized. Listeners: " + MyPlayerInitialized.GetPersistentEventCount());
				MyPlayerInitialized.Invoke();
				if (FFSNetwork.IsClient)
				{
					FFSNetwork.SendTokenToServer(new PlayerEntityInitializedEventToken());
				}
			}
		}
	}

	public static NetworkPlayer HostPlayer
	{
		get
		{
			if (hostPlayer == null)
			{
				hostPlayer = AllPlayers.FirstOrDefault((NetworkPlayer x) => !x.IsClient);
			}
			return hostPlayer;
		}
	}

	public static SlotOwnershipChangedEvent OnSlotOwnershipChanged { get; set; }

	public static ConnectionEstablishedEvent OnUserConnected { get; set; }

	public static ConnectionEstablishedEvent OnJoiningUserDisconnected { get; set; }

	public static Action OnUserReadyForAssignment { get; set; }

	public static PlayersChangedEvent OnPlayerJoined { get; set; }

	public static PlayersChangedEvent OnPlayerLeft { get; set; }

	public static UserEnterEvent OnUserEnterRoom { get; set; }

	public static bool Initialized { get; private set; }

	public static IEnumerator InitializeCrt()
	{
		if (Initialized)
		{
			throw new Exception("PlayerRegistry is already initialized");
		}
		Init();
		if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
		{
			FFSNetwork.SendTokenToServer(new PlayerEntityRequestToken());
			while (!Initialized)
			{
				yield return null;
			}
		}
		else
		{
			Initialized = true;
		}
	}

	public static async Task InitializeTask()
	{
		if (Initialized)
		{
			throw new Exception("PlayerRegistry is already initialized");
		}
		Init();
		if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
		{
			FFSNetwork.SendTokenToServer(new PlayerEntityRequestToken());
			while (!Initialized)
			{
				await Task.Yield();
			}
		}
		else
		{
			Initialized = true;
		}
	}

	private static void Init()
	{
		AllPlayers.Clear();
		if (FFSNetwork.IsHost)
		{
			JoiningPlayers = new List<NetworkBackendPlayer>();
			ConnectingUsers = new List<UserToken>();
		}
		_slotAssignment = new NetworkPlayer[4];
		_controllableAssigment = new NetworkControllable[4];
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsHost)
			{
				CreateNetworkPlayer(FFSNetwork.CurrentPlayer.GeneratePlayerToken(isPlayerHost: true, isLocalPlayer: true));
				MyPlayer.PlayerReadyForAssignment = true;
			}
		}
		else
		{
			CreateMyPlayer();
		}
		if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			for (int i = 0; i < _slotAssignment.Length; i++)
			{
				AssignPlayerToSlot(MyPlayer, PartySlotID.FromIndex(i));
			}
		}
		OtherClientsAreJoining = false;
		LoadingInFromJoiningClient = false;
		WaitForOtherPlayers = false;
		PlayersFinishedLoading.Clear();
	}

	public static void HostReinitialize()
	{
		hostPlayer = null;
		myPlayer = null;
		NetworkControllable[] controllableAssigment = _controllableAssigment;
		Init();
		_controllableAssigment = controllableAssigment;
	}

	public static void ApplySlotState(SlotStateToken token)
	{
		DesyncDetector.s_IgnoreNextHash = true;
		Dictionary<NetworkControllable, NetworkPlayer> dictionary = ControllableRegistry.AllControllables.ToDictionary((NetworkControllable c) => c, (NetworkControllable c) => c.Controller);
		for (int num = 0; num < _slotAssignment.Length; num++)
		{
			NetworkPlayer obj = _slotAssignment[num];
			_slotAssignment[num] = ((token.SlotOwnership[num] == -1) ? null : GetPlayer(token.SlotOwnership[num]));
			if (obj != _slotAssignment[num])
			{
				OnSlotOwnershipChanged?.Invoke(PartySlotID.FromIndex(num), _slotAssignment[num]);
			}
		}
		for (int num2 = 0; num2 < _controllableAssigment.Length; num2++)
		{
			_ = _controllableAssigment[num2];
			_controllableAssigment[num2] = ((token.SlotHeroAssignment[num2] == -1) ? null : ControllableRegistry.GetControllable(token.SlotHeroAssignment[num2]));
		}
		Initialized = true;
		foreach (KeyValuePair<NetworkControllable, NetworkPlayer> item in dictionary)
		{
			ControllableRegistry.ProcessControllerChanged(item.Key, item.Value);
		}
	}

	public static void Reset()
	{
		Initialized = false;
		OnUserConnected = null;
		OnJoiningUserDisconnected = null;
		OnPlayerJoined = null;
		OnPlayerLeft = null;
		OnUserEnterRoom = null;
		hostPlayer = null;
		myPlayer = null;
		JoiningPlayers = null;
		ConnectingUsers = null;
		OtherClientsAreJoining = false;
		LoadingInFromJoiningClient = false;
		foreach (NetworkPlayer allPlayer in AllPlayers)
		{
			allPlayer.Clear();
		}
		AllPlayers.Clear();
		_slotAssignment = new NetworkPlayer[4];
		_controllableAssigment = new NetworkControllable[4];
		Initialized = false;
		Log.NetworkInfo("Cleared Network Player Registry!");
	}

	public static SlotStateToken GenerateSlotState()
	{
		SlotStateToken slotStateToken = new SlotStateToken();
		for (int i = 0; i < _slotAssignment.Length; i++)
		{
			slotStateToken.SlotOwnership[i] = ((_slotAssignment[i] != null) ? _slotAssignment[i].PlayerID.ID : (-1));
		}
		for (int j = 0; j < _controllableAssigment.Length; j++)
		{
			slotStateToken.SlotHeroAssignment[j] = ((_controllableAssigment[j] != null) ? _controllableAssigment[j].ID : (-1));
		}
		return slotStateToken;
	}

	public static NetworkPlayer CreateNetworkPlayer(PlayerToken playerToken)
	{
		if (!FFSNetwork.IsOnline)
		{
			throw new Exception("CreateNetworkPlayer can be called only in online game");
		}
		NetworkPlayer networkPlayer = new NetworkPlayer(playerToken);
		Log.NetworkInfo("Created a NetworkPlayer with PlayerID: " + playerToken.PlayerID);
		AllPlayers.RemoveAll((NetworkPlayer x) => x.PlayerID.ID == playerToken.PlayerID);
		if (playerToken.IsLocalPlayer)
		{
			MyPlayer = networkPlayer;
			FFSNetwork.CurrentPlayer.PlayerID = networkPlayer.PlayerID.ID;
		}
		if (playerToken.IsHost)
		{
			networkPlayer.PlayerReadyForAssignment = true;
		}
		AllPlayers.Add(networkPlayer);
		OnPlayerJoined?.Invoke(networkPlayer);
		if (global::Singleton<UIMultiplayerEscSubmenu>.Instance != null)
		{
			global::Singleton<UIMultiplayerEscSubmenu>.Instance.Refresh();
		}
		Log.NetworkInfo("[Core] " + networkPlayer.Username + " (PlayerID: " + networkPlayer.PlayerID.ToString() + ") JOINED the session.");
		DesyncDetector.s_IgnoreNextHash = true;
		if (networkPlayer == MyPlayer)
		{
			PlatformLayer.Instance.StartCoroutine(networkPlayer.StartLoadingAvatarCrt());
		}
		return networkPlayer;
	}

	public static void CreateMyPlayer()
	{
		if (FFSNetwork.IsOnline)
		{
			throw new Exception("CreatePlayer can be called only in offline game");
		}
		string userName = FFSNetwork.CurrentUserToken?.Username ?? "Temp Player";
		NetworkPlayer networkPlayer = (MyPlayer = new NetworkPlayer(new PlayerToken(1, userName, isHost: true, isLocalPlayer: true)));
		AllPlayers.Add(networkPlayer);
		OnPlayerJoined?.Invoke(networkPlayer);
		PlatformLayer.Instance.StartCoroutine(networkPlayer.StartLoadingAvatarCrt());
		Log.NetworkInfo("[Core] " + networkPlayer.Username + " (PlayerID: " + networkPlayer.PlayerID.ToString() + ") JOINED the session.");
	}

	public static void RemovePlayer(PlayerID playerID)
	{
		try
		{
			NetworkPlayer player = AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID == playerID);
			if (player != null)
			{
				if (FFSNetwork.IsHost)
				{
					NetworkBackendPlayer networkBackendPlayer = FFSNetwork.Players.FirstOrDefault((NetworkBackendPlayer p) => p.PlayerID == player.PlayerID.ID);
					if (networkBackendPlayer != null)
					{
						FFSNetwork.Disconnect(networkBackendPlayer);
					}
				}
				Log.NetworkInfo("[Core] " + player.Username + " (PlayerID: " + player.PlayerID.ToString() + ") LEFT the session.");
				if (FFSNetwork.IsHost)
				{
					ReproStepsLogger.LogEvent("PlayerDisconnected", player.ToString());
				}
				DesyncDetector.s_IgnoreNextHash = true;
				DesyncDetector.OnPlayerDisconnected(player);
				if (MyPlayer == player && FFSNetwork.Manager.AutoShutdownUponMyPlayerRemoved)
				{
					FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
					if (global::Singleton<UIResultsManager>.Instance != null && global::Singleton<UIResultsManager>.Instance.IsShown)
					{
						global::Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
					}
				}
				if (FFSNetwork.IsOnline)
				{
					for (int num = 0; num < _slotAssignment.Length; num++)
					{
						if (_slotAssignment[num] == player)
						{
							if (FFSNetwork.Manager.AutoAssignControllablesToHost)
							{
								AssignPlayerToSlot(HostPlayer, PartySlotID.FromIndex(num));
							}
							else
							{
								AssignPlayerToSlot(null, PartySlotID.FromIndex(num));
							}
						}
					}
					AllPlayers.Remove(player);
					if (FFSNetwork.IsHost)
					{
						FFSNetwork.SendTokenToClients(new PlayerLeftToken
						{
							PlayerID = player.PlayerID.ID
						});
					}
					player.Clear();
				}
				OnPlayerLeft?.Invoke(player);
				if (global::Singleton<UIMultiplayerEscSubmenu>.Instance != null)
				{
					global::Singleton<UIMultiplayerEscSubmenu>.Instance.Refresh();
				}
			}
			else
			{
				Log.NetworkWarning("Tried removing a player but the NetworkPlayer returns null.");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during RemovePlayer.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static NetworkPlayer GetPlayer(int playerID)
	{
		return AllPlayers.FirstOrDefault((NetworkPlayer x) => x.PlayerID.ID == playerID);
	}

	public static NetworkPlayer GetPlayer(PlayerID playerID)
	{
		return AllPlayers.FirstOrDefault((NetworkPlayer x) => x.PlayerID == playerID);
	}

	public static void AssignPlayerToSlot(NetworkPlayer player, PartySlotID slot)
	{
		if (!IsValidSlot(slot))
		{
			Log.Error($"Tried to assign player '{player.Username}' to invalid slot {slot}");
			return;
		}
		DesyncDetector.s_IgnoreNextHash = true;
		NetworkControllable controllableAssignedToSlot = GetControllableAssignedToSlot(slot);
		NetworkPlayer oldPlayer = controllableAssignedToSlot?.Controller;
		NetworkPlayer obj = _slotAssignment[slot.Index];
		_slotAssignment[slot.Index] = player;
		if (obj != player)
		{
			OnSlotOwnershipChanged?.Invoke(slot, player);
		}
		if (controllableAssignedToSlot != null)
		{
			ControllableRegistry.ProcessControllerChanged(controllableAssignedToSlot, oldPlayer);
		}
	}

	public static bool IsPlayerAssignedToSlot(NetworkPlayer player, PartySlotID slot)
	{
		if (player == null)
		{
			return false;
		}
		return GetPlayerAssignedToSlot(slot) == player;
	}

	public static IEnumerable<PartySlotID> GetPlayerSlots(NetworkPlayer player)
	{
		int i = 0;
		while (i < _slotAssignment.Length)
		{
			if (_slotAssignment[i] == player)
			{
				yield return PartySlotID.FromIndex(i);
			}
			int num = i + 1;
			i = num;
		}
	}

	public static NetworkPlayer GetPlayerAssignedToSlot(PartySlotID slot)
	{
		if (!IsValidSlot(slot))
		{
			return null;
		}
		return _slotAssignment[slot.Index];
	}

	public static IEnumerable<NetworkControllable> GetPlayerControllables(NetworkPlayer player)
	{
		foreach (PartySlotID playerSlot in GetPlayerSlots(player))
		{
			if (_controllableAssigment[playerSlot.Index] != null)
			{
				yield return _controllableAssigment[playerSlot.Index];
			}
		}
	}

	public static void AssignControllableToSlot(NetworkControllable controllable, PartySlotID slot)
	{
		if (!IsValidSlot(slot))
		{
			Log.Error($"Tried to assign controllable '{controllable}' to invalid slot {slot}");
			return;
		}
		NetworkPlayer oldPlayer = controllable?.Controller;
		_controllableAssigment[slot.Index] = controllable;
		if (controllable != null)
		{
			ControllableRegistry.ProcessControllerChanged(controllable, oldPlayer);
		}
		else if (GetPlayerAssignedToSlot(slot) == MyPlayer)
		{
			MyControllablesChanged?.Invoke();
		}
		if (global::Singleton<UIMultiplayerEscSubmenu>.Instance != null)
		{
			global::Singleton<UIMultiplayerEscSubmenu>.Instance.Refresh();
		}
	}

	public static IEnumerable<PartySlotID> GetSlots()
	{
		int i = 0;
		while (i < _slotAssignment.Length)
		{
			yield return PartySlotID.FromIndex(i);
			int num = i + 1;
			i = num;
		}
	}

	public static PartySlotID GetFirstSlotWithoutControllable()
	{
		for (int i = 0; i < _controllableAssigment.Length; i++)
		{
			if (_controllableAssigment[i] == null)
			{
				return PartySlotID.FromIndex(i);
			}
		}
		return default;
	}

	public static bool IsValidSlot(PartySlotID slot)
	{
		if (slot.IsValid)
		{
			return slot.Index < _slotAssignment.Length;
		}
		return false;
	}

	public static bool SlotHasControllable(PartySlotID slot)
	{
		return GetControllableAssignedToSlot(slot) != null;
	}

	public static NetworkControllable GetControllableAssignedToSlot(PartySlotID slot)
	{
		if (slot.Index >= _slotAssignment.Length)
		{
			return null;
		}
		return _controllableAssigment[slot.Index];
	}

	public static PartySlotID GetSlotForControllable(NetworkControllable controllable)
	{
		for (int i = 0; i < _controllableAssigment.Length; i++)
		{
			if (_controllableAssigment[i] == controllable)
			{
				return PartySlotID.FromIndex(i);
			}
		}
		return default;
	}

	public static void ProxyClientDesync(GameAction action)
	{
		NetworkPlayer player = GetPlayer(action.DataInt);
		if (player != null)
		{
			player.HasDesynched = true;
		}
	}

	public static void PrintAllPlayerControllables()
	{
		Log.NetworkTrace("Printing all players' controllables:");
		foreach (NetworkPlayer allPlayer in AllPlayers)
		{
			if (allPlayer != null)
			{
				allPlayer.PrintControllables();
			}
			else
			{
				Log.NetworkWarning("Null player detected when printing all player controllables.");
			}
		}
	}

	public static bool IsJoining(NetworkPlayer controller)
	{
		if (JoiningPlayers != null)
		{
			return JoiningPlayers.Exists((NetworkBackendPlayer it) => it.PlayerID == controller.PlayerID.ID);
		}
		return false;
	}

	public static string GetPlayerIdentifierString(int playerID)
	{
		return GetPlayerIdentifierString(GetPlayer(playerID));
	}

	public static string GetPlayerIdentifierString(NetworkPlayer player)
	{
		if (player == null)
		{
			return " (NULL PLAYER)";
		}
		return "(Player: " + player.Username + ", PlayerID: " + player.PlayerID.ToString() + ")";
	}

	public static void ToggleOtherClientsAreJoining(bool value)
	{
		OtherClientsAreJoining = value;
		if (global::Singleton<UIScenarioMultiplayerController>.InstanceFast != null)
		{
			global::Singleton<UIScenarioMultiplayerController>.InstanceFast.RefreshWaitingNotifications();
		}
		global::Singleton<FHMapChoreographer>.InstanceFast?.View?.RefreshView();
		OnUserReadyForAssignment?.Invoke();
	}

	public static void StartWaitingForPlayers()
	{
		if (MyPlayer != null)
		{
			MyPlayer.SentPlayerReadyForAssignment = false;
		}
		if (FFSNetwork.IsOnline && MyPlayer.IsParticipant)
		{
			WaitForOtherPlayers = true;
		}
		else
		{
			WaitForOtherPlayers = false;
			PlayersFinishedLoading.Clear();
		}
		Log.Info($"StartWaitingForPlayers: {WaitForOtherPlayers}");
	}

	public static void ProxyNotifyLoadingFinished(GameAction action)
	{
		NotifyLoadingFinished(AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID.ID == action.PlayerID));
	}

	public static void NotifyLoadingFinished(NetworkPlayer player)
	{
		if (player != null && !PlayersFinishedLoading.Contains(player))
		{
			PlayersFinishedLoading.Add(player);
		}
		CheckLoadingFinished();
	}

	public static bool CheckLoadingFinished()
	{
		if (WaitForOtherPlayers && Participants.Count <= PlayersFinishedLoading.Count)
		{
			WaitForOtherPlayers = false;
			PlayersFinishedLoading.Clear();
		}
		return !WaitForOtherPlayers;
	}

	[ConsoleCommand(Command = "player_stat")]
	public static void PrintPlayerRegistry(IConsole console)
	{
		if (FFSNetwork.IsHost)
		{
			console.WriteLine($"{ConnectingUsers.Count} connecting players");
			console.WriteLine($"{JoiningPlayers.Count} joining players");
		}
		console.WriteLine($"{AllPlayers.Count} active players:");
		foreach (NetworkPlayer allPlayer in AllPlayers)
		{
			List<PartySlotID> list = GetPlayerSlots(allPlayer).ToList();
			if (list.Count == 0)
			{
				console.WriteLine($"- [{allPlayer.PlayerID}] {allPlayer.Username} (Ready:{allPlayer.PlayerReadyForAssignment}) has no slots assiged");
				continue;
			}
			console.WriteLine(string.Format("- [{0}] {1} (Ready:{2}) controls slots {3}", allPlayer.PlayerID, allPlayer.Username, allPlayer.PlayerReadyForAssignment, string.Join(", ", list.Select((PartySlotID s) => s.ID))));
		}
		console.WriteLine($"{ControllableRegistry.AllControllables.Count} controllables:");
		foreach (NetworkControllable allControllable in ControllableRegistry.AllControllables)
		{
			NetworkPlayer playerAssignedToSlot = GetPlayerAssignedToSlot(allControllable.Slot);
			string text = "NO PLAYER";
			if (playerAssignedToSlot != null)
			{
				text = $"[{playerAssignedToSlot.PlayerID}] {playerAssignedToSlot.Username}";
			}
			console.WriteLine($"- '{text}' controls [{allControllable.ID}] {allControllable.ControllableObject.GetName()} ({allControllable.ControllableObject.GetType()}) at slot {allControllable.Slot.ID}");
		}
		console.WriteLine("Party leader is " + (HostPlayer?.Username ?? null));
		console.WriteLine("Current player is " + (MyPlayer?.Username ?? null));
	}

	public static bool OtherPlayersAreJoining()
	{
		bool flag = (FFSNetwork.IsHost && (JoiningPlayers.Count > 0 || AllPlayers.Any((NetworkPlayer w) => w.IsClient && !w.PlayerReadyForAssignment))) || (FFSNetwork.IsClient && OtherClientsAreJoining);
		if (flag && GHNetworkCallbacks.ShouldWaitForSavePoint())
		{
			return false;
		}
		return flag;
	}

	public static bool IsPlayerJoining(NetworkPlayer player)
	{
		if (!JoiningPlayers.Any((NetworkBackendPlayer x) => x.PlayerID == player.PlayerID.ID))
		{
			if (player.IsClient)
			{
				return !player.PlayerReadyForAssignment;
			}
			return false;
		}
		return true;
	}
}
