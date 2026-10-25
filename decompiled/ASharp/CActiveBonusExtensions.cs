using ScenarioRuleLibrary;

public static class CActiveBonusExtensions
{
	public static bool HasConsumeElements(this CActiveBonus bonus)
	{
		return bonus.Ability.ActiveBonusData.Consuming.Count > 0;
	}
}
