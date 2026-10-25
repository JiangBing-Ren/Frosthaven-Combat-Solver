using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionGameMode : MainOptionOpenSuboptions
{
	[SerializeField]
	private MenuOptionIcon newIcon;

	[SerializeField]
	private MenuOptionIcon continueIcon;

	[SerializeField]
	private MenuOptionIcon loadIcon;

	[SerializeField]
	private MenuOptionIcon backIcon;

	[SerializeField]
	private UILoadGameWindow loadGameWindow;

	[SerializeField]
	private UICreateGameWindow createGameWindow;

	[SerializeField]
	private UIContinueGamePreview continueGamePreview;

	private IGameModeService service;

	private MenuSuboption resumeOpt;

	private MenuSuboption loadOpt;

	private bool _isRefreshCrtRunning;

	protected override void Awake()
	{
		base.Awake();
		service = GetComponent<IGameModeService>();
		service.RegisterToDeletedGame(RefreshOptions);
		resumeOpt = new MenuSuboption("GUI_CONTINUE", continueIcon, service.ResumeLastGame, null, interactable: true, (bool isHovered) =>
		{
			if (isHovered)
			{
				continueGamePreview.Show(service);
			}
			else
			{
				continueGamePreview.Hide();
			}
		});
		loadOpt = new MenuSuboption("GUI_LOAD", loadIcon, () =>
		{
			Singleton<FHUILoadGame>.Instance.Show(service);
			Singleton<FHUILoadGame>.Instance.OnHide.AddListener(() =>
			{
				loadOpt.Deselect();
			});
		}, () =>
		{
			loadGameWindow.Hide();
			Singleton<FHUILoadGame>.Instance.OnHide.RemoveAllListeners();
		});
	}

	private void RefreshOptions()
	{
		loadOpt.IsInteractable = service.CanLoadGames();
		resumeOpt.IsInteractable = false;
		UIManager.Instance.StartCoroutine(RefreshResumeCrt());
	}

	public override List<MenuSuboption> BuildOptions()
	{
		resumeOpt.Reset();
		resumeOpt.IsInteractable = false;
		UIManager.Instance.StartCoroutine(RefreshResumeCrt());
		loadOpt.Reset();
		loadOpt.IsInteractable = service.CanLoadGames();
		MenuSuboption createOpt = null;
		createOpt = new MenuSuboption("GUI_NEW", newIcon, () =>
		{
			createGameWindow.Show(service, createOpt.Deselect);
		}, createGameWindow.Hide);
		MenuSuboption item = new MenuSuboption("GUI_BACK", backIcon, Button.Deselect);
		return new List<MenuSuboption> { resumeOpt, loadOpt, createOpt, item };
	}

	private IEnumerator RefreshResumeCrt()
	{
		if (!_isRefreshCrtRunning)
		{
			_isRefreshCrtRunning = true;
			Task<bool> t = service.CanResume();
			yield return new WaitForTask(t);
			resumeOpt.IsInteractable = t.Result;
			_isRefreshCrtRunning = false;
		}
	}
}
