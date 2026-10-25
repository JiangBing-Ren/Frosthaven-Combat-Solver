using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;

namespace FHGUI.Testing;

public class RenameCharacterState : TestState
{
	private readonly bool _creationProcess;

	private string _characterName;

	private readonly CharacterState _characterState;

	private readonly CharacterDataDTO _characterData;

	private CharacterDetailsScreenModule CharacterDetailsScreenModule => Controller.GetModule<CharacterDetailsScreenModule>();

	private CharacterPreviewModule CharacterPreviewModule => Controller.GetModule<CharacterPreviewModule>();

	private CharacterGoalsModule CharacterGoalsModule => Controller.GetModule<CharacterGoalsModule>();

	private HeroClassStatsModule HeroClassStatsModule => Controller.GetModule<HeroClassStatsModule>();

	private CharacterBiographyModule CharacterBiographyModule => Controller.GetModule<CharacterBiographyModule>();

	private CharacterRenameModule CharacterRenameModule => Controller.GetModule<CharacterRenameModule>();

	private CharacterPersonalQuestModule PersonalQuestModule => Controller.GetModule<CharacterPersonalQuestModule>();

	private HandCardListModule HandCardListModule => Controller.GetModule<HandCardListModule>();

	public RenameCharacterState(TestingCharacterCreationFlowViewData controller, CharacterState characterState = null, bool creationProcess = false)
		: base(controller)
	{
		_characterState = characterState;
		_creationProcess = creationProcess;
		_characterName = (creationProcess ? string.Empty : characterState?.Name);
		_characterData = TestingCharacterCreationFlowUtils.BuildCharacterDTO(Controller.DefRepo, _characterState);
	}

	public override void OnShow()
	{
		bool flag = Controller.IsCharacterNameValid(_characterName);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(OnBack), _creationProcess ? CharacterDetailsScreenModule.Button_Continue(OnContinue, flag) : CharacterDetailsScreenModule.Button_Rename(OnContinue));
		CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(), 1);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.Rename);
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint("GUI_CHARACTER_INVALID_NAME");
		}
		TestingCharacterCreationFlowUtils.PopulateCharacterDetails(Controller.DefRepo, _characterData, CharacterPreviewModule, CharacterGoalsModule, HeroClassStatsModule, CharacterBiographyModule, PersonalQuestModule, isTempCharacter: true, showName: false);
		CharacterRenameModule.Show(_characterName, 18, OnNameChanged, OnNameSubmit);
		HandCardListModule.Init(new CardListModuleBase.Params
		{
			Cards = _characterData.Cards
		});
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnNameChanged(string newName)
	{
		if (!(_characterState.Name == newName))
		{
			_characterName = newName;
			OnShow();
		}
	}

	private void OnNameSubmit(string value)
	{
		OnNameChanged(value);
		if (Controller.IsCharacterNameValid(_characterName))
		{
			OnContinue();
		}
	}

	private void OnBack()
	{
		if (_creationProcess)
		{
			Controller.CancelCreateNewCharacter(_characterState.ClassDef);
		}
		Controller.PopState();
	}

	private void OnContinue()
	{
		if (!string.Equals(_characterState.Name, _characterName, StringComparison.Ordinal))
		{
			_characterState.Name = _characterName;
		}
		List<PersonalQuestID> drawnPersonalQuests = Controller.GetPlayer(_characterState).DrawnPersonalQuests;
		if (_characterState.PersonalQuestDef != null && !drawnPersonalQuests.Contains(_characterState.PersonalQuestDef.ID))
		{
			drawnPersonalQuests.Insert(0, _characterState.PersonalQuestDef.ID);
		}
		PersonalQuestDef leftQuestDef = ((drawnPersonalQuests.Count > 0) ? Controller.GetQuestDef(drawnPersonalQuests[0]) : null);
		PersonalQuestDef rightQuestDef = ((drawnPersonalQuests.Count > 1) ? Controller.GetQuestDef(drawnPersonalQuests[1]) : null);
		Controller.PushState(new QuestSelectionState(Controller, _characterState, leftQuestDef, rightQuestDef, _creationProcess));
	}
}
