using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FH.ApparanceBaking;
using JetBrains.Annotations;
using Libs.Penumbra;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
public class PenumbraJob_RunScenario : PenumbraJob
{
	public CCustomLevelData target;

	public PartyAdventureData partyData;

	private static EGameMode gameMode = EGameMode.Campaign;

	private static ERulesFlag rules = ERulesFlag.None;

	public override IEnumerator LoadCrt(ILogger logger, PenumbraJobResults outJobResults, [CanBeNull] PenumbraStopwatch stopwatch)
	{
		logger.Log("loading scenario " + target.Name);
		stopwatch?.Time("Start");
		TelemetryAndEulaPolicy.SetEulaAccepted(accepted: true);
		TelemetryAndEulaPolicy.SetTelemetryEnabled(enabled: true);
		if (AdventureState.MapState == null || AdventureState.MapState.MapParty == null || AdventureState.MapState.MapParty.DrawPossiblePersonalQuests().Count == 0)
		{
			logger.Log("Loading YML again due to missing personal quests.");
			SceneController.Instance.YML.LoadCampaign();
		}
		ErrorMessage.Unattended = true;
		Log.onCriticalError = (Action<string>)Delegate.Combine(Log.onCriticalError, new Action<string>(CriticalError));
		try
		{
			partyData = CustomPartySetup.CreateAllUnlockedParty(Guid.NewGuid().ToString(), EAdventureDifficulty.Normal, gameMode, rules);
			CustomPartySetup.AddEachCharacter(partyData, 4);
		}
		catch (Exception e)
		{
			HandleException(e);
			yield break;
		}
		Task loadTask = SaveData.Instance.LoadCustomLevelFromData(target, LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty);
		yield return new WaitForTask(loadTask);
		if (loadTask.HasFailed(out var exception))
		{
			HandleException(exception);
			yield break;
		}
		Task startTask = SaveData.Instance.StartNewCustomLevel(target, partyData);
		yield return new WaitForTask(startTask);
		if (startTask.HasFailed(out exception))
		{
			HandleException(exception);
			yield break;
		}
		try
		{
			PlayerStartingTiles(SaveData.Instance.Global.CurrentCustomLevelData);
		}
		catch (Exception e2)
		{
			HandleException(e2);
			yield break;
		}
		stopwatch?.Time("Penumbra Loaded YML Campaign");
		yield return SceneController.Instance.LoadCustomLevelCoroutine(SaveData.Instance.Global.CurrentCustomLevelData, isFrontEndTutorial: false, new KronosProfilingScaffolding
		{
			Stopwatch = stopwatch
		});
		RoomVisibilityTracker.RunningAutomationTests = true;
		while (!EnvironmentLoader.Instance || !EnvironmentLoader.Instance.IsLoadingComplete)
		{
			yield return null;
		}
		stopwatch?.Time("Penumbra Load Job Finish");
		void CriticalError(string msg)
		{
			outJobResults.Add(new PenumbraMeasurement("ERROR")
			{
				IsAccepted = false,
				BigValue = 1
			});
			string text = new StackTrace().ToString();
			logger.LogError("Error while loading scenario " + target?.Name + "\n ------------------ DUMPING CALLSTACK --------------\n" + text + "\n------------- END ---------------", msg);
			Log.onCriticalError = (Action<string>)Delegate.Remove(Log.onCriticalError, new Action<string>(CriticalError));
		}
		void HandleException(Exception exception2)
		{
			outJobResults.Add(new PenumbraMeasurement("ERROR_PenumbraJob_RunScenario")
			{
				IsAccepted = false,
				BigValue = 1
			});
			logger.LogException(exception2);
			Log.onCriticalError = (Action<string>)Delegate.Remove(Log.onCriticalError, new Action<string>(CriticalError));
		}
	}

	private void PlayerStartingTiles(CCustomLevelData currentCustomLevelData)
	{
		foreach (PlayerState player in currentCustomLevelData.ScenarioState.Players)
		{
			if (player.Location != null)
			{
				continue;
			}
			player.Location = currentCustomLevelData.StartingTileIndexes.FirstOrDefault((TileIndex a) => !currentCustomLevelData.ScenarioState.Players.Any((PlayerState p) => p.Location == a));
		}
	}

	public override IEnumerator UnloadCrt(ILogger logger)
	{
		logger.Log("Exiting. Waiting for ESCMenu.Instance");
		while (!Singleton<ESCMenu>.IsInitialized)
		{
			yield return null;
		}
		yield return UIManager.LoadMainMenu();
		yield return new WaitForSeconds(3f);
		Task deleteTask = SaveData.Instance.DeletePartyData(partyData, gameMode);
		float timeout = 60f;
		while (!deleteTask.IsCompleted && timeout > 0f)
		{
			timeout -= Time.deltaTime;
			yield return null;
		}
		if (deleteTask.HasFailed(out var exception))
		{
			logger.LogError("Error", exception);
		}
	}
}
