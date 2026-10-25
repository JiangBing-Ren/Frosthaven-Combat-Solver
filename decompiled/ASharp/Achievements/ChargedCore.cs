using System.Linq;
using FHRL;

namespace Achievements;

public class ChargedCore : EventAchievement
{
	public ChargedCore()
	{
		ID = "ACH22_ChargedCore";
		Name = "ACH22_ChargedCore";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "ChargedCore"))
		{
			return true;
		}
		return false;
	}
}
