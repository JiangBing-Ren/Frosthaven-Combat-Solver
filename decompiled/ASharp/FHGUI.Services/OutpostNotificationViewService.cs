using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHGUI.Elements.Notifications;
using FHGUI.States;
using FHRL;
using FHRL.Buildings;
using GLOOM;
using JetBrains.Annotations;
using Libs.GameConsole;
using Libs.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Services;

public class OutpostNotificationViewService : FHMapViewService
{
	private readonly BuildingViewService _buildingViewService;

	private readonly UIElementTagsViewService _tagsViewService;

	private readonly CharacterDetailsViewService _characterViewService;

	private readonly Dictionary<uint, List<EventLogDTO>> _eventLogCache = new Dictionary<uint, List<EventLogDTO>>();

	private bool _refreshCurrentWeekEventLog = true;

	private readonly List<OutpostNotificationDTO> _notificationsCache = new List<OutpostNotificationDTO>();

	private List<string> _notificationPlayerData = new List<string>();

	private bool _storeNotificationData;

	private const string NOTIFICATION_DATA_TAG = "NOTIFICATION_DATA";

	private const string NOTIFICATION_BUILDING_ID_VAR_KEY = "BUILDING_ID";

	private const string NOTIFICATION_ITEM_ID_VAR_KEY = "ITEM_ID";

	private const string NOTIFICATION_CHARACTER_ID_VAR_KEY = "CHARACTER_ID";

	private const string NOTIFICATION_CAMPAIGNSECTION_ID_VAR_KEY = "SECTION_ID";

	private bool _init;

	private const char NOTIFICATION_SEPARATOR = ',';

	private readonly StringBuilder _tooltipBuilder = new StringBuilder();

	private static CampaignEffectType[] _typesToSkip = new CampaignEffectType[26]
	{
		CampaignEffectType.TriggerAttackOutpost,
		CampaignEffectType.FS130ConclusionEffect,
		CampaignEffectType.ProtectBuildings,
		CampaignEffectType.Event_SO18_OptionB_AfterScenarioSuccess,
		CampaignEffectType.LoseMaterials,
		CampaignEffectType.LoseLumber,
		CampaignEffectType.LoseHide,
		CampaignEffectType.LoseMetal,
		CampaignEffectType.LoseGold,
		CampaignEffectType.GainGold,
		CampaignEffectType.GainMetal,
		CampaignEffectType.GainLumber,
		CampaignEffectType.GainHide,
		CampaignEffectType.GainItem,
		CampaignEffectType.LoseItem,
		CampaignEffectType.LosePet,
		CampaignEffectType.UnlockClassChoise,
		CampaignEffectType.ChooseNextCampaignSection,
		CampaignEffectType.ChooseUnlockMandatoryScenario,
		CampaignEffectType.GainHerbs,
		CampaignEffectType.GainMaterials,
		CampaignEffectType.FS119ConclusionEffect,
		CampaignEffectType.FS116ConclusionEffect,
		CampaignEffectType.FS127ConclusionEffect,
		CampaignEffectType.FS128ConclusionEffect,
		CampaignEffectType.WR24OptionAEffect
	};

	private List<OutpostEventLogState.EventLogData.CampaignSectionTriggered> _sectionTriggered = new List<OutpostEventLogState.EventLogData.CampaignSectionTriggered>();

	private Dictionary<string, ECharacter> _retirementSections = new Dictionary<string, ECharacter>
	{
		{
			"50.2",
			ECharacter.BannerSpear
		},
		{
			"70.2",
			ECharacter.Boneshaper
		},
		{
			"53.3",
			ECharacter.Deathwalker
		},
		{
			"48.3",
			ECharacter.Drifter
		},
		{
			"60.2",
			ECharacter.Blinkblade
		},
		{
			"160.3",
			ECharacter.Geminate
		},
		{
			"120.3",
			ECharacter.Frozenfist
		},
		{
			"107.3",
			ECharacter.Snowdancer
		},
		{
			"51.3",
			ECharacter.Pyroclast
		},
		{
			"179.6",
			ECharacter.Painconduit
		},
		{
			"56.5",
			ECharacter.Trapper
		},
		{
			"148.2",
			ECharacter.HIVE
		},
		{
			"103.4",
			ECharacter.Metalmosaic
		},
		{
			"92.2",
			ECharacter.Infuser
		},
		{
			"54.2",
			ECharacter.Crashingtide
		},
		{
			"99.3",
			ECharacter.Deepwraith
		},
		{
			"132.1",
			ECharacter.Shattersong
		}
	};

	[ConsoleVariable(Alias = "dbg_show_demo_completed")]
	public static bool DbgShowDemoCompleted_kvar = false;

	private OutpostState _outpostState => _view.GameState.Outpost;

	private OutpostEventLogState _eventLog => _outpostState.EventLog;

	public bool HasUnseenStoryEvents { get; private set; }

	public OutpostNotificationViewService(FHMapView view)
		: base(view)
	{
		_buildingViewService = view.GetService<BuildingViewService>();
		_tagsViewService = view.GetService<UIElementTagsViewService>();
		_characterViewService = view.GetService<CharacterDetailsViewService>();
	}

	public void Init()
	{
		if (!_init)
		{
			_init = true;
			Refresh();
		}
	}

	public void Refresh()
	{
		BuildEventLogCache(_outpostState.Week);
		BuildAvailableNotificationsFromEventLog();
		_refreshCurrentWeekEventLog = false;
		LoadPerPlayerState();
	}

	public void ProcessGameResult(MultiplayerGameController.Result result)
	{
		if (!_init)
		{
			Init();
		}
		if (result.CampaignResult == null || Choreographer.IsInRoadToFrosthaven)
		{
			return;
		}
		if (result.CampaignResult.HasEvent<CampaignEvent.WeekPassed>())
		{
			BuildAvailableNotificationsFromEventLog();
			ClearPerPlayerState();
		}
		if (result.CampaignResult.HasEvent<CampaignEvent.CampaignSectionActivated>())
		{
			foreach (CampaignEvent.CampaignSectionActivated item in result.CampaignResult.Events.OfType<CampaignEvent.CampaignSectionActivated>())
			{
				if (!ShouldSkipSectionNotification(item.Event))
				{
					HasUnseenStoryEvents = true;
					break;
				}
			}
		}
		foreach (CampaignEvent.CharacterRetired item2 in result.CampaignResult.Events.OfType<CampaignEvent.CharacterRetired>())
		{
			CharacterState character = item2.Character;
			string id = OutpostNotificationUtils.BuildID(OutpostNotificationType.HeroReadyForRetirement, character.ID);
			_notificationsCache.RemoveWhere((OutpostNotificationDTO n) => n.ID == id);
		}
		foreach (CampaignEvent.JobPostingAccepted ev in result.CampaignResult.Events.OfType<CampaignEvent.JobPostingAccepted>())
		{
			_notificationsCache.RemoveWhere((OutpostNotificationDTO n) => n.ID == OutpostNotificationUtils.BuildID(OutpostNotificationType.NewJobPosting, ev.Event.ID.ID));
		}
		OutpostEventLogState eventLog = result.CampaignResult.BeginStats.EventLog;
		if (eventLog.Count != _eventLog.Count)
		{
			_refreshCurrentWeekEventLog = true;
			for (int num = eventLog.Count; num < _eventLog.Count; num++)
			{
				AddOrInvalidateNotificationData(_eventLog.Events[num]);
			}
		}
	}

	public IList<EventLogDTO> GetFullEventLog()
	{
		uint week = _outpostState.Week;
		return Enumerable.Range(1, (int)week).SelectMany((int w) => GetEventLog((uint)w)).ToArray();
	}

	public IList<EventLogDTO> GetEventLog(uint? week = null)
	{
		uint valueOrDefault = week.GetValueOrDefault();
		if (!week.HasValue)
		{
			valueOrDefault = _outpostState.Week;
			week = valueOrDefault;
		}
		bool flag = week == _outpostState.Week;
		if (!_eventLogCache.ContainsKey(week.Value) || (flag && _refreshCurrentWeekEventLog))
		{
			BuildEventLogCache(week.Value);
		}
		_refreshCurrentWeekEventLog = !flag && _refreshCurrentWeekEventLog;
		return _eventLogCache[week.Value];
	}

	private void BuildEventLogCache(uint week)
	{
		if (!_eventLogCache.ContainsKey(week))
		{
			_eventLogCache[week] = new List<EventLogDTO>();
		}
		_eventLogCache[week].Clear();
		OutpostEventLogState.EventLogData[] array = _eventLog.GetEventsByWeek(week).ToArray();
		foreach (OutpostEventLogState.EventLogData eventLogData in array)
		{
			try
			{
				string eventLogText = GetEventLogText(eventLogData);
				EventLogDTO item = new EventLogDTO
				{
					Text = eventLogText,
					IsNewWeek = (eventLogData is OutpostEventLogState.EventLogData.WeekChanged),
					Week = eventLogData.Week
				};
				if (!eventLogText.IsNullOrEmpty())
				{
					_eventLogCache[week].Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.GUIError($"Event log of type {eventLogData.GetType()} failed parsing with {ex.Message}");
			}
		}
	}

	private string GetEventLogText(OutpostEventLogState.EventLogData eventLog)
	{
		UIInfoTools instance = UIInfoTools.Instance;
		OutpostBuildingDataSO buildings = instance.Buildings;
		if (!(eventLog is OutpostEventLogState.EventLogData.BuildingAttacked buildingAttacked))
		{
			if (!(eventLog is OutpostEventLogState.EventLogData.BuildingConstructionCompleted buildingConstructionCompleted))
			{
				BuildingState buildingStateByID;
				if (!(eventLog is OutpostEventLogState.EventLogData.BuildingDamaged buildingDamaged))
				{
					if (!(eventLog is OutpostEventLogState.EventLogData.BuildingDefended buildingDefended))
					{
						if (!(eventLog is OutpostEventLogState.EventLogData.BuildingDonationStarted buildingDonationStarted))
						{
							if (!(eventLog is OutpostEventLogState.EventLogData.BuildingDonationFinished buildingDonationFinished))
							{
								if (!(eventLog is OutpostEventLogState.EventLogData.BuildingDonationCancelled buildingDonationCancelled))
								{
									if (!(eventLog is OutpostEventLogState.EventLogData.BuildingRebuilt buildingRebuilt))
									{
										if (!(eventLog is OutpostEventLogState.EventLogData.BuildingRepaired buildingRepaired))
										{
											if (!(eventLog is OutpostEventLogState.EventLogData.BuildingWrecked buildingWrecked))
											{
												if (!(eventLog is OutpostEventLogState.EventLogData.CampaignSectionTriggered) && !(eventLog is OutpostEventLogState.EventLogData.CharacterDonated))
												{
													CharacterState character;
													if (!(eventLog is OutpostEventLogState.EventLogData.CharacterLeveledUp characterLeveledUp))
													{
														if (!(eventLog is OutpostEventLogState.EventLogData.CharacterPersonalQuestCompleted characterPersonalQuestCompleted))
														{
															if (!(eventLog is OutpostEventLogState.EventLogData.CharacterRetired characterRetired))
															{
																if (eventLog is OutpostEventLogState.EventLogData.ClassUnlocked classUnlocked)
																{
																	string text = DefRepo.CharacterClasses[classUnlocked.ClassID].ID.ID.Replace("ID", "");
																	return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_CLASS_UNLOCKED"), LocalizationManager.GetTranslation(text + "_FullName"));
																}
																if (eventLog is OutpostEventLogState.EventLogData.GatheredHerbsFromGarden { Herbs: var herbs } gatheredHerbsFromGarden)
																{
																	if (!herbs.HasHerbs)
																	{
																		return string.Empty;
																	}
																	return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_GARDEN_HERBS"), FHUILocFormatting.GetHerbList(gatheredHerbsFromGarden.Herbs));
																}
																ItemDef value;
																if (!(eventLog is OutpostEventLogState.EventLogData.NewBlueprintUnlocked newBlueprintUnlocked))
																{
																	if (eventLog is OutpostEventLogState.EventLogData.NewBuildingUnlocked newBuildingUnlocked)
																	{
																		return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_UNLOCKED"), LocalizationManager.GetTranslation(instance.Buildings.GetBuildingName(newBuildingUnlocked.BuildingID, 0)));
																	}
																	if (!(eventLog is OutpostEventLogState.EventLogData.NewItemsInTrader newItemsInTrader))
																	{
																		if (!(eventLog is OutpostEventLogState.EventLogData.TravellingMerchantEnabled))
																		{
																			if (eventLog is OutpostEventLogState.EventLogData.NewJobPosting newJobPosting)
																			{
																				ScenarioDef jobPostingScenarioDef = GameController.OutpostController.GetJobPostingScenarioDef(newJobPosting.CampaignSectionID);
																				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_JOBPOSTING_ADDED"), LocalizationManager.GetTranslation($"FS_{jobPostingScenarioDef.ID}_NAME"));
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.NewPetCaptured newPetCaptured)
																			{
																				PetDef petDef = DefRepo.Pets[newPetCaptured.PetID];
																				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_PET_CAPTURED"), LocalizationManager.GetTranslation(petDef.Name));
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.ProsperityChanged prosperityChanged)
																			{
																				return string.Format(LocalizationManager.GetTranslation((prosperityChanged.Prosperity > 0) ? "GUI_EVENTLOG_PROSPERITY_GAINED" : "GUI_EVENTLOG_PROSPERITY_LOST"), Mathf.Abs(prosperityChanged.Prosperity));
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.QuestStarted questStarted)
																			{
																				ScenarioDef jobPostingScenarioDef = DefRepo.Scenarios[questStarted.Scenario];
																				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_QUEST_STARTED"), LocalizationManager.GetTranslation($"FS_{jobPostingScenarioDef.ID}_NAME"));
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.SeasonChanged)
																			{
																				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_SEASON_CHANGED"));
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.TownAttackFinished)
																			{
																				return LocalizationManager.GetTranslation("GUI_EVENTLOG_TOWN_ATTACK_FINISHED");
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.TownAttackStarted)
																			{
																				return LocalizationManager.GetTranslation("GUI_EVENTLOG_TOWN_ATTACK_STARTED");
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.TownLevelChanged townLevelChanged)
																			{
																				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_TOWN_LEVEL_INCREASED"), townLevelChanged.TownLevel);
																			}
																			if (eventLog is OutpostEventLogState.EventLogData.WeekChanged weekChanged)
																			{
																				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_WEEK_STARTED"), weekChanged.Week);
																			}
																		}
																		else
																		{
																			OutpostEventsService service = _view.GetService<OutpostEventsService>();
																			if (service != null)
																			{
																				CampaignEffect effect = new CampaignEffect(CampaignEffectType.EnableTravellingMerchantThisWeek);
																				return service.GetFormattedEffect(effect).Text;
																			}
																		}
																	}
																	else if (DefRepo.Items.TryGetValue(newItemsInTrader.ItemID, out value))
																	{
																		return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_ITEMS_TRADER"), ItemDTOUtilities.GetNameLocalized(value));
																	}
																}
																else if (DefRepo.Items.TryGetValue(newBlueprintUnlocked.ItemID, out value))
																{
																	return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_NEW_CRAFTING"), ItemDTOUtilities.GetNameLocalized(value));
																}
															}
															else if (GameController.CharacterController.TryGetRetiredCharacter(characterRetired.CharacterID, out character, throwsException: true))
															{
																return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_HERO_RETIRED"), character.Name);
															}
														}
														else
														{
															PersonalQuestDef personalQuestDef = DefRepo.PersonalQuests[characterPersonalQuestCompleted.PersonalQuestID];
															if (GameController.CharacterController.TryGetCharacter(characterPersonalQuestCompleted.CharacterID, out character))
															{
																return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_HERO_PQ_COMPLETED"), character?.Name, LocalizationManager.GetTranslation(personalQuestDef.Name));
															}
														}
													}
													else if (GameController.CharacterController.TryGetCharacter(characterLeveledUp.CharacterID, out character))
													{
														return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_HERO_LEVELUP"), "<sprite name=" + FHUILocKeys.GetClassTMPIcon(character.ClassID) + "> " + character.Name);
													}
												}
												return string.Empty;
											}
											buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingWrecked.BuildingID);
											return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_WRECKED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingWrecked.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
										}
										buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingRepaired.BuildingID);
										return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_REPAIRED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingRepaired.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
									}
									buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingRebuilt.BuildingID);
									return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_REBUILT"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingRebuilt.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
								}
								buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingDonationCancelled.BuildingID);
								return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_DONATION_CANCELLED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingDonationCancelled.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
							}
							buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingDonationFinished.BuildingID);
							return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_DONATION_FINISHED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingDonationFinished.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
						}
						return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_DONATION_STARTED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingDonationStarted.BuildingID, 0)));
					}
					buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingDefended.BuildingID);
					return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_DEFENDED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingDefended.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
				}
				buildingStateByID = _buildingViewService.GetBuildingStateByID(buildingDamaged.BuildingID);
				return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_DAMAGED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingDamaged.BuildingID, (int)(buildingStateByID?.Level ?? 0))));
			}
			return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_CONSTRUCTION_FINISHED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingConstructionCompleted.BuildingID)));
		}
		return string.Format(LocalizationManager.GetTranslation("GUI_EVENTLOG_BUILDING_ATTACKED"), LocalizationManager.GetTranslation(buildings.GetBuildingName(buildingAttacked.BuildingID)));
	}

	public IEnumerable<OutpostNotificationDTO> GetAvailableNotifications()
	{
		if (!_init)
		{
			Init();
		}
		if (Choreographer.IsInRoadToFrosthaven)
		{
			return Array.Empty<OutpostNotificationDTO>();
		}
		return _notificationsCache.Where((OutpostNotificationDTO n) => !_notificationPlayerData.Contains(n.ID)).Concat(GetNotificationsFromState());
	}

	private void BuildAvailableNotificationsFromEventLog()
	{
		_notificationsCache.Clear();
		_notificationsCache.AddRange((from n in ParseEventLogForNotifications()
			group n by n.ID into g
			select g.Last()).ToList());
	}

	private IEnumerable<OutpostNotificationDTO> ParseEventLogForNotifications()
	{
		OutpostEventLogState.EventLogData[] eventLogData = _eventLog.GetEventsByWeek(_outpostState.Week).ToArray();
		if (eventLogData.OfType<OutpostEventLogState.EventLogData.NewItemsInTrader>().Any())
		{
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.NewTraderItems, "*"), OutpostNotificationType.NewTraderItems, LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_NEW_TRADER_ITEMS_TT"), () =>
			{
				_view.ReplaceState(new ShopViewState());
			});
		}
		if (eventLogData.OfType<OutpostEventLogState.EventLogData.TravellingMerchantEnabled>().Any())
		{
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.TravellingMerchant, "*"), OutpostNotificationType.TravellingMerchant, LocalizationManager.GetTranslation("GUI_TRAVELLING_MERCHANT"), () =>
			{
				_view.ReplaceState(new TravellingMerchantViewState());
			});
		}
		if (eventLogData.OfType<OutpostEventLogState.EventLogData.NewBlueprintUnlocked>().Any())
		{
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.NewCraftingBlueprints, "*"), OutpostNotificationType.NewCraftingBlueprints, LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_NEW_CRAFTING_TT"), () =>
			{
				_view.ReplaceState(new CraftingViewState());
			});
		}
		foreach (OutpostEventLogState.EventLogData.CharacterLeveledUp item in eventLogData.OfType<OutpostEventLogState.EventLogData.CharacterLeveledUp>())
		{
			GameController.CharacterController.TryGetCharacter(item.CharacterID, out var characterState);
			if (characterState == null)
			{
				continue;
			}
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(item), OutpostNotificationType.HeroLeveledUp, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_LEVELUP_TT"), characterState.Name), () =>
			{
				bool flag = _view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, characterState);
				if (flag)
				{
					_view.ReplaceState(new EditCharacterViewState(_characterViewService.PopulateCharacterDTO(characterState), !flag));
				}
			});
		}
		foreach (OutpostEventLogState.EventLogData.CharacterRetired characterEv in eventLogData.OfType<OutpostEventLogState.EventLogData.CharacterRetired>())
		{
			GameController.CharacterController.TryGetCharacter(characterEv.CharacterID, out var characterState2, throwsException: false, searchRetired: true);
			if (characterState2 != null)
			{
				yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(characterEv), OutpostNotificationType.HeroRetired, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_RETIRED_TT"), characterState2.Name), () =>
				{
					_view.ReplaceState(new RetirementStoryViewState(DefRepo.CharacterClasses[characterState2.ClassID], characterEv.AppliedEffects));
				});
			}
		}
		foreach (OutpostEventLogState.EventLogData.CharacterPersonalQuestCompleted item2 in eventLogData.OfType<OutpostEventLogState.EventLogData.CharacterPersonalQuestCompleted>())
		{
			GameController.CharacterController.TryGetCharacter(item2.CharacterID, out var character, throwsException: false, searchRetired: true);
			if (character.WeekRetired == 0)
			{
				yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(item2), OutpostNotificationType.HeroReadyForRetirement, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_READYRETIRE_TT"), character.Name), () =>
				{
					_view.ReplaceState(new ManageRosterViewState());
				});
			}
		}
		foreach (OutpostEventLogState.EventLogData.CampaignSectionTriggered campaignSectionEv in from e in eventLogData.OfType<OutpostEventLogState.EventLogData.CampaignSectionTriggered>()
			where e.SectionID.Valid && !ShouldSkipSectionNotification(DefRepo.CampaignSections[e.SectionID])
			select e)
		{
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(campaignSectionEv), OutpostNotificationType.StoryEvents, campaignSectionEv.SectionID.ID + " triggered!", () =>
			{
				_view.ReplaceState(GetCampaignSectionState(campaignSectionEv));
			});
		}
		if (eventLogData.OfType<OutpostEventLogState.EventLogData.NewJobPosting>().Any())
		{
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.NewJobPosting, "*"), OutpostNotificationType.NewJobPosting, LocalizationManager.GetTranslation("GUI_Notification_new_job_posting"), () =>
			{
				_view.ReplaceState(new ScenarioSelectionViewState());
				_view.PushState(new JobPostingsViewState());
			});
		}
	}

	private IEnumerable<OutpostNotificationDTO> GetNotificationsFromState()
	{
		int maxPets = _buildingViewService.GetMaxPets();
		if (_buildingViewService.HasBuilding<Stables_BuildingTypeDef>() && maxPets != 0 && _outpostState.Pets.Count >= maxPets)
		{
			CVars vars = new CVars();
			UIInfoTools.Instance.GetBuildingConfig(BuildingId.Stables);
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.FullStables, null), OutpostNotificationType.FullStables, LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_STABLES_FULL_TT"), () =>
			{
				_view.ReplaceState(new OutpostVillageMapState(BuildingId.Stables.ToBuildingTypeID()));
			}, null, vars);
		}
		if (_buildingViewService.HasWreckedBuildings)
		{
			foreach (BuildingState wreckedBuilding in _buildingViewService.GetWreckedBuildings())
			{
				yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.BuildingWrecked, wreckedBuilding.ID.ID), OutpostNotificationType.BuildingWrecked, string.Format(LocalizationManager.GetTranslation("GUI_Notification_wrecked_building"), UIInfoTools.Instance.Buildings.GetBuildingName(wreckedBuilding.ID)), () =>
				{
					_view.ReplaceState(new OutpostVillageMapState(null, UIBuildingStateFilter.Wrecked));
				}, null, new CVars());
			}
		}
		if (_buildingViewService.HasEmptyGardenPlots())
		{
			UIInfoTools.Instance.GetBuildingConfig(BuildingId.Garden);
			CVars vars2 = new CVars();
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.EmptyGardenPlot, null), OutpostNotificationType.EmptyGardenPlot, LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_EMPTY_GARDEN_TT"), () =>
			{
				_view.ReplaceState(new OutpostVillageMapState(BuildingId.Garden.ToBuildingTypeID()));
			}, null, vars2);
		}
		if (_buildingViewService.TotalConstructionsRemaining != 0 && (_buildingViewService.HasBuildingsToConstruct || _buildingViewService.HasBuildingsToUpgrade))
		{
			CVars vars3 = new CVars();
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.ConstructionProjectAvailable, null), OutpostNotificationType.ConstructionProjectAvailable, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_CONSTRUCTION_AVAILABLE_TT"), _buildingViewService.TotalConstructionsRemaining), () =>
			{
				FHMapView view = _view;
				UIBuildingStateFilter filter = (_buildingViewService.HasBuildingsToConstruct ? UIBuildingStateFilter.Construct : UIBuildingStateFilter.Upgradeable);
				view.ReplaceState(new OutpostVillageMapState(null, filter));
			}, null, vars3);
		}
		foreach (CharacterState character in GameController.State.ActiveCharacters)
		{
			if (character.ClassDef.MaxCards != character.SelectedCardIDs.Count)
			{
				bool readOnly = !_view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, _view.SelectedCharacter);
				CVars cVars = new CVars();
				cVars.Set("CHARACTER_ID", new TValue(character.ID));
				yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.HeroUnselectedCards, character.ID), OutpostNotificationType.HeroUnselectedCards, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_UNSELECTED_CARDS_TT"), character.Name), () =>
				{
					_view.ReplaceState(new UnlockCardsViewState(_characterViewService.PopulateCharacterDTO(character), readOnly));
				}, null, cVars);
			}
		}
		foreach (CharacterState character2 in GameController.State.ActiveCharacters)
		{
			if (character2.PerkMarks == 0 || !_view.GetService<PerksViewService>().HasUnlockablePerks(character2))
			{
				continue;
			}
			bool ownsCharacter = _view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, character2);
			CVars cVars2 = new CVars();
			cVars2.Set("CHARACTER_ID", new TValue(character2.ID));
			yield return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.HeroUnspentPerkPoints, character2.ID), OutpostNotificationType.HeroUnspentPerkPoints, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_UNSPENT_PERKS_TT"), character2.Name), () =>
			{
				if (ownsCharacter)
				{
					if (_view.SelectedCharacter != character2)
					{
						_view.SetSelectedCharacter(character2, _characterViewService.SlotIdFromCharacterState(character2));
					}
					_view.ReplaceState(new CharacterPerksViewState(!ownsCharacter));
				}
			}, null, cVars2);
		}
	}

	private bool AddOrInvalidateNotificationData(OutpostEventLogState.EventLogData log)
	{
		string id = OutpostNotificationUtils.BuildID(log);
		int num = _notificationsCache.FindIndex((OutpostNotificationDTO n) => n.ID == id);
		OutpostNotificationDTO notificationFromLog = GetNotificationFromLog(log);
		if (num != -1)
		{
			_notificationsCache.RemoveAt(num);
			_notificationPlayerData.Remove(id);
			if (notificationFromLog != null)
			{
				_notificationsCache.Add(notificationFromLog);
				return true;
			}
			return false;
		}
		if (notificationFromLog == null)
		{
			return false;
		}
		_notificationsCache.Add(notificationFromLog);
		return true;
	}

	[CanBeNull]
	private OutpostNotificationDTO GetNotificationFromLog(OutpostEventLogState.EventLogData log)
	{
		CharacterState characterState;
		if (!(log is OutpostEventLogState.EventLogData.CharacterLeveledUp characterLeveledUp))
		{
			if (!(log is OutpostEventLogState.EventLogData.CharacterPersonalQuestCompleted characterPersonalQuestCompleted))
			{
				OutpostEventLogState.EventLogData.CharacterRetired characterRetired = log as OutpostEventLogState.EventLogData.CharacterRetired;
				if ((object)characterRetired == null)
				{
					if (log is OutpostEventLogState.EventLogData.NewBlueprintUnlocked newBlueprintUnlocked)
					{
						_ = DefRepo.Items[newBlueprintUnlocked.ItemID];
						return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.NewCraftingBlueprints, "*"), OutpostNotificationType.NewCraftingBlueprints, LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_NEW_CRAFTING_TT"), () =>
						{
							_view.ReplaceState(new CraftingViewState());
						});
					}
					if (log is OutpostEventLogState.EventLogData.TravellingMerchantEnabled)
					{
						return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.TravellingMerchant, "*"), OutpostNotificationType.TravellingMerchant, LocalizationManager.GetTranslation("GUI_TRAVELLING_MERCHANT"), () =>
						{
							_view.ReplaceState(new TravellingMerchantViewState());
						}, UIInfoTools.Instance.GetGenericIcon("FH_UI_MENU_QuickAccessBar_Outpost_ICO_TravelingMerc_Selected"));
					}
					if (log is OutpostEventLogState.EventLogData.NewItemsInTrader)
					{
						return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.NewTraderItems, "*"), OutpostNotificationType.NewTraderItems, LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_NEW_TRADER_ITEMS_TT"), () =>
						{
							_view.ReplaceState(new ShopViewState());
						});
					}
					if (log is OutpostEventLogState.EventLogData.NewJobPosting newJobPosting)
					{
						return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(OutpostNotificationType.NewJobPosting, newJobPosting.CampaignSectionID.ID), OutpostNotificationType.NewJobPosting, LocalizationManager.GetTranslation("GUI_Notification_new_job_posting"), () =>
						{
							_view.ReplaceState(new ScenarioSelectionViewState());
							_view.PushState(new JobPostingsViewState());
						});
					}
					OutpostEventLogState.EventLogData.CampaignSectionTriggered campaignSectionTriggered = log as OutpostEventLogState.EventLogData.CampaignSectionTriggered;
					if ((object)campaignSectionTriggered != null && campaignSectionTriggered.SectionID.Valid && !ShouldSkipSectionNotification(DefRepo.CampaignSections[campaignSectionTriggered.SectionID]))
					{
						return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(campaignSectionTriggered), OutpostNotificationType.StoryEvents, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_SECTION_TRIGGERED"), LocalizationManager.GetTranslation($"Event_{campaignSectionTriggered.SectionID}_Name")), () =>
						{
							_view.ReplaceState(GetCampaignSectionState(campaignSectionTriggered));
						});
					}
				}
				else if (GameController.CharacterController.TryGetCharacter(characterRetired.CharacterID, out characterState, throwsException: false, searchRetired: true))
				{
					return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(characterRetired), OutpostNotificationType.HeroRetired, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_RETIRED_TT"), characterState.Name), () =>
					{
						_view.ReplaceState(new RetirementStoryViewState(DefRepo.CharacterClasses[characterState.ClassID], characterRetired.AppliedEffects));
					});
				}
			}
			else if (GameController.CharacterController.TryGetCharacter(characterPersonalQuestCompleted.CharacterID, out characterState))
			{
				return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(characterPersonalQuestCompleted), OutpostNotificationType.HeroReadyForRetirement, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_READYRETIRE_TT"), characterState.Name), () =>
				{
					_view.ReplaceState(new ManageRosterViewState());
				});
			}
		}
		else if (GameController.CharacterController.TryGetCharacter(characterLeveledUp.CharacterID, out characterState))
		{
			bool ownsCharacter = _view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, characterState);
			return new OutpostNotificationDTO(OutpostNotificationUtils.BuildID(characterLeveledUp), OutpostNotificationType.HeroLeveledUp, string.Format(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_LEVELUP_TT"), characterState?.Name), () =>
			{
				if (ownsCharacter)
				{
					_view.ReplaceState(new EditCharacterViewState(_view.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(characterState)));
				}
			});
		}
		return null;
	}

	private void LoadPerPlayerState()
	{
		try
		{
			string text = _tagsViewService.GetPlayerTagsByKey("NOTIFICATION_DATA").FirstOrDefault() ?? string.Empty;
			_notificationPlayerData = text.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
		}
		catch (Exception ex)
		{
			Log.GUIError("Parsing per player notification data failed with " + ex.Message + "!");
			_notificationPlayerData = new List<string>();
		}
	}

	private void StorePerPlayerState()
	{
		if (_storeNotificationData)
		{
			_tagsViewService.AddPlayerTag("NOTIFICATION_DATA", string.Join(',', _notificationPlayerData), 1);
		}
	}

	private void ClearPerPlayerState()
	{
		_tagsViewService.ClearPlayerTagsByKey("NOTIFICATION_DATA");
		_notificationPlayerData.Clear();
	}

	public string BuildNotificationGroupTooltip(OutpostNotificationType notificationType, IEnumerable<OutpostNotificationDTO> notifications)
	{
		_tooltipBuilder.Clear();
		try
		{
			OutpostNotificationDTO[] array = (notifications as OutpostNotificationDTO[]) ?? notifications.ToArray();
			switch (notificationType)
			{
			case OutpostNotificationType.ConstructionProjectAvailable:
				return LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_CONSTRUCTION_AVAILABLE_TT");
			case OutpostNotificationType.EmptyGardenPlot:
				return LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_EMPTY_GARDEN_TT");
			case OutpostNotificationType.NewTraderItems:
			{
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_NEW_TRADER_ITEMS_TT"));
				OutpostNotificationDTO[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					uint intVar2 = (uint)array2[i].GetIntVar("ITEM_ID");
					ItemDef itemDef2 = DefRepo.Items[new ItemID(intVar2)];
					_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation(itemDef2.NameLocKey));
				}
				break;
			}
			case OutpostNotificationType.NewCraftingBlueprints:
			{
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_NEW_CRAFTING_TT"));
				OutpostNotificationDTO[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					uint intVar = (uint)array2[i].GetIntVar("ITEM_ID");
					ItemDef itemDef = DefRepo.Items[new ItemID(intVar)];
					_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation(itemDef.NameLocKey));
				}
				break;
			}
			case OutpostNotificationType.NewJobPosting:
			{
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_Notification_new_job_posting"));
				OutpostNotificationDTO[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string stringVar = array2[i].GetStringVar("SECTION_ID");
					_ = DefRepo.CampaignSections[new CampaignSectionID(stringVar)];
					_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("Event_" + stringVar + "_Name"));
				}
				break;
			}
			case OutpostNotificationType.FullStables:
				return LocalizationManager.GetTranslation("GUI_NOTIFICATION_OUTPOST_STABLES_FULL_TT");
			case OutpostNotificationType.TownGuardHasUnspentPoints:
				return LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_UNSPENT_PERKS_TT");
			case OutpostNotificationType.HeroRetired:
			{
				_tooltipBuilder.Append(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_RETIRED_TT"));
				OutpostNotificationDTO[] array2 = array;
				foreach (OutpostNotificationDTO outpostNotificationDTO4 in array2)
				{
					string characterID4 = outpostNotificationDTO4.GetStringVar("CHARACTER_ID");
					CharacterState characterState4 = GameController.State.RetiredCharacters.FirstOrDefault((CharacterState c) => c.ClassID.ID == characterID4);
					_tooltipBuilder.AppendLine(characterState4?.Name);
				}
				break;
			}
			case OutpostNotificationType.HeroReadyForRetirement:
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_READYRETIRE_TT"));
				foreach (OutpostNotificationDTO outpostNotificationDTO3 in array)
				{
					string characterID3 = outpostNotificationDTO3.GetStringVar("CHARACTER_ID");
					CharacterState characterState3 = GameController.State.ActiveCharacters.FirstOrDefault((CharacterState c) => c.ClassID.ID == characterID3);
					_tooltipBuilder.AppendLine(characterState3?.Name);
				}
				break;
			case OutpostNotificationType.HeroLeveledUp:
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_LEVELUP_TT"));
				foreach (OutpostNotificationDTO outpostNotificationDTO2 in array)
				{
					string characterID2 = outpostNotificationDTO2.GetStringVar("CHARACTER_ID");
					CharacterState characterState2 = GameController.State.ActiveCharacters.FirstOrDefault((CharacterState c) => c.ClassID.ID == characterID2);
					if (characterState2 != null)
					{
						_tooltipBuilder.AppendLine(characterState2.Name);
					}
				}
				break;
			case OutpostNotificationType.HeroUnspentPerkPoints:
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_UNSPENT_PERKS_TT"));
				foreach (OutpostNotificationDTO outpostNotificationDTO in array)
				{
					string characterID = outpostNotificationDTO.GetStringVar("CHARACTER_ID");
					CharacterState characterState = GameController.State.ActiveCharacters.FirstOrDefault((CharacterState c) => c.ClassID.ID == characterID);
					_tooltipBuilder.AppendLine(characterState?.Name);
				}
				break;
			case OutpostNotificationType.HeroUnselectedCards:
			{
				_tooltipBuilder.AppendLine(LocalizationManager.GetTranslation("GUI_NOTIFICATION_CHARACTER_UNSELECTED_CARDS_TT"));
				OutpostNotificationDTO[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					CharacterID id = (CharacterID)array2[i].GetStringVar("CHARACTER_ID");
					if (GameController.CharacterController.TryGetCharacter(id, out var character))
					{
						_tooltipBuilder.AppendLine(character.Name);
					}
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("notificationType", notificationType, null);
			case OutpostNotificationType.StoryEvents:
				break;
			}
		}
		catch (Exception ex)
		{
			Log.GUIWarning("Building notification tooltip failed with " + ex.Message);
		}
		return _tooltipBuilder.ToString();
	}

	public bool CanDismiss(OutpostNotificationType notificationType)
	{
		return OutpostNotificationUtils.CanDismiss(notificationType);
	}

	public bool CanDismiss(OutpostNotificationDTO notification)
	{
		return OutpostNotificationUtils.CanDismiss(notification.OutpostNotificationType);
	}

	public bool DismissNotificationGroup(OutpostNotificationType notificationType)
	{
		OutpostNotificationDTO[] source = _notificationsCache.ToArray();
		bool flag = false;
		foreach (OutpostNotificationDTO item in source.Where((OutpostNotificationDTO n) => n.OutpostNotificationType == notificationType))
		{
			if (!_notificationPlayerData.Contains(item.ID))
			{
				_notificationPlayerData.Add(item.ID);
				flag = true;
			}
		}
		if (flag)
		{
			StorePerPlayerState();
		}
		return flag;
	}

	public bool DismissNotification(OutpostNotificationDTO notification)
	{
		if (_notificationPlayerData.Contains(notification.ID))
		{
			return false;
		}
		_notificationPlayerData.Add(notification.ID);
		StorePerPlayerState();
		return true;
	}

	public void OnNotificationGroupClick(OutpostNotificationType notificationType)
	{
		switch (notificationType)
		{
		case OutpostNotificationType.ConstructionProjectAvailable:
			if (_view.GetService<BuildingViewService>().GetAvailableToConstruct().Any())
			{
				_view.ReplaceState(new OutpostVillageMapState(null, UIBuildingStateFilter.Construct));
			}
			break;
		case OutpostNotificationType.EmptyGardenPlot:
			_view.ReplaceState(new OutpostVillageMapState(BuildingId.Garden.ToBuildingTypeID()));
			break;
		case OutpostNotificationType.NewTraderItems:
			_view.ReplaceState(new ShopViewState());
			break;
		case OutpostNotificationType.TravellingMerchant:
			_view.ReplaceState(new TravellingMerchantViewState());
			break;
		case OutpostNotificationType.NewJobPosting:
			_view.ReplaceState(new ScenarioSelectionViewState());
			_view.PushState(new JobPostingsViewState());
			break;
		case OutpostNotificationType.NewCraftingBlueprints:
			_view.ReplaceState(new CraftingViewState());
			break;
		case OutpostNotificationType.FullStables:
			_view.ReplaceState(new OutpostVillageMapState(BuildingId.Stables.ToBuildingTypeID()));
			break;
		case OutpostNotificationType.TownGuardHasUnspentPoints:
			_view.ReplaceState(new TownPerksViewState());
			break;
		case OutpostNotificationType.HeroRetired:
			_view.ReplaceState(new ManageRosterViewState());
			break;
		case OutpostNotificationType.HeroLeveledUp:
			_view.ReplaceState(new EditCharacterViewState());
			break;
		case OutpostNotificationType.HeroUnspentPerkPoints:
			_view.ReplaceState(new CharacterPerksViewState(_view.SelectedCharacter));
			break;
		case OutpostNotificationType.HeroUnselectedCards:
			_view.ReplaceState(new UnlockCardsViewState(_characterViewService.PopulateCharacterDTO(_view.SelectedCharacter), _view.Choreographer.GameClient.GameController.PlayerOwnsCharacter(_view.PlayerID, _view.SelectedCharacter)));
			break;
		case OutpostNotificationType.HeroReadyForRetirement:
			_view.ReplaceState(new ManageRosterViewState());
			break;
		case OutpostNotificationType.BuildingWrecked:
			_view.ReplaceState(new OutpostVillageMapState(null, UIBuildingStateFilter.Wrecked));
			break;
		default:
			throw new ArgumentOutOfRangeException("notificationType", notificationType, null);
		case OutpostNotificationType.StoryEvents:
			break;
		}
	}

	private bool ShouldSkipSectionNotification(OutpostEventLogState.EventLogData.CampaignSectionTriggered se)
	{
		if (se.SectionID.Valid && !DefRepo.CampaignSections[se.SectionID].SkipCompletedNotification)
		{
			return se.AppliedEffects.Any((CampaignEffect e) => _typesToSkip.Contains(e.Type));
		}
		return true;
	}

	public static bool ShouldSkipSectionNotification(CampaignSectionDef def)
	{
		if (!def.SkipCompletedNotification)
		{
			return def.Effects.Any((CampaignEffect e) => _typesToSkip.Contains(e.Type));
		}
		return true;
	}

	private LinkingSectionDTO GetLinkingSectionDTO(OutpostEventLogState.EventLogData.CampaignSectionTriggered sectionTriggered)
	{
		return _view.GetService<OutpostEventsService>().GetLinkingSectionDTO(DefRepo.CampaignSections[sectionTriggered.SectionID], sectionTriggered.AppliedEffects);
	}

	private LinkingSectionDTO GetLinkingSectionDTO(CampaignSectionID id)
	{
		OutpostEventLogState.EventLogData.CampaignSectionTriggered sectionFromEventLog = GetSectionFromEventLog(id);
		if (!(sectionFromEventLog == null))
		{
			return GetLinkingSectionDTO(sectionFromEventLog);
		}
		return _view.GetService<OutpostEventsService>().GetLinkingSectionDTO(DefRepo.CampaignSections[id], new List<CampaignEffect>());
	}

	private OutpostEventLogState.EventLogData.CampaignSectionTriggered GetSectionFromEventLog(CampaignSectionID id)
	{
		return (OutpostEventLogState.EventLogData.CampaignSectionTriggered)_eventLog.Events.FirstOrDefault((OutpostEventLogState.EventLogData e) => e is OutpostEventLogState.EventLogData.CampaignSectionTriggered campaignSectionTriggered && campaignSectionTriggered.SectionID == id);
	}

	private IEnumerable<LinkingSectionDTO> GetLinkedSections(OutpostEventLogState.EventLogData.CampaignSectionTriggered eventLog)
	{
		if (eventLog.AppliedEffects == null || !eventLog.AppliedEffects.Any())
		{
			return Array.Empty<LinkingSectionDTO>();
		}
		int found = _eventLog.Events.IndexOf(eventLog) + 1;
		int count = _eventLog.Events.Count;
		_sectionTriggered.Clear();
		OutpostEventLogState.EventLogData.CampaignSectionTriggered campaignSectionTriggered = eventLog;
		while (campaignSectionTriggered != null)
		{
			campaignSectionTriggered = GetNextSection(campaignSectionTriggered, found, count, out found);
			if (campaignSectionTriggered != null)
			{
				_sectionTriggered.Add(campaignSectionTriggered);
			}
		}
		return _sectionTriggered.Select(GetLinkingSectionDTO);
	}

	[CanBeNull]
	private OutpostEventLogState.EventLogData.CampaignSectionTriggered GetNextSection(OutpostEventLogState.EventLogData.CampaignSectionTriggered s, int from, int to, out int found)
	{
		found = -1;
		if (s.AppliedEffects == null || !s.AppliedEffects.Any())
		{
			return null;
		}
		CampaignEffect[] appliedEffects = s.AppliedEffects;
		for (int i = 0; i < appliedEffects.Length; i++)
		{
			CampaignEffect campaignEffect = appliedEffects[i];
			switch (campaignEffect.Type)
			{
			case CampaignEffectType.ActivateCampaignSection:
			{
				OutpostEventLogState.EventLogData.CampaignSectionTriggered eventLogForSection = GetEventLogForSection(campaignEffect.Param1, from, to, out found);
				if (eventLogForSection != null)
				{
					return eventLogForSection;
				}
				break;
			}
			case CampaignEffectType.IfRaceActivateCampaignSections:
			case CampaignEffectType.IfTraitActivateCampaignSections:
			{
				OutpostEventLogState.EventLogData.CampaignSectionTriggered eventLogForSection = GetEventLogForSection(campaignEffect.Param2, from, to, out found);
				if (eventLogForSection != null)
				{
					return eventLogForSection;
				}
				eventLogForSection = GetEventLogForSection(campaignEffect.Param3, from, to, out found);
				if (eventLogForSection != null)
				{
					return eventLogForSection;
				}
				break;
			}
			case CampaignEffectType.IfScenarioActivateCampaignSection:
			{
				OutpostEventLogState.EventLogData.CampaignSectionTriggered eventLogForSection = GetEventLogForSection(campaignEffect.Param2, from, to, out found);
				if (eventLogForSection != null)
				{
					return eventLogForSection;
				}
				break;
			}
			}
		}
		return null;
	}

	private OutpostEventLogState.EventLogData.CampaignSectionTriggered GetEventLogForSection(string id, int from, int to, out int indexFound)
	{
		indexFound = -1;
		for (int i = from; i < to; i++)
		{
			if (_eventLog.Events[i] is OutpostEventLogState.EventLogData.CampaignSectionTriggered campaignSectionTriggered && !(campaignSectionTriggered.SectionID.ID != id))
			{
				indexFound = i;
				return campaignSectionTriggered;
			}
		}
		return null;
	}

	public void OpenStoryEvents()
	{
		HasUnseenStoryEvents = false;
		_view.PushState(new StoryEventLogViewState());
	}

	public void OpenCampaignSectionView(StoryEventDTO data)
	{
		OutpostEventLogState.EventLogData.CampaignSectionTriggered sectionFromEventLog = GetSectionFromEventLog(data.ID);
		if (sectionFromEventLog != null)
		{
			_view.PushState(GetCampaignSectionState(sectionFromEventLog));
		}
	}

	private ViewCampaignSectionViewState GetCampaignSectionState(OutpostEventLogState.EventLogData.CampaignSectionTriggered se)
	{
		return new ViewCampaignSectionViewState(GetLinkingSectionDTO(se), GetLinkedSections(se).ToArray());
	}

	public UIStoryEventsNotification.StoryEventParams GetStoryEventParams()
	{
		return new UIStoryEventsNotification.StoryEventParams
		{
			HasNew = HasUnseenStoryEvents,
			OpenStoryEvents = OpenStoryEvents,
			Events = GetStoryEvents().ToArray()
		};
	}

	private IEnumerable<(int, OutpostEventLogState.EventLogData.CampaignSectionTriggered)> GetStoryEventsFromLog()
	{
		for (int i = 0; i < _eventLog.Events.Count; i++)
		{
			if (_eventLog.Events[i] is OutpostEventLogState.EventLogData.CampaignSectionTriggered { SectionID: { Valid: not false } } campaignSectionTriggered && !DefRepo.CampaignSections[campaignSectionTriggered.SectionID].SkipCompletedNotification && !ShouldSkipSectionNotification(campaignSectionTriggered))
			{
				yield return (i, campaignSectionTriggered);
			}
		}
	}

	public IEnumerable<StoryEventDTO> GetStoryEvents()
	{
		if (_view.Choreographer.IsInRoadToFrosthaven)
		{
			return Array.Empty<StoryEventDTO>();
		}
		return from data in GetStoryEventsFromLog()
			select new StoryEventDTO(DefRepo.CampaignSections[data.Item2.SectionID], data.Item2.Week, (uint)data.Item1, data.Item2.Week == _outpostState.Week);
	}

	private string GetStoryEventTagKey(CampaignSectionID id, string storyID)
	{
		return "EL:" + id.ID + ":" + storyID;
	}

	public void MarkStoryEventAsSeen(StoryEventDTO data)
	{
	}

	private bool IsStoryEventSeen(StoryEventDTO data)
	{
		return IsStoryEventSeen(data.ID, data.StoryID.ToString());
	}

	private bool IsStoryEventSeen(CampaignSectionID id, string eventLogID)
	{
		return _tagsViewService.GetPlayerTagsByKey(GetStoryEventTagKey(id, eventLogID)).Any();
	}

	private bool IsStoryEventSeen(CampaignSectionID id)
	{
		return IsStoryEventSeen(id, "");
	}

	public void ShowToastNotification(string text, Action onClick = null)
	{
		_view.Modules.ToastNotifications.ShowNotification(text);
	}

	public bool IsDemoCompleted()
	{
		if (DbgShowDemoCompleted_kvar)
		{
			return true;
		}
		if (ReleaseStage.IsDemo())
		{
			return !GameController.ScenarioController.GetUnlockedScenarios().Any((FHRL.ScenarioState x) => !x.Completed);
		}
		return false;
	}

	[ConsoleCommand(Command = "store_notification_data", RequiredContext = typeof(FHMapChoreographer), Synchronize = true, Description = "Choose whether to store notification data in ui tags")]
	public static void StoreNotificationData(IConsole console, bool arg0)
	{
		OutpostNotificationViewService service = Singleton<FHMapChoreographer>.InstanceFast.View.GetService<OutpostNotificationViewService>();
		service._storeNotificationData = arg0;
		if (arg0)
		{
			service.StorePerPlayerState();
		}
	}
}
