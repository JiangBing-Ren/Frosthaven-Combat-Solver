using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeKeepWatch : CChallenge
{
	public override uint ID => 1494u;

	public override string Name => "keep_watch";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnAttack;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor is CPlayerActor && triggerParams.actorBeingAttacked is CEnemyActor cEnemyActor && triggerParams.attackAbility != null)
		{
			if (triggerParams.attackAbility.MiscAbilityData == null)
			{
				triggerParams.attackAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			if (cEnemyActor.PlayedAtLeastOnceThisRound)
			{
				triggerParams.attackAbility.MiscAbilityData.AttackHasAdvantage = true;
			}
			else
			{
				triggerParams.attackAbility.MiscAbilityData.AttackHasDisadvantage = true;
			}
		}
	}
}
