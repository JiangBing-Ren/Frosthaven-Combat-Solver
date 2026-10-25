using System;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIPartyCharacterEquippementExtraSlot : MonoBehaviour
{
	[SerializeField]
	private UIPartyCharacterEquippementSlot slot;

	[SerializeField]
	private UINewNotificationTip newNotification;

	public UIPartyCharacterEquippementSlot Slot => slot;

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void Show(string characterID, CItem.EItemSlot smallItem, CItem item, bool availableSmall, Action<UIPartyCharacterEquippementSlot> onItemSelect, Action<UIPartyCharacterEquippementSlot, bool> onHovered, Action<UIPartyCharacterEquippementSlot, bool> showItemTooltip, int slotNum, string emptySlotTitle, bool isNew)
	{
		slot.InitUnlocked(characterID, smallItem, item, availableSmall, (UIPartyCharacterEquippementSlot selectedItem) =>
		{
			newNotification.Hide();
			onItemSelect(selectedItem);
		}, onHovered, showItemTooltip, slotNum, emptySlotTitle);
		if (!gameObject.activeSelf & isNew)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
		gameObject.SetActive(value: true);
	}
}
