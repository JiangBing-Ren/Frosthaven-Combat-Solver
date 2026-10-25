using System.Collections;
using Chronos;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIProgressBar : UISliderBar
{
	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private GameObject completedMask;

	[SerializeField]
	private string completedAudioItem;

	[SerializeField]
	private float progressAnimationTime = 3f;

	[SerializeField]
	private float progressAnimationFreq = 0.01f;

	[SerializeField]
	private string progressAudioItem;

	[SerializeField]
	private Color incompletedProgressColor = new Color(172f / 255f, 0.3215686f, 0.3215686f);

	[SerializeField]
	private Color completedProgressColor = new Color(172f / 255f, 0.3215686f, 0.3215686f);

	public UnityEvent OnFinishedProgress;

	public override void SetAmount(int amount)
	{
		StopAllCoroutines();
		base.SetAmount(amount);
	}

	protected override void UpdateProgressBar(float amount)
	{
		base.UpdateProgressBar(amount);
		progress.enabled = amount > 0f && amount < progress.maxValue;
		ShowCompleted(amount >= progress.maxValue);
	}

	public void ShowCompleted(bool isCompleted)
	{
		if (completedMask != null)
		{
			completedMask.SetActive(isCompleted);
		}
		progressImage.color = (isCompleted ? completedProgressColor : incompletedProgressColor);
	}

	public void PlayFromProgress(int fromProgress)
	{
		StopAllCoroutines();
		float value = progress.value;
		UpdateProgressBar(fromProgress);
		StartCoroutine(Progress(value));
	}

	private IEnumerator Progress(float to)
	{
		float amount = Mathf.Abs(to - progress.value);
		int sign = ((to > progress.value) ? 1 : (-1));
		float num = progressAnimationTime / progressAnimationFreq;
		float tickValue = amount / num;
		if (amount > 0f)
		{
			AudioControllerUtils.PlaySound(progressAudioItem);
		}
		while (to > progress.value)
		{
			float amount2 = Mathf.Min(progress.value + tickValue * (float)sign, to);
			UpdateProgressBar(amount2);
			yield return Timekeeper.instance.WaitForSeconds(progressAnimationFreq);
		}
		if (amount > 0f)
		{
			AudioControllerUtils.PlaySound(completedAudioItem);
		}
		OnFinishedProgress?.Invoke();
	}
}
