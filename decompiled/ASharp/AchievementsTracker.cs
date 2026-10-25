using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Achievements;
using FH.Platforms;
using FHGUI.Common;
using FHRL;
using Libs.GameConsole;
using Libs.Utils;
using ScenarioRuleLibrary;
using SharedLibrary.Achievments;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class AchievementsTracker : Singleton<AchievementsTracker>
{
	public bool AchievementsEnabled = true;

	private const string StorageID = "AchivementStats";

	private volatile bool _globalDataChanged;

	private volatile bool _achievementsChanged;

	private bool _initialized;

	private ScenarioAchievementContext _scenarioContext;

	private CampaignAchievementContext _campaignContext;

	private AchievementContext _globalContext;

	private ConcurrentQueue<Achievement> _completedAchievements = new ConcurrentQueue<Achievement>();

	private object _lock = new object();

	private NewPlatformLayer Platform => PlatformLayer.Instance?.NewPlatformLayer;

	public AchievementsRepository AchievementsRepo { get; private set; }

	public static void CheckAchievements()
	{
		if (Singleton<AchievementsTracker>.Instance == null || !Singleton<AchievementsTracker>.Instance.AchievementsEnabled)
		{
			return;
		}
		lock (Singleton<AchievementsTracker>.Instance._lock)
		{
			Log.Info("[Achievement] Checking achievements for changes");
			IEnumerable<Achievement> enumerable = Singleton<AchievementsTracker>.Instance.AchievementsRepo.Check(Singleton<AchievementsTracker>.Instance.GetContext());
			if (!enumerable.Any())
			{
				return;
			}
			Singleton<AchievementsTracker>.Instance._achievementsChanged = true;
			Log.Info($"[Achievement] {enumerable.Count()} achievement changed");
			foreach (Achievement item in enumerable)
			{
				if (item.Completed)
				{
					Singleton<AchievementsTracker>.Instance.HandleAchievementCompleted(item);
				}
			}
		}
	}

	protected override void AwakeInit()
	{
		base.AwakeInit();
		UIContextualTutorialModule.Cache();
		AchievementStatRepository.OnRepoDataChanged = OnRepoDataChanged;
		AchievementsRepo = new AchievementsRepository();
		ScenarioManager.CheckAchievementsCallback = CheckAchievements;
	}

	protected override void OnDestroy()
	{
		_completedAchievements.Clear();
		GameConsole.Context.Remove(this);
		_initialized = false;
		base.OnDestroy();
	}

	private void OnRepoDataChanged(AchievementStatRepository repo)
	{
		if (repo == GlobalStatRepository.Instance)
		{
			_globalDataChanged = true;
		}
		CheckAchievements();
	}

	private AchievementContext GetContext()
	{
		if (ScenarioManager.Scenario != null)
		{
			return _scenarioContext ?? (_scenarioContext = new ScenarioAchievementContext());
		}
		if (SaveData.Instance?.Global?.CurrentAdventureData != null)
		{
			return _campaignContext ?? (_campaignContext = new CampaignAchievementContext());
		}
		return _globalContext ?? (_globalContext = new AchievementContext());
	}

	public void Update()
	{
		if (AchievementsEnabled && !_initialized)
		{
			NewPlatformLayer platform = Platform;
			if (platform != null && platform.IsAvailableAndReady)
			{
				StartCoroutine(UpdateCrt());
			}
		}
	}

	private IEnumerator UpdateCrt()
	{
		GameConsole.Context.Add(this);
		_initialized = true;
		yield return RestoreAchievementsStorageDataCrt();
		yield return RestoreAchievementsStateFromPlatformCrt();
		StartCoroutine(ShowAchievementNotificationCrt());
		while (_initialized)
		{
			if (_globalDataChanged || _achievementsChanged)
			{
				_globalDataChanged = false;
				_achievementsChanged = false;
				yield return SaveAchievementsStorageDataCrt();
			}
			yield return null;
		}
	}

	private IEnumerator ShowAchievementNotificationCrt()
	{
		while (_initialized)
		{
			Achievement result;
			while (_completedAchievements.TryDequeue(out result))
			{
				AnalyticsWrapper.LogAchievementUnlocked(result.ID);
			}
			yield return null;
		}
	}

	private IEnumerator RestoreAchievementsStorageDataCrt()
	{
		ByRef<object> data = new ByRef<object>();
		ByRef<bool> success = new ByRef<bool>(value: false);
		yield return Platform.ReadData("AchivementStats", data, success);
		if (success.Value)
		{
			AchievementsStorageData achievementsStorageData = new StateSerializer<AchievementsStorageData>().BinaryDeserialzie((byte[])data.Value);
			if (achievementsStorageData?.GlobalStats != null)
			{
				achievementsStorageData.GlobalStats.Normalize();
				GlobalStatRepository.RestoreInstance(achievementsStorageData.GlobalStats);
			}
			else
			{
				GlobalStatRepository.ResetInstance();
			}
			if (achievementsStorageData?.Achievements != null)
			{
				AchievementsRepo.Restore(achievementsStorageData.Achievements);
			}
			else
			{
				AchievementsRepo.Reset();
			}
		}
	}

	private IEnumerator RestoreAchievementsStateFromPlatformCrt()
	{
		foreach (Achievement achievement in AchievementsRepo.AllAchievements)
		{
			Task<bool> task = Platform.IsAchievementCompleted(achievement.ID);
			while (!task.IsCompleted)
			{
				yield return null;
			}
			if (task.Result && !achievement.Completed)
			{
				achievement.Complete();
			}
		}
	}

	private IEnumerator SaveAchievementsStorageDataCrt()
	{
		ByRef<bool> success = new ByRef<bool>(value: false);
		AchievementsStorageData state = new AchievementsStorageData
		{
			GlobalStats = GlobalStatRepository.Instance,
			Achievements = AchievementsRepo.AllAchievements.Select((Achievement a) => new Achievement.StorageData
			{
				ID = a.ID,
				Completed = a.Completed
			}).ToList()
		};
		byte[] data = new StateSerializer<AchievementsStorageData>().BinarySerialize(state);
		yield return Platform.StoreData("AchivementStats", data, success);
	}

	private void HandleAchievementCompleted(Achievement ach)
	{
		_completedAchievements.Enqueue(ach);
		Log.Info("[Achievement] '" + ach.ID + "' completed");
		Platform.SetAchievementCompleted(ach.ID.AsEnumerable());
		_achievementsChanged = true;
	}

	[ConsoleCommand(Command = "ach_stats_print_all", Description = "Print achievement statistics", RequiredContext = typeof(AchievementsTracker))]
	public static void PrintStat(IConsole console)
	{
		foreach (KeyValuePair<string, AchievementStatRepository> allRepo in AchievementStatRepository.GetAllRepos())
		{
			foreach (KeyValuePair<string, AchievementStat> item in from t in allRepo.Value.GetAllStats()
				orderby t.Key
				select t)
			{
				console.WriteLine(allRepo.Key + "." + item.Key + " = " + item.Value.GetStringValue());
			}
		}
	}

	[ConsoleCommand(Command = "ach_print_all", Description = "Print all achievements status", RequiredContext = typeof(AchievementsTracker))]
	public static void PrintAchievements(IConsole console)
	{
		foreach (Achievement allAchievement in Singleton<AchievementsTracker>.Instance.AchievementsRepo.AllAchievements)
		{
			console.WriteLine($"{allAchievement.ID} [{allAchievement.Completed}] = {allAchievement.GetStringProgress()}");
		}
	}

	[ConsoleCommand(Command = "ach_clear_all", Description = "Reset all statistics and set all achievements to uncompleted", RequiredContext = typeof(AchievementsTracker))]
	public static void ClearAchievementsAndStats(IConsole console)
	{
		Singleton<AchievementsTracker>.Instance.AchievementsRepo.Reset();
		Singleton<AchievementsTracker>.Instance.Platform.ClearAllAchievements().Wait();
		GlobalStatRepository.Instance.Reset();
		CampaignStatRepository.Instance.Reset();
		ScenarioStatRepository.Instance.Reset();
		Singleton<AchievementsTracker>.Instance._globalDataChanged = true;
		Singleton<AchievementsTracker>.Instance._achievementsChanged = true;
	}

	[ConsoleCommand(Command = "tutorial_clear_all", Description = "Reset all tutorials so that you can see them again")]
	public static void ClearTutorialStats(IConsole console)
	{
		GlobalStatRepository.Instance.ResetTutorials();
	}

	[ConsoleCommand(Command = "toggle_tutorials", Description = "Reset all tutorials so that you can see them again")]
	public static void ToggleTutorials(IConsole console)
	{
		UIContextualTutorialModule.SetTutorialsEnabled(!UIContextualTutorialModule.AreTutorialsEnabled);
		console.WriteLine($"Tutorials enabled: {UIContextualTutorialModule.AreTutorialsEnabled}");
	}

	[ConsoleCommand(Command = "scenario_mechanics_clear_all", Description = "Reset scenario mechanics so that you can see them again")]
	public static void ClearScenarioMechanicsStats(IConsole console)
	{
		GlobalStatRepository.Instance.ResetScenarioMechanics();
	}

	[ConsoleCommand(Command = "codex_clear_all", Description = "Reset Codex entries unlocked/seen state")]
	public static void ClearCodexStats(IConsole console)
	{
		GlobalStatRepository.Instance.ResetCodex();
	}

	[ConsoleCommand(Command = "ach_check", Description = "Check for all achievements progress/completion", RequiredContext = typeof(AchievementsTracker))]
	public static void TriggerCheckAchievements(IConsole console)
	{
		CheckAchievements();
	}

	[ConsoleCommand(Command = "ach_complete_all", Description = "Complete all uncompleted achievements", RequiredContext = typeof(AchievementsTracker))]
	public static void CompleteAllAchievements(IConsole console)
	{
		foreach (Achievement allAchievement in Singleton<AchievementsTracker>.Instance.AchievementsRepo.AllAchievements)
		{
			if (!allAchievement.Completed)
			{
				allAchievement.Complete();
				Singleton<AchievementsTracker>.Instance.HandleAchievementCompleted(allAchievement);
			}
		}
	}

	[ConsoleCommand(Command = "ach_complete", Description = "Complete specific achievement", RequiredContext = typeof(AchievementsTracker))]
	public static void CompleteAllAchievements(IConsole console, string achievementID)
	{
		Achievement achievement = Singleton<AchievementsTracker>.Instance.AchievementsRepo.AllAchievements.FirstOrDefault((Achievement a) => a.ID == achievementID);
		if (achievement == null)
		{
			console.WriteLineWithStyle("Invalid achievement id '" + achievementID + "'", CommandOutputStyle.Error);
			return;
		}
		if (achievement.Completed)
		{
			console.WriteLineWithStyle("Achievement '" + achievementID + "' is already completed", CommandOutputStyle.Warning);
			return;
		}
		achievement.Complete();
		Singleton<AchievementsTracker>.Instance.HandleAchievementCompleted(achievement);
	}
}
