using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

public interface IPartyItemEquipmentService
{
	List<CItem> GetUnboundItemsForSlot(CItem.EItemSlot slotType, CMapCharacter character);

	bool CanUnbound(CMapCharacter characterID, CItem item);

	void UnbindItem(CMapCharacter characterID, CItem item);

	void BindItem(CMapCharacter characterID, CItem item);

	Dictionary<CItem, Tuple<CMapCharacter, bool>> GetBoundAndEquippedItemsForSlot(CItem.EItemSlot slotType, CMapCharacter character = null);

	int CountNewItems(CItem.EItemSlot itemSlot, CMapCharacter character = null);

	List<CItem.EItemSlot> GetAvailableSlotItems(CMapCharacter character = null);

	int CountNewItems(CMapCharacter character = null);

	void UnmarkNewItem(CItem item);

	bool CanEquip(CItem item, CMapCharacter character);

	bool CanUnequip(CItem item, CMapCharacter character);
}
