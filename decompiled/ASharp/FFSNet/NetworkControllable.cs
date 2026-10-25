using FHRL;
using ScenarioRuleLibrary;

namespace FFSNet;

public class NetworkControllable : IDumpState
{
	public string GetControllableType
	{
		get
		{
			if (ControllableObject is CharacterManager)
			{
				return "CharacterManager";
			}
			if (ControllableObject is NewPartyCharacterUI)
			{
				return "NewPartyCharacterUI";
			}
			if (ControllableObject is FHControllableCharacter)
			{
				return "FHControllableCharacter";
			}
			return "None";
		}
	}

	public string GetCharacter
	{
		get
		{
			if (ControllableObject is CharacterManager characterManager && characterManager != null && characterManager.CharacterActor != null)
			{
				return characterManager.CharacterActor.Class.ID;
			}
			if (ControllableObject is NewPartyCharacterUI newPartyCharacterUI && newPartyCharacterUI != null && newPartyCharacterUI.Data != null)
			{
				return newPartyCharacterUI.Data.CharacterID;
			}
			if (ControllableObject is FHControllableCharacter fHControllableCharacter && fHControllableCharacter != null && fHControllableCharacter.Character != null)
			{
				return fHControllableCharacter.Character.ClassID.ID;
			}
			return "None";
		}
	}

	public int ID { get; private set; }

	public NetworkPlayer Controller => PlayerRegistry.GetPlayerAssignedToSlot(Slot);

	public PartySlotID Slot => PlayerRegistry.GetSlotForControllable(this);

	public IControllable ControllableObject { get; set; }

	public bool IsParticipant => ControllableObject != null;

	public bool IsAlive
	{
		get
		{
			if (ControllableObject != null)
			{
				return ControllableObject.IsAlive;
			}
			return false;
		}
	}

	public override string ToString()
	{
		return $"[{Slot}] ID: {ID} Type: {GetControllableType} Character: {GetCharacter} Controller: {Controller}";
	}

	public string ToStateDumpString(StateDump dump)
	{
		return $"[{ID}] {GetCharacter} -> {Controller?.PlayerToken?.ToStateDumpString(dump)}";
	}

	public void DumpMembers(StateDump dump)
	{
	}

	public NetworkControllable(int id, IControllable controllableObject)
	{
		Log.NetworkInfo("Creating new NetworkControllable. ID: " + id);
		ID = id;
		ControllableObject = controllableObject;
	}
}
