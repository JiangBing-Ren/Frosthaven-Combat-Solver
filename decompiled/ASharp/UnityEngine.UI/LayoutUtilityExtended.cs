using System;
using System.Linq;

namespace UnityEngine.UI;

public static class LayoutUtilityExtended
{
	public static RectOffset GetMargin(RectTransform rect)
	{
		return GetMargin(rect, new RectOffset(0, 0, 0, 0));
	}

	public static RectOffset GetMargin(RectTransform rect, RectOffset defaultValue)
	{
		if (rect == null)
		{
			return new RectOffset(0, 0, 0, 0);
		}
		LayoutElementExtended[] components = rect.GetComponents<LayoutElementExtended>();
		foreach (LayoutElementExtended layoutElementExtended in components)
		{
			if (layoutElementExtended.enabled)
			{
				RectOffset margin = layoutElementExtended.margin;
				defaultValue.top += margin.top;
				defaultValue.bottom += margin.bottom;
				defaultValue.left += margin.left;
				defaultValue.right += margin.right;
			}
		}
		return defaultValue;
	}

	public static float GetMinSize(RectTransform rect, int axis)
	{
		LayoutElementExtended[] components = rect.GetComponents<LayoutElementExtended>();
		if (axis == 0)
		{
			if (components.Any((LayoutElementExtended it) => it.overrideMinWidthWithPrefered))
			{
				return Math.Max(LayoutUtility.GetPreferredSize(rect, axis), LayoutUtility.GetMinSize(rect, axis));
			}
			return LayoutUtility.GetMinSize(rect, axis);
		}
		if (components.Any((LayoutElementExtended it) => it.overrideMinHeightWithPrefered))
		{
			return Math.Max(LayoutUtility.GetPreferredSize(rect, axis), LayoutUtility.GetMinSize(rect, axis));
		}
		return LayoutUtility.GetMinSize(rect, axis);
	}

	public static float GetPreferredSize(RectTransform rect, int axis)
	{
		LayoutElementExtended[] components = rect.GetComponents<LayoutElementExtended>();
		float num = 1f;
		foreach (LayoutElementExtended layoutElementExtended in components)
		{
			if (layoutElementExtended.enabled)
			{
				num *= ((axis == 0) ? layoutElementExtended.scalePreferredWidth : layoutElementExtended.scalePreferredHeight);
			}
		}
		if (axis == 0)
		{
			return GetPreferredWidth(rect) * num;
		}
		return GetPreferredHeight(rect) * num;
	}

	public static float GetPreferredHeight(RectTransform rect)
	{
		LayoutElementExtended[] components = rect.GetComponents<LayoutElementExtended>();
		float? num = null;
		foreach (LayoutElementExtended layoutElementExtended in components)
		{
			if (layoutElementExtended.enabled && layoutElementExtended.MaxHeight > -1f && (!num.HasValue || num.Value > layoutElementExtended.MaxHeight))
			{
				num = layoutElementExtended.MaxHeight;
			}
		}
		if (num.HasValue)
		{
			return Math.Min(LayoutUtility.GetPreferredHeight(rect), num.Value);
		}
		return LayoutUtility.GetPreferredHeight(rect);
	}

	public static float GetPreferredWidth(RectTransform rect)
	{
		LayoutElementExtended[] components = rect.GetComponents<LayoutElementExtended>();
		float? num = null;
		foreach (LayoutElementExtended layoutElementExtended in components)
		{
			if (layoutElementExtended.enabled && layoutElementExtended.MaxWidth > -1f && (!num.HasValue || num.Value > layoutElementExtended.MaxWidth))
			{
				num = layoutElementExtended.MaxWidth;
			}
		}
		if (num.HasValue)
		{
			return Math.Min(LayoutUtility.GetPreferredWidth(rect), num.Value);
		}
		return LayoutUtility.GetPreferredWidth(rect);
	}

	public static int GetPriority(RectTransform rect)
	{
		LayoutElement[] components = rect.GetComponents<LayoutElement>();
		int num = 1;
		foreach (LayoutElement layoutElement in components)
		{
			if (layoutElement.enabled)
			{
				num += layoutElement.layoutPriority;
			}
		}
		return num;
	}
}
