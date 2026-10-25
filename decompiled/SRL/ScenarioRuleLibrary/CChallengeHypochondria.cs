using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeHypochondria : CChallenge
{
	public override uint ID => 1487u;

	public override string Name => "hypochondria";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnGainNegativeCondition;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor is CPlayerActor cPlayerActor && triggerParams.appliedCondition != CCondition.ENegativeCondition.Wound)
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Wound, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player), isMonster: false, isTargetedAbility: true, 0, 1, 1, 0, CAbility.EAbilityFlag.None, 1, EConditionDecTrigger.Never);
			cAbility.IsScenarioModifierAbility = true;
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.SkipAnim = true;
			cAbility.SetMiscVar("IsMapCondition", true);
			cAbility.Targeting = CAbility.EAbilityTargeting.Range;
			cAbility.Range = 1;
			cAbility.NumberTargets = 1;
			cAbility.Start(cPlayerActor, cPlayerActor);
			cAbility.ApplyToActor(cPlayerActor);
		}
	}
}
