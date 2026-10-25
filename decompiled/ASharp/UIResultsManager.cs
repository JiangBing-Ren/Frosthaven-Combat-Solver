using System;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FFSNet;
using FHGUI.Elements;
using MEC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIResultsManager : Singleton<UIResultsManager>
{
	public delegate void EndGameCallback(EResult result);

	public enum ERetryOptionType
	{
		None,
		Only,
		Additional
	}

	[SerializeField]
	protected UIWindow window;

	[SerializeField]
	protected RectTransform ButtonsBarContainer;

	[SerializeField]
	private GameObject[] HideElements;

	[SerializeField]
	public GameObject ForwardButtonPrototype;

	[SerializeField]
	public GameObject BackButtonPrototype;

	[SerializeField]
	public GameObject CenterButtonPrototype;

	protected EndGameCallback m_EndGameCallback;

	protected EResult m_Result;

	protected ERetryOptionType m_Retry;

	protected bool m_InCustomLevelMode;

	protected bool m_QueuedShow;

	private CoroutineHandle m_runningCoroutine;

	protected UnityAction m_OnRetry;

	public bool IsShown
	{
		get
		{
			if (!window.IsOpen)
			{
				return m_QueuedShow;
			}
			return true;
		}
	}

	public virtual void Show(float delay = 0f, EndGameCallback onButtonClicked = null, EResult result = EResult.None, ERetryOptionType retryOption = ERetryOptionType.None, UnityAction retryCallback = null)
	{
		SaveData.Instance.Global.CurrentAdventureData?.ScrapeStats(result);
		if (!AutoTestController.s_Instance.AutotestStarted)
		{
			if (FFSNetwork.IsClient)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
			}
			m_EndGameCallback = onButtonClicked;
			m_InCustomLevelMode = SaveData.Instance.Global.CurrentlyPlayingCustomLevel;
			m_Retry = ((retryCallback != null) ? retryOption : ERetryOptionType.None);
			AnalyticsWrapper.LogScreenDisplay(AWScreenName.results_screen);
			m_OnRetry = retryCallback;
			m_Result = result;
			if (!m_QueuedShow && !IsShown)
			{
				m_runningCoroutine = Timing.RunCoroutine(WaitAndShow(delay));
				m_QueuedShow = true;
			}
			ButtonsBarParam[] array = new ButtonsBarParam[2];
			if (m_OnRetry != null)
			{
				array[0] = Button_Back(Retry);
			}
			if (m_EndGameCallback != null)
			{
				array[1] = Button_Continue(OnButtonClicked);
			}
			SetButtonsBar(array);
		}
	}

	protected virtual void Retry()
	{
		m_OnRetry?.Invoke();
	}

	public void MPSessionEndedOnResults()
	{
		ButtonsBarParam[] array = new ButtonsBarParam[2];
		if (m_OnRetry != null)
		{
			array[0] = Button_Back(Retry);
		}
		if (m_EndGameCallback != null)
		{
			array[1] = Button_Continue(OnButtonClicked);
		}
		SetButtonsBar(array);
	}

	protected virtual void OnButtonClicked()
	{
		m_EndGameCallback?.Invoke(m_Result);
		Hide();
	}

	private IEnumerator<float> WaitAndShow(float delay)
	{
		float showTime = Timekeeper.instance.m_GlobalClock.time + delay;
		while (Timekeeper.instance.m_GlobalClock.time < showTime)
		{
			yield return 0f;
		}
		Show();
	}

	public void SetButtonsBar(params ButtonsBarParam[] buttonParams)
	{
		ClearButtonBar();
		if (buttonParams.Count((ButtonsBarParam p) => p.IsValid) == 1)
		{
			for (int num = 0; num < buttonParams.Length; num++)
			{
				if (buttonParams[num].IsValid)
				{
					buttonParams[num].Prototype = CenterButtonPrototype;
				}
			}
		}
		ButtonsBarElement.SetButtons(ButtonsBarContainer, buttonParams);
	}

	protected void ClearButtonBar()
	{
		ButtonsBarElement.ClearButtons(ButtonsBarContainer);
	}

	protected virtual void Show()
	{
		if (!AutoTestController.s_Instance.AutotestStarted)
		{
			m_QueuedShow = false;
			window.Show();
			GameObject[] hideElements = HideElements;
			for (int i = 0; i < hideElements.Length; i++)
			{
				hideElements[i].SetActive(value: false);
			}
		}
	}

	public virtual void Hide()
	{
		window.Hide();
		GameObject[] hideElements = HideElements;
		for (int i = 0; i < hideElements.Length; i++)
		{
			hideElements[i].SetActive(value: true);
		}
	}

	protected override void AwakenedDestroy()
	{
		Timing.Instance.KillCoroutinesOnInstance(m_runningCoroutine);
	}

	protected ButtonsBarParam Button_Back(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_RESULTS_REPLAY", InputFHManager.Controls.Generic.Back, callback);
		result.NonInteractable = !interactable;
		result.Prototype = BackButtonPrototype;
		return result;
	}

	protected ButtonsBarParam Button_Continue(Action callback, bool interactable = true)
	{
		ButtonsBarParam result = new ButtonsBarParam("GUI_RESULTS_TOOUTPOST", InputFHManager.Controls.Generic.Confirm, callback);
		result.NonInteractable = !interactable;
		result.Prototype = ForwardButtonPrototype;
		return result;
	}
}
