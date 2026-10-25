namespace ScenarioRuleLibrary.YML;

public class HeroClassStats
{
	public int Melee;

	public int Ranged;

	public int Mobility;

	public int Support;

	public int Defense;

	public int Control;

	public HeroClassStats Clone()
	{
		return (HeroClassStats)MemberwiseClone();
	}
}
