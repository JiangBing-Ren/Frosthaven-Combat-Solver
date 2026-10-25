using System.Linq;
using FHRL;

namespace Achievements;

public class BugInAJar : EventAchievement
{
	public BugInAJar()
	{
		ID = "ACH20_BuginaJar";
		Name = "ACH20_BuginaJar";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "BugInAJar"))
		{
			return true;
		}
		return false;
	}
}
