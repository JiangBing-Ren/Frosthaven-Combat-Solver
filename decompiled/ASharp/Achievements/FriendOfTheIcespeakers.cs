using System.Linq;
using FHRL;

namespace Achievements;

public class FriendOfTheIcespeakers : EventAchievement
{
	public FriendOfTheIcespeakers()
	{
		ID = "ACH6_FriendoftheIcespeakers";
		Name = "ACH6_FriendoftheIcespeakers";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "FriendOfTheIcespeakers"))
		{
			return true;
		}
		return false;
	}
}
