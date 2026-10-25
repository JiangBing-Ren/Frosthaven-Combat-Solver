using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

public class ItemsUI : MonoBehaviour
{
	[SerializeField]
	protected GameObject itemPrefab;

	[SerializeField]
	protected List<GameObject> unfocusedMasks;

	protected CActor m_Actor;

	private List<UIItemScenario> m_AllItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_HeadItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_BodyItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_LegsItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_OneHandItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_TwoHandItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_SmallItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_QuestItems = new List<UIItemScenario>();

	private List<UIItemScenario> m_Pets = new List<UIItemScenario>();

	[StateDump(Skip = true)]
	private List<InteractabilityIsolatedUIControl> m_IsolatableControls = new List<InteractabilityIsolatedUIControl>();

	private List<UIItemScenario> AllItems
	{
		get
		{
			m_AllItems.Clear();
			m_AllItems.AddRange(m_HeadItems);
			m_AllItems.AddRange(m_BodyItems);
			m_AllItems.AddRange(m_LegsItems);
			m_AllItems.AddRange(m_OneHandItems);
			m_AllItems.AddRange(m_TwoHandItems);
			m_AllItems.AddRange(m_SmallItems);
			m_AllItems.AddRange(m_QuestItems);
			m_AllItems.AddRange(m_Pets);
			return m_AllItems;
		}
	}

	private void OnDestroy()
	{
		SetActor(null, subscribeToEvents: false);
	}

	private void SetItemUIState(CItem item)
	{
		UIItemScenario uIItemScenario = AllItems.FirstOrDefault((UIItemScenario x) => x.Item != null && x.Item.ItemGuid == item.ItemGuid);
		if (uIItemScenario != null)
		{
			uIItemScenario.RefreshItemState();
		}
	}

	private void OnItemAdded(object o, EventArgs e)
	{
		Log.GUIInfo("OnItemAdded: " + (o as CItem).Name);
		RefreshItems();
	}

	private void OnItemRemoved(object o, EventArgs e)
	{
		Log.GUIInfo("OnItemRemoved: " + (o as CItem).Name);
		RefreshItems();
	}

	private void OnItemRefreshed(object o, EventArgs e)
	{
		Log.GUIInfo("OnItemRefreshed: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RefreshItem(o as CItem);
	}

	private void OnItemUseable(object o, EventArgs e)
	{
		Log.GUIInfo("OnItemUseable: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.AddItem(o as CItem);
	}

	private void OnItemNoLongerUseable(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Log.GUIInfo("OnItemNoLongerUseable: " + cItem.Name + " " + cItem.SlotState);
		SetItemUIState(cItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemSelected(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Log.GUIInfo("OnItemSelected: " + cItem.Name);
		Singleton<UIUseItemsBar>.Instance.RefreshItems(clear: false, disableIfInvalid: true);
		Singleton<TakeDamagePanel>.Instance.PreviewDamage();
		SetItemUIState(cItem);
		if ((!(cItem.FindAbilityOnItemCard(CAbility.EAbilityType.Infuse) is CAbilityInfuse { ShowPicker: not false, ElementsToInfuse: var elementsToInfuse }) || !elementsToInfuse.Any((ElementInfusionBoardManager.EElement el) => el == ElementInfusionBoardManager.EElement.Any)) && !cItem.GetVar("SkipSaveInfusionsState").GetBool())
		{
			ElementInfusionBoardManager.SaveState();
			InfusionBoardUI.Instance?.SaveState();
		}
		WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
		WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
	}

	private void OnItemDeselected(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Log.GUIInfo("OnItemDeselected: " + cItem.Name);
		Singleton<UIUseItemsBar>.Instance.RefreshItems(clear: false, disableIfInvalid: true);
		SetItemUIState(cItem);
		ResetItemElements(cItem);
		if (!cItem.ChosenElement.IsNullOrEmpty())
		{
			cItem.ChosenElement.FindAll((ElementInfusionBoardManager.EElement it) => it != ElementInfusionBoardManager.EElement.Any);
			cItem.ChosenElement.Clear();
		}
		InfusionBoardUI.Instance?.MarkElementsDirty();
		InfusionBoardUI.Instance?.UpdateBoard();
		WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
		WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
	}

	private void OnItemSpent(object o, EventArgs e)
	{
		Log.GUIInfo("OnItemSpent: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemConsumed(object o, EventArgs e)
	{
		Log.GUIInfo("OnItemConsumed: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemUnrestrictedUsed(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Log.GUIInfo("OnItemUnrestrictedUsed: " + cItem.Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemActive(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Log.GUIInfo("OnItemActive: " + cItem.Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void RefreshItems()
	{
		RefreshItemsList(m_Actor.Inventory.HeadItems, m_HeadItems);
		RefreshItemsList(m_Actor.Inventory.BodyItems, m_BodyItems);
		RefreshItemsList(m_Actor.Inventory.LegsItems, m_LegsItems);
		RefreshItemsList(m_Actor.Inventory.OneHandItems, m_OneHandItems);
		RefreshItemsList(m_Actor.Inventory.TwoHandItems, m_TwoHandItems);
		RefreshItemsList(m_Actor.Inventory.SmallItems, m_SmallItems);
		RefreshItemsList(m_Actor.Inventory.QuestItems, m_QuestItems);
		RefreshItemsList(CInventory.AllPets().ToList(), m_Pets);
		SortItems();
		ClearOutInteractabilityControls();
		SetUpInteractabilityControls();
		SetUnfocused(FFSNetwork.IsOnline && !m_Actor.IsUnderMyControl());
	}

	private void RefreshItemsList(List<CItem> items, List<UIItemScenario> uiItems)
	{
		if (items.Count != uiItems.Count || ScenarioManager.IsFastReloading)
		{
			foreach (UIItemScenario uiItem in uiItems)
			{
				UnityEngine.Object.Destroy(uiItem.gameObject);
			}
			uiItems.Clear();
			for (int i = 0; i < items.Count; i++)
			{
				UIItemScenario component = UnityEngine.Object.Instantiate(itemPrefab, transform).GetComponent<UIItemScenario>();
				component.SetItem(m_Actor, items[i].Slot, i);
				uiItems.Add(component);
			}
			return;
		}
		foreach (UIItemScenario uiItem2 in uiItems)
		{
			uiItem2.Refresh();
		}
	}

	public void SetActor(CActor actor, bool subscribeToEvents = true, bool force = false)
	{
		if ((actor != m_Actor) | force)
		{
			if (m_Actor != null)
			{
				m_Actor.Inventory.ItemAdded -= OnItemAdded;
				m_Actor.Inventory.ItemRemoved -= OnItemRemoved;
				m_Actor.Inventory.ItemRefreshed -= OnItemRefreshed;
				m_Actor.Inventory.ItemUsable -= OnItemUseable;
				m_Actor.Inventory.ItemNoLongerUsable -= OnItemNoLongerUseable;
				m_Actor.Inventory.ItemSelected -= OnItemSelected;
				m_Actor.Inventory.ItemDeSelected -= OnItemDeselected;
				m_Actor.Inventory.ItemSpent -= OnItemSpent;
				m_Actor.Inventory.ItemConsumed -= OnItemConsumed;
				m_Actor.Inventory.ItemUnrestrictedUsed -= OnItemUnrestrictedUsed;
				m_Actor.Inventory.ItemActive -= OnItemActive;
			}
			m_Actor = actor;
			if (subscribeToEvents)
			{
				m_Actor.Inventory.ItemAdded += OnItemAdded;
				m_Actor.Inventory.ItemRemoved += OnItemRemoved;
				m_Actor.Inventory.ItemRefreshed += OnItemRefreshed;
				m_Actor.Inventory.ItemUsable += OnItemUseable;
				m_Actor.Inventory.ItemNoLongerUsable += OnItemNoLongerUseable;
				m_Actor.Inventory.ItemSelected += OnItemSelected;
				m_Actor.Inventory.ItemDeSelected += OnItemDeselected;
				m_Actor.Inventory.ItemSpent += OnItemSpent;
				m_Actor.Inventory.ItemConsumed += OnItemConsumed;
				m_Actor.Inventory.ItemUnrestrictedUsed += OnItemUnrestrictedUsed;
				m_Actor.Inventory.ItemActive += OnItemActive;
			}
			if (m_Actor != null)
			{
				RefreshItems();
			}
		}
	}

	public void RefreshItemsState()
	{
		foreach (UIItemScenario allItem in AllItems)
		{
			allItem.RefreshItemState();
		}
	}

	public void ClearOutInteractabilityControls()
	{
		foreach (InteractabilityIsolatedUIControl isolatableControl in m_IsolatableControls)
		{
			UnityEngine.Object.Destroy(isolatableControl);
		}
		m_IsolatableControls.Clear();
	}

	public void SetUpInteractabilityControls()
	{
		CreateInteractabilityControls(m_HeadItems);
		CreateInteractabilityControls(m_BodyItems);
		CreateInteractabilityControls(m_LegsItems);
		CreateInteractabilityControls(m_OneHandItems);
		CreateInteractabilityControls(m_TwoHandItems);
		CreateInteractabilityControls(m_SmallItems);
		CreateInteractabilityControls(m_QuestItems);
		CreateInteractabilityControls(m_Pets);
	}

	private void CreateInteractabilityControls(List<UIItemScenario> uiItems)
	{
		for (int i = 0; i < uiItems.Count; i++)
		{
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl = uiItems[i].gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			interactabilityIsolatedUIControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
			interactabilityIsolatedUIControl.ControlIdentifier = uiItems[i].Item.Slot.ToString();
			interactabilityIsolatedUIControl.ControlSecondIdentifier = m_Actor.ActorGuid;
			interactabilityIsolatedUIControl.ControlIndex = i;
			interactabilityIsolatedUIControl.ExtendedButtonsToAllow.Add(uiItems[i].GetComponent<ExtendedButton>());
			interactabilityIsolatedUIControl.Initialise();
			m_IsolatableControls.Add(interactabilityIsolatedUIControl);
		}
	}

	private void SortItems()
	{
		UIItemScenario componentInChildren = transform.GetComponentInChildren<UIItemScenario>();
		if (componentInChildren == null)
		{
			return;
		}
		int num = componentInChildren.transform.GetSiblingIndex();
		foreach (UIItemScenario allItem in AllItems)
		{
			allItem.transform.SetSiblingIndex(num);
			num++;
		}
	}

	public List<InfuseElement> PickUnselectedInfusionsForItem(CItem item)
	{
		UIItemScenario uIItemScenario = AllItems.FirstOrDefault((UIItemScenario x) => x.Item != null && x.Item.ID == item.ID);
		if (uIItemScenario != null)
		{
			return uIItemScenario.UnselectedInfusions();
		}
		return null;
	}

	public void ResetItemElements(CItem item)
	{
		UIItemScenario uIItemScenario = AllItems.FirstOrDefault((UIItemScenario x) => x.Item != null && x.Item.ID == item.ID);
		if (uIItemScenario != null)
		{
			uIItemScenario.ResetItemElements();
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		foreach (GameObject unfocusedMask in unfocusedMasks)
		{
			if (unfocused)
			{
				unfocusedMask.transform.SetAsLastSibling();
			}
			unfocusedMask.SetActive(unfocused);
		}
	}
}
