using System;
using FFSNet;
using FHRL;
using MapRuleLibrary.Party;

public class FHControllableCharacter : IControllable
{
	private CampaignState _campaingState;

	public int ID { get; set; }

	public CharacterState Character { get; private set; }

	public CMapCharacter GHCharacter { get; private set; }

	public bool IsParticipant => true;

	public bool IsAlive => true;

	public FHControllableCharacter(CharacterState character, CampaignState state, CampaignController controller)
	{
		ID = ControllableRegistry.GetControllableID(character.ClassID.ID);
		Character = character;
		RecreateGHCharacter(state, controller);
	}

	public void UpdateState(CharacterState characterState)
	{
		if (ID != ControllableRegistry.GetControllableID(characterState.ClassID.ID))
		{
			throw new ArgumentException("characterState");
		}
		Character = characterState;
	}

	public void RecreateGHCharacter(CampaignState state, CampaignController controller)
	{
		if (!(SceneController.Instance == null))
		{
			GHCharacter = FHGlue.CreateGHCharacter(Character, state, controller);
		}
	}

	public string GetName()
	{
		return Character.Name;
	}

	public string GetID()
	{
		return Character.ClassID.ID;
	}

	public void OnControlAssigned(NetworkPlayer controller)
	{
	}

	public void OnControlReleased()
	{
	}
}
