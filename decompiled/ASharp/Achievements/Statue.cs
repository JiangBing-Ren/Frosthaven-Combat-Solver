using System.Linq;
using FHRL;

namespace Achievements;

public class Statue : EventAchievement
{
	public Statue()
	{
		ID = "ACH12_Statue";
		Name = "ACH12_Statue";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "Statue"))
		{
			return true;
		}
		return false;
	}
}
