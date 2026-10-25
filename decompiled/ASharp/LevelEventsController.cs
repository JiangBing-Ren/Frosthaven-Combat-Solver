using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FHGUI.Common;
using GLOOM;
using I2.Loc;
using MapRuleLibrary.MapState;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class LevelEventsController : Singleton<LevelEventsController>
{
	private bool m_CurrentlyActive;

	private CCustomLevelData m_CurrentlyPlayedLevel;

	private CMapScenarioState m_CurrentlyPlayedMapState;

	private List<CLevelMessage> m_MessagesToShow = new List<CLevelMessage>();

	private List<ScenarioState.TriggeredLevelMessageInfo> m_AlreadyShownMessagesBackup = new List<ScenarioState.TriggeredLevelMessageInfo>();

	private List<CLevelEvent> m_LevelEventsToShow = new List<CLevelEvent>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_CustomWinObjectivesToTrigger = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_AlreadyTriggeredCustomWinObjectives = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_CustomLoseObjectivesToTrigger = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<Tuple<CObjective_CustomTrigger, int>> m_AlreadyTriggeredCustomLoseObjectives = new List<Tuple<CObjective_CustomTrigger, int>>();

	private List<SEvent> m_SEventsPostedToProcess = new List<SEvent>();

	private List<UIEvent> m_UIEventsPostedToProcess = new List<UIEvent>();

	private Dictionary<CLevelMessage, UnityAction> m_PendingInteractionLimiting = new Dictionary<CLevelMessage, UnityAction>();

	private UnityAction<CLevelMessage> m_ActionForNextMessageDismissal;

	private bool m_DebugLog;

	private string m_ShortRestCardDataPending;

	public GameObject StoryControllerPrefab;

	private GameObject m_StoryControllerSceneObject;

	public const string LOG_CATEGORY = "[Events]";

	public static bool s_EventsControllerActive
	{
		get
		{
			if (Singleton<LevelEventsController>.IsInitialized)
			{
				return Singleton<LevelEventsController>.Instance.m_CurrentlyActive;
			}
			return false;
		}
	}

	private List<ScenarioState.TriggeredLevelMessageInfo> m_AlreadyShownMessages
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState?.TriggeredLevelMessages != null)
			{
				return ScenarioManager.CurrentScenarioState.TriggeredLevelMessages;
			}
			return m_AlreadyShownMessagesBackup;
		}
	}

	public int NumberOfAlreadyDisplayedMessages => m_AlreadyShownMessages.Count;

	public List<CLevelMessage> LevelMessages => m_MessagesToShow;

	public bool IsBusy
	{
		get
		{
			if (m_CurrentlyActive)
			{
				if (m_UIEventsPostedToProcess.Count <= 0)
				{
					return m_SEventsPostedToProcess.Count > 0;
				}
				return true;
			}
			return false;
		}
	}

	protected override void AwakeInit()
	{
		InteractabilityManager.s_Instance?.LoadDefaultMessagelessProfile();
		if (ScenarioManager.CurrentScenarioState != null)
		{
			SEventLog.ClearEventLog();
		}
	}

	protected override void AwakenedDestroy()
	{
		InteractabilityManager.s_Instance?.LoadProfile(null);
		UIEventManager.OnEventAdded -= UIEventLogged;
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		m_CurrentlyPlayedLevel = SaveData.Instance.Global.CurrentCustomLevelData;
		if (m_CurrentlyPlayedLevel == null)
		{
			m_CurrentlyPlayedLevel = SaveData.Instance.Global.CurrentAutoTestDataCopy;
		}
		bool num = m_CurrentlyPlayedLevel != null && (m_CurrentlyPlayedLevel.HasScriptedEvents || SaveData.Instance.Global.GameMode == EGameMode.SingleScenario);
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState != null)
		{
			m_CurrentlyPlayedMapState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState;
		}
		else
		{
			m_CurrentlyPlayedMapState = null;
		}
		if (!num)
		{
			m_CurrentlyPlayedLevel = null;
		}
		if (num || m_CurrentlyPlayedMapState != null)
		{
			m_CurrentlyActive = true;
			Log.Info("[Events] LevelEventsController set to active");
			UIEventManager.OnEventAdded -= UIEventLogged;
			UIEventManager.OnEventAdded += UIEventLogged;
			StartListeningForEvents();
		}
	}

	public void RemoveAlreadyPlayedMessages()
	{
		if (m_AlreadyShownMessages == null || m_AlreadyShownMessages.Count <= 0)
		{
			return;
		}
		m_MessagesToShow.RemoveAll((CLevelMessage x) => !x.CanRepeat && m_AlreadyShownMessages.Any((ScenarioState.TriggeredLevelMessageInfo y) => y.MessageName == x.MessageName));
	}

	private void AssignPortraitToMessageFromPageKeys(CLevelMessage message)
	{
		foreach (CLevelMessagePage page in message.Pages)
		{
			StoryCharacterConfigUI narrativeStoryCharacter = UIInfoTools.Instance.GetNarrativeStoryCharacter(page.PageTextKey);
			if (narrativeStoryCharacter != null)
			{
				page.SpeakingCharacterGUID = narrativeStoryCharacter.CharacterGUID;
				page.SpeakingCharacterExpression = 0;
			}
		}
	}

	public void StartListeningForEvents()
	{
		m_SEventsPostedToProcess.Clear();
		m_UIEventsPostedToProcess.Clear();
		m_MessagesToShow.Clear();
		m_LevelEventsToShow.Clear();
		m_CustomWinObjectivesToTrigger.Clear();
		m_AlreadyTriggeredCustomWinObjectives.Clear();
		m_CustomLoseObjectivesToTrigger.Clear();
		m_AlreadyTriggeredCustomLoseObjectives.Clear();
		m_ActionForNextMessageDismissal = null;
		if (m_CurrentlyPlayedLevel != null)
		{
			if (m_CurrentlyPlayedLevel.LevelMessages != null && m_CurrentlyPlayedLevel.LevelMessages.Count > 0)
			{
				m_MessagesToShow = m_CurrentlyPlayedLevel.LevelMessages.ToList();
			}
			if (m_CurrentlyPlayedLevel.LevelEvents != null && m_CurrentlyPlayedLevel.LevelEvents.Count > 0)
			{
				m_LevelEventsToShow = m_CurrentlyPlayedLevel.LevelEvents.ToList();
			}
			for (int i = 0; i < m_CurrentlyPlayedLevel.ScenarioState.WinObjectives.Count; i++)
			{
				if (m_CurrentlyPlayedLevel.ScenarioState.WinObjectives[i].ObjectiveType == EObjectiveType.CustomTrigger)
				{
					m_CustomWinObjectivesToTrigger.Add(new Tuple<CObjective_CustomTrigger, int>(m_CurrentlyPlayedLevel.ScenarioState.WinObjectives[i] as CObjective_CustomTrigger, i));
				}
			}
			for (int j = 0; j < m_CurrentlyPlayedLevel.ScenarioState.LoseObjectives.Count; j++)
			{
				if (m_CurrentlyPlayedLevel.ScenarioState.LoseObjectives[j].ObjectiveType == EObjectiveType.CustomTrigger)
				{
					m_CustomLoseObjectivesToTrigger.Add(new Tuple<CObjective_CustomTrigger, int>(m_CurrentlyPlayedLevel.ScenarioState.LoseObjectives[j] as CObjective_CustomTrigger, j));
				}
			}
		}
		if (m_CurrentlyPlayedMapState != null)
		{
			if (m_CurrentlyPlayedMapState.ScenarioStartMessage != null)
			{
				m_MessagesToShow.Add(m_CurrentlyPlayedMapState.ScenarioStartMessage);
			}
			if (m_CurrentlyPlayedMapState.ScenarioCompleteMessage != null)
			{
				m_MessagesToShow.Add(m_CurrentlyPlayedMapState.ScenarioCompleteMessage);
			}
			if (m_CurrentlyPlayedMapState.ScenarioRoomRevealMessages != null)
			{
				m_MessagesToShow.AddRange(m_CurrentlyPlayedMapState.ScenarioRoomRevealMessages);
			}
			if (SaveData.Instance.Global.GameMode != EGameMode.Campaign)
			{
				return;
			}
			bool flag = false;
			int num = 0;
			List<string> list = new List<string>();
			while (!flag)
			{
				num++;
				string term = string.Format(m_CurrentlyPlayedMapState.LocalisedStartKey, num);
				if (GLOOM.LocalizationManager.TryGetTranslation(term, out var _))
				{
					TermData termData = I2.Loc.LocalizationManager.GetTermData(term);
					list.Add(termData.Term);
				}
				else
				{
					flag = true;
				}
			}
			if (list.Count > 0)
			{
				CLevelMessage cLevelMessage = CLevelMessage.CreateLevelMessageForCampaign(list, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioStart);
				AssignPortraitToMessageFromPageKeys(cLevelMessage);
				m_MessagesToShow.Add(cLevelMessage);
			}
			flag = false;
			num = 0;
			list.Clear();
			while (!flag)
			{
				num++;
				string term2 = string.Format(m_CurrentlyPlayedMapState.LocalisedSuccessKey, num);
				if (GLOOM.LocalizationManager.TryGetTranslation(term2, out var _))
				{
					TermData termData2 = I2.Loc.LocalizationManager.GetTermData(term2);
					list.Add(termData2.Term);
				}
				else
				{
					flag = true;
				}
			}
			if (list.Count > 0)
			{
				CLevelMessage cLevelMessage2 = CLevelMessage.CreateLevelMessageForCampaign(list, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioEnd);
				AssignPortraitToMessageFromPageKeys(cLevelMessage2);
				m_MessagesToShow.Add(cLevelMessage2);
			}
			{
				foreach (CMap map in m_CurrentlyPlayedMapState.CurrentState.Maps)
				{
					flag = false;
					num = 0;
					list.Clear();
					while (!flag)
					{
						num++;
						string term3 = string.Format(m_CurrentlyPlayedMapState.LocalisedOpenRoomKey, map.RoomName, num);
						if (GLOOM.LocalizationManager.TryGetTranslation(term3, out var _))
						{
							TermData termData3 = I2.Loc.LocalizationManager.GetTermData(term3);
							list.Add(termData3.Term);
						}
						else
						{
							flag = true;
						}
					}
					if (list.Count > 0)
					{
						CLevelMessage cLevelMessage3 = CLevelMessage.CreateLevelMessageForCampaign(list, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.RoomRevealed, map.MapGuid);
						AssignPortraitToMessageFromPageKeys(cLevelMessage3);
						m_MessagesToShow.Add(cLevelMessage3);
					}
				}
				return;
			}
		}
		if (m_CurrentlyPlayedLevel == null)
		{
			return;
		}
		bool flag2 = false;
		int num2 = 0;
		List<string> list2 = new List<string>();
		while (!flag2)
		{
			num2++;
			string term4 = string.Format(m_CurrentlyPlayedLevel.Name + "_Start_{0}", num2);
			if (GLOOM.LocalizationManager.TryGetTranslation(term4, out var _))
			{
				TermData termData4 = I2.Loc.LocalizationManager.GetTermData(term4);
				list2.Add(termData4.Term);
				continue;
			}
			term4 = string.Format(m_CurrentlyPlayedLevel.Name + "_Start_{0}", num2);
			if (GLOOM.LocalizationManager.TryGetTranslation(term4, out var _))
			{
				TermData termData5 = I2.Loc.LocalizationManager.GetTermData(term4);
				list2.Add(termData5.Term);
			}
			else
			{
				flag2 = true;
			}
		}
		if (list2.Count > 0)
		{
			CLevelMessage cLevelMessage4 = CLevelMessage.CreateLevelMessageForCampaign(list2, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioStart);
			AssignPortraitToMessageFromPageKeys(cLevelMessage4);
			m_MessagesToShow.Add(cLevelMessage4);
		}
		flag2 = false;
		num2 = 0;
		list2.Clear();
		while (!flag2)
		{
			num2++;
			string term5 = string.Format(m_CurrentlyPlayedLevel.Name + "_Success_{0}", num2);
			if (GLOOM.LocalizationManager.TryGetTranslation(term5, out var _))
			{
				TermData termData6 = I2.Loc.LocalizationManager.GetTermData(term5);
				list2.Add(termData6.Term);
				continue;
			}
			term5 = string.Format(m_CurrentlyPlayedLevel.Name + "_Success_{0}", num2);
			if (GLOOM.LocalizationManager.TryGetTranslation(term5, out var _))
			{
				TermData termData7 = I2.Loc.LocalizationManager.GetTermData(term5);
				list2.Add(termData7.Term);
			}
			else
			{
				flag2 = true;
			}
		}
		if (list2.Count > 0)
		{
			CLevelMessage cLevelMessage5 = CLevelMessage.CreateLevelMessageForCampaign(list2, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.ScenarioEnd);
			AssignPortraitToMessageFromPageKeys(cLevelMessage5);
			m_MessagesToShow.Add(cLevelMessage5);
		}
		foreach (CMap map2 in m_CurrentlyPlayedLevel.ScenarioState.Maps)
		{
			flag2 = false;
			num2 = 0;
			list2.Clear();
			while (!flag2)
			{
				num2++;
				string term6 = string.Format(m_CurrentlyPlayedLevel.Name + "_{0}_{1}", map2.RoomName, num2);
				if (GLOOM.LocalizationManager.TryGetTranslation(term6, out var _))
				{
					TermData termData8 = I2.Loc.LocalizationManager.GetTermData(term6);
					list2.Add(termData8.Term);
					continue;
				}
				term6 = string.Format(m_CurrentlyPlayedLevel.Name + "_{0}_{1}", map2.RoomName, num2);
				if (GLOOM.LocalizationManager.TryGetTranslation(term6, out var _))
				{
					TermData termData9 = I2.Loc.LocalizationManager.GetTermData(term6);
					list2.Add(termData9.Term);
				}
				else
				{
					flag2 = true;
				}
			}
			if (list2.Count > 0)
			{
				CLevelMessage cLevelMessage6 = CLevelMessage.CreateLevelMessageForCampaign(list2, CLevelTrigger.ELevelMessagePredefinedDisplayTrigger.RoomRevealed, map2.MapGuid);
				AssignPortraitToMessageFromPageKeys(cLevelMessage6);
				m_MessagesToShow.Add(cLevelMessage6);
			}
		}
	}

	private void UIEventLogged(UIEvent eventToProcess)
	{
		if (!m_CurrentlyActive)
		{
			Log.WarningFormat(string.Format("{0} LevelEventsController received UIEvent while it shouldn't have. EventType received:{1}", "[Events]", eventToProcess.EventType));
		}
		else
		{
			m_UIEventsPostedToProcess.Add(eventToProcess);
		}
	}

	private void DebugLogSEvent(SEvent eventToLog)
	{
		string text = $"[Events] LEVEL EVENTS CONTROLLER SEVENT OF TYPE [{eventToLog.Type.ToString()}] FROM ROUND [{eventToLog.Round}] PROCESSED IN ROUND [{ScenarioManager.CurrentScenarioState.RoundNumber}]:";
		switch (eventToLog.Type)
		{
		case ESEType.Internal:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventInternal).SubType;
			break;
		case ESEType.Phase:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventPhase).SubType;
			text = text + "\nPHASE: " + (eventToLog as SEventPhase).SubTypePhase;
			break;
		case ESEType.Actor:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventActor).ActorSubType;
			text = text + "\nACTOR: " + (eventToLog as SEventActor).MainActorClassID;
			break;
		case ESEType.Item:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventItem).ItemSubType;
			text = text + "\nITEM: " + (eventToLog as SEventItem).ItemType;
			break;
		case ESEType.Action:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventAction).SubTypeAction;
			text = text + "\nTEXT: " + (eventToLog as SEventAction).Text;
			break;
		case ESEType.Ability:
			text = text + "\nSUBTYPE: " + (eventToLog as SEventAbility).AbilitySubType;
			text = text + "\nABILITY: " + (eventToLog as SEventAbility).AbilityType;
			if ((eventToLog as SEventAbility).AbilityType == CAbility.EAbilityType.Move)
			{
				text = text + "\nMOVESTATE: " + (eventToLog as SEventAbilityMove).MoveState;
			}
			else if (eventToLog is SEventAbilityAttack sEventAbilityAttack)
			{
				text = text + "\nATTACKSTATE: " + sEventAbilityAttack.AttackState;
			}
			break;
		case ESEType.AttackModifier:
			text = text + "\nTEXT: " + (eventToLog as SEventAttackModifier).Text;
			break;
		}
	}

	private void DebugLogUIEvent(UIEvent eventToLog)
	{
		string text = "[Events] LEVEL EVENTS CONTROLLER UIEVENT PROCESSED IN ROUND [" + eventToLog.RoundInt + "]:\nTYPE:" + eventToLog.EventType.ToString() + "\nPHASE:" + eventToLog.CurrentPhaseType.ToString() + "\nCURRENT PHASE ACTOR:" + eventToLog.CurrentPhaseActorName;
		switch (eventToLog.EventType)
		{
		case UIEvent.EUIEventType.AbilityCardSelected:
		case UIEvent.EUIEventType.CardTopHalfSelected:
		case UIEvent.EUIEventType.CardBottomHalfSelected:
			text = text + "\nABILITY: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.LevelMessageDismissed:
			text = text + "\nMESSAGE NAME: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.WindowOpened:
			text = text + "\nWINDOW OPENED: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.WindowClosed:
			text = text + "\nWINDOW CLOSED: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.TileSelectedForAbility:
			text = text + "\nABILITY: " + eventToLog.ContextID;
			text = text + "\nTILE: X: " + eventToLog.TileForEvent.X + " Y:" + eventToLog.TileForEvent.Y;
			text = text + "\nINPUT TYPE: " + eventToLog.InputType;
			break;
		case UIEvent.EUIEventType.InitiativeAvatarHovered:
			text = text + "\nACTOR HOVERED: " + eventToLog.ContextID;
			break;
		case UIEvent.EUIEventType.ConsumeElementCardTop:
		case UIEvent.EUIEventType.ConsumeElementCardBottom:
			text = text + "\nABILITY: " + eventToLog.ContextID + ((eventToLog.EventType == UIEvent.EUIEventType.ConsumeElementCardTop) ? " [TOP]" : " [BOTTOM]");
			text = text + "\nINDEX: " + ((eventToLog.ContextInt < 0) ? "ANY" : eventToLog.ContextInt.ToString());
			break;
		}
	}

	public void TickEvents(bool initial = false, bool endScenario = false)
	{
		if (!m_CurrentlyActive || ScenarioManager.CurrentScenarioState == null || GameState.IsInLevelEditor)
		{
			return;
		}
		try
		{
			List<SEvent> list = SEventLog.CacheUnprocessedEvents()?.ToList();
			if (list != null && list.Count > 0)
			{
				m_SEventsPostedToProcess.AddRange(list);
				bool clear = SEventLog.FindAllEventsOfType<SEventActorEndRound>(onlyUnprocessed: true, toScrape: false, checkQueue: true).Count > 0;
				SEventLog.ClearUnprocessedEventsCache();
				StatsDataStorage.ScrapeEventLogAndCheckBattleGoals(EResult.InProgress, initial, endScenario, list, ScenarioManager.CurrentScenarioState.RoundNumber, clear);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception);
		}
		try
		{
			if (m_SEventsPostedToProcess.Count > 0)
			{
				long num = DateTime.Now.Ticks / 10000;
				while (m_SEventsPostedToProcess.Count > 0 && DateTime.Now.Ticks / 10000 - num < 8)
				{
					if (m_SEventsPostedToProcess[0] != null)
					{
						ProcessEvent(m_SEventsPostedToProcess[0]);
					}
					m_SEventsPostedToProcess.RemoveAt(0);
				}
			}
			else if (m_UIEventsPostedToProcess.Count > 0)
			{
				if (m_UIEventsPostedToProcess[0] != null)
				{
					ProcessEvent(null, m_UIEventsPostedToProcess[0]);
				}
				m_UIEventsPostedToProcess.RemoveAt(0);
			}
		}
		catch (Exception exception2)
		{
			Log.Exception(exception2);
		}
	}

	private void ProcessEvent(SEvent SEventToProcess = null, UIEvent UIEventToProcess = null)
	{
		bool flag = SEventToProcess != null;
		if (m_DebugLog)
		{
			if (flag)
			{
				DebugLogSEvent(SEventToProcess);
			}
			else
			{
				DebugLogUIEvent(UIEventToProcess);
			}
		}
		if (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage != null)
		{
			CActor contextActor = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedBoxMessage.DismissTrigger, out contextActor) : ShouldEventCauseTrigger(UIEventToProcess, Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedBoxMessage.DismissTrigger, out contextActor))
			{
				Singleton<LevelMessagesUIHandler>.Instance.HideCurrentlyShownBoxMessage();
			}
		}
		if (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedHelpTextMessage != null)
		{
			CActor contextActor2 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedHelpTextMessage.DismissTrigger, out contextActor2) : ShouldEventCauseTrigger(UIEventToProcess, Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedHelpTextMessage.DismissTrigger, out contextActor2))
			{
				Singleton<LevelMessagesUIHandler>.Instance.HideCurrentlyShownHelpTextMessage();
			}
		}
		List<CLevelMessage> list = new List<CLevelMessage>();
		foreach (CLevelMessage item in m_MessagesToShow)
		{
			CActor contextActor3 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item.DisplayTrigger, out contextActor3) : ShouldEventCauseTrigger(UIEventToProcess, item.DisplayTrigger, out contextActor3))
			{
				list.Add(item);
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
				{
					if (scenarioScript.AllowTriggerMessage(SEventToProcess, item))
					{
						list.Add(item);
						break;
					}
				}
			}
		}
		if (SEventToProcess is SEventCampaignSectionTriggered sEventCampaignSectionTriggered)
		{
			int num = 0;
			List<string> list2 = new List<string>();
			while (true)
			{
				num++;
				string text = string.Format(CMapScenarioState.LocalizedCampaignSectionKey, sEventCampaignSectionTriggered.SectionID, num);
				if (!GLOOM.LocalizationManager.TryGetTranslation(text, out var _))
				{
					break;
				}
				list2.Add(text);
			}
			if (list2.Count == 0)
			{
				string text2 = string.Format(CMapScenarioState.LocalizedCampaignSectionKeyShort, sEventCampaignSectionTriggered.SectionID);
				if (GLOOM.LocalizationManager.TryGetTranslation(text2, out var _))
				{
					list2.Add(text2);
				}
			}
			if (list2.Count == 0)
			{
				num = 0;
				while (true)
				{
					num++;
					string text3 = sEventCampaignSectionTriggered.SectionID + "_" + num;
					if (!GLOOM.LocalizationManager.TryGetTranslation(text3, out var _))
					{
						break;
					}
					list2.Add(text3);
				}
			}
			if (list2.Count == 0)
			{
				string sectionID = sEventCampaignSectionTriggered.SectionID;
				if (GLOOM.LocalizationManager.TryGetTranslation(sectionID, out var _))
				{
					list2.Add(sectionID);
				}
			}
			if (list2.Count > 0)
			{
				CLevelMessage cLevelMessage = CLevelMessage.CreateLevelMessageForCampaign(list2, sEventCampaignSectionTriggered.SectionID, null, sEventCampaignSectionTriggered.ShouldPauseGame);
				AssignPortraitToMessageFromPageKeys(cLevelMessage);
				list.Add(cLevelMessage);
			}
			else if (GameState.IsWaitingForCampaignSection(sEventCampaignSectionTriggered.SectionID))
			{
				Log.Error("[Events] Received SEventCampaignSectionTriggered for message " + sEventCampaignSectionTriggered.SectionID + " but no keys were found for this section. Unfreezing logic thread to avoid softlock");
				GameState.SelectedContinueAbility();
			}
			else
			{
				Log.Error("[Events] Received SEventCampaignSectionTriggered for message " + sEventCampaignSectionTriggered.SectionID + " but no keys were found for this section.");
			}
		}
		foreach (CLevelMessage message in list)
		{
			if (!message.CanRepeat && m_AlreadyShownMessages.Any((ScenarioState.TriggeredLevelMessageInfo x) => x.MessageName == message.MessageName))
			{
				if (GameState.IsWaitingForCampaignSection(message.MessageName))
				{
					GameState.SelectedContinueAbility();
				}
				continue;
			}
			if (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage != null && Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedBoxMessage.DisplayTrigger?.EventTriggerNotPlayedMessageReq?.Contains(message.MessageName) == true)
			{
				Singleton<LevelMessagesUIHandler>.Instance.HideCurrentlyShownBoxMessage();
			}
			if (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedHelpTextMessage != null && Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedHelpTextMessage.DisplayTrigger?.EventTriggerNotPlayedMessageReq?.Contains(message.MessageName) == true)
			{
				Singleton<LevelMessagesUIHandler>.Instance.HideCurrentlyShownHelpTextMessage();
			}
			foreach (ScenarioScript scenarioScript2 in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				scenarioScript2.OnMessagetrigger(message);
			}
			ShowLevelMessage(message);
			m_AlreadyShownMessages.Add(new ScenarioState.TriggeredLevelMessageInfo
			{
				MessageName = message.MessageName,
				ShownInRound = ScenarioManager.CurrentScenarioState.RoundNumber
			});
			if (!message.CanRepeat)
			{
				m_MessagesToShow.RemoveAll((CLevelMessage x) => x.MessageName == message.MessageName);
			}
		}
		List<Tuple<CLevelEvent, CActor>> list3 = new List<Tuple<CLevelEvent, CActor>>();
		foreach (CLevelEvent item2 in m_LevelEventsToShow)
		{
			CActor contextActor4 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item2.DisplayTrigger, out contextActor4) : ShouldEventCauseTrigger(UIEventToProcess, item2.DisplayTrigger, out contextActor4))
			{
				list3.Add(new Tuple<CLevelEvent, CActor>(item2, contextActor4));
			}
		}
		foreach (Tuple<CLevelEvent, CActor> item3 in list3)
		{
			RunLevelEvent(item3.Item1, item3.Item2);
			if (!item3.Item1.Repeats)
			{
				m_LevelEventsToShow.Remove(item3.Item1);
			}
		}
		List<Tuple<CObjective_CustomTrigger, int>> list4 = new List<Tuple<CObjective_CustomTrigger, int>>();
		foreach (Tuple<CObjective_CustomTrigger, int> item4 in m_CustomWinObjectivesToTrigger)
		{
			CActor contextActor5 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item4.Item1.CustomTrigger, out contextActor5) : ShouldEventCauseTrigger(UIEventToProcess, item4.Item1.CustomTrigger, out contextActor5))
			{
				list4.Add(item4);
			}
		}
		bool flag2 = false;
		foreach (Tuple<CObjective_CustomTrigger, int> item5 in list4)
		{
			SetCustomObjectiveTriggered(item5, isWinObjective: true);
			flag2 = true;
			m_AlreadyTriggeredCustomWinObjectives.Add(item5);
			m_CustomWinObjectivesToTrigger.Remove(item5);
		}
		List<Tuple<CObjective_CustomTrigger, int>> list5 = new List<Tuple<CObjective_CustomTrigger, int>>();
		foreach (Tuple<CObjective_CustomTrigger, int> item6 in m_CustomLoseObjectivesToTrigger)
		{
			CActor contextActor6 = null;
			if (flag ? ShouldEventCauseTrigger(SEventToProcess, item6.Item1.CustomTrigger, out contextActor6) : ShouldEventCauseTrigger(UIEventToProcess, item6.Item1.CustomTrigger, out contextActor6))
			{
				list5.Add(item6);
			}
		}
		foreach (Tuple<CObjective_CustomTrigger, int> item7 in list5)
		{
			SetCustomObjectiveTriggered(item7, isWinObjective: false);
			flag2 = true;
			m_AlreadyTriggeredCustomLoseObjectives.Add(item7);
			m_CustomLoseObjectivesToTrigger.Remove(item7);
		}
		if (flag2)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
		if (SEventToProcess != null)
		{
			UIContextualTutorialModule.OnEvent(SEventToProcess);
		}
		if (UIEventToProcess != null)
		{
			UIContextualTutorialModule.OnEvent(UIEventToProcess);
		}
	}

	private bool ShouldEventCauseTrigger(SEvent eventToCheck, CLevelTrigger triggerToCheck, out CActor contextActor)
	{
		contextActor = null;
		if (triggerToCheck.IsUIEventTypeTrigger)
		{
			return false;
		}
		if (triggerToCheck.IsTriggeredByDismiss)
		{
			return false;
		}
		if (triggerToCheck.EventTriggerRound != 0 && triggerToCheck.EventTriggerRound != eventToCheck.Round)
		{
			return false;
		}
		if (eventToCheck.Type != ESEType.Actor && !string.IsNullOrEmpty(triggerToCheck.EventTriggerActorGuid) && !string.IsNullOrEmpty(eventToCheck.CurrentPhaseActorGuid) && !string.Equals(triggerToCheck.EventTriggerActorGuid, eventToCheck.CurrentPhaseActorGuid, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (!CheckRequiredMessagesPlayed(triggerToCheck))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(triggerToCheck.EventAliveActorGUID))
		{
			CActor cActor = ScenarioManager.Scenario.FindActor(triggerToCheck.EventAliveActorGUID);
			if (cActor == null || cActor.IsDead)
			{
				return false;
			}
		}
		if (eventToCheck.Type == (ESEType)triggerToCheck.EventTriggerTypeInt)
		{
			try
			{
				switch (eventToCheck.Type)
				{
				case ESEType.CustomSEvent:
					return eventToCheck.Text == triggerToCheck.EventTriggerContextId;
				case ESEType.Internal:
					if ((eventToCheck as SEventInternal).SubType == (ESESubTypeInternal)triggerToCheck.EventTriggerSubTypeInt)
					{
						return true;
					}
					return false;
				case ESEType.Phase:
					if (((eventToCheck as SEventPhase).SubTypePhase == (ESESubTypePhase)triggerToCheck.EventTriggerSubTypeInt || triggerToCheck.EventTriggerSubTypeInt == 0) && ((eventToCheck as SEventPhase).SubType == (CPhase.PhaseType)triggerToCheck.EventTriggerContextTypeInt || triggerToCheck.EventTriggerContextTypeInt == 9))
					{
						return true;
					}
					return false;
				case ESEType.Item:
					if ((eventToCheck as SEventItem).ItemSubType == (ESESubTypeItem)triggerToCheck.EventTriggerSubTypeInt && string.Equals((eventToCheck as SEventItem).ItemStringID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					return false;
				case ESEType.Action:
					if ((eventToCheck as SEventAction).SubTypeAction == (ESESubTypeAction)triggerToCheck.EventTriggerSubTypeInt)
					{
						return true;
					}
					return false;
				case ESEType.Ability:
					if ((eventToCheck as SEventAbility).AbilitySubType == (ESESubTypeAbility)triggerToCheck.EventTriggerSubTypeInt && (eventToCheck as SEventAbility).AbilityType == (CAbility.EAbilityType)triggerToCheck.EventTriggerContextTypeInt)
					{
						if ((eventToCheck as SEventAbility).AbilityType == CAbility.EAbilityType.Move)
						{
							if (triggerToCheck.EventTriggerContextSubTypeInt == 0 || (eventToCheck as SEventAbilityMove).MoveState == (CAbilityMove.EMoveState)triggerToCheck.EventTriggerContextSubTypeInt)
							{
								return true;
							}
							return false;
						}
						if ((eventToCheck as SEventAbility).AbilityType == CAbility.EAbilityType.Attack)
						{
							if (triggerToCheck.EventTriggerContextSubTypeInt == 0 || (eventToCheck as SEventAbilityAttack).AttackState == (CAbilityAttack.EAttackState)triggerToCheck.EventTriggerContextSubTypeInt)
							{
								return true;
							}
							return false;
						}
						return true;
					}
					return false;
				case ESEType.Actor:
					if (eventToCheck is SEventActor sEventActor && sEventActor.ActorSubType == (ESESubTypeActor)triggerToCheck.EventTriggerSubTypeInt && (string.IsNullOrEmpty(triggerToCheck.EventTriggerActorGuid) || (eventToCheck as SEventActor).MainActorGuid == triggerToCheck.EventTriggerActorGuid))
					{
						return true;
					}
					return false;
				case ESEType.ObjectProp:
					if ((eventToCheck as SEventObjectProp).ObjectPropSubType == (ESESubTypeObjectProp)triggerToCheck.EventTriggerSubTypeInt && (string.IsNullOrEmpty(triggerToCheck.EventTriggerPropGuid) || (eventToCheck as SEventObjectProp).ObjectGuid == triggerToCheck.EventTriggerPropGuid))
					{
						return true;
					}
					return false;
				case ESEType.AttackModifier:
					return false;
				default:
					if (triggerToCheck.EventTriggerTypeInt == 0)
					{
						return false;
					}
					return true;
				}
			}
			catch (Exception exception)
			{
				Log.Exception(exception);
				Log.Error(string.Format("{0} Exception matching TutorialMessageTrigger to SEvent[{1}]", "[Events]", eventToCheck.Type));
				return false;
			}
		}
		return false;
	}

	private bool ShouldEventCauseTrigger(UIEvent eventToCheck, CLevelTrigger triggerToCheck, out CActor contextActor)
	{
		contextActor = null;
		if (!triggerToCheck.IsUIEventTypeTrigger)
		{
			return false;
		}
		if (triggerToCheck.IsTriggeredByDismiss)
		{
			return false;
		}
		if (triggerToCheck.EventTriggerRound != 0 && triggerToCheck.EventTriggerRound != eventToCheck.RoundInt)
		{
			return false;
		}
		if (triggerToCheck.EventTriggerContextTypeInt != 9 && eventToCheck.CurrentPhaseType != CPhase.PhaseType.None && triggerToCheck.EventTriggerContextTypeInt != (int)eventToCheck.CurrentPhaseType)
		{
			return false;
		}
		if (eventToCheck.EventType != UIEvent.EUIEventType.InitiativeAvatarHovered && !string.IsNullOrEmpty(triggerToCheck.EventTriggerActorName) && !string.IsNullOrEmpty(eventToCheck.CurrentPhaseActorName) && !string.Equals(triggerToCheck.EventTriggerActorName, eventToCheck.CurrentPhaseActorName, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (!CheckRequiredMessagesPlayed(triggerToCheck))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(eventToCheck.ContextActorGUID))
		{
			contextActor = ScenarioManager.Scenario.FindActor(eventToCheck.ContextActorGUID);
		}
		if (!string.IsNullOrEmpty(triggerToCheck.EventAliveActorGUID))
		{
			CActor cActor = ScenarioManager.Scenario.FindActor(triggerToCheck.EventAliveActorGUID);
			if (cActor == null || cActor.IsDead)
			{
				return false;
			}
		}
		if (eventToCheck.EventType == UIEvent.EUIEventType.ActorEndedTurnOnTile && triggerToCheck.EventTriggerTile != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == eventToCheck.ContextID);
			if (actorState == null || triggerToCheck.EventFilter == null || triggerToCheck.EventFilter.IsValidTarget(actorState))
			{
				if (triggerToCheck.EventTriggerTypeInt == 32)
				{
					if (eventToCheck.TileForEvent.X == triggerToCheck.EventTriggerTile.X && eventToCheck.TileForEvent.Y == triggerToCheck.EventTriggerTile.Y)
					{
						return true;
					}
					return false;
				}
				if (triggerToCheck.EventTriggerTypeInt == 33)
				{
					if (eventToCheck.TileForEvent.X == triggerToCheck.EventTriggerTile.X && eventToCheck.TileForEvent.Y == triggerToCheck.EventTriggerTile.Y)
					{
						return false;
					}
					return true;
				}
			}
		}
		if (eventToCheck.EventType == (UIEvent.EUIEventType)triggerToCheck.EventTriggerTypeInt)
		{
			switch (eventToCheck.EventType)
			{
			case UIEvent.EUIEventType.AbilityCardSelected:
			case UIEvent.EUIEventType.CardTopHalfSelected:
			case UIEvent.EUIEventType.CardBottomHalfSelected:
			case UIEvent.EUIEventType.ActorSelected:
			case UIEvent.EUIEventType.LevelMessageDismissed:
			case UIEvent.EUIEventType.RoomRevealed:
			case UIEvent.EUIEventType.ObjectiveCompleted:
			case UIEvent.EUIEventType.PropActivated:
			case UIEvent.EUIEventType.PropDestroyed:
			case UIEvent.EUIEventType.PropDeactivated:
			case UIEvent.EUIEventType.DoorUnlocked:
			case UIEvent.EUIEventType.WindowOpened:
			case UIEvent.EUIEventType.WindowClosed:
			case UIEvent.EUIEventType.ObjectiveActivated:
				if (string.IsNullOrEmpty(eventToCheck.ContextID) || string.IsNullOrEmpty(triggerToCheck.EventTriggerContextId) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase) || eventToCheck.ContextID.ToLower().Contains(triggerToCheck.EventTriggerContextId.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty)))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.TileSelectedForAbility:
				if (eventToCheck.InputType == CInteractable.EInputType.Alternative)
				{
					return false;
				}
				if ((eventToCheck.TileForEvent == null || triggerToCheck.EventTriggerTile == null || (eventToCheck.TileForEvent.X == triggerToCheck.EventTriggerTile.X && eventToCheck.TileForEvent.Y == triggerToCheck.EventTriggerTile.Y)) && (string.IsNullOrEmpty(eventToCheck.ContextID) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase) || eventToCheck.ContextID.ToLower().Contains(triggerToCheck.EventTriggerContextId.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty))))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.InitiativeAvatarHovered:
				if (string.IsNullOrEmpty(eventToCheck.ContextID) || string.IsNullOrEmpty(triggerToCheck.EventTriggerActorName) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerActorName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.ConsumeElementCardTop:
			case UIEvent.EUIEventType.ConsumeElementCardBottom:
				if ((string.IsNullOrEmpty(eventToCheck.ContextID) || string.IsNullOrEmpty(triggerToCheck.EventTriggerContextId) || string.Equals(eventToCheck.ContextID, triggerToCheck.EventTriggerContextId, StringComparison.OrdinalIgnoreCase)) && (eventToCheck.ContextInt < 0 || triggerToCheck.EventTriggerContextIndex < 0 || eventToCheck.ContextInt == triggerToCheck.EventTriggerContextIndex))
				{
					return true;
				}
				return false;
			case UIEvent.EUIEventType.AllActorsDeadFiltered:
			{
				CObjectiveFilter eventFilter = triggerToCheck.EventFilter;
				if (eventFilter == null)
				{
					return false;
				}
				bool result = true;
				foreach (ActorState actorState2 in ScenarioManager.CurrentScenarioState.ActorStates)
				{
					if ((!(actorState2 is EnemyState enemyState) || enemyState.ConfigPerPartySize[ScenarioManager.CurrentScenarioState.Players.Count] != ScenarioManager.EPerPartySizeConfig.Hidden) && eventFilter.IsValidTarget(actorState2))
					{
						CActor cActor2 = ScenarioManager.FindActor(actorState2.ActorGuid);
						if (cActor2 != null)
						{
							if (!cActor2.IsDead)
							{
								result = false;
								break;
							}
						}
						else if (!actorState2.IsDead)
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
			case UIEvent.EUIEventType.ActorEndedTurnOnTile:
			case UIEvent.EUIEventType.ActorDidntEndTurnOnTile:
				return false;
			default:
				return true;
			}
		}
		return false;
	}

	private bool CheckRequiredMessagesPlayed(CLevelTrigger triggerToCheck)
	{
		if (!string.IsNullOrEmpty(triggerToCheck.EventTriggerPlayedMessageReq))
		{
			List<string> list = triggerToCheck.EventTriggerPlayedMessageReq.Split(',').ToList();
			list.TrimAll();
			if (triggerToCheck.EventTriggerPlayedMessageReqOR)
			{
				bool flag = false;
				foreach (string msg in list)
				{
					if (m_AlreadyShownMessages.Any((ScenarioState.TriggeredLevelMessageInfo m) => string.Equals(m.MessageName, msg, StringComparison.OrdinalIgnoreCase)))
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			else
			{
				foreach (string msg2 in list)
				{
					if (!m_AlreadyShownMessages.Any((ScenarioState.TriggeredLevelMessageInfo m) => string.Equals(m.MessageName, msg2, StringComparison.OrdinalIgnoreCase)))
					{
						return false;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(triggerToCheck.EventTriggerNotPlayedMessageReq))
		{
			List<string> list2 = triggerToCheck.EventTriggerNotPlayedMessageReq.Split(',').ToList();
			list2.TrimAll();
			if (triggerToCheck.EventTriggerNotPlayedMessageReqOR)
			{
				bool flag2 = false;
				foreach (string msg3 in list2)
				{
					if (!m_AlreadyShownMessages.Any((ScenarioState.TriggeredLevelMessageInfo m) => string.Equals(m.MessageName, msg3, StringComparison.OrdinalIgnoreCase)))
					{
						flag2 = true;
					}
					if (flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					return false;
				}
			}
			else
			{
				foreach (string msg4 in list2)
				{
					if (m_AlreadyShownMessages.Any((ScenarioState.TriggeredLevelMessageInfo m) => string.Equals(m.MessageName, msg4, StringComparison.OrdinalIgnoreCase)))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public void ShowLevelMessage(string messageName)
	{
		CLevelMessage cLevelMessage = m_MessagesToShow.FirstOrDefault((CLevelMessage m) => m.MessageName == messageName);
		if (cLevelMessage != null)
		{
			StartCoroutine(ShowLevelMessageRoutine(cLevelMessage));
		}
	}

	public void HideLevelMessage(string messageName)
	{
		Singleton<StoryController>.Instance.Clear();
	}

	private void ShowLevelMessage(CLevelMessage message)
	{
		StartCoroutine(ShowLevelMessageRoutine(message));
	}

	private IEnumerator ShowLevelMessageRoutine(CLevelMessage message)
	{
		string text = "Cinematic_";
		string text2 = ((message.Pages.Count > 0) ? GLOOM.LocalizationManager.GetTranslation(message.Pages[0].PageTextKey) : "");
		if (message.TitleKey.StartsWith(text) || text2.StartsWith(text))
		{
			int length = text.Length;
			StartCinematic_MessageData message2 = new StartCinematic_MessageData(message.TitleKey.StartsWith(text) ? message.TitleKey.Substring(length, message.TitleKey.Length - length) : text2.Substring(length, text2.Length - length), PhaseManager.CurrentPhase.Type);
			Choreographer.s_Choreographer.PostMessageFromUI(message2);
			yield break;
		}
		if (!Choreographer.s_Choreographer.LoadingComplete)
		{
			yield return new WaitUntil(() => Choreographer.s_Choreographer.LoadingComplete);
		}
		if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
		{
			yield return new WaitUntil(() => TransitionManager.s_Instance.TransitionDone);
		}
		if (message.LayoutType != CLevelMessage.ELevelMessageLayoutType.EmptyMessageFlag)
		{
			if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.StoryDialog)
			{
				Singleton<StoryController>.Instance.Show(message);
			}
			else if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.HelpText)
			{
				Singleton<LevelMessagesUIHandler>.Instance.ShowHelpText(message);
			}
			else
			{
				Singleton<LevelMessagesUIHandler>.Instance.ShowBoxMessage(message);
			}
		}
	}

	public void CompleteLevel(bool completionSuccess, bool userQuit = false, UnityAction postCompleteAction = null)
	{
		if (!m_CurrentlyActive)
		{
			Log.Warning("[Events] Tried to complete null Level");
			return;
		}
		if (AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			SEventLog.ClearEventLog();
		}
		UnityAction<string> EndEventControllerAction = (string loggingString) =>
		{
			m_CurrentlyPlayedLevel = null;
			m_CurrentlyActive = false;
			Log.Info("[Events] - set to inactive. REASON: " + loggingString);
		};
		bool flag = false;
		if (!userQuit)
		{
			if (completionSuccess)
			{
				CLevelMessage cLevelMessage = m_MessagesToShow.FirstOrDefault((CLevelMessage m) => m.DisplayTrigger.IsUIEventTypeTrigger && m.DisplayTrigger.EventTriggerTypeInt == 30);
				if (cLevelMessage != null && CheckRequiredMessagesPlayed(cLevelMessage.DisplayTrigger))
				{
					ShowLevelMessage(cLevelMessage);
					m_AlreadyShownMessages.Add(new ScenarioState.TriggeredLevelMessageInfo
					{
						MessageName = cLevelMessage.MessageName,
						ShownInRound = ScenarioManager.CurrentScenarioState.RoundNumber
					});
					m_MessagesToShow.Remove(cLevelMessage);
					flag = true;
				}
			}
			else
			{
				CLevelMessage cLevelMessage2 = m_MessagesToShow.FirstOrDefault((CLevelMessage m) => m.DisplayTrigger.IsUIEventTypeTrigger && m.DisplayTrigger.EventTriggerTypeInt == 31);
				if (cLevelMessage2 != null && CheckRequiredMessagesPlayed(cLevelMessage2.DisplayTrigger))
				{
					ShowLevelMessage(cLevelMessage2);
					m_AlreadyShownMessages.Add(new ScenarioState.TriggeredLevelMessageInfo
					{
						MessageName = cLevelMessage2.MessageName,
						ShownInRound = ScenarioManager.CurrentScenarioState.RoundNumber
					});
					m_MessagesToShow.Remove(cLevelMessage2);
					flag = true;
				}
			}
		}
		if (flag)
		{
			m_ActionForNextMessageDismissal = (CLevelMessage messageDismissed) =>
			{
				postCompleteAction?.Invoke();
				EndEventControllerAction?.Invoke(string.Format("Ended when dismissing message with MessageName {0}. ScenarioCompletionSuccess:{1} | UserQuitManually {2}", messageDismissed?.MessageName ?? "NULL", completionSuccess.ToString(), userQuit.ToString()));
			};
		}
		else
		{
			postCompleteAction?.Invoke();
			EndEventControllerAction?.Invoke($"Ended via the CompleteLevel() method. ScenarioCompletionSuccess:{completionSuccess.ToString()} | UserQuitManually {userQuit.ToString()}");
			m_ActionForNextMessageDismissal = null;
		}
	}

	public void MessageWasDisplayed(CLevelMessage messageDisplayed, bool fromStoryController = false)
	{
		if (SaveData.Instance.Global.CurrentAdventureData?.FrosthavenCampaign?.ActiveScenario != null && SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.GameMode.Name.Contains("Tutorial", StringComparison.InvariantCultureIgnoreCase) && StatsDataTracker.Instance != null)
		{
			int num = 0;
			using List<CLevelMessage>.Enumerator enumerator = m_CurrentlyPlayedLevel.LevelMessages.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.MessageName.Equals(messageDisplayed.MessageName, StringComparison.InvariantCultureIgnoreCase))
			{
				num++;
			}
		}
		UnityAction unityAction = () =>
		{
			if (!fromStoryController)
			{
				InteractabilityManager.s_Instance.ConfigureDefaultControlsForMessage(messageDisplayed);
				Singleton<LevelMessagesUIHandler>.Instance.ConfigureIsolatedUIControlForBoxMessage(messageDisplayed);
			}
			if (messageDisplayed.InteractabilityProfileForMessage != null && messageDisplayed.InteractabilityProfileForMessage.ControlsToAllow.Count > 0)
			{
				InteractabilityManager.s_Instance.LoadProfile(messageDisplayed.InteractabilityProfileForMessage);
			}
			else if (messageDisplayed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDisplayed.DismissTrigger.IsTriggeredByDismiss)
			{
				InteractabilityManager.s_Instance.LoadDefaultDismissMessageProfile();
			}
		};
		if (InteractabilityManager.s_Instance.CurrentlyLoadedProfileIsNullOrDefault)
		{
			unityAction?.Invoke();
		}
		else
		{
			m_PendingInteractionLimiting.Add(messageDisplayed, unityAction);
		}
		if (messageDisplayed.ShouldPauseGame)
		{
			Main.Pause3DWorld();
		}
		if (messageDisplayed.CameraProfile != null && messageDisplayed.CameraProfile.ShouldForceCamera)
		{
			CameraController.s_CameraController.SetCameraWithMessageProfile(messageDisplayed.CameraProfile);
		}
	}

	public void MessageWasDismissed(CLevelMessage messageDismissed)
	{
		if (SaveData.Instance.Global.CurrentAdventureData?.FrosthavenCampaign?.ActiveScenario != null && SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.GameMode.Name.Contains("Tutorial", StringComparison.InvariantCultureIgnoreCase) && StatsDataTracker.Instance != null)
		{
			int num = 0;
			using List<CLevelMessage>.Enumerator enumerator = m_CurrentlyPlayedLevel.LevelMessages.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.MessageName.Equals(messageDismissed.MessageName, StringComparison.InvariantCultureIgnoreCase))
			{
				num++;
			}
		}
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.LevelMessageDismissed, null, messageDismissed.MessageName));
		GameState.OnLevelMessageWasDismissed(messageDismissed);
		if ((messageDismissed.InteractabilityProfileForMessage != null && messageDismissed.InteractabilityProfileForMessage.ControlsToAllow.Count > 0) || (messageDismissed.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText && messageDismissed.DismissTrigger.IsTriggeredByDismiss))
		{
			InteractabilityManager.s_Instance.LoadDefaultMessagelessProfile();
		}
		if (messageDismissed.ShouldPauseGame)
		{
			Main.Unpause3DWorld();
		}
		m_ActionForNextMessageDismissal?.Invoke(messageDismissed);
		m_ActionForNextMessageDismissal = null;
		ResolvePendingInteractionProfiles();
	}

	private void ResolvePendingInteractionProfiles()
	{
		if (m_PendingInteractionLimiting.Count == 0)
		{
			return;
		}
		List<CLevelMessage> list = new List<CLevelMessage>();
		foreach (CLevelMessage key in m_PendingInteractionLimiting.Keys)
		{
			list.Add(key);
			if (Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedBoxMessage == key || Singleton<LevelMessagesUIHandler>.Instance.CurrentlyDisplayedHelpTextMessage == key)
			{
				m_PendingInteractionLimiting[key]?.Invoke();
				break;
			}
		}
		foreach (CLevelMessage item in list)
		{
			m_PendingInteractionLimiting.Remove(item);
		}
	}

	private void RunLevelEvent(CLevelEvent levelEvent, CActor overrideContextActor = null)
	{
		Log.Info(string.Format("{0} Run Level Event: {1} {2} {3} {4} {5} Round: {6}", "[Events]", Log.Colorize(levelEvent.EventType.ToString(), Color.yellow), levelEvent.EventResource, levelEvent.EventSecondResource, levelEvent.EventThirdResource, levelEvent.EventFourthResource, ScenarioManager.CurrentScenarioState.RoundNumber));
		switch (levelEvent.EventType)
		{
		case CLevelEvent.ELevelEventType.RevealCharacterAfterAnim:
		{
			if (!ScenarioManager.CurrentScenarioState.Players.Any((PlayerState p) => p.ClassID == levelEvent.EventResource))
			{
				break;
			}
			PlayerState playerToReveal = ScenarioManager.CurrentScenarioState.Players.Single((PlayerState p) => p.ClassID == levelEvent.EventResource);
			UnityAction actionToPerform = () =>
			{
				if (LevelEditorController.RevealHiddenCharacterInState(playerToReveal))
				{
					Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
					InitiativeTrack.Refresh(forceSelectFirstActor: true);
				}
			};
			StartCoroutine(WaitBeforePerformingAction(levelEvent.EventFloatResource, actionToPerform));
			if (string.IsNullOrEmpty(levelEvent.EventSecondResource))
			{
				break;
			}
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[playerToReveal.Location.X, playerToReveal.Location.Y];
			GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Player, levelEvent.EventSecondResource);
			if (characterPrefabFromBundle != null)
			{
				try
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(characterPrefabFromBundle, cClientTile.m_GameObject.transform.parent);
					gameObject2.transform.position = cClientTile.m_GameObject.transform.position;
					PlayableDirector componentInChildren2 = gameObject2.GetComponentInChildren<PlayableDirector>();
					TimelineAsset asset = gameObject2.GetComponentInChildren<TimelineAssets>().FindTimelineAsset("RevealCharacterTimeline");
					componentInChildren2.Play(asset);
					break;
				}
				catch (Exception exception)
				{
					Log.Exception(exception);
					Log.Error("[Events] Reveal Character anim not setup as expected. Failed with exception");
					break;
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SetShortRestCard:
			m_ShortRestCardDataPending = levelEvent.EventResource;
			break;
		case CLevelEvent.ELevelEventType.SetCharacterSelectedRoundCards:
		{
			if (!ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor p) => p.CharacterClass.CharacterID == levelEvent.EventResource))
			{
				break;
			}
			Log.Info("[Events] Found player in scenario to set selected cards");
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.Single((CPlayerActor p) => p.CharacterClass.CharacterID == levelEvent.EventResource);
			if (cPlayerActor.CharacterClass.SelectedAbilityCards.Any((CAbilityCard c) => c.Name == levelEvent.EventSecondResource))
			{
				Log.Info("[Events] Found card [" + levelEvent.EventSecondResource + "] in scenario to set selected");
				CAbilityCard cAbilityCard = cPlayerActor.CharacterClass.SelectedAbilityCards.Single((CAbilityCard c) => c.Name == levelEvent.EventSecondResource);
				ScenarioRuleClient.MoveAbilityCard(cPlayerActor.CharacterClass, cAbilityCard, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.RoundAbilityCards, "HandAbilityCards", "RoundAbilityCards", networkAction: false);
				cPlayerActor.CharacterClass.SetInitiativeAbilityCard(cAbilityCard);
			}
			if (cPlayerActor.CharacterClass.SelectedAbilityCards.Any((CAbilityCard c) => c.Name == levelEvent.EventThirdResource))
			{
				Log.Info("[Events] Found card [" + levelEvent.EventThirdResource + "] in scenario to set selected");
				CAbilityCard cAbilityCard2 = cPlayerActor.CharacterClass.SelectedAbilityCards.Single((CAbilityCard c) => c.Name == levelEvent.EventThirdResource);
				ScenarioRuleClient.MoveAbilityCard(cPlayerActor.CharacterClass, cAbilityCard2, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.RoundAbilityCards, "HandAbilityCards", "RoundAbilityCards", networkAction: false);
				cPlayerActor.CharacterClass.SetSubInitiativeAbilityCard(cAbilityCard2);
			}
			ScenarioRuleClient.SortInitiative();
			InitiativeTrack.Refresh(forceSelectFirstActor: false);
			break;
		}
		case CLevelEvent.ELevelEventType.SetCameraPosition:
			if (levelEvent.EventCameraProfile != null)
			{
				CameraController.s_CameraController.SetCameraWithMessageProfile(levelEvent.EventCameraProfile);
			}
			break;
		case CLevelEvent.ELevelEventType.WinScenario:
			Choreographer.s_Choreographer.WinScenario();
			break;
		case CLevelEvent.ELevelEventType.LoseScenario:
			Choreographer.s_Choreographer.LoseScenario();
			break;
		case CLevelEvent.ELevelEventType.EditSpawner:
		{
			CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.FirstOrDefault((CSpawner s) => s.SpawnerGuid == levelEvent.EventResource);
			if (cSpawner == null || cSpawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) == ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				break;
			}
			switch (CLevelEvent.LevelEventSpawnerEditTypes.SingleOrDefault((CLevelEvent.ELevelEventSpawnerEditType t) => t.ToString() == levelEvent.EventSecondResource))
			{
			case CLevelEvent.ELevelEventSpawnerEditType.OnlyActivateSpawner:
				cSpawner.SetActive(active: true, forceDontSpawn: true);
				cSpawner.UpdateWillSpawnNextRoundIndicator(ScenarioManager.CurrentScenarioState.Players.Count);
				break;
			case CLevelEvent.ELevelEventSpawnerEditType.ActivateSpawner:
				cSpawner.SetActive(active: true);
				cSpawner.UpdateWillSpawnNextRoundIndicator(ScenarioManager.CurrentScenarioState.Players.Count);
				break;
			case CLevelEvent.ELevelEventSpawnerEditType.DeactivateSpawner:
				cSpawner.SetActive(active: false);
				break;
			case CLevelEvent.ELevelEventSpawnerEditType.TriggerSpawn:
				cSpawner.SpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: true);
				ScenarioRuleClient.SortInitiative();
				break;
			}
			break;
		}
		case CLevelEvent.ELevelEventType.UnlockDoor:
			if (levelEvent.EventSecondResource.Equals("true"))
			{
				ScenarioRuleClient.ToggleDoor(levelEvent.EventResource, openDoor: true);
			}
			else
			{
				ScenarioRuleClient.ToggleDoor(levelEvent.EventResource, openDoor: true, unlockOnly: true);
			}
			break;
		case CLevelEvent.ELevelEventType.RevealRoom:
		{
			ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.MapGuid == levelEvent.EventResource).Reveal(initial: false);
			CRevealRooms_MessageData message = new CRevealRooms_MessageData();
			Choreographer.s_Choreographer.PostMessageFromUI(message);
			break;
		}
		case CLevelEvent.ELevelEventType.CloseDoor:
		{
			bool lockDoor = levelEvent.EventSecondResource.Equals("true");
			ScenarioRuleClient.ToggleDoor(levelEvent.EventResource, openDoor: false, unlockOnly: false, lockDoor);
			break;
		}
		case CLevelEvent.ELevelEventType.DeactivateModifier:
			ScenarioRuleClient.ToggleScenarioModifierActivation(levelEvent.EventResource, activate: false);
			break;
		case CLevelEvent.ELevelEventType.ActivateModifier:
			ScenarioRuleClient.ToggleScenarioModifierActivation(levelEvent.EventResource, activate: true);
			break;
		case CLevelEvent.ELevelEventType.TriggerScenarioModifier:
			ScenarioRuleClient.TriggerScenarioModifier(levelEvent.EventResource);
			break;
		case CLevelEvent.ELevelEventType.DeactivateObjective:
		{
			CObjective cObjective2 = ScenarioManager.CurrentScenarioState.WinObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			if (cObjective2 == null)
			{
				cObjective2 = ScenarioManager.CurrentScenarioState.LoseObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			}
			if (cObjective2 != null)
			{
				cObjective2.SetActivation(active: false);
			}
			else
			{
				Log.Error("[Events] Unable to find objective with Event Identifier: " + levelEvent.EventResource + " to deactivate.");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.ActivateObjective:
		{
			CObjective cObjective = ScenarioManager.CurrentScenarioState.WinObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			if (cObjective == null)
			{
				cObjective = ScenarioManager.CurrentScenarioState.LoseObjectives.FirstOrDefault((CObjective s) => s.EventIdentifier == levelEvent.EventResource);
			}
			if (cObjective != null)
			{
				cObjective.SetActivation(active: true);
			}
			else
			{
				Log.Error("[Events] Unable to find objective with Event Identifier: " + levelEvent.EventResource + " to activate.");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnActor:
		{
			CActor cActor4 = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventSecondResource);
			if (cActor4 == null || MonsterClassManager.MonsterAndObjectClasses.SingleOrDefault((CMonsterClass x) => x.ID == levelEvent.EventResource) == null)
			{
				break;
			}
			CTile cTile = null;
			if (cActor4.IsDead)
			{
				cTile = ScenarioManager.Tiles[cActor4.ArrayIndex.X, cActor4.ArrayIndex.Y];
			}
			else
			{
				List<CTile> list = ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[cActor4.ArrayIndex.X, cActor4.ArrayIndex.Y]).FindAll((CTile x) => CAbilityFilter.IsValidTile(x, CAbilityFilter.EFilterTile.EmptyHex)).ToList();
				cTile = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
			}
			if (!Enum.TryParse<CActor.EType>(levelEvent.EventFourthResource, out var result3))
			{
				result3 = CActor.EType.Enemy;
			}
			if (cTile != null)
			{
				LevelEditorController.SpawnMonsterAtLocation(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y], levelEvent.EventResource, ScenarioManager.CurrentScenarioState.Level, result3, playSummonedAnim: true);
			}
			else
			{
				Log.Error("[Events] Unable to find tile to spawn actor to for SpawnMonsterOnActor level event");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnActor:
		{
			CActor cActor2 = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventSecondResource);
			if (cActor2 != null)
			{
				CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActor2.ArrayIndex.X, cActor2.ArrayIndex.Y];
				LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { item }, levelEvent.EventResource);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnPropOnProp:
		{
			bool flag3 = false;
			CObjectProp cObjectProp3 = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
			if (cObjectProp3 == null)
			{
				cObjectProp3 = ScenarioManager.CurrentScenarioState.ActivatedProps.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
			}
			if (cObjectProp3 == null)
			{
				cObjectProp3 = ScenarioManager.CurrentScenarioState.DestroyedProps.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
				if (cObjectProp3 != null)
				{
					flag3 = true;
				}
			}
			if (cObjectProp3 != null)
			{
				if (!flag3)
				{
					cObjectProp3.DestroyProp();
				}
				CClientTile item2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cObjectProp3.ArrayIndex.X, cObjectProp3.ArrayIndex.Y];
				LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { item2 }, levelEvent.EventResource);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnProp:
		{
			CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventSecondResource);
			if (cObjectProp == null || MonsterClassManager.MonsterAndObjectClasses.SingleOrDefault((CMonsterClass x) => x.ID == levelEvent.EventResource) == null)
			{
				break;
			}
			bool flag2 = true;
			string monsterID2 = levelEvent.EventResource;
			CClientTile tile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y];
			List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource2 = LevelEditorEventsPanel.GetPerPartySizeConfigsFromResource(levelEvent.EventThirdResource);
			if (perPartySizeConfigsFromResource2.Count == 4)
			{
				ScenarioState currentScenarioState2 = ScenarioManager.CurrentScenarioState;
				if (currentScenarioState2 != null)
				{
					_ = currentScenarioState2.Players.Count;
					if (true)
					{
						switch (perPartySizeConfigsFromResource2[ScenarioManager.CurrentScenarioState.Players.Count - 1])
						{
						case ScenarioManager.EPerPartySizeConfig.ToElite:
						{
							CMonsterClass monsterClass2 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == monsterID2);
							CMonsterClass cMonsterClass2 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == monsterClass2.ID);
							if (cMonsterClass2 != null)
							{
								monsterID2 = cMonsterClass2.ID;
							}
							break;
						}
						case ScenarioManager.EPerPartySizeConfig.Hidden:
							flag2 = false;
							break;
						}
					}
				}
			}
			if (!Enum.TryParse<CActor.EType>(levelEvent.EventFourthResource, out var result2))
			{
				result2 = CActor.EType.Enemy;
			}
			if (flag2)
			{
				cObjectProp.DestroyProp();
				LevelEditorController.SpawnMonsterAtLocation(tile2, monsterID2, ScenarioManager.CurrentScenarioState.Level, result2, playSummonedAnim: true);
				ScenarioRuleClient.SortInitiative();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SetModifierHiddenState:
		{
			CScenarioModifier cScenarioModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier d) => d.EventIdentifier == levelEvent.EventResource);
			if (cScenarioModifier != null)
			{
				bool hiddenState = levelEvent.EventSecondResource.Equals("true");
				cScenarioModifier.SetHiddenState(hiddenState);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveActiveBonusFromCurrentActor:
		{
			CActor actor2 = overrideContextActor ?? GameState.InternalCurrentActor;
			CheckForCurrentActorOverride(levelEvent, ref actor2);
			{
				foreach (CActiveBonus item3 in CActiveBonus.FindApplicableActiveBonuses(actor2))
				{
					if (item3.Ability.Name == levelEvent.EventResource || item3.Ability.Name == levelEvent.EventSecondResource || item3.Ability.Name == levelEvent.EventThirdResource)
					{
						CClass.CancelActiveBonus(item3);
					}
				}
				break;
			}
		}
		case CLevelEvent.ELevelEventType.TriggerScenarioModifierOnCurrentActor:
		{
			CActor actor = overrideContextActor ?? GameState.InternalCurrentActor;
			CheckForCurrentActorOverride(levelEvent, ref actor);
			ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier s) => s.EventIdentifier == levelEvent.EventResource)?.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, actor, ScenarioManager.CurrentScenarioState.Players.Count, forceActivate: true);
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveDifficultTerrain:
		{
			CObjectProp cObjectProp4 = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropGuid == levelEvent.EventResource);
			if (cObjectProp4 != null && cObjectProp4 is CObjectDifficultTerrain cObjectDifficultTerrain2)
			{
				cObjectDifficultTerrain2.RemoveDifficultTerrain();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.RemoveProp:
		{
			CObjectProp cObjectProp2 = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropGuid == levelEvent.EventResource);
			if (cObjectProp2 == null)
			{
				cObjectProp2 = ScenarioManager.CurrentScenarioState.ActivatedProps.FirstOrDefault((CObjectProp s) => s.PropGuid == levelEvent.EventResource);
			}
			if (cObjectProp2 != null)
			{
				if (cObjectProp2 is CObjectDifficultTerrain cObjectDifficultTerrain)
				{
					cObjectDifficultTerrain.RemoveDifficultTerrain();
				}
				else
				{
					cObjectProp2.DestroyProp();
				}
			}
			else if (ScenarioManager.CurrentScenarioState.DestroyedProps.All((CObjectProp s) => s.PropGuid != levelEvent.EventResource))
			{
				Log.GameplayError("Trying to remove prop \"" + levelEvent.EventResource + "\" that doesn't exist.");
			}
			break;
		}
		case CLevelEvent.ELevelEventType.LoseGoalChestRewardChoice:
			ScenarioManager.CurrentScenarioState.GoalChestRewards.Clear();
			break;
		case CLevelEvent.ELevelEventType.KillActor:
		{
			CActor cActor3 = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventResource);
			GameState.KillActor(cActor3, cActor3, CActor.ECauseOfDeath.Suicide, out var _);
			break;
		}
		case CLevelEvent.ELevelEventType.KillPropWithHealth:
		{
			CObjectActor cObjectActor = ScenarioManager.Scenario.AllObjects.FirstOrDefault((CObjectActor s) => s.AttachedProp?.PropGuid == levelEvent.EventResource);
			if (cObjectActor != null)
			{
				GameState.KillActor(cObjectActor, cObjectActor, CActor.ECauseOfDeath.Suicide, out var _);
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SpawnMonsterOnLastDestroyedObstacle:
		{
			CObjectProp lastDestroyedObstacle = GameState.LastDestroyedObstacle;
			if (lastDestroyedObstacle == null || MonsterClassManager.MonsterAndObjectClasses.SingleOrDefault((CMonsterClass x) => x.ID == levelEvent.EventResource) == null)
			{
				break;
			}
			bool flag = true;
			string monsterID = levelEvent.EventResource;
			CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[lastDestroyedObstacle.ArrayIndex.X, lastDestroyedObstacle.ArrayIndex.Y];
			List<ScenarioManager.EPerPartySizeConfig> perPartySizeConfigsFromResource = LevelEditorEventsPanel.GetPerPartySizeConfigsFromResource(levelEvent.EventThirdResource);
			if (perPartySizeConfigsFromResource.Count == 4)
			{
				ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
				if (currentScenarioState != null)
				{
					_ = currentScenarioState.Players.Count;
					if (true)
					{
						switch (perPartySizeConfigsFromResource[ScenarioManager.CurrentScenarioState.Players.Count - 1])
						{
						case ScenarioManager.EPerPartySizeConfig.ToElite:
						{
							CMonsterClass monsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == monsterID);
							CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass s) => s.NonEliteVariant != null && s.NonEliteVariant.ID == monsterClass.ID);
							if (cMonsterClass != null)
							{
								monsterID = cMonsterClass.ID;
							}
							break;
						}
						case ScenarioManager.EPerPartySizeConfig.Hidden:
							flag = false;
							break;
						}
					}
				}
			}
			if (!Enum.TryParse<CActor.EType>(levelEvent.EventFourthResource, out var result))
			{
				result = CActor.EType.Enemy;
			}
			if (flag)
			{
				LevelEditorController.SpawnMonsterAtLocation(tile, monsterID, ScenarioManager.CurrentScenarioState.Level, result, playSummonedAnim: true);
				ScenarioRuleClient.SortInitiative();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.SetActorAnimParameter:
		{
			CActor cActor = ScenarioManager.Scenario.AllAliveActors.FirstOrDefault((CActor s) => s.ActorGuid == levelEvent.EventResource);
			if (cActor != null)
			{
				string eventSecondResource = levelEvent.EventSecondResource;
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
				if (gameObject != null)
				{
					MF.GetGameObjectAnimator(gameObject).SetBool(eventSecondResource, value: true);
				}
			}
			break;
		}
		case CLevelEvent.ELevelEventType.TriggerPlayableDirectorOnGameObject:
		{
			PlayableDirector componentInChildren = GameObject.Find(levelEvent.EventResource).GetComponentInChildren<PlayableDirector>();
			if (componentInChildren != null)
			{
				componentInChildren.Play();
			}
			break;
		}
		case CLevelEvent.ELevelEventType.ShowMessage:
			ShowLevelMessage(m_CurrentlyPlayedLevel.LevelMessages.FirstOrDefault((CLevelMessage msg) => msg.MessageName == levelEvent.EventResource));
			break;
		}
	}

	private void CheckForCurrentActorOverride(CLevelEvent levelEvent, ref CActor actor)
	{
		if (!levelEvent.EventFourthResource.IsNOTNullOrEmpty())
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.ActorGuid == levelEvent.EventFourthResource);
		if (cActor == null)
		{
			cActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x is CObjectActor { IsAttachedToProp: not false } cObjectActor && cObjectActor.AttachedProp.PropGuid == levelEvent.EventFourthResource);
		}
		if (cActor != null)
		{
			actor = cActor;
		}
	}

	private IEnumerator WaitBeforePerformingAction(float timeToWait, UnityAction actionToPerform)
	{
		if (timeToWait > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(timeToWait);
		}
		actionToPerform?.Invoke();
	}

	public void RunActionIfShortRestDataPending(Action<string> dataPendingAction)
	{
		if (!string.IsNullOrEmpty(m_ShortRestCardDataPending))
		{
			dataPendingAction?.Invoke(m_ShortRestCardDataPending);
			m_ShortRestCardDataPending = string.Empty;
		}
	}

	private void SetCustomObjectiveTriggered(Tuple<CObjective_CustomTrigger, int> objectiveTupleToTrigger, bool isWinObjective)
	{
		if (isWinObjective)
		{
			if (ScenarioManager.CurrentScenarioState.WinObjectives[objectiveTupleToTrigger.Item2] is CObjective_CustomTrigger cObjective_CustomTrigger)
			{
				cObjective_CustomTrigger.SetObjectiveTriggered();
			}
		}
		else if (ScenarioManager.CurrentScenarioState.LoseObjectives[objectiveTupleToTrigger.Item2] is CObjective_CustomTrigger cObjective_CustomTrigger2)
		{
			cObjective_CustomTrigger2.SetObjectiveTriggered();
		}
	}
}
