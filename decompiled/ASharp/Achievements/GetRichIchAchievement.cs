namespace Achievements;

public class GetRichIchAchievement : EventAchievement
{
	public GetRichIchAchievement()
	{
		Name = "Get Rich Ich";
		ID = "GetRichIch";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext campaignAchievementContext)
		{
			return campaignAchievementContext.CampaignStats.GoldCollected > 1000;
		}
		return false;
	}
}
