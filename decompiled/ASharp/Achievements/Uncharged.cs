using System.Linq;
using FHRL;

namespace Achievements;

public class Uncharged : EventAchievement
{
	public Uncharged()
	{
		ID = "ACH23_Uncharged";
		Name = "ACH23_Uncharged";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "UnchargedCore"))
		{
			return true;
		}
		return false;
	}
}
