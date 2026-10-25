using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using GLOOM;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ScenarioBriefingSummaryViewState : FHMapViewState, BattleBriefingViewService.IBriefingStepParamsProvider
{
	public override StateCategory Category => StateCategory.Briefing;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private ScenarioBriefingPartyModule ScenarioBriefingPartyModule => View.Modules.Characters.ScenarioBriefingParty;

	[Preserve]
	private ScenarioLauncherModule ScenarioLauncherModule => View.Modules.WorldMap.ScenarioLauncher;

	[Preserve]
	private UIScenarioBriefingSummeryList ChallengesSummery => View.Modules.BriefingModules.ChallengesSummery;

	[Preserve]
	private UIScenarioBriefingSummeryList PetsSummery => View.Modules.BriefingModules.PetsSummery;

	[Preserve]
	private UIScenarioBriefingSummeryList FavorsSummery => View.Modules.BriefingModules.FavorsSummery;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SettingsShortcutsModule SettingsShortcuts => View.Modules.SettingsShortcuts;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private BattleBriefingViewService BattleBriefingService => View.GetService<BattleBriefingViewService>();

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	public BattleBriefingViewService.BriefingStepTags StepTag => BattleBriefingViewService.BriefingStepTags.Summary;

	public string TitleLocKey => "GUI_SUMMARY";

	public string DescriptionLocKey => "GUI_SUMMARY_DESCRIPTION";

	public string[] DescriptionArgs => null;

	public override void OnShow()
	{
		if (GameState.ActiveScenario == null)
		{
			View.ResetView();
			return;
		}
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		BattleBriefingViewService battleBriefingService = BattleBriefingService;
		bool flag = battleBriefingService.HasChallengesEnabled();
		bool flag2 = battleBriefingService.HasPetsEnabled();
		bool flag3 = battleBriefingService.HasFavorsEnabled();
		ScenarioBriefingPartyModule.ListParams listParams = new ScenarioBriefingPartyModule.ListParams
		{
			ShowGoalsList = false,
			ShowSelectedGoal = true,
			SlotHintedActions = battleBriefingService.GetPartySlotInteractions,
			OnSelectedGoalClicked = OnSelectedGoalClicked
		};
		List<ScenarioBriefingPartyModule.BriefingSlotData> briefingSlotDatas = battleBriefingService.GetBriefingSlotDatas();
		ScenarioBriefingPartyModule.Init(listParams, briefingSlotDatas);
		battleBriefingService.PopulateCommonModules(CharacterDetailsScreenModule, ScenarioLauncherModule, this, SettingsShortcuts, canContinue: true);
		ChallengesSummery.gameObject.SetActive(flag);
		if (flag)
		{
			UIScenarioBriefingSummeryList.ListParams listParams2 = new UIScenarioBriefingSummeryList.ListParams
			{
				CaptionLiteral = LocalizationManager.GetTranslation("GUI_CHALLENGES"),
				Interactions = GetSummeryInteractions(BattleBriefingViewService.BriefingStepTags.Challenge),
				tooltipInitParams = BattleBriefingService.GetChallengeSlotTooltipParams
			};
			List<SuggestibleSlotDTO> challenges = battleBriefingService.GetChallenges(onlySelected: true);
			challenges.ForEach((SuggestibleSlotDTO dto) =>
			{
				dto.SuppressInteraction = true;
			});
			ChallengesSummery.Init(listParams2, challenges);
		}
		PetsSummery.gameObject.SetActive(flag2);
		if (flag2)
		{
			UIScenarioBriefingSummeryList.ListParams listParams3 = new UIScenarioBriefingSummeryList.ListParams
			{
				CaptionLiteral = LocalizationManager.GetTranslation("GUI_PETS"),
				Interactions = GetSummeryInteractions(BattleBriefingViewService.BriefingStepTags.Pets),
				tooltipInitParams = BattleBriefingService.GetPetSlotTooltipParams
			};
			List<SuggestibleSlotDTO> pets = battleBriefingService.GetPets(onlySelected: true);
			pets.ForEach((SuggestibleSlotDTO dto) =>
			{
				dto.SuppressInteraction = true;
			});
			PetsSummery.Init(listParams3, pets);
		}
		FavorsSummery.gameObject.SetActive(flag3);
		if (flag3)
		{
			UIScenarioBriefingSummeryList.ListParams listParams4 = new UIScenarioBriefingSummeryList.ListParams
			{
				CaptionLiteral = LocalizationManager.GetTranslation("GUI_FAVORS"),
				Interactions = GetSummeryInteractions(BattleBriefingViewService.BriefingStepTags.Favor),
				tooltipInitParams = BattleBriefingService.GetFavorSlotTooltipParams
			};
			List<SuggestibleSlotDTO> favors = battleBriefingService.GetFavors(onlySelected: true);
			favors.ForEach((SuggestibleSlotDTO dto) =>
			{
				dto.SuppressInteraction = true;
			});
			FavorsSummery.Init(listParams4, favors);
		}
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		CharacterDetailsScreenModule.ShowProceedHint(BattleBriefingService.GetAdvanceHints(isFinalStep: true).FirstOrDefault());
		Background.SetEnabled(isEnabled: true);
	}

	private IEnumerable<FHInputEventContainer.Params> GetSummeryInteractions(BattleBriefingViewService.BriefingStepTags stepTag)
	{
		yield return new FHInputEventContainer.Params
		{
			ActionIcon = UIInfoTools.Instance.GetInputActionConfig(FHInputEventElement.UserInteractionType.LeftClick)?.Icon,
			HintKey = "GUI_EDIT",
			InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
			ActionCallback = () =>
			{
				BattleBriefingService.GoToBriefingStepState(stepTag, isFromSummery: true);
			}
		};
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private void OnSelectedGoalClicked(PartySlotDTO partySlotDto, GoalDTO goalDto)
	{
		if (!PartyService.TryGetCharacter(partySlotDto, out var _, out var isSlotOwner) || !isSlotOwner)
		{
			Log.GUIError("Trying to assign a battle goal to a missing or unowned character!");
		}
		else if (!BattleBriefingService.IsPlayerVotedReady())
		{
			View.PushState(new BattleGoalSelectionViewState(isFromSummary: true));
		}
	}

	public IEnumerable<ButtonsBarParam> GetButtonsBarParams(bool _)
	{
		yield return BattleBriefingService.GetCommonBackParam(isInteractable: true, TargetLayerType);
		BattleBriefingService.HandleBriefingVoting(out var continueParams);
		yield return continueParams;
	}
}
