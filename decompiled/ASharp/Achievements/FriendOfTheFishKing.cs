using System.Linq;
using FHRL;

namespace Achievements;

public class FriendOfTheFishKing : EventAchievement
{
	public FriendOfTheFishKing()
	{
		ID = "ACH11_FriendoftheFishKing";
		Name = "ACH11_FriendoftheFishKing";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "FriendOfTheFishKing"))
		{
			return true;
		}
		return false;
	}
}
