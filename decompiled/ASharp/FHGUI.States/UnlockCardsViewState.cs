using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class UnlockCardsViewState : FHMapViewState
{
	private CharacterDataDTO _characterDataDto;

	private readonly bool _readOnly;

	private readonly HeaderViewService _headerService;

	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly CardViewService _cardService;

	private readonly QuickAccessBarViewService _quickAccessService;

	private readonly UICardInteractionEvents _newCardsInteractions = new UICardInteractionEvents();

	private readonly PartyViewService _partyService;

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
	private NewCardsListModule NewCardsListModule => View.Modules.Characters.NewCardsList;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private CharacterController CharacterController => GameController.CharacterController;

	public UnlockCardsViewState(CharacterDataDTO characterDataDto, bool readOnly)
	{
		_characterDataDto = characterDataDto;
		_readOnly = readOnly;
		_headerService = View.GetService<HeaderViewService>();
		_cardService = View.GetService<CardViewService>();
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_quickAccessService = View.GetService<QuickAccessBarViewService>();
		_partyService = View.GetService<PartyViewService>();
		_newCardsInteractions.OnClick = OnNewCardClicked;
	}

	public void RefreshView()
	{
		View.RefreshView();
	}

	public override void OnShow()
	{
		PartyAdventureData.OnRulesChanged = (Action)Delegate.Remove(PartyAdventureData.OnRulesChanged, new Action(RefreshView));
		PartyAdventureData.OnRulesChanged = (Action)Delegate.Combine(PartyAdventureData.OnRulesChanged, new Action(RefreshView));
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		CharacterDetailsScreenModule.ClearAll();
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		bool flag = _headerService.SetupCharacterDetailsHeader(MainHeaderModule, SimpleHeader, string.Empty);
		if (_characterDataDto == null || !flag)
		{
			_characterDataDto = _characterDetailsService.PopulateCharacterDTO(View.SelectedCharacter);
		}
		bool flag2 = _cardService.TryGetCardUnlocks(_characterDataDto.CharacterID, out var characterState, out var cardUnlocks, out var fullyUnlocked);
		string hinKey;
		if (cardUnlocks > 0)
		{
			hinKey = "GUI_UNLOCKCARDS_CANUNLOCK";
		}
		else
		{
			hinKey = (fullyUnlocked ? "GUI_UNLOCKCARDS_MAXUNLOCK_REACHED" : "GUI_UNLOCKCARDS_CANNOTUNLOCK");
		}
		if (!_readOnly & flag2)
		{
			_characterDataDto = _characterDetailsService.PopulateCharacterDTO(characterState);
		}
		int characterLevel = (int)((!flag2) ? 1 : _characterDataDto.Level);
		IEnumerable<CardDTO> levelUpCards = _cardService.GetLevelUpCards(_characterDataDto);
		NewCardsListModule.Init(levelUpCards, hinKey, cardUnlocks, characterLevel, View.GameController.CharacterController.CanRespec(), View.GameController.CharacterController.GetCanRespecCharacter(characterState), View.GameController.CharacterController.GetCharacterCanAffordRespec(characterState), View.GameController.CharacterController.GetCostToRespec(characterState), OnRespecClicked, _newCardsInteractions);
		if (flag)
		{
			QuickAccessBar.gameObject.SetActive(value: false);
			PartyColumn.gameObject.SetActive(value: false);
			MainHeaderModule.gameObject.SetActive(value: false);
		}
		else
		{
			_quickAccessService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.NewCards);
			_partyService.SetupPartyColumn(PartyColumn);
		}
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
	}

	private void OnRespecClicked()
	{
		CharacterState character = View.GameState.AllCharacters.Find((CharacterState x) => x.ID.Equals(_characterDataDto.CharacterID));
		View.PushState(new GenericModalViewState(FHUILocFormatting.PopulateParams("GUI_RESPEC_CONFIRM", View.GameController.CharacterController.GetCostToRespec(character)), LocalizationManager.GetTranslation("GUI_RESPEC"), (bool Confirmed) =>
		{
			if (Confirmed)
			{
				View.Choreographer.ExecuteCommand(FHRPC.Campaign.Character.Respec, _characterDataDto.CharacterID);
			}
			View.PopState(FHMapViewLayer.LayerType.Modal);
		}), FHMapViewLayer.LayerType.Modal);
	}

	public override void OnHide()
	{
		PartyAdventureData.OnRulesChanged = (Action)Delegate.Remove(PartyAdventureData.OnRulesChanged, new Action(RefreshView));
		CharacterDetailsScreenModule.ClearAll();
		NewCardsListModule.Clear();
	}

	private void OnNewCardClicked(CardDTO cardDto)
	{
		if (cardDto.IsDisabled || cardDto.IsInHand)
		{
			return;
		}
		ClassCardDef cardDef = _cardService.GetCardDef(cardDto);
		if (!CharacterController.TryGetCharacter(_characterDataDto.CharacterID, out var character) || !CharacterController.CanUnlockCard(character, cardDef))
		{
			return;
		}
		View.PushState(new GenericModalViewState(FHUILocFormatting.PopulateParams("GUI_UNLOCK_CARD_DESC", LocalizationManager.GetTranslation(cardDto.NameLocKey)), LocalizationManager.GetTranslation("GUI_UNLOCK_CARD_TITLE"), (bool Confirmed) =>
		{
			if (Confirmed)
			{
				View.Choreographer.ExecuteCommand(FHRPC.Campaign.Character.UnlockCard, _characterDataDto.CharacterID, cardDef);
			}
			View.PopState(FHMapViewLayer.LayerType.Modal);
		}), FHMapViewLayer.LayerType.Modal);
	}
}
