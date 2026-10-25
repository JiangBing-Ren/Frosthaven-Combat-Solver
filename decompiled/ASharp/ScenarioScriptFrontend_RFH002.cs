using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FHGUI.Common;
using FHGUI.Scenario;
using Libs.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

internal class ScenarioScriptFrontend_RFH002 : ScenarioScriptFrontend
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

	private ScenarioScript_RFH002 _script_rfh002 => (ScenarioScript_RFH002)_script;

	private int _tutStep
	{
		get
		{
			if (_script_rfh002 == null)
			{
				return -1;
			}
			return _script_rfh002._tutStep;
		}
		set
		{
			if (_script_rfh002 != null)
			{
				_script_rfh002._tutStep = value;
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
		bool flag = _tutStep >= 3;
		ToggleShowUI(flag, flag, CLevelUIInteractionProfile.EIsolatedControlType.ScenarioObjectivesAndModifiers);
		tutorialSteps = new List<TutorialStep>
		{
			new TutorialStep
			{
				MessageKey = "CardSelection_1",
				OnTrigger = () =>
				{
					StartCoroutine(CardSelection1InitiativeCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "CardSelection_2",
				OnTrigger = () =>
				{
					StartCoroutine(CardSelection2InitiativeCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "CardSelection_4",
				OnTrigger = () =>
				{
					StartCoroutine(CardSelectionCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "RangeAttack_1",
				OnTrigger = () =>
				{
					StartCoroutine(RangeAttack1Coroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "RangeAttackUndo",
				OnTrigger = () =>
				{
					_tutStep++;
					ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.ScenarioObjectivesAndModifiers);
					_script_rfh002.MoveObjective.SetActivation(active: true);
				}
			},
			new TutorialStep
			{
				MessageKey = "Move_2",
				OnTrigger = () =>
				{
					StartCoroutine(MoveCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "HealthBar_3",
				OnTrigger = () =>
				{
					StartCoroutine(GeneralInfoCharacterCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "BasicActions_2",
				OnTrigger = () =>
				{
					StartCoroutine(BasicActionsCoroutine());
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

	private bool UpdateAbilityCardInitiativeHighlight(bool active, string highlightPrefab = null)
	{
		if (active)
		{
			CardsHandUI cardsHandUI = Singleton<CardsHandManager>.Instance.cardHandsUI[0];
			List<RectTransform> list = (from x in InteractabilityManager.s_Instance.ControlsCurrentlyIsolatable
				where x.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.CardSelectionInitiative
				select x.GetHighlightRectTransform()).ToList();
			if (list != null && list.Count > 0)
			{
				CalcMinMaxForCustomRect(list, out var finalMin, out var finalMax);
				ShowCustomHighlight(finalMin, finalMax, cardsHandUI.transform, "1", highlightPrefab);
				return true;
			}
		}
		ClearCustomRects();
		return false;
	}

	private bool UpdateDefaultAbilityHighlights(bool active, string highlightPrefab = null)
	{
		if (active)
		{
			CardsHandUI cardsHandUI = Singleton<CardsHandManager>.Instance.cardHandsUI[0];
			IEnumerable<InteractabilityIsolatedUIControl> enumerable = InteractabilityManager.s_Instance.ControlsCurrentlyIsolatable.Where((InteractabilityIsolatedUIControl x) => x.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility || x.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility);
			if (enumerable != null && enumerable.Any())
			{
				foreach (IGrouping<string, InteractabilityIsolatedUIControl> item in from x in enumerable
					group x by x.ControlIdentifier)
				{
					List<RectTransform> rects = item.Select((InteractabilityIsolatedUIControl x) => x.GetHighlightRectTransform()).ToList();
					CalcMinMaxForCustomRect(rects, out var finalMin, out var finalMax);
					ShowCustomHighlight(finalMin, finalMax, cardsHandUI.transform, item.Key, highlightPrefab);
				}
				return true;
			}
		}
		ClearCustomRects();
		return false;
	}

	private IEnumerator CardSelection1InitiativeCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		do
		{
			UpdateAbilityCardInitiativeHighlight(active: true, "InteractabilityHighlightStatic");
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		UpdateAbilityCardInitiativeHighlight(active: false);
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CardSelection2InitiativeCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		RectTransform initiativeTrack = InitiativeTrack.Instance?.InitiativeTrackRect;
		if (initiativeTrack != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(initiativeTrack, new Vector4(-10f, -10f, -10f, -10f), "InteractabilityHighlightStatic");
		}
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		if (initiativeTrack != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(initiativeTrack);
		}
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CardSelectionCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<CardsHandManager>.Instance?.CurrentHand?.SelectedCards == null || Singleton<CardsHandManager>.Instance.CurrentHand.SelectedCards.Count < 2);
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "SelectedJavelinAndTipOfTheSpear"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator RangeAttack1Coroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		IEnumerable<AbilityCardUI> enumerable = Singleton<CardsHandManager>.Instance.CurrentHand.AllCards.Where((AbilityCardUI x) => x.IsSelected);
		List<RectTransform> initiativeRects = new List<RectTransform>();
		foreach (AbilityCardUI item in enumerable)
		{
			RectTransform rectTransform = item.transform.FindTransformInChildren("ranged_attack") as RectTransform;
			if (rectTransform != null)
			{
				initiativeRects.Add(rectTransform);
			}
			RectTransform rectTransform2 = item.transform.FindTransformInChildren("FH_UI_HUD_CARD_AttackSingle") as RectTransform;
			if (rectTransform2 != null)
			{
				initiativeRects.Add(rectTransform2);
			}
		}
		foreach (RectTransform item2 in initiativeRects)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(item2, new Vector4(-10f, -10f, -10f, -10f), "InteractabilityHighlightStatic");
		}
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		foreach (RectTransform item3 in initiativeRects)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(item3);
		}
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator MoveCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (ScenarioManager.Scenario.FindPlayerAt(new Point(3, 2)) == null);
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "ReachedTile32"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator GeneralInfoCharacterCoroutine()
	{
		int curStep = _tutStep;
		_tutStep++;
		ToggleShowUI(interactable: true, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton);
		ToggleShowUI(interactable: true, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText);
		UIGeneralInfoModule generalInfo = Singleton<FHScenarioViewController>.Instance.GeneralInfo;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		IGeneralInfoData actor = UIGeneralInfoUtils.GetActor(ScenarioManager.Scenario.PlayerActors[0]);
		generalInfo.RequestGeneralInfo(actor);
		InputFHManager.Controls.UI.ShowInfo.Disable();
		ToggleStepControls(curStep, interactable: false, visible: false);
		UIGeneralInfoBase uIGeneralInfoBase = generalInfo.CurrentModule as UIGeneralInfoBase;
		ClickTrackerScopeElement scope = uIGeneralInfoBase.GetComponentInChildren<ClickTrackerScopeElement>();
		if (scope != null)
		{
			scope.enabled = false;
		}
		RectTransform closeButton = null;
		if (uIGeneralInfoBase != null)
		{
			closeButton = uIGeneralInfoBase.transform.FindTransformInChildren("CloseButton") as RectTransform;
			if (closeButton == null)
			{
				closeButton = uIGeneralInfoBase.transform.FindTransformInChildren("FH_Button_Close_Modal") as RectTransform;
			}
		}
		RectTransform attackModifiers = null;
		if (uIGeneralInfoBase != null)
		{
			attackModifiers = uIGeneralInfoBase.transform.FindTransformInChildren("AttackModifiers") as RectTransform;
		}
		int oldHighlightOrder = 0;
		int oldMessageOrder = 0;
		Canvas generalInfoCanvas = generalInfo.GetComponent<Canvas>();
		Canvas highlightCanvas = InteractabilityHighlightCanvas.s_Instance.GetComponent<Canvas>();
		Canvas levelMessagesCanvas = InteractabilityManager.s_Instance.ControlsCurrentlyIsolatable.SingleOrDefault((InteractabilityIsolatedUIControl x) => x.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow)?.GetComponent<Canvas>();
		if (generalInfoCanvas != null)
		{
			if (highlightCanvas != null)
			{
				oldHighlightOrder = highlightCanvas.sortingOrder;
				highlightCanvas.sortingOrder = generalInfoCanvas.sortingOrder + 1;
			}
			if (levelMessagesCanvas != null)
			{
				oldMessageOrder = levelMessagesCanvas.sortingOrder;
				levelMessagesCanvas.sortingOrder = generalInfoCanvas.sortingOrder + 1;
			}
		}
		while (true)
		{
			yield return new WaitForEndOfFrame();
			if (generalInfo.CurrentModule == null)
			{
				break;
			}
			if (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage != null)
			{
				if (attackModifiers != null)
				{
					InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(attackModifiers, new Vector4(-10f, -10f, -10f, -10f), "InteractabilityHighlightStatic");
				}
			}
			else if (closeButton != null)
			{
				InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(closeButton);
			}
		}
		if (generalInfoCanvas != null)
		{
			if (highlightCanvas != null)
			{
				highlightCanvas.sortingOrder = oldHighlightOrder;
			}
			if (levelMessagesCanvas != null)
			{
				levelMessagesCanvas.sortingOrder = oldMessageOrder;
			}
		}
		if (scope != null)
		{
			scope.enabled = true;
		}
		if (attackModifiers != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(attackModifiers);
		}
		if (closeButton != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(closeButton);
		}
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton);
		ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText);
		curCoroutine = null;
	}

	private IEnumerator BasicActionsCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		do
		{
			UpdateDefaultAbilityHighlights(active: true, "InteractabilityHighlightStatic");
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		UpdateDefaultAbilityHighlights(active: false);
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
