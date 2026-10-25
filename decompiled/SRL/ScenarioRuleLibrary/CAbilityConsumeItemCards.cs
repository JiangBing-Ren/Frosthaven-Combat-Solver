using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityConsumeItemCards : CAbilityTargeting
{
	public static int STRENGTH_ALL = -1;

	[JsonProperty]
	public List<CItem.EItemSlot> SlotsToConsume;

	[JsonProperty]
	public List<CItem.EItemSlotState> SlotStatesToConsume;

	[JsonProperty]
	public List<CItem.EUsageType> SlotUsagesToConsume;

	public CAbilityConsumeItemCards(List<CItem.EItemSlot> slotsToConsume, List<CItem.EItemSlotState> slotStatesToConsume, List<CItem.EUsageType> slotUsagesToConsume)
		: base(EAbilityType.ConsumeItemCards)
	{
		SlotsToConsume = slotsToConsume;
		SlotStatesToConsume = slotStatesToConsume;
		SlotUsagesToConsume = slotUsagesToConsume;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (Strength != STRENGTH_ALL)
		{
			m_OneTargetAtATime = true;
		}
		base.Start(targetActor, filterActor, controllingActor);
		SetHasStarted(val: true);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsSelectingItemCards_MessageData message = new CActorIsSelectingItemCards_MessageData(AnimOverload, actorApplying)
		{
			m_ActorsRefreshed = actorsAppliedTo,
			m_ItemSelectionAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			List<CItem> list = null;
			if (Vars != null)
			{
				List<object> list2 = Vars.SingleOrDefault(((string, TValue) x) => x.Item1 == "SpecificItems").Item2.Calc(VarsContext).GetObject<List<object>>();
				if (list2 != null && list2.Count > 0)
				{
					list = new List<CItem>();
					foreach (object item2 in list2)
					{
						if (item2 is CItem item)
						{
							list.Add(item);
						}
					}
				}
			}
			List<CItem> list3 = null;
			list3 = ((list == null || list.Count <= 0) ? actor.Inventory.AllItems.Where((CItem it) => (SlotStatesToConsume == null || SlotStatesToConsume.Count == 0 || SlotStatesToConsume.Contains(it.SlotState)) && (SlotsToConsume == null || SlotsToConsume.Count == 0 || SlotsToConsume.Contains(it.YMLData.Slot))).ToList() : list);
			if (list3 != null)
			{
				CInventory.RemoveBeforeScenarioItems(list3);
			}
			if (list3 != null && list3.Count > 0)
			{
				CSelectRefreshOrConsumeItems_MessageData cSelectRefreshOrConsumeItems_MessageData = new CSelectRefreshOrConsumeItems_MessageData(TargetingActor, actor);
				cSelectRefreshOrConsumeItems_MessageData.m_Ability = this;
				cSelectRefreshOrConsumeItems_MessageData.m_SlotsToChooseFrom = SlotsToConsume;
				cSelectRefreshOrConsumeItems_MessageData.m_SlotStatesToChooseFrom = SlotStatesToConsume;
				cSelectRefreshOrConsumeItems_MessageData.m_SpecificItems = list;
				ScenarioRuleClient.MessageHandler(cSelectRefreshOrConsumeItems_MessageData);
				if (Strength == CAbilityRefreshItemCards.STRENGTH_ALL)
				{
					actor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
				}
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		STRENGTH_ALL = -1;
	}
}
