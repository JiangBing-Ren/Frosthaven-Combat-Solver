using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;

public class UseItemService
{
	private CActor inventoryOwner;

	public UseItemService(CActor inventoryOwner)
	{
		this.inventoryOwner = inventoryOwner;
	}

	public void UseItem(CItem Item, bool networkActionIfOnline = true, List<ElementInfusionBoardManager.EElement> infusions = null)
	{
		if (FFSNetwork.IsOnline)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionProcessor.CurrentPhase, savePreviousState: true);
		}
		if (Item == null)
		{
			Log.GUIInfo("Invalid item");
			return;
		}
		if (Item.YMLData.Trigger == CItem.EItemTrigger.PassiveEffect)
		{
			Log.GUIInfo("Passive items can't be selected.");
			return;
		}
		if (Item.SlotState != CItem.EItemSlotState.Useable && Item.SlotState != CItem.EItemSlotState.Selected)
		{
			Log.GUIInfo("Item is not in a usable state.");
			return;
		}
		Log.NetworkInfo("Item.NetworkID: " + Item.NetworkID + "Item.ItemGuid: " + Item.ItemGuid + "Item.SlotType: " + Item.YMLData.Slot);
		if ((FFSNetwork.IsOnline && inventoryOwner.IsUnderMyControl()) & networkActionIfOnline)
		{
			Synchronizer.SendGameAction(GameActionType.UseItem, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, inventoryOwner.ID, 0, 0, 0, supplementaryDataBoolean: false, null, new ItemToken(Item.NetworkID, (int)Item.YMLData.Slot, int.MaxValue, Item.ChosenElement, infusions));
		}
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
		ScenarioRuleClient.ToggleItem(Item, inventoryOwner);
	}
}
