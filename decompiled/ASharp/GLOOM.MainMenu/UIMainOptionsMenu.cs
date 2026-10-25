using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIMainOptionsMenu : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	protected CanvasGroup canvasGroupInteraction;

	[SerializeField]
	private UIMainMenuSubptionsPanel suboptionsMenu;

	[SerializeField]
	private GUIAnimator showAnimator;

	[Header("Options")]
	[SerializeField]
	private MainOptionOpenSuboptions sandboxButton;

	[SerializeField]
	private MainOptionOpenSuboptions fhCampaignButton;

	[SerializeField]
	private MainOptionMultiplayer multiplayerButton;

	[SerializeField]
	private MainOptionOpenSuboptions extraButton;

	[SerializeField]
	private MainOption optionsButton;

	[SerializeField]
	private UIMainMenuOption exitButton;

	private List<UIMainMenuOption> menuOptions = new List<UIMainMenuOption>();

	private void Awake()
	{
		showAnimator.OnAnimationStarted.AddListener(() =>
		{
			SetInteractable(interactable: false);
		});
		showAnimator.OnAnimationFinished.AddListener(() =>
		{
			SetInteractable(interactable: true);
		});
		showAnimator.OnAnimationStopped.AddListener(() =>
		{
			SetInteractable(interactable: true);
		});
	}

	private void Start()
	{
		InitializeOption(fhCampaignButton);
		InitializeOption(multiplayerButton);
		InitializeOption(extraButton);
		InitializeButton(exitButton, Application.Quit);
		InitializeOption(optionsButton);
		sandboxButton.gameObject.SetActive(value: false);
	}

	private void InitializeOption(MainOptionOpenSuboptions option, int autoSelectOptionIdx)
	{
		if (autoSelectOptionIdx == -1)
		{
			InitializeOption(option);
			return;
		}
		UIMainMenuOption button = option.Button;
		menuOptions.Add(button);
		button.Init(() =>
		{
			option.BuildOptions()[autoSelectOptionIdx].OnSelected();
			option.Button.SetFocused(isFocused: false);
			option.Button.Deselect();
		});
	}

	private void InitializeOption(MainOptionOpenSuboptions option)
	{
		InitializeButton(option.Button, () =>
		{
			suboptionsMenu.Show(option.BuildOptions(), option.Button.transform as RectTransform, option.Button.Deselect);
		}, suboptionsMenu.Hide);
	}

	private void InitializeOption(MainOption option)
	{
		InitializeButton(option.Button, option.Select, option.Deselect);
	}

	private void InitializeButton(UIMainMenuOption button, Action onSelected, Action onDeselected = null)
	{
		menuOptions.Add(button);
		button.Init(() =>
		{
			Hide(instant: true, hideSuboptions: false);
			SetFocused(isFocused: false);
			onSelected?.Invoke();
		}, () =>
		{
			onDeselected?.Invoke();
			SetFocused(isFocused: true);
			Show();
		});
	}

	private void SetFocused(bool isFocused)
	{
		fhCampaignButton.SetFocused(isFocused);
		extraButton.SetFocused(isFocused);
		exitButton.SetFocused(isFocused);
		optionsButton.SetFocused(isFocused);
		multiplayerButton.SetFocused(isFocused);
		if (sandboxButton.gameObject.activeSelf)
		{
			sandboxButton.SetFocused(isFocused);
		}
	}

	public void SetInteractable(bool interactable)
	{
		if (window.IsOpen)
		{
			canvasGroupInteraction.blocksRaycasts = interactable;
		}
	}

	public void Show(bool instant = false)
	{
		showAnimator.Stop();
		SetFocused(isFocused: true);
		window.Show(instant);
		if (Singleton<PromotionDLCManager>.InstanceFast != null)
		{
			Singleton<PromotionDLCManager>.InstanceFast.Show();
		}
		if (instant)
		{
			showAnimator.GoToFinishState();
		}
		else
		{
			showAnimator.Play();
		}
	}

	public void Hide()
	{
		Hide(instant: true);
	}

	public void Hide(bool instant, bool hideSuboptions = true)
	{
		SetFocused(isFocused: false);
		window.Hide(instant);
		if (hideSuboptions)
		{
			suboptionsMenu.Hide();
		}
		if (Singleton<PromotionDLCManager>.InstanceFast != null)
		{
			Singleton<PromotionDLCManager>.InstanceFast.Hide();
		}
	}

	public void OpenJoinMultiplayer()
	{
		Show(instant: true);
		multiplayerButton.SelectJoinMultiplayer();
	}

	public void BackFromLevelEditor()
	{
		if (ObjectPool.instance != null && !ObjectPool.KeepMonsterCardsLoaded)
		{
			ObjectPool.ClearAllMonsterCards();
		}
		OpenExtras();
	}

	public void OpenExtras()
	{
		Show(instant: true);
		extraButton.Select();
	}

	public void BackFromSandbox()
	{
		if (ObjectPool.instance != null && !ObjectPool.KeepMonsterCardsLoaded)
		{
			ObjectPool.ClearAllMonsterCards();
		}
		Show(instant: true);
	}
}
