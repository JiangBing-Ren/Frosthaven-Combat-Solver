using System.Linq;
using SharedLibrary.Achievments;

namespace Achievements;

public class WhiteDeath : CounterAchievement
{
	public WhiteDeath()
	{
		ID = "ACH29_WhiteDeath";
		Name = "ACH29_WhiteDeath";
		Max = 1000;
	}

	protected override int CheckProgress(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignStats.DamageDealtByCharacters.Stats.Count > 0)
		{
			return campaignAchievementContext.CampaignStats.DamageDealtByCharacters.Stats.Max(((string, CounterAchievementStat) x) => x.Item2.Value);
		}
		return Progress;
	}
}
