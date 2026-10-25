using System.Linq;
using FHRL;

namespace Achievements;

public class Firepepper : EventAchievement
{
	public Firepepper()
	{
		ID = "ACH19_Firepepper";
		Name = "ACH19_Firepepper";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "Firepepper"))
		{
			return true;
		}
		return false;
	}
}
