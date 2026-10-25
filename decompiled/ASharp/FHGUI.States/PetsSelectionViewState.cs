using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class PetsSelectionViewState : FHMapViewState, BattleBriefingViewService.IBriefingStepParamsProvider
{
	private readonly bool _isFromSummary;

	private readonly UISuggestibleSlotInteractionEvents _interactionEvents;

	public override StateCategory Category => StateCategory.Briefing;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private ScenarioLauncherModule ScenarioLauncherModule => View.Modules.WorldMap.ScenarioLauncher;

	[Preserve]
	private UIScenarioBriefingSuggestibleList PetsList => View.Modules.BriefingModules.PetsList;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SettingsShortcutsModule SettingsShortcuts => View.Modules.SettingsShortcuts;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private BattleBriefingViewService BattleBriefingService => View.GetService<BattleBriefingViewService>();

	public BattleBriefingViewService.BriefingStepTags StepTag => BattleBriefingViewService.BriefingStepTags.Pets;

	public string TitleLocKey => "GUI_PETS";

	public string DescriptionLocKey => "GUI_PETS_DESCRIPTION";

	public string[] DescriptionArgs => null;

	public PetsSelectionViewState(bool isFromSummary)
	{
		_isFromSummary = isFromSummary;
		_interactionEvents = new UISuggestibleSlotInteractionEvents
		{
			OnClick = OnPetClicked
		};
	}

	public override void OnShow()
	{
		if (GameState.ActiveScenario == null)
		{
			View.ResetView();
			return;
		}
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		BattleBriefingService.PopulateCommonModules(CharacterDetailsScreenModule, ScenarioLauncherModule, this, SettingsShortcuts, canContinue: true);
		UIScenarioBriefingSuggestibleList.ListParams listParams = new UIScenarioBriefingSuggestibleList.ListParams
		{
			InteractionEvents = _interactionEvents,
			tooltipInitParams = BattleBriefingService.GetPetSlotTooltipParams,
			CurrentCurrent = BattleBriefingService.GetCurrentPetPicks(),
			MaxCharges = BattleBriefingService.GetTotalPetPicks()
		};
		PetsList.Init(listParams, BattleBriefingService.GetPets());
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnPetClicked(SuggestibleSlotDTO target)
	{
		if (target.Id.IsNullOrEmpty())
		{
			return;
		}
		PetID petID = new PetID(uint.Parse(target.Id));
		if (View.IsPartyLeader)
		{
			if (BattleBriefingService.IsPetPicked(petID))
			{
				BattleBriefingService.RemovePet(petID);
			}
			else
			{
				BattleBriefingService.PickPet(petID);
			}
		}
		else if (BattleBriefingService.IsPetSuggested(petID, byPartyLeader: false))
		{
			BattleBriefingService.DeSuggestPet(petID);
		}
		else
		{
			BattleBriefingService.SuggestPet(petID);
		}
		OnShow();
	}

	public IEnumerable<ButtonsBarParam> GetButtonsBarParams(bool canContinue)
	{
		return BattleBriefingService.GetCommonButtonsBarParams(StepTag, _isFromSummary, canGoBack: true, canContinue, TargetLayerType);
	}
}
