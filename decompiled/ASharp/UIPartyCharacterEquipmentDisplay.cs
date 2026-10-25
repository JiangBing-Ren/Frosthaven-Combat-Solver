using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPartyCharacterEquipmentDisplay : Singleton<UIPartyCharacterEquipmentDisplay>
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private GUIAnimator errorAnimator;

	[SerializeField]
	private Color regularTitleColor;

	[SerializeField]
	private Color errorTitleColor;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private List<Image> focusMasks;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private ScrollRect itemsPanel;

	[SerializeField]
	private CanvasGroup lockedTooltip;

	[SerializeField]
	private TextMeshProUGUI lockedTooltipTitle;

	[SerializeField]
	private TextMeshProUGUI lockedTooltipDescription;

	[SerializeField]
	private UIPartyItemInventoryDisplay partyInventory;

	[SerializeField]
	private UIPartyItemInventoryTooltip itemTooptip;

	[Header("Head")]
	[SerializeField]
	private UIPartyCharacterEquippementSlot headSlot;

	[SerializeField]
	private UINewNotificationTip newHeadNotification;

	[Header("Body")]
	[SerializeField]
	private UIPartyCharacterEquippementSlot bodySlot;

	[SerializeField]
	private UINewNotificationTip newBodyNotification;

	[Header("Legs")]
	[SerializeField]
	private UIPartyCharacterEquippementSlot legsSlot;

	[SerializeField]
	private UINewNotificationTip newLegsNotification;

	[Header("Hands ")]
	[SerializeField]
	private UIPartyCharacterEquippementSlot hand1Slot;

	[SerializeField]
	private UIPartyCharacterEquippementSlot hand2Slot;

	[SerializeField]
	private UINewNotificationTip newHandNotification;

	[SerializeField]
	private UIPartyCharacter2HandSlot twoHandHandler;

	[Header("Small items")]
	[SerializeField]
	private List<UIPartyCharacterEquippementSlot> smallItemSlots;

	[SerializeField]
	private UINewNotificationTip newSmallItemsNotification;

	[SerializeField]
	private List<UIPartyCharacterEquippementExtraSlot> smallItemOverrideSlots;

	private Action<CItem> onNewItemsDisplayed;

	private Action<CItem> onItemRemoved;

	private Action<CItem> onItemEquipped;

	private Action onHidden;

	private CMapCharacter characterData;

	private IPartyItemEquipmentService service;

	private bool isFocused;

	private UIPartyCharacterEquippementSlot lastSlotSelected;

	private bool editAllowed;

	public bool IsVisible => window.IsVisible;

	public UIPartyItemInventoryDisplay Inventory => partyInventory;

	protected override void AwakeInit()
	{
		window.onHidden.AddListener(() =>
		{
			onHidden?.Invoke();
		});
	}

	private void Start()
	{
		partyInventory.OnHidden.AddListener(() =>
		{
			lastSlotSelected = null;
			Focus(focus: true);
		});
	}

	private UIPartyCharacterEquippementSlot GetSlot(CItem.EItemSlot type, int num)
	{
		switch (type)
		{
		case CItem.EItemSlot.Body:
			return bodySlot;
		case CItem.EItemSlot.Head:
			return headSlot;
		case CItem.EItemSlot.Legs:
			return legsSlot;
		case CItem.EItemSlot.OneHand:
			if (num != 0)
			{
				return hand2Slot;
			}
			return hand1Slot;
		case CItem.EItemSlot.SmallItem:
			if (num == int.MaxValue)
			{
				return null;
			}
			if (num >= characterData.BaseSmallItemMax)
			{
				return smallItemOverrideSlots[num - characterData.BaseSmallItemMax].Slot;
			}
			return smallItemSlots[num];
		default:
			return null;
		}
	}

	public void RefreshSlots(CMapCharacter character, bool isRefresh = true)
	{
		List<CItem.EItemSlot> availableSlotItems = service.GetAvailableSlotItems(character);
		List<CItem> checkEquippedItems = characterData.CheckEquippedItems;
		headSlot.InitUnlocked(character.CharacterID, CItem.EItemSlot.Head, checkEquippedItems.FirstOrDefault((CItem it) => it.CanBeAssignedToSlot(CItem.EItemSlot.Head)), availableSlotItems.Contains(CItem.EItemSlot.Head), OnItemSelect, OnHovered, ShowItemTooltip);
		bodySlot.InitUnlocked(character.CharacterID, CItem.EItemSlot.Body, checkEquippedItems.FirstOrDefault((CItem it) => it.CanBeAssignedToSlot(CItem.EItemSlot.Body)), availableSlotItems.Contains(CItem.EItemSlot.Body), OnItemSelect, OnHovered, ShowItemTooltip);
		legsSlot.InitUnlocked(character.CharacterID, CItem.EItemSlot.Legs, checkEquippedItems.FirstOrDefault((CItem it) => it.CanBeAssignedToSlot(CItem.EItemSlot.Legs)), availableSlotItems.Contains(CItem.EItemSlot.Legs), OnItemSelect, OnHovered, ShowItemTooltip);
		bool availableItems = availableSlotItems.Contains(CItem.EItemSlot.OneHand) || availableSlotItems.Contains(CItem.EItemSlot.TwoHand);
		List<CItem> list = checkEquippedItems.FindAll((CItem it) => it.CanBeAssignedToSlot(CItem.EItemSlot.OneHand));
		if (list.Count > 0 && list[0].YMLData.Slot == CItem.EItemSlot.TwoHand)
		{
			hand1Slot.InitUnlocked(character.CharacterID, CItem.EItemSlot.OneHand, list[0], availableItems, OnItemSelect, OnHovered, ShowItemTooltip);
			hand2Slot.InitUnlocked(character.CharacterID, CItem.EItemSlot.OneHand, list[0], availableItems, OnItemSelect, OnHovered, ShowItemTooltip, 1);
			twoHandHandler.Enable(enable: true);
		}
		else
		{
			twoHandHandler.Enable(enable: false);
			hand1Slot.InitUnlocked(character.CharacterID, CItem.EItemSlot.OneHand, list.FirstOrDefault((CItem it) => it.SlotIndex == 0), availableItems, OnItemSelect, OnHovered, ShowItemTooltip);
			hand2Slot.InitUnlocked(character.CharacterID, CItem.EItemSlot.OneHand, list.FirstOrDefault((CItem it) => it.SlotIndex == 1), availableItems, OnItemSelect, OnHovered, ShowItemTooltip, 1);
		}
		List<CItem> list2 = (from it in character.CheckEquippedItems
			where it.CanBeAssignedToSlot(CItem.EItemSlot.SmallItem)
			orderby it.SlotIndex
			select it).ToList();
		bool flag = availableSlotItems.Contains(CItem.EItemSlot.SmallItem);
		int num = 0;
		int num2 = characterData.SmallItemMax - characterData.CurrentSmallItemOverride;
		for (int num3 = 0; num3 < num2; num3++)
		{
			if (num < list2.Count && num3 == list2[num].SlotIndex)
			{
				smallItemSlots[num3].InitUnlocked(character.CharacterID, CItem.EItemSlot.SmallItem, list2[num], flag, OnItemSelect, OnHovered, ShowItemTooltip, num3);
				num++;
			}
			else
			{
				smallItemSlots[num3].InitUnlocked(character.CharacterID, CItem.EItemSlot.SmallItem, null, flag, OnItemSelect, OnHovered, ShowItemTooltip, num3);
			}
		}
		for (int num4 = num2; num4 < smallItemSlots.Count; num4++)
		{
			smallItemSlots[num4].InitLocked(character.CharacterID, CItem.EItemSlot.SmallItem, num4, OnHovered);
		}
		for (int num5 = 0; num5 < characterData.CurrentSmallItemOverride; num5++)
		{
			if (num < list2.Count && num2 + num5 == list2[num].SlotIndex)
			{
				smallItemOverrideSlots[num5].Show(character.CharacterID, CItem.EItemSlot.SmallItem, list2[num], flag, OnItemSelect, OnHovered, ShowItemTooltip, num2 + num5, string.Format(LocalizationManager.GetTranslation("GUI_INVENTORY_SLOT_POCKET"), num5 + 1), isRefresh);
				num++;
			}
			else
			{
				smallItemOverrideSlots[num5].Show(character.CharacterID, CItem.EItemSlot.SmallItem, null, flag, OnItemSelect, OnHovered, ShowItemTooltip, num2 + num5, string.Format(LocalizationManager.GetTranslation("GUI_INVENTORY_SLOT_POCKET"), num5 + 1), isRefresh);
			}
		}
		for (int num6 = characterData.CurrentSmallItemOverride; num6 < smallItemOverrideSlots.Count; num6++)
		{
			smallItemOverrideSlots[num6].Hide();
			if (lastSlotSelected == smallItemOverrideSlots[num6].Slot)
			{
				lastSlotSelected = null;
				partyInventory.Hide();
			}
		}
	}

	public void Refresh(CMapCharacter character)
	{
		if (!IsVisible)
		{
			return;
		}
		RefreshSlots(character);
		UpdateDisplay();
		Focus(isFocused);
		if (lastSlotSelected != null && partyInventory.IsVisible)
		{
			partyInventory.Display(characterData, service, lastSlotSelected.Slot, lastSlotSelected.SlotNum, lastSlotSelected.transform as RectTransform, lastSlotSelected.Item, (CItem item) =>
			{
				OnItemEquipped(lastSlotSelected, item);
			}, OnItemRemoved, OnItemHovered, resetOpen: false, editAllowed);
		}
	}

	private void OnHovered(UIPartyCharacterEquippementSlot slot, bool hovered)
	{
		if (hovered && Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			itemsPanel.ScrollVerticallyTo(slot.transform as RectTransform);
		}
		if (!slot.IsLocked)
		{
			return;
		}
		if (hovered && isFocused)
		{
			string title = string.Format(LocalizationManager.GetTranslation("GUI_LOCKED_OBJECT"), LocalizationManager.GetTranslation("GUI_ITEM_SLOT_SMALLITEM"));
			string description = string.Format(LocalizationManager.GetTranslation("GUI_ITEM_GUI_ITEM_SLOT_SMALLITEM_LOCKED_DESCRIPTION"), (slot.SlotNum + 1) * 2 - 1);
			if (slot.SlotNum - characterData.CurrentSmallItemOverride > 4)
			{
				description = string.Format(LocalizationManager.GetTranslation("GUI_ITEM_GUI_ITEM_SLOT_SMALLITEM_LOCKED_MYSTERY"));
			}
			ShowLockedTooltip(slot.transform as RectTransform, title, description);
		}
		else
		{
			HideLockedTooltip();
		}
	}

	private void ShowItemTooltip(UIPartyCharacterEquippementSlot slot, bool hovered)
	{
		if (!slot.IsLocked)
		{
			if (hovered && slot.Item != null)
			{
				RectTransform rectTransform = ((slot.Item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? twoHandHandler.transform : slot.transform) as RectTransform;
				itemTooptip.Show(slot.Item, rectTransform, isFocused ? Vector2.zero : new Vector2((0f - rectTransform.rect.width) / 2f, 0f));
			}
			else
			{
				itemTooptip.Hide();
			}
		}
	}

	private void RefreshTooltip()
	{
		if (itemTooptip.IsShown)
		{
			itemTooptip.RefreshPosition(isFocused ? Vector2.zero : new Vector2((0f - ((RectTransform)itemTooptip.transform.parent).rect.width) / 2f, 0f));
		}
	}

	public void Display(IPartyItemEquipmentService service, CMapCharacter characterData, Action<CItem> onItemEquipped, Action<CItem> onItemRemoved, RectTransform sourceUI, Action<CItem> onNewItemsDisplayed, bool editAllowed = true, Action onHidden = null)
	{
		this.service = service;
		this.characterData = characterData;
		this.onNewItemsDisplayed = onNewItemsDisplayed;
		this.onItemEquipped = onItemEquipped;
		this.onItemRemoved = onItemRemoved;
		this.editAllowed = editAllowed;
		this.onHidden = onHidden;
		lastSlotSelected = null;
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.equipment_selection);
		verticalPointer.PointAt(sourceUI);
		HideLockedTooltip();
		RefreshSlots(characterData, isRefresh: false);
		for (int i = 1; i < Enum.GetValues(typeof(CItem.EItemSlot)).Length; i++)
		{
			if (i != 5 && i != 7)
			{
				RefreshNotification((CItem.EItemSlot)i);
			}
		}
		itemsPanel.verticalNormalizedPosition = 1f;
		UpdateDisplay();
		Show();
	}

	private void RefreshNotification(CItem.EItemSlot itemSlot)
	{
		if (service.CountNewItems(itemSlot, characterData) > 0)
		{
			GetNotification(itemSlot)?.Show();
		}
		else
		{
			GetNotification(itemSlot)?.Hide();
		}
	}

	private UINewNotificationTip GetNotification(CItem.EItemSlot slot)
	{
		UINewNotificationTip result = null;
		switch (slot)
		{
		case CItem.EItemSlot.Body:
			result = newBodyNotification;
			break;
		case CItem.EItemSlot.Head:
			result = newHeadNotification;
			break;
		case CItem.EItemSlot.Legs:
			result = newLegsNotification;
			break;
		case CItem.EItemSlot.OneHand:
		case CItem.EItemSlot.TwoHand:
			result = newHandNotification;
			break;
		case CItem.EItemSlot.SmallItem:
			result = newSmallItemsNotification;
			break;
		}
		return result;
	}

	private void ShowLockedTooltip(RectTransform point, string title, string description)
	{
		lockedTooltipDescription.text = description;
		lockedTooltipTitle.text = title;
		lockedTooltip.alpha = 1f;
		lockedTooltip.transform.position = new Vector3(lockedTooltip.transform.position.x, point.transform.position.y, 0f);
		lockedTooltip.transform.position += (lockedTooltip.transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, 20f);
	}

	private void HideLockedTooltip()
	{
		lockedTooltip.alpha = 0f;
	}

	private void CancelAnimations()
	{
		showAnimation.Stop();
		errorAnimator.Stop();
	}

	public void Show(bool instant = false)
	{
		CancelAnimations();
		Focus(focus: true);
		window.Show();
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
	}

	private void OnItemSelect(UIPartyCharacterEquippementSlot slot)
	{
		if (!isFocused)
		{
			if (lastSlotSelected == slot)
			{
				lastSlotSelected = null;
				partyInventory.Hide();
			}
			else
			{
				lastSlotSelected = slot;
				partyInventory.Display(characterData, service, slot.Slot, slot.SlotNum, slot.transform as RectTransform, slot.Item, (CItem item) =>
				{
					OnItemEquipped(slot, item);
				}, OnItemRemoved, OnItemHovered, resetOpen: true, editAllowed);
			}
		}
		else
		{
			lastSlotSelected = slot;
			Focus(focus: false);
			partyInventory.Display(characterData, service, slot.Slot, slot.SlotNum, slot.transform as RectTransform, slot.Item, (CItem item) =>
			{
				OnItemEquipped(slot, item);
			}, OnItemRemoved, OnItemHovered, resetOpen: true, editAllowed);
		}
		RefreshTooltip();
	}

	private void OnItemHovered(CItem item, bool hovered)
	{
		if (hovered && item.IsNew)
		{
			service.UnmarkNewItem(item);
			RefreshNotification(item.YMLData.Slot);
			onNewItemsDisplayed(item);
		}
	}

	private void OnItemRemoved(CItem item, int slotIndex)
	{
		if (characterData.CheckEquippedItems.Contains(item))
		{
			if (item.YMLData.Slot == CItem.EItemSlot.TwoHand)
			{
				hand1Slot.RemoveItem();
				hand2Slot.RemoveItem();
				twoHandHandler.Enable(enable: false);
			}
			else
			{
				GetSlot(item.YMLData.Slot, slotIndex)?.RemoveItem();
			}
			UpdateDisplay();
		}
		RefreshNotification(item.YMLData.Slot);
		onItemRemoved?.Invoke(item);
	}

	private void OnItemEquipped(UIPartyCharacterEquippementSlot slot, CItem item)
	{
		if (item.YMLData.Slot == CItem.EItemSlot.TwoHand)
		{
			hand1Slot.AddItem(item, animate: true);
			hand2Slot.AddItem(item, animate: true);
			twoHandHandler.Enable(enable: true);
		}
		else
		{
			slot.AddItem(item, animate: true);
		}
		onItemEquipped?.Invoke(item);
		RefreshSlots(characterData);
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		int num = characterData.TotalEquippedItemsNum();
		int num2 = characterData.TotalSlotCapacity();
		TextMeshProUGUI textMeshProUGUI = infoText;
		string text = (warningText.text = num + " / " + num2 + " " + LocalizationManager.GetTranslation("GUI_EQUIPPED"));
		textMeshProUGUI.text = text;
		infoText.color = ((num2 >= num) ? regularTitleColor : errorTitleColor);
	}

	private void Focus(bool focus)
	{
		isFocused = focus;
		headSlot.Focus(focus);
		bodySlot.Focus(focus);
		hand1Slot.Focus(focus);
		hand2Slot.Focus(focus);
		legsSlot.Focus(focus);
		for (int i = 0; i < smallItemSlots.Count; i++)
		{
			smallItemSlots[i].Focus(focus);
		}
		for (int j = 0; j < smallItemOverrideSlots.Count; j++)
		{
			smallItemOverrideSlots[j].Slot.Focus(focus);
		}
		for (int k = 0; k < focusMasks.Count; k++)
		{
			focusMasks[k].enabled = !focus;
		}
		itemsPanel.enabled = focus;
	}

	public void Hide(bool playHideAudio = true)
	{
		partyInventory.Hide();
		CancelAnimations();
		string audioItemHide = window.AudioItemHide;
		if (!playHideAudio)
		{
			window.AudioItemHide = null;
		}
		window.Hide();
		window.AudioItemHide = audioItemHide;
	}

	private void OnDisable()
	{
		CancelAnimations();
	}
}
