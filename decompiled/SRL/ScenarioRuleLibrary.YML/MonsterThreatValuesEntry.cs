namespace ScenarioRuleLibrary.YML;

public class MonsterThreatValuesEntry
{
	public string ClassID { get; private set; }

	public int Normal { get; private set; }

	public int Elite { get; private set; }

	public MonsterThreatValuesEntry(string classID, int normal, int elite)
	{
		ClassID = classID;
		Normal = normal;
		Elite = elite;
	}
}
