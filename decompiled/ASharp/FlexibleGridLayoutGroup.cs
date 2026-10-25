using System;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayoutGroup : LayoutGroup
{
	public enum Corner
	{
		UpperLeft,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	public enum Axis
	{
		Horizontal,
		Vertical
	}

	public enum Constraint
	{
		Flexible,
		FixedColumnCount,
		FixedRowCount
	}

	[SerializeField]
	protected Corner m_StartCorner;

	[SerializeField]
	protected Axis m_StartAxis;

	[SerializeField]
	protected bool m_LimitMaxCellSize;

	[ConditionalField("m_LimitMaxCellSize", "true", true)]
	[SerializeField]
	protected float m_MaxCellWidth = 100f;

	[ConditionalField("m_LimitMaxCellSize", "true", true)]
	[SerializeField]
	protected float m_MaxCellHeight = 100f;

	[SerializeField]
	protected Vector2 m_Spacing = Vector2.zero;

	[SerializeField]
	protected bool m_RefreshOnRectDimensionChange;

	[SerializeField]
	protected Constraint m_Constraint;

	[SerializeField]
	protected int m_ConstraintCount = 2;

	public Corner startCorner
	{
		get
		{
			return m_StartCorner;
		}
		set
		{
			SetProperty(ref m_StartCorner, value);
		}
	}

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

	public Vector2 spacing
	{
		get
		{
			return m_Spacing;
		}
		set
		{
			SetProperty(ref m_Spacing, value);
		}
	}

	public Constraint constraint
	{
		get
		{
			return m_Constraint;
		}
		set
		{
			SetProperty(ref m_Constraint, value);
		}
	}

	public int constraintCount
	{
		get
		{
			return m_ConstraintCount;
		}
		set
		{
			SetProperty(ref m_ConstraintCount, Mathf.Max(1, value));
		}
	}

	private float SpacingX => spacing.x;

	private float SpacingY => spacing.y;

	protected FlexibleGridLayoutGroup()
	{
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (m_RefreshOnRectDimensionChange)
		{
			SetDirty();
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		int num = 0;
		int num2 = 0;
		if (m_Constraint == Constraint.FixedColumnCount)
		{
			num = (num2 = m_ConstraintCount);
		}
		else if (m_Constraint == Constraint.FixedRowCount)
		{
			num = (num2 = Mathf.CeilToInt((float)rectChildren.Count / (float)m_ConstraintCount - 0.001f));
		}
		else
		{
			num = 1;
			num2 = Mathf.CeilToInt(Mathf.Sqrt(rectChildren.Count));
		}
		SetLayoutInputForAxis((float)padding.horizontal + (m_MaxCellWidth + SpacingX) * (float)num - SpacingX, (float)padding.horizontal + (m_MaxCellWidth + SpacingX) * (float)num2 - SpacingX, -1f, 0);
	}

	public override void CalculateLayoutInputVertical()
	{
		int num = 0;
		if (m_Constraint == Constraint.FixedColumnCount)
		{
			num = Mathf.CeilToInt((float)rectChildren.Count / (float)m_ConstraintCount - 0.001f);
		}
		else if (m_Constraint == Constraint.FixedRowCount)
		{
			num = m_ConstraintCount;
		}
		else
		{
			float width = rectTransform.rect.width;
			int num2 = Mathf.Max(1, Mathf.FloorToInt((width - (float)padding.horizontal + SpacingX + 0.001f) / (m_MaxCellWidth + SpacingX)));
			num = Mathf.CeilToInt((float)rectChildren.Count / (float)num2);
		}
		float num3 = Mathf.Max(0f, (float)padding.vertical + (SpacingY - 1f) * (float)num);
		float totalPreferred = num3 + m_MaxCellHeight * (float)num;
		SetLayoutInputForAxis(num3, totalPreferred, -1f, 1);
	}

	public override void SetLayoutHorizontal()
	{
		SetCellsAlongAxis(0);
	}

	public override void SetLayoutVertical()
	{
		SetCellsAlongAxis(1);
	}

	private void SetCellsAlongAxis(int axis)
	{
		Vector2 sizeDelta;
		if (constraint == Constraint.FixedColumnCount)
		{
			sizeDelta = CalculateCellSizePerColumn(constraintCount);
		}
		else
		{
			sizeDelta = ((constraint != Constraint.FixedRowCount) ? CalculateMaxCellSizeFits() : CalculateCellSizePerColumn(Mathf.CeilToInt((float)rectChildren.Count / (float)constraintCount)));
		}
		if (axis == 0)
		{
			for (int i = 0; i < rectChildren.Count; i++)
			{
				RectTransform rectTransform = rectChildren[i];
				m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
				rectTransform.anchorMin = Vector2.up;
				rectTransform.anchorMax = Vector2.up;
				rectTransform.sizeDelta = sizeDelta;
			}
			return;
		}
		float x = base.rectTransform.rect.size.x;
		float y = base.rectTransform.rect.size.y;
		int num = 1;
		int num2 = 1;
		if (m_Constraint == Constraint.FixedColumnCount)
		{
			num = m_ConstraintCount;
			if (rectChildren.Count > num)
			{
				num2 = rectChildren.Count / num + ((rectChildren.Count % num > 0) ? 1 : 0);
			}
		}
		else if (m_Constraint != Constraint.FixedRowCount)
		{
			num = ((!(sizeDelta.x + SpacingX <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((x - (float)padding.horizontal + SpacingX + 0.001f) / (sizeDelta.x + SpacingX))) : int.MaxValue);
			num2 = ((!(sizeDelta.y + SpacingY <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((y - (float)padding.vertical + SpacingY + 0.001f) / (sizeDelta.y + SpacingY))) : int.MaxValue);
		}
		else
		{
			num2 = m_ConstraintCount;
			if (rectChildren.Count > num2)
			{
				num = rectChildren.Count / num2 + ((rectChildren.Count % num2 > 0) ? 1 : 0);
			}
		}
		int num3 = (int)startCorner % 2;
		int num4 = (int)startCorner / 2;
		int num5;
		int num6;
		int num7;
		if (startAxis == Axis.Horizontal)
		{
			num5 = num;
			num6 = Mathf.Clamp(num, 1, rectChildren.Count);
			num7 = Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)rectChildren.Count / (float)num5));
		}
		else
		{
			num5 = num2;
			num7 = Mathf.Clamp(num2, 1, rectChildren.Count);
			num6 = Mathf.Clamp(num, 1, Mathf.CeilToInt((float)rectChildren.Count / (float)num5));
		}
		Vector2 vector = new Vector2((float)num6 * sizeDelta.x + (float)(num6 - 1) * SpacingX, (float)num7 * sizeDelta.y + (float)(num7 - 1) * SpacingY);
		Vector2 vector2 = new Vector2(GetStartOffset(0, vector.x), GetStartOffset(1, vector.y));
		for (int j = 0; j < rectChildren.Count; j++)
		{
			int num8;
			int num9;
			if (startAxis == Axis.Horizontal)
			{
				num8 = j % num5;
				num9 = j / num5;
			}
			else
			{
				num8 = j / num5;
				num9 = j % num5;
			}
			if (num3 == 1)
			{
				num8 = num6 - 1 - num8;
			}
			if (num4 == 1)
			{
				num9 = num7 - 1 - num9;
			}
			SetChildAlongAxis(rectChildren[j], 0, vector2.x + (sizeDelta.x + SpacingX) * (float)num8, sizeDelta.x);
			SetChildAlongAxis(rectChildren[j], 1, vector2.y + (sizeDelta.y + SpacingY) * (float)num9, sizeDelta.y);
		}
	}

	private Vector2 CalculateMaxCellSizeFits()
	{
		Vector2 result = new Vector2(0f, 0f);
		float num = m_MaxCellWidth * m_MaxCellHeight;
		for (int num2 = rectChildren.Count; num2 > 0; num2--)
		{
			Vector2 vector = CalculateCellSizePerColumn(num2);
			float num3 = vector.y * vector.x;
			if (m_LimitMaxCellSize && num3 >= num)
			{
				return new Vector2(m_MaxCellWidth, m_MaxCellHeight);
			}
			if (num3 > result.x * result.y)
			{
				result = vector;
			}
		}
		return result;
	}

	private Vector2 CalculateCellSizePerColumn(int columns)
	{
		Vector2 result = new Vector2(0f, 0f);
		float num = m_MaxCellWidth / m_MaxCellHeight;
		int num2 = Mathf.CeilToInt((float)rectChildren.Count / (float)columns);
		float num3 = rectTransform.rect.width - (float)padding.horizontal - SpacingX * (float)(columns - 1);
		float num4 = rectTransform.rect.height - (float)padding.vertical - SpacingY * (float)(num2 - 1);
		float num5 = num3 / ((float)columns * num);
		float num6 = num4 / (float)num2;
		if (num5 <= num6)
		{
			result.x = (m_LimitMaxCellSize ? Math.Min(num3 / (float)columns, m_MaxCellWidth) : (num3 / (float)columns));
			result.y = result.x / num;
		}
		else
		{
			result.y = (m_LimitMaxCellSize ? Math.Min(num4 / (float)num2, m_MaxCellHeight) : (num4 / (float)num2));
			result.x = result.y * num;
		}
		return result;
	}
}
