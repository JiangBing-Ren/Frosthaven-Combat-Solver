using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectExtensions
{
	public static void ScrollToTop(this ScrollRect scrollRect)
	{
		scrollRect.verticalNormalizedPosition = 1f;
	}

	public static void ScrollVerticallyTo(this ScrollRect scrollRect, RectTransform target)
	{
		Vector3 vector = scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position);
		scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, vector.y);
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public static void ScrollVerticallyToTop(this ScrollRect scrollRect, RectTransform target, float offset = 0f)
	{
		Vector3 vector = scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position);
		scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, vector.y - (1f - target.pivot.y) * target.sizeDelta.y - offset);
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public static void ScrollHorizontallyTo(this ScrollRect scrollRect, RectTransform target)
	{
		if (!(target == null))
		{
			Vector3 vector = scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position);
			scrollRect.content.anchoredPosition = new Vector2(vector.x, scrollRect.content.anchoredPosition.y);
			scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
		}
	}

	public static void ScrollLayoutGroupVerticallyToTop(this ScrollRect scrollRect, RectTransform target)
	{
		LayoutGroup component = scrollRect.content.GetComponent<LayoutGroup>();
		scrollRect.ScrollVerticallyToTop(target, (!(component == null)) ? component.padding.top : 0);
	}

	public static void ScrollToFit(this ScrollRect scrollRect, RectTransform target)
	{
		Vector2 vector = scrollRect.viewport.InverseTransformPoint(target.position);
		float num = vector.y - target.pivot.y * target.rect.height;
		float num2 = vector.y + (1f - target.pivot.y) * target.rect.height;
		if (num < 0f - scrollRect.viewport.rect.height)
		{
			scrollRect.content.anchoredPosition += new Vector2(0f, 0f - (scrollRect.viewport.rect.height + num));
		}
		else if (num2 > 0f)
		{
			scrollRect.content.anchoredPosition -= new Vector2(0f, num2);
		}
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public static float GetNormalizedChildPositionVertical(this ScrollRect scrollRect, RectTransform child)
	{
		Vector2 vector = scrollRect.content.InverseTransformPoint(child.position);
		return Mathf.Clamp01((scrollRect.content.rect.height + vector.y) / scrollRect.content.rect.height);
	}
}
