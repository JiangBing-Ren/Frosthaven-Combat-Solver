using System.Linq;
using FHRL;

namespace Achievements;

public class FriendOfTheFrostspeakers : EventAchievement
{
	public FriendOfTheFrostspeakers()
	{
		ID = "ACH8_FriendoftheFrostspeakers";
		Name = "ACH8_FriendoftheFrostspeakers";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "FriendOfTheFrostspeakers"))
		{
			return true;
		}
		return false;
	}
}
