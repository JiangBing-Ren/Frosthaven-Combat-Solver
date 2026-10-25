namespace Achievements;

public class IAmJustGettingStartedAchievement : CounterAchievement
{
	public IAmJustGettingStartedAchievement()
	{
		ID = "IAmJustGettingStarted";
		Name = "I Am Just Getting Started";
		Max = 10;
	}

	protected override int CheckProgress(AchievementContext context)
	{
		return context.GlobalStats.MonstersKilled.Value;
	}
}
