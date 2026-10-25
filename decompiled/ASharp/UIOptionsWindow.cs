using System;
using System.Collections.Generic;
using FHGUI;
using GLOOM;
using GLOOM.MainMenu;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIOptionsWindow : Singleton<UIOptionsWindow>
{
	[Serializable]
	private class OptionTab
	{
		public UIMainMenuOption OptionToggle;

		public UISubmenuGOWindow TabWindow;
	}

	[SerializeField]
	private UIWindow m_Window;

	[SerializeField]
	private VerticalPointerUI m_VerticalPointer;

	[SerializeField]
	private ToggleGroup m_ToggleGroup;

	[SerializeField]
	private List<OptionTab> m_Tabs;

	[SerializeField]
	private UIMainMenuOption m_HideOption;

	[SerializeField]
	private UIMainMenuOption m_DifficultyOption;

	[SerializeField]
	private UIMainMenuOption m_HouseRulesOption;

	[SerializeField]
	private UIMainMenuOption m_EpicStoreOption;

	[SerializeField]
	private TextLocalizedListener m_EpicStoreOptionText;

	private Action m_OnHidden;

	private INetworkLoginService epicLoginService = new DummyNetworkLoginService(signedIn: true, 5f);

	protected override void AwakeInit()
	{
		m_Window.onShown.AddListener(OnShow);
		m_Window.onHidden.AddListener(OnHidden);
		RefreshLogIn();
		for (int i = 0; i < m_Tabs.Count; i++)
		{
			InitializeOption(m_Tabs[i].OptionToggle, m_Tabs[i].TabWindow);
		}
		m_HideOption.Init(() =>
		{
			Hide();
			m_HideOption.Deselect();
		});
	}

	private void InitializeOption(UIMainMenuOption option, UISubmenuGOWindow tab)
	{
		tab.OnHidden.AddListener(option.Deselect);
		option.Init(() =>
		{
			tab.Show();
		}, () =>
		{
			tab.Hide();
		});
	}

	public void Show(RectTransform fromPointer, Action onHidden = null, bool showHideOption = false)
	{
		m_OnHidden = onHidden;
		m_VerticalPointer.PointAt(fromPointer);
		m_Window.Show();
		m_HideOption.gameObject.SetActive(showHideOption);
	}

	public void Hide()
	{
		m_Window.Hide();
	}

	private void OnShow()
	{
		bool flag = SaveData.Instance?.Global != null && SaveData.Instance.Global.GameMode == EGameMode.Campaign;
		m_DifficultyOption.gameObject.SetActive(flag);
		if (!flag)
		{
			m_Tabs.Find((OptionTab tab) => tab.OptionToggle == m_DifficultyOption)?.TabWindow?.Hide();
		}
		m_HouseRulesOption.gameObject.SetActive(GameSettingsService.CanShowHouseRulesOptions());
		m_EpicStoreOption.gameObject.SetActive(PlatformLayer.UserData.CanLogOutEpicStore());
		RefreshLogIn();
		if (m_HouseRulesOption.gameObject.activeSelf)
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
			{
				m_HouseRulesOption.SetTooltip(enable: false);
			}
			else
			{
				m_HouseRulesOption.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_HOUSE_RULES_SCENARIO_WARNING"));
			}
		}
	}

	private void OnHidden()
	{
		m_OnHidden?.Invoke();
		m_OnHidden = null;
	}

	private void RefreshLogIn()
	{
		if (m_EpicStoreOption.gameObject.activeSelf)
		{
			if (epicLoginService.IsSignedIn())
			{
				m_EpicStoreOptionText.SetParametersFormatted(LocalizationManager.GetTranslation("GUI_LOGOUT"), UIInfoTools.Instance.warningColor.ToHex());
				m_EpicStoreOption.Init(AskLogOut);
			}
			else
			{
				m_EpicStoreOptionText.SetParametersFormatted(LocalizationManager.GetTranslation("GUI_LOGIN"), UIInfoTools.DefInstance.basicTextColor.ToHex());
				m_EpicStoreOption.Init(LogIn);
			}
			m_EpicStoreOption.IsInteractable = !epicLoginService.IsSigningIn() && !epicLoginService.IsSigningOut();
		}
	}

	private void AskLogOut()
	{
		if (!epicLoginService.IsSigningOut())
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_LOG_OUT_EPICSTORE_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_LOG_OUT_EPICSTORE"), LogOut, m_EpicStoreOption.Deselect);
		}
	}

	private void LogOut()
	{
		m_EpicStoreOption.Deselect();
		m_EpicStoreOption.IsInteractable = false;
		epicLoginService.SignOut().Done(() =>
		{
			if (m_Window.IsOpen)
			{
				RefreshLogIn();
			}
		});
	}

	private void LogIn()
	{
		m_EpicStoreOption.Deselect();
		m_EpicStoreOption.IsInteractable = false;
		epicLoginService.SignIn().Done(() =>
		{
			if (m_Window.IsOpen)
			{
				RefreshLogIn();
			}
		});
	}

	protected override void AwakenedDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			Log.GUIError("Options menu can't be destroyed " + transform.parent?.name);
		}
	}
}
