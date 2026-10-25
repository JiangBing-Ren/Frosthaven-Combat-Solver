using System.Collections;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace FH.Platforms.Steam;

public class NewPlatformSteamPlayerStatistics : NewPlatformPlayerStatistics
{
	private Result _userStatsState;

	private bool HasValidStatsState => _userStatsState == Result.OK;

	public override bool AchievementsSupported => true;

	protected override float PlatformPushCooldownSeconds => 60f;

	internal override IEnumerator Init(INewPlatformStorage storage)
	{
		yield return base.Init(storage);
		SteamUserStats.OnUserStatsReceived += OnReceiveUserStats;
		RequestCurrentStats();
	}

	protected override bool IsSingleAchievementCompleted(string achievementID)
	{
		foreach (Achievement achievement in SteamUserStats.Achievements)
		{
			if (achievement.Identifier == achievementID)
			{
				return achievement.State;
			}
		}
		return false;
	}

	protected override bool SetSingleAchievementCompleted(string achievementID)
	{
		foreach (Achievement achievement in SteamUserStats.Achievements)
		{
			if (achievement.Identifier == achievementID)
			{
				return achievement.Trigger();
			}
		}
		return false;
	}

	protected override async void ClearAllPlatformAchievements()
	{
		if (!HasValidStatsState)
		{
			await RequestCurrentStats();
		}
		foreach (Achievement achievement in SteamUserStats.Achievements)
		{
			achievement.Clear();
		}
		await RequestCurrentStats();
	}

	protected override bool SetStatValue(string statID, int value)
	{
		return SteamUserStats.SetStat(statID, value);
	}

	protected override async Task<bool> PushToPlatform()
	{
		if (!HasValidStatsState)
		{
			await RequestCurrentStats();
		}
		return SteamUserStats.StoreStats();
	}

	private async Task RequestCurrentStats()
	{
		SteamUserStats.RequestCurrentStats();
		_userStatsState = Result.None;
		await FHTaskExtensions.WaitWhile(() => _userStatsState == Result.None);
	}

	private void OnReceiveUserStats(SteamId userId, Result result)
	{
		_userStatsState = result;
	}

	public override void Dispose()
	{
		base.Dispose();
		SteamUserStats.OnUserStatsReceived -= OnReceiveUserStats;
	}
}
