using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class PersonalQuestSelectionViewState : FHMapViewState
{
	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly PartyViewService _partyService;

	private readonly CharacterID _characterID;

	private readonly PersonalQuestDef _leftQuestDef;

	private readonly PersonalQuestDef _rightQuestDef;

	private readonly bool _creationProcess;

	private readonly bool _unassignedCharacter;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private CharacterPreviewModule CharacterPreviewModule => View.Modules.Characters.CharacterPreview;

	[Preserve]
	private HeroClassStatsModule HeroClassStatsModule => View.Modules.Characters.HeroClassStats;

	[Preserve]
	private CharacterBiographyModule CharacterBiographyModule => View.Modules.Characters.CharacterBio;

	[Preserve]
	private HandCardListModule HandCardListModule => View.Modules.Characters.AbilitiesHandCard;

	[Preserve]
	private PersonalQuestSelectionModule PersonalQuestSelectionModule => View.Modules.Characters.PersonalQuestSelection;

	public PersonalQuestSelectionViewState(CharacterState characterState, PersonalQuestDef leftQuestDef, PersonalQuestDef rightQuestDef, bool creationProcess, bool unassignedCharacter)
	{
		_characterID = characterState.ID;
		_leftQuestDef = leftQuestDef;
		_rightQuestDef = rightQuestDef;
		_creationProcess = creationProcess;
		_unassignedCharacter = unassignedCharacter;
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_partyService = View.GetService<PartyViewService>();
	}

	public override void OnShow()
	{
		if (!_unassignedCharacter && !_partyService.LocalPlayerOwnsCharacter(_characterID))
		{
			View.ResetView();
			return;
		}
		if (!GameController.CharacterController.TryGetCharacter(_characterID, out var character) || !View.GameController.CharacterController.IsCharacterNameValid(character))
		{
			PopThisState();
			return;
		}
		bool flag = character.PersonalQuestDef != null;
		List<ButtonsBarParam> list = new List<ButtonsBarParam>();
		list.Add(CharacterDetailsScreenModule.Button_Back(OnBack, _creationProcess | flag));
		if (_creationProcess)
		{
			list.Add(CharacterDetailsScreenModule.Button_Continue(OnContinue, flag));
		}
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(list.ToArray());
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint("GUI_SELECT_QUEST");
		}
		CharacterDataDTO characterDataDTO = _characterDetailsService.PopulateCharacterDTO(character);
		CharacterPreviewModule.Init(characterDataDTO, null, showValues: false);
		_characterDetailsService.PopulateCharacterDetails(characterDataDTO, character.PartyState == CharacterPartyStateType.CreationProcess, CharacterPreviewModule, HeroClassStatsModule, CharacterBiographyModule, HandCardListModule);
		if (_creationProcess)
		{
			bool excludePersonalQuest = !_characterDetailsService.HasAvailablePersonalQuest(View.PlayerID);
			CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(excludePersonalQuest), 2);
		}
		CharacterDetailsScreenModule.SetCharacterClass(characterDataDTO.Class);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.PersonalQuest);
		View.OutpostSceneController.OpenCharacterScreen(characterDataDTO);
		UpdateSelectionModule();
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void UpdateSelectionModule()
	{
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			PersonalQuestSelectionModule.Init(character.PersonalQuestDef, (_leftQuestDef != null) ? new CharacterPersonalQuestDTO(_leftQuestDef, GameController.CharacterController.CampaignController) : null, (_rightQuestDef != null) ? new CharacterPersonalQuestDTO(_rightQuestDef, GameController.CharacterController.CampaignController) : null, OnQuestSelected, OnQuestVisibilityChanged);
		}
	}

	private void OnQuestVisibilityChanged(bool isOn)
	{
	}

	private void OnQuestSelected(PersonalQuestDef def)
	{
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Character.SelectPersonalQuest, character.ID, def, false);
		}
	}

	private void OnBack()
	{
		if (!_creationProcess)
		{
			Choreographer.ExecuteCommand("ClearPersonalQuests", View.PlayerID);
			Choreographer.ExecuteCommand(FHRPC.Campaign.Character.FinishSelectPersonalQuest, _characterID, false);
		}
		PopThisState();
	}

	private void OnContinue()
	{
		if (GameController.CharacterController.TryGetCharacter(_characterID, out var character))
		{
			View.PushState(new SpendStartingGoldViewState(character, _creationProcess, _unassignedCharacter));
		}
	}
}
