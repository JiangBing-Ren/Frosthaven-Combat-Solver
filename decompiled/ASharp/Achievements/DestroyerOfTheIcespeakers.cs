using System.Linq;
using FHRL;

namespace Achievements;

public class DestroyerOfTheIcespeakers : EventAchievement
{
	public DestroyerOfTheIcespeakers()
	{
		ID = "ACH4_DestroyeroftheIcespeakers";
		Name = "ACH4_DestroyeroftheIcespeakers";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "DestroyerOfTheIcespeakers"))
		{
			return true;
		}
		return false;
	}
}
