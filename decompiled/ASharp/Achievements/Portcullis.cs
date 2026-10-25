using System.Linq;
using FHRL;

namespace Achievements;

public class Portcullis : EventAchievement
{
	public Portcullis()
	{
		ID = "ACH16_Portcullis";
		Name = "ACH16_Portcullis";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "Portcullis"))
		{
			return true;
		}
		return false;
	}
}
