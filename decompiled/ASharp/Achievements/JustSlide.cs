using System.Linq;
using SharedLibrary.Achievments;

namespace Achievements;

public class JustSlide : CounterAchievement
{
	public JustSlide()
	{
		ID = "ACH30_JustSlide";
		Name = "ACH30_JustSlide";
		Max = 500;
	}

	protected override int CheckProgress(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignStats.TilesMovedByCharacters.Stats.Count > 0)
		{
			return campaignAchievementContext.CampaignStats.TilesMovedByCharacters.Stats.Max(((string, CounterAchievementStat) x) => x.Item2.Value);
		}
		return Progress;
	}
}
