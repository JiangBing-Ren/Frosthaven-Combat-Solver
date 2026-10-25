namespace ScenarioRuleLibrary;

public interface IGetItemByIndex
{
	TValue GetItemByIndex(TValue key, CVarsContext context = null, bool asValue = true);
}
