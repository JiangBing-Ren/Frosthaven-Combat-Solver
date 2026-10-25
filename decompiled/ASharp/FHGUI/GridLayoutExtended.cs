using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

[AddComponentMenu("Layout/Grid Layout Extended", 159)]
public class GridLayoutExtended : LayoutGroup
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
	protected Vector2 m_CellSize = new Vector2(100f, 100f);

	[SerializeField]
	protected Vector2 m_Spacing = Vector2.zero;

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

	public Vector2 cellSize
	{
		get
		{
			return m_CellSize;
		}
		set
		{
			SetProperty(ref m_CellSize, value);
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

	protected GridLayoutExtended()
	{
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
		SetLayoutInputForAxis((float)padding.horizontal + (cellSize.x + spacing.x) * (float)num - spacing.x, (float)padding.horizontal + (cellSize.x + spacing.x) * (float)num2 - spacing.x, -1f, 0);
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
			int num2 = Mathf.Max(1, Mathf.FloorToInt((width - (float)padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
			num = Mathf.CeilToInt((float)rectChildren.Count / (float)num2);
		}
		float num3 = (float)padding.vertical + (cellSize.y + spacing.y) * (float)num - spacing.y;
		SetLayoutInputForAxis(num3, num3, -1f, 1);
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
		if (axis == 0)
		{
			for (int i = 0; i < rectChildren.Count; i++)
			{
				RectTransform rectTransform = rectChildren[i];
				m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
				rectTransform.anchorMin = Vector2.up;
				rectTransform.anchorMax = Vector2.up;
				rectTransform.sizeDelta = cellSize;
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
			num = ((!(cellSize.x + spacing.x <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((x - (float)padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x))) : int.MaxValue);
			num2 = ((!(cellSize.y + spacing.y <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((y - (float)padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y))) : int.MaxValue);
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
		int num5 = 0;
		int num6 = 0;
		int num7;
		int num8;
		int num9;
		if (startAxis == Axis.Horizontal)
		{
			num7 = num;
			num8 = Mathf.Clamp(num, 1, rectChildren.Count);
			num9 = Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)rectChildren.Count / (float)num7));
			int num10 = ((num8 != 0) ? (rectChildren.Count % num8) : 0);
			num5 = ((num10 != 0) ? (num8 - num10) : 0);
		}
		else
		{
			num7 = num2;
			num9 = Mathf.Clamp(num2, 1, rectChildren.Count);
			num8 = Mathf.Clamp(num, 1, Mathf.CeilToInt((float)rectChildren.Count / (float)num7));
			int num11 = ((num9 != 0) ? (rectChildren.Count % num9) : 0);
			num6 = ((num11 != 0) ? (num9 - num11) : 0);
		}
		Vector2 vector = new Vector2((float)num8 * cellSize.x + (float)(num8 - 1) * spacing.x, (float)num9 * cellSize.y + (float)(num9 - 1) * spacing.y);
		Vector2 vector2 = new Vector2(GetStartOffset(0, vector.x), GetStartOffset(1, vector.y));
		for (int j = 0; j < rectChildren.Count; j++)
		{
			Vector2 vector3 = new Vector2(0f, 0f);
			int num12;
			int num13;
			if (startAxis == Axis.Horizontal)
			{
				num12 = j % num7;
				num13 = j / num7;
				if (num13 == num9 - 1 && num5 > 0)
				{
					vector3.x = (float)num5 * cellSize[0] / 2f;
					vector3.x += spacing[0] * (float)num5 / 2f;
				}
			}
			else
			{
				num12 = j / num7;
				num13 = j % num7;
				if (num12 == num8 - 1 && num6 > 0)
				{
					vector3.y = (float)num6 * cellSize[1] / 2f;
					vector3.y += spacing[1] * (float)num6 / 2f;
				}
			}
			if (num3 == 1)
			{
				num12 = num8 - 1 - num12;
				vector3.x = 0f - vector3.x;
			}
			if (num4 == 1)
			{
				num13 = num9 - 1 - num13;
				vector3.y = 0f - vector3.y;
			}
			SetChildAlongAxis(rectChildren[j], 0, vector3.x + vector2.x + (cellSize[0] + spacing[0]) * (float)num12, cellSize[0]);
			SetChildAlongAxis(rectChildren[j], 1, vector3.y + vector2.y + (cellSize[1] + spacing[1]) * (float)num13, cellSize[1]);
		}
	}
}
