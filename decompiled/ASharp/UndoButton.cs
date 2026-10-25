using System;
using System.Collections.Generic;
using FFSNet;
using FHGUI.Elements;
using FHGUI.Scenario;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UndoButton : MonoBehaviour, ILegacyButton, ILegacyButtonFromButtonParams
{
	public enum EButtonState
	{
		EUNDOBUTTONUNDO,
		EUNDOBUTTONCLEARTARGETS,
		EUNDOBUTTONUNDOWAYPOINT
	}

	private class ActionContainer
	{
		public Action m_ActionOverrider;

		public string m_PreviousText;

		public EButtonState m_PreviousButtonState;

		public bool m_PreviousEnabled;

		public bool m_PreviousInteractable;
	}

	[SerializeField]
	private Button m_UndoButton;

	[SerializeField]
	private TextMeshProUGUI m_ButtonText;

	[SerializeField]
	private UITextTooltipTarget m_Tooltip;

	private Stack<ActionContainer> m_OnClickOverrides = new Stack<ActionContainer>();

	private EButtonState m_ButtonState;

	private bool m_Interactable;

	private bool m_Active;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactability;

	private string _text;

	public bool IsActiveAndEnabled => isActiveAndEnabled;

	public bool IsInteractable => m_UndoButton.interactable;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			this.SetTextAndTooltip(value, ref _text, m_ButtonText, m_Tooltip);
		}
	}

	private void Awake()
	{
		Text = LocalizationManager.GetTranslation("GUI_UNDO");
	}

	private void Update()
	{
		CheckButtonInteractability();
	}

	public void SetupInteractability(InteractabilityIsolatedUIControl control = null)
	{
		if (control == null)
		{
			control = GetComponent<InteractabilityIsolatedUIControl>();
		}
		if (control != null)
		{
			interactability = new InteractabilityIsolatedUIControl.InteractabilityReference
			{
				control = control,
				onInteractabilityChanged = () =>
				{
					RefreshActive();
				}
			};
		}
	}

	private void OnDisable()
	{
		ClearOnClickOverriders();
	}

	public void OnClick()
	{
		OnClick(networkActionIfOnline: true);
	}

	public void OnClick(bool networkActionIfOnline)
	{
		if (m_UndoButton.interactable)
		{
			OnClickInternal(networkActionIfOnline);
		}
	}

	public void OnClickInternal(bool networkActionIfOnline = true, GameAction action = null)
	{
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.UndoButtonPressed));
		Singleton<FHScenarioViewController>.Instance.RefreshView();
		CAbility cAbility = null;
		if (PhaseManager.Phase is CPhaseAction { CurrentPhaseAbility: var currentPhaseAbility })
		{
			cAbility = currentPhaseAbility?.m_Ability;
			if (cAbility != null && cAbility is CAbilityMerged { ActiveAbility: { } activeAbility })
			{
				cAbility = activeAbility;
			}
		}
		bool flag = false;
		flag = ((action == null) ? (m_ButtonState == EButtonState.EUNDOBUTTONCLEARTARGETS && (cAbility == null || !cAbility.AllTargets)) : (action.ActionTypeID == 115));
		if (FFSNetwork.IsOnline & networkActionIfOnline)
		{
			if (flag)
			{
				Synchronizer.SendGameAction(GameActionType.ClearTargets, ActionProcessor.CurrentPhase);
			}
			else
			{
				Synchronizer.SendGameAction(GameActionType.UndoAction, ActionProcessor.CurrentPhase);
			}
		}
		if (m_OnClickOverrides.Count > 0)
		{
			ActionContainer actionContainer = m_OnClickOverrides.Pop();
			actionContainer.m_ActionOverrider();
			Text = actionContainer.m_PreviousText;
			m_ButtonState = actionContainer.m_PreviousButtonState;
			HandleEmptyButtonOverrides();
			Toggle(actionContainer.m_PreviousEnabled, m_ButtonState);
			SetInteractable(actionContainer.m_PreviousInteractable);
			return;
		}
		WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
		List<CActiveBonus> selectedActiveBonuses = Singleton<UIActiveBonusBar>.Instance.GetSelectedActiveBonuses();
		SetInteractable(active: false);
		Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
		Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
		Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
		if (cAbility == null)
		{
			return;
		}
		if (flag)
		{
			Singleton<UIUseItemsBar>.Instance.SetItemsInteractable(enable: false);
			Singleton<UIActiveBonusBar>.Instance.SetInteractionAvailableSlots(interactable: false);
			Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
			Singleton<UIUseAugmentationsBar>.Instance.SetInteractionAvailableSlots(interactable: false);
			ScenarioRuleClient.ClearTargets();
			return;
		}
		Singleton<UIUseItemsBar>.Instance.Hide(resetSlots: true, deselectItems: true);
		Singleton<UIActiveBonusBar>.Instance.Hide();
		Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		CAbility parentAbility = cAbility.ParentAbility;
		bool flag2 = parentAbility != null && parentAbility.AbilityType == CAbility.EAbilityType.ChooseAbility;
		if (InitiativeTrack.Instance?.SelectedActor()?.Actor is CPlayerActor && !cAbility.IsItemAbility && !flag2 && GameState.CurrentActionInitiator == GameState.EActionInitiator.AbilityCard)
		{
			Singleton<CardsHandManager>.Instance.Undo((CPlayerActor)InitiativeTrack.Instance.SelectedActor().Actor);
		}
		else if (GameState.CurrentActor is CPlayerActor && !cAbility.IsItemAbility && !flag2 && GameState.CurrentActionInitiator == GameState.EActionInitiator.AbilityCard)
		{
			Singleton<CardsHandManager>.Instance.Undo((CPlayerActor)GameState.CurrentActor);
		}
		ScenarioRuleClient.Undo(selectedActiveBonuses);
	}

	private void HandleEmptyButtonOverrides()
	{
		if (m_OnClickOverrides.Count <= 0)
		{
			Text = LocalizationManager.GetTranslation("GUI_UNDO");
			m_ButtonState = EButtonState.EUNDOBUTTONUNDO;
			if (Choreographer.s_Choreographer.LastMessage is CActorIsSelectingMoveTile_MessageData)
			{
				CActorIsSelectingMoveTile_MessageData cActorIsSelectingMoveTile_MessageData = Choreographer.s_Choreographer.LastMessage as CActorIsSelectingMoveTile_MessageData;
				gameObject.SetActive(cActorIsSelectingMoveTile_MessageData.m_MoveAbility.CanUndo);
			}
			else if (Choreographer.s_Choreographer.LastMessage is CActorIsSelectingPushTile_MessageData)
			{
				CActorIsSelectingPushTile_MessageData cActorIsSelectingPushTile_MessageData = Choreographer.s_Choreographer.LastMessage as CActorIsSelectingPushTile_MessageData;
				gameObject.SetActive(cActorIsSelectingPushTile_MessageData.m_PushAbility.CanUndo);
			}
			else if (Choreographer.s_Choreographer.LastMessage is CActorIsSelectingPullTile_MessageData)
			{
				CActorIsSelectingPullTile_MessageData cActorIsSelectingPullTile_MessageData = Choreographer.s_Choreographer.LastMessage as CActorIsSelectingPullTile_MessageData;
				gameObject.SetActive(cActorIsSelectingPullTile_MessageData.m_PullAbility.CanUndo);
			}
		}
	}

	public void SetOnClickOverrider(Action action, EButtonState buttonState, string text = null)
	{
		if (text != null)
		{
			Text = text;
		}
		m_OnClickOverrides.Push(new ActionContainer
		{
			m_ActionOverrider = action,
			m_PreviousText = Text,
			m_PreviousEnabled = m_UndoButton.gameObject.activeSelf,
			m_PreviousInteractable = m_UndoButton.interactable,
			m_PreviousButtonState = m_ButtonState
		});
		Log.SimpleInfo("Adding Undo override.  Overrides Count: " + m_OnClickOverrides.Count);
		Toggle(active: true, buttonState, text);
	}

	public void ClearOnClickOverriders()
	{
		m_OnClickOverrides.Clear();
		Text = LocalizationManager.GetTranslation("GUI_UNDO");
		m_ButtonState = EButtonState.EUNDOBUTTONUNDO;
	}

	private void RefreshActive()
	{
		if (!(m_UndoButton == null) && !destroyCancellationToken.IsCancellationRequested)
		{
			m_UndoButton.gameObject.SetActive(m_Active && (interactability == null || interactability.cachedEnabled));
		}
	}

	public void Toggle(bool active, EButtonState buttonState = EButtonState.EUNDOBUTTONUNDO, string text = null)
	{
		if (buttonState == EButtonState.EUNDOBUTTONCLEARTARGETS && (!(PhaseManager.Phase is CPhaseAction { CurrentPhaseAbility: var currentPhaseAbility } cPhaseAction) || (currentPhaseAbility?.m_Ability is CAbilityTeleport2 cAbilityTeleport && !cAbilityTeleport.CanClearTargets()) || (cPhaseAction.CurrentPhaseAbility != null && !cPhaseAction.CurrentPhaseAbility.m_Ability.CanClearTargets()) || (cPhaseAction.CurrentPhaseAbility != null && cPhaseAction.CurrentPhaseAbility.m_Ability.AllTargets && GameState.OverridingCurrentActor)))
		{
			return;
		}
		Log.GUIInfo("TOGGLE UNDO = " + active + " = " + buttonState.ToString() + " = " + text);
		if (GameState.OverridingCurrentActor && buttonState == EButtonState.EUNDOBUTTONUNDO && Waypoint.s_Waypoints.Count <= 0 && (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == null || !(PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityChooseAbility { State: CAbilityTargeting.TargetingState.PreProcessingBeforeActorApplies })))
		{
			m_Active = false;
			RefreshActive();
			SetInteractable(active: false);
			return;
		}
		if (text != null)
		{
			Text = text;
		}
		m_ButtonState = buttonState;
		m_Active = active;
		RefreshActive();
		SetInteractable(active);
	}

	public void SetInteractable(bool active)
	{
		active = active && Choreographer.s_Choreographer.ThisPlayerHasTurnControl;
		m_Interactable = active;
		m_ButtonText.color = (active ? UIInfoTools.DefInstance.basicTextColor : UIInfoTools.DefInstance.greyedOutTextColor);
		CheckButtonInteractability();
	}

	private void CheckButtonInteractability()
	{
		bool flag = !FastReload.IsSavingMultiThreaded;
		bool flag2 = true;
		if (FFSNetwork.IsOnline)
		{
			flag2 = GHClientCallbacks.CurrentStreamMode == GHClientCallbacks.EStreamMode.NotSet;
		}
		m_UndoButton.interactable = (m_Interactable & flag2 & flag) && !Choreographer.WaitingForSyncPoint && (!ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse) && (Choreographer.s_Choreographer.m_MessageQueue.Count <= 0 || AutoTestController.s_AutoTestCurrentlyLoaded) && (interactability == null || interactability.cachedInteractable);
	}

	public EButtonState GetButtonState()
	{
		return m_ButtonState;
	}

	public void ApplyButtonParams(ButtonsBarParam parameters)
	{
		Text = parameters.HintKey ?? parameters.LabelKey;
		SetInteractable(parameters.Interactable);
	}
}
