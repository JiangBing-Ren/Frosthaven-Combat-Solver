using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityShuffleModifierDeck : CAbilityTargeting
{
	public CAbilityShuffleModifierDeck()
		: base(EAbilityType.ShuffleModifierDeck)
	{
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
			if (actor.Class is CCharacterClass || actor.Class is CHeroSummonClass)
			{
				CHeroSummonActor cHeroSummonActor = ((actor is CHeroSummonActor) ? (actor as CHeroSummonActor) : null);
				((cHeroSummonActor != null) ? cHeroSummonActor.Summoner : ((CPlayerActor)actor)).CharacterClass.CheckAttackModifierCardShuffle(force: true);
			}
			else if (actor is CEnemyActor forceShuffleForActor)
			{
				MonsterClassManager.CheckAttackModifierCardShuffle(forceShuffleForActor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
