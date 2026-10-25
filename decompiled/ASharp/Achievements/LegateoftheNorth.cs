using System.Linq;
using FHRL;

namespace Achievements;

public class LegateoftheNorth : EventAchievement
{
	public LegateoftheNorth()
	{
		ID = "ACH35_LegateoftheNorth";
		Name = "ACH35_LegateoftheNorth";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext)
		{
			ScenarioState scenarioState = campaignAchievementContext.CampaignState.UnlockedScenarios.SingleOrDefault((ScenarioState x) => x.ScenarioID.ID == 64);
			if (scenarioState != null && scenarioState.Completed && (scenarioState.CompletedOnDifficulties.Contains("Brutal") || scenarioState.CompletedOnDifficulties.Contains("Unfair")))
			{
				return true;
			}
		}
		return false;
	}
}
