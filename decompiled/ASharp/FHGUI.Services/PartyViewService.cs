using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FFSNet;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.States;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Services;

public class PartyViewService : FHMapViewService
{
	public const string TAG_SLOT_PREFIX = "SLOT";

	public PartyViewService(FHMapView view)
		: base(view)
	{
	}

	public PartySlotDTO GetSlotData(int slotId, CharacterState characterState, bool isSelected)
	{
		MultiplayerGameState.Player player = (_view.IsOnline ? _view.MultiplayerGameController.GetCharacterOwner(characterState) : null);
		bool isLeader = player != null && _view.MultiplayerGameState.PartyLeaderID == player.ID;
		bool isAvailable = player == null || player.ID == _view.PlayerID;
		return GetSlotData(slotId, characterState, player, isLeader, isAvailable, isSelected);
	}

	public PartySlotDTO GetSlotData(int slotId, [AllowNull] CharacterState characterState, [AllowNull] MultiplayerGameState.Player player, bool isLeader, bool isAvailable, bool isSelected)
	{
		UIMultiplayerUser.Data? multiplayerUserData = null;
		string username = ((player == null) ? string.Empty : PlayerRegistry.GetPlayer(player.ID)?.Username);
		if (player != null)
		{
			multiplayerUserData = new UIMultiplayerUser.Data
			{
				Id = player.ID.ID,
				Username = username,
				Avatar = PlayerRegistry.GetPlayer(player.ID)?.Avatar,
				IsLeader = isLeader
			};
		}
		if (characterState == null)
		{
			return new PartySlotDTO
			{
				SlotId = slotId,
				MultiplayerUserData = multiplayerUserData,
				Owned = _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID.ID == slotId)
			};
		}
		string iD = characterState.ClassID.ID;
		ECharacter character = CrossReference.ClassIdToECharacterModel(iD);
		return new PartySlotDTO
		{
			SlotId = slotId,
			Character = character,
			ClassID = iD,
			CharacterID = characterState.ID,
			Portrait = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration,
			Illustration = UIInfoTools.Instance.GetCharacterConfigUI(character).illustration,
			InfoIllustration = UIInfoTools.Instance.GetCharacterConfigUI(character).InfoIllustration,
			Color = UIInfoTools.Instance.GetCharacterColor(character),
			Name = characterState.Name,
			Gold = characterState.Gold,
			Level = characterState.Level,
			ShowValues = true,
			HasAvailableCardUnlocks = (isAvailable && characterState.CardUnlocks != 0),
			HasAvailablePerkMarks = (isAvailable && characterState.PerkMarks != 0 && _view.GetService<PerksViewService>().HasUnlockablePerks(_view.SelectedCharacter)),
			CanRetire = _view.GameController.CharacterController.IsPersonalQuestCompleted(characterState),
			IsAvailable = isAvailable,
			IsSelected = isSelected,
			MultiplayerUserData = multiplayerUserData,
			Owned = _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc desc) => desc.ID.ID == slotId),
			IsReady = ((_view.IsReadyCheckRunning && player != null) ? new bool?(player.Ready) : ((bool?)null)),
			InCreation = (characterState.PartyState == CharacterPartyStateType.CreationProcess && !characterState.CreationFlowPassed)
		};
	}

	public static PartySlotDTO FromClassType(int slotId, string classId, ECharacter eCharacter, bool isSelected = false, bool isAvailable = true)
	{
		if (eCharacter == ECharacter.None)
		{
			return new PartySlotDTO
			{
				SlotId = slotId
			};
		}
		string name = eCharacter.ToString();
		Sprite illustration = UIInfoTools.Instance.GetCharacterConfigUI(eCharacter).illustration;
		Sprite illustration2 = UIInfoTools.Instance.GetCharacterConfigUI(eCharacter).illustration;
		Sprite infoIllustration = UIInfoTools.Instance.GetCharacterConfigUI(eCharacter).InfoIllustration;
		return new PartySlotDTO
		{
			SlotId = slotId,
			Character = eCharacter,
			ClassID = classId,
			CharacterID = null,
			Portrait = illustration,
			Illustration = illustration2,
			InfoIllustration = infoIllustration,
			Color = UIInfoTools.Instance.GetCharacterColor(eCharacter),
			Name = name,
			IsAvailable = isAvailable,
			IsSelected = isSelected
		};
	}

	public List<PartySlotDTO> GetProloguePartySlotsData()
	{
		return GetPartySlotsData(null, GetPartySlots());
	}

	public List<PartySlotDTO> GetPartySlotsData([AllowNull] CharacterState selectedCharacter)
	{
		return GetPartySlotsData(selectedCharacter, GetPartySlots());
	}

	private List<PartySlotDTO> GetPartySlotsData([AllowNull] CharacterState selectedCharacter, IEnumerable<MultiplayerGameController.PartySlotDesc> partySlotDescs)
	{
		List<PartySlotDTO> list = new List<PartySlotDTO>((int)_view.GameController.Settings.MaxPartySize);
		foreach (MultiplayerGameController.PartySlotDesc partySlotDesc in partySlotDescs)
		{
			CharacterState character = partySlotDesc.Character;
			NetworkPlayer netPlayer = PlayerRegistry.GetPlayerAssignedToSlot(partySlotDesc.ID);
			MultiplayerGameState.Player player = (_view.IsOnline ? _view.MultiplayerGameState.Players.FirstOrDefault((MultiplayerGameState.Player p) => p.ID == netPlayer.PlayerID) : null);
			bool isLeader = player != null && _view.MultiplayerGameState.PartyLeaderID == player.ID;
			bool isAvailable = player == null || player.ID == _view.PlayerID;
			bool isSelected = character != null && character == selectedCharacter;
			PartySlotDTO slotData = GetSlotData((int)partySlotDesc.ID.ID, character, player, isLeader, isAvailable, isSelected);
			list.Add(slotData);
		}
		return list;
	}

	private MultiplayerGameController.PartySlotDesc[] GetPartySlots()
	{
		return _view.MultiplayerGameController.GetSlots().ToArray();
	}

	public bool IsSlotOwner(PartySlotDTO partySlotDTO)
	{
		return _view.MultiplayerGameController.GetSlotOwner(new PartySlotID(partySlotDTO.SlotId)) == _view.Player;
	}

	public bool TryGetCharacter(PartySlotDTO partySlotDTO, out CharacterState characterState, out bool isSlotOwner)
	{
		isSlotOwner = IsSlotOwner(partySlotDTO);
		if (partySlotDTO.IsEmpty)
		{
			characterState = null;
			return false;
		}
		return GameController.CharacterController.TryGetCharacter(partySlotDTO.CharacterID, out characterState);
	}

	public CharacterClassDef GetClassDef(PartySlotDTO partySlotDto)
	{
		CharacterClassID key = new CharacterClassID(partySlotDto.ClassID);
		return DefRepo.CharacterClasses[key];
	}

	public bool TrySelectPartySlot(PartySlotDTO partySlotDTO, out bool isSlotOwner)
	{
		TryGetCharacter(partySlotDTO, out var characterState, out isSlotOwner);
		if (!isSlotOwner || partySlotDTO.IsEmpty)
		{
			return false;
		}
		_view.SetSelectedCharacter(characterState, partySlotDTO.SlotId);
		return true;
	}

	private bool TrySelectPartySlot(int id)
	{
		CharacterState character = MultiplayerGameController.GetSlots().First((MultiplayerGameController.PartySlotDesc s) => s.ID.ID == id).Character;
		_view.SetSelectedCharacter(character, id);
		return true;
	}

	public bool TrySelectPartySlot(UIPartySlot partySlot, out bool isSlotOwner)
	{
		isSlotOwner = _view.MultiplayerGameController.GetSlotOwner(new PartySlotID(partySlot.SlotId)) == _view.Player;
		if (!isSlotOwner || partySlot.CurrentData.Character == ECharacter.None)
		{
			return false;
		}
		GameController.CharacterController.TryGetCharacter(partySlot.CurrentData.CharacterID, out var character);
		_view.SetSelectedCharacter(character, partySlot.SlotId);
		return true;
	}

	public void SwitchToPartySlot(UIPartySlot partySlot)
	{
		TrySelectPartySlot(partySlot, out var _);
	}

	public void SwitchToPartySlot(PartySlotDTO partySlot)
	{
		TrySelectPartySlot(partySlot, out var _);
	}

	public void SwitchToPartySlot(int id)
	{
		TrySelectPartySlot(id);
	}

	public bool IsSlotFree(PartySlotID id)
	{
		return _view.MultiplayerGameController.IsSlotFree(id);
	}

	public void ManagePartySlot(PartySlotDTO partySlotDTO)
	{
		bool isInPrologue = _view.GameState.IsInPrologue;
		bool flag = !_view.GameController.ScenarioController.HasActiveScenario() && _view.GameController.CharacterController.GetNewCharacterClasses().Any();
		CharacterState characterState2;
		bool isSlotOwner3;
		if (TrySelectPartySlot(partySlotDTO, out var isSlotOwner))
		{
			if (_view.GameController.ScenarioController.HasActiveScenario() && TryGetCharacter(partySlotDTO, out var characterState, out var _) && characterState.ClassDef.MaxCards != characterState.SelectedCardIDs.Count)
			{
				CharacterDataDTO characterDataDto = _view.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(characterState);
				_view.PushState(new HandManagementViewState(characterDataDto, readOnly: false));
			}
			else
			{
				_view.PushState(new EditCharacterViewState(partySlotDTO.WasImported & isInPrologue));
			}
		}
		else if ((isSlotOwner && partySlotDTO.IsEmpty) & flag)
		{
			_view.PushState(new NewCharacterViewState(partySlotDTO.SlotId, resetDetails: true));
		}
		else if (!isSlotOwner && TryGetCharacter(partySlotDTO, out characterState2, out isSlotOwner3))
		{
			CharacterDataDTO overrideCharacterDto = _view.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(characterState2);
			_view.SetSelectedCharacter(characterState2, partySlotDTO.SlotId);
			_view.PushState(new EditCharacterViewState(overrideCharacterDto, readOnly: true));
		}
		else if (isSlotOwner)
		{
			_view.ReplaceState(new ManageRosterViewState());
		}
	}

	public void HandleOnCharacterOwnershipChange(CharacterState characterState)
	{
		if (_view.IsPartyLeader && characterState != null && characterState.PartyState == CharacterPartyStateType.CreationProcess && !characterState.CreationFlowPassed)
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Character.CancelCreateNewCharacter, characterState.ID);
		}
	}

	public void SetInitialCreationFlowFinished(CharacterState characterState)
	{
		if (characterState == null)
		{
			Log.GUIWarning("Null character state");
			return;
		}
		Choreographer.ExecuteCommand(FHRPC.Campaign.Character.SetCreationFlowPassed, characterState.ID);
	}

	public void ViewCharacter(PartySlotDTO partySlotDTO)
	{
		if (!partySlotDTO.IsEmpty && TrySelectPartySlot(partySlotDTO, out var _))
		{
			_view.ReplaceState(new EditCharacterViewState(partySlotDTO.WasImported && _view.GameState.IsInPrologue));
		}
	}

	public List<PartySlotDTO> GetCreatedCharactersData([AllowNull] CharacterClassDef selectedCharacterDef)
	{
		return _view.GameState.AllCharacters.Select((CharacterState character, int index) =>
		{
			bool isAvailable = character.PartyState == CharacterPartyStateType.Removed;
			bool isSelected = character.ClassDef == selectedCharacterDef;
			return GetSlotData(index + 1, character, null, isLeader: false, isAvailable, isSelected);
		}).ToList();
	}

	public List<PartySlotDTO> GetAvailableClassesData([AllowNull] CharacterClassDef selectedCharacterDef, bool checkParty = true)
	{
		string selectedCharacterClassId = ((selectedCharacterDef != null) ? selectedCharacterDef.ID.ID : string.Empty);
		return _view.GameController.CharacterController.UnlockedClasses.Select((CharacterClassDef classDef, int index) =>
		{
			string iD = classDef.ID.ID;
			ECharacter eCharacter = CrossReference.ClassIdToECharacterModel(iD);
			return FromClassType(index + 1, iD, eCharacter, selectedCharacterClassId == iD, GameController.CharacterController.CanCreateCharacterFromClass(classDef, checkParty));
		}).ToList();
	}

	public void SetupPartyColumn(PartyColumnModule partyColumnModule, bool isInteractable = true, ConfirmActionDTO onNonInteractableClick = null)
	{
		List<PartySlotDTO> partySlotsData = GetPartySlotsData(_view.SelectedCharacter);
		PartyColumnModule.PartyColumnParams partyColumnParams = (isInteractable ? new PartyColumnModule.PartyColumnParams
		{
			slotData = partySlotsData,
			OnEditParty = () =>
			{
				_view.ReplaceState(new ManageRosterViewState());
			},
			OnSlotClick = null,
			HintedActions = GetPartyColumnSlotInteractions
		} : new PartyColumnModule.PartyColumnParams
		{
			slotData = partySlotsData,
			OnEditParty = () =>
			{
				if (onNonInteractableClick != null)
				{
					onNonInteractableClick.Execute(() =>
					{
						_view.ReplaceState(new ManageRosterViewState());
					}, null);
				}
			},
			HintedActions = ((onNonInteractableClick != null) ? ((UIPartySlot.SlotToHintedActions)((PartySlotDTO dto) => InsertAction(dto, onNonInteractableClick))) : null)
		});
		partyColumnModule.Init(partyColumnParams);
		IEnumerable<UIPartySlotHints.HintedActionParams> InsertAction(PartySlotDTO partySlotDto, ConfirmActionDTO action)
		{
			foreach (UIPartySlotHints.HintedActionParams interaction in GetPartyColumnSlotInteractions(partySlotDto))
			{
				yield return new UIPartySlotHints.HintedActionParams
				{
					InteractionType = interaction.InteractionType,
					ActionCallback = () =>
					{
						action.Execute(interaction.ActionCallback, null);
					}
				};
			}
		}
	}

	public bool LocalPlayerHasFreePartySlot()
	{
		return _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc s) => s.Character == null);
	}

	public bool LocalPlayerOwnsSlot(int slotId)
	{
		if (!_view.IsSoloGame)
		{
			return _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc slotDesc) => slotDesc.ID.ID == slotId);
		}
		return true;
	}

	public bool LocalPlayerOwnsCharacter(CharacterState character)
	{
		return _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc playerSlot) => playerSlot.Character?.ID == character.ID);
	}

	public bool LocalPlayerOwnsCharacter(CharacterID characterID)
	{
		return _view.PlayerSlots.Any((MultiplayerGameController.PartySlotDesc playerSlot) => playerSlot.Character?.ID == characterID);
	}

	public bool HasCharacterInRoster(CharacterID characterID)
	{
		return _view.GameState.AllCharacters.Any((CharacterState state) => state.ID == characterID);
	}

	public bool TryGetLocalPlayerFreePartySlot(out MultiplayerGameController.PartySlotDesc partySlot)
	{
		foreach (MultiplayerGameController.PartySlotDesc playerSlot in _view.PlayerSlots)
		{
			if (playerSlot.Character == null)
			{
				partySlot = playerSlot;
				return true;
			}
		}
		partySlot = default;
		return false;
	}

	public bool HasFreeHeroClass()
	{
		return _view.GameController.CharacterController.UnlockedClasses.FirstOrDefault((CharacterClassDef def) => GameController.CharacterController.CanCreateCharacterFromClass(def)) != null;
	}

	public void RedrawPersonalQuest()
	{
		CharacterState selectedCharacter = _view.SelectedCharacter;
		if (selectedCharacter.PersonalQuest != null)
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Character.CancelPersonalQuestSelection, selectedCharacter.ID);
		}
		if (MultiplayerGameController.CanDrawPesonalQuests(_view.PlayerID))
		{
			Choreographer.ExecuteCommand("DrawPersonalQuests", _view.PlayerID);
		}
	}

	public IEnumerable<UIPartySlotHints.HintedActionParams> GetSlotInteractions(PartySlotDTO partySlotDto, bool creationPhase)
	{
		bool isSlowOwner = IsSlotOwner(partySlotDto);
		bool hasActiveScenario = _view.GameController.ScenarioController.HasActiveScenario();
		if (((!hasActiveScenario & creationPhase) && partySlotDto.IsEmpty) & isSlowOwner)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_CREATE_CHARACTER_NAME_CONFIRM",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_CREATE_CHARACTER_NAME_CONFIRM"),
				ColorSchemeID = "AdditiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					ManagePartySlot(partySlotDto);
				}
			};
		}
		if ((!hasActiveScenario & creationPhase) && !partySlotDto.IsEmpty && !partySlotDto.InCreation)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = (isSlowOwner ? "GUI_EDIT_CHARACTER" : "GUI_VIEW_CHARACTER"),
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, isSlowOwner ? "GUI_EDIT_CHARACTER" : "GUI_VIEW_CHARACTER"),
				ColorSchemeID = "PositiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					ManagePartySlot(partySlotDto);
				}
			};
		}
		if (((!hasActiveScenario & creationPhase) && !partySlotDto.IsEmpty) & isSlowOwner)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_DELETE_CHARACTER",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, "GUI_DELETE_CHARACTER"),
				ColorSchemeID = "NegativeAction",
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = () =>
				{
					RemovePrologueCharacter(partySlotDto);
				}
			};
		}
		if (!creationPhase && !partySlotDto.IsEmpty)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_VIEW_CHARACTER",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VIEW_CHARACTER"),
				ColorSchemeID = "PositiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					ManagePartySlot(partySlotDto);
				}
			};
		}
		if (!creationPhase && !partySlotDto.IsEmpty)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_LEAVE",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.RightClick, "GUI_LEAVE"),
				ColorSchemeID = "NegativeAction",
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = () =>
				{
					RemoveCharacter(partySlotDto);
				}
			};
		}
		if (!creationPhase && partySlotDto.IsEmpty)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_ACTION_ADD",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ACTION_ADD"),
				ColorSchemeID = "PositiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					ManagePartySlot(partySlotDto);
				}
			};
		}
	}

	[Obsolete]
	private IEnumerable<UIPartySlotHints.HintedActionParams> ToNonInteractableClick(Action action)
	{
		if (action != null)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = action
			};
			yield return new UIPartySlotHints.HintedActionParams
			{
				InteractionType = FHInputEventElement.UserInteractionType.RightClick,
				ActionCallback = action
			};
		}
	}

	public IEnumerable<UIPartySlotHints.HintedActionParams> GetPartyColumnSlotInteractions(PartySlotDTO partySlotDto)
	{
		if (partySlotDto.IsEmpty)
		{
			if (partySlotDto.Owned)
			{
				yield return new UIPartySlotHints.HintedActionParams
				{
					HintKey = "GUI_ASSIGN_SLOT",
					HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_ASSIGN_SLOT"),
					ColorSchemeID = "PositiveAction",
					InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
					ActionCallback = () =>
					{
						ManagePartySlot(partySlotDto);
					}
				};
			}
			yield break;
		}
		yield return new UIPartySlotHints.HintedActionParams
		{
			HintKey = "GUI_SELECT",
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_SELECT"),
			ColorSchemeID = "PositiveAction",
			InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
			ActionCallback = () =>
			{
				SwitchToPartySlot(partySlotDto);
			}
		};
		yield return new UIPartySlotHints.HintedActionParams
		{
			HintKey = "GUI_VIEW_CHARACTER",
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VIEW_CHARACTER"),
			ColorSchemeID = "PositiveAction",
			InteractionType = FHInputEventElement.UserInteractionType.RightClick,
			ActionCallback = () =>
			{
				ViewCharacter(partySlotDto);
			}
		};
	}

	private void RemoveCharacter(PartySlotDTO slotDTO)
	{
		GameController.CharacterController.TryGetCharacter(slotDTO.CharacterID, out var character);
		RemoveCharacterFromParty(character);
	}

	private void RemovePrologueCharacter(PartySlotDTO slotDTO)
	{
		PartySlotDTO partySlotDTO = slotDTO;
		if (!IsSlotOwner(partySlotDTO))
		{
			return;
		}
		if (!GameController.CharacterController.TryGetCharacter(slotDTO.CharacterID, out var _))
		{
			Log.GUIError($"Could not find character [{partySlotDTO.Character}]");
			return;
		}
		_view.PushState(new GenericModalViewState(LocalizationManager.GetTranslation("GUI_DELETE_CHARACTER_CONFIRMATION"), LocalizationManager.GetTranslation("GUI_DELETE_CHARACTER"), (bool deleteConfirmed) =>
		{
			if (deleteConfirmed)
			{
				CancelCreateNewCharacter(slotDTO.CharacterID);
			}
		}));
	}

	public uint NewCharacterBonusLevelFromProsperity()
	{
		return Math.Max(1u, (uint)Math.Ceiling((float)_view.GameState.Outpost.ProsperityLevel / 2f));
	}

	public bool HasBonusStartingGold()
	{
		return _view.GameController.CharacterController.NewCharacterGoldAmount > _view.GameState.GameMode.StartingCharacterGold;
	}

	public bool HasValidCharacterSelection(FHMapViewState state)
	{
		if (!(state is TravellingMerchantViewState) && !(state is ShopViewState) && !(state is AlchemistViewState) && !(state is EnhancerViewState) && !(state is CraftingViewState) && !(state is EditCharacterViewState) && !(state is HandManagementViewState) && !(state is UnlockCardsViewState) && !(state is CharacterPerksViewState) && !(state is CharacterEquipmentViewState) && !(state is RenameCharacterViewState) && !(state is SpendStartingGoldViewState))
		{
			if (state is ManageRosterViewState || state is ScenarioSelectionViewState || state is OutpostVillageMapState || state is BattleGoalSelectionViewState || state is ChallengeSelectionViewState || state is PetsSelectionViewState || state is FavorsSelectionViewState || state is ScenarioBriefingSummaryViewState || state is DistributionViewState || state is EventViewState || state is NarrationViewState || state is OutpostAttackViewState || state is BuildingMaterialDistributionViewState || state is ProloguePartyCreationViewState || state is GenericListModalViewState || state is GenericModalViewState || state is ImportFromPartyViewState || state is InitViewState || state is PersonalQuestSelectionViewState || state is TownPerksViewState)
			{
				return true;
			}
			Log.GUIWarning($"View State {state} is not listed. Please include desired behaviors");
			return false;
		}
		if (_view.SelectedCharacter != null)
		{
			return _view.PlayerCharacters.Any((CharacterState characterState) => _view.SelectedCharacter == characterState);
		}
		return false;
	}

	public void StartCreateNewCharacter(CharacterDataDTO characterData, out CharacterID characterID, Action OnSuccessCallback = null, Action OnFallCallback = null)
	{
		GameController.CharacterController.GetCharacterNameSizeRange(out var _, out var max);
		string charNameTrimmed = characterData.Name.Take(max);
		if (_view.GameState.AllCharacters.Any((CharacterState c) => c.Name == charNameTrimmed) || _view.GameState.CreatingCharacters.Any((CharacterState c) => c.Name == charNameTrimmed))
		{
			charNameTrimmed += "#";
		}
		characterID = CharacterID.NewID();
		FHMultiplayerClient.Promise promise = ((characterData.SlotId != -1) ? Choreographer.ExecuteCommand("StartCreateNewCharacter", new PartySlotID(characterData.SlotId), characterData.Class, characterID, charNameTrimmed) : Choreographer.ExecuteCommand("StartCreateNewCharacterWithoutSlot", characterData.Class, characterID, charNameTrimmed));
		characterData.CharacterID = characterID;
		if (_view.IsOnline)
		{
			promise.OnFailed(OnFallCallback);
			promise.OnComplete(OnSuccessCallback);
		}
		else
		{
			OnSuccessCallback?.Invoke();
		}
		if (MultiplayerGameController.CanDrawPesonalQuests(_view.PlayerID))
		{
			Choreographer.ExecuteCommand("DrawPersonalQuests", _view.PlayerID);
		}
	}

	public void CancelCreateNewCharacter(CharacterID characterID)
	{
		Choreographer.ExecuteCommand(FHRPC.Campaign.Character.CancelCreateNewCharacter, characterID);
	}

	public void RemoveCharacterFromParty(CharacterState characterState)
	{
		bool flag = _view.SelectedCharacter == characterState;
		Choreographer.ExecuteCommand(FHRPC.Campaign.Character.RemoveCharacterFromParty, characterState.ID);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.heroRemoveAudioItem);
		if (flag)
		{
			_view.TryEnsureSelectedCharacter();
		}
	}

	public void DeleteCharacter(CharacterState characterState)
	{
		_view.PushState(new GenericModalViewState(LocalizationManager.GetTranslation("GUI_DELETE_CHARACTER_CONFIRMATION"), LocalizationManager.GetTranslation("GUI_DELETE_CHARACTER"), (bool deleteConfirmed) =>
		{
			if (deleteConfirmed)
			{
				bool flag = _view.SelectedCharacter == characterState;
				Choreographer.ExecuteCommand(FHRPC.Campaign.Character.DeleteCharacter, characterState.ID);
				if (flag)
				{
					_view.TryEnsureSelectedCharacter();
				}
			}
		}));
	}

	public void LeveUpCharacter(CharacterState characterState)
	{
		uint levelFromProsperity = _view.GameController.CharacterController.GetLevelFromProsperity();
		_view.PushState(new GenericModalViewState(FHUILocFormatting.PopulateParams("GUI_CHARACTER_LEVEL_CU_PROMPT_DESC", levelFromProsperity), LocalizationManager.GetTranslation("GUI_CHARACTER_LEVEL_CU_PROMPT_TITLE"), (bool confirmed) =>
		{
			if (confirmed)
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Character.FinalizeProsperityLevelUp, characterState.ID);
			}
		}));
	}

	private void SetSlotTag(int slot, [AllowNull] CharacterClassDef classDef)
	{
		string text = _view.MultiplayerGameState.UITags.FirstOrDefault((string s) => s.StartsWith("SLOT" + slot));
		if (!string.IsNullOrEmpty(text))
		{
			Choreographer.ExecuteCommand("RemoveUITag", text);
		}
		if (!(classDef == null))
		{
			string text2 = string.Format("{0}{1}={2}", "SLOT", slot, classDef.ID.ID);
			Choreographer.ExecuteCommand("AddUITag", text2);
		}
	}

	public void ConfirmPrologueParty()
	{
		Choreographer.ExecuteCommand("FinishCreateCharacters");
	}

	public void ConfirmCharacterCreation(CharacterID characterID, bool addToParty, bool levelUpToProsperity)
	{
		Choreographer.ExecuteCommand("FinishCreateCharacter", characterID, addToParty, levelUpToProsperity);
	}

	public void AddCharacterToParty(CharacterState character, uint slotId)
	{
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Character.AddCharacterFromParty, character.ID);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.heroAddAudioItem);
		if (_view.PlayerSlots.FirstOrDefault((MultiplayerGameController.PartySlotDesc psd) => psd.Character == character).ID.ID != slotId)
		{
			_view.Choreographer.ExecuteCommand("AssignCharacterToSlot", new PartySlotID(slotId), character.ID);
		}
	}
}
