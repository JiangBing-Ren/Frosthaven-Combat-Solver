using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkipButton : MonoBehaviour, ILegacyButton
{
	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	private bool m_HideOnClick;

	private string m_OriginalText;

	private bool m_Interactable;

	private bool m_Active;

	public Action m_OnSkipAction;

	private List<Action> m_ExtraActions = new List<Action>();

	private string _text;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactability;

	public bool IsActiveAndEnabled => isActiveAndEnabled;

	public bool IsInteractable => m_Interactable;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			this.SetTextAndTooltip(value, ref _text, buttonText, tooltip);
		}
	}

	private void Start()
	{
		Text = LocalizationManager.GetTranslation("GUI_SKIP_MOVEMENT");
		RefreshInteractionStyle();
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

	private void Update()
	{
		CheckButtonInteractability();
	}

	private void RefreshInteractionStyle()
	{
		buttonText.color = ((m_Interactable && skipButton.interactable) ? UIInfoTools.DefInstance.basicTextColor : UIInfoTools.DefInstance.greyedOutTextColor);
	}

	private void RefreshActive()
	{
		if (!(skipButton == null) && !destroyCancellationToken.IsCancellationRequested)
		{
			skipButton.gameObject.SetActive(m_Active && (interactability == null || interactability.cachedEnabled));
		}
	}

	public void OnClick()
	{
		OnClick(networkActionIfOnline: true);
	}

	public void OnClick(bool networkActionIfOnline)
	{
		if (FFSNetwork.IsOnline)
		{
			if (skipButton.interactable & networkActionIfOnline)
			{
				Synchronizer.SendGameAction(GameActionType.SkipAction, ActionProcessor.CurrentPhase);
			}
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
		}
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.SkipButtonPressed));
		TileBehaviour.SetCallback(Choreographer.s_Choreographer.TileHandler);
		Choreographer.s_Choreographer.DisableTileSelection(active: true);
		Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
		Choreographer.s_Choreographer.readyButton.Toggle(active: false);
		Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
		Singleton<UIAbilityCardPicker>.Instance.Hide();
		Singleton<UIUseItemsBar>.Instance.Hide(resetSlots: true, deselectItems: true);
		Singleton<UIUseAbilitiesBar>.Instance.Hide(clearSelection: true);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.LockToggledActiveBonuses();
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
		m_OnSkipAction?.Invoke();
		ClearSkipAction();
		foreach (Action item in m_ExtraActions.ToList())
		{
			item();
		}
		RemoveAllExtraActions();
		if (m_HideOnClick)
		{
			gameObject.SetActive(value: false);
		}
		CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
		if (currentAbility == null || currentAbility.AbilityType != CAbility.EAbilityType.MoveShadow)
		{
			CAbility currentAbility2 = Choreographer.s_Choreographer.m_CurrentAbility;
			if (currentAbility2 == null || currentAbility2.AbilityType != CAbility.EAbilityType.Push || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { State: CAbilityPush.EPushState.ActorIsSelectingPushTile }))
			{
				CAbility currentAbility3 = Choreographer.s_Choreographer.m_CurrentAbility;
				if (currentAbility3 == null || currentAbility3.AbilityType != CAbility.EAbilityType.Pull || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull { State: CAbilityPull.EPullState.ActorIsSelectingPullTile }))
				{
					CAbility currentAbility4 = Choreographer.s_Choreographer.m_CurrentAbility;
					if (currentAbility4 == null || currentAbility4.AbilityType != CAbility.EAbilityType.Teleport2)
					{
						if (GameState.CurrentExtraTurnAbility != null && !GameState.CurrentExtraTurnAbility.IsPending() && GameState.ExtraTurnActionSelectionFlagStack.Count == 0)
						{
							GameState.EndExtraTurn(force_cancel: true);
						}
						if (GameState.CurrentSelectAndPlayCardAbility != null && PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == GameState.CurrentSelectAndPlayCardAbility && GameState.CurrentSelectAndPlayCardAbility.IsSelectingOrPlayingCards)
						{
							GameState.CurrentSelectAndPlayCardAbility.EndOutOfPhaseAbilities();
						}
						else if (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.EndAbilityAnimSync)
						{
							Choreographer.s_Choreographer.Pass();
						}
						Choreographer.s_Choreographer.m_CurrentAbility = null;
						goto IL_02c2;
					}
				}
			}
		}
		if (Choreographer.s_Choreographer.m_CurrentAbility.NumberTargets == 99)
		{
			Choreographer.s_Choreographer.m_CurrentAbility.SetCancelAbilityFlag(cancel: true);
		}
		ScenarioRuleClient.PassStep();
		goto IL_02c2;
		IL_02c2:
		Choreographer.s_Choreographer.FirstAbility = false;
	}

	public void Toggle(bool active, string text = null, bool hideOnClick = true, bool? interactable = null, Action onSkipAction = null)
	{
		if (!interactable.HasValue)
		{
			interactable = active;
		}
		m_HideOnClick = hideOnClick;
		m_OnSkipAction = onSkipAction;
		m_Active = active;
		RefreshActive();
		SetInteractable(interactable.Value);
		if (!string.IsNullOrEmpty(text))
		{
			Text = text;
		}
	}

	public void SetInteractable(bool active)
	{
		m_Interactable = active;
		CheckButtonInteractability();
	}

	public void AddExtraAction(Action action)
	{
		m_ExtraActions.Add(action);
	}

	public void RemoveExtraAction(Action action)
	{
		m_ExtraActions.Remove(action);
	}

	public void RemoveAllExtraActions()
	{
		m_ExtraActions.Clear();
	}

	private void CheckButtonInteractability()
	{
		GameObject gameObject = ((Choreographer.s_Choreographer.m_CurrentActor != null) ? Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.m_CurrentActor) : null);
		bool flag = GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse || GameState.WaitingForPlayerActorToContinueAbility;
		bool flag2 = !FastReload.IsSavingMultiThreaded;
		bool flag3 = true;
		if (FFSNetwork.IsOnline)
		{
			flag3 = GHClientCallbacks.CurrentStreamMode == GHClientCallbacks.EStreamMode.NotSet;
		}
		skipButton.interactable = (m_Interactable & flag3 & flag2) && !Choreographer.WaitingForSyncPoint && (interactability == null || interactability.cachedInteractable) && (!ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse || GameState.WaitingForGenericConfirmation || (GameState.ThreadIsSleeping && !string.IsNullOrEmpty(GameState.ThreadSleepReason))) && (Choreographer.s_Choreographer.m_MessageQueue.Count <= 0 || AutoTestController.s_AutoTestCurrentlyLoaded) && Choreographer.s_Choreographer.ThisPlayerHasTurnControl && (((!CPhaseAction.AboutToEndACancelledAbility() && !CPhaseAction.CurrentlyEndingAnAbility()) || Choreographer.s_Choreographer.HasMessageOnQueue(CMessageData.MessageType.EndAbilityAnimSync) || Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForEndAbilityAnimSync) | flag) && (Choreographer.s_Choreographer.m_CurrentActor == null || gameObject == null || Choreographer.IsPlayingIdleAnimation(gameObject));
		RefreshInteractionStyle();
	}

	private void OnDisable()
	{
		ClearSkipAction();
	}

	public void ClearSkipAction()
	{
		m_OnSkipAction = null;
	}
}
