using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class BattleGoalSelectionViewState : FHMapViewState, BattleBriefingViewService.IBriefingStepParamsProvider
{
	private readonly bool _isFromSummary;

	private bool _hasBattleGoals;

	public override StateCategory Category => StateCategory.Briefing;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	[Preserve]
	private ScenarioBriefingPartyModule ScenarioBriefingPartyModule => View.Modules.Characters.ScenarioBriefingParty;

	[Preserve]
	private ScenarioLauncherModule ScenarioLauncherModule => View.Modules.WorldMap.ScenarioLauncher;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	[Preserve]
	private SettingsShortcutsModule SettingsShortcuts => View.Modules.SettingsShortcuts;

	[Preserve]
	private SimpleBackgroundModule Background => View.Modules.Background;

	private BattleBriefingViewService BattleBriefingService => View.GetService<BattleBriefingViewService>();

	private PartyViewService PartyService => View.GetService<PartyViewService>();

	public BattleBriefingViewService.BriefingStepTags StepTag => BattleBriefingViewService.BriefingStepTags.BattleGoals;

	public string TitleLocKey
	{
		get
		{
			if (!_hasBattleGoals)
			{
				return string.Empty;
			}
			return "GUI_BATTLE_GOALS";
		}
	}

	public string DescriptionLocKey
	{
		get
		{
			if (!_hasBattleGoals)
			{
				return string.Empty;
			}
			return "GUI_BATTLE_GOALS_DESCRIPTION";
		}
	}

	public string[] DescriptionArgs => null;

	public BattleGoalSelectionViewState(bool isFromSummary)
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
		BattleBriefingService.StartScenarioBriefing();
		List<ScenarioBriefingPartyModule.BriefingSlotData> briefingSlotDatas = BattleBriefingService.GetBriefingSlotDatas();
		_hasBattleGoals = BattleBriefingService.HasBattleGoals();
		bool isFinalStep = !BattleBriefingService.HasMultipleSteps();
		bool flag = CanContinue(briefingSlotDatas, isFinalStep);
		ScenarioBriefingPartyModule.ListParams listParams = new ScenarioBriefingPartyModule.ListParams
		{
			ShowGoalsList = true,
			SlotHintedActions = BattleBriefingService.GetPartySlotInteractions,
			OnGoalListElementClicked = OnBattleGoalSelected
		};
		ScenarioBriefingPartyModule.Init(listParams, briefingSlotDatas);
		BattleBriefingService.PopulateCommonModules(CharacterDetailsScreenModule, ScenarioLauncherModule, this, SettingsShortcuts, flag);
		if (!flag)
		{
			CharacterDetailsScreenModule.ShowProceedHint(BattleBriefingService.GetAdvanceHints(isFinalStep).FirstOrDefault());
		}
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
		View.OutpostSceneController.OpenDefault();
		Background.SetEnabled(isEnabled: true);
		UIContextualTutorialModule.OnEvent("QuestBriefingShown");
	}

	public override void OnHide()
	{
		CharacterDetailsScreenModule.ClearAll();
	}

	private bool CanContinue(List<ScenarioBriefingPartyModule.BriefingSlotData> briefingSlotDatas, bool isFinalStep)
	{
		if (isFinalStep)
		{
			return View.GameController.ScenarioController.CanEnterActiveScenario();
		}
		if (!briefingSlotDatas.All((ScenarioBriefingPartyModule.BriefingSlotData bsd) => !bsd.PartySlotDto.Owned || bsd.PartySlotDto.Character == ECharacter.None || bsd.SelectedGoalDto != null))
		{
			return false;
		}
		if (!View.PlayerCharacters.All((CharacterState slot) => slot == null || slot.ClassDef.MaxCards == slot.SelectedCardIDs.Count))
		{
			return false;
		}
		return true;
	}

	private void OnBattleGoalSelected(PartySlotDTO partySlotDto, GoalDTO goalDto)
	{
		if (!PartyService.TryGetCharacter(partySlotDto, out var characterState, out var isSlotOwner) || !isSlotOwner)
		{
			Log.GUIError("Trying to assign a battle goal to a missing or unowned character!");
			return;
		}
		if (BattleBriefingService.IsPlayerVotedReady())
		{
			Log.GUITrace("Battle Goal Selection is disabled for player marked as ready.");
			return;
		}
		BattleGoalID goalId = new BattleGoalID(goalDto.Id);
		BattleGoalDef battleGoalDef = View.GameController.ScenarioController.GetCharacterBattleGoals(characterState).First((BattleGoalDef def) => def.ID == goalId);
		View.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.SelectCharacterBattleGoal, characterState.ID, battleGoalDef);
	}

	private void ShowAbandonModal()
	{
		View.PushState(new GenericModalViewState(LocalizationManager.GetTranslation("GUI_ABANDON_CONFIRM"), LocalizationManager.GetTranslation("GUI_ABANDON_QUEST"), (bool confirmed) =>
		{
			if (confirmed)
			{
				BattleBriefingService.AbandonScenario();
			}
			View.PopState(FHMapViewLayer.LayerType.Modal);
		}), FHMapViewLayer.LayerType.Modal);
	}

	public IEnumerable<ButtonsBarParam> GetButtonsBarParams(bool canContinue)
	{
		if (_isFromSummary)
		{
			yield return BattleBriefingService.GetReturnToSummaryParam(TargetLayerType);
			yield break;
		}
		FHRL.ScenarioState selectedScenario = View.GameState.SelectedScenario;
		if (selectedScenario != null && !selectedScenario.Mandatory)
		{
			bool flag = (View.IsOnline && !View.IsPartyLeader) || ((object)SaveData.Instance != null && SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial) || SaveData.Instance?.Global?.CurrentAdventureData?.FrosthavenCampaign?.IsInPrologue == true || View.IsReadyCheckRunning || View.GameState.ActiveScenario.HasEnteredScenario || View.GameState.ActiveScenario.Def.CanNotReturnToOutpost || View.GetService<ScenarioViewService>().HasPlayerJoining;
			yield return new ButtonsBarParam
			{
				LabelKey = "GUI_ABANDON_QUEST",
				Callback = ShowAbandonModal,
				Interactable = !flag,
				Prototype = CharacterDetailsScreenModule._backButton
			};
		}
		if (BattleBriefingService.HasMultipleSteps())
		{
			yield return BattleBriefingService.GetCommonContinueParam(StepTag, canContinue);
			yield break;
		}
		BattleBriefingService.HandleBriefingVoting(out var continueParams);
		yield return continueParams;
	}
}
