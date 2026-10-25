using FHGUI.Elements;
using FHGUI.Modules;
using FHRL;

namespace FHGUI.Testing;

public class PartyCreationState : TestState
{
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => Controller.GetModule<CharacterDetailsScreenModule>();

	private PartyManagementModule PartyManagementModule => Controller.GetModule<PartyManagementModule>();

	public PartyCreationState(TestingCharacterCreationFlowViewData controller)
		: base(controller)
	{
	}

	public override void OnShow()
	{
		CharacterDetailsScreenModule.ClearAll();
		PartyManagementModule.PartyManagementParams partyParams = new PartyManagementModule.PartyManagementParams
		{
			slotData = Controller.GetPartySlotDTOs(),
			HintedActions = null,
			isOffset = false
		};
		PartyManagementModule.Init(partyParams);
		ButtonsBarParam buttonParams = new ButtonsBarParam
		{
			LabelKey = "GUI_START",
			Callback = () =>
			{
				OnStartScenario();
			},
			Interactable = false
		};
		buttonParams.HintKey = (buttonParams.NonInteractable ? "GUI_PARTY_CREATE_TWO_CHARACTERS" : null);
		CharacterDetailsScreenModule.ShowProceedButton(buttonParams);
	}

	private void OnStartScenario()
	{
	}

	private void OnSlotSelect(UIPartySlot slot)
	{
		CharacterState characterAtSlot = Controller.GetCharacterAtSlot(slot.SlotId);
		if (characterAtSlot != null)
		{
			Controller.PushState(new StartingGearState(Controller, characterAtSlot));
		}
		else
		{
			Controller.PushState(new NewCharacterState(Controller, slot.SlotId));
		}
	}

	private void OnRemoveCharacter(UIPartySlot slot)
	{
		Controller.RemoveCharacterAtSlot(slot.SlotId);
		Controller.RefreshView();
	}

	public override void OnHide()
	{
		PartyManagementModule.ClearEvents();
		CharacterDetailsScreenModule.ClearAll();
	}
}
