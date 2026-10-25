using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class TownPerksViewState : FHMapViewState
{
	private readonly UIPerkInteractionEvents _perkInteractions = new UIPerkInteractionEvents();

	private string _lasSelected;

	private readonly bool _readOnly;

	private readonly PartyViewService _partyService;

	public override StateCategory Category => StateCategory.Shop;

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

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

	public TownPerksViewState(bool readOnly = false)
	{
		_readOnly = readOnly;
		_perkInteractions.OnClick = HandleOnPerkSelected;
		_perkInteractions.OnDoubleClick = HandleOnPerkPurchaseRequest;
		_partyService = View.GetService<PartyViewService>();
	}

	public override void OnShow()
	{
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		AttackModifiers.Init(PerksViewService.GetActiveTownAttackModifiers(), null, showTactical: false);
		List<PerkDTO> availableTownPerks = PerksViewService.GetAvailableTownPerks();
		PerksInventoryModule.Params obj = new PerksInventoryModule.Params
		{
			Perks = availableTownPerks,
			Interactions = _perkInteractions,
			Marks = PerksViewService.GetTownGuardPerksPoints(),
			Checks = PerksViewService.GetTownGuardPerkChecks(),
			Illustration = UIInfoTools.Instance.GetOutpostPanelIllustration("Perks_TownGuard"),
			CaptionKey = "GUI_TOWNGUARD_PERKS_INVENTORY_CAPTION",
			PerksTooltipKey = "GUI_TOWNGUARD_POINTS_VALUE"
		};
		PerksInventory.Init(obj);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		TrySelectPreviousOrDefault(availableTownPerks);
		QuickAccessBarService.Setup(MainViewMainCategories.Outpost, MainViewSubCategories.Defense);
		_partyService.SetupPartyColumn(PartyColumn);
		HeaderService.SetupHeader(HeaderModule);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true, UIInfoTools.Instance.GetOutpostPanelIllustration("FH_UI_View_Perks_Background"));
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void TrySelectPreviousOrDefault(List<PerkDTO> perks)
	{
		if (perks == null || perks.Count == 0)
		{
			SelectCard(null);
			return;
		}
		int num = perks.FindIndex((PerkDTO dto) => dto.ID == _lasSelected);
		SelectCard(perks[(num != -1) ? num : 0]);
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
			ModifierToAdd = PerksViewService.GetTownGuardAttackModifiers(perk.ModifiersToAdd),
			ModifierToRemove = PerksViewService.GetTownGuardAttackModifiers(perk.ModifiersToRemove),
			OnPurchaseCallback = HandleOnPerkPurchaseRequest,
			UnlockHints = PerksViewService.GetUnlockTownGuardPerksHintsLocalized(perk),
			ReadOnly = (_readOnly || !View.IsPartyLeader),
			CostLocKey = "GUI_TOWNGUARD_PERK_COST",
			HintLocKey = "GUI_TOWNGUARD_DESCRIPTION"
		};
		PerkPurchase.Init(obj);
	}

	private void HandleOnPerkSelected(PerkDTO perkDto)
	{
		SelectCard(perkDto);
	}

	private void HandleOnPerkPurchaseRequest(PerkDTO perkDto)
	{
		if (PerksViewService.CanLearnTownGuardPerk(perkDto))
		{
			PerksViewService.LearnTownGuardPerk(perkDto);
		}
	}
}
