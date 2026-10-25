using System.Linq;
using FHRL;

namespace Achievements;

public class HarvestFestival : EventAchievement
{
	public HarvestFestival()
	{
		ID = "ACH13_HarvestFestival";
		Name = "ACH13_HarvestFestival";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "HarvestFestival"))
		{
			return true;
		}
		return false;
	}
}
