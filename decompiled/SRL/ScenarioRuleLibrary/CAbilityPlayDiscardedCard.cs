using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityPlayDiscardedCard : CAbility
{
	[JsonProperty]
	public List<EAbilityType> RecoverCardsWithAbilityOfTypeFilter;

	[JsonProperty]
	public List<int> SpecificCardsFilter;

	[JsonProperty]
	public int DamageReduction;

	[JsonProperty]
	public bool UseLostCards;

	[JsonProperty]
	public bool UseParentTargeting;

	[JsonProperty]
	public ECardAction CardAction;

	public CAbilityPlayDiscardedCard(List<EAbilityType> recoverCardsWithAbilityOfTypeFilter, List<int> specificCardsFilter, int damageReduction, bool useLostCards, bool useParentTargeting, ECardAction cardAction)
	{
		RecoverCardsWithAbilityOfTypeFilter = recoverCardsWithAbilityOfTypeFilter;
		SpecificCardsFilter = specificCardsFilter;
		DamageReduction = damageReduction;
		UseLostCards = useLostCards;
		UseParentTargeting = useParentTargeting;
		CardAction = cardAction;
	}

	public override bool Perform()
	{
		base.Perform();
		if (ShowCancelledHelpText())
		{
			return true;
		}
		CPlayerActor cPlayerActor = (CPlayerActor)TargetingActor;
		if (cPlayerActor != null)
		{
			AbilityHasHappened = true;
			List<CAbilityCard> list = (UseLostCards ? cPlayerActor.CharacterClass.LostAbilityCards : cPlayerActor.CharacterClass.DiscardedAbilityCards).ToList();
			if (SpecificCardsFilter != null && SpecificCardsFilter.Count > 0)
			{
				list = list.Where((CAbilityCard card) => SpecificCardsFilter.Contains(card.ID)).ToList();
			}
			if (RecoverCardsWithAbilityOfTypeFilter != null && RecoverCardsWithAbilityOfTypeFilter.Count > 0)
			{
				list = list.Where((CAbilityCard card) => RecoverCardsWithAbilityOfTypeFilter.Any(card.HasAbilityOfType)).ToList();
			}
			if (list.Count > 0)
			{
				CSelectReplayCard_MessageData cSelectReplayCard_MessageData = new CSelectReplayCard_MessageData(AnimOverload, TargetingActor);
				cSelectReplayCard_MessageData.m_ActorReplayingCard = TargetingActor;
				cSelectReplayCard_MessageData.m_Ability = this;
				cSelectReplayCard_MessageData.m_ReplayAbilityType = ((RecoverCardsWithAbilityOfTypeFilter.Count > 0) ? RecoverCardsWithAbilityOfTypeFilter[0] : EAbilityType.None);
				cSelectReplayCard_MessageData.m_ValidCards = list.Select((CAbilityCard c) => c.ID).ToList();
				cSelectReplayCard_MessageData.m_DamageReduction = DamageReduction;
				cSelectReplayCard_MessageData.m_useLostCards = UseLostCards;
				cSelectReplayCard_MessageData.m_UseParentTargeting = UseParentTargeting;
				cSelectReplayCard_MessageData.m_CardAction = CardAction;
				ScenarioRuleClient.MessageHandler(cSelectReplayCard_MessageData);
				return true;
			}
		}
		SetCancelAbilityFlag(cancel: true);
		PhaseManager.NextStep();
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		return AbilityHasHappened;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
