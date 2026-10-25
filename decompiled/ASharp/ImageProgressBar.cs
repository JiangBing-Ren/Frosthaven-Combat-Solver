using System;
using System.Collections.Generic;
using DG.Tweening;
using Libs.Utils.Maths;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageProgressBar : MonoBehaviour
{
	[SerializeField]
	private List<TextMeshProUGUI> amountTexts;

	[SerializeField]
	private List<TextLocalizedListener> amountTextListeners;

	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private GameObject completedMask;

	[SerializeField]
	private float progressAnimationTime = 3f;

	[SerializeField]
	private bool allowOnlyIncrease = true;

	[Tooltip("{0} will be replaced by current amount and {1} will be replaced by the total amount")]
	[SerializeField]
	private string amountFormat = "{0}/{1}";

	[SerializeField]
	private bool previewChanges;

	[SerializeField]
	[ConditionalField("previewChanges", "true", true)]
	private Image previewProgressImage;

	[SerializeField]
	[ConditionalField("previewChanges", "true", true)]
	private Color previewIncreaseColor = Color.green;

	[SerializeField]
	[ConditionalField("previewChanges", "true", true)]
	private Color previewDecreaseColor = Color.red;

	public UnityEvent OnCompletedProgress;

	private Tween progressAnim;

	private bool showProgressText = true;

	public void SetAmount(float currentValue, float maxValue)
	{
		StopProgress();
		SetAmountText(string.Format(amountFormat, currentValue, maxValue));
		float num = Mathf.Clamp01(currentValue / maxValue);
		if (num.IsNaN())
		{
			num = 0f;
		}
		UpdateProgressBar(num);
	}

	public void SetAmount(float progress, string amountText)
	{
		StopProgress();
		SetAmountText(string.Format(amountFormat, amountText));
		UpdateProgressBar(progress);
	}

	private void SetAmountText(string text)
	{
		for (int i = 0; i < amountTexts.Count; i++)
		{
			amountTexts[i].text = text;
		}
		LocalizationParam[] parameters = new LocalizationParam[1]
		{
			new LocalizationParam("0", text)
		};
		for (int j = 0; j < amountTextListeners.Count; j++)
		{
			amountTextListeners[j].SetParameters(parameters);
		}
	}

	private void UpdateProgressBar(float amount)
	{
		progressImage.fillAmount = amount;
		if (completedMask != null)
		{
			completedMask.SetActive(amount == 1f);
		}
		if (amount == 1f)
		{
			OnCompletedProgress.Invoke();
		}
	}

	public void PlayProgressTo(float currentValue, float maxValue, Action onFinished = null)
	{
		SetAmountText(string.Format(amountFormat, currentValue, maxValue));
		float to = Mathf.Clamp01(currentValue / maxValue);
		Progress(to, onFinished);
	}

	public void PlayProgressTo(float progressTo, string amountText, Action onFinished = null)
	{
		SetAmountText(string.Format(amountFormat, amountText));
		Progress(progressTo, onFinished);
	}

	private void StopProgress()
	{
		if (previewProgressImage != null)
		{
			previewProgressImage.gameObject.SetActive(value: false);
		}
		if (progressAnim != null)
		{
			progressAnim.Kill();
		}
		progressAnim = null;
	}

	private void Progress(float to, Action onFinished = null)
	{
		StopProgress();
		float fillAmount = progressImage.fillAmount;
		if (to > fillAmount)
		{
			if (previewProgressImage != null && previewChanges)
			{
				previewProgressImage.color = previewIncreaseColor;
				previewProgressImage.fillAmount = to;
				previewProgressImage.gameObject.SetActive(value: true);
			}
			progressAnim = DOTween.To(UpdateProgressBar, fillAmount, to, progressAnimationTime).SetTarget(gameObject).OnComplete(() =>
			{
				progressAnim = null;
				StopProgress();
				onFinished?.Invoke();
			});
		}
		else if (!allowOnlyIncrease)
		{
			if (previewProgressImage != null && previewChanges)
			{
				previewProgressImage.color = previewDecreaseColor;
				previewProgressImage.fillAmount = fillAmount;
				previewProgressImage.gameObject.SetActive(value: true);
			}
			progressAnim = DOTween.To(UpdateProgressBar, fillAmount, to, progressAnimationTime).SetTarget(gameObject).OnComplete(() =>
			{
				progressAnim = null;
				StopProgress();
				onFinished?.Invoke();
			});
		}
		else
		{
			onFinished?.Invoke();
		}
	}

	public void ShowProgressText(bool show)
	{
		showProgressText = show;
		for (int i = 0; i < amountTexts.Count; i++)
		{
			amountTexts[i].gameObject.SetActive(show);
		}
		for (int j = 0; j < amountTextListeners.Count; j++)
		{
			amountTextListeners[j].gameObject.SetActive(show);
		}
	}

	private void OnEnable()
	{
		amountTexts?.ForEach((TextMeshProUGUI ugui) =>
		{
			ugui.gameObject.SetActive(showProgressText);
		});
	}

	private void OnDisable()
	{
		StopProgress();
		amountTexts?.ForEach((TextMeshProUGUI ugui) =>
		{
			ugui.gameObject.SetActive(value: false);
		});
	}
}
