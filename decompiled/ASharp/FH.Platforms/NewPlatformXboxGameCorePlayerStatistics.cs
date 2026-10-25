using System.Threading.Tasks;

namespace FH.Platforms;

public class NewPlatformXboxGameCorePlayerStatistics : NewPlatformPlayerStatistics
{
	public override bool AchievementsSupported => false;

	protected override float PlatformPushCooldownSeconds => 60f;

	protected override bool IsSingleAchievementCompleted(string achievementID)
	{
		return false;
	}

	protected override bool SetSingleAchievementCompleted(string achievementID)
	{
		return false;
	}

	protected override void ClearAllPlatformAchievements()
	{
	}

	protected override bool SetStatValue(string statID, int value)
	{
		return false;
	}

	protected override Task<bool> PushToPlatform()
	{
		return Task.FromResult(result: false);
	}
}
