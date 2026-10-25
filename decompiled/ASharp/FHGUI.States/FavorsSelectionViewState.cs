using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class FavorsSelectionViewState : FHMapViewState, BattleBriefingViewService.IBriefingStepParamsProvider
{
	private readonly bool _isFromSummary;

	public override StateCategory Category => StateCategory.Briefing;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private ScenarioLauncherModule ScenarioLauncherModule => View.Modules.WorldMap.ScenarioLauncher;

	[Preserve]
	private BattleFavorModule BattleFavorModule => View.Modules.BriefingModules.BattleFavorModule;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SettingsShortcutsModule SettingsShortcuts => View.Modules.SettingsShortcuts;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private BattleBriefingViewService BattleBriefingService => View.GetService<BattleBriefingViewService>();

	public BattleBriefingViewService.BriefingStepTags StepTag => BattleBriefingViewService.BriefingStepTags.Favor;

	public string TitleLocKey => "GUI_FAVORS";

	public string DescriptionLocKey => "GUI_FAVORS_DESCRIPTION";

	public string[] DescriptionArgs
	{
		get
		{
			ScenarioController scenarioController = View?.GameController?.ScenarioController;
			return new string[1] { scenarioController.GetMaxPotentialFavorPoints().ToString() };
		}
	}

	public FavorsSelectionViewState(bool isFromSummary)
	{
		_isFromSummary = isFromSummary;
	}

	public override void OnShow()
	{
		if (GameState.ActiveScenario == null)
		{
			View.ResetView();
			return;
		}
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		BattleFavorModule.Params initParams = new BattleFavorModule.Params
		{
			Favors = BattleBriefingService.GetFavors(),
			Difficulty = BattleBriefingService.GetFavorDifficulty(),
			MaxPoints = BattleBriefingService.GetMaxPotentialFavorPoints(),
			SlotInteractions = BattleBriefingService.GetFavorSlotInteractions,
			TooltipInitParams = BattleBriefingService.GetFavorSlotTooltipParams
		};
		BattleFavorModule.Init(initParams);
		BattleBriefingService.PopulateCommonModules(CharacterDetailsScreenModule, ScenarioLauncherModule, this, SettingsShortcuts, canContinue: true);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	public IEnumerable<ButtonsBarParam> GetButtonsBarParams(bool canContinue)
	{
		return BattleBriefingService.GetCommonButtonsBarParams(StepTag, _isFromSummary, canGoBack: true, canContinue, TargetLayerType);
	}
}
