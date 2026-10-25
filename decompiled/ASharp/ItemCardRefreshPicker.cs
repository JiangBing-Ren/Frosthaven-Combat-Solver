using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardRefreshPicker : Singleton<ItemCardRefreshPicker>
{
	[SerializeField]
	private ItemCardPicker picker;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private TMP_Text confirmButtonText;

	[SerializeField]
	private GameObject intertactableEffect;

	private CActor actor;

	private bool refreshingItems;

	private Color defaultColorConfirmText;

	private CAbility ability;

	protected override void AwakeInit()
	{
		defaultColorConfirmText = confirmButtonText.color;
		confirmButton.onClick.AddListener(ConfirmSelectedCards);
		enabled = false;
	}

	public void Show(CActor actor, bool refreshingItems, List<CItem.EItemSlotState> slotStatesToChooseFrom, List<CItem.EItemSlot> slotsToChooseFrom, List<CItem> specificItems, int itemsToSelect = 1, bool maxSelectIsOptional = false, CAbility ability = null)
	{
		this.actor = actor;
		this.refreshingItems = refreshingItems;
		this.ability = ability;
		List<CItem> list = ((specificItems != null && specificItems.Count > 0) ? specificItems : actor.Inventory.AllItems.FindAll((CItem it) => it.YMLData.PermanentlyConsumed != true && (slotStatesToChooseFrom == null || slotStatesToChooseFrom.Count == 0 || slotStatesToChooseFrom.Contains(it.SlotState)) && (slotsToChooseFrom == null || slotsToChooseFrom.Count == 0 || slotsToChooseFrom.Contains(it.YMLData.Slot)))).ToList();
		CInventory.RemoveBeforeScenarioItems(list);
		if (itemsToSelect == 0 || list.Count == 0)
		{
			Finish();
			return;
		}
		picker.Show(list, itemsToSelect, (List<CItem> items) =>
		{
			SetConfirmButtonInteractable(interact: true);
		}, () =>
		{
			SetConfirmButtonInteractable(interact: false);
		}, maxSelectIsOptional);
		SetConfirmButtonInteractable(maxSelectIsOptional);
	}

	public void ShowForOtherPlayers(CActor actor, bool refreshingItems, List<CItem.EItemSlotState> slotStatesToChooseFrom, List<CItem.EItemSlot> slotsToChooseFrom, List<CItem> specificItems, int itemsToSelect = 1, bool maxSelectIsOptional = false, CAbility ability = null)
	{
		this.actor = actor;
		this.refreshingItems = refreshingItems;
		this.ability = ability;
		List<CItem> list = ((specificItems != null && specificItems.Count > 0) ? specificItems : actor.Inventory.AllItems.FindAll((CItem it) => it.YMLData.PermanentlyConsumed != true && (slotStatesToChooseFrom == null || slotStatesToChooseFrom.Count == 0 || slotStatesToChooseFrom.Contains(it.SlotState)) && (slotsToChooseFrom == null || slotsToChooseFrom.Count == 0 || slotsToChooseFrom.Contains(it.YMLData.Slot)))).ToList();
		CInventory.RemoveBeforeScenarioItems(list);
		if (itemsToSelect == 0 || list.Count == 0)
		{
			Finish();
		}
		else
		{
			SetConfirmButtonInteractable(interact: false);
		}
	}

	private void SetConfirmButtonInteractable(bool interact)
	{
		enabled = interact;
		confirmButton.interactable = interact;
		intertactableEffect.SetActive(interact);
		if (!Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			confirmButtonText.color = (interact ? defaultColorConfirmText : UIInfoTools.DefInstance.greyedOutTextColor);
		}
	}

	private void ConfirmSelectedCards()
	{
		SetConfirmButtonInteractable(interact: false);
		picker.Hide();
		foreach (CItem currentSelectedItem in picker.GetCurrentSelectedItems())
		{
			Choreographer.ItemPickerFinishSelectingItem(currentSelectedItem, actor, ability);
		}
		if (FFSNetwork.IsOnline)
		{
			if (refreshingItems)
			{
				Synchronizer.SendGameAction(GameActionType.RefreshItem, ActionPhaseType.ItemRefreshing, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemsToken(picker.GetCurrentSelectedItems()));
			}
			else
			{
				Synchronizer.SendGameAction(GameActionType.ConsumeItem, ActionPhaseType.ItemConsuming, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actor.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemsToken(picker.GetCurrentSelectedItems()));
			}
		}
		Finish();
	}

	private void Finish()
	{
		Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
		ScenarioRuleClient.StepComplete();
	}

	public void ProxyRefreshItems(GameAction action)
	{
		CActor cActor = Choreographer.s_Choreographer.FindPlayerActor(action.ActorID);
		if (cActor != null)
		{
			Log.NetworkInfo("Refreshing item cards for a proxy playerActor.");
			uint[] itemNetworkIDs = ((ItemsToken)action.SupplementaryDataToken).ItemNetworkIDs;
			foreach (uint itemNetworkID in itemNetworkIDs)
			{
				CItem cItem = cActor.Inventory.AllItems.SingleOrDefault((CItem x) => x.NetworkID == itemNetworkID);
				if (cItem != null)
				{
					cActor.Inventory.ReactivateItem(cItem, cActor);
				}
			}
			if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForItemRefresh)
			{
				Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
				ScenarioRuleClient.StepComplete();
			}
			return;
		}
		throw new Exception("Error refreshing items for proxy playerActor. Target actor not found (ActorID: " + cActor.ID + ").");
	}

	public void ProxyConsumeItems(GameAction action)
	{
		CActor cActor = Choreographer.s_Choreographer.FindPlayerActor(action.ActorID);
		if (cActor != null)
		{
			Log.NetworkInfo("Consuming item cards for a proxy playerActor.");
			uint[] itemNetworkIDs = ((ItemsToken)action.SupplementaryDataToken).ItemNetworkIDs;
			foreach (uint itemNetworkID in itemNetworkIDs)
			{
				CItem cItem = cActor.Inventory.AllItems.SingleOrDefault((CItem x) => x.NetworkID == itemNetworkID);
				if (cItem != null)
				{
					Choreographer.ItemPickerFinishSelectingItem(cItem, actor, ability);
				}
			}
			if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForItemRefresh)
			{
				Choreographer.s_Choreographer.SetChoreographerState(Choreographer.ChoreographerStateType.Play, 0, null);
				ScenarioRuleClient.StepComplete();
			}
			return;
		}
		throw new Exception("Error consuming items for proxy playerActor. Target actor not found (ActorID: " + cActor.ID + ").");
	}
}
