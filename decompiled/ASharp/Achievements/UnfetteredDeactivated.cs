using System.Linq;
using FHRL;

namespace Achievements;

public class UnfetteredDeactivated : EventAchievement
{
	public UnfetteredDeactivated()
	{
		ID = "ACH9_UnfetteredDeactivated";
		Name = "ACH9_UnfetteredDeactivated";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "UnfetteredDeactivated"))
		{
			return true;
		}
		return false;
	}
}
