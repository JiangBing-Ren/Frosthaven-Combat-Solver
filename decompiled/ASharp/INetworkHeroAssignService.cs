using System.Collections.Generic;
using FFSNet;

public interface INetworkHeroAssignService
{
	PlayersChangedEvent OnPlayerJoined { get; set; }

	PlayersChangedEvent OnPlayerLeft { get; set; }

	List<NetworkPlayer> GetAllPlayers();

	void AssignHeroToPlayer(NetworkPlayer playerActor, string characterID, string characterName, int slot);

	void AssignSlotToPlayer(NetworkPlayer player, int slot);

	void AssignSlotToPlayer(GameAction action);

	void RemoveClient(NetworkPlayer client);
}
