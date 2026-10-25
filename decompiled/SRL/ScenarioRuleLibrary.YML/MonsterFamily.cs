using System.Collections.Generic;

namespace ScenarioRuleLibrary.YML;

public class MonsterFamily
{
	public string FamilyID { get; private set; }

	public Dictionary<string, List<string>> Descriptions { get; private set; }

	public List<MonsterSpawnRates> DefaultMonsterGroup { get; private set; }

	public MonsterFamily(string familyID, Dictionary<string, List<string>> descriptions, List<MonsterSpawnRates> defaultMonsterGroup)
	{
		FamilyID = familyID;
		Descriptions = descriptions;
		DefaultMonsterGroup = defaultMonsterGroup;
	}
}
