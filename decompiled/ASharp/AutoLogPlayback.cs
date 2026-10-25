using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AStar;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoLogPlayback
{
	private string m_CurrentEventReport;

	private List<CAuto> m_RemoveEvents = new List<CAuto>();

	private List<CAuto> m_RemoveEventsAndDecrementChoreographerCount = new List<CAuto>();

	private List<CAutoStepChoreographer> m_InsertMissingChoreographerSteps = new List<CAutoStepChoreographer>();

	private List<CAuto> m_Events;

	private int m_CurrentIndex;

	private int m_EventIdWaitingToResolveFor;

	private float m_TimeWhenStartedWaitingForMessageQueue;

	private float m_TimeAtStartOfWaitingForChoreographerToBePaused;

	private const float cMessageQueueResolveDelay = 3f;

	private const int cNumberOfFutureRecordingStepsToCheck = 15;

	private const float cPlaybackActionInterval = 0.3f;

	private const float cSafeTimeBetweenMessagesInterval = 20f;

	private const float cPlaybackStartLockTimeout = 10f;

	private CAuto m_EventCurrentlyWaitingOn;

	private const float cChoreographerNotPausedTimeout = 5f;

	private GameObject m_ObjectWaitingForPointerExit;

	private bool m_UseRealtime;

	public CAutoLog CurrentLogPlayedBack { get; private set; }

	public CAuto CurrentEvent { get; private set; }

	public bool TestEnded { get; set; }

	public bool FinalRecordedChoreographerEventOccured { get; private set; }

	public string AutotestPlaybackReport { get; private set; }

	public void PlaybackAutoLog(CAutoLog logToPlayback, bool useRealtime)
	{
		AutoTestController.s_Instance.m_AutotestResult = false;
		m_RemoveEvents.Clear();
		m_RemoveEventsAndDecrementChoreographerCount.Clear();
		m_InsertMissingChoreographerSteps.Clear();
		if (logToPlayback == null)
		{
			Log.Warning("No autolog to playback");
			return;
		}
		CurrentLogPlayedBack = logToPlayback;
		m_UseRealtime = useRealtime;
		m_Events = logToPlayback.Events.ToList();
		SaveData.Instance.Global.RegularSpeedMultiplier = 4f;
		TestEnded = false;
		CoroutineHelper.RunCoroutine(PlaybackRecordedUIActions());
	}

	public void StopPlayback()
	{
		TestEnded = true;
	}

	private IEnumerator PlaybackRecordedUIActions()
	{
		float timeLastMessageWasProcessed = Time.realtimeSinceStartup;
		List<CAutoStepChoreographer> list = new List<CAutoStepChoreographer>();
		for (int i = 0; i < m_Events.Count; i++)
		{
			if (m_Events[i].EventType == CAuto.EAutoType.ChoreographerStep && m_Events[i] is CAutoStepChoreographer item)
			{
				list.Add(item);
			}
			else if (m_Events[i] is CAutoButtonHover || m_Events[i] is CAutoButtonClick)
			{
				CAuto buttonEvent = m_Events[i];
				FixButtonHierarchy(ref buttonEvent);
			}
		}
		int finalChoreographerEventID = list.Last().ID;
		FinalRecordedChoreographerEventOccured = false;
		AutotestPlaybackReport = string.Empty;
		if (!CurrentLogPlayedBack.ContainsTypedChoreographerSteps)
		{
			AutotestPlaybackReport = "\t[AUTOTEST PLAYBACK] Status: Starting playback for *LEGACY* playback type. Recorded actions should be updated to avoid consistency issues";
		}
		while (!TestEnded && !AutoTestController.s_Instance.TestErrorOccurred && AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			if (FinalRecordedChoreographerEventOccured)
			{
				SaveData.Instance.Global.RegularSpeedMultiplier = 1f;
				Log.Info("Autotest waiting for scenario to end safely");
				Choreographer.s_Choreographer.IsRestarting = true;
				while (CPathFinder.Locked || Choreographer.s_Choreographer.m_MessageQueue.Count > 0 || ScenarioRuleClient.IsProcessingOrMessagesQueued)
				{
					AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
					yield return new WaitForSecondsRealtime(0.5f);
				}
				yield return SceneController.Instance.EndScenarioSafely();
				Choreographer.s_Choreographer.IsRestarting = false;
				Log.Info("Autotest scenario ended");
				List<Tuple<int, string>> results = AutoTestController.AutoTestPlaybackCompleted();
				TestEnded = true;
				if (SaveData.Instance.AutoupdateAutotests)
				{
					bool updateState = false;
					if (results.Count > 0 && AutoTestController.CheckResults(results))
					{
						updateState = true;
					}
					if (results.Count == 0)
					{
						bool flag = false;
						if (m_InsertMissingChoreographerSteps.Count > 0)
						{
							int num = 0;
							foreach (CAutoStepChoreographer insertMissingChoreographerStep in m_InsertMissingChoreographerSteps)
							{
								int index = insertMissingChoreographerStep.ID + num;
								num++;
								insertMissingChoreographerStep.ID = AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Select((CAuto s) => s.ID).Max() + 1;
								insertMissingChoreographerStep.ChoreographerStepsCompleted -= num;
								AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Insert(index, insertMissingChoreographerStep);
							}
							flag = true;
						}
						if (m_RemoveEvents.Count > 0)
						{
							foreach (CAuto removeEvent in m_RemoveEvents)
							{
								AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Remove(removeEvent);
							}
							flag = true;
						}
						if (m_RemoveEventsAndDecrementChoreographerCount.Count > 0)
						{
							foreach (CAuto item2 in m_RemoveEventsAndDecrementChoreographerCount)
							{
								for (int num2 = AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.IndexOf(item2) + 1; num2 < AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Count; num2++)
								{
									AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events[num2].ChoreographerStepsCompleted--;
								}
								AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.Remove(item2);
							}
							flag = true;
						}
						if (flag)
						{
							Task<bool> task = SaveData.Instance.AutoTestDataManager.SaveAutoTestData(AutoTestController.s_Instance.CurrentAutoTestData, SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile);
							yield return new WaitForTask(task);
						}
					}
					if (SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile != null && ((SaveData.Instance.Global.LastLoadedAutoTestData.ScenarioState.StateNeedsUpdatesSaved && results.Count == 0) | updateState))
					{
						ScenarioManager.CurrentScenarioState.Update();
						ScenarioManager.CurrentScenarioState.ScenarioEventLog.Events.Clear();
						Task<ScenarioState> copyTask = ScenarioManager.CurrentScenarioState.DeepCopySerializableObject<ScenarioState>();
						yield return new WaitForTask(copyTask);
						AutoTestData lastLoadedAutoTestData = SaveData.Instance.Global.LastLoadedAutoTestData;
						lastLoadedAutoTestData.ExpectedResultingScenarioState = copyTask.Result;
						Task<bool> task2 = SaveData.Instance.AutoTestDataManager.SaveAutoTestData(lastLoadedAutoTestData, SaveData.Instance.AutoTestDataManager.CurrentlyRunningAutotestFile);
						yield return new WaitForTask(task2);
					}
				}
				AutoTestController.s_Instance.CurrentAutoLogPlayback = null;
				continue;
			}
			if (m_CurrentIndex >= m_Events.Count)
			{
				if (!Choreographer.s_Choreographer.HasPendingAnimations)
				{
					FinalRecordedChoreographerEventOccured = true;
				}
				else
				{
					yield return null;
				}
				continue;
			}
			if (m_ObjectWaitingForPointerExit != null)
			{
				if (m_ObjectWaitingForPointerExit != null)
				{
					ExecuteEvents.Execute(m_ObjectWaitingForPointerExit, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
				}
				m_ObjectWaitingForPointerExit = null;
			}
			m_CurrentEventReport = string.Empty;
			int currentIndex = m_CurrentIndex;
			CAuto cAuto = m_Events[currentIndex];
			CAuto cAuto2 = ((m_Events.Count > currentIndex + 1) ? m_Events[currentIndex + 1] : null);
			if (cAuto.EventType != CAuto.EAutoType.ChoreographerStep && (!CurrentLogPlayedBack.ContainsTypedChoreographerSteps || !AutoTestController.s_Instance.IsPausedForSpecificMessageType))
			{
				AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
			}
			if (cAuto.ChoreographerStepsCompleted <= AutoTestController.s_Instance.ChoreographerMessagesProcessed && (!cAuto.ForTutorial || Singleton<LevelEventsController>.Instance.NumberOfAlreadyDisplayedMessages >= cAuto.DisplayedMessageCount))
			{
				switch (cAuto.EventType)
				{
				case CAuto.EAutoType.ButtonClick:
				{
					if ((cAuto.ChoreographerState != Choreographer.s_Choreographer.m_WaitState.m_State && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForPlayerIdle || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForPlayerIdle) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForEndAbilityAnimSync || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForEndAbilityAnimSync) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForGeneralAnim || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForGeneralAnim) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForPlayerWaypointSelection || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForPlayerWaypointSelection)) || (cAuto.ForTutorial && Singleton<LevelMessagesUIHandler>.Instance.DisplayDelayInEffect))
					{
						break;
					}
					CAutoButtonClick cAutoButtonClick = cAuto as CAutoButtonClick;
					if (!cAutoButtonClick.ButtonHierarchy.Select((AutoLogUtility.ButtonAndOrder s) => s.ButtonName).Contains("Card Hand") || !Singleton<CardsHandManager>.Instance.cardHandsUI.Any((CardsHandUI a) => a.AnimatingLostCards))
					{
						if (GetButton(cAutoButtonClick.ButtonHierarchy, out var button))
						{
							CurrentEvent = cAutoButtonClick;
							ExecuteEvents.Execute(button, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
							m_CurrentIndex++;
						}
						else if (Time.realtimeSinceStartup - m_TimeAtStartOfWaitingForChoreographerToBePaused > 5f)
						{
							AutoTestController.s_Instance.EndTestPlaybackFromТimeout("AUTOTEST PLAYBACK] Playback Issue: Recording is trying to playback a button click, but the specified button is missing - Skipping recorded action.\n\t ===== Skipped recorded MessageType: " + cAutoButtonClick.EventType);
						}
					}
					break;
				}
				case CAuto.EAutoType.ButtonHover:
				{
					if (cAuto.ChoreographerState != Choreographer.s_Choreographer.m_WaitState.m_State && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForPlayerIdle || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForPlayerIdle) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForEndAbilityAnimSync || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForEndAbilityAnimSync) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForGeneralAnim || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForGeneralAnim) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.WaitingForPlayerWaypointSelection || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.Play) && (cAuto.ChoreographerState != Choreographer.ChoreographerStateType.Play || Choreographer.s_Choreographer.m_WaitState.m_State != Choreographer.ChoreographerStateType.WaitingForPlayerWaypointSelection))
					{
						break;
					}
					CAutoButtonHover cAutoButtonHover = cAuto as CAutoButtonHover;
					if (GetButton(cAutoButtonHover.ButtonHierarchy, out var button2, activeInHierarchyRequired: false, interactivityRequired: false, buttonRequired: false))
					{
						CurrentEvent = cAutoButtonHover;
						if (button2 != null)
						{
							m_ObjectWaitingForPointerExit = button2;
							ExecuteEvents.Execute(button2, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
						}
						m_CurrentIndex++;
					}
					break;
				}
				case CAuto.EAutoType.TileClick:
					if (cAuto.ChoreographerState == Choreographer.s_Choreographer.m_WaitState.m_State)
					{
						if ((!cAuto.ForTutorial || !Singleton<LevelMessagesUIHandler>.Instance.DisplayDelayInEffect) && TileBehaviour.s_Callback != null)
						{
							CAutoTileClick cAutoTileClick = (CAutoTileClick)(CurrentEvent = cAuto as CAutoTileClick);
							TileBehaviour.s_Callback(CAutoTileClick.TileIndexToClientTile(cAutoTileClick.SelectedTile), CAutoTileClick.TileIndexToCTileList(cAutoTileClick.OptionalTileList), networkActionIfOnline: false, isUserClick: false, SaveData.Instance.Global.EnableSecondClickHexToConfirm, cAutoTileClick.InputType);
							UIEventManager.AddTileSelectEvent(new Point(cAutoTileClick.SelectedTile), cAutoTileClick.InputType);
							m_CurrentIndex++;
						}
					}
					else if (cAuto.ChoreographerState == Choreographer.ChoreographerStateType.WaitingForPlayerWaypointSelection && Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.Play)
					{
						Log.Warning("Extra tile click detected in test");
						m_RemoveEvents.Add(cAuto);
						m_CurrentIndex++;
					}
					break;
				case CAuto.EAutoType.ChoreographerStep:
				{
					CAutoStepChoreographer choreoStep = cAuto as CAutoStepChoreographer;
					CurrentEvent = choreoStep;
					if (AutoTestController.s_ChoreographerPaused)
					{
						if (CurrentLogPlayedBack.ContainsTypedChoreographerSteps && AutoTestController.s_Instance.IsPausedForSpecificMessageType)
						{
							if (AutoTestController.s_Instance.MessageTypePausedFor == choreoStep.ChoreographerMessageType)
							{
								AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
								m_CurrentIndex++;
								break;
							}
							bool flag2 = false;
							lock (Choreographer.s_Choreographer.m_MessageQueue)
							{
								if (Choreographer.s_Choreographer.m_MessageQueue.Count > 0 && Choreographer.s_Choreographer.m_MessageQueue.Any((CMessageData m) => m.m_Type == choreoStep.ChoreographerMessageType))
								{
									flag2 = true;
								}
							}
							if (flag2)
							{
								m_CurrentEventReport = $"\t[AUTOTEST PLAYBACK] Playback Issue: Current Choregrapher step does not match recorded step to playback. Played back step found in messages waiting to be processed - letting choreographer continue.\n\t=====Processed MessageTypePausedFor:{AutoTestController.s_Instance.MessageTypePausedFor} CurrentIndex:{m_CurrentIndex}";
								m_InsertMissingChoreographerSteps.Add(new CAutoStepChoreographer(AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.IndexOf(cAuto), AutoTestController.s_Instance.MessageTypePausedFor));
								AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
							}
							else if (m_EventIdWaitingToResolveFor != choreoStep.ID)
							{
								m_EventIdWaitingToResolveFor = choreoStep.ID;
								m_TimeWhenStartedWaitingForMessageQueue = Time.realtimeSinceStartup;
							}
							else
							{
								if (!(Time.realtimeSinceStartup - m_TimeWhenStartedWaitingForMessageQueue >= 3f))
								{
									break;
								}
								bool flag3 = false;
								for (int num3 = 0; num3 < 15; num3++)
								{
									if (m_Events.Count > m_CurrentIndex + num3 && m_Events[m_CurrentIndex + num3].EventType == CAuto.EAutoType.ChoreographerStep && m_Events[m_CurrentIndex + num3] is CAutoStepChoreographer cAutoStepChoreographer && cAutoStepChoreographer.ChoreographerMessageType == AutoTestController.s_Instance.MessageTypePausedFor)
									{
										flag3 = true;
										break;
									}
								}
								if (flag3)
								{
									m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: Recording contains step missing from current flow. Skipping recorded step based on forward search of recorded steps.\n\t=====Skipped recorded MessageType:" + choreoStep.ChoreographerMessageType.ToString() + " | Current Choreographer MessageType:" + AutoTestController.s_Instance.MessageTypePausedFor;
									AutoTestController.s_Instance.ChoreographerMessagesProcessed++;
									m_CurrentIndex++;
									m_RemoveEventsAndDecrementChoreographerCount.Add(cAuto);
								}
								else
								{
									m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: Recording is missing current choreographer step. Now assuming new step has been added and letting choreographer continue.\n\t=====Processed MessageType:" + AutoTestController.s_Instance.MessageTypePausedFor;
									m_InsertMissingChoreographerSteps.Add(new CAutoStepChoreographer(AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.IndexOf(cAuto), AutoTestController.s_Instance.MessageTypePausedFor));
									AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
								}
							}
						}
						else
						{
							AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
							m_CurrentIndex++;
							if (choreoStep.ID == finalChoreographerEventID)
							{
								FinalRecordedChoreographerEventOccured = true;
							}
						}
					}
					else if (CurrentLogPlayedBack.ContainsTypedChoreographerSteps && Choreographer.s_Choreographer.m_WaitState.m_State > Choreographer.ChoreographerStateType.Play)
					{
						if (m_EventCurrentlyWaitingOn == CurrentEvent)
						{
							if (Time.realtimeSinceStartup - m_TimeAtStartOfWaitingForChoreographerToBePaused > 5f)
							{
								AutoTestController.s_Instance.EndTestPlaybackFromТimeout("[AUTOTEST PLAYBACK] Playback Issue: Recording is trying to playback a choreographer step, but the choreographer is waiting for a user interaction - Skipping recorded action.\n\t=====Skipped recorded MessageType:" + choreoStep.ChoreographerMessageType);
							}
						}
						else
						{
							m_EventCurrentlyWaitingOn = CurrentEvent;
							m_TimeAtStartOfWaitingForChoreographerToBePaused = Time.realtimeSinceStartup;
						}
					}
					else if (currentIndex == m_CurrentIndex && Time.realtimeSinceStartup - timeLastMessageWasProcessed > 20f)
					{
						m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: Recording is trying to playback a choreographer step, but the choreographer is not paused and per message interval is exceeded - Skipping recorded action.\n\t=====Skipped recorded MessageType:" + choreoStep.ChoreographerMessageType.ToString() + "\nIsRestarting: " + Choreographer.s_Choreographer.IsRestarting + "\nSRL Message Queue: " + string.Join(",", ScenarioRuleClient.MessageQueueCopy.Select((ScenarioRuleClient.CSRLMessage s) => s.m_MessageType.ToString()));
						AutoTestController.s_Instance.ChoreographerMessagesProcessed++;
						m_CurrentIndex++;
						m_RemoveEventsAndDecrementChoreographerCount.Add(cAuto);
					}
					break;
				}
				case CAuto.EAutoType.AOETileHover:
				{
					if (cAuto.ChoreographerState != Choreographer.s_Choreographer.m_WaitState.m_State || (cAuto.ForTutorial && Singleton<LevelMessagesUIHandler>.Instance.DisplayDelayInEffect))
					{
						break;
					}
					CAutoAOETileHover currentEvent = cAuto as CAutoAOETileHover;
					CurrentEvent = currentEvent;
					if (WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.AreaOfEffect)
					{
						WorldspaceStarHexDisplay.Instance.DisplayAOEStars();
					}
					else
					{
						if (WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType != WorldspaceStarHexDisplay.EAbilityDisplayType.SelectObjectPositionAreaOfEffect)
						{
							break;
						}
						WorldspaceStarHexDisplay.Instance.DisplayAOEStars();
					}
					m_CurrentIndex++;
					break;
				}
				case CAuto.EAutoType.AOERotate:
					if (cAuto.ChoreographerState == Choreographer.s_Choreographer.m_WaitState.m_State && (!cAuto.ForTutorial || !Singleton<LevelMessagesUIHandler>.Instance.DisplayDelayInEffect))
					{
						CAutoAOERotate cAutoAOERotate = (CAutoAOERotate)(CurrentEvent = cAuto as CAutoAOERotate);
						if (WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.AreaOfEffect || WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.SelectObjectPositionAreaOfEffect)
						{
							WorldspaceStarHexDisplay.Instance.RotateAOEClockwise(cAutoAOERotate.IsClockWise);
							m_CurrentIndex++;
						}
					}
					break;
				default:
					m_CurrentEventReport = "[AUTOTEST PLAYBACK] Playback Issue: Invalid AutoLog event type:" + cAuto.EventType.ToString() + " - Skipping";
					m_CurrentIndex++;
					m_RemoveEvents.Add(CurrentEvent);
					break;
				}
				if (m_CurrentIndex == currentIndex && m_CurrentIndex == 0 && Time.realtimeSinceStartup - timeLastMessageWasProcessed > 10f && AutoTestController.s_ChoreographerPaused)
				{
					AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
					m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: Unable to start playback due to backed up Choregrapher message required to commence playback. Unpausing Choregrapher";
				}
			}
			if (!CurrentLogPlayedBack.ContainsTypedChoreographerSteps && currentIndex == m_CurrentIndex && Time.realtimeSinceStartup - timeLastMessageWasProcessed > 20f)
			{
				int num4 = 0;
				lock (Choreographer.s_Choreographer.m_MessageQueue)
				{
					num4 = Choreographer.s_Choreographer.m_MessageQueue.Count;
				}
				if (AutoTestController.s_ChoreographerPaused && num4 > 0)
				{
					m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: In state where we have an unexpected Choreographer step - Letting choreographer continue";
					m_InsertMissingChoreographerSteps.Add(new CAutoStepChoreographer(AutoTestController.s_Instance.CurrentAutoTestData.RecordedUIActions.Events.IndexOf(cAuto), AutoTestController.s_Instance.MessageTypePausedFor));
					AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
				}
				else if (cAuto.ChoreographerStepsCompleted > AutoTestController.s_Instance.ChoreographerMessagesProcessed || (cAuto.EventType == CAuto.EAutoType.ChoreographerStep && !AutoTestController.s_ChoreographerPaused))
				{
					if (cAuto.ID == finalChoreographerEventID)
					{
						FinalRecordedChoreographerEventOccured = true;
					}
					m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: Expecting a choreographer step that does not exist currently - manually skipping over playback step";
					AutoTestController.s_Instance.ChoreographerMessagesProcessed++;
					m_CurrentIndex++;
					m_RemoveEventsAndDecrementChoreographerCount.Add(cAuto);
				}
				else
				{
					m_CurrentEventReport = "\t[AUTOTEST PLAYBACK] Playback Issue: Unable to Resolve Playback error - Ending Test";
					FinalRecordedChoreographerEventOccured = true;
				}
			}
			if (!string.IsNullOrEmpty(m_CurrentEventReport))
			{
				Log.Warning(m_CurrentEventReport);
				AutotestPlaybackReport = AutotestPlaybackReport + (string.IsNullOrEmpty(AutotestPlaybackReport) ? "" : "\n") + m_CurrentEventReport;
			}
			if (currentIndex != m_CurrentIndex)
			{
				timeLastMessageWasProcessed = Time.realtimeSinceStartup;
			}
			if (cAuto2 == null || cAuto.EventType == CAuto.EAutoType.ButtonHover || cAuto.EventType == CAuto.EAutoType.ChoreographerStep || cAuto.EventType == CAuto.EAutoType.AOETileHover)
			{
				yield return null;
			}
			else if (m_UseRealtime)
			{
				yield return new WaitForSecondsRealtime(0.3f);
			}
			else
			{
				yield return Timekeeper.instance.WaitForSeconds(0.3f);
			}
		}
	}

	private bool GetButton(List<AutoLogUtility.ButtonAndOrder> hierarchy, out GameObject button, bool activeInHierarchyRequired = true, bool interactivityRequired = true, bool buttonRequired = true)
	{
		button = null;
		try
		{
			if (hierarchy != null && hierarchy.Count > 0)
			{
				GameObject gameObject = GameObject.Find(hierarchy[0].ButtonName);
				if (hierarchy.Count == 1)
				{
					button = gameObject;
					return button != null;
				}
				if (hierarchy.Exists((AutoLogUtility.ButtonAndOrder e) => e.ButtonName == "Take Damage Panel") && !Singleton<TakeDamagePanel>.Instance.IsOpen)
				{
					return false;
				}
				GameObject gameObject2 = null;
				if (gameObject.name == "Overlay Canvas")
				{
					gameObject2 = gameObject.transform.Find("Game Elements").gameObject;
				}
				for (int num = 1; num < hierarchy.Count; num++)
				{
					if (gameObject != null)
					{
						gameObject = AutoLogUtility.FindInTransform(gameObject.transform, hierarchy[num]);
					}
					if (gameObject2 != null)
					{
						gameObject2 = AutoLogUtility.FindInTransform(gameObject2.transform, hierarchy[num]);
					}
				}
				if (gameObject == null && gameObject2 != null)
				{
					gameObject = gameObject2;
				}
				if (gameObject != null && (gameObject.activeInHierarchy || !activeInHierarchyRequired))
				{
					if (interactivityRequired)
					{
						Button component = gameObject.GetComponent<Button>();
						if ((object)component == null || !component.interactable)
						{
							Toggle component2 = gameObject.GetComponent<Toggle>();
							if ((object)component2 == null || !component2.interactable)
							{
								return false;
							}
						}
					}
					button = gameObject;
					return true;
				}
				if (!buttonRequired)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	private void FixButtonHierarchy(ref CAuto buttonEvent)
	{
		List<AutoLogUtility.ButtonAndOrder> list = null;
		if (buttonEvent is CAutoButtonHover cAutoButtonHover)
		{
			list = cAutoButtonHover.ButtonHierarchy;
		}
		else if (buttonEvent is CAutoButtonClick cAutoButtonClick)
		{
			list = cAutoButtonClick.ButtonHierarchy;
		}
		if (list == null)
		{
			return;
		}
		AutoLogUtility.ButtonAndOrder buttonAndOrder = list.SingleOrDefault((AutoLogUtility.ButtonAndOrder s) => s.ButtonName == "Cards Hands Manager");
		if (buttonAndOrder != null)
		{
			int num = list.IndexOf(buttonAndOrder);
			if (list[num + 1].ButtonName == "Holder")
			{
				AutoLogUtility.ButtonAndOrder item = new AutoLogUtility.ButtonAndOrder("Card Hand", 0);
				list.Insert(num + 1, item);
			}
		}
	}
}
