using ScenarioRuleLibrary;

public static class CItemExtensions
{
	public static bool CanBeAssignedToSlot(this CItem item, CItem.EItemSlot selectedItemSlot)
	{
		if (item.YMLData.Slot != selectedItemSlot)
		{
			if (selectedItemSlot == CItem.EItemSlot.OneHand)
			{
				return item.YMLData.Slot == CItem.EItemSlot.TwoHand;
			}
			return false;
		}
		return true;
	}

	public static int CalculateCostToEquip(this CItem item, CItem.EItemSlot slot)
	{
		if ((slot == CItem.EItemSlot.OneHand || slot == item.YMLData.Slot) && item.YMLData.Slot == CItem.EItemSlot.TwoHand)
		{
			return 2;
		}
		if (slot != item.YMLData.Slot)
		{
			return 0;
		}
		return 1;
	}
}
