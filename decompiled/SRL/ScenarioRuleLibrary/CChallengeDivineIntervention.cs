using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CChallengeDivineIntervention : CChallenge
{
	public override uint ID => 1486u;

	public override string Name => "divine_intervention";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnStartScenario;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		for (int i = 0; i < 3; i++)
		{
			MonsterClassManager.AddBlessCardToEnemies();
		}
	}
}
