namespace ScenarioRuleLibrary.YML;

public static class PerkExtensions
{
	public static bool HasFlags(this EPerkFlag mask, EPerkFlag flagsToCheck)
	{
		return (mask & flagsToCheck) == flagsToCheck;
	}
}
