using System;
using System.Collections.Generic;
using MapRuleLibrary.Adventure;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UICreateGameWindow : MonoBehaviour
{
	[SerializeField]
	private List<UICreateGameStep> steps;

	[SerializeField]
	private GUIAnimator openAnimator;

	[SerializeField]
	private UIWindow window;

	private int currentStep;

	private IGameModeService service;

	private GameData data;

	private Action onFinished;

	private bool isOpen;

	private void GoBack()
	{
		if (currentStep == 0)
		{
			Hide();
		}
		else
		{
			OpenStep(currentStep - 1, instant: true);
		}
	}

	private void OpenStep(int step, bool instant)
	{
		currentStep = step;
		steps[currentStep].Show(service, data, instant).Done(GoNext, GoBack);
	}

	private void GoNext()
	{
		if (currentStep == steps.Count - 1)
		{
			Create();
			return;
		}
		steps[currentStep].Hide();
		OpenStep(currentStep + 1, instant: false);
	}

	public void Show(IGameModeService service, Action onFinished)
	{
		this.service = service;
		this.onFinished = onFinished;
		data = new GameData();
		if (service is CampaignModeService)
		{
			data.Difficulty = EAdventureDifficulty.Normal;
		}
		for (int i = 1; i < steps.Count; i++)
		{
			steps[i].Hide();
		}
		OpenStep(0, instant: false);
		isOpen = true;
		if ((bool)window)
		{
			window.Show();
		}
		openAnimator.Stop();
		openAnimator.Play();
	}

	public void Hide()
	{
		if (isOpen)
		{
			isOpen = false;
			service = null;
			data = null;
			openAnimator.Stop();
			for (int i = 0; i < steps.Count; i++)
			{
				steps[i].Hide();
			}
			if ((bool)window)
			{
				window.Hide();
			}
			onFinished?.Invoke();
		}
	}

	private void OnDisable()
	{
		openAnimator.Stop();
	}

	private void Create()
	{
		service.CreateGame(data);
		Hide();
	}
}
