using System.Linq;
using FHRL;

namespace Achievements;

public class BrummixAuthority : EventAchievement
{
	public BrummixAuthority()
	{
		ID = "ACH27_BrummixAuthority";
		Name = "ACH27_BrummixAuthority";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Count((CampaignStickerID x) => x.ID.Contains("Brummix")) >= 4)
		{
			return true;
		}
		return false;
	}
}
