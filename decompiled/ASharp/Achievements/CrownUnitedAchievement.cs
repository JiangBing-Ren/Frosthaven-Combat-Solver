using System.Linq;
using FHRL;

namespace Achievements;

public class CrownUnitedAchievement : EventAchievement
{
	public CrownUnitedAchievement()
	{
		ID = "ACH2_CrownUnited";
		Name = "ACH2_CrownUnited";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext && campaignAchievementContext.CampaignState.Stickers.Any((CampaignStickerID x) => x.ID == "CrownReunited"))
		{
			return true;
		}
		return false;
	}
}
