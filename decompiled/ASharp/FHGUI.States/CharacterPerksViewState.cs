using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class CharacterPerksViewState : FHMapViewState
{
	private readonly UIPerkInteractionEvents _perkInteractions = new UIPerkInteractionEvents();

	private CharacterDataDTO _overrideCharacterDto;

	private CharacterDataDTO _characterDto;

	private string _lasSelected;

	private readonly bool _readOnly;

	private readonly PartyViewService _partyService;

	private readonly CharacterDetailsViewService _characterDetailsService;

	public override StateCategory Category => StateCategory.Characters;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private SimpleHeaderModule SimpleHeader => View.Modules.Characters.Header;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyColumn => View.Modules.Characters.PartyColumn;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private AttackModifiersModule AttackModifiers => View.Modules.Characters.AttackModifiersModule;

	[Preserve]
	private PerksInventoryModule PerksInventory => View.Modules.Characters.PerksInventoryModule;

	[Preserve]
	private PerkPurchaseModule PerkPurchase => View.Modules.Characters.PerkPurchaseModule;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PerksViewService PerksViewService => View.GetService<PerksViewService>();

	public CharacterPerksViewState(CharacterState overrideState, bool readOnly = false)
		: this(readOnly)
	{
		_overrideCharacterDto = _characterDetailsService.PopulateCharacterDTO(overrideState);
	}

	public CharacterPerksViewState(bool readOnly = false)
	{
		_readOnly = readOnly;
		_perkInteractions.OnClick = HandleOnPerkSelected;
		_perkInteractions.OnDoubleClick = HandleOnPerkPurchaseRequest;
		_partyService = View.GetService<PartyViewService>();
		_characterDetailsService = View.GetService<CharacterDetailsViewService>();
	}

	public override void OnShow()
	{
		if (!View.GetService<PartyViewService>().HasValidCharacterSelection(this))
		{
			View.ResetView();
			return;
		}
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		CharacterState characterState = View.SelectedCharacter;
		if (_overrideCharacterDto != null && View.GameController.CharacterController.TryGetCharacter(_overrideCharacterDto.CharacterID, out var character))
		{
			characterState = character;
		}
		_characterDto = _overrideCharacterDto ?? View.GetService<CharacterDetailsViewService>().PopulateCharacterDTO(View.SelectedCharacter);
		bool flag = View.GameState.ActiveScenario != null;
		bool flag2 = HeaderService.SetupCharacterDetailsHeader(HeaderModule, SimpleHeader, "Perks");
		AttackModifiers.Init(PerksViewService.GetActiveAttackModifiers(_characterDto), PerksViewService.GetTacticalPerks(_characterDto));
		List<PerkDTO> availablePerks = PerksViewService.GetAvailablePerks(_characterDto);
		PerksInventoryModule.Params obj = new PerksInventoryModule.Params
		{
			Perks = availablePerks,
			Interactions = _perkInteractions,
			Marks = characterState.PerkMarks,
			Checks = characterState.PerkChecks,
			CaptionKey = "GUI_PERKS_INVENTORY_CAPTION",
			PerksTooltipKey = "GUI_PERKS_POINTS_VALUE"
		};
		PerksInventory.Init(obj);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		TrySelectPreviousOrDefault(availablePerks);
		if (flag)
		{
			PartyColumn.gameObject.SetActive(value: false);
			QuickAccessBarService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.Perks);
			CharacterDetailsScreenModule.ShowCloseButton(PopThisState);
		}
		else if (flag2)
		{
			QuickAccessBar.gameObject.SetActive(value: false);
			PartyColumn.gameObject.SetActive(value: false);
			CharacterDetailsScreenModule.SetButtonsBar(CharacterDetailsScreenModule.Button_Back(PopThisState));
			HeaderModule.gameObject.SetActive(value: false);
		}
		else
		{
			QuickAccessBarService.Setup(MainViewMainCategories.Characters, MainViewSubCategories.Perks);
			_partyService.SetupPartyColumn(PartyColumn);
			HeaderService.SetupHeader(HeaderModule);
		}
		Background.SetEnabled(isEnabled: true, UIInfoTools.Instance.GetOutpostPanelIllustration("FH_UI_View_Perks_Background"));
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void TrySelectPreviousOrDefault(List<PerkDTO> perks)
	{
		if (perks != null && perks.Count != 0)
		{
			int num = perks.FindIndex((PerkDTO dto) => dto.ID == _lasSelected);
			SelectCard(perks[(num != -1) ? num : 0]);
		}
	}

	private void SelectCard(PerkDTO perk)
	{
		if (perk == null)
		{
			_lasSelected = null;
			PerkPurchase.Init(default);
			return;
		}
		_lasSelected = perk.ID;
		PerkPurchaseModule.Params obj = new PerkPurchaseModule.Params
		{
			PerkDto = perk,
			ModifierToAdd = PerksViewService.GetPerkAttackModifiers(perk.ModifiersToAdd),
			ModifierToRemove = PerksViewService.GetPerkAttackModifiers(perk.ModifiersToRemove),
			OnPurchaseCallback = HandleOnPerkPurchaseRequest,
			UnlockHints = PerksViewService.GetUnlockHeroPerksHintsLocalized(View.SelectedCharacter, perk),
			ReadOnly = _readOnly,
			CostLocKey = "GUI_PERK_COST",
			HintLocKey = "GUI_PERKS_DESCRIPTION"
		};
		PerkPurchase.Init(obj);
	}

	private void HandleOnPerkSelected(PerkDTO perkDto)
	{
		SelectCard(perkDto);
	}

	private void HandleOnPerkPurchaseRequest(PerkDTO perkDto)
	{
		if (PerksViewService.CanLearnPerk(_characterDto, perkDto))
		{
			PerksViewService.LearnCharacterPerk(_characterDto, perkDto);
		}
	}
}
