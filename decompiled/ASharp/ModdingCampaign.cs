using System;
using System.Collections;
using GLOOM.MainMenu;
using UnityEngine;

public class ModdingCampaign : MonoBehaviour
{
	public GameObject ResumeButton;

	public GameObject LoadCampaignButton;

	public CampaignModeService service;

	public UICreateGameWindow CreateGameWindow;

	private IEnumerator InitCoroutine()
	{
		if (!SceneController.Instance.YML.LastLoadResult)
		{
			yield break;
		}
		try
		{
			bool active = false;
			ResumeButton.SetActive(active);
			LoadCampaignButton.SetActive(value: false);
			SceneController.Instance.DisableLoadingScreen();
			gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "Unable to load Modded Ruleset Menu");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	public void ResumeAdventure()
	{
		service.ResumeLastGame();
	}

	public void LoadAdventure()
	{
		gameObject.SetActive(value: false);
		Singleton<UILoadGameWindow>.Instance.Show(service, () =>
		{
			gameObject.SetActive(value: true);
		});
	}

	public void CreateAdventure()
	{
		gameObject.SetActive(value: false);
		CreateGameWindow.Show(service, () =>
		{
			gameObject.SetActive(value: true);
		});
	}
}
