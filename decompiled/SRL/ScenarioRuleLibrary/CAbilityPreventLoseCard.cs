using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityPreventLoseCard : CAbilityTargeting
{
	public CAbilityPreventLoseCard(EAbilityType abilityType)
		: base(abilityType)
	{
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, TargetingActor);
			}
			else if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find base ability card for ability " + Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
			CActorIsCastingPreventLoseCard message = new CActorIsCastingPreventLoseCard(AnimOverload, actor)
			{
				m_PreventLoseCardAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		return true;
	}

	public static bool TryPreventLose(CActor actor, CActiveBonus.EActiveBonusBehaviourType behaviour)
	{
		List<CActiveBonus> list = actor.FindApplicableActiveBonuses(EAbilityType.PreventLoseCard, behaviour);
		if (list != null && list.Count > 0)
		{
			CActiveBonus cActiveBonus = list[0];
			cActiveBonus.UpdateXPTracker();
			cActiveBonus.FinishIfNoRemaining();
			return true;
		}
		return false;
	}
}
