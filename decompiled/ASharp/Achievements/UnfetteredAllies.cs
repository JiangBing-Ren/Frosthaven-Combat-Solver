using System.Linq;
using FHRL;

namespace Achievements;

public class UnfetteredAllies : EventAchievement
{
	public UnfetteredAllies()
	{
		ID = "ACH10_UnfetteredAllies";
		Name = "ACH10_UnfetteredAllies";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "UnfetteredAllies"))
		{
			return true;
		}
		return false;
	}
}
