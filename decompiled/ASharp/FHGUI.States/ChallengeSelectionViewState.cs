using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ChallengeSelectionViewState : FHMapViewState, BattleBriefingViewService.IBriefingStepParamsProvider
{
	private readonly UISuggestibleSlotInteractionEvents _interactionEvents;

	private readonly bool _isFromSummary;

	public override StateCategory Category => StateCategory.Briefing;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private ScenarioLauncherModule ScenarioLauncherModule => View.Modules.WorldMap.ScenarioLauncher;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private UIScenarioBriefingSuggestibleList ChallengesList => View.Modules.BriefingModules.ChallengesList;

	[Preserve]
	private SettingsShortcutsModule SettingsShortcuts => View.Modules.SettingsShortcuts;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private BattleBriefingViewService BattleBriefingService => View.GetService<BattleBriefingViewService>();

	public BattleBriefingViewService.BriefingStepTags StepTag => BattleBriefingViewService.BriefingStepTags.Challenge;

	public string TitleLocKey => "GUI_CHALLENGES";

	public string DescriptionLocKey => "GUI_CHALLENGES_DESCRIPTION";

	public string[] DescriptionArgs => null;

	public ChallengeSelectionViewState(bool isFromSummary)
	{
		_isFromSummary = isFromSummary;
		_interactionEvents = new UISuggestibleSlotInteractionEvents
		{
			OnClick = OnChallengeClicked
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
		UIScenarioBriefingSuggestibleList.ListParams listParams = new UIScenarioBriefingSuggestibleList.ListParams
		{
			InteractionEvents = _interactionEvents,
			tooltipInitParams = BattleBriefingService.GetChallengeSlotTooltipParams,
			CurrentCurrent = BattleBriefingService.GetCurrentChallengePicks(),
			MaxCharges = BattleBriefingService.GetTotalChallengePicks()
		};
		ChallengesList.Init(listParams, BattleBriefingService.GetChallenges());
		BattleBriefingService.PopulateCommonModules(CharacterDetailsScreenModule, ScenarioLauncherModule, this, SettingsShortcuts, canContinue: true);
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnChallengeClicked(SuggestibleSlotDTO target)
	{
		if (target.Id.IsNullOrEmpty())
		{
			return;
		}
		ChallengeID challengeId = new ChallengeID(uint.Parse(target.Id));
		if (View.IsPartyLeader)
		{
			if (BattleBriefingService.IsChallengeSelected(challengeId))
			{
				BattleBriefingService.DeselectChallenge(challengeId);
			}
			else
			{
				BattleBriefingService.SelectChallenge(challengeId);
			}
		}
		else if (BattleBriefingService.IsChallengeSuggested(challengeId, byPartyLeader: false))
		{
			BattleBriefingService.DeSuggestChallenge(challengeId);
		}
		else
		{
			BattleBriefingService.SuggestChallenge(challengeId);
		}
		OnShow();
	}

	public IEnumerable<ButtonsBarParam> GetButtonsBarParams(bool canContinue)
	{
		return BattleBriefingService.GetCommonButtonsBarParams(StepTag, _isFromSummary, canGoBack: true, canContinue, TargetLayerType);
	}
}
