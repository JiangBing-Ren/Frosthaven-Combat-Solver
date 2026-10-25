using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;

namespace FHGUI.Testing;

public class NewCharacterState : TestState
{
	private readonly CharacterDataDTO _characterData;

	private int _slotId;

	private CharacterDetailsScreenModule CharacterDetailsScreenModule => Controller.GetModule<CharacterDetailsScreenModule>();

	private CharacterPreviewModule CharacterPreviewModule => Controller.GetModule<CharacterPreviewModule>();

	private CharacterGoalsModule CharacterGoalsModule => Controller.GetModule<CharacterGoalsModule>();

	private CharactersSelectorModule CharactersSelectorModule => Controller.GetModule<CharactersSelectorModule>();

	private HeroClassStatsModule HeroClassStatsModule => Controller.GetModule<HeroClassStatsModule>();

	private CharacterBiographyModule CharacterBiographyModule => Controller.GetModule<CharacterBiographyModule>();

	private CharacterPersonalQuestModule CharacterPersonalQuestModule => Controller.GetModule<CharacterPersonalQuestModule>();

	private HandCardListModule HandCardListModule => Controller.GetModule<HandCardListModule>();

	public NewCharacterState(TestingCharacterCreationFlowViewData controller, int slotId)
		: base(controller)
	{
		_slotId = slotId;
		_characterData = new CharacterDataDTO
		{
			SlotId = slotId
		};
		CharacterClassDef characterClassDef = Controller.UnlockedClasses.FirstOrDefault((CharacterClassDef def) => Controller.CanCreateCharacterFromClass(def));
		OnClassSelected(characterClassDef, callOnShow: false);
	}

	public override void OnShow()
	{
		bool flag = _characterData.Class != null && Controller.CanCreateCharacterFromClass(_characterData.Class);
		ButtonsBarParam[] buttonsBar = new ButtonsBarParam[2]
		{
			CharacterDetailsScreenModule.Button_Back(OnBack),
			CharacterDetailsScreenModule.Button_Continue(OnContinue, flag)
		};
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetButtonsBar(buttonsBar);
		CharacterDetailsScreenModule.ShowBreadcrumbs(CharacterDetailsViewService.GetCharacterCreationCrumbsParams(), 0);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.ClassSelect);
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint("GUI_ONE_HERO_PER_CLASS");
		}
		TestingCharacterCreationFlowUtils.PopulateCharacterDetails(Controller.DefRepo, _characterData, CharacterPreviewModule, CharacterGoalsModule, HeroClassStatsModule, CharacterBiographyModule, CharacterPersonalQuestModule, isTempCharacter: true);
		HandCardListModule.Init(new CardListModuleBase.Params
		{
			Cards = _characterData.Cards
		});
		List<PartySlotDTO> availableClassesData = TestingCharacterCreationFlowUtils.GetAvailableClassesData(Controller, _characterData.Class);
		CharactersSelectorModule.Init(availableClassesData, OnClassSlotClicked);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnClassSlotClicked(UIPartySlot slot)
	{
		CharacterClassDef characterClassDef = Controller.ECharacterModelToClassDef(slot.CurrentData.Character);
		OnClassSelected(characterClassDef, callOnShow: true);
	}

	private void OnClassSelected(CharacterClassDef characterClassDef, bool callOnShow)
	{
		TestingCharacterCreationFlowUtils.PopulateCharacterDTO(Controller.DefRepo, _characterData, characterClassDef);
		if (callOnShow)
		{
			OnShow();
		}
	}

	private void OnBack()
	{
		if (Controller.TryGetCharacter(_characterData.Class, out var character))
		{
			character.PersonalQuest = null;
		}
		Controller.SetState(new PartyCreationState(Controller));
	}

	private void OnContinue()
	{
		Controller.CreateNewCharacter(_characterData.Class, "Generic Character", _slotId);
		Controller.TryGetCharacter(_characterData.Class, out var character);
		Controller.SetSelectedCharacter(character, _characterData.SlotId);
		Controller.PushState(new RenameCharacterState(Controller, character, creationProcess: true));
	}
}
