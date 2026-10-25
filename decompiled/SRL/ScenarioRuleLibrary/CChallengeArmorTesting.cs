using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CChallengeArmorTesting : CChallenge
{
	public override uint ID => 1482u;

	public override string Name => "armor_testing";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnSufferDamageFromAttackWithNoShield;
	}

	protected override void Perform(TriggerParams triggerParams)
	{
		if (triggerParams.actorBeingAttacked is CPlayerActor)
		{
			CAbilityAttack attackAbility = triggerParams.attackAbility;
			CAbility cAbility = CAbilityPush.CreateDefaultPush(2, 1, 1, isMonster: true);
			cAbility.IsInlineSubAbility = true;
			attackAbility.AddSubAbility(cAbility, isInlineSubAbility: true);
			cAbility.IsSubAbility = true;
		}
	}
}
