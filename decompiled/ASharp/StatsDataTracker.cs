using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FFSNet;
using FH.YML;
using FHRL;
using GLOOM;
using Libs.Threading;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class StatsDataTracker : MonoBehaviour
{
	private static StatsDataTracker m_Instance;

	[NonSerialized]
	public bool m_IsCampaignModeRunning;

	[NonSerialized]
	public bool m_IsNewAdventureModeRunning;

	[NonSerialized]
	public bool m_IsEventRunning;

	[NonSerialized]
	public bool m_IsTutorialStepRunning;

	private bool m_IsNewAdventureModeMatchRunning;

	private bool m_IsCampaignModeMatchRunning;

	private CMapScenarioState m_CachedNewAdventureModeMapScenarioState;

	public static StatsDataTracker Instance => m_Instance;

	public StatsDataStorage m_StatsDataStorage { get; private set; }

	private void Awake()
	{
		Log.SharedLogEvent += SharedLogHandler;
	}

	private IEnumerator Start()
	{
		m_Instance = this;
		while (PersistentData.s_Instance == null || !PersistentData.s_Instance.IsDataLoaded || SaveData.Instance == null || SaveData.Instance.Global == null || SaveData.Instance.Global.m_StatsDataStorage == null)
		{
			yield return null;
		}
		m_StatsDataStorage = SaveData.Instance.Global.m_StatsDataStorage;
	}

	private void Update()
	{
		if (m_StatsDataStorage != null && PersistentData.s_Instance.IsDataLoaded)
		{
			m_StatsDataStorage.m_TimeLTD += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			if (m_IsNewAdventureModeMatchRunning && m_CachedNewAdventureModeMapScenarioState != null)
			{
				m_CachedNewAdventureModeMapScenarioState.TimeActiveSec += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			}
			_ = m_IsCampaignModeRunning;
			if (m_IsEventRunning)
			{
				m_StatsDataStorage.m_CurrentEventTime += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			}
			if (m_IsTutorialStepRunning)
			{
				m_StatsDataStorage.m_CurrentTutorialStepTime += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			}
		}
	}

	public void TrackNewAdventureModeMatch()
	{
		m_IsNewAdventureModeMatchRunning = true;
		m_CachedNewAdventureModeMapScenarioState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState;
		m_CachedNewAdventureModeMapScenarioState.TimeActiveSec = 0f;
	}

	public void TrackNewCampaignRun()
	{
	}

	public void TrackCampaignVictory()
	{
	}

	public void TrackCampaignGoldEarnings(int earnedOnWin, int totalEarningsInCurrentRun)
	{
	}

	public void TrackRunSessionID(string runSessionID)
	{
		if (m_StatsDataStorage != null)
		{
			m_StatsDataStorage.m_RunSessionID = runSessionID;
		}
	}

	public void TrackMatchSessionID(string matchSessionID)
	{
		if (m_StatsDataStorage != null)
		{
			m_StatsDataStorage.m_MatchSessionID = matchSessionID;
		}
	}

	public void TrackRunEnd(EGameMode gameMode, AWSessionEndReason endReason)
	{
		if (gameMode == EGameMode.Campaign)
		{
			AnalyticsWrapper.LogRunStop(AWGameMode.campaign, endReason, SaveData.Instance.Global.CurrentAdventureData, PlayerRegistry.AllPlayers.Count > 1);
		}
		m_StatsDataStorage.m_RunSessionID = null;
	}

	public void TrackMatchEnd(EResult result)
	{
		try
		{
			EGameMode gameMode = SaveData.Instance.Global.GameMode;
			if (AutoTestController.s_AutoTestCurrentlyLoaded || gameMode != EGameMode.Campaign || ScenarioManager.Scenario == null)
			{
				return;
			}
			List<string> characterStatus = ((SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState?.CurrentState == null) ? null : SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.GetCharacterStatuses());
			bool flag = SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState?.CurrentState != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.AreAllCharactersExhausted();
			CMapState adventureMapState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState;
			if (adventureMapState?.CurrentMapScenarioState != null)
			{
				ScenarioRuleLibrary.ScenarioState currentState = adventureMapState.CurrentMapScenarioState.CurrentState;
				CampaignState frosthavenCampaign = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign;
				m_IsNewAdventureModeMatchRunning = false;
				m_CachedNewAdventureModeMapScenarioState = null;
				MatchStopConfig matchStopConfig = new MatchStopConfig
				{
					m_MissionGoals = adventureMapState.CurrentMapScenarioState.CurrentState.GetAllObjectivesForAnalytics(),
					m_MissionGoalsPassed = ScenarioManager.CurrentScenarioState.GetAllCompleteObjectivesForAnalytics(),
					m_BattleGoalsFailed = (from x in adventureMapState.CurrentMapScenarioState.ChosenBattleGoalStates
						where x.Item2.BattleGoalConditionState.Failed || result != EResult.Win
						select x.Item2.ID).ToList(),
					m_BattleGoalsPassed = (from x in adventureMapState.CurrentMapScenarioState.ChosenBattleGoalStates
						where x.Item2.BattleGoalConditionState.CheckUnlocked(result == EResult.InProgressAppKill)
						select x.Item2.ID).ToList(),
					m_MasteriesFailed = (from x in adventureMapState.CurrentMapScenarioState.MasteryBattleGoalStates
						where x.Item2.BattleGoalConditionState.Failed || result != EResult.Win
						select x.Item2.ID).ToList(),
					m_MasteriesPassed = (from x in adventureMapState.CurrentMapScenarioState.MasteryBattleGoalStates
						where x.Item2.BattleGoalConditionState.CheckUnlocked(result == EResult.InProgressAppKill)
						select x.Item2.ID).ToList(),
					m_Challenges = new List<string>(),
					m_ChallengesPassed = new List<string>(),
					m_MissionID = adventureMapState.CurrentMapScenarioState.ID,
					m_RunDifficulty = ((frosthavenCampaign != null) ? frosthavenCampaign.Difficulty : adventureMapState.DifficultySetting.ToString()),
					m_PlayerCountHuman = Math.Max(1, PlayerRegistry.AllPlayers.Count),
					m_RoundCount = Choreographer.s_Choreographer.m_CurrentState.RoundNumber,
					m_RosterLevel = adventureMapState.MapParty.PartyLevel,
					m_ClassesSelected = adventureMapState.CurrentMapScenarioState.CurrentState.Players.Select((PlayerState c) => c.ClassID + "_0" + c.Level).ToList(),
					m_CharacterStatus = characterStatus,
					m_EnemyEncounters = GameState.EnemyEncounteredByClass,
					m_EnemyKilled = GameState.EnemyKillStatsByClass,
					m_TimeActiveSeconds = (int)adventureMapState.CurrentMapScenarioState.TimeActiveSec,
					m_TimeActiveSecRunLTD = (int)SaveData.Instance.Global.CurrentAdventureData.TimeActiveSecRunLTD,
					m_CardActionActivations = new Dictionary<string, int>()
				};
				if (currentState?.Stats != null)
				{
					foreach (IGrouping<string, CPlayerStatsActionEnded> item in from x in (from x in currentState.Stats.m_PlayerStats.SelectMany((KeyValuePair<string, CPlayerStatsScenario> stats) => stats.Value.ActionsEnded)
							where x.CardType == CBaseCard.ECardType.CharacterAbility && !x.DefaultAction
							select x).ToList()
						group x by x.CardName)
					{
						string text = item.Key;
						if (LocalizationManager.TryGetTranslation(text, out var translation, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, "English"))
						{
							text = translation;
						}
						matchStopConfig.m_CardActionActivations.Add(text + " Top", item.Count((CPlayerStatsActionEnded x) => x.TopAction));
						matchStopConfig.m_CardActionActivations.Add(text + " Bottom", item.Count((CPlayerStatsActionEnded x) => !x.TopAction));
					}
				}
				if (frosthavenCampaign != null)
				{
					List<string> collection = adventureMapState.CurrentMapScenarioState.CurrentState.Challenges.Select((CChallenge x) => x.Name).ToList();
					matchStopConfig.m_Challenges.AddRange(collection);
					if (result == EResult.Win)
					{
						matchStopConfig.m_ChallengesPassed.AddRange(collection);
					}
				}
				if (result == EResult.Win)
				{
					matchStopConfig.m_EndReason = AWMatchEndReason.dungeon_completed;
					matchStopConfig.m_PlayerResult = AWPlayerResult.victory;
				}
				else if (result == EResult.Lose)
				{
					matchStopConfig.m_EndReason = ((!flag) ? AWMatchEndReason.game_over_objective : AWMatchEndReason.game_over_cards);
					matchStopConfig.m_PlayerResult = AWPlayerResult.defeat;
				}
				else if (result == EResult.InProgress)
				{
					matchStopConfig.m_EndReason = AWMatchEndReason.quit;
				}
				else if (result == EResult.InProgressAppKill)
				{
					matchStopConfig.m_EndReason = AWMatchEndReason.app_kill;
				}
				else if (result == EResult.Resign)
				{
					matchStopConfig.m_EndReason = AWMatchEndReason.resign;
					matchStopConfig.m_PlayerResult = AWPlayerResult.defeat;
				}
				AnalyticsWrapper.LogMatchStop(matchStopConfig);
			}
			else
			{
				Log.Error("Unable to track match end as map state was null.");
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during TrackMatchEnd.");
		}
	}

	public void TrackMatchStart(AWLaunchMethod launchMethod)
	{
		try
		{
			MatchStartConfig matchStartConfig = new MatchStartConfig();
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				CMapState adventureMapState = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState;
				CampaignState frosthavenCampaign = SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign;
				TrackNewAdventureModeMatch();
				TrackMatchSessionID(adventureMapState.CurrentMapScenarioState.MatchSessionID);
				matchStartConfig.m_MissionGoal = adventureMapState.CurrentMapScenarioState.CurrentState.GetAllObjectivesForAnalytics();
				matchStartConfig.m_MissionID = adventureMapState.CurrentMapScenarioState.ID;
				matchStartConfig.m_GameMode = AWGameMode.new_adventure_mode;
				matchStartConfig.m_RunDifficulty = ((frosthavenCampaign != null) ? frosthavenCampaign.Difficulty : adventureMapState.DifficultySetting.ToString());
				matchStartConfig.m_PlayerCountHuman = Math.Max(1, PlayerRegistry.AllPlayers.Count);
				matchStartConfig.m_RosterLevel = adventureMapState.MapParty.PartyLevel;
				matchStartConfig.m_QuestPathLength = 1;
				List<(string, int)> selectedCharacters = new List<(string, int)>();
				List<(string, int)> list = new List<(string, int)>();
				if (frosthavenCampaign != null)
				{
					selectedCharacters.AddRange(SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.PartyCharacters.Select((CharacterState c) => (ID: c.ClassID.ID, (int)c.Level)));
					list.AddRange(SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign.RemovedCharacters.Select((CharacterState c) => (ID: c.ClassID.ID, (int)c.Level)));
					matchStartConfig.m_ClassesSelected = selectedCharacters.Select(((string, int) x) => $"{x.Item1}_0{x.Item2}").ToList();
					matchStartConfig.m_ClassesUnselected = list.Select(((string, int) x) => $"{x.Item1}_0{x.Item2}").ToList();
				}
				else
				{
					selectedCharacters.AddRange(adventureMapState.CurrentMapScenarioState.CurrentState.Players.Select((PlayerState c) => (ClassID: c.ClassID, Level: c.Level)));
					list.AddRange(from c in adventureMapState.MapParty.CheckCharacters
						where !selectedCharacters.Any(((string, int) x) => x.Item1 == c.CharacterID)
						select (CharacterID: c.CharacterID, Level: c.Level));
					matchStartConfig.m_ClassesSelected = selectedCharacters.Select(((string, int) x) => $"{x.Item1}_0{x.Item2}").ToList();
					matchStartConfig.m_ClassesUnselected = list.Select(((string, int) x) => $"{x.Item1}_0{x.Item2}").ToList();
				}
				matchStartConfig.m_ClassesCardsSelected = adventureMapState.CurrentMapScenarioState.CurrentState.GetAllPlayersSelectedAbilityCards();
				if (adventureMapState.CurrentMapScenarioState.MapScenarioType == EMapScenarioType.Custom && adventureMapState.CurrentMapScenarioState.CustomLevelData.PartySpawnType != ELevelPartyChoiceType.LoadAdventureParty)
				{
					matchStartConfig.m_ClassesCardsUnselected = new List<List<string>>();
				}
				else
				{
					matchStartConfig.m_ClassesCardsUnselected = adventureMapState.MapParty.GetAllSelectedCharactersUnselectedAbilityCards;
				}
				matchStartConfig.m_ClassesItemsSelected = new List<List<string>>();
				matchStartConfig.m_ClassesItemsUnselected = new List<List<string>>();
				DefRepository defRepository = YMLDefRepository.Repo.DefRepository;
				if (defRepository != null)
				{
					foreach (List<int> allEquippedItemsForSelectedCharacter in adventureMapState.CurrentMapScenarioState.CurrentState.GetAllEquippedItemsForSelectedCharacters())
					{
						List<string> list2 = new List<string>();
						matchStartConfig.m_ClassesItemsSelected.Add(list2);
						foreach (int item in allEquippedItemsForSelectedCharacter)
						{
							ItemDef itemDef = defRepository.Items.Values.SingleOrDefault((ItemDef x) => x.ScenarioItemID == item);
							if (itemDef != null)
							{
								uint iD = itemDef.ID.ID;
								list2.Add(iD.ToString());
							}
						}
					}
				}
				if (frosthavenCampaign != null)
				{
					foreach (CharacterState partyCharacter in frosthavenCampaign.PartyCharacters)
					{
						List<string> list3 = new List<string>();
						matchStartConfig.m_ClassesItemsUnselected.Add(list3);
						foreach (ItemDef tuckedItem in partyCharacter.TuckedItems)
						{
							uint iD = tuckedItem.ID.ID;
							list3.Add(iD.ToString());
						}
					}
				}
				matchStartConfig.m_TimeActiveSecRunLTD = (int)SaveData.Instance.Global.CurrentAdventureData.TimeActiveSecRunLTD;
				matchStartConfig.m_GoldEarningsRunLTD = adventureMapState.MapParty.TotalEarnedGold;
				matchStartConfig.m_GoldBalance = ((adventureMapState.GoldMode == EGoldMode.PartyGold) ? adventureMapState.MapParty.PartyGold : (-1));
			}
			AnalyticsWrapper.LogMatchStart(matchStartConfig, SaveData.Instance.Global.CurrentAdventureData);
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during TrackMatchStart.\n");
		}
	}

	public void TrackEventStart()
	{
		if (m_StatsDataStorage != null)
		{
			m_StatsDataStorage.m_CurrentEventTime = 0.0;
			m_IsEventRunning = true;
		}
	}

	public void TrackEventStop()
	{
		if (m_StatsDataStorage != null)
		{
			m_IsEventRunning = false;
		}
	}

	public void TrackTutorialStepStart()
	{
		if (m_StatsDataStorage != null)
		{
			m_StatsDataStorage.m_CurrentTutorialStepTime = 0.0;
			m_IsTutorialStepRunning = true;
		}
	}

	public void TrackTutorialStepStop()
	{
		m_IsTutorialStepRunning = false;
	}

	public void TrackDebug(string errorCode, string messageContent, AWMessageType messageType)
	{
		AnalyticsWrapper.LogDebug(new EventDebugStepConfig
		{
			m_IsVisible = true,
			m_MessageType = messageType,
			m_ErrorCode = errorCode,
			m_MessageContent = messageContent,
			m_CurrentScreen = null
		});
	}

	private void SharedLogHandler(Log.MessageData messageData)
	{
		if (messageData.Category == LogCategory.Network && messageData.Level == LogLevel.Error)
		{
			string text = (messageData.Context as string) ?? "";
			if (!ThreadManagerComponent.Manager.IsMainThread)
			{
				Log.Error("StatsDataTracker got error " + text + " from different thread. Abort!\n" + messageData.Message, this);
			}
		}
	}

	private void TrackDebugError(string errorCode, string messageContent)
	{
		TrackDebug(errorCode, messageContent, AWMessageType.error);
	}

	public void TrackContentUnlocked(string contentID, string contentType, CUnlockCondition unlockCondition)
	{
		EventContentUnlockedConfig eventContentUnlockedConfig = new EventContentUnlockedConfig();
		AWContentType contentType2 = AnalyticsWrapper.AWContentTypes.SingleOrDefault((AWContentType x) => x.ToString() == contentType);
		eventContentUnlockedConfig.m_ContentID = contentID;
		eventContentUnlockedConfig.m_ContentType = contentType2;
		eventContentUnlockedConfig.m_UnlockReasons = new List<string>();
		AnalyticsWrapper.LogContentUnlocked(eventContentUnlockedConfig);
	}

	private Dictionary<string, List<string>> ProcessEventRewards(List<RewardGroup> eventRewards)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (RewardGroup eventReward in eventRewards)
		{
			foreach (Reward reward in eventReward.Rewards)
			{
				switch (reward.Type)
				{
				case ETreasureType.Gold:
				{
					if (dictionary.TryGetValue("Gold", out var value7))
					{
						value7.Add(reward.Amount.ToString());
						break;
					}
					value7 = new List<string>();
					value7.Add(reward.Amount.ToString());
					dictionary.Add("Gold", value7);
					break;
				}
				case ETreasureType.XP:
				{
					if (dictionary.TryGetValue("XP", out var value19))
					{
						value19.Add(reward.Amount.ToString());
						break;
					}
					value19 = new List<string>();
					value19.Add(reward.Amount.ToString());
					dictionary.Add("XP", value19);
					break;
				}
				case ETreasureType.Damage:
				{
					if (dictionary.TryGetValue("Damage", out var value10))
					{
						value10.Add(reward.Amount.ToString());
						break;
					}
					value10 = new List<string>();
					value10.Add(reward.Amount.ToString());
					dictionary.Add("Damage", value10);
					break;
				}
				case ETreasureType.Item:
				{
					if (dictionary.TryGetValue("Items", out var value16))
					{
						value16.Add(reward.Item.YMLData.StringID);
						break;
					}
					value16 = new List<string>();
					value16.Add(reward.Item.YMLData.StringID);
					dictionary.Add("Items", value16);
					break;
				}
				case ETreasureType.ItemStock:
				{
					if (dictionary.TryGetValue("ItemStock", out var value2))
					{
						value2.Add(reward.Item.YMLData.StringID);
						break;
					}
					value2 = new List<string>();
					value2.Add(reward.Item.YMLData.StringID);
					dictionary.Add("ItemStock", value2);
					break;
				}
				case ETreasureType.ItemBlueprint:
				{
					if (dictionary.TryGetValue("ItemBlueprint", out var value12))
					{
						value12.Add(reward.Item.YMLData.StringID);
						break;
					}
					value12 = new List<string>();
					value12.Add(reward.Item.YMLData.StringID);
					dictionary.Add("ItemBlueprint", value12);
					break;
				}
				case ETreasureType.LoseItem:
				{
					if (dictionary.TryGetValue("LoseItems", out var value3))
					{
						value3.Add(reward.Item.YMLData.StringID);
						break;
					}
					value3 = new List<string>();
					value3.Add(reward.Item.YMLData.StringID);
					dictionary.Add("LoseItems", value3);
					break;
				}
				case ETreasureType.Infuse:
				{
					if (dictionary.TryGetValue("Infusions", out var value15))
					{
						foreach (ElementInfusionBoardManager.EElement infusion in reward.Infusions)
						{
							value15.Add(infusion.ToString());
						}
						break;
					}
					value15 = new List<string>();
					foreach (ElementInfusionBoardManager.EElement infusion2 in reward.Infusions)
					{
						value15.Add(infusion2.ToString());
					}
					dictionary.Add("Infusions", value15);
					break;
				}
				case ETreasureType.Condition:
				{
					if (dictionary.TryGetValue("Conditions", out var value6))
					{
						foreach (RewardCondition condition in reward.Conditions)
						{
							if (condition.Type == RewardCondition.EConditionType.Negative)
							{
								value6.Add(condition.NegativeCondition.ToString());
							}
							else
							{
								value6.Add(condition.PositiveCondition.ToString());
							}
						}
						break;
					}
					value6 = new List<string>();
					foreach (RewardCondition condition2 in reward.Conditions)
					{
						if (condition2.Type == RewardCondition.EConditionType.Negative)
						{
							value6.Add(condition2.NegativeCondition.ToString());
						}
						else
						{
							value6.Add(condition2.PositiveCondition.ToString());
						}
					}
					dictionary.Add("Conditions", value6);
					break;
				}
				case ETreasureType.EnemyCondition:
				{
					if (dictionary.TryGetValue("EnemyConditions", out var value18))
					{
						foreach (RewardCondition enemyCondition in reward.EnemyConditions)
						{
							if (enemyCondition.Type == RewardCondition.EConditionType.Negative)
							{
								value18.Add(enemyCondition.NegativeCondition.ToString());
							}
							else
							{
								value18.Add(enemyCondition.PositiveCondition.ToString());
							}
						}
						break;
					}
					value18 = new List<string>();
					foreach (RewardCondition enemyCondition2 in reward.EnemyConditions)
					{
						if (enemyCondition2.Type == RewardCondition.EConditionType.Negative)
						{
							value18.Add(enemyCondition2.NegativeCondition.ToString());
						}
						else
						{
							value18.Add(enemyCondition2.PositiveCondition.ToString());
						}
					}
					dictionary.Add("EnemyConditions", value18);
					break;
				}
				case ETreasureType.Reputation:
				{
					if (dictionary.TryGetValue("Reputation", out var value13))
					{
						value13.Add(reward.Amount.ToString());
						break;
					}
					value13 = new List<string>();
					value13.Add(reward.Amount.ToString());
					dictionary.Add("Reputation", value13);
					break;
				}
				case ETreasureType.Prosperity:
				{
					if (dictionary.TryGetValue("Prosperity", out var value9))
					{
						value9.Add(reward.Amount.ToString());
						break;
					}
					value9 = new List<string>();
					value9.Add(reward.Amount.ToString());
					dictionary.Add("Prosperity", value9);
					break;
				}
				case ETreasureType.UnlockQuest:
				{
					if (dictionary.TryGetValue("UnlockQuest", out var value4))
					{
						value4.Add(reward.UnlockName);
						break;
					}
					value4 = new List<string>();
					value4.Add(reward.UnlockName);
					dictionary.Add("UnlockQuest", value4);
					break;
				}
				case ETreasureType.Discard:
				{
					if (dictionary.TryGetValue("Discard", out var value20))
					{
						value20.Add(reward.Amount.ToString());
						break;
					}
					value20 = new List<string>();
					value20.Add(reward.Amount.ToString());
					dictionary.Add("Discard", value20);
					break;
				}
				case ETreasureType.CityEvent:
				{
					if (dictionary.TryGetValue("CityEvent", out var value17))
					{
						value17.Add(reward.UnlockName);
						break;
					}
					value17 = new List<string>();
					value17.Add(reward.UnlockName);
					dictionary.Add("CityEvent", value17);
					break;
				}
				case ETreasureType.RoadEvent:
				{
					if (dictionary.TryGetValue("RoadEvent", out var value14))
					{
						value14.Add(reward.UnlockName);
						break;
					}
					value14 = new List<string>();
					value14.Add(reward.UnlockName);
					dictionary.Add("RoadEvent", value14);
					break;
				}
				case ETreasureType.ConsumeItem:
				{
					if (dictionary.TryGetValue("ConsumeItem", out var value11))
					{
						value11.Add(reward.ConsumeSlot);
						break;
					}
					value11 = new List<string>();
					value11.Add(reward.UnlockName);
					dictionary.Add("ConsumeItem", value11);
					break;
				}
				case ETreasureType.PerkCheck:
				{
					if (dictionary.TryGetValue("PerkCheck", out var value8))
					{
						value8.Add(reward.Amount.ToString());
						break;
					}
					value8 = new List<string>();
					value8.Add(reward.Amount.ToString());
					dictionary.Add("PerkCheck", value8);
					break;
				}
				case ETreasureType.PerkPoint:
				{
					if (dictionary.TryGetValue("PerkPoint", out var value5))
					{
						value5.Add(reward.Amount.ToString());
						break;
					}
					value5 = new List<string>();
					value5.Add(reward.Amount.ToString());
					dictionary.Add("PerkPoint", value5);
					break;
				}
				case ETreasureType.AddModifiers:
				{
					if (dictionary.TryGetValue("AttackModifiers", out var value))
					{
						foreach (KeyValuePair<string, int> modifier in reward.Modifiers)
						{
							for (int i = 0; i < modifier.Value; i++)
							{
								value.Add(modifier.Key);
							}
						}
						break;
					}
					value = new List<string>();
					foreach (KeyValuePair<string, int> modifier2 in reward.Modifiers)
					{
						for (int j = 0; j < modifier2.Value; j++)
						{
							value.Add(modifier2.Key);
						}
					}
					dictionary.Add("AttackModifiers", value);
					break;
				}
				default:
					Log.Error("Unsupported Treasure Type: " + reward.Type);
					break;
				}
			}
		}
		return dictionary;
	}
}
