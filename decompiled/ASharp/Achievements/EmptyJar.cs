using System.Linq;
using FHRL;

namespace Achievements;

public class EmptyJar : EventAchievement
{
	public EmptyJar()
	{
		ID = "ACH21_EmptyJar";
		Name = "ACH21_EmptyJar";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "EmptyJar"))
		{
			return true;
		}
		return false;
	}
}
