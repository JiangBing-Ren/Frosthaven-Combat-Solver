using System;
using FHRL;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO;

[Serializable]
public class PartySlotDTO
{
	public int SlotId;

	public ECharacter Character;

	public string ClassID;

	public string Name;

	public CharacterID CharacterID;

	public Sprite Portrait;

	public Sprite Illustration;

	public Sprite InfoIllustration;

	public Color Color;

	public bool ShowValues;

	public uint Gold;

	public uint Level;

	public RangedValueDTO Experience;

	public bool HasAvailableCardUnlocks;

	public bool HasAvailablePerkMarks;

	public bool ForceAlert;

	public bool IsAvailable;

	public bool IsSelected;

	public UIMultiplayerUser.Data? MultiplayerUserData;

	public bool? IsReady;

	public uint MaxLevelToBeGainedFromProsperity;

	public bool Owned;

	public bool CanRetire;

	public bool Retired;

	public bool InCreation;

	public bool WasImported;

	public bool HasNotificationBadge
	{
		get
		{
			if (!ForceAlert && !HasAvailablePerkMarks && !HasAvailableCardUnlocks)
			{
				return CanRetire;
			}
			return true;
		}
	}

	public bool IsEmpty => Character == ECharacter.None;

	public bool InParty => SlotId > 0;

	public bool HasLevelToBeGainedFromProsperity => MaxLevelToBeGainedFromProsperity > Level;

	public PartySlotDTO()
	{
	}

	public PartySlotDTO(CharacterDataDTO characterData)
	{
		string text = ((characterData.Class != null) ? characterData.Class.ID.ID : string.Empty);
		ECharacter eCharacter = ((characterData.Class != null) ? CrossReference.ClassIdToECharacterModel(text) : ECharacter.None);
		SlotId = characterData.SlotId;
		Character = eCharacter;
		ClassID = text;
		CharacterID = characterData.CharacterID;
		Name = characterData.Name;
		Portrait = null;
		Illustration = UIInfoTools.Instance.GetCharacterConfigUI(eCharacter).illustration;
		InfoIllustration = UIInfoTools.Instance.GetCharacterConfigUI(eCharacter).InfoIllustration;
		Color = UIInfoTools.Instance.GetCharacterColor(eCharacter);
		ShowValues = eCharacter != ECharacter.None;
		Gold = characterData.Gold;
		Level = characterData.Level;
		Experience = characterData.Experience;
		IsAvailable = eCharacter != ECharacter.None;
		InCreation = characterData.InCreation && !characterData.CreationFlowPassed;
		MaxLevelToBeGainedFromProsperity = characterData.LevelToBeGainedFromProsperity;
		WasImported = characterData.WasImported;
	}

	public static PartySlotDTO Empty()
	{
		return new PartySlotDTO
		{
			Character = ECharacter.None,
			SlotId = -1
		};
	}

	public override string ToString()
	{
		if (IsEmpty)
		{
			return $"PartySlotDTO[Empty Slot {SlotId}]";
		}
		return $"PartySlotDTO[Name: {Name}, Character: {Character}, Slot{SlotId}]";
	}
}
