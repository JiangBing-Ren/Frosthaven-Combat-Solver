using System.Linq;
using ScenarioRuleLibrary;

namespace Achievements;

public class CleanAndMeanAchievement : EventAchievement
{
	public CleanAndMeanAchievement()
	{
		ID = "CleanAndMean";
		Name = "Clean and Mean";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context is ScenarioAchievementContext scenarioAchievementContext && scenarioAchievementContext.ScenarioState.AllEnemyStates.Where((EnemyState c) => c.IsRevealed).All((EnemyState c) => c.Actor.IsDead))
		{
			return true;
		}
		return false;
	}
}
