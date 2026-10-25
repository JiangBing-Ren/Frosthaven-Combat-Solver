using FHRL;
using SharedLibrary.Achievments;

namespace Achievements;

public class CampaignAchievementContext : AchievementContext
{
	public CampaignState CampaignState => SaveData.Instance?.Global?.CurrentAdventureData?.FrosthavenCampaign;

	public CampaignStatRepository CampaignStats => CampaignStatRepository.Instance;
}
