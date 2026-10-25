using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CInventory
{
	[StateDump(Skip = true)]
	private List<CItem> m_HeadItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_BodyItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_LegsItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_OneHandItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_TwoHandItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_SmallItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_QuestItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_EquipmentItems = new List<CItem>();

	[StateDump(Skip = true)]
	private List<CItem> m_SelectedItems = new List<CItem>();

	public CActor InventoryActor { get; set; }

	public int SmallItemMax { get; set; }

	public List<CItem> AllItems { get; } = new List<CItem>();

	public IEnumerable<CItem> AllItemsAndPets => AllItems.Concat(AllPets());

	public List<CItem> HeadItems => GetItems(m_HeadItems, (CItem i) => i.Slot == CItem.EItemSlot.Head);

	public List<CItem> BodyItems => GetItems(m_BodyItems, (CItem i) => i.Slot == CItem.EItemSlot.Body);

	public List<CItem> LegsItems => GetItems(m_LegsItems, (CItem i) => i.Slot == CItem.EItemSlot.Legs);

	public List<CItem> OneHandItems => GetItems(m_OneHandItems, (CItem i) => i.Slot == CItem.EItemSlot.OneHand);

	public List<CItem> TwoHandItems => GetItems(m_TwoHandItems, (CItem i) => i.Slot == CItem.EItemSlot.TwoHand);

	public List<CItem> SmallItems => GetItems(m_SmallItems, (CItem i) => i.Slot == CItem.EItemSlot.SmallItem);

	public List<CItem> QuestItems => GetItems(m_QuestItems, (CItem i) => i.Slot == CItem.EItemSlot.QuestItem);

	public List<CItem> EquipmentItems => GetItems(m_EquipmentItems, (CItem i) =>
	{
		CItem.EItemSlot slot = i.Slot;
		return slot == CItem.EItemSlot.Head || slot == CItem.EItemSlot.Body || slot == CItem.EItemSlot.Legs || slot == CItem.EItemSlot.OneHand || slot == CItem.EItemSlot.TwoHand;
	});

	public List<CItem> SelectedItems => GetItems(m_SelectedItems, (CItem i) => i.SlotState == CItem.EItemSlotState.Selected || i.SlotState == CItem.EItemSlotState.Locked);

	public event EventHandler ItemAdded;

	public event EventHandler ItemRemoved;

	public event EventHandler ItemSpent;

	public event EventHandler ItemConsumed;

	public event EventHandler ItemUnrestrictedUsed;

	public event EventHandler ItemFlipped;

	public event EventHandler ItemRefreshed;

	public event EventHandler ItemSelected;

	public event EventHandler ItemDeSelected;

	public event EventHandler ItemUsable;

	public event EventHandler ItemNoLongerUsable;

	public event EventHandler ItemActive;

	public event EventHandler ItemLocked;

	private static void SendClientItemMessage(EventHandler callback, CItem item, ESESubTypeItem callbackType, CActor actor = null, bool hideLog = false)
	{
		COnItemCallback_MessageData message = new COnItemCallback_MessageData(actor)
		{
			m_Callback = callback,
			m_Item = item,
			m_CallbackType = callbackType,
			m_HideLog = hideLog
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	protected virtual void OnItemAdded(CItem item)
	{
		if (ItemAdded != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemAdded, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemAdded, item, ESESubTypeItem.ItemAdded, InventoryActor);
		}
	}

	protected virtual void OnItemRemoved(CItem item)
	{
		if (ItemRemoved != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemRemoved, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemRemoved, item, ESESubTypeItem.ItemRemoved, InventoryActor);
		}
	}

	protected virtual void OnItemSpent(CItem item)
	{
		if (ItemSpent != null)
		{
			CChallenge.CheckTrigger(CChallenge.Trigger.OnItemSpent, new CChallenge.TriggerParams
			{
				actor = InventoryActor,
				item = item
			});
			if (item.SlotState == CItem.EItemSlotState.Spent)
			{
				HandleItemCost(item);
				HandleItemCost(item);
				SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemSpent, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
				SendClientItemMessage(ItemSpent, item, ESESubTypeItem.ItemSpent, InventoryActor);
			}
		}
	}

	protected virtual void OnItemConsumed(CItem item)
	{
		if (ItemConsumed != null)
		{
			HandleItemCost(item);
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemConsumed, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemConsumed, item, ESESubTypeItem.ItemConsumed, InventoryActor);
		}
	}

	protected virtual void OnItemLocked(CItem item)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemLocked, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
		SendClientItemMessage(ItemLocked, item, ESESubTypeItem.ItemLocked, InventoryActor);
	}

	protected virtual void OnItemUnrestrictedUsed(CItem item, bool hideLog)
	{
		if (ItemUnrestrictedUsed != null)
		{
			HandleItemCost(item);
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemUnrestrictedUsed, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemUnrestrictedUsed, item, ESESubTypeItem.ItemUnrestrictedUsed, InventoryActor, hideLog);
		}
	}

	protected virtual void OnItemFlipped(CItem item)
	{
		if (ItemFlipped != null)
		{
			HandleItemCost(item);
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemFlipped, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemFlipped, item, ESESubTypeItem.ItemFlipped, InventoryActor);
		}
	}

	protected virtual void OnItemRefreshed(CItem item, CActor actor)
	{
		if (ItemRefreshed != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemRefreshed, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemRefreshed, item, ESESubTypeItem.ItemRefreshed, actor);
		}
	}

	protected virtual void OnItemSelected(CItem item)
	{
		if (ItemSelected != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemSelected, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemSelected, item, ESESubTypeItem.ItemSelected, InventoryActor);
		}
	}

	protected virtual void OnItemDeSelected(CItem item)
	{
		if (item.GetVar("ApplyAbilitiesToItemOwner").GetBool())
		{
			CItem.EItemSlotState slotState = item.SlotState;
			item.SlotState = CItem.EItemSlotState.TempLocked;
			CancelActiveBonusesOnRefreshingItems(item);
			item.SlotState = slotState;
		}
		if (ItemDeSelected != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemDeSelected, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemDeSelected, item, ESESubTypeItem.ItemDeSelected, InventoryActor);
			UndoOverridesAndSubAbilities(item, PhaseManager.CurrentPhaseAction);
		}
	}

	protected virtual void OnItemUsable(CItem item)
	{
		if (ItemUsable != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemUsable, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemUsable, item, ESESubTypeItem.ItemUsable, InventoryActor);
		}
	}

	protected virtual void OnItemNoLongerUsable(CItem item)
	{
		if (ItemNoLongerUsable != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemNoLongerUsable, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemNoLongerUsable, item, ESESubTypeItem.ItemNoLongerUsable, InventoryActor);
		}
	}

	protected virtual void OnItemActive(CItem item)
	{
		if (ItemActive != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemActive, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(ItemActive, item, ESESubTypeItem.ItemActive, InventoryActor);
		}
	}

	public static IEnumerable<CItem> AllPets()
	{
		return ScenarioManager.CurrentScenarioState.Pets;
	}

	public CInventory(CActor inventoryActor, int maxSmallItems)
	{
		InventoryActor = inventoryActor;
		SmallItemMax = maxSmallItems;
	}

	private List<CItem> GetItems(List<CItem> outItems, Func<CItem, bool> predicate)
	{
		outItems.Clear();
		foreach (CItem allItem in AllItems)
		{
			if (predicate(allItem))
			{
				outItems.Add(allItem);
			}
		}
		return outItems;
	}

	public void AddItem(CItem item, bool reactivate = true)
	{
		item.GenerateNetworkID(InventoryActor.ID);
		AllItems.Add(item);
		if (reactivate)
		{
			ReactivateItem(item, InventoryActor, force: true);
		}
		else
		{
			item.SlotState = CItem.EItemSlotState.Equipped;
		}
		OnItemAdded(item);
	}

	public void RemoveItem(CItem item)
	{
		if (item != null)
		{
			DeselectItem(item);
			ReactivateItem(item);
			AllItems.Remove(item);
			OnItemRemoved(item);
		}
	}

	public CItem GetItem(CItem.EItemSlot slot, int index = 0)
	{
		if (slot == CItem.EItemSlot.Pet)
		{
			List<CItem> list = AllPets().ToList();
			if (index < list.Count)
			{
				return list[index];
			}
			return null;
		}
		if (slot == CItem.EItemSlot.Head)
		{
			List<CItem> headItems = HeadItems;
			if (index < headItems.Count)
			{
				return headItems[index];
			}
			return null;
		}
		if (slot == CItem.EItemSlot.Body)
		{
			List<CItem> bodyItems = BodyItems;
			if (index < bodyItems.Count)
			{
				return bodyItems[index];
			}
			return null;
		}
		if (slot == CItem.EItemSlot.Legs)
		{
			List<CItem> legsItems = LegsItems;
			if (index < legsItems.Count)
			{
				return legsItems[index];
			}
			return null;
		}
		if (slot == CItem.EItemSlot.OneHand)
		{
			List<CItem> oneHandItems = OneHandItems;
			if (index < oneHandItems.Count)
			{
				return oneHandItems[index];
			}
			return null;
		}
		if (slot == CItem.EItemSlot.TwoHand)
		{
			List<CItem> twoHandItems = TwoHandItems;
			if (index < twoHandItems.Count)
			{
				return twoHandItems[index];
			}
		}
		if (slot == CItem.EItemSlot.SmallItem)
		{
			List<CItem> smallItems = SmallItems;
			if (index < smallItems.Count)
			{
				return smallItems[index];
			}
			return null;
		}
		if (slot == CItem.EItemSlot.QuestItem)
		{
			List<CItem> questItems = QuestItems;
			if (index < questItems.Count)
			{
				return questItems[index];
			}
			return null;
		}
		return AllItems.FirstOrDefault((CItem i) => i.Slot == slot);
	}

	public void ToggleItem(CItem item)
	{
		CActor singleTarget = item.SingleTarget;
		if (item.SlotState != CItem.EItemSlotState.Selected && item.YMLData.ItemType == CItem.EItemType.Ability && !item.GetVar("ApplyAbilitiesToItemOwner").GetBool())
		{
			CItem cItem = SelectedItems.SingleOrDefault((CItem a) => a.YMLData.ItemType == CItem.EItemType.Ability);
			if (cItem != null)
			{
				CheckForItemAbilityOverridesAndSubAbilitiesToDeselect();
				DeselectItemAbility(cItem);
			}
		}
		if (item.SlotState != CItem.EItemSlotState.Selected)
		{
			if (item.YMLData.ItemType == CItem.EItemType.Ability)
			{
				if (item.GetVar("ApplyAbilitiesToItemOwner").GetBool())
				{
					SelectItem(item);
					{
						foreach (CAbility ability in item.YMLData.Data.Abilities)
						{
							CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
							cAbility.Start(InventoryActor, InventoryActor);
							cAbility.ApplyToActor(InventoryActor);
						}
						return;
					}
				}
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
				{
					cPhaseAction.StackItemAbilities(item);
					return;
				}
				CPhase currentPhase = PhaseManager.CurrentPhase;
				if (!(currentPhase is CPhaseStartTurn))
				{
					if (!(currentPhase is CPhaseActionSelection))
					{
						if (currentPhase is CPhaseEndRound || currentPhase is CPhaseEndTurn)
						{
							if (item.YMLData.Trigger == CItem.EItemTrigger.DuringOwnTurn)
							{
								ToggleItemAbility(item);
							}
							else
							{
								Log.ErrorWithReport("Item triggered at invalid phase");
							}
						}
					}
					else if (item.YMLData.Trigger == CItem.EItemTrigger.DuringOwnTurn || (item.YMLData.Trigger == CItem.EItemTrigger.AtEndOfTurn && (GameState.GetCurrentActionSequenceType(InventoryActor.SelectedRoundCardsCount) == GameState.EActionSequenceType.Complete || InventoryActor.Class is CCharacterClass { HasLongRested: not false })))
					{
						ToggleItemAbility(item);
					}
					else
					{
						Log.ErrorWithReport("Item triggered at invalid phase");
					}
				}
				else if (item.YMLData.Trigger == CItem.EItemTrigger.AtStartOfRound || item.YMLData.Trigger == CItem.EItemTrigger.DuringOwnTurn)
				{
					ToggleItemAbility(item);
				}
				else
				{
					Log.ErrorWithReport("Item triggered at invalid phase");
				}
			}
			else if (item.YMLData.ItemType == CItem.EItemType.Override)
			{
				SelectItem(item);
				bool flag = false;
				if (item.YMLData.Consumes.Count == 0 || (item.YMLData.Consumes.Count > 0 && (item.YMLData.Consumes[0] != ElementInfusionBoardManager.EElement.Any || (item.YMLData.Consumes[0] == ElementInfusionBoardManager.EElement.Any && item.ChosenElement != null && item.ChosenElement.Count > 0))))
				{
					if (PhaseManager.CurrentPhase is CPhaseAction)
					{
						CPhaseAction cPhaseAction2 = PhaseManager.CurrentPhase as CPhaseAction;
						if (cPhaseAction2.CurrentPhaseAbility?.m_Ability != null)
						{
							if (item.YMLData.Trigger == CItem.EItemTrigger.EntireAction)
							{
								foreach (CAbilityOverride @override in item.YMLData.Data.Overrides)
								{
									CAbilityOverride cAbilityOverride = null;
									if ((item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(cPhaseAction2.CurrentPhaseAbility.m_Ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(cPhaseAction2.CurrentPhaseAbility.m_Ability.TargetingActor, cPhaseAction2.CurrentPhaseAbility.m_Ability, null, item)))
									{
										flag = true;
										cAbilityOverride = cPhaseAction2.CurrentPhaseAbility.m_Ability.OverrideAbilityValues(@override, perform: false, item);
									}
									foreach (CPhaseAction.CPhaseAbility item2 in cPhaseAction2.RemainingPhaseAbilities.Where((CPhaseAction.CPhaseAbility w) => !w.ItemID.HasValue))
									{
										if ((item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(item2.m_Ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(item2.m_Ability.TargetingActor, item2.m_Ability, null, item)))
										{
											flag = true;
											CAbilityOverride cAbilityOverride2 = item2.m_Ability.OverrideAbilityValues(@override, perform: false, item);
											if (cAbilityOverride2 != null)
											{
												cAbilityOverride = cAbilityOverride2;
											}
										}
									}
									if (cAbilityOverride.SubAbilities == null)
									{
										continue;
									}
									foreach (CAbility subAbility in cAbilityOverride.SubAbilities)
									{
										flag = true;
										CAbility cAbility2 = CAbility.CopyAbility(subAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, cPhaseAction2.CurrentPhaseAbility.m_Ability?.VarsContext);
										cAbility2.ParentAbility = cPhaseAction2.CurrentPhaseAbility.m_Ability;
										if (!cAbility2.IsInlineSubAbility)
										{
											cPhaseAction2.RemainingPhaseAbilities.Add(new CPhaseAction.CPhaseAbility(cAbility2, cPhaseAction2.CurrentPhaseAbility.TargetingActor, item, null, item.ID));
										}
									}
								}
								if (flag)
								{
									cPhaseAction2.CurrentPhaseAbility.m_Ability.Perform();
								}
							}
							else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleAbility)
							{
								foreach (CAbilityOverride override2 in item.YMLData.Data.Overrides)
								{
									CAbilityOverride cAbilityOverride3 = cPhaseAction2.CurrentPhaseAbility.m_Ability.OverrideAbilityValues(override2, perform: false, item);
									if (cAbilityOverride3.SubAbilities == null)
									{
										continue;
									}
									foreach (CAbility subAbility2 in cAbilityOverride3.SubAbilities)
									{
										CAbility cAbility3 = CAbility.CopyAbility(subAbility2, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, cPhaseAction2.CurrentPhaseAbility.m_Ability?.VarsContext);
										cAbility3.ParentAbility = cPhaseAction2.CurrentPhaseAbility.m_Ability;
										if (cPhaseAction2.CurrentPhaseAbilities.Count > 1)
										{
											PhaseManager.StartAbilities(cAbility3, cPhaseAction2.CurrentPhaseAbility.TargetingActor, stackInsteadOfInject: true, null, item);
										}
										else
										{
											cPhaseAction2.RemainingPhaseAbilities.Insert(0, new CPhaseAction.CPhaseAbility(cAbility3, cPhaseAction2.CurrentPhaseAbility.TargetingActor, item, null, item.ID));
										}
									}
								}
								cPhaseAction2.CurrentPhaseAbility.m_Ability.Perform();
							}
							else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleTarget)
							{
								cPhaseAction2.CurrentPhaseAbility.m_Ability.ToggleSingleTargetItem(item);
							}
							else
							{
								Log.ErrorWithReport("Invalid Item Trigger");
							}
						}
						else
						{
							Log.ErrorWithReport("Invalid phase for item");
						}
					}
					else
					{
						Log.ErrorWithReport("Invalid phase for item");
					}
				}
				else
				{
					Log.Warning("Cannot activate " + item.YMLData.StringID + ". No Element selected.");
				}
			}
			else if (item.YMLData.ItemType != CItem.EItemType.Vars)
			{
				Log.ErrorWithReport("Invalid Item Type");
			}
		}
		else if (item.YMLData.ItemType == CItem.EItemType.Ability)
		{
			CheckForItemAbilityOverridesAndSubAbilitiesToDeselect();
			DeselectItemAbility(item);
		}
		else if (item.YMLData.ItemType == CItem.EItemType.Override)
		{
			if (PhaseManager.CurrentPhase is CPhaseAction { CurrentPhaseAbility: var currentPhaseAbility } cPhaseAction3)
			{
				if (currentPhaseAbility?.m_Ability != null)
				{
					DeselectItem(item);
					if (item.YMLData.Trigger == CItem.EItemTrigger.SingleAbility || (item.YMLData.Trigger == CItem.EItemTrigger.EntireAction && (item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(cPhaseAction3.CurrentPhaseAbility.m_Ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(cPhaseAction3.CurrentPhaseAbility.m_Ability.TargetingActor, cPhaseAction3.CurrentPhaseAbility.m_Ability, null, item))))
					{
						cPhaseAction3.CurrentPhaseAbility.m_Ability.Perform();
					}
					else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleTarget)
					{
						cPhaseAction3.CurrentPhaseAbility.m_Ability.ToggleSingleTargetItem(item);
						List<CActor> targetActors = null;
						if (item.SlotState == CItem.EItemSlotState.Useable)
						{
							targetActors = ((singleTarget == null) ? cPhaseAction3.CurrentPhaseAbility.m_Ability.ActorsToTarget.ToList() : new List<CActor> { singleTarget });
						}
						cPhaseAction3.CurrentPhaseAbility.m_Ability.TargetingActor.Inventory.HighlightUsableItems(cPhaseAction3.CurrentPhaseAbility.m_Ability, targetActors, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
					}
				}
				else
				{
					Log.ErrorWithReport("Unable to undo item override.  No valid ability in phase.");
				}
			}
			else
			{
				Log.ErrorWithReport("Invalid phase for item");
			}
		}
		else if (item.YMLData.ItemType != CItem.EItemType.Vars)
		{
			Log.ErrorWithReport("Invalid Item Type");
		}
	}

	private void ToggleItemAbility(CItem item)
	{
		if (item.YMLData.ItemType != CItem.EItemType.Ability)
		{
			return;
		}
		if (item.SlotState == CItem.EItemSlotState.Selected)
		{
			DeselectItem(item);
			foreach (CItem item2 in AllItemsAndPets.Where((CItem x) => x.SlotState == CItem.EItemSlotState.Selected && x.GetVar("ApplyAbilitiesToItemOwner").GetBool()).ToList())
			{
				ToggleItem(item2);
			}
			PhaseManager.EndItemAbilities();
		}
		else if (item.SlotState != CItem.EItemSlotState.Locked)
		{
			SelectItem(item);
			PhaseManager.StartItemAbilities(item);
		}
	}

	private void DeselectItemAbility(CItem item)
	{
		if (item.GetVar("ApplyAbilitiesToItemOwner").GetBool() && item.SlotState == CItem.EItemSlotState.Selected)
		{
			item.SlotState = CItem.EItemSlotState.TempLocked;
			CancelActiveBonusesOnRefreshingItems(item);
			item.SlotState = CItem.EItemSlotState.Useable;
		}
		else if (PhaseManager.PreviousPhase == null)
		{
			DeselectItem(item);
			if (PhaseManager.CurrentPhase is CPhaseAction)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).UnstackItemAbilities(item);
			}
			else
			{
				Log.ErrorWithReport("Invalid phase for item");
			}
		}
		else
		{
			ToggleItemAbility(item);
		}
	}

	private void CheckForItemAbilityOverridesAndSubAbilitiesToDeselect()
	{
		List<CItem> list = SelectedItems.FindAll((CItem a) => a.YMLData.ItemType == CItem.EItemType.Override);
		if (list.Count <= 0)
		{
			return;
		}
		CPhaseAction cPhaseAction = PhaseManager.CurrentPhase as CPhaseAction;
		if (cPhaseAction.CurrentPhaseAbility?.m_Ability == null)
		{
			return;
		}
		foreach (CItem item in list)
		{
			if (UndoOverridesAndSubAbilities(item, cPhaseAction))
			{
				DeselectItem(item);
			}
		}
	}

	private bool UndoOverridesAndSubAbilities(CItem item, CPhaseAction phaseAction)
	{
		if (phaseAction == null)
		{
			return false;
		}
		if (item.YMLData.ItemType != CItem.EItemType.Override)
		{
			return false;
		}
		bool result = false;
		if (item.YMLData.Trigger == CItem.EItemTrigger.EntireAction)
		{
			foreach (CAbilityOverride @override in item.YMLData.Data.Overrides)
			{
				if (phaseAction.CurrentPhaseAbility.m_Ability.FindOverride(@override) != null)
				{
					result = true;
					phaseAction.CurrentPhaseAbility.m_Ability.UndoOverride(@override, perform: false, item);
				}
				foreach (CPhaseAction.CPhaseAbility item2 in phaseAction.RemainingPhaseAbilities.Where((CPhaseAction.CPhaseAbility w) => !w.ItemID.HasValue))
				{
					if (item2.m_Ability.FindOverride(@override) != null)
					{
						result = true;
						item2.m_Ability.UndoOverride(@override, perform: false, item);
					}
				}
				if (@override.SubAbilities != null)
				{
					result = true;
					phaseAction.RemainingPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility r) => r.ItemID == item.ID);
				}
			}
		}
		else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleAbility)
		{
			foreach (CAbilityOverride override2 in item.YMLData.Data.Overrides)
			{
				result = true;
				phaseAction.CurrentPhaseAbility.m_Ability.UndoOverride(override2, perform: false, item);
				if (override2.SubAbilities != null)
				{
					phaseAction.RemainingPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility r) => r.ItemID == item.ID);
					phaseAction.CurrentPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility r) => r.ItemID == item.ID);
				}
			}
		}
		else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleTarget)
		{
			foreach (CAbilityOverride override3 in item.YMLData.Data.Overrides)
			{
				result = true;
				phaseAction.CurrentPhaseAbility.m_Ability.UndoOverride(override3, perform: false, item);
				if (override3.SubAbilities != null)
				{
					phaseAction.RemainingPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility r) => r.ItemID == item.ID);
				}
			}
		}
		return result;
	}

	public bool SelectItem(CItem item)
	{
		if (item.SlotState == CItem.EItemSlotState.Selected || item.SlotState == CItem.EItemSlotState.Locked)
		{
			return false;
		}
		item.SlotState = CItem.EItemSlotState.Selected;
		OnItemSelected(item);
		return true;
	}

	public bool HasCurrentItemAbilities(CActor actor, CItem item)
	{
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbilities == null)
		{
			return false;
		}
		return PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.GetAbilityBaseCard(actor) == item);
	}

	public bool HasRemainingItemAbilities(CActor actor, CItem item)
	{
		if (PhaseManager.CurrentPhaseAction?.RemainingPhaseAbilities == null)
		{
			return false;
		}
		return PhaseManager.CurrentPhaseAction.RemainingPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.GetAbilityBaseCard(actor) == item);
	}

	public bool DeselectItem(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected)
		{
			return false;
		}
		item.SlotState = CItem.EItemSlotState.Useable;
		OnItemDeSelected(item);
		if (item.YMLData.ItemType == CItem.EItemType.Ability)
		{
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		}
		return true;
	}

	public void RefreshItems(CItem.EItemSlotState slotStateToRefresh, CItem.EItemSlot itemSlot = CItem.EItemSlot.None, CItem.EUsageType usage = CItem.EUsageType.None)
	{
		CItem[] array = AllItemsAndPets.Where((CItem w) => w.SlotState == slotStateToRefresh && (itemSlot == CItem.EItemSlot.None || w.YMLData.Slot == itemSlot) && (usage == CItem.EUsageType.None || w.YMLData.Usage == usage)).ToArray();
		foreach (CItem cItem in array)
		{
			cItem.SlotState = CItem.EItemSlotState.Equipped;
			OnItemRefreshed(cItem, InventoryActor);
			CancelActiveBonusesOnRefreshingItems(cItem);
		}
	}

	public List<CItem> GetItemsInSlot(CItem.EItemSlot slot)
	{
		return AllItems.Where((CItem x) => x.YMLData.Slot == slot).ToList();
	}

	public bool IsItemUsed(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.Consumed && item.SlotState != CItem.EItemSlotState.Spent)
		{
			return item.SlotState == CItem.EItemSlotState.TempLocked;
		}
		return true;
	}

	public bool IsItemActive(CItem item)
	{
		return item.SlotState == CItem.EItemSlotState.Active;
	}

	public bool IsItemUsedOrActive(CItem item)
	{
		if (!IsItemUsed(item))
		{
			return IsItemActive(item);
		}
		return true;
	}

	public bool IsItemUsable(CItem item)
	{
		if (InventoryActor.IsSpecificItemLocked(item))
		{
			return false;
		}
		if (IsItemUsedOrActive(item))
		{
			return false;
		}
		if (ScenarioManager.CurrentScenarioState.ScenarioSetup.MustHaveUnusedSmallItem && item.YMLData.Slot == CItem.EItemSlot.SmallItem)
		{
			return SmallItems.Any((CItem x) => x != null && x != item && !x.HasBeenUsed);
		}
		return true;
	}

	public void ReactivateItem(CItem item, CActor actor = null, bool force = false)
	{
		if (IsItemUsed(item) | force)
		{
			OnItemRefreshed(item, InventoryActor);
			CancelActiveBonusesOnRefreshingItems(item);
			item.SlotState = CItem.EItemSlotState.Equipped;
			actor?.ActivatePassiveItems(firstLoad: true, null, item);
		}
	}

	public void CancelActiveBonusesOnRefreshingItems(CItem item)
	{
		if (!(InventoryActor.Class is CCharacterClass cCharacterClass))
		{
			return;
		}
		for (int num = cCharacterClass.ActivatedCards.Count - 1; num >= 0; num--)
		{
			CBaseCard cBaseCard = cCharacterClass.ActivatedCards[num];
			if (cBaseCard.IsItemOrPet && cBaseCard.ID.Equals(item.ID))
			{
				List<CActiveBonus> list = item.MultiUseBonuses.ToList();
				if (list.Count > 0)
				{
					for (int num2 = list.Count - 1; num2 >= 0; num2--)
					{
						CClass.CancelActiveBonus(list[num2]);
					}
				}
			}
		}
	}

	public void ClearSelectedItems()
	{
		foreach (CItem item in AllItemsAndPets.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected))
		{
			DeselectItem(item);
		}
	}

	public int GetItemShieldValue()
	{
		return GameState.RecievedDamagedReducedByShieldItems;
	}

	public void HandleUsedItems()
	{
		foreach (CItem item in AllItemsAndPets.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected || w.SlotState == CItem.EItemSlotState.Locked).ToList())
		{
			bool flag = false;
			if (InventoryActor is CPlayerActor cPlayerActor && cPlayerActor.CharacterClass.ActivatedCards.FindAll((CBaseCard x) => x is CItem).SingleOrDefault((CBaseCard s) => s.ID.Equals(item.ID)) is CItem cItem && cItem.MultiUseBonuses.Any())
			{
				flag = true;
			}
			if (flag)
			{
				HandleActiveItemTriggered(item);
			}
			else
			{
				HandleUsedItem(item);
			}
		}
	}

	public void HandleUsedItem(int ItemID, CActiveBonus activeBonus = null)
	{
		CItem cItem = AllItemsAndPets.SingleOrDefault((CItem s) => s.ID == ItemID);
		if (cItem != null)
		{
			HandleUsedItem(cItem, activeBonus);
		}
	}

	public void HandleUsedItems(params CItem.EItemTrigger[] triggers)
	{
		foreach (CItem item in AllItemsAndPets.ToList())
		{
			foreach (CItem.EItemTrigger eItemTrigger in triggers)
			{
				if (item.YMLData.Trigger.HasFlag(eItemTrigger))
				{
					HandleUsedItem(item);
					break;
				}
			}
		}
	}

	public void HandleActiveItemTriggered(CItem item, bool ignoreConsume = false)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Locked && item.SlotState != CItem.EItemSlotState.None)
		{
			return;
		}
		if (item.SlotState != CItem.EItemSlotState.None && !ignoreConsume)
		{
			int num = 0;
			foreach (ElementInfusionBoardManager.EElement consume in item.YMLData.Consumes)
			{
				ElementInfusionBoardManager.EElement eElement = consume;
				if (eElement == ElementInfusionBoardManager.EElement.Any && item.ChosenElement.Count > num)
				{
					eElement = item.ChosenElement[num];
					num++;
				}
				ElementInfusionBoardManager.Consume(eElement, InventoryActor);
			}
		}
		item.SlotState = CItem.EItemSlotState.Active;
		OnItemActive(item);
	}

	public void HandleItemCost(CItem item, CActiveBonus activeBonus = null)
	{
		int num = 0;
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		foreach (ElementInfusionBoardManager.EElement consume in item.YMLData.Consumes)
		{
			ElementInfusionBoardManager.EElement eElement = consume;
			if (eElement == ElementInfusionBoardManager.EElement.Any && item.ChosenElement.Count > num)
			{
				eElement = item.ChosenElement[num];
				num++;
			}
			list.Add(eElement);
			ElementInfusionBoardManager.Consume(eElement, InventoryActor);
		}
		if (list.Count > 0)
		{
			CElementsConsumed_MessageData message = new CElementsConsumed_MessageData(null, InventoryActor)
			{
				m_ChangedElements = list,
				m_Consume = null,
				m_Card = item
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		if (activeBonus != null)
		{
			CAbility ability = activeBonus.Ability;
			if (ability != null && ability.ActiveBonusData?.Consuming.Count > 0)
			{
				foreach (ElementInfusionBoardManager.EElement item2 in activeBonus.Ability.ActiveBonusData.Consuming)
				{
					ElementInfusionBoardManager.EElement element;
					if (item2 == ElementInfusionBoardManager.EElement.Any)
					{
						element = (activeBonus.ToggledElement.HasValue ? activeBonus.ToggledElement.Value : item2);
					}
					else
					{
						element = item2;
					}
					ElementInfusionBoardManager.Consume(element, InventoryActor);
				}
			}
		}
		if (item.YMLData.MoraleCost > 0)
		{
			ScenarioManager.CurrentScenarioState.MoraleSpent += item.YMLData.MoraleCost;
		}
	}

	public void HandleUsedItem(CItem item, CActiveBonus activeBonus = null)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Locked && item.SlotState != CItem.EItemSlotState.Active && item.SlotState != CItem.EItemSlotState.Spent)
		{
			return;
		}
		bool flag = ((activeBonus != null) ? (!activeBonus.HasTracker) : (item.YMLData.Data.Abilities?.SingleOrDefault((CAbility x) =>
		{
			AbilityData.ActiveBonusData activeBonusData = x.ActiveBonusData;
			return activeBonusData != null && activeBonusData.Tracker?.Count > 0;
		}) == null));
		bool hideLog = activeBonus?.Ability?.ActiveBonusData != null && activeBonus.Ability.ActiveBonusData.Hidden;
		if (item.YMLData.Usage == CItem.EUsageType.Spent && item.SlotState != CItem.EItemSlotState.Spent)
		{
			item.SlotState = CItem.EItemSlotState.Spent;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemSpent(item);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
		}
		else if (item.YMLData.Usage == CItem.EUsageType.Consumed && item.SlotState != CItem.EItemSlotState.Consumed)
		{
			item.SlotState = CItem.EItemSlotState.Consumed;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemConsumed(item);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
		}
		else if (item.YMLData.Usage == CItem.EUsageType.Unrestricted)
		{
			item.SlotState = CItem.EItemSlotState.Equipped;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemUnrestrictedUsed(item, hideLog);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
		}
		else
		{
			if (item.YMLData.Usage != CItem.EUsageType.Flip || item.SlotState == CItem.EItemSlotState.Consumed)
			{
				return;
			}
			if (item.YMLData.FlipEndOfTurn)
			{
				item.SlotState = CItem.EItemSlotState.TempLocked;
				item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
				OnItemFlipped(item);
				if (flag)
				{
					InventoryActor.UsedItem(item, firstTimeUse: true);
				}
				return;
			}
			item.SlotState = CItem.EItemSlotState.Equipped;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemFlipped(item);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
			FlipItem(item);
		}
	}

	public void LockInSelectedItemsAndResetUnselected()
	{
		foreach (CItem item in AllItemsAndPets.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected))
		{
			item.SlotState = CItem.EItemSlotState.Locked;
			OnItemLocked(item);
		}
		foreach (CItem item2 in AllItemsAndPets.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Useable))
		{
			item2.SlotState = CItem.EItemSlotState.Equipped;
			OnItemNoLongerUsable(item2);
		}
	}

	public void OnAbilityEnd()
	{
		foreach (CItem item in AllItemsAndPets.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Locked))
		{
			item.SlotState = CItem.EItemSlotState.Selected;
		}
	}

	public void HighlightUsableItems(CAbility ability, params CItem.EItemTrigger[] acceptedTriggers)
	{
		HighlightUsableItems(ability, null, null, acceptedTriggers);
	}

	public void HighlightUsableItems(CAbility ability, CActor targetActor, params CItem.EItemTrigger[] acceptedTriggers)
	{
		HighlightUsableItems(ability, (targetActor == null) ? null : new List<CActor> { targetActor }, null, acceptedTriggers);
	}

	public void HighlightUsableItems(CVarsContext context, CActor targetActor, params CItem.EItemTrigger[] acceptedTriggers)
	{
		HighlightUsableItems(null, (targetActor == null) ? null : new List<CActor> { targetActor }, context, acceptedTriggers);
	}

	public void HighlightUsableItems(CAbility ability, List<CActor> targetActors, params CItem.EItemTrigger[] acceptedTriggers)
	{
		HighlightUsableItems(ability, targetActors, null, acceptedTriggers);
	}

	public void HighlightUsableItems(CAbility ability, List<CActor> targetActors, CVarsContext context, params CItem.EItemTrigger[] acceptedTriggers)
	{
		IEnumerable<CItem> enumerable = AllItemsAndPets.Where((CItem w) => w.SlotState != CItem.EItemSlotState.Selected && w.SlotState != CItem.EItemSlotState.Locked);
		if (acceptedTriggers.All((CItem.EItemTrigger x) => x.ToString().Contains("Turn")) && (InventoryActor.IsTakingExtraTurn || GameState.IsPerformingCardOutOfPhase))
		{
			foreach (CItem item in enumerable)
			{
				if (item.SlotState == CItem.EItemSlotState.Useable)
				{
					item.SlotState = CItem.EItemSlotState.Equipped;
					OnItemNoLongerUsable(item);
				}
			}
			return;
		}
		foreach (CItem item2 in enumerable)
		{
			if (IsItemUsable(item2) && acceptedTriggers.Contains(item2.YMLData.Trigger) && CheckItemConsumes(item2) && CheckConditions(GameState.InternalCurrentActor, item2))
			{
				if (targetActors != null && targetActors.Count > 0)
				{
					foreach (CActor targetActor in targetActors)
					{
						if (((ability == null && item2.YMLData.Data.CompareAbility == null) || (ability != null && (item2.YMLData.Data.CompareAbility == null || item2.YMLData.Data.CompareAbility.CompareAbility(ability, targetActor)))) && (item2.YMLData.Data.ItemRequirements == null || item2.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(targetActor, ability, context, item2)))
						{
							item2.SlotState = CItem.EItemSlotState.Useable;
							OnItemUsable(item2);
							break;
						}
						if (item2.SlotState == CItem.EItemSlotState.Useable)
						{
							item2.SlotState = CItem.EItemSlotState.Equipped;
							OnItemNoLongerUsable(item2);
						}
					}
				}
				else if ((ability == null || item2.YMLData.Data.CompareAbility == null || item2.YMLData.Data.CompareAbility.CompareAbility(ability)) && (item2.YMLData.Data.ItemRequirements == null || item2.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(InventoryActor, ability, context, item2)))
				{
					item2.SlotState = CItem.EItemSlotState.Useable;
					OnItemUsable(item2);
				}
				else if (item2.SlotState == CItem.EItemSlotState.Useable)
				{
					item2.SlotState = CItem.EItemSlotState.Equipped;
					OnItemNoLongerUsable(item2);
				}
			}
			else if (item2.SlotState == CItem.EItemSlotState.Useable)
			{
				item2.SlotState = CItem.EItemSlotState.Equipped;
				OnItemNoLongerUsable(item2);
			}
		}
	}

	public void Undo()
	{
		foreach (CItem item in AllItemsAndPets.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected))
		{
			item.SlotState = CItem.EItemSlotState.Useable;
			OnItemDeSelected(item);
		}
	}

	public void FlipItem(CItem item)
	{
		RemoveItem(item);
		int flipItemID = int.Parse(item.YMLData.FlipItemID);
		CItem getItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData x) => x.ID == flipItemID).GetItem;
		InventoryActor.Inventory.AddItem(getItem);
	}

	private bool CheckItemConsumes(CItem item)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements(InventoryActor);
		if (item.YMLData.Consumes.Contains(ElementInfusionBoardManager.EElement.Any) && availableElements.Count > 0)
		{
			return true;
		}
		foreach (ElementInfusionBoardManager.EElement consume in item.YMLData.Consumes)
		{
			if (!availableElements.Contains(consume))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckConditions(CActor actor, CItem item)
	{
		if (item.YMLData.Data.CompareConditions == null || item.YMLData.Data.CompareConditions.Count == 0)
		{
			return true;
		}
		foreach (string compareCondition in item.YMLData.Data.CompareConditions)
		{
			if (compareCondition == "AnyNegative")
			{
				if (actor.Tokens.GetAllNegativeConditions().Count > 0)
				{
					return true;
				}
				return false;
			}
			if (compareCondition == "AnyPositive")
			{
				if (actor.Tokens.GetAllPositiveConditions().Count > 0)
				{
					return true;
				}
				return false;
			}
			if (Enum.TryParse<CCondition.ENegativeCondition>(compareCondition, out var result))
			{
				if (actor.Tokens.HasKey(result))
				{
					return true;
				}
				return false;
			}
			if (Enum.TryParse<CCondition.EPositiveCondition>(compareCondition, out var result2))
			{
				if (actor.Tokens.HasKey(result2))
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	public void UseItem(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.TempLocked)
		{
			if (item.YMLData.Usage == CItem.EUsageType.Spent && item.SlotState != CItem.EItemSlotState.Spent)
			{
				item.SlotState = CItem.EItemSlotState.Spent;
				OnItemSpent(item);
			}
			else if (item.YMLData.Usage == CItem.EUsageType.Consumed && item.SlotState != CItem.EItemSlotState.Consumed)
			{
				item.SlotState = CItem.EItemSlotState.Consumed;
				OnItemConsumed(item);
			}
		}
	}

	public void ConsumeItem(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.TempLocked)
		{
			item.SlotState = CItem.EItemSlotState.Consumed;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemConsumed(item);
		}
	}

	public static void RemoveBeforeScenarioItems(List<CItem> items)
	{
		items.RemoveAll((CItem x) => x.ID == 886 || x.ID == 902 || x.ID == 962 || x.ID == 1208 || x.ID == 1294 || x.ID == 1295);
	}
}
