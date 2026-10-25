using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu.Credits;

public class UICreditsWindow : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private RectTransform scrollContainer;

	[SerializeField]
	private float scrollSpeed = 10f;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private Vector2 backgroundMaxDisplacement;

	[SerializeField]
	private Image backgroundHolder;

	[SerializeField]
	private AnimationCurve backgroundAnimCurve;

	[SerializeField]
	private float backgroundAnimDuration;

	[SerializeField]
	private List<Sprite> backgrounds;

	private Sequence animationSeq;

	private Coroutine scrollAnim;

	private void Awake()
	{
		exitButton.onClick.AddListener(window.Hide);
		window.onHidden.AddListener(StopAnimation);
	}

	private void OnEnable()
	{
		Show();
		scrollAnim = StartCoroutine(PlayCredits());
	}

	[ContextMenu("Play")]
	public void Show()
	{
		StopAnimation();
		backgroundHolder.SetAlpha(0f);
		scrollContainer.anchoredPosition = new Vector2(scrollContainer.anchoredPosition.x, 0f);
	}

	private IEnumerator PlayCredits()
	{
		yield return null;
		ShowBackgroundImages();
		scrollContainer.anchoredPosition = Vector2.zero;
		float num = ((RectTransform)scrollContainer.parent.transform).rect.height + 10f;
		float endY = scrollContainer.sizeDelta.y + num;
		while (scrollContainer.anchoredPosition.y < endY)
		{
			Vector2 anchoredPosition = scrollContainer.anchoredPosition;
			anchoredPosition.y += scrollSpeed * Time.deltaTime;
			scrollContainer.anchoredPosition = anchoredPosition;
			yield return null;
		}
		scrollAnim = null;
		window.Hide();
	}

	private void ShowBackgroundImages()
	{
		animationSeq = DOTween.Sequence();
		System.Random random = new System.Random();
		foreach (Sprite image in backgrounds.OrderBy((Sprite it) => random.Next()))
		{
			animationSeq.AppendCallback(() =>
			{
				backgroundHolder.sprite = image;
				backgroundHolder.rectTransform.anchoredPosition = Vector2.zero;
			});
			animationSeq.Append(BuildShowImageTween(new Vector2(LimitRandomDirection(random.NextDouble()), LimitRandomDirection(random.NextDouble()))));
		}
		animationSeq.AppendCallback(ShowBackgroundImages);
	}

	private float LimitRandomDirection(double num)
	{
		float num2 = (float)num * 2f - 1f;
		if (Math.Abs(num2) < 0.3f)
		{
			return num2 + 0.3f * (float)Math.Sign(num2);
		}
		return num2;
	}

	private Tween BuildShowImageTween(Vector2 moveDirection)
	{
		Vector2 displacement = moveDirection * backgroundMaxDisplacement;
		return DOTween.To((float progress) =>
		{
			backgroundHolder.SetAlpha(backgroundAnimCurve.Evaluate(progress));
			backgroundHolder.rectTransform.anchoredPosition = displacement * progress;
		}, 0f, 1f, backgroundAnimDuration).SetTarget(backgroundHolder.gameObject);
	}

	private void StopAnimation()
	{
		StopAllCoroutines();
		if (animationSeq != null)
		{
			animationSeq.Kill();
		}
		animationSeq = null;
		if (scrollAnim != null)
		{
			StopCoroutine(scrollAnim);
		}
		scrollAnim = null;
	}
}
