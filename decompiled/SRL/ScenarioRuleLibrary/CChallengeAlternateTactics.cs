using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CChallengeAlternateTactics : CChallenge
{
	public override uint ID => 1491u;

	public override string Name => "alternate_tactics";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.ReverseInitiative;
	}
}
