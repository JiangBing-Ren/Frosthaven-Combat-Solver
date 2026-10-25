using System.Collections.Generic;

namespace ScenarioRuleLibrary.YML;

public class MonsterGroup
{
	public string GroupID { get; private set; }

	public List<MonsterSpawnRates> Monsters { get; private set; }

	public MonsterGroup(string groupID, List<MonsterSpawnRates> monsters)
	{
		GroupID = groupID;
		Monsters = monsters;
	}
}
