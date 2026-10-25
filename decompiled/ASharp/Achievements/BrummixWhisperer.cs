using System.Linq;
using FHRL;

namespace Achievements;

public class BrummixWhisperer : EventAchievement
{
	public BrummixWhisperer()
	{
		ID = "ACH28_BrummixWhisperer";
		Name = "ACH28_BrummixWhisperer";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Count((CampaignStickerID x) => x.ID.Contains("Brummix")) >= 5)
		{
			return true;
		}
		return false;
	}
}
