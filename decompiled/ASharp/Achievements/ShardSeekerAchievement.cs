using System.Linq;
using FHRL;

namespace Achievements;

public class ShardSeekerAchievement : EventAchievement
{
	public ShardSeekerAchievement()
	{
		ID = "ACH1_ShardSeeker";
		Name = "ACH1_ShardSeeker";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "ShardSeeker"))
		{
			return true;
		}
		return false;
	}
}
