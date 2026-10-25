using UnityEngine;

namespace FHGUI.Common;

public static class UIRectTransformUtils
{
	public static UIImageSettings GetRectSettings(this RectTransform rt)
	{
		return new UIImageSettings(rt);
	}

	public static void ApplyRectSettings(this UIImageSettings settings, RectTransform rt)
	{
		rt.localScale = settings.LocalScale;
		rt.offsetMin = settings.OffsetMin;
		rt.offsetMax = settings.OffsetMax;
		rt.anchoredPosition = settings.AnchoredPosition;
	}

	public static int ActiveChildCount(this RectTransform rt)
	{
		int num = 0;
		int childCount = rt.childCount;
		for (int i = 0; i < childCount; i++)
		{
			if (rt.GetChild(i).gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		return num;
	}

	public static void StretchToParent(RectTransform rectTransform)
	{
		if (!(rectTransform == null))
		{
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
	}
}
