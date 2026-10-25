using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveProgress : Singleton<AutoSaveProgress>
{
	private const float c_MinDisplayTime = 3f;

	private int currentSavingsInProgress;

	private object _lock = new object();

	[SerializeField]
	private GameObject icon;

	[Header("Animation")]
	[SerializeField]
	private Image shineImage;

	[SerializeField]
	private float shineDelay = 1f;

	[SerializeField]
	private float shineDuration = 1f;

	[SerializeField]
	private Vector2 shineFrom;

	[SerializeField]
	private Vector2 shineTo;

	private Tween loopAnimation;

	private bool isShowing;

	private float startTime;

	public bool IsShowing => isShowing;

	protected override void AwakeInit()
	{
		icon.SetActive(value: false);
		shineImage.material = new Material(shineImage.material);
		shineImage.material.SetTextureOffset("_Mask", shineFrom);
	}

	[ContextMenu("Show Progress")]
	public void ShowProgress()
	{
		lock (_lock)
		{
			currentSavingsInProgress++;
		}
	}

	[ContextMenu("Hide Progress")]
	public void HideProgress()
	{
		lock (_lock)
		{
			currentSavingsInProgress = Mathf.Max(0, currentSavingsInProgress - 1);
		}
	}

	private void StartAnimation()
	{
		if (!isShowing)
		{
			startTime = Time.realtimeSinceStartup;
			isShowing = true;
			icon.SetActive(value: true);
			CancelAnimation();
			LoopAnimation();
		}
	}

	private void LoopAnimation()
	{
		shineImage.material.SetTextureOffset("_Mask", shineFrom);
		loopAnimation = DOTween.To(() => shineImage.material.GetTextureOffset("_Mask"), (Vector2 value) =>
		{
			shineImage.material.SetTextureOffset("_Mask", value);
		}, shineTo, shineDuration).SetDelay(shineDelay).SetLoops(-1, LoopType.Restart);
	}

	private IEnumerator StopAnimation(bool waitForMinTime)
	{
		if (!isShowing)
		{
			yield break;
		}
		if (waitForMinTime)
		{
			while (Time.realtimeSinceStartup - startTime < 3f)
			{
				yield return null;
			}
		}
		isShowing = false;
		CancelAnimation();
		icon.SetActive(value: false);
	}

	private void CancelAnimation()
	{
		if (loopAnimation != null)
		{
			loopAnimation.Kill();
			loopAnimation = null;
		}
	}

	private void OnDisable()
	{
		isShowing = false;
		CancelAnimation();
		icon.SetActive(value: false);
	}

	private void Update()
	{
		if (currentSavingsInProgress > 0)
		{
			if (!isShowing)
			{
				StartAnimation();
			}
		}
		else if (isShowing && !(Time.realtimeSinceStartup - startTime < 3f))
		{
			isShowing = false;
			CancelAnimation();
			icon.SetActive(value: false);
		}
	}
}
