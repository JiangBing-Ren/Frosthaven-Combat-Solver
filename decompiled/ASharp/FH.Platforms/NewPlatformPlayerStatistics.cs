using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Libs.GameConsole;
using Libs.Utils;

namespace FH.Platforms;

public abstract class NewPlatformPlayerStatistics : IDisposable
{
	private readonly Queue<string> _pendingAchievements = new Queue<string>();

	private readonly Queue<(string, int)> _pendingStats = new Queue<(string, int)>();

	private bool _pendingPush;

	private DateTime _nextValidPushTime = DateTime.MinValue;

	private INewPlatformStorage _storage;

	private const string SaveFilename = "PlayerStatistics.dat";

	public abstract bool AchievementsSupported { get; }

	protected abstract float PlatformPushCooldownSeconds { get; }

	internal virtual IEnumerator Init(INewPlatformStorage storage)
	{
		_storage = storage;
		if (_storage.LoadFromFile("PlayerStatistics.dat", out var data) && data is NewPlatformPlayerStatisticsSerializedData newPlatformPlayerStatisticsSerializedData)
		{
			newPlatformPlayerStatisticsSerializedData.PendingAchievements.ForEach((string x) =>
			{
				_pendingAchievements.Enqueue(x);
			});
			newPlatformPlayerStatisticsSerializedData.PendingStats.ForEach(((string, int) x) =>
			{
				_pendingStats.Enqueue(x);
			});
		}
		yield break;
	}

	internal virtual IEnumerator Tick()
	{
		ProcessQueue(_pendingAchievements, SetSingleAchievementCompleted);
		ProcessQueue(_pendingStats, ((string, int) tuple) => SetStatValue(tuple.Item1, tuple.Item2));
		if (_pendingPush && !(_nextValidPushTime > DateTime.Now))
		{
			Task<bool> pushTask = PushToPlatform();
			yield return new WaitForTask(pushTask);
			_pendingPush = !pushTask.Result;
			_nextValidPushTime = DateTime.Now.AddSeconds(PlatformPushCooldownSeconds);
		}
	}

	private void ProcessQueue<TQueue>(Queue<TQueue> queue, Func<TQueue, bool> processMethod)
	{
		int count = queue.Count;
		for (int i = 0; i < count; i++)
		{
			TQueue val = queue.Dequeue();
			if (!processMethod(val))
			{
				queue.Enqueue(val);
			}
			else
			{
				_pendingPush = true;
			}
		}
	}

	public void SetAchievementCompleted(IEnumerable<string> achievementNames)
	{
		achievementNames.ForEach((string achievement) =>
		{
			if (!_pendingAchievements.Contains(achievement))
			{
				_pendingAchievements.Enqueue(achievement);
			}
		});
	}

	public void ClearAllAchievements()
	{
		_pendingAchievements.Clear();
		ClearAllPlatformAchievements();
		_pendingPush = true;
	}

	public bool IsAchievementCompleted(string achievementName, bool includeOffline = true)
	{
		if (includeOffline && _pendingAchievements.Contains(achievementName))
		{
			return true;
		}
		return IsSingleAchievementCompleted(achievementName);
	}

	public void SetStat(string statName, int value)
	{
		if (string.IsNullOrEmpty(statName))
		{
			throw new Exception("Tried to set stat, but provided stat name was null or empty.");
		}
		_pendingStats.Enqueue((statName, value));
	}

	protected abstract bool IsSingleAchievementCompleted(string achievementID);

	protected abstract bool SetSingleAchievementCompleted(string achievementID);

	protected abstract void ClearAllPlatformAchievements();

	protected abstract bool SetStatValue(string statID, int value);

	protected abstract Task<bool> PushToPlatform();

	public virtual void Dispose()
	{
		NewPlatformPlayerStatisticsSerializedData newPlatformPlayerStatisticsSerializedData = new NewPlatformPlayerStatisticsSerializedData
		{
			PendingAchievements = _pendingAchievements,
			PendingStats = _pendingStats
		};
		_storage?.SaveToFile("PlayerStatistics.dat", newPlatformPlayerStatisticsSerializedData);
	}

	[ConsoleCommand(Command = "trigger_test_achievement")]
	public static void TriggerTestAchievement(IConsole console)
	{
		PlatformLayer.Instance.NewPlatformLayer.SetAchievementCompleted(new string[1] { "DEV_TEST_ACHIEVEMENT" });
	}

	[ConsoleCommand(Command = "DebugTestGetAchievement", Description = "Debug test for getting an achievement")]
	public static void DebugTestGetAchievement(IConsole console)
	{
		PlatformLayer.Instance.NewPlatformLayer.IsAchievementCompleted("DEV_TEST_ACHIEVEMENT", includeOffline: false);
	}

	[ConsoleCommand(Command = "clear_achievements")]
	public static void ClearAchievements(IConsole console)
	{
		PlatformLayer.Instance.NewPlatformLayer.ClearAllAchievements();
	}

	[ConsoleCommand(Command = "force_check_achievements")]
	public static void ForceCheckAchievement(IConsole console)
	{
		console.WriteWithStyle("TODO: Integrate with new gameplay achievement system", CommandOutputStyle.Warning);
	}

	[ConsoleCommand(Command = "platform_stat_set")]
	public static void SetPlatformStat(IConsole console, string name, int value)
	{
		PlatformLayer.Instance.NewPlatformLayer.SetStat(name, value);
	}
}
