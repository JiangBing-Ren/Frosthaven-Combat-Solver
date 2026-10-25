using System.Linq;

namespace Achievements;

public class WarchiefDenied : EventAchievement
{
	public WarchiefDenied()
	{
		ID = "ACH38_WarchiefDenied";
		Name = "ACH38_WarchiefDenied";
	}

	protected override bool CheckCompleted(AchievementContext context)
	{
		if (context.GlobalStats.CompletedScenarios.Values.Contains("207"))
		{
			return true;
		}
		return false;
	}
}
