using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ProloguePartyCreationViewState : FHMapViewState
{
	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly ScenarioViewService _scenarioService;

	private readonly ScenarioDef _initialScenarioDef;

	public override StateCategory Category => StateCategory.Roster;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private PartyManagementModule PartyManagementModule => View.Modules.Characters.PartyManagementSelector;

	[Preserve]
	private SimpleHeaderModule Header => View.Modules.Characters.Header;

	[Preserve]
	private PartyViewService PartyService => View.GetService<PartyViewService>();

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	private IEnumerable<UIPartySlotHints.HintedActionParams> SlotInteractions(PartySlotDTO slot)
	{
		return PartyService.GetSlotInteractions(slot, creationPhase: true);
	}

	public ProloguePartyCreationViewState()
	{
		_initialScenarioDef = (from s in GameController.ScenarioController.GetUnlockedScenarios()
			orderby s.ScenarioID
			select s).First().Def;
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
		_scenarioService = View.GetService<ScenarioViewService>();
	}

	public override void OnShow()
	{
		bool num = GameState.SelectedScenario != null && !GameState.ActiveScenario.HasEnteredScenario;
		Header.SetFunctionality(CharacterDetailsScreenModule.Functionality.ImportParty);
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		UIPartySlotInteractionEvents partySlotInteractions = GetPartySlotInteractions();
		CharacterDetailsScreenModule.ClearAll();
		PartyManagementModule.PartyManagementParams partyManagementParams = new PartyManagementModule.PartyManagementParams
		{
			slotData = PartyService.GetProloguePartySlotsData(),
			HintedActions = SlotInteractions,
			SlotInteractionEvents = partySlotInteractions,
			isOffset = false
		};
		PartyManagementModule.Init(partyManagementParams);
		if (num)
		{
			return;
		}
		_scenarioService.PrepareScenario(_initialScenarioDef, out var continueParams);
		if (!View.Choreographer.AutoStartFirstScenario)
		{
			continueParams.Callback = () =>
			{
				View.ResetView();
			};
			continueParams.CallbackWithData = null;
		}
		continueParams.HintKey = (continueParams.NonInteractable ? _scenarioService.GetProceedWarning().FirstOrDefault() : null);
		CharacterDetailsScreenModule.ShowProceedButton(continueParams);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.ImportParty);
		_characterDetailsService.SetFunctionality(View.SelectedCharacter, CharacterDetailsScreenModule.Functionality.ImportParty);
		View.OutpostSceneController.OpenPartyScreen(new FHPartyViewScene.PartySceneParams
		{
			SlotData = partyManagementParams.slotData
		});
	}

	private UIPartySlotInteractionEvents GetPartySlotInteractions()
	{
		return new UIPartySlotInteractionEvents
		{
			OnHover = OnPartySlotHover
		};
	}

	private void OnPartySlotHover(PartySlotDTO target, bool isFlagRaised)
	{
		View.OutpostSceneController.PartyScene.HoverSlot(target.SlotId, isFlagRaised);
	}

	public override void OnHide()
	{
		PartyManagementModule.ClearEvents();
		CharacterDetailsScreenModule.ClearAll();
	}
}
