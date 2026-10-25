using System;
using System.Collections;
using Assets.Script.GUI.MainMenu.Modding;
using FFSThreads;
using OxOD;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIModdingWindow : MonoBehaviour
{
	[SerializeField]
	private Button createModButton;

	[SerializeField]
	private Button steamButton;

	[SerializeField]
	private Button directoryButton;

	[SerializeField]
	private ExtendedButton closeButton;

	[SerializeField]
	private TextLocalizedListener subtitle;

	[SerializeField]
	private UICreateModView createModView;

	[SerializeField]
	private UIListModsView listModsView;

	[SerializeField]
	private Image unfocusedMask;

	[SerializeField]
	private FileSelector fileSelector;

	private IModdingService service;

	private Action closeButtonCallback;

	[SerializeField]
	private UnityEvent OnClosed = new UnityEvent();

	private void Awake()
	{
		createModView.OnConfirmedModEvent.AddListener(CreateMod);
		createModView.OnHidden.AddListener(() =>
		{
			FocusOptions(focus: true);
		});
		closeButton.onClick.AddListener(() =>
		{
			closeButtonCallback?.Invoke();
		});
		createModButton.onClick.AddListener(OpenCreateModView);
		steamButton.onClick.AddListener(OpenSteamWorkshop);
		directoryButton.onClick.AddListener(ChangeModdingDirectory);
		service = SceneController.Instance.ModService;
		listModsView.Setup(SceneController.Instance.ModService);
	}

	public void Show(bool reloadMods)
	{
		if (reloadMods && service != null)
		{
			listModsView.ReloadMods();
		}
		gameObject.SetActive(value: true);
		OpenListModsView();
	}

	public void Show()
	{
		Show(reloadMods: false);
	}

	private void Hide()
	{
		if (gameObject.activeSelf)
		{
			gameObject.SetActive(value: false);
			createModView.Hide();
			OnClosed.Invoke();
		}
	}

	private void FocusOptions(bool focus)
	{
		unfocusedMask.gameObject.SetActive(!focus);
	}

	private void OpenListModsView(IMod newMod = null)
	{
		subtitle.SetTextKey("GUI_MODDING_LIST_VIEW_TITLE");
		FocusOptions(focus: true);
		closeButton.TextLanguageKey = "GUI_BACK";
		closeButtonCallback = Hide;
		createModView.Hide();
		listModsView.Show(newMod);
	}

	private void OpenCreateModView()
	{
		subtitle.SetTextKey("GUI_MODDING_CREATE_VIEW_TITLE");
		FocusOptions(focus: false);
		closeButton.TextLanguageKey = "GUI_CANCEL";
		closeButtonCallback = () =>
		{
			OpenListModsView();
		};
		listModsView.Hide();
		createModView.Show();
	}

	private void OpenSteamWorkshop()
	{
		service.OpenSteam();
	}

	public void CreateMod(ModDataView modData)
	{
		IMod mod = service.CreateMod(modData);
		if (mod != null)
		{
			OpenListModsView(mod);
			UIModdingNotifications.ShowCreatedModNotification(mod.Name);
		}
	}

	public void ChangeModdingDirectory()
	{
		StartCoroutine(SelectDirectoryCoroutine());
	}

	private IEnumerator SelectDirectoryCoroutine()
	{
		fileSelector.result = null;
		fileSelector.saveLastPath = false;
		FileDialog.FileDialogMode defMode = fileSelector.mode;
		fileSelector.mode = FileDialog.FileDialogMode.FolderSelector;
		yield return fileSelector.Select(fileSelector.defaultPath);
		if (fileSelector.result != null)
		{
			SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.ModLoadingWithProgress);
			SceneController.Instance.ShowLoadingScreen();
			ThreadMessageReceiver threadReceiver = new ThreadMessageReceiver();
			threadReceiver.StartMessageProcessing();
			yield return null;
			while (threadReceiver.IsBusy)
			{
				yield return null;
			}
			threadReceiver.StopMessageProcessing();
			SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			SceneController.Instance.DisableLoadingScreen();
			fileSelector.mode = defMode;
			fileSelector.saveLastPath = true;
		}
	}
}
