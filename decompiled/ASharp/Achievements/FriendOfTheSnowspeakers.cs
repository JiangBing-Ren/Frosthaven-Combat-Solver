using System.Linq;
using FHRL;

namespace Achievements;

public class FriendOfTheSnowspeakers : EventAchievement
{
	public FriendOfTheSnowspeakers()
	{
		ID = "ACH7_FriendoftheSnowspeakers";
		Name = "ACH7_FriendoftheSnowspeakers";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "FriendOfTheSnowspeakers"))
		{
			return true;
		}
		return false;
	}
}
