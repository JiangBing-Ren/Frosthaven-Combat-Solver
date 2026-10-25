using System.Linq;
using FHRL;

namespace Achievements;

public class DestroyerOfTheSnowspeakers : EventAchievement
{
	public DestroyerOfTheSnowspeakers()
	{
		ID = "ACH5_DestroyeroftheSnowspeakers";
		Name = "ACH5_DestroyeroftheSnowspeakers";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "DestroyerOfTheSnowspeakers"))
		{
			return true;
		}
		return false;
	}
}
