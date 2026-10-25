using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using FH;
using FH.YML;
using FHRL;
using MapRuleLibrary.Party;
using MiniJSON;
using ScenarioRuleLibrary;

public static class AnalyticsWrapper
{
	private static AmplitudeClient m_AmplitudeClient;

	private static StatsDataStorage m_StatsDataStorage;

	public static readonly AWContentType[] AWContentTypes = (AWContentType[])Enum.GetValues(typeof(AWContentType));

	public static bool IsAppBootSent = false;

	public static bool IsReady => true;

	public static bool IsAnalyticsDisabled()
	{
		if (TelemetryAndEulaPolicy.IsTelemetryEnabled)
		{
			return false;
		}
		if (m_StatsDataStorage == null)
		{
			return true;
		}
		return false;
	}

	public static void InitHeader()
	{
		m_StatsDataStorage = SaveData.Instance.Global.m_StatsDataStorage;
		m_AmplitudeClient = AmplitudeClient.Instance;
		if (m_StatsDataStorage.m_IsStorageNew)
		{
			m_StatsDataStorage.m_IsStorageNew = false;
		}
	}

	public static Dictionary<string, object> MergeEventProperties(Dictionary<string, object> specificEventProperties)
	{
		if (IsAnalyticsDisabled())
		{
			return null;
		}
		Dictionary<string, object> second = new Dictionary<string, object>
		{
			{
				"user_id",
				PlatformLayer.UserData.PlatformPlayerID
			},
			{ "run_session_id", m_StatsDataStorage.m_RunSessionID }
		};
		return (from d in specificEventProperties.Concat(second)
			group d by d.Key).ToDictionary((IGrouping<string, KeyValuePair<string, object>> d) => d.Key, (IGrouping<string, KeyValuePair<string, object>> d) => d.First().Value);
	}

	public static void LogAppBoot()
	{
		if (!IsAnalyticsDisabled())
		{
			IsAppBootSent = true;
			Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>());
			m_AmplitudeClient.LogEvent("BOOT", properties);
		}
	}

	public static void LogRunStart(AWGameMode gameMode, AWLaunchMethod launchMethod, PartyAdventureData partyData, bool isOnline = false, bool isPrivate = false)
	{
		try
		{
			if (IsAnalyticsDisabled())
			{
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"character_list_in",
					partyData.AdventureMapState.MapParty.CheckCharacters.Select((CMapCharacter c) => c.CharacterID + "_0" + c.Level).ToList()
				},
				{
					"launch_method",
					launchMethod.ToString()
				},
				{ "is_online", isOnline },
				{
					"run_difficulty",
					partyData.AdventureMapState.DifficultySetting.ToString()
				},
				{ "campaign_type", null }
			};
			if (partyData.FrosthavenCampaign != null)
			{
				dictionary["character_list_in"] = partyData.FrosthavenCampaign.PartyCharacters.Select((CharacterState x) => x.ClassID.ID + "_0" + x.Level);
				dictionary["launch_method"] = launchMethod.ToString();
				dictionary["is_online"] = isOnline;
				dictionary["run_difficulty"] = partyData.FrosthavenCampaign.Difficulty;
				dictionary["campaign_type"] = partyData.FrosthavenCampaign.GameMode.Name;
			}
			Dictionary<string, object> properties = MergeEventProperties(dictionary);
			m_AmplitudeClient.LogEvent("RUN_START", properties);
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogRunStart.");
		}
	}

	public static void LogRunStop(AWGameMode gameMode, AWSessionEndReason sessionEndReason, PartyAdventureData partyData, bool isOnline = false)
	{
		try
		{
			if (IsAnalyticsDisabled())
			{
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"character_list_in",
					partyData.AdventureMapState.MapParty.CheckCharacters.Select((CMapCharacter c) => c.CharacterID + "_0" + c.Level).ToList()
				},
				{ "is_online", isOnline },
				{
					"run_difficulty",
					partyData.AdventureMapState.DifficultySetting.ToString()
				},
				{ "campaign_type", null }
			};
			if (partyData.FrosthavenCampaign != null)
			{
				dictionary["character_list_in"] = partyData.FrosthavenCampaign.PartyCharacters.Select((CharacterState x) => x.ClassID.ID + "_0" + x.Level);
				dictionary["is_online"] = isOnline;
				dictionary["run_difficulty"] = partyData.FrosthavenCampaign.Difficulty;
				dictionary["campaign_type"] = partyData.FrosthavenCampaign.GameMode.Name;
			}
			Dictionary<string, object> properties = MergeEventProperties(dictionary);
			m_AmplitudeClient.LogEvent("RUN_STOP", properties);
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogRunStop.");
		}
	}

	public static void LogMatchStart(MatchStartConfig matchStartConfig, PartyAdventureData partyData)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				if (m_StatsDataStorage.m_MatchSessionID == null)
				{
					m_StatsDataStorage.m_MatchSessionID = "0";
					Log.Error("Attempted to log a null match session ID at Match Start.");
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary = new Dictionary<string, object>
				{
					{ "match_session_id", m_StatsDataStorage.m_MatchSessionID },
					{ "mission_goals", matchStartConfig.m_MissionGoal },
					{ "mission_id", matchStartConfig.m_MissionID },
					{ "run_difficulty", matchStartConfig.m_RunDifficulty },
					{ "player_count_human", matchStartConfig.m_PlayerCountHuman },
					{ "scenario_level", matchStartConfig.m_RosterLevel },
					{ "character_list_in", matchStartConfig.m_ClassesSelected },
					{ "character_list_out", matchStartConfig.m_ClassesUnselected },
					{
						"card_list_in_character_1",
						(matchStartConfig.m_ClassesCardsSelected.Count > 0) ? Json.Serialize(matchStartConfig.m_ClassesCardsSelected[0]) : null
					},
					{
						"card_list_in_character_2",
						(matchStartConfig.m_ClassesCardsSelected.Count > 1) ? Json.Serialize(matchStartConfig.m_ClassesCardsSelected[1]) : null
					},
					{
						"card_list_in_character_3",
						(matchStartConfig.m_ClassesCardsSelected.Count > 2) ? Json.Serialize(matchStartConfig.m_ClassesCardsSelected[2]) : null
					},
					{
						"card_list_in_character_4",
						(matchStartConfig.m_ClassesCardsSelected.Count > 3) ? Json.Serialize(matchStartConfig.m_ClassesCardsSelected[3]) : null
					},
					{
						"card_list_out_character_1",
						(matchStartConfig.m_ClassesCardsUnselected.Count > 0) ? Json.Serialize(matchStartConfig.m_ClassesCardsUnselected[0]) : null
					},
					{
						"card_list_out_character_2",
						(matchStartConfig.m_ClassesCardsUnselected.Count > 1) ? Json.Serialize(matchStartConfig.m_ClassesCardsUnselected[1]) : null
					},
					{
						"card_list_out_character_3",
						(matchStartConfig.m_ClassesCardsUnselected.Count > 2) ? Json.Serialize(matchStartConfig.m_ClassesCardsUnselected[2]) : null
					},
					{
						"card_list_out_character_4",
						(matchStartConfig.m_ClassesCardsUnselected.Count > 3) ? Json.Serialize(matchStartConfig.m_ClassesCardsUnselected[3]) : null
					},
					{
						"item_list_in_character_1",
						(matchStartConfig.m_ClassesItemsSelected.Count > 0) ? Json.Serialize(matchStartConfig.m_ClassesItemsSelected[0]) : null
					},
					{
						"item_list_in_character_2",
						(matchStartConfig.m_ClassesItemsSelected.Count > 1) ? Json.Serialize(matchStartConfig.m_ClassesItemsSelected[1]) : null
					},
					{
						"item_list_in_character_3",
						(matchStartConfig.m_ClassesItemsSelected.Count > 2) ? Json.Serialize(matchStartConfig.m_ClassesItemsSelected[2]) : null
					},
					{
						"item_list_in_character_4",
						(matchStartConfig.m_ClassesItemsSelected.Count > 3) ? Json.Serialize(matchStartConfig.m_ClassesItemsSelected[3]) : null
					},
					{
						"item_list_out_character_1",
						(matchStartConfig.m_ClassesItemsUnselected.Count > 0) ? Json.Serialize(matchStartConfig.m_ClassesItemsUnselected[0]) : null
					},
					{
						"item_list_out_character_2",
						(matchStartConfig.m_ClassesItemsUnselected.Count > 1) ? Json.Serialize(matchStartConfig.m_ClassesItemsUnselected[1]) : null
					},
					{
						"item_list_out_character_3",
						(matchStartConfig.m_ClassesItemsUnselected.Count > 2) ? Json.Serialize(matchStartConfig.m_ClassesItemsUnselected[2]) : null
					},
					{
						"item_list_out_character_4",
						(matchStartConfig.m_ClassesItemsUnselected.Count > 3) ? Json.Serialize(matchStartConfig.m_ClassesItemsUnselected[3]) : null
					},
					{ "time_active_sec_run_ltd", matchStartConfig.m_TimeActiveSecRunLTD }
				};
				if (partyData.FrosthavenCampaign != null)
				{
					dictionary["week"] = $"{partyData.FrosthavenCampaign.Outpost.Week}";
					dictionary["run_difficulty"] = $"{partyData.Difficulty}";
				}
				Dictionary<string, object> properties = MergeEventProperties(dictionary);
				m_AmplitudeClient.LogEvent("MATCH_START", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogMatchStart.");
		}
	}

	public static void LogMatchStop(MatchStopConfig matchStopConfig)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				if (m_StatsDataStorage.m_MatchSessionID == null)
				{
					m_StatsDataStorage.m_MatchSessionID = "0";
				}
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "match_session_id", m_StatsDataStorage.m_MatchSessionID },
					{ "mission_goals_passed", matchStopConfig.m_MissionGoalsPassed },
					{
						"mission_goals_failed",
						matchStopConfig.m_MissionGoals.Except(matchStopConfig.m_MissionGoalsPassed)
					},
					{ "battle_goal_passed_list", matchStopConfig.m_BattleGoalsPassed },
					{ "battle_goal_failed_list", matchStopConfig.m_BattleGoalsFailed },
					{ "masteries_passed_list", matchStopConfig.m_MasteriesPassed },
					{ "masteries_goal_failed_list", matchStopConfig.m_MasteriesFailed },
					{ "challenges_completed_list", matchStopConfig.m_ChallengesPassed },
					{
						"challenges_failed_list",
						matchStopConfig.m_Challenges.Except(matchStopConfig.m_ChallengesPassed)
					},
					{ "mission_id", matchStopConfig.m_MissionID },
					{ "run_difficulty", matchStopConfig.m_RunDifficulty },
					{ "player_count_human", matchStopConfig.m_PlayerCountHuman },
					{ "time_active_sec", matchStopConfig.m_TimeActiveSeconds },
					{
						"end_reason",
						matchStopConfig.m_EndReason.ToString()
					},
					{
						"player_result",
						matchStopConfig.m_PlayerResult.ToString()
					},
					{ "round_count", matchStopConfig.m_RoundCount },
					{ "scenario_level", matchStopConfig.m_RosterLevel },
					{ "character_list_in", matchStopConfig.m_ClassesSelected },
					{
						"times_card_action_activated",
						Json.Serialize(matchStopConfig.m_CardActionActivations)
					},
					{ "character_status_list", matchStopConfig.m_CharacterStatus },
					{
						"enemy_encounter_count",
						Json.Serialize(matchStopConfig.m_EnemyEncounters)
					},
					{
						"enemy_kill_count",
						Json.Serialize(matchStopConfig.m_EnemyKilled)
					},
					{ "time_active_sec_run_ltd", matchStopConfig.m_TimeActiveSecRunLTD }
				});
				m_AmplitudeClient.LogEvent("MATCH_STOP", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogMatchStop.");
		}
	}

	public static void LogContentUnlocked(EventContentUnlockedConfig eventContentUnlockedConfig)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "run_session_id", m_StatsDataStorage.m_RunSessionID },
					{ "content_id", eventContentUnlockedConfig.m_ContentID },
					{ "content_type", eventContentUnlockedConfig.m_ContentType },
					{ "unlock_reason", eventContentUnlockedConfig.m_UnlockReasons }
				});
				m_AmplitudeClient.LogEvent("CONTENT_UNLOCK", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogContentUnlocked.");
		}
	}

	private static async Task WaitForAppBootEvent()
	{
		while (!IsAppBootSent)
		{
			await Task.Delay(TimeSpan.FromSeconds(0.10000000149011612));
		}
	}

	[Obsolete]
	public static async void LogScreenDisplay(AWScreenName screenName)
	{
	}

	public static void LogEventStop(PartyAdventureData partyData, OutpostEventState eventState, int time)
	{
		try
		{
			if (IsAnalyticsDisabled())
			{
				return;
			}
			CampaignState frosthavenCampaign = partyData.FrosthavenCampaign;
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"week",
					frosthavenCampaign.Outpost.Week
				},
				{
					"fh_event_id",
					eventState.ID.ID
				},
				{ "fh_event_outcome", eventState.FirstOptionSelectedID },
				{
					"player_count_human",
					Math.Max(1, PlayerRegistry.AllPlayers.Count)
				},
				{
					"character_list_in",
					frosthavenCampaign.PartyCharacters.Select((CharacterState x) => x.ClassID.ID)
				},
				{
					"scenarios_completed",
					frosthavenCampaign.UnlockedScenarios.Count((FHRL.ScenarioState x) => x.Completed)
				},
				{ "time_active_sec", time }
			};
			int num = 1;
			foreach (CharacterState partyCharacter in frosthavenCampaign.PartyCharacters)
			{
				dictionary[$"character_{num}_gold"] = partyCharacter.Gold;
				dictionary[$"character_{num}_lumber"] = partyCharacter.Resources.Lumber;
				dictionary[$"character_{num}_metal"] = partyCharacter.Resources.Metal;
				dictionary[$"character_{num}_hide"] = partyCharacter.Resources.Hide;
				num++;
			}
			dictionary["outpost_arrowvine"] = frosthavenCampaign.Outpost.Resources.Arrowvine;
			dictionary["outpost_axenut"] = frosthavenCampaign.Outpost.Resources.Axenut;
			dictionary["outpost_corpsecap"] = frosthavenCampaign.Outpost.Resources.Corpsecap;
			dictionary["outpost_flamefruit"] = frosthavenCampaign.Outpost.Resources.Flamefruit;
			dictionary["outpost_rockroot"] = frosthavenCampaign.Outpost.Resources.Rockroot;
			dictionary["outpost_snowthistle"] = frosthavenCampaign.Outpost.Resources.Snowthistle;
			dictionary["outpost_inspiration"] = frosthavenCampaign.Inspiration;
			dictionary["outpost_defence"] = frosthavenCampaign.Outpost.BaseDefense;
			dictionary["outpost_soldiers"] = frosthavenCampaign.Outpost.Soldiers;
			dictionary["outpost_morale"] = frosthavenCampaign.Outpost.Morale;
			dictionary["outpost_prosperity"] = frosthavenCampaign.Outpost.Prosperity;
			DefRepository defRepository = YMLDefRepository.Repo.DefRepository;
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			foreach (BuildingTypeDef buildingDef in defRepository.Buildings.Values)
			{
				BuildingState buildingState = frosthavenCampaign.Outpost.Buildings.SingleOrDefault((BuildingState x) => x.Def == buildingDef);
				if (buildingState != null)
				{
					string arg = "active";
					if (buildingState.Wrecked)
					{
						arg = "wrecked";
					}
					if (buildingState.UnderConstruction)
					{
						arg = buildingState.ConstructionState.ToString().ToLowerInvariant();
					}
					string item = $"{buildingState.Def.Name} {buildingState.Level} {arg}";
					list2.Add(item);
				}
				if (buildingState != null || frosthavenCampaign.Outpost.AvailableBuildingsToConstruct.Contains(buildingDef))
				{
					list.Add(buildingDef.Name);
					if (buildingState == null || (buildingState.Level == 1 && buildingState.ConstructionState == BuildingConstructionType.NewBuilding) || (buildingState.Level == 0 && buildingState.IsWaitingForDonations))
					{
						list3.Add(buildingDef.Name);
					}
				}
			}
			dictionary["outpost_unlocked_buildings_list"] = list;
			dictionary["outpost_buildings_status_list"] = list2;
			dictionary["outpost_notbuild_buildings_list"] = list3;
			Dictionary<string, object> properties = MergeEventProperties(dictionary);
			if (eventState.Def.Type == OutpostEventType.Outpost)
			{
				m_AmplitudeClient.LogEvent("OUTPOST_EVENT_STOP", properties);
			}
			else
			{
				m_AmplitudeClient.LogEvent("ROAD_BOAT_EVENT_STOP", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogEventStop.");
		}
	}

	public static void LogAchievementUnlocked(string achievementID)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object> { { "achievement_id", achievementID } });
				m_AmplitudeClient.LogEvent("ACHIEVEMENT_UNLOCK", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogAchievementUnlocked.");
		}
	}

	public static void LogTutorialStep(EventTutorialStepConfig eventTutorialStepConfig)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "mission_id", eventTutorialStepConfig.m_MissionID },
					{ "step_id", eventTutorialStepConfig.m_StepID },
					{ "step_sequence_number", eventTutorialStepConfig.m_StepSequenceNumber },
					{
						"step_status",
						eventTutorialStepConfig.m_StepStatus.ToString()
					},
					{ "time_on_step", eventTutorialStepConfig.m_TimeOnStep }
				});
				m_AmplitudeClient.LogEvent("TUTORIAL_STEP", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogTutorialStep.");
		}
	}

	public static void LogErrorMessage(string message)
	{
		if (ScenarioRuleClient.IsWorkerThread)
		{
			return;
		}
		try
		{
			if (!IsAnalyticsDisabled())
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object> { { "message", message } });
				m_AmplitudeClient.LogEvent("ERROR", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred during the AnalyticsWrapper.LogDebug function.");
		}
	}

	public static void LogDebug(EventDebugStepConfig eventDebugStepConfig)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "is_visible", eventDebugStepConfig.m_IsVisible },
					{ "message_type", eventDebugStepConfig.m_MessageType },
					{ "error_code", eventDebugStepConfig.m_ErrorCode },
					{ "message_content", eventDebugStepConfig.m_MessageContent },
					{
						"run_session_id",
						m_StatsDataStorage?.m_RunSessionID
					},
					{
						"match_session_id",
						m_StatsDataStorage?.m_MatchSessionID
					},
					{ "screen_current", eventDebugStepConfig.m_CurrentScreen }
				});
				m_AmplitudeClient.LogEvent("DEBUG", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred during the AnalyticsWrapper.LogDebug function.");
		}
	}

	public static void LogInventoryUpdate(EventInventoryUpdateConfig eventInventoryUpdateConfig)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "run_session_id", m_StatsDataStorage.m_RunSessionID },
					{
						"source",
						eventInventoryUpdateConfig.m_Source ?? "other"
					},
					{ "updated_item", eventInventoryUpdateConfig.m_UpdatedItem },
					{ "quantity", eventInventoryUpdateConfig.m_Quantity },
					{ "character", eventInventoryUpdateConfig.m_Character }
				});
				m_AmplitudeClient.LogEvent("INVENTORY_UPDATE", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogInventoryUpdate.");
		}
	}

	public static void LogCharacterRetired(PartyAdventureData partyData, CharacterState retiredCharacter)
	{
		try
		{
			if (!IsAnalyticsDisabled() && partyData.FrosthavenCampaign != null)
			{
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					["week"] = $"{retiredCharacter.WeekRetired}",
					["character_class"] = retiredCharacter.ClassID.ID ?? "",
					["level"] = $"{retiredCharacter.Level}",
					["mission_started "] = $"{retiredCharacter.ScenariosStarted}",
					["win_ratio"] = $"{((retiredCharacter.ScenariosStarted == 0) ? 0f : (100f * (float)retiredCharacter.ScenariosWon / (float)retiredCharacter.ScenariosStarted))}",
					["retirement_quest"] = retiredCharacter.CompletedPersonalQuests.Select((PersonalQuestID x) => x.ID).ToList(),
					["perks"] = retiredCharacter.Perks.Count(),
					["masteries"] = retiredCharacter.CompletedMasteryIDs.Count
				});
				m_AmplitudeClient.LogEvent("CHARACTER_RETIRES", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogCharacterRetired.");
		}
	}

	public static void LogRoomRevealed(RoomRevealedConfig roomRevealedConfig)
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				if (m_StatsDataStorage.m_MatchSessionID == null)
				{
					m_StatsDataStorage.m_MatchSessionID = "0";
				}
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "match_session_id", m_StatsDataStorage.m_MatchSessionID },
					{ "mission_id", roomRevealedConfig.m_MissionID },
					{ "room_name", roomRevealedConfig.m_RoomName },
					{ "room_id", roomRevealedConfig.m_RoomID },
					{ "round", roomRevealedConfig.m_Round },
					{ "revealing_character", roomRevealedConfig.m_RevealingCharacterClassID },
					{ "time_active_sec", roomRevealedConfig.m_TimeActiveSeconds }
				});
				m_AmplitudeClient.LogEvent("ROOM_REVEAL", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogRoomRevealed.");
		}
	}

	public static void LogQ1DoorOpen()
	{
		try
		{
			if (!IsAnalyticsDisabled())
			{
				if (m_StatsDataStorage.m_MatchSessionID == null)
				{
					m_StatsDataStorage.m_MatchSessionID = "0";
				}
				Dictionary<string, object> properties = MergeEventProperties(new Dictionary<string, object>
				{
					{ "match_session_id", m_StatsDataStorage.m_MatchSessionID },
					{ "q1_door_open", true }
				});
				m_AmplitudeClient.LogEvent("Q1_DOOR_OPEN", properties);
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception during LogQ1DoorOpen.");
		}
	}
}
