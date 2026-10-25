using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityImprovedShortRest : CAbilityTargeting
{
	public bool m_CanExcludeCards;

	public CAbilityImprovedShortRest(bool canExcludeCards = false)
		: base(EAbilityType.ImprovedShortRest)
	{
		m_CanExcludeCards = canExcludeCards;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (actor is CPlayerActor cPlayerActor)
			{
				cPlayerActor.CharacterClass.ImprovedShortRest = true;
				cPlayerActor.CharacterClass.ImprovedShortRestDrawCount = (uint)Strength;
				cPlayerActor.CharacterClass.ImprovedShortRestCanExcludeCards = m_CanExcludeCards;
			}
			else
			{
				Log.ErrorWithReport("Trying to apply Improved Short Rest to a non player actor");
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
