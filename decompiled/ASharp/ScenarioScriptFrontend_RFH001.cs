using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.Scenario;
using Libs.Utils;
using Libs.Utils.Maths;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

internal class ScenarioScriptFrontend_RFH001 : ScenarioScriptFrontend
{
	private class TutorialStep
	{
		public string MessageKey;

		public CLevelUIInteractionProfile.EIsolatedControlType[] ControlsToHide;

		public Action OnTrigger;
	}

	private int _tutStep;

	private List<TutorialStep> tutorialSteps;

	private Coroutine curCoroutine;

	private MonoBehaviour coroGO;

	private ScenarioScript_RFH001 _script_rfh001 => (ScenarioScript_RFH001)_script;

	private void StartCoroutine(IEnumerator enumerator)
	{
		coroGO = InteractabilityManager.s_Instance;
		curCoroutine = coroGO.StartCoroutine(enumerator);
	}

	protected override void InitializeInternal()
	{
		_tutStep = 0;
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility);
		tutorialSteps = new List<TutorialStep>
		{
			new TutorialStep
			{
				MessageKey = "StartStory",
				OnTrigger = () =>
				{
					StartCoroutine(StartStoryInteractabilityCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "CameraMovement",
				ControlsToHide = new CLevelUIInteractionProfile.EIsolatedControlType[3]
				{
					CLevelUIInteractionProfile.EIsolatedControlType.CharacterTab,
					CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton,
					CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText
				},
				OnTrigger = () =>
				{
					StartCoroutine(CameraMovementCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "CardDetailed_1",
				ControlsToHide = new CLevelUIInteractionProfile.EIsolatedControlType[2]
				{
					CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton,
					CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText
				},
				OnTrigger = () =>
				{
					StartCoroutine(CardDetailed1Coroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "CardDetailed_2",
				ControlsToHide = new CLevelUIInteractionProfile.EIsolatedControlType[3]
				{
					CLevelUIInteractionProfile.EIsolatedControlType.CharacterTab,
					CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton,
					CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText
				},
				OnTrigger = () =>
				{
					StartCoroutine(CardDetailed2Coroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "CardSelection_1",
				OnTrigger = () =>
				{
					StartCoroutine(CardSelectionCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "Waypoint_1",
				OnTrigger = () =>
				{
					StartCoroutine(ObjectivesCoroutine());
				}
			},
			new TutorialStep
			{
				MessageKey = "Enemies_2",
				OnTrigger = () =>
				{
					StartCoroutine(EnemiesCoroutine());
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
		if (_tutStep < tutorialSteps.Count)
		{
			TutorialStep tutorialStep = tutorialSteps[_tutStep];
			if (curCoroutine == null && (cLevelMessage2?.MessageName == tutorialStep.MessageKey || cLevelMessage?.MessageName == tutorialStep.MessageKey))
			{
				tutorialStep.OnTrigger();
			}
			if (_tutStep < 4)
			{
				InputFHManager.Controls.UI.ShowInfo.Disable();
			}
			else
			{
				InputFHManager.Controls.UI.ShowInfo.Enable();
			}
		}
	}

	private bool UpdateAbilityCardHighlight(bool active, string highlightPrefab = null)
	{
		if (active)
		{
			CardsHandUI cardsHandUI = Singleton<CardsHandManager>.Instance.cardHandsUI[0];
			List<RectTransform> list = (from x in cardsHandUI.AllCards
				where x.isActiveAndEnabled
				select x.GetComponent<RectTransform>()).ToList();
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

	private IEnumerator StartStoryInteractabilityCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.CardSelection, "ABILITY_CARD_LongRest");
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.ShortRestButton);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.ScenarioObjectivesAndModifiers);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.ElementInfusionBoard);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.CombatLog);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarUndoButton);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarSkipButton);
		yield return null;
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CameraMovementCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		Vector2 vector;
		bool flag;
		do
		{
			yield return new WaitForEndOfFrame();
			vector = InputFHManager.Controls.Camera.Movement.ReadValue<Vector2>();
			flag = InputFHManager.Controls.Camera.DragMove.IsPressed();
		}
		while (!((!vector.x.IsZero() || !vector.y.IsZero()) | flag));
		yield return new WaitForSeconds(2f);
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "MovedWASD"));
		float value;
		bool flag2;
		float value2;
		do
		{
			yield return new WaitForEndOfFrame();
			value = InputFHManager.Controls.Camera.Rotate.ReadValue<float>();
			flag2 = InputFHManager.Controls.Camera.DragRotate.IsPressed();
			value2 = InputFHManager.Controls.Camera.Zoom.ReadValue<float>();
		}
		while (!((Math.Abs(value) > 0.1f) | flag2) && !(Math.Abs(value2) > 0.1f));
		yield return new WaitForSeconds(2f);
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "MovedEQ"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		curCoroutine = null;
	}

	private IEnumerator CardDetailed1Coroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: true);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		do
		{
			UpdateAbilityCardHighlight(active: true, "InteractabilityHighlightStatic");
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		UpdateAbilityCardHighlight(active: false);
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton);
		ToggleShowUI(interactable: false, visible: false, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText);
		curCoroutine = null;
	}

	private IEnumerator CardDetailed2Coroutine()
	{
		int curStep = _tutStep;
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		UIGeneralInfoModule generalInfo = Singleton<FHScenarioViewController>.Instance.GeneralInfo;
		int oldHighlightOrder = 0;
		int oldMessageOrder = 0;
		CLevelMessage cLevelMessage;
		do
		{
			UpdateAbilityCardHighlight(active: true);
			yield return new WaitForEndOfFrame();
			if (generalInfo.CurrentUIObject != null && generalInfo.CurrentUIObject.GetInfoData() is UIGeneralInfoCard.UIGeneralInfoCardData)
			{
				InputFHManager.Controls.UI.ShowInfo.Enable();
			}
			else
			{
				InputFHManager.Controls.UI.ShowInfo.Disable();
			}
			cLevelMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		}
		while (generalInfo.CurrentModule == null || cLevelMessage == initialMessage);
		Canvas generalInfoCanvas = generalInfo.GetComponent<Canvas>();
		UpdateAbilityCardHighlight(active: false);
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
		InputFHManager.Controls.UI.ShowInfo.Disable();
		ToggleStepControls(curStep, interactable: false, visible: false);
		UIGeneralInfoBase uIGeneralInfoBase = generalInfo.CurrentModule as UIGeneralInfoBase;
		RectTransform closeButton = null;
		if (uIGeneralInfoBase != null)
		{
			closeButton = uIGeneralInfoBase.transform.FindTransformInChildren("CloseButton") as RectTransform;
			if (closeButton == null)
			{
				closeButton = uIGeneralInfoBase.transform.FindTransformInChildren("FH_Button_Close_Modal") as RectTransform;
			}
		}
		if (closeButton != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(closeButton);
		}
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (generalInfo.CurrentModule != null);
		if (closeButton != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(closeButton);
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
		ToggleStepControls(curStep, interactable: true, visible: true);
		ToggleStepControls(_tutStep, interactable: false, visible: false);
		ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarConfirmButton);
		ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.BottomBarHelpText);
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
		while (Singleton<CardsHandManager>.Instance.cardHandsUI[0].AllCards.Count((AbilityCardUI x) => x.IsSelected) != 2);
		SEventLogMessageHandler.AddEventLogMessage(new SEvent(ESEType.CustomSEvent, "Selected2Cards"));
		ToggleStepControls(curStep, interactable: true, visible: true);
		curCoroutine = null;
	}

	private IEnumerator ObjectivesCoroutine()
	{
		int curStep = _tutStep;
		ToggleStepControls(curStep, interactable: false, visible: false);
		ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.ScenarioObjectivesAndModifiers);
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage == initialMessage);
		ToggleStepControls(curStep, interactable: true, visible: true);
		curCoroutine = null;
	}

	private IEnumerator EnemiesCoroutine()
	{
		int curStep = _tutStep;
		_tutStep++;
		CLevelMessage initialMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		UIGeneralInfoModule generalInfo = Singleton<FHScenarioViewController>.Instance.GeneralInfo;
		int oldHighlightOrder = 0;
		int oldMessageOrder = 0;
		CLevelMessage cLevelMessage;
		do
		{
			yield return new WaitForEndOfFrame();
			IGeneralInfoData generalInfoData = null;
			if (generalInfo.HexInfo.ActorOnTileWithIniative != null || generalInfo.CurrentMapObject != null)
			{
				generalInfoData = ((generalInfo.HexInfo.ActorOnTileWithIniative != null) ? UIGeneralInfoUtils.GetActor(generalInfo.HexInfo.ActorOnTileWithIniative) : generalInfo.CurrentMapObject.GetInfoData());
			}
			if (generalInfoData is UIGeneralInfoMonster.MonsterGeneralInfoData)
			{
				InputFHManager.Controls.UI.ShowInfo.Enable();
			}
			else
			{
				InputFHManager.Controls.UI.ShowInfo.Disable();
			}
			cLevelMessage = Singleton<LevelMessagesUIHandler>.Instance?.CurrentlyDisplayedBoxMessage;
		}
		while (generalInfo.CurrentModule == null || cLevelMessage == initialMessage);
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
		InputFHManager.Controls.UI.ShowInfo.Disable();
		ToggleStepControls(curStep, interactable: false, visible: false);
		UIGeneralInfoBase uIGeneralInfoBase = generalInfo.CurrentModule as UIGeneralInfoBase;
		RectTransform closeButton = null;
		if (uIGeneralInfoBase != null)
		{
			closeButton = uIGeneralInfoBase.transform.FindTransformInChildren("CloseButton") as RectTransform;
			if (closeButton == null)
			{
				closeButton = uIGeneralInfoBase.transform.FindTransformInChildren("FH_Button_Close_Modal") as RectTransform;
			}
		}
		if (closeButton != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryAddHighlightForRectTransform(closeButton);
		}
		do
		{
			yield return new WaitForEndOfFrame();
		}
		while (generalInfo.CurrentModule != null);
		if (closeButton != null)
		{
			InteractabilityHighlightCanvas.s_Instance?.TryRemoveHighlightForRectTransform(closeButton);
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
		if (!(InteractabilityManager.s_Instance == null))
		{
			ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.DefaultAttackAbility);
			ToggleShowUI(interactable: true, visible: true, CLevelUIInteractionProfile.EIsolatedControlType.DefaultMoveAbility);
		}
	}
}
