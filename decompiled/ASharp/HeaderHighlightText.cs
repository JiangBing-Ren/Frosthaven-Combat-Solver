using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeaderHighlightText : HeaderHighlight
{
	[SerializeField]
	private List<TextLocalizedListener> textListeners;

	[SerializeField]
	private List<TextMeshProUGUI> texts;

	private Action onFinishCallback;

	private bool empty;

	private void Awake()
	{
		highlightAnimator.OnAnimationFinished.AddListener(OnFinished);
	}

	public void SetWarningText(string textKey)
	{
		highlightAnimator.Stop();
		highlightAnimator.GoInitState();
		PrepareWarningHighlight();
		SetText(textKey);
	}

	public void SetHighlightKey(string textKey)
	{
		highlightAnimator.Stop();
		highlightAnimator.GoInitState();
		PrepareHighlight();
		SetText(textKey);
	}

	private void SetText(string textKey)
	{
		empty = string.IsNullOrEmpty(textKey);
		for (int i = 0; i < textListeners.Count; i++)
		{
			textListeners[i].SetTextKey(textKey);
		}
		for (int j = 0; j < texts.Count; j++)
		{
			texts[j].text = textKey;
		}
	}

	public void Show(Action callback)
	{
		if (empty)
		{
			callback();
			return;
		}
		onFinishCallback = callback;
		highlightAnimator.Play();
	}

	private void OnFinished()
	{
		onFinishCallback?.Invoke();
	}
}
