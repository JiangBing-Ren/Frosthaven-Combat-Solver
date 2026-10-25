using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Scenario;
using FHGUI.Scenario.Elements;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

internal class ScenarioScriptFrontend_RFH003 : ScenarioScriptFrontend
{
	private class TutorialStep
	{
		public string MessageKey;

		public CLevelUIInteractionProfile.EIsolatedControlType[] ControlsToHide;

		public Action OnTrigger;
	}

	private List<TutorialStep> tutorialSteps;

	private Coroutine curCoroutine;

	private MonoBehaviour coroGO;

	private ScenarioScript_RFH003 _script_rfh003 => (ScenarioScript_RFH003)_script;

	private int _tutStep
	{
		get
		{
			if (_script_rfh003 == null)
			{
				return -1;
			}
			return _script_rfh003._tutStep;
		}
		set
		{
			if (_script_rfh003 != null)
			{
				_script_rfh003._tutStep = value;
			}
		}
	}

	private void StartCoroutine(IEnumerator enumerator)
	{
		coroGO = InteractabilityManager.s_Instance;
		curCoroutine = coroGO.StartCoroutine(enumerator);
	}

	protected override void InitializeInternal()
	{
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.ElementInfusionBoard);
		tutorialSteps = new List<TutorialStep>
		{
			new TutorialStep
			{
				MessageKey = "PartyColumn",
				OnTrigger = () =>
				{
					StartCoroutine(PartyColumnCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "AbilityCards_Deflecting",
				OnTrigger = () =>
				{
					StartCoroutine(CardSelectionBannerspearCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "SelectDrifter",
				OnTrigger = () =>
				{
					StartCoroutine(SelectDrifterCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "AbilityCards_Violent",
				OnTrigger = () =>
				{
					StartCoroutine(CardSelectionDrifterCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "Buffs_7",
				OnTrigger = () =>
				{
					StartCoroutine(CheckDrifterFinishedMovement());
				}
			},
			new TutorialStep
			{
				MessageKey = "SelectDrifter_Round2",
				OnTrigger = () =>
				{
					StartCoroutine(SelectDrifterCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "UsingPatternActions_5",
				OnTrigger = () =>
				{
					StartCoroutine(TargetMentorWithPatternAttackCoroutine());
				}
			}
		};
	}

	protected override void TickInternal()
	{
		if (InteractabilityManager.s_Instance == null)
		{
			return;
		}
		CLevelMessage cLevelMessage = Singleton<StoryController>.Instance.CurrentMessage?.LevelMsg;
		CLevelMessage cLevelMessage2 = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		InputFHManager.Controls.UI.ShowInfo.Disable();
		if (_tutStep < tutorialSteps.Count && curCoroutine == null)
		{
			TutorialStep tutorialStep = tutorialSteps[_tutStep];
			if (cLevelMessage2?.MessageName == tutorialStep.MessageKey || cLevelMessage?.MessageName == tutorialStep.MessageKey)
			{
				tutorialStep.OnTrigger();
			}
		}
	}

	private void ToggleShowUI(bool interactable, bool visible, CLevelUIInteractionProfile.EIsolatedControlType controlType, string identifier = null)
	{
		foreach (InteractabilityIsolatedUIControl item in InteractabilityManager.s_Instance.ControlsCurrentlyIsolatable.Where((InteractabilityIsolatedUIControl x) => x.ControlType == controlType))
		{
			if (identifier == null || !(item.ControlIdentifier != identifier))
			{
				if (!visible)
				{
					InteractabilityManager.AddExplicilyDisabledControl(item, !visible);
				}
				else if (!interactable & visible)
				{
					InteractabilityManager.AddExplicilyDisabledControl(item, !visible);
				}
				else
				{
					InteractabilityManager.RemoveExplicilyDisabledControl(item);
				}
			}
		}
	}

	private void ToggleStepControls(int step, bool interactable, bool visible)
	{
		if (step >= tutorialSteps.Count)
		{
			return;
		}
		TutorialStep tutorialStep = tutorialSteps[step];
		if (tutorialStep.ControlsToHide != null)
		{
			CLevelUIInteractionProfile.EIsolatedControlType[] controlsToHide = tutorialStep.ControlsToHide;
			foreach (CLevelUIInteractionProfile.EIsolatedControlType controlType in controlsToHide)
			{
				ToggleShowUI(interactable, visible, controlType);
			}
		}
	}

	private bool UpdatePartyColumnHighlight(bool active, string highlightPrefab = null, Vector4 padding = default(Vector4))
	{
		if (active)
		{
			CardsHandUI cardsHandUI = Singleton<CardsHandManager>.Instance.cardHandsUI[0];
			List<RectTransform> list = Singleton<FHScenarioViewController>.Instance?.Modules?.PartyModule?.Elements?.Select((ScenarioPartySlotDetailsElement x) => x.GetComponent<RectTransform>()).ToList();
			if (list != null && list.Count > 0)
			{
				CalcMinMaxForCustomRect(list, out var finalMin, out var finalMax);
				ShowCustomHighlight(finalMin, finalMax, cardsHandUI.transform, "1", highlightPrefab, padding);
				return true;
			}
		}
		ClearCustomRects();
		return false;
	}

	private IEnumerator PartyColumnCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		do
		{
			UpdatePartyColumnHighlight(active: true, "InteractabilityHighlightStatic", new Vector4(-10f, -10f, -10f, -10f));
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		UpdatePartyColumnHighlight(active: false);
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CardSelectionBannerspearCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_script_rfh003.SelectCardsObjective.Current = 0;
		_tutStep++;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<CardsHandManager>.Instance?.CurrentHand?.SelectedCards == null || Singleton<CardsHandManager>.Instance.CurrentHand.SelectedCards.Count < 2);
		_script_rfh003.SelectCardsObjective.Current = 1;
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator SelectDrifterCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (!(Singleton<CardsHandManager>.Instance?.CurrentHand?.PlayerActor?.Class?.ID == "DrifterID"));
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "SelectedDrifter"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator TargetMentorWithPatternAttackCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (!InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight.WasPressedThisFrame());
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "FlippedPattern"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CardSelectionDrifterCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<CardsHandManager>.Instance?.CurrentHand?.SelectedCards == null || Singleton<CardsHandManager>.Instance.CurrentHand.SelectedCards.Count < 2);
		_script_rfh003.SelectCardsObjective.Current = 2;
		_script_rfh003.SelectCardsObjective.ScriptedIsComplete = true;
		_script_rfh003.CompleteFirstRoundObjective.SetActivation(active: true);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CheckDrifterFinishedMovement()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CPlayerActor drifter = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.Class.ID == "DrifterID");
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (drifter.ArrayIndex.X != 4 || drifter.ArrayIndex.Y != 3);
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "DrifterFinishedFirstMove"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	protected override void DeinitializeInternal()
	{
		InputFHManager.Controls.UI.ShowInfo.Enable();
		if (curCoroutine != null && coroGO != null)
		{
			coroGO.StopCoroutine(curCoroutine);
		}
		_ = InteractabilityManager.s_Instance == null;
	}
}
