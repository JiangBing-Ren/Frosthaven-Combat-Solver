using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHRL;
using UnityEngine;

public class NetworkHeroAssignService : MonoBehaviour, INetworkHeroAssignService, INetworkSessionService
{
	public PlayersChangedEvent OnPlayerJoined
	{
		get
		{
			return PlayerRegistry.OnPlayerJoined;
		}
		set
		{
			PlayerRegistry.OnPlayerJoined = value;
		}
	}

	public PlayersChangedEvent OnPlayerLeft
	{
		get
		{
			return PlayerRegistry.OnPlayerLeft;
		}
		set
		{
			PlayerRegistry.OnPlayerLeft = value;
		}
	}

	public List<NetworkPlayer> GetAllPlayers()
	{
		return PlayerRegistry.AllPlayers;
	}

	public void EndSession()
	{
		if (!PlayerRegistry.MyPlayer.IsClient)
		{
			foreach (PartySlotID slot in PlayerRegistry.GetSlots())
			{
				PlayerRegistry.AssignPlayerToSlot(PlayerRegistry.MyPlayer, slot);
			}
		}
		FFSNetwork.Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
		if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
		{
			Singleton<UIResultsManager>.Instance.MPSessionEndedOnResults();
		}
	}

	public void AssignHeroToPlayer(NetworkPlayer networkPlayer, string characterID, string characterName, int slot)
	{
		AssignSlotToPlayer(networkPlayer, slot);
	}

	public void AssignSlotToPlayer(GameAction action)
	{
		NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer s) => s.PlayerID.ID == action.DataInt);
		if (networkPlayer == null)
		{
			Log.NetworkError(null, $"Received AssignSlot for unknown player {action.DataInt}");
		}
		else
		{
			AssignSlotToPlayer(networkPlayer, action.DataInt2);
		}
	}

	public void AssignSlotToPlayer(NetworkPlayer player, int slot)
	{
		PlayerRegistry.AssignPlayerToSlot(player, PartySlotID.FromIndex(slot));
		Singleton<FHMapChoreographer>.InstanceFast?.View?.RefreshMultiplayerNotifications();
	}

	public bool IsOnline(NetworkPlayer player)
	{
		return !PlayerRegistry.IsJoining(player);
	}

	public void StartSession(Action onHostStarted, Action onHostEnded)
	{
		if (!FFSNetwork.IsOnline && !FFSNetwork.IsStartingUp)
		{
			FFSNetwork.ToggleServer(onHostStarted, onHostEnded);
		}
	}

	public string GetInviteCode()
	{
		return FFSNetwork.SessionID;
	}

	public void GenerateInviteCode()
	{
		throw new NotImplementedException("TODO?!");
	}

	public void RemoveClient(NetworkPlayer client)
	{
		PlayerRegistry.RemovePlayer(client.PlayerID);
	}
}
