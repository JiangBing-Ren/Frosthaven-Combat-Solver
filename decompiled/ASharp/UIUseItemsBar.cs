using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.Events;

public class UIUseItemsBar : Singleton<UIUseItemsBar>
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseItemScenario itemSlotPrefab;

	private Dictionary<CItem, UIUseItemScenario> itemSlots = new Dictionary<CItem, UIUseItemScenario>();

	private Stack<UIUseItemScenario> pool = new Stack<UIUseItemScenario>();

	private Func<CItem, bool> condition;

	private Action<CItem, bool> onClickItem;

	private bool isShown;

	private string tooltip;

	private string tooltipTitle;

	private Transform lastCreatedSlot;

	private CActor actor;

	[SerializeField]
	private UnityEvent OnChangedItemsAvailable;

	private List<CItem> unavailableItems = new List<CItem>();

	public bool IsShown => isShown;

	public CActor Actor => actor;

	public IEnumerable<ElementInfusionBoardManager.EElement> GetAllConsumes()
	{
		if (!isShown)
		{
			yield break;
		}
		foreach (KeyValuePair<CItem, UIUseItemScenario> item in itemSlots.Where((KeyValuePair<CItem, UIUseItemScenario> x) => x.Value.IsSelected()))
		{
			foreach (ElementInfusionBoardManager.EElement selectedConsume in item.Value.GetSelectedConsumes())
			{
				yield return selectedConsume;
			}
		}
	}

	public bool HasShownUsableSlots()
	{
		if (isShown)
		{
			return itemSlots.Any((KeyValuePair<CItem, UIUseItemScenario> it) => it.Value.gameObject.activeSelf);
		}
		return false;
	}

	public void AddItem(CItem item)
	{
		if (!isShown || !actor.Inventory.AllItemsAndPets.Contains(item) || (condition != null && !condition(item)))
		{
			return;
		}
		if (!CanConsume(item))
		{
			if (!unavailableItems.Contains(item))
			{
				unavailableItems.Add(item);
			}
			return;
		}
		if (!itemSlots.ContainsKey(item))
		{
			UIUseItemScenario slotFromPool = GetSlotFromPool();
			itemSlots[item] = slotFromPool;
			slotFromPool.Show();
			unavailableItems.Remove(item);
			CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(actor as CPlayerActor, throwError: false);
			itemSlots[item].SetItem(item, (CItem it) =>
			{
				onClickItem(it, arg2: true);
			}, (CItem it) =>
			{
				onClickItem(it, arg2: true);
			}, actor, null, hand?.PickUnselectedInfusionsForItem(item));
			itemSlots[item].SetInteractable(interactable: true, updateLook: true);
		}
		else
		{
			itemSlots[item].gameObject.SetActive(value: true);
			CardsHandUI hand2 = Singleton<CardsHandManager>.Instance.GetHand(actor as CPlayerActor, throwError: false);
			itemSlots[item].SetItem(item, (CItem it) =>
			{
				onClickItem(it, arg2: true);
			}, (CItem it) =>
			{
				onClickItem(it, arg2: true);
			}, actor, null, hand2?.PickUnselectedInfusionsForItem(item));
		}
		RefreshTooltip();
		OnChangedItemsAvailable.Invoke();
	}

	private void OnUnreservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (CItem item in unavailableItems.ToList())
		{
			if (CanConsume(item))
			{
				unavailableItems.Remove(item);
				AddItem(item);
			}
		}
	}

	private void OnReservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> item in itemSlots.ToList())
		{
			if (!item.Value.IsSelected() && !CanConsume(item.Key))
			{
				RemoveItem(item.Key);
				if (unavailableItems.Contains(item.Key))
				{
					unavailableItems.Add(item.Key);
				}
			}
		}
	}

	private bool CanConsume(CItem item)
	{
		List<ElementInfusionBoardManager.EElement> list = item.YMLData.Consumes.ToList();
		if (item.YMLData.Data.Abilities != null)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in item.YMLData.Data.Abilities.Where((CAbility it) => it is CAbilityConsumeElement).SelectMany((CAbility it) => (it as CAbilityConsumeElement).ElementsToConsume))
			{
				int num = list.LastIndexOf(item2);
				if (num != -1)
				{
					list.RemoveAt(num);
				}
			}
		}
		List<ElementInfusionBoardManager.EElement> list2 = InfusionBoardUI.Instance?.GetAvailableElements();
		if (list2.Count < list.Count)
		{
			return false;
		}
		foreach (ElementInfusionBoardManager.EElement item3 in list)
		{
			if (item3 != ElementInfusionBoardManager.EElement.Any && !list2.Contains(item3))
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveItem(CItem item, bool clear = false)
	{
		if (itemSlots.ContainsKey(item))
		{
			itemSlots[item].gameObject.SetActive(value: false);
			unavailableItems.Remove(item);
			if (clear)
			{
				RemoveItemSlot(item, clearSelection: true);
			}
			RefreshTooltip();
			OnChangedItemsAvailable.Invoke();
		}
	}

	private void RemoveItemSlot(CItem item, bool clearSelection)
	{
		if (itemSlots.ContainsKey(item))
		{
			itemSlots[item].Hide();
			if (clearSelection)
			{
				itemSlots[item].ClearSelection();
			}
			pool.Push(itemSlots[item]);
			itemSlots.Remove(item);
		}
	}

	public void RefreshItem(CItem item)
	{
		RefreshOrRemovedItem(item, clearIfRemoved: false);
	}

	private void RefreshOrRemovedItem(CItem item, bool clearIfRemoved, bool disableIfInvalid = false)
	{
		if (!itemSlots.ContainsKey(item))
		{
			return;
		}
		if (condition != null)
		{
			if (item.SlotState == CItem.EItemSlotState.Selected)
			{
				ItemCardYMLData yMLData = item.YMLData;
				if (yMLData == null || yMLData.Data?.ItemRequirements?.ValidateWhileSelected != true)
				{
					goto IL_00ac;
				}
			}
			if (!condition(item))
			{
				if (disableIfInvalid)
				{
					if (item.SlotState == CItem.EItemSlotState.Selected)
					{
						item.SlotState = CItem.EItemSlotState.Useable;
					}
					SetItemInteractable(item, interactable: false);
				}
				else
				{
					RemoveItem(item, clearIfRemoved);
				}
				return;
			}
		}
		goto IL_00ac;
		IL_00ac:
		SetItemInteractable(item, interactable: true);
		itemSlots[item].Refresh();
	}

	public void SetItemInteractable(CItem item, bool interactable)
	{
		if (itemSlots.ContainsKey(item))
		{
			itemSlots[item].SetInteractable(interactable, updateLook: true);
		}
	}

	public void SetItemsInteractable(bool enable, List<CItem> excludedItems = null, Func<CItem, bool> excludedItemsPredicate = null)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
		{
			if ((excludedItems == null || !excludedItems.Contains(itemSlot.Key)) && (excludedItemsPredicate == null || !excludedItemsPredicate(itemSlot.Key)))
			{
				itemSlot.Value.SetInteractable(enable, updateLook: true);
			}
		}
	}

	private UIUseItemScenario GetSlotFromPool()
	{
		UIUseItemScenario uIUseItemScenario;
		if (pool.Count > 0)
		{
			uIUseItemScenario = pool.Pop();
			uIUseItemScenario.gameObject.SetActive(value: true);
			if (lastCreatedSlot != null)
			{
				uIUseItemScenario.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
			}
			lastCreatedSlot = uIUseItemScenario.transform;
			return uIUseItemScenario;
		}
		uIUseItemScenario = UnityEngine.Object.Instantiate(itemSlotPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseItemScenario.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseItemScenario.transform;
		return uIUseItemScenario;
	}

	public void ShowItems(CActor actor, IEnumerable<CItem> items, Func<CItem, bool> condition = null, Action<CItem, bool> onClickItem = null, bool resetShow = false, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE")
	{
		this.onClickItem = onClickItem;
		this.condition = condition;
		this.tooltip = tooltip;
		this.tooltipTitle = tooltipTitle;
		this.actor = actor;
		isShown = true;
		InfusionBoardUI.Instance?.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		InfusionBoardUI.Instance?.OnReservedElement.AddListener(OnReservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.AddListener(OnUnreservedElement);
		lastCreatedSlot = null;
		if (resetShow)
		{
			Clear();
		}
		else
		{
			foreach (CItem item in itemSlots.Keys.ToList())
			{
				RefreshItem(item);
			}
		}
		foreach (CItem item2 in items)
		{
			AddItem(item2);
		}
		if (itemSlots.Any((KeyValuePair<CItem, UIUseItemScenario> pair) => pair.Value.gameObject.activeSelf && !pair.Key.GetVar("SkipSaveInfusionsState").GetBool()))
		{
			ElementInfusionBoardManager.SaveState();
			InfusionBoardUI.Instance?.SaveState();
		}
	}

	public void ShowItems(CActor inventoryOwner, Func<CItem, bool> condition = null, Action<CItem, bool> onClickItem = null, bool clear = false, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE")
	{
		ShowItems(inventoryOwner, inventoryOwner.Inventory.AllItemsAndPets, condition, action, clear, tooltip, tooltipTitle);
		void action(CItem item, bool networkActionIfOnline = true)
		{
			if (onClickItem != null)
			{
				if (FFSNetwork.IsOnline && inventoryOwner.IsUnderMyControl() && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation && ActionProcessor.CurrentPhase != ActionPhaseType.ShieldConfirmation)
				{
					Synchronizer.SendGameAction(GameActionType.ClickItemBonusSlot, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, inventoryOwner.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemToken(item.NetworkID, -1, int.MaxValue, item.ChosenElement, itemSlots[item].GetSelectedInfusions()));
				}
				onClickItem(item, networkActionIfOnline);
			}
		}
	}

	public void ShowUsableItems(CActor inventoryOwner, Func<CItem, bool> condition = null, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE")
	{
		ShowItems(inventoryOwner, inventoryOwner.Inventory.AllItemsAndPets, ExtendedCondition, OnItemClicked, resetShow: false, tooltip, tooltipTitle);
		bool ExtendedCondition(CItem item)
		{
			if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Useable && item.SlotState != CItem.EItemSlotState.Locked)
			{
				return false;
			}
			if (condition != null)
			{
				return condition(item);
			}
			return true;
		}
		void OnItemClicked(CItem item, bool networkActionIfOnline)
		{
			new UseItemService(inventoryOwner).UseItem(item, networkActionIfOnline, itemSlots[item].GetSelectedInfusions());
		}
	}

	public void Clear(bool deselectSelectedItems = false)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> item in itemSlots.Reverse())
		{
			if (item.Key.SlotState == CItem.EItemSlotState.Selected)
			{
				item.Value.ClearSelection();
				if (deselectSelectedItems && !actor.Inventory.HasRemainingItemAbilities(actor, item.Key))
				{
					actor.Inventory.DeselectItem(item.Key);
					item.Key.SlotState = CItem.EItemSlotState.Useable;
				}
			}
			item.Value.gameObject.SetActive(value: false);
			pool.Push(item.Value);
		}
		if (itemSlots.Count > 0)
		{
			OnChangedItemsAvailable.Invoke();
		}
		itemSlots.Clear();
		unavailableItems.Clear();
	}

	public void ShowCurrentlyPlayingItems(CActor inventoryOwner, Func<CItem, bool> condition = null, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE")
	{
		if (inventoryOwner?.Inventory != null)
		{
			ShowItems(inventoryOwner, inventoryOwner.Inventory.AllItemsAndPets, ExtendedCondition, OnItemClicked, resetShow: false, tooltip, tooltipTitle);
		}
		bool ExtendedCondition(CItem item)
		{
			if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Useable && item.SlotState != CItem.EItemSlotState.Locked)
			{
				return false;
			}
			if (!inventoryOwner.Inventory.HasCurrentItemAbilities(inventoryOwner, item) && !inventoryOwner.Inventory.HasRemainingItemAbilities(inventoryOwner, item))
			{
				return false;
			}
			if (condition != null)
			{
				return condition(item);
			}
			return true;
		}
		void OnItemClicked(CItem item, bool networkActionIfOnline)
		{
			new UseItemService(inventoryOwner).UseItem(item, networkActionIfOnline, itemSlots[item].GetSelectedInfusions());
		}
	}

	public void Hide(bool resetSlots = true, bool deselectItems = false)
	{
		InfusionBoardUI.Instance?.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance?.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		if (resetSlots)
		{
			Clear(deselectItems);
		}
		else
		{
			foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
			{
				itemSlot.Value.gameObject.SetActive(value: false);
			}
			OnChangedItemsAvailable.Invoke();
		}
		isShown = false;
		tooltip = null;
	}

	public void RefreshItems(bool clear = false, bool disableIfInvalid = false)
	{
		foreach (CItem item in itemSlots.Keys.ToList())
		{
			RefreshOrRemovedItem(item, clear, disableIfInvalid);
		}
		RefreshTooltip();
	}

	public void SetCustomTooltips(string tooltip, string tooltipTitle)
	{
		this.tooltip = tooltip;
		this.tooltipTitle = tooltipTitle;
	}

	public void RefreshTooltip()
	{
		if (isShown && tooltip.IsNOTNullOrEmpty() && HasShownUsableSlots())
		{
			Singleton<HelpBox>.Instance.ShowControllerOrKeyboardTip(LocalizationManager.GetTranslation(tooltip), Singleton<HelpBox>.Instance.ControllerText, LocalizationManager.GetTranslation(tooltipTitle ?? "GUI_USE_ITEM_TITLE"), Singleton<HelpBox>.Instance.ControllerTitle, "USE_ITEM_BAR");
		}
		else
		{
			Singleton<HelpBox>.Instance.HideKeyboard("USE_ITEM_BAR", onlyIfIdMatches: true);
		}
	}

	public void SetTooltip(string tooltip, string tooltipTitle)
	{
		this.tooltip = tooltip;
		this.tooltipTitle = tooltipTitle;
		RefreshTooltip();
	}

	public void ProxyUseItemBonus(GameAction action)
	{
		ProxyUseItemBonus((action.SupplementaryDataToken as ItemToken).ItemNetworkID, action.SupplementaryDataToken as ItemToken);
	}

	public void ProxyUseItemBonus(uint itemNetworkID, ItemToken itemToken)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
		{
			Log.NetworkTrace("Slotted Item " + itemSlot.Key.Name + " (NetworkID: " + itemSlot.Key.NetworkID + ", ItemGuid: " + itemSlot.Key.ItemGuid + ").");
		}
		KeyValuePair<CItem, UIUseItemScenario> tObj = itemSlots.FirstOrDefault((KeyValuePair<CItem, UIUseItemScenario> x) => x.Key.NetworkID == itemNetworkID);
		if (!tObj.IsTNull())
		{
			if (!tObj.Value.IsSelected())
			{
				List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>(itemToken.ChosenElement.Length);
				for (int num = 0; num < itemToken.ChosenElement.Length; num++)
				{
					list.Add((ElementInfusionBoardManager.EElement)itemToken.ChosenElement[num]);
				}
				tObj.Key.ChosenElement = list;
				List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>(itemToken.InfusionElements.Length);
				for (int num2 = 0; num2 < itemToken.InfusionElements.Length; num2++)
				{
					list2.Add((ElementInfusionBoardManager.EElement)itemToken.InfusionElements[num2]);
				}
				tObj.Value.SetInfusions(list2);
				tObj.Value.SetAnyConsume(list);
			}
			tObj.Value.OnPointerDown();
			return;
		}
		throw new Exception("Error using activatable item bonus for proxy playerActor. No such bonus found with item NetworkID: " + itemNetworkID);
	}
}
