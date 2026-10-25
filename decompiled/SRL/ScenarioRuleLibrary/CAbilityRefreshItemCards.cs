using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityRefreshItemCards : CAbilityTargeting
{
	public static int STRENGTH_ALL = -1;

	[JsonProperty]
	public List<CItem.EItemSlot> SlotsToRefresh;

	[JsonProperty]
	public List<CItem.EItemSlotState> SlotStatesToRefresh;

	[JsonProperty]
	public List<CItem.EUsageType> SlotUsagesToRefresh;

	public CAbilityRefreshItemCards(List<CItem.EItemSlot> slotsToRefresh, List<CItem.EItemSlotState> slotStatesToRefresh, List<CItem.EUsageType> slotUsagesToRefresh)
		: base(EAbilityType.RefreshItemCards)
	{
		SlotsToRefresh = slotsToRefresh;
		SlotStatesToRefresh = slotStatesToRefresh;
		SlotUsagesToRefresh = slotUsagesToRefresh;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (MiscVars == null)
		{
			MiscVars = new List<(string, TValue)>();
		}
		if (!MiscVars.Any(((string, TValue) x) => x.Item1 == "HelpTextWhenCancelled"))
		{
			MiscVars.Add(("HelpTextWhenCancelled", new TValue("FH/GUI_CANCEL_REFRESH_ITEMS_NO_VALID_TARGET")));
		}
		if (!MiscVars.Any(((string, TValue) x) => x.Item1 == "DontCancelIfNoValid"))
		{
			MiscVars.Add(("DontCancelIfNoValid", new TValue(val: true)));
		}
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
		if (!actorsAppliedTo.Any((CActor x) => HasValidItems(x)))
		{
			SetCancelAbilityFlag(cancel: true);
			return true;
		}
		CActorIsSelectingItemCards_MessageData message = new CActorIsSelectingItemCards_MessageData(AnimOverload, actorApplying)
		{
			m_ActorsRefreshed = actorsAppliedTo,
			m_ItemSelectionAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	private List<CItem> ValidItems(CActor actor)
	{
		List<object> list = VarsContext.GetVar("SpecificItems").GetObject<List<object>>(VarsContext);
		List<CItem> list2 = null;
		if (list != null)
		{
			list2 = new List<CItem>();
			foreach (object item2 in list)
			{
				if (item2 is CItem item)
				{
					list2.Add(item);
				}
			}
		}
		else
		{
			list2 = actor.Inventory.AllItems.Where((CItem it) => it.YMLData.PermanentlyConsumed != true && (SlotStatesToRefresh == null || SlotStatesToRefresh.Count == 0 || SlotStatesToRefresh.Contains(it.SlotState)) && (SlotsToRefresh == null || SlotsToRefresh.Count == 0 || SlotsToRefresh.Contains(it.YMLData.Slot))).ToList();
		}
		return list2;
	}

	private bool HasValidItems(CActor actor)
	{
		List<CItem> specificItems = ValidItems(actor);
		return HasValidItems(specificItems, actor);
	}

	private bool HasValidItems(List<CItem> specificItems, CActor actor)
	{
		if (actor.Type == CActor.EType.Player)
		{
			if (specificItems != null)
			{
				return specificItems.Count > 0;
			}
			return false;
		}
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			List<CItem> specificItems = ValidItems(actor);
			if (!HasValidItems(specificItems, actor))
			{
				return false;
			}
			CSelectRefreshOrConsumeItems_MessageData cSelectRefreshOrConsumeItems_MessageData = new CSelectRefreshOrConsumeItems_MessageData(TargetingActor, actor);
			cSelectRefreshOrConsumeItems_MessageData.m_Ability = this;
			cSelectRefreshOrConsumeItems_MessageData.m_SlotsToChooseFrom = SlotsToRefresh;
			cSelectRefreshOrConsumeItems_MessageData.m_SlotStatesToChooseFrom = SlotStatesToRefresh;
			cSelectRefreshOrConsumeItems_MessageData.m_SpecificItems = specificItems;
			ScenarioRuleClient.MessageHandler(cSelectRefreshOrConsumeItems_MessageData);
			if (Strength == STRENGTH_ALL)
			{
				actor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
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
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		STRENGTH_ALL = -1;
	}
}
