using System;
using System.Collections;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionSandbox : MainOptionOpenSuboptions
{
	[SerializeField]
	private SandboxUI sandboxUI;

	[SerializeField]
	private MenuOptionIcon icon;

	public override List<MenuSuboption> BuildOptions()
	{
		return new List<MenuSuboption>
		{
			new MenuSuboption("GUI_ADVENTURE2", icon, () =>
			{
				SceneController.Instance.StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign));
			}),
			new MenuSuboption("GUI_CAMPAIGN", icon, () =>
			{
				SceneController.Instance.StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign));
			})
		};
	}

	private IEnumerator LoadYMLAndSetMenu(ScenarioManager.EDLLMode mode)
	{
		SceneController.Instance.ShowLoadingScreen();
		yield return null;
		MainMenuUIManager.Instance.mainMenu.Hide();
		yield return SceneController.Instance.YML.LoadSingleScenariosAsyncCrt(mode);
		if (SceneController.Instance.YML.LastLoadResult)
		{
			SceneController.Instance.DisableLoadingScreen();
			sandboxUI.gameObject.SetActive(value: true);
		}
		else
		{
			Log.GUIError("Unable to load YML for DLL Mode: " + mode);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
		}
	}
}
