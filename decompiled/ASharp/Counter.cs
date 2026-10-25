using System.Collections;
using Chronos;
using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI text;

	[SerializeField]
	private AnimationCurve animationTime;

	[SerializeField]
	private AnimationCurve animationIncrease;

	[SerializeField]
	private GUIAnimator increaseAnimator;

	[SerializeField]
	protected Color increaseColor;

	[SerializeField]
	private GUIAnimator decreaseAnimator;

	[Tooltip("If enabled when the count exceeds 1000 it will show 1K, 1M, etc")]
	[SerializeField]
	private bool useCompactNaming = true;

	private int currentCount;

	private Color defaultTextColor;

	protected virtual void Awake()
	{
		defaultTextColor = text.color;
	}

	public virtual void CountTo(int count)
	{
		if (count > currentCount)
		{
			ShowIncrease();
		}
		else
		{
			ShowDecrease();
		}
		if (!useCompactNaming && count >= 1000 && currentCount >= 1000)
		{
			SetCount(count);
			return;
		}
		StopAllCoroutines();
		StartCoroutine(CountAnimation(count));
	}

	private IEnumerator CountAnimation(int to)
	{
		float time = 0f;
		while (to != currentCount)
		{
			int num = Mathf.Abs(to - currentCount);
			int num2 = ((to > currentCount) ? 1 : (-1));
			time += Timekeeper.instance.m_GlobalClock.deltaTime;
			while (num > 0)
			{
				float num3 = animationTime.Evaluate(num);
				if (!(num3 < time))
				{
					break;
				}
				time -= num3;
				int num4 = (int)animationIncrease.Evaluate(num);
				num -= num4;
				UpdateCount(currentCount + num4 * num2);
			}
			yield return null;
		}
		ResetEffects();
	}

	protected virtual void ResetEffects()
	{
		text.color = defaultTextColor;
	}

	protected virtual void ShowDecrease()
	{
		text.color = UIInfoTools.Instance.warningColor;
		increaseAnimator.Stop();
		decreaseAnimator.Play();
	}

	protected virtual void ShowIncrease()
	{
		text.color = increaseColor;
		decreaseAnimator.Stop();
		increaseAnimator.Play();
	}

	public virtual void SetCount(int count)
	{
		StopAllCoroutines();
		ResetEffects();
		UpdateCount(count);
	}

	protected virtual void UpdateCount(int count)
	{
		currentCount = count;
		if (useCompactNaming)
		{
			text.text = count.ToString();
		}
		else if (count >= 1000000000)
		{
			text.text = $"{Mathf.FloorToInt((float)count / 1E+09f)}B";
		}
		else if (count >= 1000000)
		{
			text.text = $"{Mathf.FloorToInt((float)count / 1000000f)}M";
		}
		else if (count >= 1000)
		{
			text.text = $"{Mathf.FloorToInt((float)count / 1000f)}K";
		}
		else
		{
			text.text = count.ToString();
		}
	}
}
