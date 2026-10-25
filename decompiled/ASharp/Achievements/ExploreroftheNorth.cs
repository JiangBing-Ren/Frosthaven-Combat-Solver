using System.Linq;
using FHRL;

namespace Achievements;

public class ExploreroftheNorth : EventAchievement
{
	public ExploreroftheNorth()
	{
		ID = "ACH31_ExploreroftheNorth";
		Name = "ACH31_ExploreroftheNorth";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext)
		{
			ScenarioState scenarioState = campaignAchievementContext.CampaignState.UnlockedScenarios.SingleOrDefault((ScenarioState x) => x.ScenarioID.ID == 64);
			if (scenarioState != null && scenarioState.Completed && (scenarioState.CompletedOnDifficulties.Contains("Story") || scenarioState.CompletedOnDifficulties.Contains("Normal") || scenarioState.CompletedOnDifficulties.Contains("Veteran") || scenarioState.CompletedOnDifficulties.Contains("Hard") || scenarioState.CompletedOnDifficulties.Contains("Brutal") || scenarioState.CompletedOnDifficulties.Contains("Unfair")))
			{
				return true;
			}
		}
		return false;
	}
}
