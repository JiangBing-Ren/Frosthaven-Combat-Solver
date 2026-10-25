namespace ScenarioRuleLibrary;

public interface IGetVar
{
	TValue GetVar(string key, CVarsContext context = null, bool asValue = true);
}
