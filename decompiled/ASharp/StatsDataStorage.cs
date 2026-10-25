using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

[Serializable]
public class StatsDataStorage : ISerializable
{
	public class SEventHandlerCall
	{
		public EResult result;

		public bool initial;

		public bool endScenario;

		public int round;

		public List<SEvent> newEvents;

		public bool clear;
	}

	public bool m_IsStorageNew = true;

	public double m_TimeLTD;

	public int m_RoguelikeRunCount;

	public int m_RoguelikeRunVictoryCount;

	public double m_RoguelikeTimeLTD;

	public int m_RoguelikeGoldEarningsLTD;

	public int m_CampaignRunCount;

	public int m_CampaignRunVictoryCount;

	public double m_CampaignTimeLTD;

	public double m_CampaignTimeCurrentRun;

	public int m_CampaignGoldEarningsLTD;

	public double m_NewAdventureModeLTD;

	public string m_RunSessionID;

	public string m_MatchSessionID;

	public double m_CurrentEventTime;

	public double m_TutorialTimeLTD;

	public double m_TutorialTimeCurrentRun;

	public double m_CurrentTutorialStepTime;

	public bool m_CompletedTutorial;

	public int m_TotalKills;

	public Dictionary<string, int> m_DefeatedAdventureBosses;

	public EResult m_LastResult;

	public int m_TotalEnemiesLastScenario;

	public static readonly object Synchronizer = new object();

	public Tuple<int, List<ECharacter>> HighestItemsUsedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = Choreographer.s_Choreographer.m_CurrentState.Stats.m_ItemsUsedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	public Tuple<int, List<ECharacter>> HighestGoldLootedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = Choreographer.s_Choreographer.m_CurrentState.Stats.m_GoldPilesLootedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	public Tuple<int, List<ECharacter>> HighestXPEarnedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = Choreographer.s_Choreographer.m_CurrentState.Stats.m_TotalXPEarnedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	public Tuple<int, List<ECharacter>> HighestChestLootedX
	{
		get
		{
			Tuple<int, List<KeyValuePair<string, int>>> tuple = Choreographer.s_Choreographer.m_CurrentState.Stats.m_ChestsLootedLastScenarioByCharacter.MaxManyByAndValue((KeyValuePair<string, int> it) => it.Value);
			if (tuple != null)
			{
				return new Tuple<int, List<ECharacter>>(tuple.Item1, tuple.Item2.ConvertAll((KeyValuePair<string, int> it) => (ECharacter)Enum.Parse(typeof(ECharacter), it.Key)));
			}
			return new Tuple<int, List<ECharacter>>(0, null);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsStorageNew", m_IsStorageNew);
		info.AddValue("TimeLTD", m_TimeLTD);
		info.AddValue("RoguelikeRunCount", m_RoguelikeRunCount);
		info.AddValue("RoguelikeRunVictoryCount", m_RoguelikeRunVictoryCount);
		info.AddValue("RoguelikeTimeLTD", m_RoguelikeTimeLTD);
		info.AddValue("RoguelikeGoldEarningsLTD", m_RoguelikeGoldEarningsLTD);
		info.AddValue("CampaignRunCount", m_CampaignRunCount);
		info.AddValue("CampaignRunVictoryCount", m_CampaignRunVictoryCount);
		info.AddValue("CampaignTimeLTD", m_CampaignTimeLTD);
		info.AddValue("CampaignTimeCurrentRun", m_CampaignTimeCurrentRun);
		info.AddValue("CampaignGoldEarningsLTD", m_CampaignGoldEarningsLTD);
		info.AddValue("m_NewAdventureModeLTD", m_NewAdventureModeLTD);
		info.AddValue("CurrentEventTime", m_CurrentEventTime);
		info.AddValue("TutorialTimeLTD", m_TutorialTimeLTD);
		info.AddValue("TutorialTimeCurrentRun", m_TutorialTimeCurrentRun);
		info.AddValue("CurrentTutorialStepTime", m_CurrentTutorialStepTime);
		info.AddValue("CompletedTutorial", m_CompletedTutorial);
		info.AddValue("TotalKills", m_TotalKills);
		info.AddValue("DefeatedAdventureBosses", m_DefeatedAdventureBosses);
	}

	private StatsDataStorage(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "IsStorageNew":
					m_IsStorageNew = info.GetBoolean("IsStorageNew");
					break;
				case "TimeLTD":
					m_TimeLTD = info.GetDouble("TimeLTD");
					break;
				case "RoguelikeRunCount":
					m_RoguelikeRunCount = info.GetInt32("RoguelikeRunCount");
					break;
				case "RoguelikeRunVictoryCount":
					m_RoguelikeRunVictoryCount = info.GetInt32("RoguelikeRunVictoryCount");
					break;
				case "RoguelikeTimeLTD":
					m_RoguelikeTimeLTD = info.GetDouble("RoguelikeTimeLTD");
					break;
				case "RoguelikeGoldEarningsLTD":
					m_RoguelikeGoldEarningsLTD = info.GetInt32("RoguelikeGoldEarningsLTD");
					break;
				case "CampaignRunCount":
					m_CampaignRunCount = info.GetInt32("CampaignRunCount");
					break;
				case "CampaignRunVictoryCount":
					m_CampaignRunVictoryCount = info.GetInt32("CampaignRunVictoryCount");
					break;
				case "CampaignTimeLTD":
					m_CampaignTimeLTD = info.GetDouble("CampaignTimeLTD");
					break;
				case "CampaignTimeCurrentRun":
					m_CampaignTimeCurrentRun = info.GetDouble("CampaignTimeCurrentRun");
					break;
				case "CampaignGoldEarningsLTD":
					m_CampaignGoldEarningsLTD = info.GetInt32("CampaignGoldEarningsLTD");
					break;
				case "m_NewAdventureModeLTD":
					m_NewAdventureModeLTD = info.GetDouble("m_NewAdventureModeLTD");
					break;
				case "CurrentEventTime":
					m_CurrentEventTime = info.GetInt32("CurrentEventTime");
					break;
				case "TutorialTimeLTD":
					m_TutorialTimeLTD = info.GetDouble("TutorialTimeLTD");
					break;
				case "TutorialTimeCurrentRun":
					m_TutorialTimeCurrentRun = info.GetDouble("TutorialTimeCurrentRun");
					break;
				case "CurrentTutorialStepTime":
					m_CurrentTutorialStepTime = info.GetInt32("CurrentTutorialStepTime");
					break;
				case "CompletedTutorial":
					m_CompletedTutorial = info.GetBoolean("CompletedTutorial");
					break;
				case "TotalKills":
					m_TotalKills = info.GetInt32("TotalKills");
					break;
				case "DefeatedAdventureBosses":
					m_DefeatedAdventureBosses = (Dictionary<string, int>)info.GetValue("DefeatedAdventureBosses", typeof(Dictionary<string, int>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize StatsDataStorage entry " + item.Name);
				throw ex;
			}
		}
		if (m_DefeatedAdventureBosses == null)
		{
			m_DefeatedAdventureBosses = new Dictionary<string, int>();
		}
	}

	public StatsDataStorage()
	{
		m_DefeatedAdventureBosses = new Dictionary<string, int>();
		m_CompletedTutorial = false;
	}

	public bool GetPlayer(Dictionary<string, CPlayerStatsScenario> playerStats, string classID, string playerTurn, out CPlayerStatsScenario currentPlayerStat)
	{
		bool flag = false;
		currentPlayerStat = null;
		flag = classID != null && playerStats.TryGetValue(classID, out currentPlayerStat);
		if (!flag)
		{
			flag = playerTurn != null && playerStats.TryGetValue(playerTurn, out currentPlayerStat);
		}
		return flag;
	}

	public static void ScrapeEventLogAndCheckBattleGoals(EResult result, bool initial, bool endScenario, List<SEvent> newEvents, int round = 0, bool clear = false)
	{
		try
		{
			ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
			{
				SaveData.Instance.Global.m_StatsDataStorage.ScrapeEventLog(result, initial, endScenario, newEvents, round);
				if (clear)
				{
					SEventLog.ClearEventLog();
				}
				SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState?.CheckBattleGoals(useCurrentScenarioStats: true);
				ScenarioRuleClient.MessageHandler(new CUpdateBattleGoalsMessage(null));
			}, "SCRAPE_STATS");
		}
		catch (Exception arg)
		{
			Log.ErrorWithReport($"Exception in ScrapeEventLogAndCheckBattleGoals: {arg}");
		}
	}

	public void ScrapeEventLog(EResult result, bool initial, bool endScenario, List<SEvent> newEvents, int round = 0)
	{
		bool flag = false;
		if (newEvents == null && !ScenarioRuleClient.IsWorkerThread)
		{
			flag = true;
			newEvents = SEventLog.CacheUnprocessedEvents()?.ToList();
		}
		if (flag)
		{
			SEventLog.ClearUnprocessedEventsCache();
		}
		if (SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState?.CurrentState == null)
		{
			return;
		}
		SEventLog.StartScrapingEvents(newEvents);
		m_LastResult = result;
		result.ToString();
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		string empty = string.Empty;
		if (initial || Choreographer.s_Choreographer.m_CurrentState.Stats == null)
		{
			Choreographer.s_Choreographer.m_CurrentState.Stats = new StatsState(currentScenarioState.Players);
		}
		StatsState stats = Choreographer.s_Choreographer.m_CurrentState.Stats;
		if (newEvents != null)
		{
			try
			{
				string partyName = SaveData.Instance.Global.CurrentAdventureData.PartyName;
				string iD = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState.ID;
				try
				{
					List<SEventActor> list = SEventLog.FindAllActorEventsOfSubType(onlyUnprocessed: true, ESESubTypeActor.ActorInitialized, toScrape: true, checkQueue: true);
					for (int i = 0; i < list.Count; i++)
					{
						SEventActor sEventActor = list[i];
						CActor.EType mainActorType = sEventActor.MainActorType;
						ConvertSummon(sEventActor.MainActorType, sEventActor.MainActorEnemySummon, sEventActor.SecondaryActorType, sEventActor.SecondaryActorEnemySummon, out var mainActorTypeString, out var SecondaryActorTypeString);
						int num = sEventActor.MainActorMaxHealth;
						int num2 = sEventActor.MainActorHealth;
						CActor cActor = ScenarioManager.FindActor(sEventActor.MainActorGuid);
						if (cActor != null)
						{
							num = cActor.MaxHealth;
							num2 = cActor.Health;
						}
						CPlayerStatsBase.CPlayerSatsBaseData baseData = new CPlayerStatsBase.CPlayerSatsBaseData(partyName, iD, empty, sEventActor.Round, sEventActor.EventOrder);
						string mainActorClassID = sEventActor.MainActorClassID;
						string secondaryActorClassID = sEventActor.SecondaryActorClassID;
						string mainActorType2 = mainActorTypeString;
						string secondaryActorType = SecondaryActorTypeString;
						List<ElementInfusionBoardManager.EElement> elements = sEventActor.Elements;
						List<PositiveConditionPair> mainActorPositiveConditions = sEventActor.MainActorPositiveConditions;
						List<NegativeConditionPair> mainActorNegativeConditions = sEventActor.MainActorNegativeConditions;
						string mainActorMonsterType = sEventActor.MainActorMonsterType;
						string secondaryActorMonsterType = sEventActor.SecondaryActorMonsterType;
						List<PositiveConditionPair> secondaryActorPositiveConditions = sEventActor.SecondaryActorPositiveConditions;
						List<NegativeConditionPair> secondaryActorNegativeConditions = sEventActor.SecondaryActorNegativeConditions;
						string mainActorGuid = sEventActor.MainActorGuid;
						string secondaryActorGUID = sEventActor.SecondaryActorGUID;
						string secondaryActorMonsterType2 = secondaryActorMonsterType;
						string turnActorGUID = sEventActor.TurnActorGUID;
						int cardID = sEventActor.CardID;
						CBaseCard.ECardType cardType = sEventActor.CardType;
						CAbility.EAbilityType abilityType = sEventActor.AbilityType;
						string actingAbilityName = sEventActor.ActingAbilityName;
						int abilityStrength = sEventActor.AbilityStrength;
						int health = num2;
						int maxHealth = num;
						int mainActorEnemyAdjacent = sEventActor.MainActorEnemyAdjacent;
						bool mainActorWallAdjacent = sEventActor.MainActorWallAdjacent;
						int mainActorAllyAdjacent = sEventActor.MainActorAllyAdjacent;
						int mainActorObstacleAdjacent = sEventActor.MainActorObstacleAdjacent;
						bool mainActorWallAdjacent2 = mainActorWallAdjacent;
						int mainActorPlayerAdjacent = sEventActor.MainActorPlayerAdjacent;
						CPlayerStatsAction item = new CPlayerStatsAction(baseData, new CPlayerStatsAction.CPlayerStatsActionData(mainActorClassID, mainActorType2, mainActorPositiveConditions, mainActorNegativeConditions, mainActorGuid, mainActorMonsterType, secondaryActorClassID, secondaryActorType, secondaryActorPositiveConditions, secondaryActorNegativeConditions, secondaryActorGUID, secondaryActorMonsterType2, turnActorGUID, elements, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons: false, rolledIntoSummoner: false, health, maxHealth, mainActorAllyAdjacent, mainActorEnemyAdjacent, mainActorObstacleAdjacent, mainActorWallAdjacent2, null, null, default, mainActorPlayerAdjacent));
						switch (mainActorType)
						{
						case CActor.EType.Enemy:
						case CActor.EType.Ally:
						case CActor.EType.Enemy2:
							stats.m_MonsterStats.ActorsInitialized.Add(item);
							break;
						case CActor.EType.Player:
						case CActor.EType.HeroSummon:
						{
							if (mainActorType == CActor.EType.Player && sEventActor.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(sEventActor.MainActorClassID, out var value) && sEventActor.ActorSubType != ESESubTypeActor.ActorGainXP)
							{
								value.ActorsInitialized.Add(item);
							}
							if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString))
							{
								stats.m_HeroSummonStats.ActorsInitialized.Add(item);
							}
							break;
						}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Could not process Scraping Actor Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
				}
				try
				{
					foreach (SEventHeroSummonRangeChange moveEvent in SEventLog.FindAllEventsOfType<SEventHeroSummonRangeChange>(onlyUnprocessed: true, toScrape: true, checkQueue: true))
					{
						if (moveEvent.MainActorType != CActor.EType.HeroSummon)
						{
							continue;
						}
						HeroSummonState heroSummonState = currentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == moveEvent.MainActorGuid) as HeroSummonState;
						if (heroSummonState != null)
						{
							ActorState actorState = currentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == heroSummonState.Summoner);
							if (stats.m_PlayerStats.TryGetValue(actorState.ClassID, out var value2))
							{
								value2.HeroSummons.Add(new CPlayerStatsHeroSummon(heroSummonState.ActorGuid, heroSummonState.ClassID, moveEvent.RangeToSummoner, moveEvent.IsDead, moveEvent.SummonerCauseOfDeath, moveEvent.RoundSpawned, new CPlayerStatsBase.CPlayerSatsBaseData(partyName, iD, empty, moveEvent.Round, moveEvent.EventOrder)));
							}
						}
					}
				}
				catch (Exception ex2)
				{
					Log.Exception(ex2, "Could not process Scraping Actor Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
				}
				try
				{
					foreach (SEventActiveBonus activeBonusEvent in SEventLog.FindAllEventsOfType<SEventActiveBonus>(onlyUnprocessed: true, toScrape: true, checkQueue: true))
					{
						if (activeBonusEvent.Round >= ScenarioManager.CurrentScenarioState.RoundNumber)
						{
							ActorState actorState2 = currentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == activeBonusEvent.MainActorGuid);
							if (actorState2 != null && stats.m_PlayerStats.TryGetValue(actorState2.ClassID, out var value3))
							{
								value3.ActiveBonuses.Add(new CPlayerStatsActiveBonuses(activeBonusEvent.ActiveBonuses.ToList(), activeBonusEvent.CauseOfDeath, new CPlayerStatsBase.CPlayerSatsBaseData(partyName, iD, empty, activeBonusEvent.Round, activeBonusEvent.EventOrder)));
							}
						}
					}
				}
				catch (Exception ex3)
				{
					Log.Exception(ex3, "Could not process Scraping Actor Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex3.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex3.Message);
				}
				try
				{
					foreach (SEventActorAgility agilityEvent in SEventLog.FindAllEventsOfType<SEventActorAgility>(onlyUnprocessed: true, toScrape: true, checkQueue: true))
					{
						ActorState actorState3 = currentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == agilityEvent.MainActorGuid);
						if (actorState3 != null && stats.m_PlayerStats.TryGetValue(actorState3.ClassID, out var value4))
						{
							value4.Agility.Add(new CPlayerStatsAgility(agilityEvent.Agility, new CPlayerStatsBase.CPlayerSatsBaseData(partyName, iD, empty, agilityEvent.Round, agilityEvent.EventOrder)));
						}
					}
				}
				catch (Exception ex4)
				{
					Log.Exception(ex4, "Could not process Scraping Actor Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex4.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex4.Message);
				}
				try
				{
					foreach (SEventActorActiveBonuses activeBonusesEvent in SEventLog.FindAllEventsOfType<SEventActorActiveBonuses>(onlyUnprocessed: true, toScrape: true, checkQueue: true))
					{
						ActorState actorState4 = currentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == activeBonusesEvent.MainActorGuid);
						if (actorState4 != null && stats.m_PlayerStats.TryGetValue(actorState4.ClassID, out var value5))
						{
							value5.ActorActiveBonuses.Add(CPlayerStatsActorActiveBonuses.CreateActorActiveBonusesStat(activeBonusesEvent, partyName, iD, empty));
						}
					}
				}
				catch (Exception ex5)
				{
					Log.Exception(ex5, "Could not process Scraping Actor Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex5.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex5.Message);
				}
				try
				{
					foreach (SEventActorForm formEvent in SEventLog.FindAllEventsOfType<SEventActorForm>(onlyUnprocessed: true, toScrape: true, checkQueue: true))
					{
						ActorState actorState5 = currentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == formEvent.MainActorGuid);
						if (actorState5 != null && stats.m_PlayerStats.TryGetValue(actorState5.ClassID, out var value6))
						{
							value6.Form.Add(new CPlayerStatsForm(formEvent.Form, new CPlayerStatsBase.CPlayerSatsBaseData(partyName, iD, empty, formEvent.Round, formEvent.EventOrder)));
						}
					}
				}
				catch (Exception ex6)
				{
					Log.Exception(ex6, "Could not process Scraping Actor Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex6.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex6.Message);
				}
				try
				{
					foreach (SEventActorDamaged item5 in SEventLog.FindAllEventsOfType<SEventActorDamaged>(onlyUnprocessed: true, toScrape: true))
					{
						int damageTaken = item5.DamageTaken;
						CActor.EType mainActorType3 = item5.MainActorType;
						ConvertSummon(item5.MainActorType, item5.MainActorEnemySummon, item5.SecondaryActorType, item5.SecondaryActorEnemySummon, out var mainActorTypeString2, out var SecondaryActorTypeString2);
						string mainActorClassID2 = item5.MainActorClassID;
						string secondaryActorClassID2 = item5.SecondaryActorClassID;
						switch (mainActorType3)
						{
						case CActor.EType.Enemy:
						case CActor.EType.Ally:
						case CActor.EType.Enemy2:
						{
							stats.m_TotalDamageDoneLastScenario += damageTaken;
							if (GetPlayer(stats.m_PlayerStats, secondaryActorClassID2, item5.CurrentPhaseActorName, out var currentPlayerStat2))
							{
								currentPlayerStat2.DamageDealt.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, mainActorClassID2, secondaryActorClassID2));
							}
							if (CActor.EType.HeroSummon.ToString().Contains(SecondaryActorTypeString2))
							{
								bool rolledIntoSummoner2 = false;
								if (!string.IsNullOrEmpty(item5.SecondaryActorSummonerID) && GetPlayer(stats.m_PlayerStats, item5.SecondaryActorSummonerID, "", out var currentPlayerStat3))
								{
									SecondaryActorTypeString2 = CActor.EType.Player.ToString();
									currentPlayerStat3.DamageDealt.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, mainActorClassID2, secondaryActorClassID2, performedBySummons: true));
									rolledIntoSummoner2 = true;
								}
								stats.m_HeroSummonStats.DamageDealt.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, null, null, performedBySummons: false, rolledIntoSummoner2));
							}
							stats.m_MonsterStats.DamageReceived.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, item5.SecondaryActorClassID, item5.MainActorClassID));
							break;
						}
						case CActor.EType.Player:
						case CActor.EType.HeroSummon:
						{
							stats.m_TotalDamageTakenLastScenario += damageTaken;
							if (mainActorType3 == CActor.EType.Player && item5.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item5.MainActorClassID, out var value7))
							{
								value7.DamageReceived.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, mainActorClassID2, secondaryActorClassID2));
							}
							if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString2))
							{
								bool rolledIntoSummoner = false;
								if (!string.IsNullOrEmpty(item5.MainActorSummonerID) && GetPlayer(stats.m_PlayerStats, item5.MainActorSummonerID, "", out var currentPlayerStat))
								{
									mainActorTypeString2 = CActor.EType.Player.ToString();
									currentPlayerStat.DamageReceived.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, mainActorClassID2, secondaryActorClassID2, performedBySummons: true));
									rolledIntoSummoner = true;
								}
								stats.m_HeroSummonStats.DamageReceived.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2, mainActorClassID2, secondaryActorClassID2, performedBySummons: false, rolledIntoSummoner));
							}
							stats.m_MonsterStats.DamageDealt.Add(CPlayerStatsDamage.CreateDamageStat(item5, partyName, iD, empty, mainActorTypeString2, SecondaryActorTypeString2));
							break;
						}
						}
					}
				}
				catch (Exception ex7)
				{
					Log.Exception(ex7, "Could not process Scraping Damage Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex7.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex7.Message);
				}
				try
				{
					m_TotalEnemiesLastScenario = ScenarioManager.CurrentScenarioState.Monsters.Count;
					foreach (SEventActor item6 in SEventLog.FindAllActorEventsOfSubType(onlyUnprocessed: true, ESESubTypeActor.ActorOnDeath, toScrape: true))
					{
						ConvertSummon(item6.MainActorType, item6.MainActorEnemySummon, item6.SecondaryActorType, item6.SecondaryActorEnemySummon, out var mainActorTypeString3, out var SecondaryActorTypeString3);
						if (item6.MainActorType == CActor.EType.Enemy || item6.MainActorType == CActor.EType.Enemy2 || item6.MainActorType == CActor.EType.Ally)
						{
							m_TotalKills++;
							stats.m_EnemiesKilledLastScenario++;
							if (GetPlayer(stats.m_PlayerStats, item6.SecondaryActorClassID, item6.CurrentPhaseActorName, out var currentPlayerStat4))
							{
								SecondaryActorTypeString3 = CActor.EType.Player.ToString();
								currentPlayerStat4.Kills.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3));
							}
							if (CActor.EType.HeroSummon.ToString().Contains(SecondaryActorTypeString3))
							{
								bool rolledIntoSummoner3 = false;
								if (!string.IsNullOrEmpty(item6.SecondaryActorSummonerID) && GetPlayer(stats.m_PlayerStats, item6.SecondaryActorSummonerID, "", out var currentPlayerStat5))
								{
									SecondaryActorTypeString3 = CActor.EType.Player.ToString();
									currentPlayerStat5.Kills.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3, null, null, performedBySummons: true));
									rolledIntoSummoner3 = true;
								}
								stats.m_HeroSummonStats.Kills.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3, null, null, performedBySummons: false, rolledIntoSummoner3));
							}
							stats.m_MonsterStats.Deaths.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3));
						}
						if (item6.MainActorType != CActor.EType.Player && item6.MainActorType != CActor.EType.HeroSummon)
						{
							continue;
						}
						if (item6.MainActorType == CActor.EType.Player && GetPlayer(stats.m_PlayerStats, item6.MainActorClassID, item6.CurrentPhaseActorName, out var currentPlayerStat6))
						{
							mainActorTypeString3 = CActor.EType.Player.ToString();
							currentPlayerStat6.Deaths.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3));
						}
						if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString3))
						{
							bool rolledIntoSummoner4 = false;
							if (GetPlayer(stats.m_PlayerStats, item6.SecondaryActorClassID, "", out var currentPlayerStat7) || GetPlayer(stats.m_PlayerStats, item6.SecondaryActorSummonerID, "", out currentPlayerStat7))
							{
								SecondaryActorTypeString3 = CActor.EType.Player.ToString();
								if (item6.SecondaryActorType == CActor.EType.Player && item6.CauseOfDeath != CActor.ECauseOfDeath.SummonActiveBonusCancelled)
								{
									currentPlayerStat7.Kills.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3));
								}
								rolledIntoSummoner4 = true;
							}
							stats.m_HeroSummonStats.Deaths.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3, item6.SecondaryActorClassID, item6.MainActorClassID, performedBySummons: false, rolledIntoSummoner4));
						}
						if (item6.SecondaryActorType != CActor.EType.Player)
						{
							stats.m_MonsterStats.Kills.Add(CPlayerStatsKill.CreateKillStat(item6, partyName, iD, empty, mainActorTypeString3, SecondaryActorTypeString3));
						}
					}
				}
				catch (Exception ex8)
				{
					Log.Exception(ex8, "Could not process Scraping Kill Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex8.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex8.Message);
				}
				foreach (SEventActorHealed item7 in SEventLog.FindAllEventsOfType<SEventActorHealed>(onlyUnprocessed: true, toScrape: true))
				{
					CActor.EType? secondaryActorType2 = item7.SecondaryActorType;
					ConvertSummon(item7.MainActorType, item7.MainActorEnemySummon, item7.SecondaryActorType, item7.SecondaryActorEnemySummon, out var mainActorTypeString4, out var SecondaryActorTypeString4);
					if (item7.SecondaryActorClassID != null && stats.m_PlayerStats.TryGetValue(item7.SecondaryActorClassID, out var value8))
					{
						value8.Heals.Add(CPlayerStatsHeal.CreateHealStat(item7, partyName, iD, empty, mainActorTypeString4, SecondaryActorTypeString4));
					}
					if (item7.SecondaryActorType == CActor.EType.HeroSummon)
					{
						bool rolledIntoSummoner5 = false;
						if (!string.IsNullOrEmpty(item7.SecondaryActorSummonerID) && GetPlayer(stats.m_PlayerStats, item7.SecondaryActorSummonerID, "", out var currentPlayerStat8))
						{
							SecondaryActorTypeString4 = CActor.EType.Player.ToString();
							currentPlayerStat8.Heals.Add(CPlayerStatsHeal.CreateHealStat(item7, partyName, iD, empty, mainActorTypeString4, SecondaryActorTypeString4, null, null, performedBySummons: true));
							rolledIntoSummoner5 = true;
						}
						stats.m_HeroSummonStats.Heals.Add(CPlayerStatsHeal.CreateHealStat(item7, partyName, iD, empty, mainActorTypeString4, SecondaryActorTypeString4, null, null, performedBySummons: false, rolledIntoSummoner5));
					}
					if (item7.SecondaryActorType == CActor.EType.Enemy || item7.SecondaryActorType == CActor.EType.Enemy2 || item7.SecondaryActorType == CActor.EType.Ally)
					{
						stats.m_MonsterStats.Heals.Add(CPlayerStatsHeal.CreateHealStat(item7, partyName, iD, empty, mainActorTypeString4, SecondaryActorTypeString4));
					}
					if (secondaryActorType2 == CActor.EType.Player || secondaryActorType2 == CActor.EType.HeroSummon)
					{
						stats.m_TotalHealingDoneLastScenario += item7.HealAmount;
					}
				}
				foreach (SEventAbilityDestroyObstacle item8 in from x in SEventLog.FindAllAbilityEventsOfTypeAndSubType(onlyUnprocessed: true, CAbility.EAbilityType.DestroyObstacle, ESESubTypeAbility.AbilityEnded, toScrape: true)
					where x is SEventAbilityDestroyObstacle
					select x)
				{
					ConvertSummon(item8.MainActorType, item8.MainActorIsEnemySummon, item8.SecondaryActorType, item8.SecondaryActorIsSummon, out var mainActorTypeString5, out var SecondaryActorTypeString5);
					if (item8.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item8.MainActorClassID, out var value9))
					{
						value9.DestroyedObstacles.Add(CPlayerStatsDestroyObstacle.CreateDestroyObstacleStat(item8, partyName, iD, empty, mainActorTypeString5, SecondaryActorTypeString5));
					}
				}
				foreach (SEventAbilityDestroyProp item9 in SEventLog.FindAllEventsOfType<SEventAbilityDestroyProp>(onlyUnprocessed: true, toScrape: true))
				{
					ConvertSummon(item9.MainActorType, item9.MainActorIsEnemySummon, item9.SecondaryActorType, item9.SecondaryActorIsSummon, out var mainActorTypeString6, out var SecondaryActorTypeString6);
					if (item9.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item9.MainActorClassID, out var value10))
					{
						value10.PropsDestroyedByAbilities.Add(CPlayerStatsDestroyProp.CreateDestroyPropStat(item9, partyName, iD, empty, mainActorTypeString6, SecondaryActorTypeString6));
					}
				}
				foreach (SEventAbilityCreateProp item10 in SEventLog.FindAllEventsOfType<SEventAbilityCreateProp>(onlyUnprocessed: true, toScrape: true))
				{
					ConvertSummon(item10.MainActorType, item10.MainActorIsEnemySummon, item10.SecondaryActorType, item10.SecondaryActorIsSummon, out var mainActorTypeString7, out var SecondaryActorTypeString7);
					if (item10.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item10.MainActorClassID, out var value11))
					{
						value11.PropsCreatedByAbilities.Add(CPlayerStatsCreateProp.CreateCreatePropStat(item10, partyName, iD, empty, mainActorTypeString7, SecondaryActorTypeString7));
					}
				}
				foreach (SEventLoseCard item11 in SEventLog.FindAllEventsOfType<SEventLoseCard>(onlyUnprocessed: true, toScrape: true))
				{
					if (item11.CharacterID != null && stats.m_PlayerStats.TryGetValue(item11.CharacterID, out var value12))
					{
						value12.LoseCards.Add(CPlayerStatsLoseCard.CreateLoseCardStat(item11, partyName, iD, empty, CActor.EType.Player.ToString(), null, item11.CharacterID));
					}
				}
				foreach (SEventRecoverCard item12 in SEventLog.FindAllEventsOfType<SEventRecoverCard>(onlyUnprocessed: true, toScrape: true))
				{
					if (item12.CharacterID != null && stats.m_PlayerStats.TryGetValue(item12.CharacterID, out var value13))
					{
						value13.RecoverCards.Add(CPlayerStatsRecoverCard.CreateRecoverCardStat(item12, partyName, iD, empty, CActor.EType.Player.ToString(), null, item12.CharacterID));
					}
				}
				foreach (SEventDiscardCard item13 in SEventLog.FindAllEventsOfType<SEventDiscardCard>(onlyUnprocessed: true, toScrape: true))
				{
					if (item13.CharacterID != null && stats.m_PlayerStats.TryGetValue(item13.CharacterID, out var value14))
					{
						value14.DiscardCard.Add(CPlayerStatsDiscardCard.CreateDiscardCardStat(item13, partyName, iD, empty, CActor.EType.Player.ToString(), null, item13.CharacterID));
					}
				}
				foreach (SEventActorHealthChanged item14 in SEventLog.FindAllEventsOfType<SEventActorHealthChanged>(onlyUnprocessed: true, toScrape: true))
				{
					if (item14.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item14.MainActorClassID, out var value15))
					{
						value15.Healed.Add(CPlayerStatsActorHealthChanged.CreateHealthChangedStat(item14, partyName, iD, empty, CActor.EType.Player.ToString(), null, item14.MainActorClassID));
					}
				}
				foreach (SEventHand item15 in SEventLog.FindAllEventsOfType<SEventHand>(onlyUnprocessed: true, toScrape: true))
				{
					if (item15.CharacterID != null && stats.m_PlayerStats.TryGetValue(item15.CharacterID, out var value16))
					{
						value16.Hand.Clear();
						value16.Hand.Add(CPlayerStatsHand.CreateHandStat(item15, partyName, iD, empty, CActor.EType.Player.ToString(), null, item15.CharacterID));
					}
				}
				foreach (SEventLostAdjacency item16 in SEventLog.FindAllEventsOfType<SEventLostAdjacency>(onlyUnprocessed: true, toScrape: true))
				{
					if (item16.CharacterID != null && stats.m_PlayerStats.TryGetValue(item16.CharacterID, out var value17))
					{
						value17.LostAdjacency.Add(CPlayerStatsLostAdjacency.CreateLostAdjacencyStat(item16, partyName, iD, empty, CActor.EType.Player.ToString(), null, item16.CharacterID));
					}
				}
				foreach (SEventObjectProp item17 in SEventLog.FindAllEventsOfType<SEventObjectProp>(onlyUnprocessed: true, toScrape: true))
				{
					if (item17.ObjectAction == "Activate" && item17.ObjectCharacterID != null && stats.m_PlayerStats.TryGetValue(item17.ObjectCharacterID, out var value18))
					{
						if (item17.ObjectType == ScenarioManager.ObjectImportType.Door)
						{
							value18.Door.Add(CPlayerStatsDoor.CreateDoorStat(item17, partyName, iD, empty, CActor.EType.Player.ToString(), null, item17.ObjectCharacterID));
						}
						value18.ActivatedProps.Add(CPlayerStatsPropActivated.CreatePropActivatedStat(item17, partyName, iD, empty, CActor.EType.Player.ToString(), null, item17.ObjectCharacterID));
					}
				}
				foreach (SEventObjectPropTrap item18 in SEventLog.FindAllEventsOfType<SEventObjectPropTrap>(onlyUnprocessed: true, toScrape: true))
				{
					if (item18.ObjectType == ScenarioManager.ObjectImportType.Trap && item18.ActionActor != null && stats.m_PlayerStats.TryGetValue(item18.ActionActor, out var value19))
					{
						value19.Trap.Add(CPlayerStatsTrap.CreateTrapActivatedOrDisarmedStat(item18, partyName, iD, empty, CActor.EType.Player.ToString(), null, item18.ActionActor));
					}
				}
				foreach (SEventTrapActor item19 in SEventLog.FindAllEventsOfType<SEventTrapActor>(onlyUnprocessed: true, toScrape: true))
				{
					if (item19.Round >= ScenarioManager.CurrentScenarioState.RoundNumber)
					{
						stats.m_TrapActorStats.Add(CTrapActorStats.CreateTrapStat(item19, partyName, iD, empty));
					}
				}
				foreach (SEventActorForcedIntoTrap item20 in SEventLog.FindAllEventsOfType<SEventActorForcedIntoTrap>(onlyUnprocessed: true, toScrape: true))
				{
					if (item20.Round >= ScenarioManager.CurrentScenarioState.RoundNumber && stats.m_PlayerStats.TryGetValue(item20.ControllingClassID, out var value20))
					{
						value20.ForcedActorsIntoTraps.Add(CPlayerStatsForcedActorsIntoTraps.CreateForcedIntoTrapStat(item20, partyName, iD, empty, null, null, item20.ControllingClassID));
					}
				}
				foreach (SEventRound item21 in SEventLog.FindAllEventsOfType<SEventRound>(onlyUnprocessed: true, toScrape: true))
				{
					foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat3 in stats.m_PlayerStats)
					{
						playerStat3.Value.Monsters.Add(CPlayerStatsMonsters.CreateRoundMonsterStat(item21, partyName, iD, empty, null, null, playerStat3.Key));
					}
				}
				List<CObjectProp> activatedProps = ScenarioManager.CurrentScenarioState.ActivatedProps;
				for (int num3 = 0; num3 < activatedProps.Count; num3++)
				{
					if (activatedProps[num3].ObjectType == ScenarioManager.ObjectImportType.MoneyToken)
					{
						stats.m_ScenarioGoldLastScenario++;
					}
					else if (activatedProps[num3].ObjectType == ScenarioManager.ObjectImportType.Chest || activatedProps[num3].ObjectType == ScenarioManager.ObjectImportType.GoalChest)
					{
						stats.m_ScenarioChestsLastScenario++;
					}
				}
				foreach (SEventActorLooted item22 in SEventLog.FindAllEventsOfType<SEventActorLooted>(onlyUnprocessed: true, toScrape: true))
				{
					ScenarioManager.ObjectImportType lootedType = item22.LootedType;
					if (item22.MainActorType != CActor.EType.Player)
					{
						continue;
					}
					ConvertSummon(item22.MainActorType, item22.MainActorEnemySummon, item22.SecondaryActorType, item22.SecondaryActorEnemySummon, out var mainActorTypeString8, out var SecondaryActorTypeString8);
					if (item22.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item22.MainActorClassID, out var value21))
					{
						value21.Loot.Add(CPlayerStatsLoot.CreateLootStat(item22, partyName, iD, empty, mainActorTypeString8, SecondaryActorTypeString8));
					}
					switch (lootedType)
					{
					case ScenarioManager.ObjectImportType.MoneyToken:
						stats.m_GoldPilesLootedLastScenario++;
						if (stats.m_GoldPilesLootedLastScenarioByCharacter.ContainsKey(item22.MainActorClassID))
						{
							stats.m_GoldPilesLootedLastScenarioByCharacter[item22.MainActorClassID]++;
						}
						else if (item22.MainActorClassID != null)
						{
							stats.m_GoldPilesLootedLastScenarioByCharacter.Add(item22.MainActorClassID, 1);
						}
						break;
					case ScenarioManager.ObjectImportType.Chest:
					case ScenarioManager.ObjectImportType.GoalChest:
						stats.m_ChestsLootedLastScenario++;
						if (stats.m_ChestsLootedLastScenarioByCharacter.ContainsKey(item22.MainActorClassID))
						{
							stats.m_ChestsLootedLastScenarioByCharacter[item22.MainActorClassID]++;
						}
						else if (item22.MainActorClassID != null)
						{
							stats.m_ChestsLootedLastScenarioByCharacter.Add(item22.MainActorClassID, 1);
						}
						break;
					}
				}
				foreach (SEventActorArrayIndexChanged item23 in SEventLog.FindAllEventsOfType<SEventActorArrayIndexChanged>(onlyUnprocessed: true, toScrape: true))
				{
					if (item23.MainActorType == CActor.EType.Player)
					{
						ConvertSummon(item23.MainActorType, item23.MainActorEnemySummon, item23.SecondaryActorType, item23.SecondaryActorEnemySummon, out var mainActorTypeString9, out var SecondaryActorTypeString9);
						if (item23.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item23.MainActorClassID, out var value22))
						{
							value22.ArrayIndexChanged.Add(CPlayerStatsArrayIndexChanged.CreateArrayIndexChangedStat(item23, partyName, iD, empty, mainActorTypeString9, SecondaryActorTypeString9));
						}
					}
					else if (item23.MainActorType == CActor.EType.HeroSummon)
					{
						stats.m_HeroSummonStats.ArrayIndexChanged.Add(CPlayerStatsArrayIndexChanged.CreateArrayIndexChangedStat(item23, partyName, iD, empty, item23.MainActorType.ToString(), null));
					}
					else
					{
						stats.m_MonsterStats.ArrayIndexChanged.Add(CPlayerStatsArrayIndexChanged.CreateArrayIndexChangedStat(item23, partyName, iD, empty, item23.MainActorType.ToString(), null));
					}
				}
				foreach (SEventActorRemovedCondition item24 in SEventLog.FindAllEventsOfType<SEventActorRemovedCondition>(onlyUnprocessed: true, toScrape: true))
				{
					if (item24.MainActorType == CActor.EType.Player)
					{
						ConvertSummon(item24.MainActorType, item24.MainActorEnemySummon, item24.SecondaryActorType, item24.SecondaryActorEnemySummon, out var mainActorTypeString10, out var SecondaryActorTypeString10);
						if (item24.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item24.MainActorClassID, out var value23))
						{
							value23.RemovedCondition.Add(CPlayerStatsRemovedCondition.CreateRemovedConditionStat(item24, partyName, iD, empty, mainActorTypeString10, SecondaryActorTypeString10));
						}
					}
					else if (item24.MainActorType == CActor.EType.HeroSummon)
					{
						stats.m_HeroSummonStats.RemovedCondition.Add(CPlayerStatsRemovedCondition.CreateRemovedConditionStat(item24, partyName, iD, empty, item24.MainActorType.ToString(), null));
					}
					else
					{
						stats.m_MonsterStats.RemovedCondition.Add(CPlayerStatsRemovedCondition.CreateRemovedConditionStat(item24, partyName, iD, empty, item24.MainActorType.ToString(), null));
					}
				}
				foreach (SEventActorEarnedAbilityXP item25 in SEventLog.FindAllEventsOfType<SEventActorEarnedAbilityXP>(onlyUnprocessed: true, toScrape: true))
				{
					if (item25.MainActorType == CActor.EType.Player)
					{
						ConvertSummon(item25.MainActorType, item25.MainActorEnemySummon, item25.SecondaryActorType, item25.SecondaryActorEnemySummon, out var mainActorTypeString11, out var SecondaryActorTypeString11);
						if (item25.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item25.MainActorClassID, out var value24))
						{
							value24.XP.Add(CPlayerStatsXP.CreateEarnedXPStat(item25, partyName, iD, empty, mainActorTypeString11, SecondaryActorTypeString11));
						}
						stats.m_TotalXPEarnedLastScenario += item25.XPEarned;
						if (stats.m_TotalXPEarnedLastScenarioByCharacter.ContainsKey(item25.MainActorClassID))
						{
							stats.m_TotalXPEarnedLastScenarioByCharacter[item25.MainActorClassID] += item25.XPEarned;
						}
						else if (item25.MainActorClassID != null)
						{
							stats.m_TotalXPEarnedLastScenarioByCharacter.Add(item25.MainActorClassID, item25.XPEarned);
						}
					}
				}
				foreach (SEventActorUsedItem item26 in SEventLog.FindAllEventsOfType<SEventActorUsedItem>(onlyUnprocessed: true, toScrape: true))
				{
					stats.m_ItemsUsedLastScenario++;
					if (stats.m_ItemsUsedLastScenarioByCharacter.ContainsKey(item26.MainActorClassID))
					{
						stats.m_ItemsUsedLastScenarioByCharacter[item26.MainActorClassID]++;
					}
					else if (item26.MainActorClassID != null)
					{
						stats.m_ItemsUsedLastScenarioByCharacter.Add(item26.MainActorClassID, 1);
					}
					ConvertSummon(item26.MainActorType, item26.MainActorEnemySummon, item26.SecondaryActorType, item26.SecondaryActorEnemySummon, out var mainActorTypeString12, out var SecondaryActorTypeString12);
					if (item26.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item26.MainActorClassID, out var value25))
					{
						value25.Items.Add(CPlayerStatsItem.CreateItemUsedStat(item26, partyName, iD, empty, mainActorTypeString12, SecondaryActorTypeString12));
					}
				}
				List<SEventActorLootedRandomItemBlueprint> list2 = SEventLog.FindAllEventsOfType<SEventActorLootedRandomItemBlueprint>(onlyUnprocessed: true, toScrape: true);
				for (int num4 = 0; num4 < list2.Count; num4++)
				{
					SEventActorLootedRandomItemBlueprint sEventActorLootedRandomItemBlueprint = list2[num4];
					if (sEventActorLootedRandomItemBlueprint.Round == round)
					{
						if (stats.m_RandomItemBlueprintsLootedLastScenarioByCharacter.ContainsKey(sEventActorLootedRandomItemBlueprint.MainActorClassID))
						{
							stats.m_RandomItemBlueprintsLootedLastScenarioByCharacter[sEventActorLootedRandomItemBlueprint.MainActorClassID]++;
						}
						else if (sEventActorLootedRandomItemBlueprint.MainActorClassID != null)
						{
							stats.m_RandomItemBlueprintsLootedLastScenarioByCharacter.Add(sEventActorLootedRandomItemBlueprint.MainActorClassID, 1);
						}
					}
				}
				foreach (SEventActorEndTurn item27 in SEventLog.FindAllEventsOfType<SEventActorEndTurn>(onlyUnprocessed: true, toScrape: true))
				{
					ConvertSummon(item27.MainActorType, item27.MainActorEnemySummon, item27.SecondaryActorType, item27.SecondaryActorEnemySummon, out var mainActorTypeString13, out var SecondaryActorTypeString13);
					if (item27.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item27.MainActorClassID, out var value26))
					{
						value26.EndTurn.Add(CPlayerStatsEndTurn.CreateEndTurnStat(item27, partyName, iD, empty, mainActorTypeString13, SecondaryActorTypeString13));
					}
					else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString13))
					{
						stats.m_HeroSummonStats.EndTurn.Add(CPlayerStatsEndTurn.CreateEndTurnStat(item27, partyName, iD, empty, mainActorTypeString13, SecondaryActorTypeString13));
					}
					else
					{
						stats.m_MonsterStats.EndTurn.Add(CPlayerStatsEndTurn.CreateEndTurnStat(item27, partyName, iD, empty, mainActorTypeString13, SecondaryActorTypeString13));
					}
				}
				foreach (SEventAction item28 in from x in SEventLog.FindAllEventsOfType<SEventAction>(onlyUnprocessed: true, toScrape: true)
					where x.SubTypeAction == ESESubTypeAction.ActionEnded
					select x)
				{
					ConvertSummon(item28.MainActorType, item28.MainActorType == CActor.EType.HeroSummon, null, secondaryActorIsSummon: false, out var mainActorTypeString14, out var _);
					if (item28.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item28.MainActorClassID, out var value27))
					{
						value27.ActionsEnded.Add(CPlayerStatsActionEnded.CreateActionEndedStat(item28, partyName, iD, empty));
					}
					else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString14))
					{
						stats.m_HeroSummonStats.ActionsEnded.Add(CPlayerStatsActionEnded.CreateActionEndedStat(item28, partyName, iD, empty));
					}
					else
					{
						stats.m_MonsterStats.ActionsEnded.Add(CPlayerStatsActionEnded.CreateActionEndedStat(item28, partyName, iD, empty));
					}
				}
				foreach (SEventActorEndRound item29 in SEventLog.FindAllEventsOfType<SEventActorEndRound>(onlyUnprocessed: true, toScrape: true))
				{
					ConvertSummon(item29.MainActorType, item29.MainActorEnemySummon, item29.SecondaryActorType, item29.SecondaryActorEnemySummon, out var mainActorTypeString15, out var SecondaryActorTypeString15);
					if (item29.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item29.MainActorClassID, out var value28))
					{
						value28.EndRound.Add(CPlayerStatsEndRound.CreateEndRoundStat(item29, partyName, iD, empty, mainActorTypeString15, SecondaryActorTypeString15));
					}
					else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString15))
					{
						stats.m_HeroSummonStats.EndRound.Add(CPlayerStatsEndRound.CreateEndRoundStat(item29, partyName, iD, empty, mainActorTypeString15, SecondaryActorTypeString15));
					}
					else
					{
						stats.m_MonsterStats.EndRound.Add(CPlayerStatsEndRound.CreateEndRoundStat(item29, partyName, iD, empty, mainActorTypeString15, SecondaryActorTypeString15));
					}
				}
				foreach (SEventActorStartTurn item30 in SEventLog.FindAllEventsOfType<SEventActorStartTurn>(onlyUnprocessed: true, toScrape: true))
				{
					ConvertSummon(item30.MainActorType, item30.MainActorEnemySummon, item30.SecondaryActorType, item30.SecondaryActorEnemySummon, out var mainActorTypeString16, out var SecondaryActorTypeString16);
					if (item30.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item30.MainActorClassID, out var value29))
					{
						value29.StartTurn.Add(CPlayerStatsStartTurn.CreateStartTurnStat(item30, partyName, iD, empty, mainActorTypeString16, SecondaryActorTypeString16));
					}
					else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString16))
					{
						stats.m_HeroSummonStats.StartTurn.Add(CPlayerStatsStartTurn.CreateStartTurnStat(item30, partyName, iD, empty, mainActorTypeString16, SecondaryActorTypeString16));
					}
					else
					{
						stats.m_MonsterStats.StartTurn.Add(CPlayerStatsStartTurn.CreateStartTurnStat(item30, partyName, iD, empty, mainActorTypeString16, SecondaryActorTypeString16));
					}
				}
				try
				{
					foreach (SEventElement item31 in SEventLog.FindAllEventsOfType<SEventElement>(onlyUnprocessed: true, toScrape: true))
					{
						ConvertSummon(item31.ActorType, item31.ActorEnemySummon, null, secondaryActorIsSummon: false, out var mainActorTypeString17, out var _);
						if (item31.ActorClassID != null && stats.m_PlayerStats.TryGetValue(item31.ActorClassID, out var value30))
						{
							if (item31.ElementSubType == ESESubTypeElement.Consumed)
							{
								value30.Consumed.Add(CPlayerStatsElement.CreateElementsStat(item31, partyName, iD, empty, CPlayerStatsElement.EStatsElementType.Consumed, "Player"));
							}
							else if (item31.ElementSubType == ESESubTypeElement.Infused)
							{
								value30.Infusions.Add(CPlayerStatsElement.CreateElementsStat(item31, partyName, iD, empty, CPlayerStatsElement.EStatsElementType.Infused, "Player"));
							}
						}
						else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString17))
						{
							if (item31.ElementSubType == ESESubTypeElement.Consumed)
							{
								stats.m_HeroSummonStats.Consumed.Add(CPlayerStatsElement.CreateElementsStat(item31, partyName, iD, empty, CPlayerStatsElement.EStatsElementType.Consumed, mainActorTypeString17));
							}
							else if (item31.ElementSubType == ESESubTypeElement.Infused)
							{
								stats.m_HeroSummonStats.Infusions.Add(CPlayerStatsElement.CreateElementsStat(item31, partyName, iD, empty, CPlayerStatsElement.EStatsElementType.Infused, mainActorTypeString17));
							}
						}
						else if (item31.ElementSubType == ESESubTypeElement.Consumed)
						{
							stats.m_MonsterStats.Consumed.Add(CPlayerStatsElement.CreateElementsStat(item31, partyName, iD, empty, CPlayerStatsElement.EStatsElementType.Consumed, mainActorTypeString17));
						}
						else if (item31.ElementSubType == ESESubTypeElement.Infused)
						{
							stats.m_MonsterStats.Infusions.Add(CPlayerStatsElement.CreateElementsStat(item31, partyName, iD, empty, CPlayerStatsElement.EStatsElementType.Infused, mainActorTypeString17));
						}
					}
				}
				catch (Exception ex9)
				{
					Log.Exception(ex9, "Could not process Scraping Element Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex9.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex9.Message);
				}
				try
				{
					List<SEventAbility> list3 = SEventLog.FindAllAbilityEventsOfSubType(onlyUnprocessed: true, ESESubTypeAbility.AbilityEnded, toScrape: true);
					list3.AddRange(SEventLog.FindAllAbilityEventsOfSubType(onlyUnprocessed: true, ESESubTypeAbility.ApplyToActor, toScrape: true));
					List<SEventAbility> list4 = SEventLog.FindAllAbilityEventsOfSubType(onlyUnprocessed: true, ESESubTypeAbility.AbilityStart, toScrape: true);
					if (list4 != null && list4.Count > 0)
					{
						for (int num5 = list4.Count - 1; num5 >= 0; num5--)
						{
							if (list4[num5].AbilityType != CAbility.EAbilityType.Damage || (list4[num5].AbilityType == CAbility.EAbilityType.Damage && list4[num5].MainActorClassID != "BerserkerID"))
							{
								list4.RemoveAt(num5);
							}
						}
						list3.AddRange(list4);
					}
					foreach (SEventAbility item32 in list3)
					{
						if (item32 is SEventAbilityDestroyProp)
						{
							continue;
						}
						List<Point> list5 = new List<Point>();
						bool? areaEffect = null;
						int targets = 0;
						bool specialAbility = false;
						int num6 = 1;
						List<bool> advantage = null;
						if (item32 is SEventAbilityRemoveShadow sEventAbilityRemoveShadow)
						{
							num6 = sEventAbilityRemoveShadow.Targets;
						}
						if (item32 is SEventAbilityMove sEventAbilityMove)
						{
							num6 = sEventAbilityMove.TilesMoved;
							specialAbility = sEventAbilityMove.Jumped;
							_ = sEventAbilityMove.Strength;
							list5.Add(sEventAbilityMove.MovedToPoint.ToPoint());
						}
						else if (item32.AbilityType == CAbility.EAbilityType.Move)
						{
							continue;
						}
						if (item32 is SEventAbilityPush sEventAbilityPush)
						{
							list5.AddRange(sEventAbilityPush.ActorsDistanceMoved.Select((CAbilityPush.PushedActorStats x) => x.MovedToPoint.ToPoint()));
						}
						if (item32 is SEventAbilityPull sEventAbilityPull)
						{
							list5.AddRange(sEventAbilityPull.ActorsDistanceMoved.Select((CAbilityPull.PulledActorStats x) => x.MovedToPoint.ToPoint()));
						}
						if (item32 is SEventAbilityCreate sEventAbilityCreate)
						{
							num6 = sEventAbilityCreate.PropsSpawned;
						}
						if (item32 is SEventAbilityTargeting sEventAbilityTargeting)
						{
							num6 = sEventAbilityTargeting.Amount;
						}
						if (item32.AbilityType == CAbility.EAbilityType.ModeTransfer)
						{
							_ = item32.Strength;
						}
						if (item32 is SEventAbilityAttack { Advantage: var advantage2 } sEventAbilityAttack)
						{
							advantage = advantage2?.ToList();
							_ = sEventAbilityAttack.Strength;
							areaEffect = sEventAbilityAttack.AreaEffect;
							targets = sEventAbilityAttack.TargetIDs?.Count() ?? 0;
							if (sEventAbilityAttack.TargetIDs != null)
							{
								for (int num7 = 0; num7 < sEventAbilityAttack.TargetIDs.Count; num7++)
								{
									string key = sEventAbilityAttack.TargetIDs[num7];
									bool item2 = sEventAbilityAttack.Advantage[num7];
									int abilityStrength2 = sEventAbilityAttack.ModifiedStrength[num7];
									ConvertSummon(item32.MainActorType, item32.MainActorIsEnemySummon, item32.SecondaryActorType, item32.SecondaryActorIsSummon, out var mainActorTypeString18, out var SecondaryActorTypeString18);
									CPlayerStatsAbilities cPlayerStatsAbilities = CPlayerStatsAbilities.CreateAbilityStat(sEventAbilityAttack, partyName, iD, empty, mainActorTypeString18, SecondaryActorTypeString18, num6, specialAbility, areaEffect, targets);
									cPlayerStatsAbilities.Advantage = new List<bool> { item2 };
									cPlayerStatsAbilities.AbilityStrength = abilityStrength2;
									if (stats.m_PlayerStats.TryGetValue(key, out var value31))
									{
										value31.AttackedByAbilities.Add(cPlayerStatsAbilities);
									}
									else if (item32.SecondaryActorType != CActor.EType.Player && item32.SecondaryActorType != CActor.EType.HeroSummon)
									{
										stats.m_MonsterStats.AttackedByAbilities.Add(cPlayerStatsAbilities);
									}
								}
							}
						}
						if (item32 is SEventAbilityDisarmTrap sEventAbilityDisarmTrap)
						{
							num6 = 0;
							foreach (KeyValuePair<string, int> item33 in sEventAbilityDisarmTrap.TrapsDisarmedDictionary)
							{
								num6 += item33.Value;
							}
						}
						if ((item32.AbilitySubType != ESESubTypeAbility.ApplyToActor || (item32.AbilityType != CAbility.EAbilityType.RecoverDiscardedCards && item32.AbilityType != CAbility.EAbilityType.PlaySong && item32.AbilityType != CAbility.EAbilityType.RecoverLostCards && item32.AbilityType != CAbility.EAbilityType.RefreshItemCards && item32.AbilityType != CAbility.EAbilityType.LoseCards && item32.AbilityType != CAbility.EAbilityType.IncreaseCardLimit && item32.AbilityType != CAbility.EAbilityType.ChooseAbility && item32.AbilityType != CAbility.EAbilityType.NullTargeting)) && (item32.AbilitySubType != ESESubTypeAbility.AbilityEnded || (item32.AbilityType != CAbility.EAbilityType.Kill && item32.AbilityType != CAbility.EAbilityType.ControlActor && item32.AbilityType != CAbility.EAbilityType.Advantage && item32.AbilityType != CAbility.EAbilityType.Bless && item32.AbilityType != CAbility.EAbilityType.Curse && item32.AbilityType != CAbility.EAbilityType.Disarm && item32.AbilityType != CAbility.EAbilityType.Impair && item32.AbilityType != CAbility.EAbilityType.Immobilize && item32.AbilityType != CAbility.EAbilityType.Invisible && item32.AbilityType != CAbility.EAbilityType.Muddle && item32.AbilityType != CAbility.EAbilityType.Poison && item32.AbilityType != CAbility.EAbilityType.Strengthen && item32.AbilityType != CAbility.EAbilityType.Retaliate && item32.AbilityType != CAbility.EAbilityType.Stun && item32.AbilityType != CAbility.EAbilityType.Wound && item32.AbilityType != CAbility.EAbilityType.Regenerate && item32.AbilityType != CAbility.EAbilityType.Shield && item32.AbilityType != CAbility.EAbilityType.AddDoom && item32.AbilityType != CAbility.EAbilityType.StopFlying && item32.AbilityType != CAbility.EAbilityType.Sleep && item32.AbilityType != CAbility.EAbilityType.LoseGoalChestReward && item32.AbilityType != CAbility.EAbilityType.Bane && item32.AbilityType != CAbility.EAbilityType.Ward)))
						{
							ConvertSummon(item32.MainActorType, item32.MainActorIsEnemySummon, item32.SecondaryActorType, item32.SecondaryActorIsSummon, out var mainActorTypeString19, out var SecondaryActorTypeString19);
							CPlayerStatsAbilities item3 = CPlayerStatsAbilities.CreateAbilityStat(item32, partyName, iD, empty, mainActorTypeString19, SecondaryActorTypeString19, num6, specialAbility, areaEffect, targets, null, null, performedBySummons: false, rolledIntoSummoner: false, advantage, list5);
							if (item32.MainActorClassID != null && stats.m_PlayerStats.TryGetValue(item32.MainActorClassID, out var value32))
							{
								value32.Abilities.Add(item3);
							}
							else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString19))
							{
								stats.m_HeroSummonStats.Abilities.Add(item3);
							}
							else
							{
								stats.m_MonsterStats.Abilities.Add(item3);
							}
						}
					}
				}
				catch (Exception ex10)
				{
					Log.Exception(ex10, "Could not process Scraping Ability Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex10.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex10.Message);
				}
				try
				{
					foreach (SEventAttackModifier modifierEvent in SEventLog.FindAllEventsOfType<SEventAttackModifier>(onlyUnprocessed: true, toScrape: true))
					{
						ConvertSummon(modifierEvent.MainActorType, modifierEvent.MainActorIsEnemySummon, modifierEvent.SecondaryActorType, modifierEvent.SecondaryActorIsSummon, out var mainActorTypeString20, out var SecondaryActorTypeString20);
						PlayerState playerState = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ActorGuid == modifierEvent.CurrentPhaseActorGuid);
						CPlayerStatsModifiers item4 = CPlayerStatsModifiers.CreateAttackModifierStat(modifierEvent, partyName, iD, empty, mainActorTypeString20, SecondaryActorTypeString20);
						if (playerState != null)
						{
							if (playerState.ClassID != null && stats.m_PlayerStats.TryGetValue(playerState.ClassID, out var value33))
							{
								value33.Modifiers.Add(item4);
							}
						}
						else if (CActor.EType.HeroSummon.ToString().Contains(mainActorTypeString20))
						{
							stats.m_HeroSummonStats.Modifiers.Add(item4);
						}
						else
						{
							stats.m_MonsterStats.Modifiers.Add(item4);
						}
					}
				}
				catch (Exception ex11)
				{
					Log.Exception(ex11, "Could not process Scraping Modifier Event Log.cs.");
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex11.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex11.Message);
				}
			}
			catch (Exception ex12)
			{
				Log.Exception(ex12, "Could not process Scraping Event Log.cs.");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex12.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex12.Message);
			}
		}
		if (endScenario)
		{
			AdventureState.MapState.MapParty.CurrentScenarioStats.Clear();
			AdventureState.MapState.MapParty.LastScenarioStats.Clear();
			AdventureState.MapState.MapParty.LastScenarioHeroSummon.Clear();
			foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat in stats.m_PlayerStats)
			{
				PlayerState playerState2 = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ClassID == playerStat.Key);
				playerStat.Value.PlayerSurvivedScenario = !playerState2.IsDead && playerState2.Player.CauseOfDeath == CActor.ECauseOfDeath.StillAlive;
				playerStat.Value.RoundsPlayed = currentScenarioState.RoundNumber;
				AdventureState.MapState.MapParty.LastScenarioStats.Add(playerStat.Value);
				AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter c) => c.CharacterID == playerStat.Key)?.UpdatePlayerStats(playerStat.Value);
			}
			try
			{
				if (currentScenarioState.HeroSummons != null)
				{
					stats.m_HeroSummonStats.RoundsPlayed = currentScenarioState.RoundNumber;
					AdventureState.MapState.MapParty.LastScenarioHeroSummon.Add(stats.m_HeroSummonStats);
					AdventureState.MapState.MapParty.HeroSummonsStats.UpdatePlayerStats(stats.m_HeroSummonStats);
				}
			}
			catch (Exception ex13)
			{
				Log.Exception(ex13, "Could not process Scraping HeroSummon updatestats.");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex13.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex13.Message);
			}
			try
			{
				if (currentScenarioState.Monsters != null)
				{
					stats.m_MonsterStats.RoundsPlayed = currentScenarioState.RoundNumber;
					AdventureState.MapState.MapParty.LastMonsterStats.Clear();
					AdventureState.MapState.MapParty.LastMonsterStats.Add(stats.m_MonsterStats);
				}
			}
			catch (Exception ex14)
			{
				Log.Exception(ex14, "Could not process Scraping Monster updatestats.");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex14.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex14.Message);
			}
		}
		else
		{
			AdventureState.MapState.MapParty.CurrentScenarioStats.Clear();
			foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat2 in stats.m_PlayerStats)
			{
				PlayerState playerState3 = currentScenarioState.Players.SingleOrDefault((PlayerState x) => x.ClassID == playerStat2.Key);
				playerStat2.Value.PlayerSurvivedScenario = !playerState3.IsDead;
				playerStat2.Value.RoundsPlayed = currentScenarioState.RoundNumber;
				AdventureState.MapState.MapParty.CurrentScenarioStats.Add(playerStat2.Value);
			}
			AdventureState.MapState.MapParty.CurrentScenarioHeroSummon = stats.m_HeroSummonStats;
			try
			{
				if (currentScenarioState.Monsters != null)
				{
					stats.m_MonsterStats.RoundsPlayed = currentScenarioState.RoundNumber;
					AdventureState.MapState.MapParty.MonsterStats.Clear();
					AdventureState.MapState.MapParty.MonsterStats.Add(stats.m_MonsterStats);
				}
			}
			catch (Exception ex15)
			{
				Log.Exception(ex15, "Could not process Scraping Monster updatestats.");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_STATSDATASTORAGE_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex15.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex15.Message);
			}
		}
		SEventLog.FinishScrapingEvents();
	}

	public void ConvertSummon(CActor.EType? mainActorType, bool mainActorIsSummon, CActor.EType? secondaryActorType, bool secondaryActorIsSummon, out string mainActorTypeString, out string SecondaryActorTypeString)
	{
		if ((mainActorType == CActor.EType.Enemy || mainActorType == CActor.EType.Enemy2) & mainActorIsSummon)
		{
			mainActorTypeString = "EnemySummon";
		}
		else
		{
			mainActorTypeString = mainActorType.ToString();
		}
		if ((secondaryActorType == CActor.EType.Enemy || secondaryActorType == CActor.EType.Enemy2) & secondaryActorIsSummon)
		{
			SecondaryActorTypeString = "EnemySummon";
		}
		else
		{
			SecondaryActorTypeString = secondaryActorType.ToString();
		}
	}

	public void UpdateDefeatedBosses(string bossName)
	{
		if (!m_DefeatedAdventureBosses.ContainsKey(bossName))
		{
			m_DefeatedAdventureBosses.Add(bossName, 1);
		}
		else
		{
			m_DefeatedAdventureBosses[bossName]++;
		}
	}
}
