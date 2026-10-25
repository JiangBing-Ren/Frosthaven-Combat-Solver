using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Extensions/Flow Layout Group")]
public class FlowLayoutGroup : LayoutGroup
{
	public enum Axis
	{
		Horizontal,
		Vertical
	}

	public float SpacingX;

	public float SpacingY;

	public bool ExpandHorizontalSpacing;

	public bool ChildForceExpandWidth;

	public bool ChildForceExpandHeight;

	public bool invertOrder;

	public bool orderByLayoutPriority;

	private float _layoutHeight;

	private float _layoutWidth;

	[SerializeField]
	protected Axis m_StartAxis;

	public List<int> columnsCount;

	private readonly IList<RectTransform> _itemList = new List<RectTransform>();

	public Axis startAxis
	{
		get
		{
			return m_StartAxis;
		}
		set
		{
			SetProperty(ref m_StartAxis, value);
		}
	}

	protected bool IsCenterAlign
	{
		get
		{
			if (childAlignment != TextAnchor.LowerCenter && childAlignment != TextAnchor.MiddleCenter)
			{
				return childAlignment == TextAnchor.UpperCenter;
			}
			return true;
		}
	}

	protected bool IsRightAlign
	{
		get
		{
			if (childAlignment != TextAnchor.LowerRight && childAlignment != TextAnchor.MiddleRight)
			{
				return childAlignment == TextAnchor.UpperRight;
			}
			return true;
		}
	}

	protected bool IsMiddleAlign
	{
		get
		{
			if (childAlignment != TextAnchor.MiddleLeft && childAlignment != TextAnchor.MiddleRight)
			{
				return childAlignment == TextAnchor.MiddleCenter;
			}
			return true;
		}
	}

	protected bool IsLowerAlign
	{
		get
		{
			if (childAlignment != TextAnchor.LowerLeft && childAlignment != TextAnchor.LowerRight)
			{
				return childAlignment == TextAnchor.LowerCenter;
			}
			return true;
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		if (startAxis == Axis.Horizontal)
		{
			base.CalculateLayoutInputHorizontal();
			float totalMin = GetGreatestMinimumChildWidth() + (float)padding.left + (float)padding.right;
			SetLayoutInputForAxis(totalMin, -1f, -1f, 0);
		}
		else
		{
			_layoutWidth = SetLayout(0, layoutInput: true);
		}
	}

	public override void SetLayoutHorizontal()
	{
		SetLayout(0, layoutInput: false);
	}

	public override void SetLayoutVertical()
	{
		SetLayout(1, layoutInput: false);
	}

	public override void CalculateLayoutInputVertical()
	{
		if (startAxis == Axis.Horizontal)
		{
			_layoutHeight = SetLayout(1, layoutInput: true);
			return;
		}
		base.CalculateLayoutInputHorizontal();
		float totalMin = GetGreatestMinimumChildHeigth() + (float)padding.bottom + (float)padding.top;
		SetLayoutInputForAxis(totalMin, -1f, -1f, 1);
	}

	private List<RectTransform> GetElementsOrdered()
	{
		if (!orderByLayoutPriority)
		{
			return rectChildren;
		}
		return rectChildren.OrderBy(LayoutUtilityExtended.GetPriority).ToList();
	}

	public float SetLayout(int axis, bool layoutInput)
	{
		float height = base.rectTransform.rect.height;
		float width = base.rectTransform.rect.width;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		if (startAxis == Axis.Horizontal)
		{
			num5 = height;
			num6 = width - (float)padding.left - (float)padding.right;
			if (IsLowerAlign)
			{
				num3 = padding.bottom;
				num4 = padding.top;
			}
			else
			{
				num3 = padding.top;
				num4 = padding.bottom;
			}
			num = SpacingY;
			num2 = SpacingX;
		}
		else if (startAxis == Axis.Vertical)
		{
			num5 = width;
			num6 = height - (float)padding.top - (float)padding.bottom;
			if (IsRightAlign)
			{
				num3 = padding.right;
				num4 = padding.left;
			}
			else
			{
				num3 = padding.left;
				num4 = padding.right;
			}
			num = SpacingX;
			num2 = SpacingY;
		}
		float num7 = 0f;
		float num8 = 0f;
		List<RectTransform> elementsOrdered = GetElementsOrdered();
		int num9 = 0;
		int num10 = 0;
		for (int i = 0; i < elementsOrdered.Count; i++)
		{
			int index = i;
			RectTransform rectTransform = elementsOrdered[index];
			float num11 = 0f;
			float num12 = 0f;
			if (startAxis == Axis.Horizontal)
			{
				if (invertOrder)
				{
					index = (IsLowerAlign ? (elementsOrdered.Count - 1 - i) : i);
				}
				rectTransform = elementsOrdered[index];
				num11 = LayoutUtility.GetPreferredSize(rectTransform, 0);
				num11 = Mathf.Min(num11, num6);
				num12 = LayoutUtility.GetPreferredSize(rectTransform, 1);
				num12 = Mathf.Min(num12, num6);
			}
			else if (startAxis == Axis.Vertical)
			{
				if (invertOrder)
				{
					index = (IsRightAlign ? (elementsOrdered.Count - 1 - i) : i);
				}
				rectTransform = elementsOrdered[index];
				num11 = LayoutUtility.GetPreferredSize(rectTransform, 1);
				num11 = Mathf.Min(num11, num6);
				num12 = LayoutUtility.GetPreferredSize(rectTransform, 0);
				num12 = Mathf.Min(num12, num6);
			}
			if (num7 + num11 > num6 || (columnsCount.Count - 1 >= num9 && num9 >= 0 && columnsCount[num9] <= num10))
			{
				num7 -= num2;
				if (!layoutInput)
				{
					if (startAxis == Axis.Horizontal)
					{
						float yOffset = CalculateRowVerticalOffset(num5, num3, num8);
						LayoutRow(_itemList, num7, num8, num6, padding.left, yOffset, axis);
					}
					else if (startAxis == Axis.Vertical)
					{
						float xOffset = CalculateColHorizontalOffset(num5, num3, num8);
						LayoutCol(_itemList, num8, num7, num6, xOffset, padding.top, axis);
					}
				}
				num9++;
				num10 = 0;
				_itemList.Clear();
				num3 += num8;
				num3 += num;
				num8 = 0f;
				num7 = 0f;
			}
			num7 += num11;
			_itemList.Add(rectTransform);
			num10++;
			if (num12 > num8)
			{
				num8 = num12;
			}
			if (i < elementsOrdered.Count - 1)
			{
				num7 += num2;
			}
		}
		if (!layoutInput)
		{
			if (startAxis == Axis.Horizontal)
			{
				float yOffset2 = CalculateRowVerticalOffset(height, num3, num8);
				num7 -= num2;
				LayoutRow(_itemList, num7, num8, num6 - (ChildForceExpandWidth ? 0f : num2), padding.left, yOffset2, axis);
			}
			else if (startAxis == Axis.Vertical)
			{
				float xOffset2 = CalculateColHorizontalOffset(width, num3, num8);
				num7 -= num2;
				LayoutCol(_itemList, num8, num7, num6 - (ChildForceExpandHeight ? 0f : num2), xOffset2, padding.top, axis);
			}
		}
		_itemList.Clear();
		num3 += num8;
		num3 += num4;
		if (layoutInput)
		{
			SetLayoutInputForAxis(num3, num3, -1f, axis);
		}
		return num3;
	}

	private float CalculateRowVerticalOffset(float groupHeight, float yOffset, float currentRowHeight)
	{
		if (IsLowerAlign)
		{
			return groupHeight - yOffset - currentRowHeight;
		}
		if (IsMiddleAlign)
		{
			return groupHeight * 0.5f - _layoutHeight * 0.5f + yOffset;
		}
		return yOffset;
	}

	private float CalculateColHorizontalOffset(float groupWidth, float xOffset, float currentColWidth)
	{
		if (IsRightAlign)
		{
			return groupWidth - xOffset - currentColWidth;
		}
		if (IsCenterAlign)
		{
			return groupWidth * 0.5f - _layoutWidth * 0.5f + xOffset;
		}
		return xOffset;
	}

	protected void LayoutRow(IList<RectTransform> contents, float rowWidth, float rowHeight, float maxWidth, float xOffset, float yOffset, int axis)
	{
		float num = xOffset;
		if (!ChildForceExpandWidth && IsCenterAlign)
		{
			num += (maxWidth - rowWidth) * 0.5f;
		}
		else if (!ChildForceExpandWidth && IsRightAlign)
		{
			num += maxWidth - rowWidth;
		}
		float num2 = 0f;
		float num3 = 0f;
		if (ChildForceExpandWidth)
		{
			num2 = (maxWidth - rowWidth) / (float)_itemList.Count;
		}
		else if (ExpandHorizontalSpacing)
		{
			num3 = (maxWidth - rowWidth) / (float)(_itemList.Count - 1);
			if (_itemList.Count > 1)
			{
				if (IsCenterAlign)
				{
					num -= num3 * 0.5f * (float)(_itemList.Count - 1);
				}
				else if (IsRightAlign)
				{
					num -= num3 * (float)(_itemList.Count - 1);
				}
			}
		}
		for (int i = 0; i < _itemList.Count; i++)
		{
			int index = (IsLowerAlign ? (_itemList.Count - 1 - i) : i);
			RectTransform rect = _itemList[index];
			float a = LayoutUtility.GetPreferredSize(rect, 0) + num2;
			float num4 = LayoutUtility.GetPreferredSize(rect, 1);
			if (ChildForceExpandHeight)
			{
				num4 = rowHeight;
			}
			a = Mathf.Min(a, maxWidth);
			float num5 = yOffset;
			if (IsMiddleAlign)
			{
				num5 += (rowHeight - num4) * 0.5f;
			}
			else if (IsLowerAlign)
			{
				num5 += rowHeight - num4;
			}
			if (ExpandHorizontalSpacing && i > 0)
			{
				num += num3;
			}
			if (axis == 0)
			{
				SetChildAlongAxis(rect, 0, num, a);
			}
			else
			{
				SetChildAlongAxis(rect, 1, num5, num4);
			}
			if (i < _itemList.Count - 1)
			{
				num += a + SpacingX;
			}
		}
	}

	protected void LayoutCol(IList<RectTransform> contents, float colWidth, float colHeight, float maxHeight, float xOffset, float yOffset, int axis)
	{
		float num = yOffset;
		if (!ChildForceExpandHeight && IsMiddleAlign)
		{
			num += (maxHeight - colHeight) * 0.5f;
		}
		else if (!ChildForceExpandHeight && IsLowerAlign)
		{
			num += maxHeight - colHeight;
		}
		float num2 = 0f;
		float num3 = 0f;
		if (ChildForceExpandHeight)
		{
			num2 = (maxHeight - colHeight) / (float)_itemList.Count;
		}
		else if (ExpandHorizontalSpacing)
		{
			num3 = (maxHeight - colHeight) / (float)(_itemList.Count - 1);
			if (_itemList.Count > 1)
			{
				if (IsMiddleAlign)
				{
					num -= num3 * 0.5f * (float)(_itemList.Count - 1);
				}
				else if (IsLowerAlign)
				{
					num -= num3 * (float)(_itemList.Count - 1);
				}
			}
		}
		for (int i = 0; i < _itemList.Count; i++)
		{
			int index = (IsRightAlign ? (_itemList.Count - 1 - i) : i);
			RectTransform rect = _itemList[index];
			float num4 = LayoutUtility.GetPreferredSize(rect, 0);
			float a = LayoutUtility.GetPreferredSize(rect, 1) + num2;
			if (ChildForceExpandWidth)
			{
				num4 = colWidth;
			}
			a = Mathf.Min(a, maxHeight);
			float num5 = xOffset;
			if (IsCenterAlign)
			{
				num5 += (colWidth - num4) * 0.5f;
			}
			else if (IsRightAlign)
			{
				num5 += colWidth - num4;
			}
			if (ExpandHorizontalSpacing && i > 0)
			{
				num += num3;
			}
			if (axis == 0)
			{
				SetChildAlongAxis(rect, 0, num5, num4);
			}
			else
			{
				SetChildAlongAxis(rect, 1, num, a);
			}
			if (i < _itemList.Count - 1)
			{
				num += a + SpacingY;
			}
		}
	}

	public float GetGreatestMinimumChildWidth()
	{
		float num = 0f;
		for (int i = 0; i < rectChildren.Count; i++)
		{
			num = Mathf.Max(LayoutUtility.GetMinWidth(rectChildren[i]), num);
		}
		return num;
	}

	public float GetGreatestMinimumChildHeigth()
	{
		float num = 0f;
		if (columnsCount.Count > 0)
		{
			int num2 = rectChildren.Count;
			for (int i = 0; i < columnsCount.Count; i++)
			{
				if (num2 >= columnsCount[i])
				{
					num++;
					num2 -= columnsCount[i];
				}
			}
			return num * LayoutUtility.GetMinHeight(rectChildren[0]);
		}
		for (int j = 0; j < rectChildren.Count; j++)
		{
			num = Mathf.Max(LayoutUtility.GetMinHeight(rectChildren[j]), num);
		}
		return num;
	}
}
