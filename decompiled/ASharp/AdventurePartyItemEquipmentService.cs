using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

public class AdventurePartyItemEquipmentService : IPartyItemEquipmentService
{
	private CMapParty partyData;

	public AdventurePartyItemEquipmentService(CMapParty partyData)
	{
		this.partyData = partyData;
	}

	public List<CItem> GetUnboundItemsForSlot(CItem.EItemSlot slotType, CMapCharacter mapCharacter)
	{
		return partyData.CheckUnboundItems.FindAll((CItem it) => it.CanBeAssignedToSlot(slotType) && it.CanEquipItem(mapCharacter.CharacterID));
	}

	public bool CanUnbound(CMapCharacter character, CItem item)
	{
		if (!item.Tradeable)
		{
			return false;
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			return item.SellPrice <= partyData.PartyGold;
		}
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			return character.CharacterGold >= item.SellPrice;
		}
		return false;
	}

	public void UnbindItem(CMapCharacter character, CItem item)
	{
		if (FFSNetwork.IsOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(character);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.UnbindItem, ActionProcessor.CurrentPhase, disableAutoReplication: true, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemToken(item.NetworkID));
			return;
		}
		if (character.CheckEquippedItems.Contains(item))
		{
			character.UnequipItems(new List<CItem> { item });
		}
		character.UnbindItem(item);
		if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
		{
			partyData.ModifyPartyGold(-item.SellPrice, useGoldModifier: false);
		}
		else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			character.ModifyGold(-item.SellPrice, useGoldModifier: false);
		}
		Log.SimpleInfo("MapCharacter: " + character.CharacterID + " unbound item: " + item.Name);
	}

	public void BindItem(CMapCharacter character, CItem item)
	{
		partyData.BindItem(character.CharacterID, character.CharacterName, item);
		Log.SimpleInfo("MapCharacter: " + character.CharacterID + " " + character.CharacterName + " bound item: " + item.Name);
	}

	public Dictionary<CItem, Tuple<CMapCharacter, bool>> GetBoundAndEquippedItemsForSlot(CItem.EItemSlot slotType, CMapCharacter character = null)
	{
		Dictionary<CItem, Tuple<CMapCharacter, bool>> dictionary = new Dictionary<CItem, Tuple<CMapCharacter, bool>>();
		foreach (CMapCharacter checkCharacter in partyData.CheckCharacters)
		{
			if (character != null && checkCharacter != character)
			{
				continue;
			}
			foreach (CItem item in checkCharacter.CheckEquippedItems.Where((CItem it) => it.CanBeAssignedToSlot(slotType)))
			{
				dictionary[item] = new Tuple<CMapCharacter, bool>(checkCharacter, item2: true);
			}
			if (checkCharacter.CheckBoundItems == null)
			{
				continue;
			}
			foreach (CItem item2 in checkCharacter.CheckBoundItems.Where((CItem it) => it.CanBeAssignedToSlot(slotType)))
			{
				dictionary[item2] = new Tuple<CMapCharacter, bool>(checkCharacter, item2: false);
			}
		}
		return dictionary;
	}

	public int CountNewItems(CItem.EItemSlot itemSlot, CMapCharacter character)
	{
		int num = partyData.CheckUnboundItems.Count((CItem it) => it.IsNew && it.CanBeAssignedToSlot(itemSlot) && (character == null || it.CanEquipItem(character.CharacterID)));
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character != null)
		{
			num += character.CheckBoundItems.Count((CItem it) => it.IsNew && it.CanBeAssignedToSlot(itemSlot));
		}
		return num;
	}

	public int CountNewItems(CMapCharacter character)
	{
		int num = partyData.CheckUnboundItems.Count((CItem it) => it.IsNew && it.YMLData.Slot != CItem.EItemSlot.QuestItem && (character == null || it.CanEquipItem(character.CharacterID)));
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character != null)
		{
			num += character.CheckBoundItems.Count((CItem it) => it.IsNew && it.YMLData.Slot != CItem.EItemSlot.QuestItem);
		}
		return num;
	}

	public void UnmarkNewItem(CItem item)
	{
		item.IsNew = false;
	}

	public bool CanEquip(CItem item, CMapCharacter character)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character == null)
		{
			return false;
		}
		if (partyData.CheckUnboundItems.Contains(item))
		{
			return true;
		}
		return !partyData.SelectedCharacters.Any((CMapCharacter it) => it != character && it.GetBoundAndEquippedItems().Contains(item));
	}

	public List<CItem.EItemSlot> GetAvailableSlotItems(CMapCharacter character = null)
	{
		if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character != null)
		{
			return (from it in partyData.CheckUnboundItems.Where((CItem it) => it.CanEquipItem(character.CharacterID)).Concat(character.GetBoundAndEquippedItems())
				select it.YMLData.Slot).Distinct().ToList();
		}
		return (from it in partyData.CheckUnboundItems.Concat(partyData.CheckCharacters.SelectMany((CMapCharacter it) => it.GetBoundAndEquippedItems()))
			select it.YMLData.Slot).Distinct().ToList();
	}

	public bool CanUnequip(CItem item, CMapCharacter character)
	{
		return character.CheckEquippedItems.Contains(item);
	}
}
