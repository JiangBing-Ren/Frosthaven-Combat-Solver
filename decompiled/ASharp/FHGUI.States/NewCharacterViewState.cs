using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using UnityEngine;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class NewCharacterViewState : FHMapViewState
{
	private readonly CharacterDataDTO _characterData;

	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly PartyViewService _partyService;

	private bool _resetDetails;

	private FHTabButton _activeTab;

	private bool _ignoreRefresh;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	[Preserve]
	private CharactersSelectorModule ClassesListModule => View.Modules.Characters.ClassesList;

	[Preserve]
	private HeroClassStatsModule HeroClassStatsModule => View.Modules.Characters.HeroClassStats;

	[Preserve]
	private CharacterBiographyModule CharacterBioModule => View.Modules.Characters.CharacterBio;

	[Preserve]
	private HandCardListModule HandCardListModule => View.Modules.Characters.AbilitiesHandCard;

	public NewCharacterViewState(int slotId = -1, bool resetDetails = false)
	{
		NewCharacterViewState newCharacterViewState = this;
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_partyService = View.GetService<PartyViewService>();
		_resetDetails = resetDetails;
		_characterData = new CharacterDataDTO
		{
			SlotId = slotId
		};
		CharacterClassDef characterClassDef = View.GameController.CharacterController.UnlockedClasses.FirstOrDefault((CharacterClassDef def) => newCharacterViewState.GameController.CharacterController.CanCreateCharacterFromClass(def, slotId != -1));
		OnClassSelected(characterClassDef, callOnShow: false);
	}

	public override void OnShow()
	{
		if (_ignoreRefresh)
		{
			return;
		}
		if (_characterData.SlotId != -1 && !_partyService.LocalPlayerOwnsSlot(_characterData.SlotId))
		{
			PopThisState();
			return;
		}
		bool isInParty = _characterData.SlotId != -1;
		if (!GameController.CharacterController.CanCreateCharacterFromClass(_characterData.Class, isInParty))
		{
			CharacterClassDef characterClassDef = View.GameController.CharacterController.UnlockedClasses.FirstOrDefault((CharacterClassDef def) => GameController.CharacterController.CanCreateCharacterFromClass(def, isInParty));
			if (characterClassDef == null)
			{
				PopThisState();
				return;
			}
			Log.GUIWarning($"Re-Selection character From {_characterData.Class} To {characterClassDef}");
			OnClassSelected(characterClassDef, callOnShow: false);
			_resetDetails = true;
		}
		SavePayload();
		bool flag = _characterData.Class != null && GameController.CharacterController.CanCreateCharacterFromClass(_characterData.Class, isInParty);
		ButtonsBarParam[] buttonsBar = new ButtonsBarParam[2]
		{
			CharacterDetailsScreenModule.Button_Back(OnBack),
			CharacterDetailsScreenModule.Button_Continue(OnContinue, flag)
		};
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(buttonsBar);
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint("GUI_ONE_HERO_PER_CLASS");
		}
		_characterDetailsService.PopulateCharacterDetails(_characterData, CharacterPreviewModule, null, HeroClassStatsModule, CharacterBioModule, isTempCharacter: true);
		List<PartySlotDTO> availableClassesData = _partyService.GetAvailableClassesData(_characterData.Class, isInParty);
		ClassesListModule.Init(availableClassesData, OnClassSlotClicked);
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(CrossReference.ClassDefToECharacterModel(_characterData.Class));
		Color characterAltColor = UIInfoTools.Instance.GetCharacterAltColor(CrossReference.ClassDefToECharacterModel(_characterData.Class));
		bool isDualDeck = _characterData.Cards.Any((CardDTO c) => c.IsAltColor);
		CardListModuleBase.Params obj = new CardListModuleBase.Params
		{
			Cards = _characterData.Cards,
			InteractionEvents = null,
			MinCount = 0
		};
		HandCardListModule.Init(obj, characterColor, characterAltColor, isDualDeck);
		bool excludePersonalQuest = !_characterDetailsService.HasAvailablePersonalQuest(View.PlayerID);
		CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(excludePersonalQuest), 0);
		CharacterDetailsScreenModule.SetCharacterClass(_characterData.Class);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.ClassSelect);
		if (_characterDetailsService.CharacterHasIntroVideo(_characterData.Class.ID))
		{
			CharacterDetailsScreenModule.ShowHeroIntroButton(() =>
			{
				_characterDetailsService.ShowCharacterIntroVideo(_characterData.Class.ID, fullscreen: false, closeOnComplete: false);
			});
		}
		View.OutpostSceneController.OpenCharacterScreen(_characterData);
		if (_resetDetails)
		{
			_resetDetails = false;
			CharacterDetailsScreenModule.ResetCharacterCategories();
			SavePayload();
		}
		LoadPayload();
	}

	public override void OnHide()
	{
		ClearPayload();
		CharacterDetailsScreenModule.ClearAll();
	}

	protected override void SavePayload()
	{
		_activeTab = CharacterDetailsScreenModule.InfoCategoriesActiveTab;
	}

	protected override void LoadPayload()
	{
		CharacterDetailsScreenModule.TrySetInfoCategoriesActiveTab(_activeTab);
	}

	protected override void ClearPayload()
	{
		_activeTab = null;
	}

	private void OnClassSlotClicked(UIPartySlot slot)
	{
		CharacterClassDef characterClassDef = CrossReference.ECharacterModelToClassDef(slot.CurrentData.ClassID);
		OnClassSelected(characterClassDef, callOnShow: true);
	}

	private void OnClassSelected(CharacterClassDef characterClassDef, bool callOnShow)
	{
		_characterData.Class = characterClassDef;
		_characterData.SetPersonalQuest(null);
		_characterData.Name = LocalizationManager.GetTranslation(characterClassDef.ClassName);
		_characterData.Experience = new RangedValueDTO(0, (int)View.GameController.CharacterController.GetExperienceForLevel(2u));
		_characterData.Cards.Clear();
		_characterData.Level = 1u;
		_characterData.Health = new RangedValueDTO((int)View.GameController.CharacterController.GetCharacterHealth(characterClassDef, 1u), (int)View.GameController.CharacterController.GetCharacterHealth(characterClassDef, View.GameController.CharacterController.GetMaxLevel()));
		IEnumerable<ClassCardDef> cardsAvailableAtLevel = characterClassDef.GetCardsAvailableAtLevel(View.GameController.DefRepository, (int)_characterData.Level);
		int num = (int)characterClassDef.MaxCards;
		foreach (ClassCardDef item in cardsAvailableAtLevel)
		{
			_characterData.Cards.Add(new CardDTO(item, num > 0));
			num--;
		}
		if (callOnShow)
		{
			OnShow();
		}
	}

	private void OnBack()
	{
		if (MultiplayerGameController.GetPersonalQuests(View.PlayerID).Any())
		{
			Choreographer.ExecuteCommand("ClearPersonalQuests", View.PlayerID);
		}
		PopThisState();
		CharacterDetailsScreenModule.ClearButtonsBar();
	}

	private void OnContinue()
	{
		_ignoreRefresh = true;
		_partyService.StartCreateNewCharacter(_characterData, out var _, () =>
		{
			_ignoreRefresh = false;
			if (GameController.CharacterController.TryGetCharacter(_characterData.CharacterID, out var character))
			{
				View.SetSelectedCharacter(character, _characterData.SlotId);
			}
			View.PushState(new RenameCharacterViewState(_characterData.CharacterID, string.Empty, creationProcess: true, _characterData.SlotId == -1));
		}, () =>
		{
			_ignoreRefresh = false;
		});
		ButtonsBarParam[] buttonsBar = new ButtonsBarParam[2]
		{
			CharacterDetailsScreenModule.Button_Back(OnBack, interactable: false),
			CharacterDetailsScreenModule.Button_Continue(OnContinue, interactable: false)
		};
		CharacterDetailsScreenModule.SetButtonsBar(buttonsBar);
	}
}
