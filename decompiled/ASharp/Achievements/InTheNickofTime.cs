using System.Linq;
using FHRL;

namespace Achievements;

public class InTheNickofTime : EventAchievement
{
	public InTheNickofTime()
	{
		ID = "ACH37_InTheNickofTime";
		Name = "ACH37_InTheNickofTime";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is CampaignAchievementContext { CampaignState: not null } campaignAchievementContext)
		{
			ScenarioState scenarioState = campaignAchievementContext.CampaignState.UnlockedScenarios.SingleOrDefault((ScenarioState x) => x.ScenarioID.ID == 1);
			if (scenarioState != null && scenarioState.Completed)
			{
				return true;
			}
		}
		return false;
	}
}
