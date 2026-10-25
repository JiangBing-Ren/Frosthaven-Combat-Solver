using System;
using System.Collections;
using System.Linq;
using ClockStone;
using FFSNet;
using FHGUI.Common;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Services;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UINewAdventureResultsManager : UIResultsManager
{
	[SerializeField]
	private HeaderHighlightText header;

	[Header("Components")]
	[SerializeField]
	public ScenarioDebriefingModule DebriefingModule;

	[SerializeField]
	private CanvasGroup CurtainObject;

	[SerializeField]
	private GameObject BackdropObject;

	[SerializeField]
	private GameObject buttonsContainer;

	[SerializeField]
	private RectTransform buttonsContainerAnchorPoint;

	[SerializeField]
	private GameObject multiplayerReadyTrackerBarContainer;

	[FormerlySerializedAs("resultsPanel")]
	[SerializeField]
	private GameObject outcomeLabel;

	private Action<UnityAction> OnFinishedHeaderAnimation;

	private AudioObject _currentWinLoseMusic;

	public static UINewAdventureResultsManager Implementation => (UINewAdventureResultsManager)Singleton<UIResultsManager>.Instance;

	private void Start()
	{
		gameObject.SetActive(value: true);
		window.onTransitionComplete.AddListener((UIWindow w, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Shown)
			{
				header.Show(() =>
				{
					if (OnFinishedHeaderAnimation == null)
					{
						ContinueShowStats();
					}
					else
					{
						OnFinishedHeaderAnimation(ContinueShowStats);
					}
				});
			}
		});
	}

	private void ContinueShowStats()
	{
		bool flag = Choreographer.IsInNoPartyCampaign();
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign && !flag)
		{
			_ = 1;
		}
		else if (!m_InCustomLevelMode && m_Retry != ERetryOptionType.Only)
		{
			_ = !flag;
		}
		else
			_ = 0;
		OnFinishedOpenAnimations();
	}

	private void OnFinishedOpenAnimations()
	{
		StartCoroutine(DoResultTransition());
	}

	private IEnumerator DoResultTransition()
	{
		if (!Choreographer.IsInNoPartyCampaign())
		{
			yield return StartCoroutine(ScenarioResultViewService.ShowCurtain(CurtainObject));
			header.Hide();
			header.gameObject.SetActive(value: false);
			BackdropObject.gameObject.SetActive(value: false);
			yield return StartCoroutine(ScenarioResultViewService.SetupPartyScene());
			DebriefingModule.Show();
			buttonsContainer.SetActive(value: true);
			AdjustInteractiveElements();
			yield return StartCoroutine(ScenarioResultViewService.SetupPartyReactions(m_Result));
			yield return StartCoroutine(ScenarioResultViewService.HideCurtain(CurtainObject));
			UIContextualTutorialModule.OnEvent("ScenarioResultsShown");
		}
		else
		{
			yield return StartCoroutine(ScenarioResultViewService.SetupPartyReactions(m_Result));
			buttonsContainer.SetActive(value: true);
			AdjustInteractiveElements();
		}
		void AdjustInteractiveElements()
		{
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
				Singleton<UIReadyTrackerBar>.Instance.gameObject.transform.SetParent(multiplayerReadyTrackerBarContainer.transform, worldPositionStays: false);
				UIHelperUtil.TrySetPositionConstraint(Singleton<UIReadyTrackerBar>.Instance.gameObject, multiplayerReadyTrackerBarContainer.transform, 1f, createConstraintIfMissing: true);
			}
		}
	}

	public void Show(float delay = 0f, EndGameCallback onButtonClicked = null, EResult result = EResult.None, ERetryOptionType retryOption = ERetryOptionType.None, UnityAction retryCallback = null, Action<UnityAction> onFinishedResultAnimation = null)
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.Reset();
		}
		base.Show(delay, onButtonClicked, result, retryOption, retryCallback);
		OnFinishedHeaderAnimation = onFinishedResultAnimation;
		buttonsContainer.SetActive(value: false);
	}

	public override void Show(float delay = 0f, EndGameCallback onButtonClicked = null, EResult result = EResult.None, ERetryOptionType retryOption = ERetryOptionType.None, UnityAction retryCallback = null)
	{
		Show(delay, onButtonClicked, result, retryOption, retryCallback);
	}

	protected override void Show()
	{
		base.Show();
		bool flag = Choreographer.IsInNoPartyCampaign();
		DebriefingModule.gameObject.SetActive(value: false);
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign && !flag)
		{
			ShowNewAdventureModeResults();
		}
		else if ((m_InCustomLevelMode || m_Retry == ERetryOptionType.Only) | flag)
		{
			outcomeLabel.SetActive(value: true);
			CurtainObject.gameObject.SetActive(value: false);
			buttonsContainer.transform.SetParent(buttonsContainerAnchorPoint);
			SetResult(m_Result);
		}
		else
		{
			ShowNewAdventureModeResults();
		}
	}

	protected override void Retry()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_RESULTS_REPLAY"), LocalizationManager.GetTranslation("GUI_RESULTS_CONFIRM_REPLAY"), () =>
		{
			if (FFSNetwork.IsOnline)
			{
				if (FFSNetwork.IsHost)
				{
					Synchronizer.SendGameAction(GameActionType.RetryScenario, ActionPhaseType.ScenarioEnded);
				}
				ShowReadyUpRetryToggles(autoReadyUp: true);
			}
			else
			{
				base.Retry();
			}
		}, UIInfoTools.DefInstance.neutralActionTextColor);
	}

	protected override void OnButtonClicked()
	{
		if (m_Result == EResult.Lose || m_Result == EResult.Resign)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_RESULTS_TOOUTPOST"), LocalizationManager.GetTranslation("GUI_RESULTS_CONFIRM_ACCEPT_DEFEAT"), AcceptResult, UIInfoTools.DefInstance.neutralActionTextColor);
		}
		else
		{
			AcceptResult();
		}
		void AcceptResult()
		{
			if (FFSNetwork.IsOnline && m_Retry == ERetryOptionType.Additional)
			{
				if (FFSNetwork.IsHost)
				{
					Synchronizer.SendGameAction(GameActionType.ExitScenario, ActionPhaseType.ScenarioEnded);
				}
				ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT", autoReadyUp: true);
			}
			else
			{
				base.OnButtonClicked();
			}
		}
	}

	public void ProxyRetryScenario(GameAction gameAction)
	{
		if (FFSNetwork.IsClient)
		{
			ShowReadyUpRetryToggles();
			Singleton<UIReadyToggle>.Instance.SetInteractable(buttonsContainer.activeSelf);
		}
	}

	public void ProxyExitScenario(GameAction gameAction)
	{
		if (FFSNetwork.IsClient)
		{
			ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT");
			Singleton<UIReadyToggle>.Instance.SetInteractable(buttonsContainer.activeSelf);
		}
	}

	private void SetResult(EResult result)
	{
		ButtonsBarParam buttonsBarParam = Button_Continue(OnButtonClicked);
		ButtonsBarParam buttonsBarParam2 = Button_Back(Retry);
		switch (result)
		{
		case EResult.Win:
			buttonsBarParam2 = default;
			AudioController.Play("PlaySound_STING_Victory");
			_currentWinLoseMusic = AudioController.Play(GlobalSettings.DefInstance.m_WinMusicItem, 1f, 2f);
			header.SetHighlightKey("GUI_RESULTS_WIN");
			if (FFSNetwork.IsOnline)
			{
				buttonsBarParam2 = default;
				buttonsBarParam = default;
				ShowReadyUpToggles("GUI_CONTINUE");
			}
			else if (Choreographer.IsInNoPartyCampaign())
			{
				buttonsBarParam.LabelKey = "GUI_CONTINUE";
			}
			else
			{
				buttonsBarParam.LabelKey = "GUI_CONTINUE";
			}
			break;
		case EResult.Lose:
		case EResult.Resign:
			buttonsBarParam.LabelKey = "GUI_CONTINUE";
			if (m_Retry != ERetryOptionType.None)
			{
				buttonsBarParam2.LabelKey = "GUI_RESULTS_REPLAY";
			}
			else
			{
				buttonsBarParam2 = default;
			}
			if (m_Retry == ERetryOptionType.Only)
			{
				buttonsBarParam = default;
			}
			AudioController.Play("PlaySound_STING_Defeat");
			_currentWinLoseMusic = AudioController.Play(GlobalSettings.DefInstance.m_LoseMusicItem);
			header.SetWarningText("GUI_RESULTS_LOSE");
			if (!FFSNetwork.IsOnline)
			{
				break;
			}
			switch (m_Retry)
			{
			case ERetryOptionType.None:
				buttonsBarParam2 = default;
				buttonsBarParam = default;
				ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT");
				break;
			case ERetryOptionType.Only:
				buttonsBarParam = default;
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioEnded);
				if (FFSNetwork.IsClient)
				{
					buttonsBarParam2.Interactable = false;
				}
				break;
			case ERetryOptionType.Additional:
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioEnded);
				if (FFSNetwork.IsClient)
				{
					buttonsBarParam2.Interactable = false;
					buttonsBarParam.Interactable = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		default:
			if (result == EResult.Resign)
			{
				if (m_Retry == ERetryOptionType.None)
				{
					buttonsBarParam2 = default;
				}
				AudioController.Play("PlaySound_STING_Defeat");
				header.SetWarningText("GUI_RESULTS_LOSE");
				if (FFSNetwork.IsOnline)
				{
					buttonsBarParam2 = default;
					buttonsBarParam = default;
					ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT");
				}
				else
				{
					buttonsBarParam.LabelKey = "GUI_CONTINUE";
				}
			}
			break;
		}
		SetButtonsBar(buttonsBarParam2, buttonsBarParam);
	}

	private void ShowReadyUpToggles(string buttonText, bool autoReadyUp = false)
	{
		ShowReadyUpToggles(buttonText, () =>
		{
			m_EndGameCallback?.Invoke(m_Result);
		}, autoReadyUp);
	}

	private void ShowReadyUpRetryToggles(bool autoReadyUp = false)
	{
		ShowReadyUpToggles("gui_retry", m_OnRetry.Invoke, autoReadyUp);
	}

	private void ShowReadyUpToggles(string buttonText, Action callback, bool autoReadyUp = false)
	{
		ClearButtonBar();
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioEnded);
		if (PlayerRegistry.MyPlayer.IsParticipant)
		{
			Singleton<UIReadyToggle>.Instance.Initialize(show: true, () =>
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			}, () =>
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
			}, () =>
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
				callback();
			}, (NetworkPlayer player, bool isReady) =>
			{
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
			}, null, buttonText, buttonText);
			if (autoReadyUp)
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
			}
		}
		else
		{
			Singleton<UIReadyToggle>.Instance.Initialize(show: false, () =>
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			}, () =>
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
			}, () =>
			{
				Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
				callback();
			}, (NetworkPlayer player, bool isReady) =>
			{
				Singleton<UIScenarioMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
			}, null, buttonText, buttonText);
		}
		Singleton<UIScenarioMultiplayerController>.Instance.ShowReadyTracker(bringToFront: false, showExhausted: true);
		DebriefingModule.InvalidateButtonLayout();
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
	}

	private void OnControllerChanged(NetworkControllable controllable, NetworkPlayer oldcontroller, NetworkPlayer newcontroller)
	{
		if (Singleton<UIReadyToggle>.Instance.IsVisible != PlayerRegistry.MyPlayer.IsParticipant)
		{
			Singleton<UIReadyToggle>.Instance.ToggleVisibility(PlayerRegistry.MyPlayer.IsParticipant);
		}
	}

	public void ShowNewAdventureModeResults()
	{
		SetResult(m_Result);
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
			Singleton<UIReadyToggle>.Instance.gameObject.transform.SetParent(buttonsContainer.transform);
		}
		CMapParty cMapParty = AdventureState.MapState?.MapParty;
		if (AdventureState.MapState != null && !Choreographer.IsInNoPartyCampaign())
		{
			if (m_Result == EResult.Win || ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor x) => x.CanBattleGoalsCompleteOnLoseScenario()))
			{
				AdventureState.MapState.CurrentMapScenarioState?.CheckBattleGoals();
			}
		}
		else
		{
			Log.GUIInfo("Skip showing accomplishments cause AdventureState.MapState it's null");
		}
		ScenarioResultViewService.PopulateScenarioDebriefingModule(DebriefingModule, m_Result, () =>
		{
			m_EndGameCallback?.Invoke(m_Result);
		}, (m_OnRetry != null) ? ((Action)(() =>
		{
			m_OnRetry?.Invoke();
		})) : null);
		outcomeLabel.SetActive(value: true);
		ScenarioResultViewService.UpdateBattleGoals(m_Result);
		cMapParty.LastScenarioStats.Clear();
		DebriefingModule.InvalidateButtonLayout();
	}

	public override void Hide()
	{
		base.Hide();
		OnHidden();
	}

	private void OnHidden()
	{
		if (_currentWinLoseMusic != null)
		{
			_currentWinLoseMusic.Stop(0.5f);
		}
		header.Hide();
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		ScenarioResultViewService.UnloadScenePartyScreen();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		OnHidden();
	}
}
