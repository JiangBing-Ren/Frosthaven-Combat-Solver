using System;
using System.Collections;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using UnityEngine;

public class UIMainMenuModeSelection : MonoBehaviour
{
	private enum EModeSelection
	{
		None,
		Sandbox,
		LevelEditor
	}

	[SerializeField]
	private GameObject CampaignButton;

	[SerializeField]
	private GameObject TitleSandbox;

	[SerializeField]
	private GameObject TitleLevelEditor;

	[ReadOnlyField]
	public bool BusyLoading;

	private EModeSelection m_Mode;

	private Action m_OnModeLoadSuccessCallback;

	private void OnEnable()
	{
		CampaignButton.SetActive(value: true);
	}

	public void SetModeSuccessfullyLoadedCallback(Action callbackToSet)
	{
		m_OnModeLoadSuccessCallback = callbackToSet;
	}

	public void ShowForSandbox()
	{
		MainMenuUIManager.Instance.mainMenu.Hide();
		m_Mode = EModeSelection.Sandbox;
		gameObject.SetActive(value: true);
	}

	public void ShowForLevelEditor()
	{
		ShowForLevelEditor(autoSelectRuleset: false, ScenarioManager.EDLLMode.None);
	}

	public void ShowForLevelEditor(bool autoSelectRuleset, ScenarioManager.EDLLMode rulesetToLoad)
	{
		m_Mode = EModeSelection.LevelEditor;
		MainMenuUIManager.Instance.mainMenu.Hide();
		gameObject.SetActive(value: true);
		if (autoSelectRuleset)
		{
			if (rulesetToLoad == ScenarioManager.EDLLMode.Campaign)
			{
				OnCampaignRulesetClick();
				return;
			}
			Log.GUIInfoFormat("Unable to autoload into {0} ruleset, it is currently unsupported", rulesetToLoad.ToString());
		}
	}

	public void OnCampaignRulesetClick()
	{
		switch (m_Mode)
		{
		case EModeSelection.Sandbox:
			StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign, gameObject, TitleSandbox));
			break;
		case EModeSelection.LevelEditor:
			LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Campaign;
			StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign, gameObject, TitleLevelEditor));
			break;
		default:
			Log.GUIError("Invalid mode " + m_Mode.ToString() + " sent to OnClick");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
			break;
		}
	}

	public void OnGuildmasterRulesetClick()
	{
		throw new Exception("This method is obsolete and should not be called");
	}

	public void OnBackClick()
	{
		gameObject.SetActive(value: false);
		switch (m_Mode)
		{
		case EModeSelection.Sandbox:
			MainMenuUIManager.Instance.mainMenu.Show(instant: true);
			break;
		case EModeSelection.LevelEditor:
			MainMenuUIManager.Instance.mainMenu.OpenExtras();
			break;
		default:
			Log.GUIError("Invalid mode " + m_Mode.ToString() + " sent to OnClick");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
			break;
		}
		m_Mode = EModeSelection.None;
		m_OnModeLoadSuccessCallback = null;
	}

	public IEnumerator LoadYMLAndSetMenu()
	{
		yield return LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign, gameObject, TitleLevelEditor);
	}

	private IEnumerator LoadYMLAndSetMenu(ScenarioManager.EDLLMode mode, GameObject setInactive, GameObject setActive)
	{
		SceneController.Instance.ShowLoadingScreen();
		BusyLoading = true;
		yield return null;
		yield return SceneController.Instance.YML.LoadSingleScenariosAsyncCrt(mode);
		BusyLoading = false;
		if (SceneController.Instance.YML.LastLoadResult)
		{
			setInactive.SetActive(value: false);
			setActive.SetActive(value: true);
			SceneController.Instance.DisableLoadingScreen();
			m_OnModeLoadSuccessCallback?.Invoke();
		}
		else
		{
			Log.GUIError("Unable to load YML for DLL Mode: " + mode);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
		}
		m_OnModeLoadSuccessCallback = null;
	}
}
