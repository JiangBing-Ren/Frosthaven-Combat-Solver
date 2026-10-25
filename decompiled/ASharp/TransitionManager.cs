using System;
using System.Collections;
using DG.Tweening;
using Libs.GameConsole;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
	public static TransitionManager s_Instance;

	[SerializeField]
	private RectTransform m_FadeInPanel;

	[SerializeField]
	[Tooltip("Min amount of frames we wait before start transitions, it's needed to awoid hitches during scene load")]
	private float m_MinTransitionFrames;

	[SerializeField]
	private float m_TransitionTime;

	[SerializeField]
	private float m_TransitionDelay;

	private Graphic fadeGraphic;

	[ConsoleVariable(Alias = "no_scenario_fade")]
	public static bool SkipScenarioFade_kvar;

	public bool TransitionDone { get; private set; }

	private event BasicEventHandler onTransitionFinished;

	private void Awake()
	{
		s_Instance = this;
		fadeGraphic = m_FadeInPanel.GetComponent<Graphic>();
	}

	private void OnDestroy()
	{
		s_Instance = null;
		onTransitionFinished = null;
	}

	public void FadeInLevel()
	{
		if (LevelEditorController.s_Instance.IsEditing || AutoTestController.s_AutoTestCurrentlyLoaded || SkipScenarioFade_kvar)
		{
			m_FadeInPanel.gameObject.SetActive(value: false);
			TransitionDone = true;
			onTransitionFinished?.Invoke();
			onTransitionFinished = null;
		}
		else
		{
			TransitionDone = false;
			StartCoroutine(FadeOutCoroutine());
		}
	}

	private IEnumerator FadeOutCoroutine()
	{
		m_FadeInPanel.gameObject.SetActive(value: true);
		WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
		for (int i = 0; (float)i < m_MinTransitionFrames; i++)
		{
			yield return waitForEndOfFrameDelay;
		}
		m_FadeInPanel.GetComponent<Image>().DOFade(0f, m_TransitionTime).SetEase(Ease.InQuart)
			.SetDelay(m_TransitionDelay)
			.SetUpdate(isIndependentUpdate: true)
			.OnComplete(() =>
			{
				TransitionDone = true;
				m_FadeInPanel.gameObject.SetActive(value: false);
				onTransitionFinished?.Invoke();
				onTransitionFinished = null;
			});
	}

	public void FadeIn(Action onFinished)
	{
		TransitionDone = false;
		StartCoroutine(FadeInCoroutine(onFinished));
	}

	private IEnumerator FadeInCoroutine(Action onFinished)
	{
		m_FadeInPanel.gameObject.SetActive(value: true);
		WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
		for (int i = 0; (float)i < m_MinTransitionFrames; i++)
		{
			yield return waitForEndOfFrameDelay;
		}
		fadeGraphic.SetAlpha(0f);
		m_FadeInPanel.GetComponent<Image>().DOFade(1f, m_TransitionTime).SetEase(Ease.InQuart)
			.SetDelay(m_TransitionDelay)
			.SetUpdate(isIndependentUpdate: true)
			.OnComplete(() =>
			{
				TransitionDone = true;
				onTransitionFinished?.Invoke();
				onTransitionFinished = null;
				onFinished?.Invoke();
			});
	}

	public void RegisterCallbackOnTransitionFinished(Action callback)
	{
		if (TransitionDone)
		{
			callback();
		}
		else
		{
			onTransitionFinished += callback.Invoke;
		}
	}

	public void SetFade(float fade)
	{
		fadeGraphic.SetAlpha(fade);
		m_FadeInPanel.gameObject.SetActive(fade > 0f);
	}

	public void Fade(Action onFinished, float duration, float from, float to)
	{
		m_FadeInPanel.gameObject.SetActive(value: true);
		fadeGraphic.SetAlpha(from);
		m_FadeInPanel.GetComponent<Image>().DOFade(to, duration).SetUpdate(isIndependentUpdate: true)
			.OnComplete(() =>
			{
				onFinished();
			});
	}
}
