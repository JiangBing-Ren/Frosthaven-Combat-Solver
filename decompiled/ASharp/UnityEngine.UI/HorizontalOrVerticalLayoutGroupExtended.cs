using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI;

public abstract class HorizontalOrVerticalLayoutGroupExtended : HorizontalOrVerticalLayoutGroup
{
	[SerializeField]
	private bool m_SubtractMarginHorizontal;

	[SerializeField]
	private bool m_SubtractMarginVertical;

	[SerializeField]
	private bool m_InvertOrder;

	[SerializeField]
	private bool m_OrderByPriority;

	public int BottonPadding
	{
		set
		{
			padding.bottom = value;
		}
	}

	protected void CalcAlongAxisExtended(int axis, bool isVertical)
	{
		float num = ((axis != 0) ? padding.vertical : padding.horizontal);
		float num2 = num;
		float num3 = num;
		float num4 = 0f;
		bool flag = isVertical ^ (axis == 1);
		List<RectTransform> children = GetChildren();
		for (int i = 0; i < children.Count; i++)
		{
			RectTransform rect = children[m_InvertOrder ? (children.Count - 1 - i) : i];
			float minSize = LayoutUtilityExtended.GetMinSize(rect, axis);
			float preferredSize = LayoutUtilityExtended.GetPreferredSize(rect, axis);
			float num5 = LayoutUtility.GetFlexibleSize(rect, axis);
			RectOffset margin = LayoutUtilityExtended.GetMargin(rect);
			float num6 = ((axis != 0) ? margin.vertical : margin.horizontal);
			if ((axis == 0) ? childForceExpandWidth : childForceExpandHeight)
			{
				num5 = Mathf.Max(num5, 1f);
			}
			if (flag)
			{
				num2 = Mathf.Max(minSize + num + num6, num2);
				num3 = Mathf.Max(preferredSize + num + num6, num3);
				num4 = Mathf.Max(num5, num4);
			}
			else
			{
				num2 += minSize + spacing + num6;
				num3 += preferredSize + spacing + num6;
				num4 += num5;
			}
		}
		if (!flag && children.Count > 0)
		{
			num2 -= spacing;
			num3 -= spacing;
		}
		float num7 = num3;
		LayoutElementExtended component = GetComponent<LayoutElementExtended>();
		if (component != null)
		{
			if (axis != 0)
			{
				num7 = ((component.MaxHeightSelf > -1f) ? component.MaxHeightSelf : num7);
			}
			else
			{
				num7 = ((component.MaxWidthSelf > -1f) ? component.MaxWidthSelf : num7);
			}
		}
		num3 = Mathf.Max(num2, MathF.Min(num7, num3));
		SetLayoutInputForAxis(num2, num3, num4, axis);
	}

	private List<RectTransform> GetChildren()
	{
		if (m_OrderByPriority)
		{
			return rectChildren.OrderBy((RectTransform it) => LayoutUtilityExtended.GetPriority(it)).ToList();
		}
		return rectChildren;
	}

	protected void SetChildrenAlongAxisExtended(int axis, bool isVertical)
	{
		float num = rectTransform.rect.size[axis];
		if (isVertical ^ (axis == 1))
		{
			float value = num - (float)((axis == 0) ? padding.horizontal : padding.vertical);
			List<RectTransform> children = GetChildren();
			for (int i = 0; i < children.Count; i++)
			{
				RectTransform rect = children[m_InvertOrder ? (children.Count - 1 - i) : i];
				float minSize = LayoutUtilityExtended.GetMinSize(rect, axis);
				float preferredSize = LayoutUtilityExtended.GetPreferredSize(rect, axis);
				float num2 = LayoutUtility.GetFlexibleSize(rect, axis);
				RectOffset margin = LayoutUtilityExtended.GetMargin(rect);
				if ((axis == 0) ? childForceExpandWidth : childForceExpandHeight)
				{
					num2 = Mathf.Max(num2, 1f);
				}
				float num3 = Mathf.Clamp(value, minSize, (num2 <= 0f) ? preferredSize : num);
				if (axis == 0 && m_SubtractMarginHorizontal)
				{
					num3 -= (float)margin.horizontal;
					num3 = Mathf.Clamp(num3, minSize, num3);
				}
				else if (axis != 0 && m_SubtractMarginVertical)
				{
					num3 -= (float)margin.vertical;
					num3 = Mathf.Clamp(num3, minSize, num3);
				}
				float pos = (float)((axis != 0) ? margin.top : margin.left) + GetStartOffset(axis, num3);
				SetChildAlongAxis(rect, axis, pos, num3);
			}
			return;
		}
		float num4 = ((axis == 0) ? padding.left : padding.top);
		if (GetTotalFlexibleSize(axis) == 0f && GetTotalPreferredSize(axis) < num)
		{
			num4 = GetStartOffset(axis, GetTotalPreferredSize(axis) - (float)((axis != 0) ? padding.vertical : padding.horizontal));
		}
		float t = 0f;
		if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
		{
			t = Mathf.Clamp01((num - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));
		}
		float num5 = 0f;
		if (num > GetTotalPreferredSize(axis) && GetTotalFlexibleSize(axis) > 0f)
		{
			num5 = (num - GetTotalPreferredSize(axis)) / GetTotalFlexibleSize(axis);
		}
		List<RectTransform> children2 = GetChildren();
		for (int j = 0; j < children2.Count; j++)
		{
			RectTransform rect2 = children2[m_InvertOrder ? (children2.Count - 1 - j) : j];
			float minSize2 = LayoutUtilityExtended.GetMinSize(rect2, axis);
			float preferredSize2 = LayoutUtilityExtended.GetPreferredSize(rect2, axis);
			float num6 = LayoutUtility.GetFlexibleSize(rect2, axis);
			RectOffset margin2 = LayoutUtilityExtended.GetMargin(rect2);
			if ((axis == 0) ? childForceExpandWidth : childForceExpandHeight)
			{
				num6 = Mathf.Max(num6, 1f);
			}
			float num7 = Mathf.Lerp(minSize2, preferredSize2, t);
			num7 += num6 * num5;
			num4 += (float)((axis != 0) ? margin2.top : margin2.left);
			SetChildAlongAxis(rect2, axis, num4, num7);
			num4 += num7 + spacing + (float)((axis != 0) ? margin2.bottom : margin2.right);
		}
	}
}
