namespace ScenarioRuleLibrary;

public interface IStateDumper
{
	string ToStateDumpString(StateDump dump, object obj);

	void DumpMembers(StateDump dump, object obj);
}
