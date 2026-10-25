using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH001 : ScenarioScript
{
	protected override List<object> GetConclusionParametersInternal()
	{
		int num = 0;
		foreach (EnemyState allyMonster in ScenarioManager.CurrentScenarioState.AllyMonsters)
		{
			Trace(allyMonster.ClassID, null, 12);
			Trace("state is dead: " + allyMonster.IsDead, null, 13);
			Trace("actor is dead: " + allyMonster.Actor?.IsDead, null, 14);
			Trace("enemy is dead: " + allyMonster.Enemy?.IsDead, null, 15);
			if (IsAlive(allyMonster))
			{
				Trace("Count as alive...", null, 17);
				num++;
			}
		}
		Trace($"num alive allies: {num}", null, 21);
		return new List<object> { num };
	}
}
