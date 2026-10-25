using System.Linq;
using FHRL;

namespace Achievements;

public class Stage : EventAchievement
{
	public Stage()
	{
		ID = "ACH17_Stage";
		Name = "ACH17_Stage";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "Stage"))
		{
			return true;
		}
		return false;
	}
}
