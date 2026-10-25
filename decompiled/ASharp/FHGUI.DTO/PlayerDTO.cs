using System;
using System.Collections.Generic;
using System.Linq;
using FHRL;

namespace FHGUI.DTO;

[Serializable]
public class PlayerDTO
{
	public List<CharacterState> OwnedCharacter = new List<CharacterState>();

	public bool IsPartyLeader;

	public bool IsReady;

	public List<UIMultiplayerUser.Data> ReadyUsers;

	public PlayerDTO()
	{
	}

	public PlayerDTO(FHMapViewState state)
	{
		OwnedCharacter = state.Choreographer.GameClient.GameController.GetOwnedCharacters(state.View.PlayerID).ToList();
		IsPartyLeader = state.View.IsPartyLeader;
		IsReady = state.View.Player.Ready;
		ReadyUsers = state.View.TryGetReadyUsersList();
	}

	public override string ToString()
	{
		return base.ToString();
	}
}
