using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHRL;
using Libs.Threading;

public static class FHMultiplayerUtils
{
	public static void SyncPlayers(MultiplayerGameController GameController)
	{
		if (!ThreadManagerComponent.Manager.IsMainThread)
		{
			throw new InvalidOperationException("MultiplayerClient SyncPlayers must be executed only on main thread!");
		}
		foreach (MultiplayerGameState.Player player in GameController.GameState.Players.ToList())
		{
			if (PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID == player.ID) == null)
			{
				GameController.RemovePlayer(player.ID);
			}
		}
		foreach (NetworkPlayer netPlayer in PlayerRegistry.AllPlayers)
		{
			MultiplayerGameState.Player player2 = GameController.GameState.Players.FirstOrDefault((MultiplayerGameState.Player p) => p.ID == netPlayer.PlayerID);
			if (player2 == null)
			{
				player2 = GameController.NewPlayer(netPlayer.PlayerID);
			}
			player2.IsParticipant = netPlayer.PlayerReadyForAssignment;
		}
		if (GameController.GameState.PartyLeaderID != PlayerRegistry.HostPlayer.PlayerID)
		{
			GameController.ChangePartyLeader(PlayerRegistry.HostPlayer.PlayerID);
		}
	}

	public static bool PlayerOwnsCharacter(this MultiplayerGameController gameController, PlayerID playerID, CharacterState character)
	{
		if (character == null)
		{
			return false;
		}
		NetworkControllable networkControllable = ControllableRegistry.AllControllables.FirstOrDefault((NetworkControllable c) => (c.ControllableObject as FHControllableCharacter)?.Character == character);
		if (networkControllable == null)
		{
			return false;
		}
		return networkControllable.Controller.PlayerID == playerID;
	}

	public static bool PlayerOwnsCharacter(this MultiplayerGameController gameController, PlayerID playerID, CharacterID characterID)
	{
		if (characterID == null || !characterID.IsValid)
		{
			return false;
		}
		NetworkControllable networkControllable = ControllableRegistry.AllControllables.FirstOrDefault((NetworkControllable c) => (c.ControllableObject as FHControllableCharacter)?.Character?.ID == characterID);
		if (networkControllable == null)
		{
			return false;
		}
		return networkControllable.Controller.PlayerID == playerID;
	}

	public static MultiplayerGameState.Player GetCharacterOwner(this MultiplayerGameController gameController, CharacterState character)
	{
		if (character == null)
		{
			return null;
		}
		if (ControllableRegistry.AllControllables.FirstOrDefault((NetworkControllable c) => (c.ControllableObject as FHControllableCharacter).Character == character)?.Controller == null)
		{
			return null;
		}
		return gameController.GameState.Players.FirstOrDefault((MultiplayerGameState.Player p) => p.ID == p.ID);
	}

	public static IEnumerable<MultiplayerGameController.PartySlotDesc> GetOwnedSlots(this MultiplayerGameController gameController, PlayerID playerID)
	{
		foreach (PartySlotID playerSlot in PlayerRegistry.GetPlayerSlots(PlayerRegistry.GetPlayer(playerID)))
		{
			yield return new MultiplayerGameController.PartySlotDesc
			{
				ID = playerSlot,
				Character = (PlayerRegistry.GetControllableAssignedToSlot(playerSlot)?.ControllableObject as FHControllableCharacter)?.Character
			};
		}
	}

	public static IEnumerable<CharacterState> GetOwnedCharacters(this MultiplayerGameController gameController, PlayerID playerID)
	{
		foreach (PartySlotID playerSlot in PlayerRegistry.GetPlayerSlots(PlayerRegistry.GetPlayer(playerID)))
		{
			yield return (PlayerRegistry.GetControllableAssignedToSlot(playerSlot)?.ControllableObject as FHControllableCharacter)?.Character;
		}
	}

	public static MultiplayerGameState.Player GetSlotOwner(this MultiplayerGameController gameController, PartySlotID slotId)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayerAssignedToSlot(slotId);
		if (player == null)
		{
			return null;
		}
		return gameController.GameState.Players.First((MultiplayerGameState.Player p) => p.ID == player.PlayerID);
	}
}
