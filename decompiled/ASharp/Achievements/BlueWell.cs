using System.Linq;
using FHRL;

namespace Achievements;

public class BlueWell : EventAchievement
{
	public BlueWell()
	{
		ID = "ACH15_BlueWell";
		Name = "ACH15_BlueWell";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "BlueWell"))
		{
			return true;
		}
		return false;
	}
}
