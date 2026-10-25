using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public static class CMapCharacterExtensions
{
	public static int TotalSlotCapacity(this CMapCharacter character, bool countTwoHand = false)
	{
		int num = 0;
		foreach (CItem.EItemSlot value in Enum.GetValues(typeof(CItem.EItemSlot)))
		{
			if ((value != CItem.EItemSlot.TwoHand || countTwoHand) && value != CItem.EItemSlot.None)
			{
				num += character.SlotCapacity(value);
			}
		}
		return num;
	}

	public static int TotalEquippedItemsNum(this CMapCharacter character)
	{
		return character.CheckEquippedItems.Sum((CItem it) => (it.YMLData.Slot != CItem.EItemSlot.TwoHand) ? 1 : 2);
	}

	public static List<CItem> GetBoundAndEquippedItems(this CMapCharacter character)
	{
		if (character.CheckBoundItems == null)
		{
			return character.CheckEquippedItems;
		}
		return character.CheckEquippedItems.Concat(character.CheckBoundItems).ToList();
	}

	public static List<CItem> GetAllCharacterItems(this CMapCharacter character, CMapParty mapParty)
	{
		return character.GetBoundAndEquippedItems().Concat(mapParty.CheckUnboundItems).ToList();
	}

	public static List<CAbilityCard> GetOwnedAbilityCards(this CMapCharacter character)
	{
		if (character == null)
		{
			return new List<CAbilityCard>();
		}
		return CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == character.CharacterID).AbilityCardsPool.Where((CAbilityCard w) => character.OwnedAbilityCardIDs.Contains(w.ID)).ToList();
	}
}
