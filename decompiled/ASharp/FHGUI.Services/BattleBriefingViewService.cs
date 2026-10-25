using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.States;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Services;

public class BattleBriefingViewService : FHMapViewService, IReadyCheckProcessor
{
	public enum BriefingStepTags
	{
		BattleGoals,
		Challenge,
		Pets,
		Favor,
		Summary
	}

	public interface IBriefingStepParamsProvider
	{
		BriefingStepTags StepTag { get; }

		string TitleLocKey { get; }

		string DescriptionLocKey { get; }

		string[] DescriptionArgs { get; }

		IEnumerable<ButtonsBarParam> GetButtonsBarParams(bool canContinue);
	}

	private class BriefingSnapshot
	{
		public List<ChallengeID> Challenges;

		public List<PetID> Pets;

		public List<ScenarioFavorToBeUsed> Favors;

		public List<BattleGoalID?> BattleGoals;

		public List<string> Tags;

		public bool SameAs(BriefingSnapshot other)
		{
			if (Challenges.SequenceEqual(other.Challenges) && Pets.SequenceEqual(other.Pets) && Favors.SequenceEqual(other.Favors) && BattleGoals.SequenceEqual(other.BattleGoals))
			{
				return Tags.SequenceEqual(other.Tags);
			}
			return false;
		}
	}

	private const string PROCEED_VOTING_COMMAND = "SCENARIO_BRIEFING_PROCEED";

	private const string UI_TAG_PETS = "PETS";

	private const string UI_TAG_CHALLENGES = "CHALLENGES";

	private const string UI_TAG_FAVORS = "FAVORS";

	private readonly PartyViewService _partyViewService;

	private readonly UIElementTagsViewService _tagsViewService;

	private BriefingSnapshot _lastBriefingSnapshot;

	public BattleBriefingViewService(FHMapView view, PartyViewService partyViewService, UIElementTagsViewService tagsViewService)
		: base(view)
	{
		_partyViewService = partyViewService;
		_tagsViewService = tagsViewService;
	}

	private CharacterState GetNextOrCurrentPendingOwnedCharacter()
	{
		if (_view.GameState.ActiveScenario.CharacterScenarioSettings.Any((ActiveScenarioState.CharacterSettings css) => css.CharacterID == _view.SelectedCharacter.ID && !css.SelectedBattleGoal.HasValue))
		{
			return _view.SelectedCharacter;
		}
		IEnumerable<CharacterID> ownedIds = _view.PlayerCharacters.Select((CharacterState c) => c.ID);
		ActiveScenarioState.CharacterSettings nextCSS = _view.GameState.ActiveScenario.CharacterScenarioSettings.FirstOrDefault((ActiveScenarioState.CharacterSettings css) => ownedIds.Contains(css.CharacterID) && !css.SelectedBattleGoal.HasValue);
		if (nextCSS == null)
		{
			return null;
		}
		return _view.PlayerCharacters.First((CharacterState c) => nextCSS.CharacterID == c.ID);
	}

	public bool IsApplicableReadyCheckCommand(string readyCheckID)
	{
		return readyCheckID == "SCENARIO_BRIEFING_PROCEED";
	}

	public bool ProcessReadyCheckResult(MultiplayerGameController.Result readyCheckResult)
	{
		if (!IsApplicableReadyCheckCommand(readyCheckResult.ReadyCheckID))
		{
			return false;
		}
		if (readyCheckResult.Action == MultiplayerGameController.Action.AllPlayersReady)
		{
			StartIntro();
		}
		return true;
	}

	public void StartIntro()
	{
		if (_view.GameState.SelectedScenario.Def.NoIntroOutro)
		{
			EnterActiveScenario();
			return;
		}
		_view.ReplaceState(new NarrationViewState(NarrationSetting.FromIntro(_view.GameState.SelectedScenario, _view.GameController), popStateOnDone: false)
		{
			OnDone = () =>
			{
				if (_view.IsPartyLeader)
				{
					if (_view.GameState.SelectedScenario.Def.LevelCondition == ScenarioDef.LevelConditionType.FS93)
					{
						_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.AddChooseAlternativeScenarioLevelAction, _view.GameState.SelectedScenario.Def);
					}
					else
					{
						EnterActiveScenario();
					}
				}
			}
		});
	}

	public void EnterActiveScenario()
	{
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.EnterActiveScenario);
	}

	public void ClearUITags()
	{
		_tagsViewService.ClearPlayerTagGroup("PETS");
		_tagsViewService.ClearPlayerTagGroup("CHALLENGES");
		_tagsViewService.ClearPlayerTagGroup("FAVORS");
	}

	public void AbandonScenario()
	{
		if (!_view.IsPartyLeader)
		{
			Log.GUITrace("Only Party Leader can abandon a scenario");
			return;
		}
		if (_view.GameController.State.SelectedScenario == null)
		{
			Log.GUITrace("Tried to complete null active scenario");
			return;
		}
		ClearUITags();
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.AbandonActiveScenario);
	}

	public void StartScenarioBriefing()
	{
		if (_view.IsPartyLeader && !_view.GameState.ActiveScenario.HasStartedBriefing)
		{
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.StartScenarioBriefing);
		}
	}

	[Obsolete]
	public void MarkMissingBattleGoals(List<PartySlotDTO> partySlots)
	{
		List<CharacterID> list = _view.PlayerCharacters.Select((CharacterState c) => c.ID).ToList();
		int i;
		for (i = 0; i < partySlots.Count; i++)
		{
			if (partySlots[i].Character != ECharacter.None && list.Contains(partySlots[i].CharacterID))
			{
				ActiveScenarioState.CharacterSettings characterSettings = _view.GameState.ActiveScenario.CharacterScenarioSettings.FirstOrDefault((ActiveScenarioState.CharacterSettings css) => css.CharacterID == partySlots[i].CharacterID);
				if (characterSettings != null && !characterSettings.SelectedBattleGoal.HasValue)
				{
					partySlots[i].ForceAlert = true;
				}
			}
		}
	}

	public List<GoalDTO> GetBattleGoals(CharacterState characterState)
	{
		List<GoalDTO> list = new List<GoalDTO>();
		BattleGoalDef selectedBattleGoal = _view.GameController.ScenarioController.GetSelectedBattleGoal(characterState);
		foreach (BattleGoalDef characterBattleGoal in _view.GameController.ScenarioController.GetCharacterBattleGoals(characterState))
		{
			GoalDTO goalDTO = new GoalDTO(characterBattleGoal, _view.GameState.PartyCharacters.Count());
			if (selectedBattleGoal == characterBattleGoal)
			{
				goalDTO.IsChecked = true;
			}
			list.Add(goalDTO);
		}
		return list;
	}

	public IEnumerable<UIPartySlotHints.HintedActionParams> GetPartySlotInteractions(PartySlotDTO partySlotDto)
	{
		if (_partyViewService.TryGetCharacter(partySlotDto, out var _, out var isSlotOwner) && !IsPlayerVotedReady())
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, isSlotOwner ? "GUI_EDIT_CHARACTER" : "GUI_VIEW_CHARACTER"),
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = () =>
				{
					_partyViewService.ManagePartySlot(partySlotDto);
				},
				ColorSchemeID = "EditPermanent"
			};
			yield return new UIPartySlotHints.HintedActionParams
			{
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					_partyViewService.ManagePartySlot(partySlotDto);
				}
			};
		}
	}

	public List<ScenarioBriefingPartyModule.BriefingSlotData> GetBriefingSlotDatas()
	{
		return _partyViewService.GetPartySlotsData(null).Select((PartySlotDTO partySlotDto) =>
		{
			ScenarioBriefingPartyModule.BriefingSlotData briefingSlotData = new ScenarioBriefingPartyModule.BriefingSlotData
			{
				PartySlotDto = partySlotDto,
				SelectedGoalDto = null,
				GoalDtos = null,
				InfoLiteral = null
			};
			if (!_partyViewService.TryGetCharacter(partySlotDto, out var characterState, out var isSlotOwner))
			{
				return briefingSlotData;
			}
			briefingSlotData.Warrnings = GetCharacterSetupWarnings(characterState);
			List<GoalDTO> battleGoals = GetBattleGoals(characterState);
			GoalDTO goalDTO = battleGoals.FirstOrDefault((GoalDTO g) => g.IsChecked);
			if (!isSlotOwner)
			{
				briefingSlotData.InfoLiteral = ((goalDTO != null) ? FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_READY"), FHUIColors.MULTIPLAYER_READY) : FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_CHOOSING"), FHUIColors.MULTIPLAYER_PENDING));
				return briefingSlotData;
			}
			briefingSlotData.GoalDtos = battleGoals;
			briefingSlotData.SelectedGoalDto = goalDTO;
			return briefingSlotData;
		}).ToList();
	}

	private IEnumerable<SpriteWithTooltipDTO> GetCharacterSetupWarnings(CharacterState characterState)
	{
		if (characterState != null)
		{
			if (characterState.ClassDef.MaxCards != characterState.SelectedCardIDs.Count)
			{
				yield return new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetGenericIcon("LockedCards_Icon"),
					Description = LocalizationManager.GetTranslation("GUI_PREREQUISITE_FULL_CARDHAND")
				};
			}
			if (!_view.GameController.ScenarioController.CanUsePotions && characterState.EquipedItems.Any((ItemDef i) => i.CanBeBrewed))
			{
				yield return new SpriteWithTooltipDTO
				{
					Sprite = UIInfoTools.Instance.GetGenericIcon("FH_UI_Icon_DisabledAlchemy"),
					Description = LocalizationManager.GetTranslation("GUI_PREREQUISITE_CANNOT_USE_POTIONS")
				};
			}
		}
	}

	public void HandleBriefingVoting(out ButtonsBarParam continueParams)
	{
		if (_view.IsReadyCheckRunning && _view.MultiplayerGameState.ReadyCheckID != "SCENARIO_BRIEFING_PROCEED" && _view.IsPartyLeader)
		{
			Log.Error("Another voting in progress [" + _view.MultiplayerGameState.ReadyCheckID + "] while in battle briefing screen.");
			Choreographer.ExecuteCommand("CancelReadyCheck", _view.PlayerID);
		}
		bool flag = _view.GameController.ScenarioController.CanEnterActiveScenario();
		bool flag2 = _view.GetService<ScenarioViewService>().ValidateScenarioPlayers();
		if (_view.IsSoloGame)
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_BATTLE_BRIEFING_START",
				Callback = () =>
				{
					StartIntro();
				},
				Interactable = flag,
				Prototype = _view.Modules.Characters.MainScreen._forwardButton
			};
			return;
		}
		if (_view.IsPartyLeader && _view.IsReadyCheckRunning && _view.MultiplayerGameState.ReadyCheckID == "SCENARIO_BRIEFING_PROCEED" && !flag2 && !flag)
		{
			_view.GetService<ScenarioViewService>()?.CancelScenarioReadyCheck();
		}
		if (_view.IsPartyLeader)
		{
			if (_view.IsReadyCheckRunning)
			{
				continueParams = new ButtonsBarParam
				{
					LabelKey = "GUI_CANCEL",
					Callback = () =>
					{
						Choreographer.ExecuteCommand("CancelReadyCheck", _view.PlayerID);
					},
					HintKey = "GUI_WAITING_PARTY_PLAYERS",
					Interactable = flag,
					Prototype = _view.Modules.Characters.MainScreen._forwardButton
				};
			}
			else
			{
				continueParams = new ButtonsBarParam
				{
					LabelKey = "GUI_START_VOTE",
					Callback = () =>
					{
						Choreographer.ExecuteCommand("StartReadyCheck", _view.PlayerID, "SCENARIO_BRIEFING_PROCEED");
					},
					Interactable = (flag & flag2),
					Prototype = _view.Modules.Characters.MainScreen._forwardButton
				};
			}
		}
		else
		{
			bool flag3 = _view.PlayerCharacters.Any();
			bool playerVotedReady = flag3 && IsPlayerVotedReady();
			continueParams = new ButtonsBarParam
			{
				LabelKey = (playerVotedReady ? "GUI_CANCEL" : "GUI_READY"),
				Callback = () =>
				{
					Choreographer.ExecuteCommand("SetReady", _view.PlayerID, !playerVotedReady);
				},
				HintKey = ((!_view.IsReadyCheckRunning | playerVotedReady) ? "GUI_WAITING_PARTY_LEADER" : ""),
				Interactable = ((flag & flag3) && _view.IsReadyCheckRunning),
				Prototype = _view.Modules.Characters.MainScreen._forwardButton
			};
		}
	}

	public bool IsPlayerVotedReady()
	{
		if (_view.IsReadyCheckRunning)
		{
			return _view.Player.Ready;
		}
		return false;
	}

	private BriefingStepTags GetNextStepTag(BriefingStepTags currentTag)
	{
		List<UIBreadcrumbs.Params> briefingCrumbsParams = GetBriefingCrumbsParams();
		int num = briefingCrumbsParams.FindIndex((UIBreadcrumbs.Params p) => p.FlowTag == (byte)currentTag);
		if (num >= 0 && num < briefingCrumbsParams.Count - 1)
		{
			return (BriefingStepTags)briefingCrumbsParams[num + 1].FlowTag;
		}
		return BriefingStepTags.Summary;
	}

	public IEnumerable<ButtonsBarParam> GetCommonButtonsBarParams(BriefingStepTags stepTag, bool isFromSummary, bool canGoBack, bool canContinue, FHMapViewLayer.LayerType viewLayerType)
	{
		if (isFromSummary)
		{
			yield return GetReturnToSummaryParam(viewLayerType);
			yield break;
		}
		yield return GetCommonBackParam(canGoBack, viewLayerType);
		yield return GetCommonContinueParam(stepTag, canContinue);
	}

	public ButtonsBarParam GetReturnToSummaryParam(FHMapViewLayer.LayerType viewLayerType)
	{
		return new ButtonsBarParam
		{
			LabelKey = "GUI_CONTINUE",
			Callback = () =>
			{
				_view.PopState(viewLayerType);
			},
			Interactable = true
		};
	}

	public ButtonsBarParam GetCommonBackParam(bool isInteractable, FHMapViewLayer.LayerType viewLayerType)
	{
		return new ButtonsBarParam
		{
			LabelKey = "GUI_BACK",
			Callback = () =>
			{
				_view.PopState(viewLayerType);
			},
			Interactable = isInteractable,
			Prototype = _view.Modules.Characters.MainScreen._backButton
		};
	}

	private bool IsSkipableStep(BriefingStepTags step)
	{
		if ((uint)(step - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	private bool IsFulfilledStep(BriefingStepTags step)
	{
		return step switch
		{
			BriefingStepTags.Favor => HasFavorsEnabled() && GetFavorDifficulty() != 0, 
			BriefingStepTags.Challenge => HasChallengesEnabled() && GetCurrentChallengePicks() > 0, 
			BriefingStepTags.Pets => HasPetsEnabled() && GetCurrentPetPicksCount() > 0, 
			BriefingStepTags.BattleGoals => true, 
			BriefingStepTags.Summary => true, 
			_ => true, 
		};
	}

	public ButtonsBarParam GetCommonContinueParam(BriefingStepTags currentStepTag, bool isInteractable)
	{
		bool flag = IsSkipableStep(currentStepTag) && !IsFulfilledStep(currentStepTag);
		return new ButtonsBarParam
		{
			LabelKey = (flag ? "GUI_SKIP" : "GUI_CONTINUE"),
			Callback = () =>
			{
				GoToBriefingStepState(GetNextStepTag(currentStepTag));
			},
			Interactable = isInteractable,
			Prototype = _view.Modules.Characters.MainScreen._forwardButton
		};
	}

	public void GoToBriefingStepState(BriefingStepTags stepTag, bool isFromSummery = false)
	{
		FHMapViewState fHMapViewState = stepTag switch
		{
			BriefingStepTags.BattleGoals => (FHMapViewState)new BattleGoalSelectionViewState(isFromSummery), 
			BriefingStepTags.Challenge => new ChallengeSelectionViewState(isFromSummery), 
			BriefingStepTags.Pets => new PetsSelectionViewState(isFromSummery), 
			BriefingStepTags.Favor => new FavorsSelectionViewState(isFromSummery), 
			BriefingStepTags.Summary => new ScenarioBriefingSummaryViewState(), 
			_ => throw new ArgumentOutOfRangeException("stepTag", stepTag, null), 
		};
		if (fHMapViewState != null)
		{
			_view.SetState(fHMapViewState);
		}
	}

	public List<UIBreadcrumbs.Params> GetBriefingCrumbsParams()
	{
		List<UIBreadcrumbs.Params> list = new List<UIBreadcrumbs.Params>();
		if (HasBattleGoals())
		{
			list.Add(new UIBreadcrumbs.Params
			{
				FlowTag = 0,
				Text = LocalizationManager.GetTranslation("GUI_BATTLE_GOALS")
			});
		}
		if (HasChallengesEnabled())
		{
			list.Add(new UIBreadcrumbs.Params
			{
				FlowTag = 1,
				Text = LocalizationManager.GetTranslation("GUI_CHALLENGES")
			});
		}
		if (HasPetsEnabled())
		{
			list.Add(new UIBreadcrumbs.Params
			{
				FlowTag = 2,
				Text = LocalizationManager.GetTranslation("GUI_PETS")
			});
		}
		if (HasFavorsEnabled())
		{
			list.Add(new UIBreadcrumbs.Params
			{
				FlowTag = 3,
				Text = LocalizationManager.GetTranslation("GUI_FAVORS")
			});
		}
		if (HasMultipleSteps())
		{
			list.Add(new UIBreadcrumbs.Params
			{
				FlowTag = 4,
				Text = LocalizationManager.GetTranslation("GUI_SUMMARY")
			});
		}
		return list;
	}

	public void PopulateCommonModules(CharacterDetailsScreenModule charDetailsModule, ScenarioLauncherModule scenarioLauncherModule, IBriefingStepParamsProvider paramsProvider, SettingsShortcutsModule shortcuts, bool canContinue)
	{
		charDetailsModule.ClearAll();
		charDetailsModule.SetButtonsBar(paramsProvider.GetButtonsBarParams(canContinue).ToArray());
		charDetailsModule.ShowBreadcrumbs(GetBriefingCrumbsParams(), (byte)paramsProvider.StepTag);
		string titleLiteral = (paramsProvider.TitleLocKey.IsNOTNullOrEmpty() ? LocalizationManager.GetTranslation(paramsProvider.TitleLocKey) : string.Empty);
		string text = (paramsProvider.DescriptionLocKey.IsNOTNullOrEmpty() ? LocalizationManager.GetTranslation(paramsProvider.DescriptionLocKey) : string.Empty);
		if (paramsProvider.DescriptionArgs != null)
		{
			string format = text;
			object[] descriptionArgs = paramsProvider.DescriptionArgs;
			text = string.Format(format, descriptionArgs);
		}
		charDetailsModule.SetDescriptionLiteral(titleLiteral, text);
		charDetailsModule.SetPadding(new Vector4(0f, 0f, -366f, 0f));
		int scenarioLevel = GameController.ScenarioController.GetScenarioLevel();
		int partySize = _view.GameState.PartyCharacters.Count();
		ScenarioInfoDTO scenarioInfoDto = new ScenarioInfoDTO(GameController, _view.GameController.State.SelectedScenario, partySize, scenarioLevel, isPlayable: true);
		scenarioLauncherModule.Init(scenarioInfoDto, null, default, hasStartQuestAuthority: true);
		scenarioLauncherModule.SetOffset(new Vector2(0f, 0f), new Vector2(-396f, -83f), new Vector2(-10f, 184f));
		shortcuts.Init(new SettingsShortcutsModule.Params
		{
			OnSettingsClicked = () =>
			{
				Singleton<ESCMenu>.InstanceFast?.Show();
			},
			OnWikiClicked = () =>
			{
				Singleton<FHUICodex>.InstanceFast?.Toggle();
			}
		});
	}

	public bool HasMultipleSteps()
	{
		if (!HasChallengesEnabled() && !HasPetsEnabled())
		{
			return HasFavorsEnabled();
		}
		return true;
	}

	public bool HasChallengesEnabled()
	{
		if (_view.GameController.ScenarioController.CanUseChallenges)
		{
			return _view.GameState.ActiveScenario.DrawnChallengeIDs.Any();
		}
		return false;
	}

	public bool HasBattleGoals()
	{
		MultiplayerGameController.PartySlotDesc[] array = _view.MultiplayerGameController.GetSlots().ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			MultiplayerGameController.PartySlotDesc partySlotDesc = array[i];
			if (partySlotDesc.Character != null && _view.GameController.ScenarioController.GetCharacterBattleGoals(partySlotDesc.Character).Any())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPetsEnabled()
	{
		return _view.GameController.PetController.CanBringPets;
	}

	public bool HasFavorsEnabled()
	{
		return _view.GameController.ScenarioController.FavorsEnabled;
	}

	public IEnumerable<string> GetAdvanceHints(bool isFinalStep)
	{
		bool flag = _view.MultiplayerGameController.GetSlots().All((MultiplayerGameController.PartySlotDesc slot) => slot.Character == null || _view.GameController.ScenarioController.GetSelectedBattleGoal(slot.Character) != null);
		bool localPlayerValidDeck = _view.PlayerCharacters.All((CharacterState slot) => slot == null || slot.ClassDef.MaxCards == slot.SelectedCardIDs.Count);
		bool allPlayerValidDeck = _view.MultiplayerGameController.GetSlots().All((MultiplayerGameController.PartySlotDesc slot) => slot.Character == null || slot.Character.ClassDef.MaxCards == slot.Character.SelectedCardIDs.Count);
		bool allPlayersValidPotions = _view.GameController.ScenarioController.CanUsePotions || !_view.PlayerCharacters.Any((CharacterState slot) => slot.EquipedItems.Any((ItemDef i) => i.CanBeBrewed));
		if (!flag)
		{
			yield return "GUI_BATTLE_BRIEFING_PREREQUISITE_GOALS";
		}
		if (!localPlayerValidDeck)
		{
			yield return "GUI_PREREQUISITE_FULL_CARDHAND";
		}
		if (isFinalStep && _view.IsPartyLeader && !allPlayerValidDeck)
		{
			yield return "GUI_PREREQUISITE_FULL_CARDHAND";
		}
		if (!allPlayersValidPotions)
		{
			yield return "GUI_PREREQUISITE_CANNOT_USE_POTIONS";
		}
	}

	public List<SuggestibleSlotDTO> GetPets(bool onlySelected = false)
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		bool isSoloGame = _view.IsSoloGame;
		int petCapacity = _view.GameController.PetController.GetPetCapacity();
		List<PetDef> list2 = _view.GameController.PetController.GetOwnedPets().ToList();
		for (int i = 0; i < petCapacity; i++)
		{
			PetDef petDef = ((list2.Count > i) ? list2[i] : null);
			if (!onlySelected || (petDef != null && IsPetPicked(petDef.ID)))
			{
				if (petDef != null)
				{
					string tag = _tagsViewService.FormatTag("PETS", petDef.ID.ToString());
					list.Add(new SuggestibleSlotDTO
					{
						Id = petDef.ID.ToString(),
						NameLiteral = LocalizationManager.GetTranslation(petDef.Name),
						InfoLiteral = null,
						TooltipLiteral = LocalizationManager.GetTranslation(petDef.Description),
						Points = 1,
						Image = UIInfoTools.Instance.GetItemMiniSprite(petDef.ArtKey),
						IsSelected = IsPetPicked(petDef.ID),
						IsSuggested = IsPetSuggested(petDef.ID, byPartyLeader: true),
						Interactable = true,
						SuggestingUsers = ((!isSoloGame) ? _tagsViewService.GetMultiplayerUsersWithTag(tag).ToList() : null)
					});
				}
				else
				{
					list.Add(new SuggestibleSlotDTO
					{
						InfoLiteral = ((i < petCapacity) ? LocalizationManager.GetTranslation("GUI_EMPTY") : FHUILocFormatting.PopulateParams("GUI_PETS_SLOT_UNLOCK", _view.GetService<BuildingViewService>().GetSlotUnlockAtLevel(i))),
						IsEmpty = true,
						IsSelected = false
					});
				}
			}
		}
		return list;
	}

	public int GetTotalPetPicks()
	{
		return _view.GameController.PetController.GetMaxBringPets();
	}

	public int GetCurrentPetPicks()
	{
		return _view.GameState?.ActiveScenario.PetIDs.Count ?? 0;
	}

	public UIPrefabTooltipTarget.TooltipInitParams GetPetSlotTooltipParams(string petId, bool ignoreActionHints = false)
	{
		if (petId.IsNullOrEmpty())
		{
			return null;
		}
		PetDef petDef = _view.GameController.DefRepository.Pets.First((KeyValuePair<PetID, PetDef> pair) =>
		{
			uint iD = pair.Value.ID.ID;
			return iD.ToString() == petId;
		}).Value;
		string actionHint;
		if (_view.IsPartyLeader)
		{
			bool num = _view.GameState.ActiveScenario.PetIDs.Count < _view.GameController.PetController.GetMaxBringPets() && !IsPetPicked(petDef.ID);
			bool flag = IsPetPicked(petDef.ID);
			if (num)
			{
				actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD");
			}
			else if (flag)
			{
				actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_REMOVE");
			}
			else
			{
				actionHint = string.Empty;
			}
		}
		else if (IsPetSuggested(petDef.ID, byPartyLeader: false))
		{
			actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VOTE_REVOKE");
		}
		else if (_tagsViewService.GetPlayerTagsByKey("PETS").Count() < _view.GameController.PetController.GetMaxBringPets())
		{
			actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VOTE");
		}
		else
		{
			actionHint = null;
		}
		return new UIPrefabTooltipTarget.TooltipInitParams
		{
			Prefab = null,
			OnObjectCreatedCallback = (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = UIInfoTools.Instance.GetItemMiniSprite(petDef.ArtKey),
					CaptionLiteral = LocalizationManager.GetTranslation(petDef.Name),
					DescriptionLiteral = LocalizationManager.GetTranslation(petDef.Description),
					Hints = ((!ignoreActionHints) ? actionHint : string.Empty)
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			}
		};
	}

	public void PickPet(PetID petID)
	{
		if (!_view.GameController.PetController.CanBringPets)
		{
			Log.GUITrace("Pets are not Available");
			return;
		}
		if (!DefRepo.Pets.Keys.Any((PetID id) => id == petID))
		{
			Log.GUITrace($"Trying to bring pet with invalid id {petID} ");
			return;
		}
		if (_view.GameState.ActiveScenario.PetIDs.Count >= _view.GameController.PetController.GetMaxBringPets())
		{
			Log.GUITrace($"Cannot bring more than {_view.GameController.PetController.GetMaxBringPets()} pets to scenario");
			return;
		}
		if (IsPetPicked(petID))
		{
			Log.GUITrace($"Pet {petID.ID} is already part of scenario");
			return;
		}
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Pets.BringPetToScenario, petID);
	}

	public void RemovePet(PetID petID)
	{
		if (!_view.GameController.PetController.CanBringPets)
		{
			Log.GUITrace("Pets are not Available");
			return;
		}
		if (!DefRepo.Pets.Keys.Any((PetID id) => id == petID))
		{
			Log.GUITrace($"Trying to remove pet with invalid id {petID} ");
			return;
		}
		if (!_view.GameState.ActiveScenario.PetIDs.Contains(petID))
		{
			Log.GUITrace($"Pet {petID} is not part of scenario yet");
			return;
		}
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Pets.RemovePetFromScenario, petID);
	}

	public bool IsPetPicked(PetID petID)
	{
		return _view.GameState.ActiveScenario.PetIDs.Contains(petID);
	}

	public bool IsPetSuggested(PetID petID, bool byPartyLeader)
	{
		UIElementTagsViewService tagsViewService = _tagsViewService;
		uint iD = petID.ID;
		string tag = tagsViewService.FormatTag("PETS", iD.ToString());
		PlayerID playerId = (byPartyLeader ? _view.MultiplayerGameState.PartyLeaderID.Value : _view.PlayerID);
		return _view.MultiplayerGameController.PlayerHasUITag(playerId, tag);
	}

	public void SuggestPet(PetID petID)
	{
		UIElementTagsViewService tagsViewService = _tagsViewService;
		uint iD = petID.ID;
		tagsViewService.AddPlayerTag("PETS", iD.ToString(), _view.GameController.PetController.GetMaxBringPets());
	}

	public void DeSuggestPet(PetID petID)
	{
		UIElementTagsViewService tagsViewService = _tagsViewService;
		uint iD = petID.ID;
		tagsViewService.RemovePlayerTag("PETS", iD.ToString());
	}

	private int GetCurrentPetPicksCount()
	{
		if (!_view.GameController.PetController.CanBringPets)
		{
			return 0;
		}
		return _view.GameState.ActiveScenario.PetIDs.Count;
	}

	public List<SuggestibleSlotDTO> GetChallenges(bool onlySelected = false)
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		bool isSoloGame = _view.IsSoloGame;
		int num = GetTotalChallengePicks() - GetCurrentChallengePicks();
		foreach (ChallengeID drawnChallengeID in _view.GameState.ActiveScenario.DrawnChallengeIDs)
		{
			ChallengeDef challengeDef = DefRepo.Challenges[drawnChallengeID];
			if (!onlySelected || IsChallengeSelected(challengeDef.ID))
			{
				uint iD = challengeDef.ID.ID;
				string text = iD.ToString();
				string tag = _tagsViewService.FormatTag("CHALLENGES", text);
				bool flag = IsChallengeSelected(challengeDef.ID);
				bool interactable = flag || num > 0;
				list.Add(new SuggestibleSlotDTO
				{
					Id = text,
					NameLiteral = LocalizationManager.GetTranslation(challengeDef.NameLocKey),
					InfoLiteral = GetChallengeDescription(challengeDef),
					IsSelected = flag,
					IsSuggested = IsChallengeSuggested(challengeDef.ID, _view.IsPartyLeader),
					SuggestingUsers = ((!isSoloGame) ? _tagsViewService.GetMultiplayerUsersWithTag(tag).ToList() : null),
					Interactable = interactable,
					Image = UIInfoTools.Instance.GetChallengesSprite(challengeDef.IllustrationKey)
				});
			}
		}
		return list;
	}

	public UIPrefabTooltipTarget.TooltipInitParams GetChallengeSlotTooltipParams(string challengeId, bool ignoreActionHints = false)
	{
		if (challengeId.IsNullOrEmpty())
		{
			return null;
		}
		ChallengeDef challengeDef = _view.GameController.DefRepository.Challenges.First((KeyValuePair<ChallengeID, ChallengeDef> pair) =>
		{
			uint iD = pair.Value.ID.ID;
			return iD.ToString() == challengeId;
		}).Value;
		int num = GetTotalChallengePicks() - GetCurrentChallengePicks();
		string actionHint;
		if (_view.IsPartyLeader)
		{
			bool num2 = num > 0 && !IsChallengeSelected(challengeDef.ID);
			bool flag = IsChallengeSelected(challengeDef.ID);
			if (num2)
			{
				actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD");
			}
			else if (flag)
			{
				actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_REMOVE");
			}
			else
			{
				actionHint = string.Empty;
			}
		}
		else if (IsChallengeSuggested(challengeDef.ID, _view.IsPartyLeader))
		{
			actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VOTE_REVOKE");
		}
		else if (_tagsViewService.GetPlayerTagsByKey("CHALLENGES").Count() < GetTotalChallengePicks())
		{
			actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VOTE");
		}
		else
		{
			actionHint = null;
		}
		return new UIPrefabTooltipTarget.TooltipInitParams
		{
			Prefab = null,
			OnObjectCreatedCallback = (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = null,
					CaptionLiteral = LocalizationManager.GetTranslation(challengeDef.NameLocKey),
					DescriptionLiteral = GetChallengeDescription(challengeDef),
					Hints = ((!ignoreActionHints) ? actionHint : string.Empty)
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			}
		};
	}

	public string GetChallengeDescription(ChallengeDef def)
	{
		string text = LocalizationManager.GetTranslation(def.DescriptionLocKey);
		switch (def.ID.ID)
		{
		case 1493u:
			text = string.Format(text, Math.Ceiling((float)_view.GameController.ScenarioController.GetScenarioLevel() / 2f).ToString());
			break;
		case 1497u:
			text = string.Format(text, (1.0 + Math.Ceiling((float)_view.GameController.ScenarioController.GetScenarioLevel() / 4f)).ToString());
			break;
		case 1510u:
		{
			int num = _view.GameState.PartyCharacters.Count();
			text = string.Format(text, num);
			break;
		}
		}
		return text;
	}

	public bool IsChallengeSelected(ChallengeID challengeId)
	{
		return _view.GameState.ActiveScenario.SelectedChallengeIDs.Contains(challengeId);
	}

	public int GetCurrentChallengePicks()
	{
		if (!_view.GameController.ScenarioController.CanUseChallenges)
		{
			return 0;
		}
		return _view.GameState.ActiveScenario.SelectedChallengeIDs.Count;
	}

	public int GetTotalChallengePicks()
	{
		if (!_view.GameController.ScenarioController.CanUseChallenges)
		{
			return 0;
		}
		return _view.GameController.ScenarioController.GetMaxSelectChallenges;
	}

	public void SelectChallenge(ChallengeID challengeId)
	{
		if (!_view.GameController.ScenarioController.CanUseChallenges)
		{
			Log.GUITrace("Can't select challenges. Feature is not unlocked/active ");
			return;
		}
		if (!_view.GameState.ActiveScenario.DrawnChallengeIDs.Contains(challengeId))
		{
			Log.GUITrace("Challenge cards have not been drawn\" ");
			return;
		}
		if (_view.GameState.ActiveScenario.SelectedChallengeIDs.Contains(challengeId))
		{
			Log.GUITrace($"Tried to select {challengeId} challenges, but it has already been selected");
			return;
		}
		if (_view.GameState.ActiveScenario.SelectedChallengeIDs.Count + 1 > _view.GameController.ScenarioController.GetMaxSelectChallenges)
		{
			Log.GUITrace("Maximum number of selected challenges reached");
			return;
		}
		ChallengeDef challengeDef = DefRepo.Challenges[challengeId];
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.SelectChallenge, challengeDef);
	}

	public void DeselectChallenge(ChallengeID challengeId)
	{
		if (!_view.GameController.ScenarioController.CanUseChallenges)
		{
			Log.GUITrace("Challenges cannot be used");
			return;
		}
		if (_view.GameState.ActiveScenario.DrawnChallengeIDs.Count == 0)
		{
			Log.GUITrace("Challenge cards have not been drawn");
			return;
		}
		if (_view.GameState.ActiveScenario.DrawnChallengeIDs.Count == 0)
		{
			Log.GUITrace("Challenge cards have not been drawn");
			return;
		}
		if (!_view.GameState.ActiveScenario.SelectedChallengeIDs.Contains(challengeId))
		{
			Log.GUITrace($"Tried to deselect {challengeId} challenges, but it is not currently selected");
			return;
		}
		ChallengeDef challengeDef = DefRepo.Challenges[challengeId];
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.DeselectChallenge, challengeDef);
	}

	public void SuggestChallenge(ChallengeID challengeId)
	{
		int getMaxSelectChallenges = _view.GameController.ScenarioController.GetMaxSelectChallenges;
		UIElementTagsViewService tagsViewService = _tagsViewService;
		uint iD = challengeId.ID;
		tagsViewService.AddPlayerTag("CHALLENGES", iD.ToString(), getMaxSelectChallenges);
	}

	public void DeSuggestChallenge(ChallengeID challengeId)
	{
		UIElementTagsViewService tagsViewService = _tagsViewService;
		uint iD = challengeId.ID;
		tagsViewService.RemovePlayerTag("CHALLENGES", iD.ToString());
	}

	public bool IsChallengeSuggested(ChallengeID challengeId, bool byPartyLeader)
	{
		UIElementTagsViewService tagsViewService = _tagsViewService;
		uint iD = challengeId.ID;
		string tag = tagsViewService.FormatTag("CHALLENGES", iD.ToString());
		PlayerID playerId = (byPartyLeader ? _view.MultiplayerGameState.PartyLeaderID.Value : _view.PlayerID);
		return _view.MultiplayerGameController.PlayerHasUITag(playerId, tag);
	}

	public List<SuggestibleSlotDTO> GetFavors(bool onlySelected = false)
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		List<ScenarioFavorType> unlockedFavors = _view.GameController.State.UnlockedFavors;
		bool isSoloGame = _view.IsSoloGame;
		foreach (ScenarioFavorType item2 in unlockedFavors)
		{
			if (_view.GameController.IsFavorValid(item2) && _view.GameController.IsFavorUnlocked(item2) && item2 != ScenarioFavorType.None && item2 != ScenarioFavorType.Potential && (!onlySelected || IsFavorSelected(item2)))
			{
				string text = item2.ToString();
				string tag = _tagsViewService.FormatTag("FAVORS", text);
				FavorConfigUI favorConfig = UIInfoTools.Instance.GetFavorConfig(item2);
				int activeFavorInstances = GetActiveFavorInstances(item2);
				SuggestibleSlotDTO item = new SuggestibleSlotDTO
				{
					Id = text,
					NameLiteral = LocalizationManager.GetTranslation(favorConfig.Name),
					TooltipLiteral = LocalizationManager.GetTranslation(favorConfig.Description),
					Image = favorConfig.Icon,
					InfoLiteral = ((activeFavorInstances > 0) ? $"x{activeFavorInstances}" : null),
					Charges = activeFavorInstances,
					MaxCharges = (int)GetMaxFavorCountByType(item2),
					Points = 1,
					IsEmpty = false,
					IsSelected = IsFavorSelected(item2),
					IsSuggested = IsFavorSuggested(text, _view.IsPartyLeader),
					SuggestingUsers = ((!isSoloGame) ? _tagsViewService.GetMultiplayerUsersWithTag(tag).ToList() : null),
					Interactable = true
				};
				list.Add(item);
			}
		}
		return list;
	}

	public UIPrefabTooltipTarget.TooltipInitParams GetFavorSlotTooltipParams(string favorId, bool ignoreActionHints = false)
	{
		if (favorId.IsNullOrEmpty())
		{
			return null;
		}
		ScenarioFavorType favor = (ScenarioFavorType)Enum.Parse(typeof(ScenarioFavorType), favorId, ignoreCase: true);
		bool flag = CanAddFavor(favor);
		bool flag2 = CanRemoveFavor(favor);
		bool maxInstancesReached = GetActiveFavorInstances(favor) == GetMaxFavorCountByType(favor);
		FavorConfigUI config = UIInfoTools.Instance.GetFavorConfig(favor);
		string actionHint = string.Empty;
		if (_view.IsPartyLeader)
		{
			if (flag)
			{
				actionHint += FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD");
			}
			if (flag2)
			{
				if (actionHint.IsNOTNullOrEmpty())
				{
					actionHint += "\n";
				}
				actionHint += FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, "GUI_ACTION_REMOVE");
			}
		}
		else
		{
			actionHint = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, IsFavorSuggested(favorId, _view.IsPartyLeader) ? "GUI_VOTE_REVOKE" : "GUI_VOTE");
		}
		return new UIPrefabTooltipTarget.TooltipInitParams
		{
			Prefab = null,
			OnObjectCreatedCallback = (GameObject go) =>
			{
				UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
				{
					Icon = null,
					CaptionLiteral = LocalizationManager.GetTranslation(config.Name),
					DescriptionLiteral = (maxInstancesReached ? LocalizationManager.GetTranslation(config.Description) : AppendFavorCost(LocalizationManager.GetTranslation(config.Description), favor)),
					Hints = ((!ignoreActionHints) ? actionHint : string.Empty)
				};
				go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
			}
		};
		string AppendFavorCost(string desc, ScenarioFavorType favorType)
		{
			string text = _view.GameController.ScenarioController.GetFavorCost(favorType).ToString();
			if (!CanAddFavor(favor))
			{
				text = FHUILocFormatting.Color(text, FHUIColors.MISSING_RESOURCE);
			}
			string text2 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST") + ": " + text;
			text2 = FHUILocFormatting.AlignCenter(text2);
			text2 = FHUILocFormatting.FontSize("+4", text2);
			return desc + "\n\n" + text2;
		}
	}

	public uint GetFavorDifficulty()
	{
		uint num = 0u;
		foreach (ScenarioFavorToBeUsed item in _view.GameState.ActiveScenario.FavorsToUse)
		{
			num += _view.GameController.ScenarioController.GetFavorCost(item.type);
		}
		return num;
	}

	[Obsolete]
	public uint GetCurrentFavorPoints()
	{
		return _view.GameController.ScenarioController.GetCurrentFavorPoints();
	}

	public IEnumerable<MonsterDef> GetAllScenarioMonsterTypesForStrategyFavor()
	{
		return _view.GameController.ScenarioController.GetAvailableScenarioMonsterTypesForStrategyFavor(_view.GameState.SelectedScenario, includeActiveFavors: false);
	}

	public IEnumerable<MonsterCardID> GetRemovedCardsForStrategyFavor(string monsterID)
	{
		return _view.GameController.ScenarioController.GetRemovedCardsForStrategyFavor(monsterID);
	}

	public IEnumerable<MonsterCardID> GetAvailableCardsForStrategyFavor(MonsterID monsterID)
	{
		MonsterDef monster = DefRepo.Monsters[monsterID];
		IEnumerable<MonsterCardDef> availableMonsterCardsForStrategyFavor = _view.GameController.ScenarioController.GetAvailableMonsterCardsForStrategyFavor(monster, applyScenarioMods: false);
		List<MonsterCardID> chosenCardsForMonster = _view.GameState.ActiveScenario.MonsterCardsDrawnForStrategyFavor;
		foreach (MonsterCardDef item in availableMonsterCardsForStrategyFavor)
		{
			if (chosenCardsForMonster.Contains(item.ID))
			{
				yield return item.ID;
			}
		}
	}

	public void StoreStrategyFavorState(IEnumerable<(string, int)> newCards, IEnumerable<(string, int)> removedCards)
	{
		List<ScenarioFavorToBeUsed> list = _view.GameState.ActiveScenario.FavorsToUse.ToList();
		foreach (var (param, num) in newCards)
		{
			list.Add(new ScenarioFavorToBeUsed(ScenarioFavorType.Strategy, param, num.ToString()));
		}
		foreach (var removedCard in removedCards)
		{
			string item = removedCard.Item1;
			int num = removedCard.Item2;
			ScenarioFavorToBeUsed scenarioFavorToBeUsed = new ScenarioFavorToBeUsed(ScenarioFavorType.Strategy, item, num.ToString());
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				if (list[num2].Equals(scenarioFavorToBeUsed))
				{
					list.RemoveAt(num2);
					break;
				}
			}
		}
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.SetFavors, list);
	}

	public void StoreDiscoveryFavorState(OutpostResources resources)
	{
		List<ScenarioFavorToBeUsed> list = _view.GameState.ActiveScenario.FavorsToUse.ToList();
		list.RemoveAll((ScenarioFavorToBeUsed f) => f.type == ScenarioFavorType.Discovery);
		list.AddRange((from herb in OutpostResources.GetAllHerbs()
			where resources.HasHerb(herb)
			select herb).Select((OutpostHerbType herb) =>
		{
			OutpostHerbType outpostHerbType = herb;
			return new ScenarioFavorToBeUsed(ScenarioFavorType.Discovery, outpostHerbType.ToString());
		}));
		list.AddRange((from material in OutpostResources.GetAllMaterials()
			where resources.HasMaterial(material)
			select material).Select((OutpostMaterialType material) =>
		{
			OutpostMaterialType outpostMaterialType = material;
			return new ScenarioFavorToBeUsed(ScenarioFavorType.Discovery, outpostMaterialType.ToString());
		}));
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.SetFavors, list);
	}

	public void AddFavor(ScenarioFavorType favor, RectTransform slotPickerPoint)
	{
		if (!_view.GameController.ScenarioController.CanUseFavor(applyingFavor: false, favor))
		{
			Log.GUITrace("Invalid favor " + favor);
			return;
		}
		switch (favor)
		{
		case ScenarioFavorType.Discovery:
		{
			DiscoveryFavorSelectionViewState state = new DiscoveryFavorSelectionViewState();
			_view.PushState(state);
			return;
		}
		case ScenarioFavorType.Strategy:
			if (GetAllScenarioMonsterTypesForStrategyFavor().Any())
			{
				_view.PushState(new StrategyFavorSelectionViewState());
			}
			return;
		}
		int attackModifierValue = (int)(0 - _view.GameController.ScenarioController.GetFavorCost(favor));
		if (_view.GameController.ScenarioController.CanGainFavorPoint(attackModifierValue, out var _))
		{
			List<ScenarioFavorToBeUsed> list = _view.GameController.ScenarioController.PrepAddNewFavor(new ScenarioFavorToBeUsed(favor));
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.SetFavors, list);
		}
	}

	public bool CanAddFavor(ScenarioFavorType favor)
	{
		if (!_view.GameController.ScenarioController.CanUseFavor(applyingFavor: false, favor))
		{
			return false;
		}
		int attackModifierValue = (int)(0 - _view.GameController.ScenarioController.GetFavorCost(favor));
		if (!_view.GameController.ScenarioController.CanGainFavorPoint(attackModifierValue, out var _))
		{
			return false;
		}
		return true;
	}

	public void RemoveFavor(ScenarioFavorType favor, RectTransform slotPickerPoint)
	{
		if (GetActiveFavorInstances(favor) == 0)
		{
			Log.GUITrace($"Invalid favor to remove: {favor}");
			return;
		}
		switch (favor)
		{
		case ScenarioFavorType.Discovery:
		{
			DiscoveryFavorSelectionViewState state = new DiscoveryFavorSelectionViewState();
			_view.PushState(state);
			break;
		}
		case ScenarioFavorType.Strategy:
			if (GetAllScenarioMonsterTypesForStrategyFavor().Any())
			{
				_view.PushState(new StrategyFavorSelectionViewState());
			}
			break;
		default:
		{
			List<ScenarioFavorToBeUsed> list = _view.GameController.ScenarioController.PrepDelNewFavor(new ScenarioFavorToBeUsed(favor));
			_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.SetFavors, list);
			Log.GUITrace("RemoveFavor " + favor);
			break;
		}
		}
	}

	public bool CanRemoveFavor(ScenarioFavorType favor)
	{
		return GetActiveFavorInstances(favor) > 0;
	}

	public void SuggestFavor(string favor)
	{
		_tagsViewService.AddPlayerTag("FAVORS", favor);
	}

	public void DeSuggestFavor(string favor)
	{
		_tagsViewService.RemovePlayerTag("FAVORS", favor);
	}

	public bool IsFavorSelected(string favorId)
	{
		if (Enum.TryParse<ScenarioFavorType>(favorId, out var result))
		{
			return IsFavorSelected(result);
		}
		return false;
	}

	public bool IsFavorSelected(ScenarioFavorType favor)
	{
		return _view.GameState.ActiveScenario.FavorsToUse.Any((ScenarioFavorToBeUsed favorToBeUsed) => favorToBeUsed.type == favor);
	}

	public bool IsFavorSuggested(string favor, bool byPartyLeader)
	{
		string tag = _tagsViewService.FormatTag("FAVORS", favor);
		PlayerID playerId = (byPartyLeader ? _view.MultiplayerGameState.PartyLeaderID.Value : _view.PlayerID);
		return _view.MultiplayerGameController.PlayerHasUITag(playerId, tag);
	}

	public uint GetMaxPotentialFavorPoints()
	{
		return GameController.ScenarioController.GetMaxPotentialFavorPoints();
	}

	public uint GetMaxFavorCountByType(ScenarioFavorType favorType)
	{
		return GetMaxPotentialFavorPoints() / _view.GameController.ScenarioController.GetFavorCost(favorType);
	}

	public int GetActiveFavorInstances(ScenarioFavorType favor)
	{
		return _view.GameState.ActiveScenario.FavorsToUse.Count((ScenarioFavorToBeUsed used) => used.type == favor);
	}

	public IEnumerable<FHInputEventContainer.Params> GetFavorSlotInteractions(SuggestibleSlotDTO dto)
	{
		ScenarioFavorType favor = (ScenarioFavorType)Enum.Parse(typeof(ScenarioFavorType), dto.Id, ignoreCase: true);
		bool isHost = _view.IsPartyLeader;
		bool flag = favor == ScenarioFavorType.Discovery || favor == ScenarioFavorType.Strategy || CanAddFavor(favor);
		bool canRemoveFavor = favor == ScenarioFavorType.Discovery || favor == ScenarioFavorType.Strategy || CanRemoveFavor(favor);
		if (isHost & flag)
		{
			yield return new FHInputEventContainer.Params
			{
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					AddFavor(favor, dto.SlotPickerPoint);
				}
			};
		}
		if (isHost & canRemoveFavor)
		{
			yield return new FHInputEventContainer.Params
			{
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = () =>
				{
					RemoveFavor(favor, dto.SlotPickerPoint);
				}
			};
		}
		if (isHost)
		{
			yield break;
		}
		yield return new FHInputEventContainer.Params
		{
			InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
			ActionCallback = () =>
			{
				if (IsFavorSuggested(dto.Id, byPartyLeader: false))
				{
					DeSuggestFavor(dto.Id);
				}
				else
				{
					SuggestFavor(dto.Id);
				}
			}
		};
	}

	public void ProcessBriefingChangeSound(MultiplayerGameController.Result result)
	{
		BriefingSnapshot briefingSnapshot = TakeBriefingSnapshot();
		bool num = briefingSnapshot != null && _lastBriefingSnapshot != null && !briefingSnapshot.SameAs(_lastBriefingSnapshot);
		_lastBriefingSnapshot = briefingSnapshot;
		if (num && !result.PlayerID.Equals(_view.PlayerID))
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.genericNotificationAudioItem);
		}
	}

	private BriefingSnapshot TakeBriefingSnapshot()
	{
		ActiveScenarioState activeScenario = _view.GameState.ActiveScenario;
		if (activeScenario == null)
		{
			return null;
		}
		return new BriefingSnapshot
		{
			Challenges = activeScenario.SelectedChallengeIDs.ToList(),
			Pets = activeScenario.PetIDs.ToList(),
			Favors = activeScenario.FavorsToUse.ToList(),
			BattleGoals = (from css in activeScenario.CharacterScenarioSettings
				select css.SelectedBattleGoal into g
				where g.HasValue
				select g).ToList(),
			Tags = _view.MultiplayerGameState.Players.SelectMany((MultiplayerGameState.Player p) => from tag in p.UITags.Where(IsBriefingTag)
				select $"{p.ID}:{tag}").ToList()
		};
	}

	private static bool IsBriefingTag(string tag)
	{
		if (!tag.StartsWith("PETS") && !tag.StartsWith("CHALLENGES"))
		{
			return tag.StartsWith("FAVORS");
		}
		return true;
	}
}
