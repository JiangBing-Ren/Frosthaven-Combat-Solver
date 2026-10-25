using System.Collections.Generic;
using FHGUI;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class MainOptionExtras : MainOptionOpenSuboptions
{
	[SerializeField]
	private MenuOptionIcon compendiumIcon;

	[SerializeField]
	private MenuOptionIcon levelEditorIcon;

	[SerializeField]
	private MenuOptionIcon backIcon;

	[SerializeField]
	private UIMainMenuModeSelection levelEditor;

	[SerializeField]
	private MenuOptionIcon moddingIcon;

	[SerializeField]
	private GameObject moddingUI;

	[SerializeField]
	private MenuOptionIcon creditsIcon;

	[SerializeField]
	private UIWindow creditsWindow;

	[SerializeField]
	private MenuOptionIcon introCinematicIcon;

	[SerializeField]
	private UIWindow introCinematicWindow;

	private UIWindow codexWindow;

	private MenuSuboption codexOpt;

	protected override void Awake()
	{
		base.Awake();
		codexWindow = Singleton<FHUICodex>.Instance.GetComponent<UIWindow>();
		codexOpt = new MenuSuboption("GUI_Wiki", compendiumIcon, () =>
		{
			Singleton<FHUICodex>.Instance.Show(null, fromOptionMenu: true);
			codexWindow.onHidden.RemoveListener(OnCloseCodex);
			codexWindow.onHidden.AddListener(OnCloseCodex);
		}, () =>
		{
			codexWindow.Hide();
			codexWindow.onHidden.RemoveListener(OnCloseCodex);
		});
	}

	private void Start()
	{
		creditsWindow.onHidden.AddListener(MainMenuUIManager.Instance.mainMenu.OpenExtras);
	}

	private void OnCloseCodex()
	{
		codexOpt.Deselect();
		codexWindow.onHidden.RemoveListener(OnCloseCodex);
	}

	public override List<MenuSuboption> BuildOptions()
	{
		codexOpt.Reset();
		return new List<MenuSuboption>
		{
			codexOpt,
			new MenuSuboption("GUI_CREDITS", creditsIcon, OpenCredits),
			new MenuSuboption("GUI_OPEN_INTRO_CINEMATIC", introCinematicIcon, OpenIntroCinematic),
			new MenuSuboption("GUI_BACK", backIcon, Button.Deselect)
		};
	}

	private void OpenModding()
	{
		moddingUI.SetActive(value: true);
		MainMenuUIManager.Instance.mainMenu.Hide();
	}

	public void OpenLevelEditor()
	{
		MainMenuUIManager.Instance.mainMenu.Hide();
		levelEditor.ShowForLevelEditor(autoSelectRuleset: true, ScenarioManager.EDLLMode.Campaign);
	}

	private void OpenCredits()
	{
		MainMenuUIManager.Instance.mainMenu.Hide();
		creditsWindow.Show();
	}

	private void OpenIntroCinematic()
	{
		MainMenuUIManager.Instance.mainMenu.Hide();
		introCinematicWindow.Show();
	}
}
