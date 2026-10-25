namespace ScenarioRuleLibrary;

public interface IDumpState
{
	string ToStateDumpString(StateDump dump);

	void DumpMembers(StateDump dump)
	{
		dump.DumpMembersUsingReflection(this);
	}
}
