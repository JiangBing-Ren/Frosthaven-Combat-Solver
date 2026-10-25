using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;

namespace FHGUI.Testing;

public class QuestSelectionState : TestState
{
	private readonly CharacterState _characterState;

	private readonly PersonalQuestDef _leftQuestDef;

	private readonly PersonalQuestDef _rightQuestDef;

	private readonly bool _creationProcess;

	private readonly CharacterDataDTO _characterData;

	private CharacterDetailsScreenModule CharacterDetailsScreenModule => Controller.GetModule<CharacterDetailsScreenModule>();

	private CharacterPreviewModule CharacterPreviewModule => Controller.GetModule<CharacterPreviewModule>();

	private HeroClassStatsModule HeroClassStatsModule => Controller.GetModule<HeroClassStatsModule>();

	private CharacterGoalsModule CharacterGoalsModule => Controller.GetModule<CharacterGoalsModule>();

	private CharacterBiographyModule CharacterBiographyModule => Controller.GetModule<CharacterBiographyModule>();

	private HandCardListModule HandCardListModule => Controller.GetModule<HandCardListModule>();

	private CharacterPersonalQuestModule PersonalQuestModule => Controller.GetModule<CharacterPersonalQuestModule>();

	private PersonalQuestSelectionModule PersonalQuestSelectionModule => Controller.GetModule<PersonalQuestSelectionModule>();

	public QuestSelectionState(TestingCharacterCreationFlowViewData controller, CharacterState characterState, PersonalQuestDef leftQuestDef, PersonalQuestDef rightQuestDef, bool creationProcess)
		: base(controller)
	{
		_characterState = characterState;
		_leftQuestDef = leftQuestDef;
		_rightQuestDef = rightQuestDef;
		_creationProcess = creationProcess;
		_characterData = TestingCharacterCreationFlowUtils.BuildCharacterDTO(Controller.DefRepo, _characterState);
	}

	public override void OnShow()
	{
		TestingCharacterCreationFlowUtils.PopulateCharacterDetails(Controller.DefRepo, _characterState, CharacterPreviewModule, CharacterGoalsModule, HeroClassStatsModule, CharacterBiographyModule, PersonalQuestModule);
		bool flag = _characterState.PersonalQuestDef != null;
		List<ButtonsBarParam> list = new List<ButtonsBarParam> { CharacterDetailsScreenModule.Button_Back(OnBack) };
		if (_creationProcess)
		{
			list.Add(CharacterDetailsScreenModule.Button_Continue(OnContinue, flag));
		}
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(list.ToArray());
		CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(), 1);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.PersonalQuest);
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint("GUI_SELECT_QUEST");
		}
		CharacterDataDTO characterData = TestingCharacterCreationFlowUtils.BuildCharacterDTO(Controller.DefRepo, _characterState);
		CharacterPreviewModule.Init(characterData, null, showValues: false);
		HandCardListModule.Init(new CardListModuleBase.Params
		{
			Cards = _characterData.Cards
		});
		UpdateSelectionModule();
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void UpdateSelectionModule()
	{
		PersonalQuestSelectionModule.Init(_characterState.PersonalQuestDef, new CharacterPersonalQuestDTO(_leftQuestDef), new CharacterPersonalQuestDTO(_rightQuestDef), OnQuestSelected, OnQuestVisibilityChanged);
	}

	private void OnQuestVisibilityChanged(bool isOn)
	{
	}

	private void OnQuestSelected(PersonalQuestDef def)
	{
		if (def != null)
		{
			_characterState.PersonalQuest = new PersonalQuestState(def.ID, Controller.DefRepo, null);
		}
		else
		{
			_characterState.PersonalQuest = null;
		}
		Controller.RefreshView();
	}

	private void OnBack()
	{
		Controller.PopState();
	}

	private void OnContinue()
	{
		Controller.PushState(new StartingGearState(Controller, _characterState));
	}
}
