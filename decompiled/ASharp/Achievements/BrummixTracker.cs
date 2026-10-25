using System.Linq;
using FHRL;

namespace Achievements;

public class BrummixTracker : EventAchievement
{
	public BrummixTracker()
	{
		ID = "ACH26_BrummixTracker";
		Name = "ACH26_BrummixTracker";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Count((CampaignStickerID x) => x.ID.Contains("Brummix")) >= 3)
		{
			return true;
		}
		return false;
	}
}
