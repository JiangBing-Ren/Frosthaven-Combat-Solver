using System.Linq;
using FHRL;

namespace Achievements;

public class BrummixTracksTyro : EventAchievement
{
	public BrummixTracksTyro()
	{
		ID = "ACH24_BrummixTracksTyro";
		Name = "ACH24_BrummixTracksTyro";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Count((CampaignStickerID x) => x.ID.Contains("Brummix")) >= 1)
		{
			return true;
		}
		return false;
	}
}
