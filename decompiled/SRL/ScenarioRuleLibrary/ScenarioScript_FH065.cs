using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH065 : ScenarioScript
{
	protected override bool Initialize()
	{
		CScenarioModifier scenarioModifier = GetScenarioModifier("AddPush");
		if (scenarioModifier != null)
		{
			scenarioModifier.CustomLocKeyArgs = new List<object> { (ScenarioManager.CurrentScenarioState.Level < 4) ? 1 : 2 };
		}
		return base.Initialize();
	}
}
