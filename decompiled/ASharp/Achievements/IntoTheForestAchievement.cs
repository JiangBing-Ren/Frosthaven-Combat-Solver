using System.Linq;
using FHRL;

namespace Achievements;

public class IntoTheForestAchievement : EventAchievement
{
	public IntoTheForestAchievement()
	{
		ID = "ACH3_IntotheForest";
		Name = "ACH3_IntotheForest";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "IntoTheForest"))
		{
			return true;
		}
		return false;
	}
}
