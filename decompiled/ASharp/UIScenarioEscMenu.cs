using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using FFSNet.Backend;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIScenarioEscMenu : ESCMenu
{
	[SerializeField]
	private UIMainMenuOption quitDungeonButton;

	[SerializeField]
	private UIMainMenuOption restartRoundButton;

	[SerializeField]
	private UIMainMenuOption restartTurnButton;

	[Obsolete]
	[SerializeField]
	private UIMainMenuOption skipMissionButton;

	[Obsolete]
	[SerializeField]
	private UIMainMenuOption skipTutorialButton;

	[SerializeField]
	private UIMainMenuOption loadLevelEditorButton;

	private Coroutine multiplayerCoroutine;

	public static UIScenarioEscMenu ScenarioInstance => Singleton<ESCMenu>.Instance as UIScenarioEscMenu;

	protected override void AwakeInit()
	{
		base.AwakeInit();
		quitDungeonButton.Init(QuitDungeon);
		restartRoundButton.Init(() =>
		{
			QueueRestartRound();
		});
		restartTurnButton.Init(RestartTurn);
		skipMissionButton.Init(SkipMission);
		skipTutorialButton.Init(SkipTutorial);
		loadLevelEditorButton.Init(LoadLevelEditor);
		InteractabilityIsolatedUIControl component = GetComponent<InteractabilityIsolatedUIControl>();
		if (component != null)
		{
			component.Initialise();
			InteractabilityManager.s_Instance?.LoadDefaultMessagelessProfile();
		}
	}

	private void Update()
	{
		if (window.IsOpen)
		{
			UpdateButtonStates();
		}
	}

	private static bool ArePlayersJoining()
	{
		if (!FFSNetwork.IsOnline)
		{
			return false;
		}
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			return false;
		}
		return PlayerRegistry.OtherPlayersAreJoining();
	}

	private void UpdateButtonStates()
	{
		bool flag = Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown;
		if (Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.WaitingForEndOfScenarioRewards)
		{
			flag = false;
		}
		if (flag)
		{
			switch (PhaseManager.PhaseType)
			{
			case CPhase.PhaseType.ShortRest:
				if (Singleton<TakeDamagePanel>.Instance.enabled && Singleton<TakeDamagePanel>.Instance.PlayerActorMakingChoice == GameState.s_ShortRestingPlayer)
				{
					flag = false;
				}
				else if (FFSNetwork.IsOnline && !GameState.s_ShortRestingPlayer.IsUnderMyControl())
				{
					flag = false;
				}
				break;
			case CPhase.PhaseType.ActionSelection:
				if (!(Choreographer.s_Choreographer.CurrentActor is CPlayerActor actor2))
				{
					flag = false;
				}
				else if (FFSNetwork.IsOnline && !actor2.IsUnderMyControl())
				{
					flag = false;
				}
				break;
			case CPhase.PhaseType.InfuserMechanicSelection:
				if (!(GameState.PendingInfusionPhase?.TriggerActor is CPlayerActor actor))
				{
					flag = false;
				}
				else if (FFSNetwork.IsOnline && !actor.IsUnderMyControl())
				{
					flag = false;
				}
				break;
			default:
				flag = false;
				break;
			case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
				break;
			}
		}
		bool flag2 = SaveData.Instance.Global.GameMode != EGameMode.FrontEndTutorial;
		bool flag3 = !ArePlayersJoining();
		bool flag4 = !FFSNetwork.IsOnline || FFSNetwork.IsHost || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest;
		bool flag5 = true;
		if (flag & flag2 & flag3 & flag4 & flag5)
		{
			restartRoundButton.IsInteractable = true;
			restartRoundButton.SetTooltip(enable: false);
		}
		else
		{
			restartRoundButton.IsInteractable = false;
			if (!flag4)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_MP_HOST_TOOLTIP"));
			}
			else if (!flag3)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_WHILE_PLAYERS_JOINING_MP_TOOLTIP"));
			}
			else if (!flag)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_PHASE_TOOLTIP"));
			}
			else if (!flag2)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_GAMEMODE_TOOLTIP"));
			}
			else
			{
				restartRoundButton.SetTooltip(enable: true);
			}
		}
		bool flag6 = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
		bool valueOrDefault = ScenarioManager.CurrentScenarioState?.ScenarioSetup?.CanNotBeLost == true;
		if ((flag & flag3 & flag6) && !valueOrDefault)
		{
			quitDungeonButton.IsInteractable = true;
			quitDungeonButton.SetTooltip(enable: false);
			return;
		}
		quitDungeonButton.IsInteractable = false;
		if (!flag6)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_MP_HOST_TOOLTIP"));
		}
		else if (!flag3)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_WHILE_PLAYERS_JOINING_MP_TOOLTIP"));
		}
		else if (!flag)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_PHASE_TOOLTIP"));
		}
		else if (valueOrDefault)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_CANNOT_BE_LOST"));
		}
		else
		{
			quitDungeonButton.SetTooltip(enable: true);
		}
	}

	protected override void SetFocused(bool isFocused)
	{
		base.SetFocused(isFocused);
		quitDungeonButton.SetFocused(isFocused);
		restartRoundButton.SetFocused(isFocused);
		restartTurnButton.SetFocused(isFocused);
		skipMissionButton.SetFocused(isFocused);
		skipTutorialButton.SetFocused(isFocused);
		loadLevelEditorButton.SetFocused(isFocused);
	}

	protected override void OnShow()
	{
		base.OnShow();
		quitDungeonButton.gameObject.SetActive(SaveData.Instance?.Global != null);
		skipMissionButton.gameObject.SetActive(value: false);
		skipTutorialButton.gameObject.SetActive(value: false);
		loadLevelEditorButton.gameObject.SetActive(LevelEditorController.s_Instance?.IsPreviewingLevel ?? false);
		quitDungeonButton.gameObject.SetActive(!loadLevelEditorButton.gameObject.activeSelf);
		if (quitDungeonButton.gameObject.activeSelf)
		{
			if (FFSNetwork.IsOnline)
			{
				OnSwitchedToMultiplayerPlayer();
			}
			else
			{
				OnSwitchedToSinglePlayer();
			}
			FFSNetwork.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
			FFSNetwork.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
			FFSNetwork.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayerPlayer);
			FFSNetwork.HostingStartedEvent.AddListener(OnSwitchedToMultiplayerPlayer);
		}
		StartTrackMultiplayerButtonState();
	}

	protected override void OnHide()
	{
		FFSNetwork.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		FFSNetwork.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayerPlayer);
		base.OnHide();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnUserConnected));
		PlayerRegistry.OnUserReadyForAssignment = (Action)Delegate.Remove(PlayerRegistry.OnUserReadyForAssignment, new Action(OnUserReadyForAssignment));
	}

	private void OnSwitchedToSinglePlayer()
	{
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnUserConnected));
		PlayerRegistry.OnUserReadyForAssignment = (Action)Delegate.Remove(PlayerRegistry.OnUserReadyForAssignment, new Action(OnUserReadyForAssignment));
	}

	private void OnSwitchedToMultiplayerPlayer()
	{
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnUserConnected));
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnUserConnected));
		PlayerRegistry.OnUserReadyForAssignment = (Action)Delegate.Remove(PlayerRegistry.OnUserReadyForAssignment, new Action(OnUserReadyForAssignment));
		PlayerRegistry.OnUserReadyForAssignment = (Action)Delegate.Combine(PlayerRegistry.OnUserReadyForAssignment, new Action(OnUserReadyForAssignment));
	}

	private void StartTrackMultiplayerButtonState()
	{
		CheckMultiplayerButtonState();
	}

	private void CheckMultiplayerButtonState()
	{
		if (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown)
		{
			RefreshMultiplayerButton();
		}
	}

	protected override bool CheckMultiplayerButton(out string tooltip)
	{
		if (!base.CheckMultiplayerButton(out tooltip))
		{
			return false;
		}
		if (Choreographer.IsMultiplayerDisabled())
		{
			tooltip = LocalizationManager.GetTranslation("FH/GUI_START_MULTIPLAYER_DISABLED_BY_CAMPAIGN_TOOLTIP");
			return false;
		}
		if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
		{
			tooltip = null;
			return false;
		}
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			tooltip = LocalizationManager.GetTranslation("GUI_START_MULTIPLAYER_WRONG_PHASE_TOOLTIP");
			return false;
		}
		tooltip = null;
		return true;
	}

	public override void LoadMainMenu(bool skipConfirmation = false)
	{
		base.LoadMainMenu(AutoTestController.s_AutoTestCurrentlyLoaded | skipConfirmation);
	}

	private void LoadLevelEditor()
	{
		switch (LevelEditorController.s_Instance.CurrentState)
		{
		case LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty:
		case LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel:
			StartCoroutine(BackToLevelEditorCrt());
			break;
		case LevelEditorController.ELevelEditorState.Idle:
		case LevelEditorController.ELevelEditorState.Editing:
			break;
		}
	}

	private IEnumerator BackToLevelEditorCrt()
	{
		yield return SceneController.Instance.EndScenarioSafely();
		CCustomLevelData cCustomLevelData = SaveData.Instance.Global.CurrentEditorLevelData ?? SaveData.Instance.Global.CurrentCustomLevelData;
		string levelName = cCustomLevelData.Name.ToLowerInvariant();
		LevelEditorDataManager levelEditorDataManager = SaveData.Instance.LevelEditorDataManager;
		levelEditorDataManager.DetermineAvailableFilesFromLoadFolder(SaveData.Instance.Global.UseCustomLevelDataFolder);
		FileInfo fi = levelEditorDataManager.AvailableFiles.FirstOrDefault((FileInfo fileInfo) => fileInfo.Name.ToLowerInvariant().Contains(levelName));
		if (fi == null)
		{
			throw new ApplicationException("Cannot find level that contains '" + levelName + "' in its name");
		}
		Task<CCustomLevelData> levelDataTask = levelEditorDataManager.GetLevelDataForFile(fi.FullName);
		yield return new WaitForTask(levelDataTask);
		if (levelDataTask.HasFailed(out var _) || levelDataTask.Result == null)
		{
			throw new ApplicationException("Could not load level data for '" + fi.FullName + "'");
		}
		Task task = SaveData.Instance.LoadCustomLevelFromData(levelDataTask.Result, LevelEditorController.ELevelEditorState.Editing);
		yield return new WaitForTask(task);
		LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Campaign;
		SceneController.Instance.LevelEditorStart();
	}

	protected override void OnLoadMainMenuConfirmed()
	{
		if (LevelEventsController.s_EventsControllerActive)
		{
			Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
		}
		base.OnLoadMainMenuConfirmed();
	}

	public void QuitDungeon()
	{
		window.Hide(instant: true);
		Main.Unpause3DWorld(forceUnpause: true);
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_SCENARIO"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_ABANDON_ADVENTURE_SCENARIO"), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, () =>
			{
				Choreographer.s_Choreographer.AbandonScenario();
			});
			return;
		}
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_SCENARIO"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_ABANDON_SINGLE_SCENARIO"), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, () =>
		{
			if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor || SaveData.Instance.Global.CurrentlyPlayingCustomLevel)
			{
				_ = SaveData.Instance.Global.GameMode;
				_ = 3;
				LoadMainMenu(skipConfirmation: true);
			}
			else
			{
				Choreographer.s_Choreographer.AbandonScenario();
			}
		});
	}

	public void RestartScenario()
	{
		if (LevelEventsController.s_EventsControllerActive)
		{
			Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
		}
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_MESSAGE"), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, () =>
		{
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: false);
			Log.SimpleInfo("User restarted the Scenario");
			SceneController.Instance.RestartScenarioFromInitial();
		}, () =>
		{
			restartRoundButton.Deselect();
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
		});
	}

	public void QueueRestartRound(Action onCancel = null)
	{
		if (SaveData.Instance.IsSavingThreadActive)
		{
			return;
		}
		if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
		{
			UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			return;
		}
		bool flag = true;
		if (Choreographer.IsInNoPartyCampaign())
		{
			flag = false;
			FastReload.FastRestartRoundEnabled = false;
		}
		UIConfirmationBoxManager.CheckboxParams checkboxParams = (flag ? new UIConfirmationBoxManager.CheckboxParams
		{
			OnCheckboxToggle = (bool isOn) =>
			{
				FastReload.FastRestartRoundEnabled = isOn;
			},
			StartingValue = FastReload.FastRestartRoundEnabled,
			Text = LocalizationManager.GetTranslation("GUI_CONFIRMATION_FASTRESTART_CHECKBOX")
		} : null);
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_MESSAGE"), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, () =>
		{
			if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
			{
				UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			}
			else
			{
				SceneController.Instance.GlobalErrorMessage.SuppressShowingErrorMessages = false;
				if (LevelEventsController.s_EventsControllerActive)
				{
					Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
				}
				if (Singleton<UIResultsManager>.IsInitialized && Singleton<UIResultsManager>.InstanceFast.IsShown)
				{
					Singleton<UIResultsManager>.InstanceFast.Hide();
					Choreographer.s_Choreographer.RestartRound();
				}
				else
				{
					Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
					Choreographer.s_Choreographer.PostMessageFromUI(new CRestartRound_MessageData());
				}
			}
		}, () =>
		{
			restartRoundButton.Deselect();
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
			onCancel?.Invoke();
		}, enableSoftlockReport: false, checkboxParams);
	}

	public void RestartTurn()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_RESTART_TURN"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_GUI_RESTART_TURN"), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, () =>
		{
			Log.SimpleInfo("User restarted the turn");
		}, () =>
		{
			restartTurnButton.Deselect();
		});
	}

	public void SkipMission()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_SKIP_MISSION"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_SKIP_MISSION"), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, () =>
		{
			Choreographer.s_Choreographer.EndGame(EResult.Win);
		}, () =>
		{
			skipMissionButton.Deselect();
		});
	}

	public void SkipTutorial()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_SKIP_FTUE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_SKIP_FTUE"), UIInfoTools.Instance.Red, UIInfoTools.DefInstance.descriptionTextColor, () =>
		{
			Choreographer.s_Choreographer.EndGameAndSkipTutorial();
		}, () =>
		{
			skipTutorialButton.Deselect();
		});
	}

	public void OnUserConnected(NetworkBackendPlayer connection)
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			UpdateButtonStates();
		}
	}

	public void OnUserReadyForAssignment()
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			UpdateButtonStates();
		}
	}
}
