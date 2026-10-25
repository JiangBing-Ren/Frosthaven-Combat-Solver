using UnityEngine;
using UnityEngine.UI;

public static class GridLayoutGroupExtensions
{
	public static Vector2 GetCellCountPerAxis(this GridLayoutGroup gridLayout, int cells)
	{
		int num = 1;
		int num2 = 1;
		Vector2 size = gridLayout.GetComponent<RectTransform>().rect.size;
		if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
		{
			num = gridLayout.constraintCount;
			num2 = Mathf.CeilToInt((float)cells / (float)num - 0.001f);
		}
		else if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedRowCount)
		{
			num2 = gridLayout.constraintCount;
			num = Mathf.CeilToInt((float)cells / (float)num2 - 0.001f);
		}
		else
		{
			num = Mathf.Max(1, Mathf.FloorToInt((size.x - (float)gridLayout.padding.horizontal + gridLayout.spacing.x + 0.001f) / (gridLayout.cellSize.x + gridLayout.spacing.x)));
			num2 = Mathf.Max(1, Mathf.FloorToInt((size.y - (float)gridLayout.padding.vertical + gridLayout.spacing.y + 0.001f) / (gridLayout.cellSize.y + gridLayout.spacing.y)));
		}
		int num3;
		int num4;
		if (gridLayout.startAxis == GridLayoutGroup.Axis.Horizontal)
		{
			num3 = Mathf.Clamp(num, 1, cells);
			num4 = Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)cells / (float)num));
		}
		else
		{
			num4 = Mathf.Clamp(num2, 1, cells);
			num3 = Mathf.Clamp(num, 1, Mathf.CeilToInt((float)cells / (float)num4));
		}
		return new Vector2(num3, num4);
	}

	public static void FitInParent(this GridLayoutGroup gridLayout, Vector2 preferredCellSize, RectTransform overrideTransform = null)
	{
		RectTransform rectTransform = overrideTransform ?? (gridLayout.transform.parent as RectTransform);
		if (!rectTransform)
		{
			return;
		}
		RectTransform rectTransform2 = gridLayout.transform as RectTransform;
		if (!rectTransform2)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < rectTransform2.childCount; i++)
		{
			if (rectTransform2.GetChild(i).gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		if (num == 0)
		{
			gridLayout.cellSize = preferredCellSize;
			return;
		}
		float width = rectTransform.rect.width;
		float num2 = (float)(gridLayout.padding.left + gridLayout.padding.right) - gridLayout.spacing.x;
		float num3 = width - num2;
		float num4 = preferredCellSize.x + gridLayout.spacing.x;
		int num5 = Mathf.FloorToInt(num3 / num4);
		if (num5 <= 0)
		{
			num5 = 1;
		}
		int num6 = Mathf.CeilToInt((float)num / (float)num5);
		float height = rectTransform.rect.height;
		float num7 = gridLayout.padding.top + gridLayout.padding.bottom;
		float b = gridLayout.spacing.y * (float)(num6 - 1);
		float num8 = (float)num6 * preferredCellSize.y + num7 + Mathf.Max(0f, b);
		if (height >= num8)
		{
			gridLayout.cellSize = preferredCellSize;
			return;
		}
		float num9 = height - num7 - Mathf.Max(0f, b);
		float num10 = (float)num6 * preferredCellSize.y;
		if (num9 <= 0f)
		{
			num9 = 1f;
		}
		float num11 = Mathf.Clamp01(num9 / num10);
		gridLayout.cellSize = new Vector2(preferredCellSize.x * num11, preferredCellSize.y * num11);
	}
}
