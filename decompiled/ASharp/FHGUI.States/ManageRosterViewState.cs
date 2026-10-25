using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ManageRosterViewState : FHMapViewState
{
	private readonly PartyViewService _partyService;

	private readonly RosterViewService _rosterService;

	private readonly HeaderViewService _headerService;

	private readonly QuickAccessBarViewService _quickAccessBarViewService;

	private readonly bool _showRetired;

	public override StateCategory Category => StateCategory.Roster;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private RosterManagementModule RosterModule => View.Modules.Characters.Roster;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private IEnumerable<UIPartySlotHints.HintedActionParams> SlotActions(PartySlotDTO characterSlot)
	{
		return _rosterService.GetInteractions(characterSlot);
	}

	private UIPartySlotHints.HintedActionParams DeleteAction(PartySlotDTO characterSlot)
	{
		return _rosterService.GetDeleteInteraction(characterSlot);
	}

	private UIPartySlotHints.HintedActionParams LevelUpAction(PartySlotDTO characterSlot)
	{
		return _rosterService.GetLevelUPInteraction(characterSlot);
	}

	public ManageRosterViewState(bool showRetired = false)
	{
		_showRetired = showRetired;
		_partyService = View.GetService<PartyViewService>();
		_rosterService = View.GetService<RosterViewService>();
		_headerService = View.GetService<HeaderViewService>();
		_quickAccessBarViewService = View.GetService<QuickAccessBarViewService>();
	}

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		HeaderViewService.HeaderFunctionality allowed = (View.PlayerCharacters.Any() ? HeaderViewService.HeaderFunctionality.All : HeaderViewService.HeaderFunctionality.None);
		_headerService.SetupHeader(HeaderModule, allowed);
		_partyService.SetupPartyColumn(PartyModule);
		bool addEmptySlot = !_showRetired && _rosterService.HasAuthorityToCreateNewCharacter();
		RosterManagementModule.RosterParams rosterParams = new RosterManagementModule.RosterParams
		{
			CharacterData = _rosterService.GetActiveRosterSlots(!_showRetired, _showRetired, addEmptySlot),
			HintedActions = SlotActions,
			DeleteAction = DeleteAction,
			LevelUpAction = LevelUpAction,
			FillEmptySlots = true
		};
		RosterModule.Init(rosterParams);
		CharacterDetailsScreenModule.ClearAll();
		_quickAccessBarViewService.Setup(MainViewMainCategories.Roster, _showRetired ? MainViewSubCategories.RetiredCharacters : MainViewSubCategories.ActiveCharacters, UIBuildingStateFilter.None, null, View.SelectedCharacter != null, new MainViewSubCategories[1] { _showRetired ? MainViewSubCategories.ActiveCharacters : MainViewSubCategories.RetiredCharacters });
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
	}

	public override void OnHide()
	{
		View.Modals.GenericInfo.Hide();
	}
}
