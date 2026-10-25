using System.Linq;
using FHRL;

namespace Achievements;

public class SaintoftheNorth : EventAchievement
{
	public SaintoftheNorth()
	{
		ID = "ACH36_SaintoftheNorth";
		Name = "ACH36_SaintoftheNorth";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext)
		{
			ScenarioState scenarioState = campaignAchievementContext.CampaignState.UnlockedScenarios.SingleOrDefault((ScenarioState x) => x.ScenarioID.ID == 64);
			if (scenarioState != null && scenarioState.Completed && scenarioState.CompletedOnDifficulties.Contains("Unfair"))
			{
				return true;
			}
		}
		return false;
	}
}
