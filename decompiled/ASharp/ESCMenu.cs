using FH.GraphicalUserInterface.MainMenu;
using FHGUI;
using GLOOM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ESCMenu : Singleton<ESCMenu>
{
	[SerializeField]
	protected UIWindow window;

	[SerializeField]
	protected ToggleGroup toggleGroup;

	[SerializeField]
	protected UIMainMenuOption continueButton;

	[SerializeField]
	protected UIMainMenuOption mainMenuButton;

	[SerializeField]
	protected UIMainMenuOption multiplayerButton;

	[SerializeField]
	protected UIMainMenuOption exitButton;

	[SerializeField]
	protected UIMainMenuOption compendiumButton;

	[SerializeField]
	protected UIMainMenuOption saveButton;

	[SerializeField]
	protected UIMainMenuOption loadButton;

	[SerializeField]
	protected UIMainMenuOption optionsButton;

	[SerializeField]
	protected GameObject onlineCheckbox;

	[SerializeField]
	[TextArea]
	private string multiplayerLockedTooltipFormat = "<sprite name=\"Lock_Icon_White\"><indent=30><font=\"MarcellusSC-Regular SDF\"><color=#EACF8CFF>{0}</color></font>\n{1}";

	public bool IsOpen => window.IsOpen;

	public UnityEvent OnShown => window.onShown;

	public UnityEvent OnHidden => window.onHidden;

	protected override void AwakeInit()
	{
		window.onTransitionBegin.AddListener(OnTransitionBegin);
		continueButton.Init(Hide);
		mainMenuButton.Init(() =>
		{
			LoadMainMenu();
		});
		exitButton.Init(ExitGame, () =>
		{
			SetFocused(isFocused: true);
		});
		if (Singleton<FHUICodex>.Instance != null)
		{
			UIWindow compendiumWindow = Singleton<FHUICodex>.Instance.GetComponent<UIWindow>();
			compendiumButton.Init(() =>
			{
				compendiumWindow.onHidden.RemoveListener(OnHideOptionWindow);
				compendiumWindow.onHidden.AddListener(OnHideOptionWindow);
				Singleton<FHUICodex>.Instance.Show(null, fromOptionMenu: true);
			}, () =>
			{
				compendiumWindow.onHidden.RemoveListener(OnHideOptionWindow);
				compendiumWindow.Hide();
			});
		}
		if (Singleton<FHUILoadGame>.Instance != null)
		{
			UIWindow loadWindow = Singleton<FHUILoadGame>.Instance.GetComponent<UIWindow>();
			loadButton.Init(() =>
			{
				loadWindow.onHidden.RemoveListener(OnHideOptionWindow);
				loadWindow.onHidden.AddListener(OnHideOptionWindow);
				Singleton<FHUILoadGame>.Instance.Show(GetComponent<CampaignModeService>());
			}, () =>
			{
				loadWindow.onHidden.RemoveListener(OnHideOptionWindow);
				loadWindow.Hide();
			});
		}
		if (Singleton<FHUISaveGame>.Instance != null)
		{
			UIWindow saveWindow = Singleton<FHUISaveGame>.Instance.GetComponent<UIWindow>();
			saveButton.Init(() =>
			{
				saveWindow.onHidden.RemoveListener(OnHideOptionWindow);
				saveWindow.onHidden.AddListener(OnHideOptionWindow);
				Singleton<FHUISaveGame>.Instance.Show(GetComponent<CampaignModeService>());
			}, () =>
			{
				saveWindow.onHidden.RemoveListener(OnHideOptionWindow);
				saveWindow.Hide();
			});
		}
		optionsButton.Init(() =>
		{
			SetFocused(isFocused: false);
			Singleton<UIOptionsWindow>.Instance.Show(optionsButton.transform as RectTransform, optionsButton.Deselect);
		}, () =>
		{
			Singleton<UIOptionsWindow>.Instance.Hide();
			SetFocused(isFocused: true);
		});
		multiplayerButton.Init(() =>
		{
			SetFocused(isFocused: false);
			Singleton<UIMultiplayerEscSubmenu>.Instance.OnHide.RemoveListener(OnHideOptionWindow);
			Singleton<UIMultiplayerEscSubmenu>.Instance.OnHide.AddListener(OnHideOptionWindow);
			if (Singleton<UIMultiplayerEscSubmenu>.Instance.IsHosting)
			{
				Singleton<UIMultiplayerEscSubmenu>.Instance.Show();
			}
			else
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOSTGAME_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HOSTGAME_CONFIRMATION_DESC"), () =>
				{
					Singleton<UIMultiplayerEscSubmenu>.Instance.StartSession();
					Singleton<UIMultiplayerEscSubmenu>.Instance.Show();
				});
			}
		}, () =>
		{
			Singleton<UIMultiplayerEscSubmenu>.Instance.OnHide.RemoveListener(OnHideOptionWindow);
			Singleton<UIMultiplayerEscSubmenu>.Instance.Hide();
			SetFocused(isFocused: true);
		});
	}

	private void OnHideOptionWindow()
	{
		toggleGroup.SetAllTogglesOff();
	}

	protected virtual void SetFocused(bool isFocused)
	{
		continueButton.SetFocused(isFocused);
		multiplayerButton.SetFocused(isFocused);
		compendiumButton.SetFocused(isFocused);
		exitButton.SetFocused(isFocused);
		optionsButton.SetFocused(isFocused);
		mainMenuButton.SetFocused(isFocused);
	}

	protected void OnTransitionBegin(UIWindow window, UIWindow.VisualState state, bool instant)
	{
		if (state == UIWindow.VisualState.Shown)
		{
			OnShow();
		}
		else
		{
			OnHide();
		}
	}

	protected virtual void OnShow()
	{
		SetFocused(isFocused: true);
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.pause);
		if (!FFSNetwork.IsOnline)
		{
			OnSwitchedToSinglePlayer();
		}
		else
		{
			OnSwitchedToMultiplayer();
		}
		RefreshMultiplayerButton();
	}

	protected void RefreshMultiplayerButton()
	{
		bool isInteractable = CheckMultiplayerButton(out var tooltip);
		multiplayerButton.IsInteractable = isInteractable;
		multiplayerButton.SetTooltip(tooltip.IsNOTNullOrEmpty(), tooltip);
		loadButton.IsInteractable = SaveData.Instance?.CanLoad() ?? false;
		saveButton.IsInteractable = SaveData.Instance?.CanSave() ?? false;
	}

	protected virtual bool CheckMultiplayerButton(out string tooltip)
	{
		if ((bool)Singleton<FHMapChoreographer>.InstanceFast)
		{
			tooltip = null;
			if (PlatformLayer.UserData.IsSignedIn && (!FFSNetwork.IsOnline || FFSNetwork.IsHost))
			{
				return !Singleton<FHMapChoreographer>.InstanceFast.View.IsReadyCheckRunning;
			}
			return false;
		}
		tooltip = null;
		if (PlatformLayer.UserData.IsSignedIn)
		{
			if (FFSNetwork.IsOnline)
			{
				return FFSNetwork.IsHost;
			}
			return true;
		}
		return false;
	}

	private void OnSwitchedToMultiplayer()
	{
		if (FFSNetwork.Manager != null)
		{
			FFSNetwork.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
		}
		onlineCheckbox.SetActive(value: true);
		RefreshMultiplayerButton();
	}

	private void OnSwitchedToSinglePlayer()
	{
		if (FFSNetwork.Manager != null)
		{
			FFSNetwork.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
			FFSNetwork.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		}
		onlineCheckbox.SetActive(value: false);
		RefreshMultiplayerButton();
	}

	protected virtual void OnHide()
	{
		toggleGroup.SetAllTogglesOff();
		if (FFSNetwork.Manager != null)
		{
			FFSNetwork.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		}
	}

	protected override void AwakenedDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			OnHide();
			multiplayerButton.Deselect();
			compendiumButton.Deselect();
			optionsButton.Deselect();
		}
	}

	public void Show()
	{
		window.Show();
	}

	public void Hide()
	{
		window.Hide();
	}

	public virtual void LoadMainMenu(bool skipConfirmation = false)
	{
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.s_Instance.EndAutotestsWithoutEval();
		}
		MainMenuUIAutomation.Reset();
		window.Hide(instant: true);
		Main.Unpause3DWorld(forceUnpause: true);
		if (skipConfirmation)
		{
			OnLoadMainMenuConfirmed();
			return;
		}
		bool flag = false;
		bool flag2 = FFSNetwork.IsOnline & flag;
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Map)
		{
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
			{
				LevelEditorController s_Instance = LevelEditorController.s_Instance;
				if ((object)s_Instance != null && !s_Instance.IsEditing)
				{
					goto IL_0076;
				}
			}
			OnLoadMainMenuConfirmed();
			return;
		}
		goto IL_0076;
		IL_0076:
		UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
		string translation = LocalizationManager.GetTranslation("GUI_CONFIRMATION_RETURN_MAINMENU");
		string translation2 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_RETURN_MAINMENU_PROGRESS_LOSS");
		UnityAction onActionConfirmed = OnLoadMainMenuConfirmed;
		bool enableSoftlockReport = flag2;
		instance.ShowGenericConfirmation(translation, translation2, onActionConfirmed, null, enableSoftlockReport);
	}

	protected virtual async void OnLoadMainMenuConfirmed()
	{
		if ((LevelEditorController.s_Instance?.IsEditing ?? false) || (LevelEditorController.s_Instance?.IsPreviewingLevel ?? false))
		{
			LevelEditorController.s_Instance?.ExitToMainMenuThroughUI();
		}
		if (Singleton<FHMapChoreographer>.InstanceFast != null)
		{
			await Singleton<FHMapChoreographer>.InstanceFast.EndGame();
		}
		UIManager.LoadMainMenu();
	}

	public void ExitGame()
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
			string translation = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_TITLE");
			string translation2 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_MAP");
			UnityAction onActionConfirmed = ApplicationQuit;
			UnityAction onActionCancelled = exitButton.Deselect;
			bool isOnline = FFSNetwork.IsOnline;
			instance.ShowGenericConfirmation(translation, translation2, onActionConfirmed, onActionCancelled, null, null, null, showHeader: true, isOnline);
			return;
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			LevelEditorController s_Instance = LevelEditorController.s_Instance;
			if ((object)s_Instance != null && !s_Instance.IsEditing)
			{
				UIConfirmationBoxManager instance2 = Singleton<UIConfirmationBoxManager>.Instance;
				string translation3 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_TITLE");
				string translation4 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_SCENARIO");
				UnityAction onActionConfirmed2 = ApplicationQuit;
				UnityAction onActionCancelled2 = exitButton.Deselect;
				bool isOnline = FFSNetwork.IsOnline;
				instance2.ShowGenericConfirmation(translation3, translation4, onActionConfirmed2, onActionCancelled2, null, null, null, showHeader: true, isOnline);
				return;
			}
		}
		ApplicationQuit();
	}

	private static void ApplicationQuit()
	{
		Application.Quit();
	}
}
