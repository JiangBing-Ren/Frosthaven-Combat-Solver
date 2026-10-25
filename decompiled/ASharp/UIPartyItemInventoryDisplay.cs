using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPartyItemInventoryDisplay : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private Image iconTypeInventory;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private ScrollRect itemsPanel;

	[SerializeField]
	private UIPartyItemSlot slotPrefab;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private UIPartyItemInventoryTooltip itemTooltip;

	[SerializeField]
	private string audioItemDisplay = "PlaySound_UISubTabOpen";

	[SerializeField]
	private string audioItemUnbind = "PlaySound_UIGenericBuy";

	private IPartyItemEquipmentService service;

	private List<UIPartyItemSlot> itemSlotPool = new List<UIPartyItemSlot>();

	private Dictionary<CItem, UIPartyItemSlot> assignedSlots = new Dictionary<CItem, UIPartyItemSlot>();

	private CMapCharacter character;

	private Action<CItem> onItemEquipped;

	private Action<CItem, int> onItemRemoved;

	private Action<CItem, bool> onItemHovered;

	private int slotNum;

	private UIPartyItemSlot selectedSlot;

	private CItem.EItemSlot slotType;

	private UIPartyItemSlot selectedItemToCofirmSwitch;

	public bool IsVisible => window.IsVisible;

	public UnityEvent OnHidden => window.onHidden;

	private void Awake()
	{
		window.onHidden.AddListener(OnHide);
	}

	public void Display(CMapCharacter character, IPartyItemEquipmentService service, CItem.EItemSlot slotType, int slotNum, RectTransform pointTo, CItem selectedItem = null, Action<CItem> onItemEquipped = null, Action<CItem, int> onItemRemoved = null, Action<CItem, bool> onItemHovered = null, bool resetOpen = true, bool editAllowed = true)
	{
		verticalPointer.PointAt(pointTo);
		this.onItemEquipped = onItemEquipped;
		this.character = character;
		this.service = service;
		this.slotNum = slotNum;
		this.slotType = slotType;
		this.onItemRemoved = onItemRemoved;
		this.onItemHovered = onItemHovered;
		selectedItemToCofirmSwitch = null;
		infoText.text = LocalizationManager.GetTranslation("GUI_ITEM_SLOT_" + slotType.ToString().ToUpper());
		iconTypeInventory.sprite = UIInfoTools.Instance.GetItemSlotIcon(slotType.ToString());
		CreateItems(slotType, slotNum, selectedItem, editAllowed);
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		if (resetOpen || !IsVisible)
		{
			AudioControllerUtils.PlaySound(audioItemDisplay);
			window.Show();
			showAnimation.Play();
		}
	}

	private void OnControllableOwnershipChanged(NetworkControllable controllable, NetworkPlayer oldcontroller, NetworkPlayer newcontroller)
	{
		if (selectedItemToCofirmSwitch != null)
		{
			Singleton<UIItemConfirmationBox>.Instance.Hide();
		}
		foreach (UIPartyItemSlot value in assignedSlots.Values)
		{
			value.RefreshMultiplayerInteraction();
		}
	}

	private void OnItemBinded(CItem item, CMapCharacter newOwner)
	{
		if (assignedSlots.ContainsKey(item))
		{
			assignedSlots[item].SetOwner(newOwner);
		}
	}

	private void CreateItems(CItem.EItemSlot slotType, int slotNum, CItem selectedItem = null, bool interactable = true)
	{
		assignedSlots.Clear();
		List<CItem> unboundItemsForSlot = service.GetUnboundItemsForSlot(slotType, character);
		Dictionary<CItem, Tuple<CMapCharacter, bool>> boundItems = service.GetBoundAndEquippedItemsForSlot(slotType, (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? character : null);
		foreach (KeyValuePair<CItem, Tuple<CMapCharacter, bool>> item in boundItems)
		{
			unboundItemsForSlot.Add(item.Key);
		}
		unboundItemsForSlot = (from it in unboundItemsForSlot
			orderby LocalizationManager.GetTranslation(it.YMLData.Name), boundItems.ContainsKey(it) ? ((!boundItems[it].Item2) ? 1 : 2) : 0
			select it).ToList();
		List<CItem> list = character.CheckEquippedItems.FindAll((CItem it) => it.CanBeAssignedToSlot(slotType));
		selectedSlot = null;
		HelperTools.NormalizePool(ref itemSlotPool, slotPrefab.gameObject, itemsPanel.content, unboundItemsForSlot.Count);
		for (int num = 0; num < unboundItemsForSlot.Count; num++)
		{
			CItem cItem = unboundItemsForSlot[num];
			CMapCharacter cMapCharacter = null;
			bool isInOtherSlot = false;
			if (boundItems.ContainsKey(cItem))
			{
				cMapCharacter = boundItems[cItem].Item1;
				if (cMapCharacter.CharacterID == character.CharacterID && list.Contains(cItem))
				{
					isInOtherSlot = cItem.SlotIndex != slotNum;
				}
			}
			itemSlotPool[num].SetItem(character, cItem, cMapCharacter, OnItemSelected, OnItemDeselected, OnItemHovered, isInOtherSlot, interactable);
			assignedSlots[cItem] = itemSlotPool[num];
			if (cItem == selectedItem)
			{
				selectedSlot = itemSlotPool[num];
			}
		}
		if (selectedSlot == null)
		{
			itemsPanel.verticalNormalizedPosition = 1f;
			return;
		}
		selectedSlot.SetSelected(selected: true);
		itemsPanel.ScrollVerticallyTo(selectedSlot.transform as RectTransform);
	}

	private void OnItemHovered(UIPartyItemSlot itemSlot, bool hovered)
	{
		if (hovered)
		{
			if (Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				itemsPanel.ScrollVerticallyTo(itemSlot.transform as RectTransform);
			}
			string information = null;
			if (character.CharacterID == itemSlot.Owner?.CharacterID && itemSlot.IsEquippedInOtherSlot)
			{
				information = string.Format(LocalizationManager.GetTranslation("GUI_ITEM_EQUIPPED_DIFFERENT_SLOT"), string.Format(LocalizationManager.GetTranslation("GUI_INVENTORY_SLOT_" + itemSlot.Item.YMLData.Slot), itemSlot.Item.SlotIndex + 1));
			}
			itemTooltip.Show(itemSlot.Item, itemSlot.transform as RectTransform, itemSlot.Owner, information);
		}
		else
		{
			itemTooltip.Hide();
		}
		onItemHovered?.Invoke(itemSlot.Item, hovered);
	}

	private void OnItemSelected(UIPartyItemSlot itemSlot)
	{
		selectedItemToCofirmSwitch = null;
		if (itemSlot.Owner == null)
		{
			if (character.CheckEquippedItems.Exists((CItem it) => itemSlot.Item.ID == it.ID))
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_REPEATED"), UIInfoTools.DefInstance.negativeTextColor);
				return;
			}
			selectedItemToCofirmSwitch = itemSlot;
			string information = (itemSlot.Item.Tradeable ? string.Format(LocalizationManager.GetTranslation("GUI_BIND_EQUIP_ITEM_CONFIRMATION"), itemSlot.Item.SellPrice) : null);
			Singleton<UIItemConfirmationBox>.Instance.ShowConfirmation(LocalizationManager.GetTranslation("GUI_BIND_EQUIP_ITEM_CONFIRMATION_TITLE"), information, itemSlot.Item, () =>
			{
				selectedItemToCofirmSwitch = null;
				BindAndEquipItem(itemSlot);
			}, "GUI_BIND_ITEM", "GUI_CANCEL", "", () =>
			{
				selectedItemToCofirmSwitch = null;
			});
			return;
		}
		if (itemSlot.Owner?.CharacterID == character.CharacterID)
		{
			if (!character.CheckEquippedItems.Contains(itemSlot.Item) && character.CheckEquippedItems.Exists((CItem it) => itemSlot.Item.ID == it.ID))
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_REPEATED"), UIInfoTools.DefInstance.negativeTextColor);
				return;
			}
			EquipItem(itemSlot);
			OnItemHovered(itemSlot, hovered: true);
			if (FFSNetwork.IsOnline && character.IsMapCharacterUnderMyControl)
			{
				int controllableID = ControllableRegistry.GetControllableID(character);
				if (FFSNetwork.IsClient)
				{
					Synchronizer.SendGameAction(GameActionType.EquipItem, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemToken(itemSlot.Item.NetworkID, -1, slotNum));
				}
				else
				{
					Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemInventoryToken(character, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode));
				}
			}
			return;
		}
		if (character.CheckEquippedItems.Exists((CItem it) => itemSlot.Item.ID == it.ID))
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_REPEATED"), UIInfoTools.DefInstance.negativeTextColor);
			return;
		}
		if (!service.CanUnbound(itemSlot.Owner, itemSlot.Item))
		{
			itemSlot.ShowWarningCost(show: true);
			NewPartyDisplayUI.PartyDisplay.ShowWarningGold(show: true, (AdventureState.MapState.GoldMode != EGoldMode.CharacterGold) ? null : itemSlot.Owner?.CharacterID);
			return;
		}
		selectedItemToCofirmSwitch = itemSlot;
		CharacterYMLData characterYMLData = itemSlot.Owner.CharacterYMLData;
		string translation = LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey);
		string title = string.Format(LocalizationManager.GetTranslation("GUI_BIND_MOVE_ITEM_CONFIRMATION_TITLE"), translation);
		string information2 = string.Format(LocalizationManager.GetTranslation("GUI_BIND_MOVE_ITEM_CONFIRMATION"), UIInfoTools.Instance.GetCharacterColor(characterYMLData.Model, characterYMLData.CustomCharacterConfig).ToHex(), LocalizationManager.GetTranslation(characterYMLData.LocKey), $"AA_{characterYMLData.Model}", translation, itemSlot.Item.SellPrice);
		Singleton<UIItemConfirmationBox>.Instance.ShowConfirmation(title, information2, itemSlot.Item, () =>
		{
			selectedItemToCofirmSwitch = null;
			BindAndEquipItem(itemSlot);
		}, "GUI_BIND_ITEM", "GUI_CANCEL", audioItemUnbind, () =>
		{
			selectedItemToCofirmSwitch = null;
		});
	}

	private void OnItemDeselected(UIPartyItemSlot itemSlot, bool networkActionIfOnline = true)
	{
		if (!service.CanUnequip(itemSlot.Item, itemSlot.Owner))
		{
			return;
		}
		if (itemSlot == selectedSlot)
		{
			selectedSlot = null;
		}
		int slotIndex = itemSlot.Item.SlotIndex;
		character.UnequipItems(new List<CItem> { itemSlot.Item });
		itemSlot.RemoveEquipped();
		onItemRemoved(itemSlot.Item, slotIndex);
		SaveData.Instance.ManualSaveCurrentAdventureData();
		if ((FFSNetwork.IsOnline && character.IsMapCharacterUnderMyControl) & networkActionIfOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(character);
			if (FFSNetwork.IsClient)
			{
				Synchronizer.SendGameAction(GameActionType.UnequipItem, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemToken(itemSlot.Item.NetworkID, -1, slotIndex));
			}
			else
			{
				Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemInventoryToken(character, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode));
			}
		}
		Log.NetworkInfo(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(PlayerRegistry.MyPlayer) + " unequipped " + itemSlot.Item.YMLData.StringID + " (NetworkID: " + itemSlot.Item.NetworkID + ").", true);
	}

	public void EquipItem(UIPartyItemSlot itemSlot)
	{
		if (itemSlot.Owner != null && itemSlot.Owner?.CharacterID == character.CharacterID)
		{
			CheckItemLimitSlot(itemSlot.Item);
		}
		if (selectedSlot != null)
		{
			OnItemDeselected(selectedSlot, networkActionIfOnline: false);
		}
		AudioControllerUtils.PlaySound(UIInfoTools.Instance.GetItemConfig(itemSlot.Item.YMLData.Art).toggleAudioItem);
		selectedSlot = itemSlot;
		character.EquipItem(itemSlot.Item, slotNum);
		itemSlot.AssignOwner(character);
		onItemEquipped?.Invoke(itemSlot.Item);
		SaveData.Instance.ManualSaveCurrentAdventureData();
		Log.NetworkInfo(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(PlayerRegistry.MyPlayer) + " equipped " + itemSlot.Item.YMLData.StringID + " (NetworkID: " + itemSlot.Item.NetworkID + ").", true);
	}

	private void BindAndEquipItem(UIPartyItemSlot itemSlot)
	{
		if (itemSlot.Owner != null && itemSlot.Owner?.CharacterID != character.CharacterID)
		{
			service.UnbindItem(itemSlot.Owner, itemSlot.Item);
		}
		if (FFSNetwork.IsOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(character);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.BindAndEquipItem, ActionProcessor.CurrentPhase, disableAutoReplication: true, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemToken(itemSlot.Item.NetworkID, -1, slotNum));
		}
		else
		{
			service.BindItem(character, itemSlot.Item);
			OnItemBinded(itemSlot.Item, character);
			EquipItem(itemSlot);
		}
	}

	private void CheckItemLimitSlot(CItem item)
	{
		List<CItem> checkEquippedItems = character.CheckEquippedItems;
		if (item.YMLData.Slot == CItem.EItemSlot.SmallItem)
		{
			if (checkEquippedItems.Contains(item))
			{
				UIPartyItemSlot itemSlot = itemSlotPool.First((UIPartyItemSlot it) => it.Item == item && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot, networkActionIfOnline: false);
			}
		}
		else if (item.YMLData.Slot == CItem.EItemSlot.OneHand)
		{
			if (checkEquippedItems.Contains(item))
			{
				UIPartyItemSlot itemSlot2 = itemSlotPool.First((UIPartyItemSlot it) => it.Item == item && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot2, networkActionIfOnline: false);
			}
			CItem twoHands = checkEquippedItems.FirstOrDefault((CItem it) => it.YMLData.Slot == CItem.EItemSlot.TwoHand);
			if (twoHands != null)
			{
				UIPartyItemSlot itemSlot3 = itemSlotPool.First((UIPartyItemSlot it) => it.Item == twoHands && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot3, networkActionIfOnline: false);
			}
		}
		else
		{
			if (item.YMLData.Slot != CItem.EItemSlot.TwoHand)
			{
				return;
			}
			foreach (CItem oneHand in checkEquippedItems.FindAll((CItem it) => it.YMLData.Slot == CItem.EItemSlot.OneHand))
			{
				UIPartyItemSlot itemSlot4 = itemSlotPool.First((UIPartyItemSlot it) => it.Item == oneHand && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot4, networkActionIfOnline: false);
			}
		}
	}

	public void Hide()
	{
		window.Hide();
	}

	private void OnHide()
	{
		if (selectedItemToCofirmSwitch != null)
		{
			Singleton<UIItemConfirmationBox>.Instance.Hide();
		}
		selectedItemToCofirmSwitch = null;
	}
}
