using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.Scenario;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour, ILegacyButton
{
	public enum EButtonState
	{
		EREADYBUTTONENDSELECTION,
		EREADYBUTTONENDROUND,
		EREADYBUTTONENDTURN,
		EREADYBUTTONESCAPE,
		EREADYBUTTONPASS,
		EREADYBUTTONCONTINUE,
		EREADYBUTTONLONGREST,
		EREADYBUTTONCONFIRMTARGETS,
		EREADYBUTTONFINISHTARGETSELECT,
		EREADYBUTTONCONFIRM,
		EREADYBUTTONCONFIRMMOVEMENT,
		EREADYBUTTONOPENDOOR,
		EREADYBUTTONCONFIRMDISABLED,
		EREADYBUTTONNA,
		EREADYBUTTONRECOVERCARD,
		EREADYBUTTONIMPROVEDSHORTREST,
		EREADYBUTTONEXCLUDECARDSFROMSHORTREST
	}

	[SerializeField]
	private Button readyButton;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private UIFX_MaterialFX_Control effectControl;

	[SerializeField]
	private GameObject highlight;

	[SerializeField]
	private Button warningMask;

	[SerializeField]
	private ImageProgressBar progressBar;

	[SerializeField]
	private string audioItemEndTurn = "PlaySound_UIEnd_Turn";

	private List<Action> actionsQueue = new List<Action>();

	private List<Action> m_ExtraActions = new List<Action>();

	private List<Action> m_OnUntoggleActions = new List<Action>();

	private bool hideOnClick;

	private string originalText;

	private bool m_Interactable;

	private bool m_Active;

	private EButtonState buttonState = EButtonState.EREADYBUTTONNA;

	private bool showEffects;

	private bool disregardTurnControlForInteractability;

	private Action onClickWarning;

	private Coroutine m_WaitOnReadyCoroutine;

	public InteractabilityIsolatedUIControl.InteractabilityReference interactability;

	[StateDump(Skip = true)]
	public EButtonState ButtonState => buttonState;

	public bool IsActiveAndEnabled => isActiveAndEnabled;

	public Button ButtonComponent => readyButton;

	public bool IsInteractable => readyButton.interactable;

	public List<Action> ActionsQueue => actionsQueue;

	public string Text
	{
		get
		{
			return buttonText.text;
		}
		set
		{
			buttonText.text = value;
		}
	}

	private void Start()
	{
		originalText = buttonText.text;
		effectControl.ToggleEnable(showEffects);
		highlight.SetActive(showEffects);
		warningMask.onClick.AddListener(() =>
		{
			onClickWarning?.Invoke();
		});
		HideWarning();
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

	private void RefreshActive()
	{
		if (!(readyButton == null) && !destroyCancellationToken.IsCancellationRequested)
		{
			readyButton.gameObject.SetActive(m_Active && (interactability == null || interactability.cachedEnabled));
		}
	}

	private void Update()
	{
		if (!SceneController.Instance.GlobalErrorMessage.ShowingMessage && buttonState != EButtonState.EREADYBUTTONCONFIRMDISABLED)
		{
			CheckButtonInteractability();
		}
	}

	public void OnClick()
	{
		OnClick(networkActionIfOnline: true);
	}

	public void OnClick(bool networkActionIfOnline)
	{
		if (ButtonComponent.enabled && !warningMask.gameObject.activeSelf && readyButton.interactable)
		{
			OnClickInternal(networkActionIfOnline);
		}
	}

	public void OnClickInternal(bool networkActionIfOnline = true)
	{
		if (FFSNetwork.IsOnline)
		{
			if (ScenarioRuleClient.IsHandStateMessagePending)
			{
				Log.Info("Ignoring ready button click because a player is about to send his hand/short rest state");
				return;
			}
			if (PlayerRegistry.OtherPlayersAreJoining())
			{
				UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
				return;
			}
			if (!buttonState.In(EButtonState.EREADYBUTTONIMPROVEDSHORTREST))
			{
				if (networkActionIfOnline)
				{
					if (FFSNetwork.IsClient && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.MonsterClassesSelectAbilityCards && !GameState.ThreadIsSleeping)
					{
						return;
					}
					if (buttonState != EButtonState.EREADYBUTTONRECOVERCARD && !(PhaseManager.CurrentPhase is CPhasePlayerExhausted))
					{
						Synchronizer.SendGameAction(GameActionType.ConfirmAction, ActionProcessor.CurrentPhase);
					}
				}
				if (Singleton<UIResultsManager>.Instance.IsShown)
				{
					return;
				}
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
			}
		}
		EButtonState eButtonState = buttonState;
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ConfirmButtonPressed));
		TileBehaviour.SetCallback(Choreographer.s_Choreographer.TileHandler);
		Choreographer.s_Choreographer.DisableTileSelection(active: true);
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		Choreographer.s_Choreographer.m_UndoButton.SetInteractable(active: false);
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
		Singleton<UIUseItemsBar>.Instance.Hide(resetSlots: false);
		Singleton<UIUseAbilitiesBar>.Instance.Hide();
		Singleton<UIActiveBonusBar>.Instance.LockToggledActiveBonuses();
		Singleton<UIActiveBonusBar>.Instance.Hide(toggle: true);
		Singleton<CardsHandManager>.Instance.CurrentHand?.DisableCharacterSpecificMechanic();
		Action actionDelayed = null;
		Log.GUIInfo("READY BUTTON - ON CLICK: \nActions Queue Count: " + actionsQueue.Count + " ButtonState: " + buttonState.ToString() + " Last Choreographer MessageType: " + Choreographer.s_Choreographer.LastMessage.m_Type.ToString() + " HideOnClick: " + hideOnClick);
		if (actionsQueue.Count > 0)
		{
			Action stateActions = null;
			Action queueAction = actionsQueue[0];
			if (actionsQueue.Count > 0)
			{
				actionsQueue.RemoveAt(0);
			}
			if ((uint)(eButtonState - 10) <= 1u)
			{
				stateActions = () =>
				{
					Choreographer.s_Choreographer.DisableTileSelection(active: true);
				};
			}
			actionDelayed = () =>
			{
				queueAction();
				stateActions?.Invoke();
				foreach (Action item in m_ExtraActions.ToList())
				{
					item();
				}
				RemoveAllExtraActions();
				Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
			};
		}
		else
		{
			Choreographer.s_Choreographer.m_UndoButton.gameObject.SetActive(value: false);
			if (eButtonState >= EButtonState.EREADYBUTTONCONTINUE)
			{
				Log.GUIInfo("STEP COMPLETE " + actionsQueue.Count + " = " + buttonState.ToString() + " = " + Choreographer.s_Choreographer.LastMessage.m_Type);
				actionDelayed = () =>
				{
					ScenarioRuleClient.StepComplete();
				};
			}
			else
			{
				UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.EndTurnPressed));
				if (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.EndAbilityAnimSync)
				{
					if (eButtonState == EButtonState.EREADYBUTTONESCAPE)
					{
						Choreographer.s_Choreographer.m_CurrentActor.AllowEscape = true;
					}
					actionDelayed = Choreographer.s_Choreographer.Pass;
				}
			}
			WorldspaceStarHexDisplay.Instance.SetAOELocked(locked: false);
		}
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		if (hideOnClick)
		{
			if (showEffects)
			{
				ButtonComponent.enabled = false;
				UIManager.Instance.RequestToggleLockUI(active: true, gameObject);
				effectControl.ButtonFXActive(() =>
				{
					HideDelayed(actionDelayed);
				});
			}
			else
			{
				Log.GUIInfo("READY BUTTON - invoked delayed action");
				actionDelayed?.Invoke();
				gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (showEffects)
			{
				effectControl.ButtonFXActive();
			}
			Log.GUIInfo("READY BUTTON - invoked delayed action");
			actionDelayed?.Invoke();
		}
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

	public void AddUntoggleAction(Action action)
	{
		m_OnUntoggleActions.Add(action);
	}

	private void HideDelayed(Action afterHideAction)
	{
		try
		{
			ButtonComponent.enabled = true;
			UIManager.Instance.RequestToggleLockUI(active: false, gameObject);
			afterHideAction?.Invoke();
			gameObject.SetActive(value: false);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the ReadyButton.HideDelayed().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00050", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void QueueAlternativeAction(Action extraAction)
	{
		Log.GUIInfo("QUEUE ALTERNATIVE ACTION");
		actionsQueue.Add(extraAction);
	}

	public void ResetAlternativeAction(string text, EButtonState state)
	{
		Log.GUIInfo("RESET ALTERNATIVE ACTION " + state.ToString() + " = " + ((text == null) ? "" : text));
		actionsQueue.Clear();
		buttonText.text = text;
		SetInteractable(interactable: true);
		buttonState = state;
		readyButton.GetComponent<UIAudioEffects>().PointerDownAudioItem = ((state == EButtonState.EREADYBUTTONENDTURN) ? audioItemEndTurn : null);
		RefreshInteractableStyle();
	}

	private void RefreshInteractableStyle()
	{
		buttonText.color = (readyButton.interactable ? UIInfoTools.DefInstance.basicTextColor : UIInfoTools.DefInstance.greyedOutTextColor);
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		highlight.SetActive(showEffects && readyButton.interactable && readyButton.gameObject.activeSelf && !warningMask.gameObject.activeSelf);
	}

	public void ClearAlternativeAction()
	{
		Log.GUIInfo("CLEAR ALTERNATIVE ACTION");
		actionsQueue.Clear();
	}

	public void AlternativeAction(Action extraAction, EButtonState state = EButtonState.EREADYBUTTONNA, string text = null)
	{
		Log.GUIInfo("ALTERNATIVE ACTION " + state.ToString() + " = " + ((text == null) ? "" : text));
		actionsQueue.Clear();
		actionsQueue.Add(extraAction);
		if (!string.IsNullOrEmpty(text))
		{
			buttonText.text = text;
		}
		if (state != EButtonState.EREADYBUTTONNA)
		{
			buttonState = state;
		}
		SetInteractable(state != EButtonState.EREADYBUTTONCONFIRMDISABLED);
		readyButton.GetComponent<UIAudioEffects>().PointerDownAudioItem = ((state == EButtonState.EREADYBUTTONENDTURN) ? audioItemEndTurn : null);
	}

	public void ToggleVisibility(bool active)
	{
		readyButton.gameObject.SetActive(active);
	}

	public void Toggle(bool active, EButtonState state = EButtonState.EREADYBUTTONNA, string text = null, bool hideOnClick = true, bool glowingEffect = false, bool interactable = true, bool disregardTurnControlForInteractability = false, bool haltActionProcessorIfDeactivated = true, string customHelpBoxTextLocKey = null, bool useProgressBar = false)
	{
		Log.GUIInfo("TOGGLE " + actionsQueue.Count + " = " + active + " = " + state.ToString() + " = " + ((text == null) ? "" : text) + " = " + hideOnClick);
		if (((FFSNetwork.IsOnline && !active) & haltActionProcessorIfDeactivated) && ActionProcessor.CurrentPhase != ActionPhaseType.ScenarioEnded && PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
		}
		this.hideOnClick = hideOnClick;
		m_Active = active;
		RefreshActive();
		if (!string.IsNullOrEmpty(text))
		{
			buttonText.text = text;
		}
		if (state != EButtonState.EREADYBUTTONNA)
		{
			buttonState = state;
			showEffects = glowingEffect;
		}
		SetInteractable((active & interactable) && state != EButtonState.EREADYBUTTONCONFIRMDISABLED, disregardTurnControlForInteractability);
		readyButton.GetComponent<UIAudioEffects>().PointerDownAudioItem = ((state == EButtonState.EREADYBUTTONENDTURN) ? audioItemEndTurn : null);
		if (active)
		{
			effectControl.ToggleEnable(showEffects);
			RefreshHighlight();
			if (showEffects)
			{
				effectControl.ButtonFXEndTurn();
			}
			Singleton<HelpBox>.Instance.OnReadyButtonShown(customHelpBoxTextLocKey);
		}
		else
		{
			foreach (Action item in m_OnUntoggleActions.ToList())
			{
				item();
			}
			m_OnUntoggleActions.Clear();
		}
		HideWarning();
		if (useProgressBar)
		{
			progressBar.SetAmount(0f, 1f);
			progressBar.gameObject.SetActive(value: true);
			Log.GUIInfoFormat("Starting ReadyButton progression");
			progressBar.PlayProgressTo(1f, LocalizationManager.GetTranslation("GUI_MULTIPLAYER_READYING"), () =>
			{
				Log.GUIInfoFormat("Completed ReadyButton progression");
				if (interactable && progressBar.gameObject.activeInHierarchy)
				{
					if (!CheckButtonInteractability(out var reason))
					{
						Log.Error("Ready button progress bar is trying to click the ready button when it's not interactable. Reason for not being interactable: " + reason);
					}
					OnClickInternal();
				}
			});
		}
		else
		{
			progressBar.gameObject.SetActive(value: false);
		}
	}

	public void SetInteractable(bool interactable, bool disregardTurnControl = false)
	{
		disregardTurnControlForInteractability = disregardTurnControl;
		bool interactable2 = m_Interactable;
		m_Interactable = interactable;
		CheckButtonInteractability();
		if (m_Interactable != interactable2)
		{
			Log.NetworkInfo("SP Ready Button set to " + (m_Interactable ? " INTERACTABLE. " : " UNINTERACTABLE."));
		}
	}

	private void CheckButtonInteractability()
	{
		CheckButtonInteractability(out var _);
	}

	private bool CheckButtonInteractability(out string reason)
	{
		GameObject gameObject = ((Choreographer.s_Choreographer.m_CurrentActor != null) ? Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.m_CurrentActor) : null);
		int num;
		if (FFSNetwork.IsOnline && !buttonState.In(EButtonState.EREADYBUTTONIMPROVEDSHORTREST) && FFSNetwork.IsClient)
		{
			CPhase currentPhase = PhaseManager.CurrentPhase;
			if (currentPhase != null && currentPhase.Type == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
			{
				num = (GameState.ThreadIsSleeping ? 1 : 0);
				goto IL_0069;
			}
		}
		num = 1;
		goto IL_0069;
		IL_0069:
		bool flag = (byte)num != 0;
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
		bool flag2 = PhaseManager.CurrentPhaseAction == null || cAbility != null;
		bool flag3 = true;
		if (cAbility != null)
		{
			flag3 = cAbility.GetMiscVar("ReadyButtonValid").GetBool(cAbility.VarsContext, defVal: true);
		}
		bool flag4 = GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse || GameState.WaitingForPlayerActorToContinueAbility;
		reason = "valid";
		bool interactable = m_Interactable;
		bool flag5 = !Choreographer.WaitingForSyncPoint;
		bool flag6 = ((!CPhaseAction.AboutToEndACancelledAbility() && !CPhaseAction.CurrentlyEndingAnAbility()) || Choreographer.s_Choreographer.HasMessageOnQueue(CMessageData.MessageType.EndAbilityAnimSync) || Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForEndAbilityAnimSync) | flag4;
		bool flag7 = !Singleton<UIConfirmationBoxManager>.Instance.IsOpen;
		bool flag8 = Choreographer.s_Choreographer.ThisPlayerHasTurnControl || disregardTurnControlForInteractability;
		bool flag9 = !ScenarioRuleClient.IsProcessingOrMessagesQueued | flag4;
		bool flag10 = Choreographer.s_Choreographer.m_MessageQueue.Count <= 0 || AutoTestController.s_AutoTestCurrentlyLoaded;
		bool flag11 = Choreographer.s_Choreographer.m_CurrentActor == null || gameObject == null || Choreographer.IsPlayingIdleAnimation(gameObject);
		bool flag12 = !Choreographer.IsProcessingTileClickMessage();
		FHScenarioViewController instanceFast = Singleton<FHScenarioViewController>.InstanceFast;
		bool? flag13;
		if ((object)instanceFast == null)
		{
			flag13 = null;
		}
		else
		{
			FHScenarioViewModulesCatalogue modules = instanceFast.Modules;
			flag13 = ((modules != null) ? new bool?(!modules.attackSummary.InTransition()) : ((bool?)null));
		}
		bool flag14 = flag13 ?? true;
		bool flag15 = !FastReload.IsSavingMultiThreaded;
		bool flag16 = true;
		if (FFSNetwork.IsOnline)
		{
			flag16 = GHClientCallbacks.CurrentStreamMode == GHClientCallbacks.EStreamMode.NotSet;
		}
		bool flag17 = interactable & flag3 & flag2 & flag & flag5 & flag6 & flag7 & flag8 & flag9 & flag10 & flag11 & flag12 & flag14 & flag15 & flag16;
		if (readyButton.interactable != flag17)
		{
			readyButton.interactable = flag17;
		}
		if (flag17)
		{
			reason = "ready button is fully interactable";
		}
		else if (!flag15)
		{
			reason = "ready button is disabled while fast undo saves checkpoints in the logic thread";
		}
		else if (!interactable)
		{
			reason = "ready button manually set to not interactable";
		}
		else if (!flag3)
		{
			reason = $"ready button is set to not because there is currently an ability that has specific code to block the ready button. The ability is {cAbility}";
		}
		else if (!flag2)
		{
			reason = "ready button is set to not interactable either because we're in the action phase with no current ability";
		}
		else if (!flag)
		{
			reason = "ready button is set to not interactable because we're in the monster card selection phase and this user is a client. Only the host should have it turned on";
		}
		else if (!flag5)
		{
			reason = "ready button is set to not interactable because we're waiting for players to sync up their game state";
		}
		else if (!flag6)
		{
			reason = "ready button is set to not interactable because we're waiting for players to sync up after finishing an ability";
		}
		else if (!flag7)
		{
			reason = "ready button is set to not interactable because the confirmation box is open";
		}
		else if (!flag8)
		{
			reason = $"ready button is set to not interactable because the current actor isn't controlled by this player. Current actor is {Choreographer.s_Choreographer.CurrentActor}";
		}
		else if (!flag9)
		{
			reason = "ready button is set to not interactable because the logic thread is currently processing messages";
		}
		else if (!flag10)
		{
			reason = "ready button is set to not interactable because the choreographer is currently processing messages";
		}
		else if (!flag11)
		{
			reason = $"ready button is set to not interactable because the current actor isn't playing an idle animation. Current actor is {Choreographer.s_Choreographer.CurrentActor}";
		}
		else if (!flag12)
		{
			reason = "ready button is set to not interactable because we're currently processing a tile click message";
		}
		else if (!flag14)
		{
			reason = "ready button is set to not interactable because the attack summary window is currently playing animations";
		}
		else if (!flag16)
		{
			reason = "ready button is set to not interactable because we're in the process of sending data to the host/other clients";
		}
		RefreshInteractableStyle();
		RefreshHighlight();
		return flag17;
	}

	public void ShowWarning(Action onClickWarningCallback = null)
	{
		onClickWarning = onClickWarningCallback;
		warningMask.gameObject.SetActive(value: true);
		RefreshHighlight();
	}

	public void HideWarning()
	{
		onClickWarning = null;
		warningMask.gameObject.SetActive(value: false);
		RefreshHighlight();
	}

	public void HideProgressBar()
	{
		progressBar.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		HideWarning();
	}
}
