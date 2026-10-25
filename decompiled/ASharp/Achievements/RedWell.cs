using System.Linq;
using FHRL;

namespace Achievements;

public class RedWell : EventAchievement
{
	public RedWell()
	{
		ID = "ACH14_RedWell";
		Name = "ACH14_RedWell";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "RedWell"))
		{
			return true;
		}
		return false;
	}
}
