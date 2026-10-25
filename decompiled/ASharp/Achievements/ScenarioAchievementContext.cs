using ScenarioRuleLibrary;
using SharedLibrary.Achievments;

namespace Achievements;

public class ScenarioAchievementContext : CampaignAchievementContext
{
	public ScenarioState ScenarioState => ScenarioManager.CurrentScenarioState;

	public ScenarioStatRepository ScenarioStats => ScenarioStatRepository.Instance;
}
