using System.Collections.Generic;
using System.Linq;
using FHRL;

namespace FHGUI.DTO;

public class CharacterDataDTO
{
	public int SlotId = -1;

	public CharacterID CharacterID;

	public string Name;

	public bool IsMaxLevel;

	public uint Level;

	public RangedValueDTO Experience;

	public RangedValueDTO Health;

	public uint Gold;

	public OutpostResources Resources;

	public CharacterClassDef Class;

	private PersonalQuestDef _personalQuestDef;

	public uint LevelToBeGainedFromProsperity;

	public List<GoalDTO> Masteries;

	public List<CardDTO> Cards = new List<CardDTO>();

	public IEnumerable<ItemDataDTO> Items;

	public uint UnlockedSmallItemCount;

	public uint MaxSmallItems;

	public uint MaxOneHandSlots;

	public bool PersonalQuestVisible;

	public bool InCreation;

	public bool CreationFlowPassed;

	public bool WasImported;

	public bool IsRetired;

	public PersonalQuestDef PersonalQuestDef => _personalQuestDef;

	public CharacterPersonalQuestDTO PersonalQuest { get; private set; }

	public CharacterDataDTO()
	{
	}

	public CharacterDataDTO(CharacterState characterState, List<CardDTO> cards, IEnumerable<ItemDataDTO> items, List<GoalDTO> masteries, uint levelToBeGainedFromProsperity)
	{
		CrossReference.GetExperienceProgress(characterState, out var currentProgress, out var maxProgress);
		CharacterID = characterState.ID;
		Name = characterState.Name;
		Level = characterState.Level;
		IsMaxLevel = CrossReference.IsMaxLevel(characterState);
		Experience = new RangedValueDTO((int)currentProgress, (int)maxProgress);
		Gold = characterState.Gold;
		Resources = characterState.Resources;
		Class = characterState.ClassDef;
		Health = new RangedValueDTO((int)Class.HealthTable[1], (int)Class.HealthTable[^1]);
		Cards = cards;
		Items = items;
		Masteries = masteries;
		PersonalQuestVisible = false;
		InCreation = characterState.PartyState == CharacterPartyStateType.CreationProcess;
		CreationFlowPassed = characterState.CreationFlowPassed;
		WasImported = characterState.WasImported;
		LevelToBeGainedFromProsperity = levelToBeGainedFromProsperity;
	}

	public IEnumerable<ItemDataDTO> GetEquippedItems()
	{
		if (Items != null)
		{
			return Items.Where((ItemDataDTO dto) => dto.IsEquipped);
		}
		return Enumerable.Empty<ItemDataDTO>();
	}

	public static CharacterDataDTO GenerateDummyDto()
	{
		return new CharacterDataDTO
		{
			Class = new CharacterClassDef()
		};
	}

	public override string ToString()
	{
		return $"CharacterDataDTO[Name: {Name}, Class: {Class}, Slot{SlotId}]";
	}

	public void SetPersonalQuest(CharacterPersonalQuestDTO personalQuest)
	{
		PersonalQuest = personalQuest;
	}
}
