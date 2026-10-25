using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.States;
using FHRL;
using I2.Loc;
using UnityEngine;

namespace FHGUI.Services;

public class CharacterDetailsViewService : FHMapViewService
{
	public enum CharacterCreationCrumbTags
	{
		SelectClass,
		ChooseName,
		SelectQuest,
		SelectEquipment
	}

	private const string UnlockMovieFilePrefix = "FH_HeroIntroduction_";

	private const string UnlockMovieFileSuffix = "_4k.mov";

	public CharacterDetailsViewService(FHMapView view)
		: base(view)
	{
	}

	public void PopulateCharacterDetails(CharacterDataDTO characterDataDto, CharacterPreviewModule previewModule, CharacterGoalsModule goalsModule, HeroClassStatsModule statsModule, CharacterBiographyModule bioModule, bool isTempCharacter)
	{
		CharacterClassDef characterClassDef = characterDataDto.Class;
		FHRL.CharacterController controller = Singleton<FHMapChoreographer>.InstanceFast.GameController.CharacterController;
		CharacterState characterState = Singleton<FHMapChoreographer>.InstanceFast.GameState.PartyCharacters.SingleOrDefault((CharacterState x) => x.ID == characterDataDto.CharacterID);
		DefRepository repo = DefRepo;
		List<GoalDTO> masteries = characterDataDto.Masteries;
		masteries = characterClassDef.MasteryIDs?.Select((MasteryID md) => new GoalDTO(repo.MasteryDefs.Values.SingleOrDefault((MasteryDef x) => x.ID == md), characterState != null && controller.GetCompletedMasteries(characterState).Any((MasteryDef x) => x.ID == md))).ToList();
		ClassInfoDTO classInfoDTO = ((characterDataDto.Class != null) ? new ClassInfoDTO(characterDataDto.Level, characterDataDto.Class, masteries) : new ClassInfoDTO());
		GetPersonalQuest(characterState);
		previewModule.Init(characterDataDto, null, showValues: false, !_view.Modules.Characters.CharacterRename.gameObject.activeInHierarchy);
		bioModule.Init(classInfoDTO);
		if (goalsModule != null)
		{
			goalsModule.Init(classInfoDTO.Masteries);
		}
		statsModule.Init(classInfoDTO, characterDataDto, isTempCharacter ? "GUI_CHARACTER_PREVIEW_CARDS" : "GUI_CHARACTER_EDIT_HAND", isTempCharacter, !isTempCharacter);
	}

	public CharacterDataDTO PopulateCharacterDTO(CharacterState characterState)
	{
		FHRL.CharacterController controller = _view.GameController.CharacterController;
		List<GoalDTO> masteries = characterState.ClassDef.MasteryIDs?.Select((MasteryID md) => new GoalDTO(DefRepo.MasteryDefs.Values.SingleOrDefault((MasteryDef x) => x.ID == md), characterState != null && controller.GetCompletedMasteries(characterState).Any((MasteryDef x) => x.ID == md))).ToList();
		List<CardDTO> cards = GenerateCardDTOList(characterState, controller);
		List<ItemDataDTO> items = (from item in controller.GetOwnedItems(characterState)
			select new ItemDataDTO(item, -1, showCost: false, isNew: false, canBuy: false, characterState.EquipedItemIDs.Contains(item.ID), 0, canAfford: false, characterState.ScenarioModifiers.Any((CharacterScenarioModifier x) => x.Type == CharacterScenarioModifierType.DisableItem && x.DisabledItemID == item.ID))).ToList();
		bool isRetired = GameController.State.RetiredCharacters.Contains(characterState);
		CharacterDataDTO characterDataDTO = new CharacterDataDTO(characterState, cards, items, masteries, GameController.CharacterController.GetLevelFromProsperity());
		characterDataDTO.UnlockedSmallItemCount = controller.GetMaxUnlockedSmallItems(characterState);
		characterDataDTO.MaxSmallItems = controller.GetMaxSmallItems(characterState);
		characterDataDTO.MaxOneHandSlots = controller.GetMaxOneHandSlots(characterState);
		characterDataDTO.IsRetired = isRetired;
		characterDataDTO.SetPersonalQuest(GetPersonalQuest(characterState));
		characterDataDTO.Health = new RangedValueDTO((int)_view.GameController.CharacterController.GetCharacterHealth(characterState.ClassDef, characterState.Level), (int)_view.GameController.CharacterController.GetCharacterHealth(characterState.ClassDef, _view.GameController.CharacterController.GetMaxLevel()));
		characterDataDTO.SlotId = SlotIdFromCharacterState(characterState);
		characterDataDTO.CharacterID = characterState.ID;
		return characterDataDTO;
	}

	public int SlotIdFromCharacterState(CharacterState characterState)
	{
		if (characterState == null)
		{
			return -1;
		}
		MultiplayerGameController.PartySlotDesc partySlotDesc = MultiplayerGameController.GetSlots().FirstOrDefault((MultiplayerGameController.PartySlotDesc s) => s.Character == characterState);
		if (!partySlotDesc.IsValid())
		{
			return -1;
		}
		return (int)partySlotDesc.ID.ID;
	}

	public bool TryRefreshDTO(ref CharacterDataDTO characterDto)
	{
		CharacterID characterID = characterDto.CharacterID;
		if (!GameController.CharacterController.TryGetCharacter(characterID, out var character))
		{
			return false;
		}
		characterDto = PopulateCharacterDTO(character);
		return true;
	}

	private CharacterDataDTO PopulateCharacterDetails(CharacterState characterState, params FHMapViewModule[] modules)
	{
		return PopulateCharacterDetails(characterState, isTempCharacter: false, modules);
	}

	private CharacterDataDTO PopulateCharacterDetails(CharacterState characterState, bool isTempCharacter, params FHMapViewModule[] modules)
	{
		bool isRetired = GameController.State.RetiredCharacters.Contains(characterState);
		CharacterDataDTO characterDataDTO = new CharacterDataDTO(characterState, GenerateCardDTOList(characterState, _view.GameController.CharacterController), null, null, GameController.CharacterController.GetLevelFromProsperity());
		characterDataDTO.IsRetired = isRetired;
		CharacterClassDef characterClassDef = characterDataDTO.Class;
		FHRL.CharacterController controller = Singleton<FHMapChoreographer>.InstanceFast.GameController.CharacterController;
		DefRepository repo = DefRepo;
		List<GoalDTO> masteryDTOs = characterClassDef.MasteryIDs?.Select((MasteryID md) => new GoalDTO(repo.MasteryDefs.Values.SingleOrDefault((MasteryDef x) => x.ID == md), characterState != null && controller.GetCompletedMasteries(characterState).Any((MasteryDef x) => x.ID == md))).ToList();
		ClassInfoDTO classInfoDTO = ((characterDataDTO.Class != null) ? new ClassInfoDTO(characterDataDTO.Level, characterDataDTO.Class, masteryDTOs) : new ClassInfoDTO());
		characterDataDTO.SetPersonalQuest(GetPersonalQuest(characterState));
		characterDataDTO.Health = new RangedValueDTO((int)_view.GameController.CharacterController.GetCharacterHealth(characterState.ClassDef, characterState.Level), (int)_view.GameController.CharacterController.GetCharacterHealth(characterState.ClassDef, _view.GameController.CharacterController.GetMaxLevel()));
		foreach (FHMapViewModule fHMapViewModule in modules)
		{
			if (!(fHMapViewModule is CharacterPreviewModule characterPreviewModule))
			{
				if (!(fHMapViewModule is CharacterGoalsModule characterGoalsModule))
				{
					if (!(fHMapViewModule is CharacterPersonalQuestModule personalQuestModule))
					{
						if (!(fHMapViewModule is HeroClassStatsModule heroClassStatsModule))
						{
							if (!(fHMapViewModule is CharacterBiographyModule characterBiographyModule))
							{
								if (fHMapViewModule is HandCardListModule handCardListModule)
								{
									List<CardDTO> cards = characterDataDTO.Cards.Where((CardDTO dto) => dto.IsInHand).ToList();
									Color characterColor = UIInfoTools.Instance.GetCharacterColor(CrossReference.ClassDefToECharacterModel(characterDataDTO.Class));
									Color characterAltColor = UIInfoTools.Instance.GetCharacterAltColor(CrossReference.ClassDefToECharacterModel(characterDataDTO.Class));
									bool isDualDeck = characterDataDTO.Cards.Any((CardDTO c) => c.IsAltColor);
									handCardListModule.Init(new CardListModuleBase.Params
									{
										Cards = cards,
										MinCount = 0
									}, characterColor, characterAltColor, isDualDeck);
								}
							}
							else
							{
								characterBiographyModule.Init(classInfoDTO);
							}
						}
						else
						{
							heroClassStatsModule.Init(classInfoDTO, characterDataDTO, isTempCharacter ? "GUI_CHARACTER_PREVIEW_CARDS" : "GUI_CHARACTER_EDIT_HAND", isTempCharacter, !isTempCharacter);
						}
					}
					else
					{
						InitPersonalQuestModule(personalQuestModule, characterDataDTO, characterState);
					}
				}
				else
				{
					characterGoalsModule.Init(classInfoDTO.Masteries);
				}
			}
			else
			{
				characterPreviewModule.Init(characterDataDTO, null, showValues: false, !_view.Modules.Characters.CharacterRename.gameObject.activeInHierarchy);
			}
		}
		return characterDataDTO;
	}

	public void PopulateCharacterDetails(CharacterDataDTO characterDTO, bool isTempCharacter, params FHMapViewModule[] modules)
	{
		ClassInfoDTO classInfoDTO = ((characterDTO.Class != null) ? new ClassInfoDTO(characterDTO.Level, characterDTO.Class, characterDTO.Masteries) : new ClassInfoDTO());
		foreach (FHMapViewModule fHMapViewModule in modules)
		{
			if (!(fHMapViewModule is CharacterPreviewModule characterPreviewModule))
			{
				if (!(fHMapViewModule is CharacterGoalsModule characterGoalsModule))
				{
					if (!(fHMapViewModule is CharacterPersonalQuestModule personalQuestModule))
					{
						if (!(fHMapViewModule is HeroClassStatsModule heroClassStatsModule))
						{
							if (!(fHMapViewModule is CharacterBiographyModule characterBiographyModule))
							{
								if (fHMapViewModule is HandCardListModule handCardListModule)
								{
									List<CardDTO> cards = (characterDTO.InCreation ? characterDTO.Cards.ToList() : characterDTO.Cards.Where((CardDTO dto) => dto.IsInHand).ToList());
									Color characterColor = UIInfoTools.Instance.GetCharacterColor(CrossReference.ClassDefToECharacterModel(characterDTO.Class));
									Color characterAltColor = UIInfoTools.Instance.GetCharacterAltColor(CrossReference.ClassDefToECharacterModel(characterDTO.Class));
									bool isDualDeck = characterDTO.Cards.Any((CardDTO c) => c.IsAltColor);
									handCardListModule.Init(new CardListModuleBase.Params
									{
										Cards = cards,
										MinCount = 0
									}, characterColor, characterAltColor, isDualDeck);
								}
							}
							else
							{
								characterBiographyModule.Init(classInfoDTO);
							}
						}
						else
						{
							heroClassStatsModule.Init(classInfoDTO, characterDTO, isTempCharacter ? "GUI_CHARACTER_PREVIEW_CARDS" : "GUI_CHARACTER_EDIT_HAND", isTempCharacter, !isTempCharacter);
						}
					}
					else
					{
						InitPersonalQuestModule(personalQuestModule, characterDTO);
					}
				}
				else
				{
					characterGoalsModule.Init(classInfoDTO.Masteries);
				}
			}
			else
			{
				characterPreviewModule.Init(characterDTO, null, showValues: false, !_view.Modules.Characters.CharacterRename.gameObject.activeInHierarchy);
			}
		}
	}

	public void SetFunctionality(CharacterState character, CharacterDetailsScreenModule.Functionality functionality)
	{
		CharacterDetailsScreenModule mainScreen = _view.Modules.Characters.MainScreen;
		mainScreen.ClearPresetButtons();
		bool flag = character != null && character.PartyState == CharacterPartyStateType.CreationProcess;
		bool flag2 = flag || (character != null && _view.MultiplayerGameController.PlayerOwnsCharacter(_view.PlayerID, character));
		if (character != null && !flag2)
		{
			return;
		}
		CharacterDetailsScreenModule.Functionality functionality2 = CharacterDetailsScreenModule.Functionality.PersonalQuest;
		if (functionality.HasFlag(functionality2))
		{
			mainScreen.SetPresetButton(functionality2, () =>
			{
				ToPersonalQuestRedrawState(character);
			}, "GUI_SELECT_QUEST");
		}
		functionality2 = CharacterDetailsScreenModule.Functionality.Rename;
		if (functionality.HasFlag(functionality2) & flag)
		{
			mainScreen.SetPresetButton(functionality2, () =>
			{
				_view.PushState(new RenameCharacterViewState(character, character.Name, creationProcess: false, unassignedCharacter: false));
			}, "GUI_RENAME");
		}
		functionality2 = CharacterDetailsScreenModule.Functionality.Equipment;
		if (functionality.HasFlag(functionality2))
		{
			mainScreen.SetPresetButton(functionality2, () =>
			{
				ToEquipmentState(character);
			}, "GUI_INVENTORY");
		}
	}

	public static List<UIBreadcrumbs.Params> GetCharacterCreationCrumbsParams(bool excludePersonalQuest = false)
	{
		List<UIBreadcrumbs.Params> list = new List<UIBreadcrumbs.Params>
		{
			new UIBreadcrumbs.Params
			{
				FlowTag = 0,
				Text = LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_SelectClass")
			},
			new UIBreadcrumbs.Params
			{
				FlowTag = 1,
				Text = LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_ChooseName")
			}
		};
		if (!excludePersonalQuest)
		{
			list.Add(new UIBreadcrumbs.Params
			{
				FlowTag = 2,
				Text = LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_SelectQuest")
			});
		}
		list.Add(new UIBreadcrumbs.Params
		{
			FlowTag = 3,
			Text = LocalizationManager.GetTranslation("GUI_CHARACTER_CREATION_SelectEquipment")
		});
		return list;
	}

	public void ToEquipmentState(CharacterState character)
	{
		if (character.PartyState == CharacterPartyStateType.CreationProcess)
		{
			_view.PushState(new SpendStartingGoldViewState(character, creationProcess: false, unassignedCharacter: false));
		}
		else
		{
			_view.PushState(new CharacterEquipmentViewState());
		}
	}

	public void ToViewCardsState(CharacterDataDTO characterDto, bool readOnly)
	{
		_view.PushState(new HandManagementViewState(characterDto, readOnly));
	}

	public void ToPersonalQuestSelectionState(CharacterState characterState, bool creationProcess, bool slotlessCharacter)
	{
		if (!creationProcess && MultiplayerGameController.CanDrawPesonalQuests(_view.PlayerID))
		{
			Choreographer.ExecuteCommand("DrawPersonalQuests", _view.PlayerID);
		}
		List<PersonalQuestDef> list = MultiplayerGameController.GetPersonalQuests(_view.PlayerID).ToList();
		if (characterState.PersonalQuestDef != null && !list.Contains(characterState.PersonalQuestDef))
		{
			list.Insert(0, characterState.PersonalQuestDef);
		}
		PersonalQuestDef leftQuestDef = ((list.Count > 0) ? list[0] : null);
		PersonalQuestDef rightQuestDef = ((list.Count > 1) ? list[1] : null);
		_view.PushState(new PersonalQuestSelectionViewState(characterState, leftQuestDef, rightQuestDef, creationProcess, slotlessCharacter));
	}

	public void ToPersonalQuestRedrawState(CharacterState characterState)
	{
		_view.GetService<PartyViewService>().RedrawPersonalQuest();
		List<PersonalQuestDef> list = MultiplayerGameController.GetPersonalQuests(_view.PlayerID).ToList();
		PersonalQuestDef leftQuestDef = ((list.Count > 0) ? list[0] : null);
		PersonalQuestDef rightQuestDef = ((list.Count > 1) ? list[1] : null);
		_view.PushState(new PersonalQuestSelectionViewState(characterState, leftQuestDef, rightQuestDef, creationProcess: false, unassignedCharacter: false));
	}

	public static List<CardDTO> GenerateCardDTOList(CharacterState character, FHRL.CharacterController characterController)
	{
		return character.OwnedCards.Select((ClassCardDef cardDef) => new CardDTO(cardDef, character.SelectedCardIDs.Any((ClassCardID id) => id == cardDef.ID), character.ID, isSelectable: characterController.CanSelectCard(character, cardDef.ID), enhancements: UIPlayerCharacterUtils.GetEnhancementsForCard(character.ClassDef, (int)cardDef.ID.ID))).ToList();
	}

	public bool HasAvailablePersonalQuest(PlayerID id)
	{
		if (!MultiplayerGameController.GameState.Players.First((MultiplayerGameState.Player p) => p.ID == id).DrawnPersonalQuests.Any())
		{
			return MultiplayerGameController.GetAvailablePersonalQuests().Any();
		}
		return true;
	}

	public void InitPersonalQuestModule(CharacterPersonalQuestModule personalQuestModule, CharacterDataDTO characterDataDto, CharacterState characterState = null)
	{
		if (characterState == null)
		{
			characterState = (characterDataDto.IsRetired ? null : GameController.State.AllCharacters.FirstOrDefault((CharacterState c) => c.ID == characterDataDto.CharacterID));
		}
		if (characterState == null)
		{
			personalQuestModule.Init(default);
			return;
		}
		bool flag = characterState != null && GameController.CharacterController.IsPersonalQuestCompleted(characterState);
		bool flag2 = characterState != null && _view.GetService<PartyViewService>().LocalPlayerOwnsCharacter(characterState) && GameController.State?.ActiveScenario == null;
		personalQuestModule.Init(new CharacterPersonalQuestModule.Params
		{
			CanRetire = flag,
			HasAuthorityToRetire = flag2,
			RetireCallback = ((flag & flag2) ? ((Action)(() =>
			{
				_view.GetService<RosterViewService>().StartCharacterRetirement(characterState.ID);
			})) : null),
			PersonalQuest = characterDataDto.PersonalQuest
		});
	}

	public CharacterPersonalQuestDTO GetPersonalQuest(CharacterState characterState)
	{
		if (characterState?.PersonalQuest != null)
		{
			return new CharacterPersonalQuestDTO(characterState, GameController.CharacterController.CampaignController);
		}
		return null;
	}

	public void EnsureAbilityCards()
	{
	}

	public bool CharacterHasIntroVideo(CharacterClassID classID)
	{
		return !_view.GameController.State.GameMode.UnlockedClasses.Any((CharacterClassID id) => id == classID);
	}

	public void ShowCharacterIntroVideo(CharacterClassID classID, bool fullscreen, bool closeOnComplete)
	{
		ReadOnlySpan<char> readOnlySpan = classID.ID;
		string text = "FH_HeroIntroduction_" + new string(readOnlySpan.Slice(0, readOnlySpan.Length - 2)) + "_4k.mov";
		string text2 = ((Application.platform == RuntimePlatform.OSXPlayer) ? Path.Combine(Application.dataPath, "Resources", "Data", "StreamingAssets", "Movies", "HeroIntroductions", text) : Path.Combine(Application.dataPath, "StreamingAssets", "Movies", "HeroIntroductions", text));
		if (File.Exists(text2 ?? ""))
		{
			_view.PushState(new StandaloneVideoViewState(text2, fullscreen, closeOnComplete));
		}
	}
}
