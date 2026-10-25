using System.Linq;
using FHRL;

namespace Achievements;

public class BrummixScout : EventAchievement
{
	public BrummixScout()
	{
		ID = "ACH25_BrummixScout";
		Name = "ACH25_BrummixScout";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Count((CampaignStickerID x) => x.ID.Contains("Brummix")) >= 2)
		{
			return true;
		}
		return false;
	}
}
