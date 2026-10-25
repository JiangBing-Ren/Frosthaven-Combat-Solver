using UnityEngine;
using UnityEngine.UI;

public class GridPercentLayoutGroup : LayoutGroup
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

	public enum CellSizeConstraint
	{
		Flexible,
		AspectRatioWidthControlsHeight,
		AspectRatioHeightControlWidth
	}

	[SerializeField]
	protected Corner m_StartCorner;

	[SerializeField]
	protected Axis m_StartAxis;

	[SerializeField]
	protected CellSizeConstraint m_CellConstraint;

	[ConditionalField("m_CellConstraint", "Flexible", false)]
	[SerializeField]
	protected float m_AspectRatioCell = 1f;

	[ConditionalField("m_CellConstraint", "AspectRatioHeightControlWidth", false)]
	[SerializeField]
	protected float m_CellPercentWidth = 0.1f;

	[ConditionalField("m_CellConstraint", "AspectRatioWidthControlsHeight", false)]
	[SerializeField]
	protected float m_CellPercentHeight = 0.1f;

	[SerializeField]
	protected Vector2 m_SpacingPercent = Vector2.zero;

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

	public Vector2 spacingPercent
	{
		get
		{
			return m_SpacingPercent;
		}
		set
		{
			SetProperty(ref m_SpacingPercent, value);
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

	private float CellWidth
	{
		get
		{
			if (m_CellConstraint == CellSizeConstraint.AspectRatioHeightControlWidth)
			{
				return CellHeight / m_AspectRatioCell;
			}
			return m_CellPercentWidth * (rectTransform.rect.width - (float)padding.top - (float)padding.bottom);
		}
	}

	private float CellHeight => m_CellConstraint switch
	{
		CellSizeConstraint.Flexible => m_CellPercentHeight * (rectTransform.rect.height - (float)padding.left - (float)padding.right), 
		CellSizeConstraint.AspectRatioHeightControlWidth => m_CellPercentHeight * (rectTransform.rect.height - (float)padding.top - (float)padding.bottom), 
		_ => CellWidth / m_AspectRatioCell, 
	};

	private float SpacingX => spacingPercent.x * rectTransform.rect.width;

	private float SpacingY => spacingPercent.y * rectTransform.rect.height;

	protected GridPercentLayoutGroup()
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
		SetLayoutInputForAxis((float)padding.horizontal + (CellWidth + SpacingX) * (float)num - SpacingX, (float)padding.horizontal + (CellWidth + SpacingX) * (float)num2 - SpacingX, -1f, 0);
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
			int num2 = Mathf.Max(1, Mathf.FloorToInt((width - (float)padding.horizontal + SpacingX + 0.001f) / (CellWidth + SpacingX)));
			num = Mathf.CeilToInt((float)rectChildren.Count / (float)num2);
		}
		float num3 = (float)padding.vertical + (CellHeight + SpacingY) * (float)num - SpacingY;
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
				rectTransform.sizeDelta = new Vector2(CellWidth, CellHeight);
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
			num = ((!(CellWidth + SpacingX <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((x - (float)padding.horizontal + SpacingX + 0.001f) / (CellWidth + SpacingX))) : int.MaxValue);
			num2 = ((!(CellHeight + SpacingY <= 0f)) ? Mathf.Max(1, Mathf.FloorToInt((y - (float)padding.vertical + SpacingY + 0.001f) / (CellHeight + SpacingY))) : int.MaxValue);
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
		Vector2 vector = new Vector2((float)num6 * CellWidth + (float)(num6 - 1) * SpacingX, (float)num7 * CellHeight + (float)(num7 - 1) * SpacingY);
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
			SetChildAlongAxis(rectChildren[j], 0, vector2.x + (CellWidth + SpacingX) * (float)num8, CellWidth);
			SetChildAlongAxis(rectChildren[j], 1, vector2.y + (CellHeight + SpacingY) * (float)num9, CellHeight);
		}
	}
}
