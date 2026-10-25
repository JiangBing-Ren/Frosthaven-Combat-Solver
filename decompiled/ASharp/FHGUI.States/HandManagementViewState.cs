using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class HandManagementViewState : FHMapViewState
{
	private struct ClassNameCombo(CharacterDataDTO characterDataDto) : IEquatable<ClassNameCombo>
	{
		public string Name = characterDataDto?.Name;

		public CharacterClassID ClassId = new CharacterClassID(characterDataDto?.Class.ID.ID);

		public bool Equals(ClassNameCombo other)
		{
			if (Name == other.Name)
			{
				return ClassId.Equals(other.ClassId);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ClassNameCombo other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, ClassId);
		}
	}

	private CharacterDataDTO _characterDataDto;

	private bool _readOnly;

	private readonly HeaderViewService _headerService;

	private readonly CardViewService _cardService;

	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly QuickAccessBarViewService _quickAccessBarService;

	private readonly PartyViewService _partyService;

	private readonly UICardInteractionEvents _handCardInteractions = new UICardInteractionEvents();

	private readonly UICardInteractionEvents _deckCardInteractions = new UICardInteractionEvents();

	private List<CardDTO> _tempCardDtos;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private MainHeaderModule MainHeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyColumn => View.Modules.Characters.PartyColumn;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private SimpleHeaderModule SimpleHeader => View.Modules.Characters.Header;

	[Preserve]
	private HandCardListModule HandCardListModule => View.Modules.Characters.HandCardList;

	[Preserve]
	private DeckCardListModule DeckCardListModule => View.Modules.Characters.DeckCardList;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private CharacterController CharacterController => GameController.CharacterController;

	public bool IsHandValid(bool useTempCards, out string reason)
	{
		if (_readOnly)
		{
			reason = null;
			return true;
		}
		if (!useTempCards)
		{
			if (!TryGetCharacterState(out var characterState))
			{
				reason = "missing_character_state";
				return false;
			}
			return CharacterController.IsCharacterHandValid(characterState, out reason);
		}
		IEnumerable<ClassCardDef> selectedCards = _tempCardDtos.Where((CardDTO c) => c.IsInHand).Select(_cardService.GetCardDef);
		return CharacterController.IsCharacterHandValid(_characterDataDto.Class.ID, selectedCards, (int)_characterDataDto.Class.MaxCards, out reason);
	}

	public HandManagementViewState(CharacterDataDTO characterDataDto, bool readOnly)
	{
		_characterDataDto = characterDataDto;
		_readOnly = readOnly;
		_headerService = View.GetService<HeaderViewService>();
		_cardService = View.GetService<CardViewService>();
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_quickAccessBarService = View.GetService<QuickAccessBarViewService>();
		_partyService = View.GetService<PartyViewService>();
		_handCardInteractions.OnClick = OnHandCardClicked;
		_deckCardInteractions.OnClick = OnDeckCardClicked;
		FillTempCardsFromCharacterDTO();
	}

	public override void OnShow()
	{
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		_characterDetailsService.EnsureAbilityCards();
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		ClassNameCombo other = new ClassNameCombo(_characterDataDto);
		bool flag = TryGetCharacterState(out var characterState);
		_readOnly = _readOnly || View.SelectedCharacter != characterState;
		if (_characterDataDto == null)
		{
			_characterDataDto = _characterDetailsService.PopulateCharacterDTO(View.SelectedCharacter);
		}
		else if (!_readOnly && View.SelectedCharacter != characterState)
		{
			_characterDataDto = _characterDetailsService.PopulateCharacterDTO(View.SelectedCharacter);
		}
		else if (!_readOnly & flag)
		{
			_characterDataDto = _characterDetailsService.PopulateCharacterDTO(characterState);
		}
		if (!new ClassNameCombo(_characterDataDto).Equals(other))
		{
			FillTempCardsFromCharacterDTO();
		}
		bool flag2 = IsHandValid(useTempCards: true, out var reason);
		bool flag3 = View.GameState.ActiveScenario != null;
		ConfirmActionDTO confirmActionDTO = new ConfirmActionDTO
		{
			TitleKey = "GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_TITLE",
			DescKey = ((reason == "insufficient_mode_cards") ? "GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_HIVE" : "GUI_HANDMANAGEMENT_INVALID_DECK_PROMPT_GENERIC"),
			ExecuteAction = ShowInvalidHandModal
		};
		string simpleHeaderTitle = (_readOnly ? "Preview Cards" : "Edit Hand");
		bool flag4 = _headerService.SetupCharacterDetailsHeader(MainHeaderModule, SimpleHeader, simpleHeaderTitle, HeaderViewService.HeaderFunctionality.All, forceSimpleHeader: false, flag2 ? null : confirmActionDTO);
		DeckCardListModule.Init(new CardListModuleBase.Params
		{
			Cards = _tempCardDtos,
			InteractionEvents = _deckCardInteractions,
			HintedActions = _cardService.GetHandCardInteractionHints
		});
		_cardService.FillHandCards(_characterDataDto, HandCardListModule, _handCardInteractions, _tempCardDtos);
		CharacterDetailsScreenModule.ClearAll();
		if (flag3)
		{
			_quickAccessBarService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.HandManagement, UIBuildingStateFilter.None, null, isInteractable: true, null, flag2 ? null : confirmActionDTO);
			if (flag2)
			{
				CharacterDetailsScreenModule.ShowCloseButton(PopThisState);
			}
			PartyColumn.gameObject.SetActive(value: false);
		}
		else if (flag4)
		{
			CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(PopThisState, flag2));
			QuickAccessBar.gameObject.SetActive(value: false);
			PartyColumn.gameObject.SetActive(value: false);
			MainHeaderModule.gameObject.SetActive(value: false);
		}
		else
		{
			_quickAccessBarService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.HandManagement, UIBuildingStateFilter.None, null, isInteractable: true, null, flag2 ? null : confirmActionDTO);
			_partyService.SetupPartyColumn(PartyColumn, flag2, confirmActionDTO);
		}
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.HandManagement);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
	}

	private void ShowInvalidHandModal(Action onConfirm, Action onCancel, string titleKey, string descKey)
	{
		View.PushState(new GenericModalViewState(LocalizationManager.GetTranslation(descKey), LocalizationManager.GetTranslation(titleKey), LocalizationManager.GetTranslation("GUI_LEAVE"), LocalizationManager.GetTranslation("GUI_STAY"), (bool result) =>
		{
			if (result)
			{
				onConfirm?.Invoke();
			}
			else
			{
				onCancel?.Invoke();
			}
		}));
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
		HandCardListModule.Clear();
		DeckCardListModule.Clear();
	}

	private void OnHandCardClicked(CardDTO cardDto)
	{
		if (!_readOnly)
		{
			cardDto.IsInHand = false;
			AudioControllerUtils.PlaySound(UIInfoTools.Instance?.cardRemoveFromDeckAudioItem);
			TryExecuteCardDeltaCommands();
		}
	}

	private void OnDeckCardClicked(CardDTO cardDto)
	{
		if (!_readOnly && cardDto.IsSelectable)
		{
			cardDto.IsInHand = !cardDto.IsInHand;
			AudioControllerUtils.PlaySound((!cardDto.IsInHand) ? UIInfoTools.Instance?.cardRemoveFromDeckAudioItem : UIInfoTools.Instance?.cardToDeckEquipAudioItem);
			TryExecuteCardDeltaCommands();
		}
	}

	private bool TryExecuteCardDeltaCommands()
	{
		if (_readOnly)
		{
			return false;
		}
		if (!IsHandValid(useTempCards: true, out var _))
		{
			OnShow();
			return false;
		}
		foreach (CardDTO item in _tempCardDtos.Where((CardDTO t) => !t.IsInHand && _characterDataDto.Cards.First((CardDTO c) => t.Id == c.Id).IsInHand))
		{
			ExecuteCardCommand(FHRPC.Campaign.Character.UnselectCard, item.Id);
		}
		foreach (CardDTO item2 in _tempCardDtos.Where((CardDTO t) => t.IsInHand && !_characterDataDto.Cards.First((CardDTO c) => t.Id == c.Id).IsInHand))
		{
			ExecuteCardCommand(FHRPC.Campaign.Character.SelectCard, item2.Id);
		}
		OnShow();
		return true;
	}

	private void ExecuteCardCommand(string commandName, int id)
	{
		ClassCardDef cardDef = _cardService.GetCardDef(id);
		View.Choreographer.ExecuteCommand(commandName, _characterDataDto.CharacterID, cardDef);
	}

	private bool TryGetCharacterState(out CharacterState characterState)
	{
		return CharacterController.TryGetCharacter(_characterDataDto.CharacterID, out characterState);
	}

	private void FillTempCardsFromCharacterDTO()
	{
		_tempCardDtos = _characterDataDto?.Cards.ToList() ?? new List<CardDTO>();
	}
}
