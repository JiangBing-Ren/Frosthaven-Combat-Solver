using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeImitation : CChallenge
{
	public override uint ID => 1503u;

	public override string Name => "imitation";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnStartScenario;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams == null || triggerParams.trigger != Trigger.OnStartScenario)
		{
			return;
		}
		List<PlayerState> list = ScenarioManager.CurrentScenarioState.Players.ToList();
		list.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
		foreach (PlayerState item in list)
		{
			List<AttackModifierYMLData> list2 = item.Actor.CharacterClass.AttackModifierCardsPool.ToList();
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				AttackModifierYMLData attackModifierYMLData = list2[num];
				bool flag = !attackModifierYMLData.Name.Contains("Basic", StringComparison.InvariantCultureIgnoreCase);
				if (flag)
				{
					foreach (CAbility ability in attackModifierYMLData.Abilities)
					{
						if (ability.AbilityType == CAbility.EAbilityType.Choose || ability.AbilityType == CAbility.EAbilityType.ModeTransfer || ability.AbilityType == CAbility.EAbilityType.MoveActiveBonusTracker || ability.AbilityType == CAbility.EAbilityType.Kill || ability.Targeting == CAbility.EAbilityTargeting.Custom)
						{
							flag = false;
						}
						if (!flag)
						{
							break;
						}
					}
				}
				if (!flag)
				{
					list2.RemoveAt(num);
				}
			}
			list2.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
			List<string> list3 = (from x in list2.Take(5)
				select x.Name).ToList();
			if (list3.Count >= 5)
			{
				MonsterClassManager.AddAdditionalModifierCardsToEnemies(list3);
				break;
			}
		}
	}
}
