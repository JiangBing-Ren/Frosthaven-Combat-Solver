using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HealthZoomConfigUI
{
	public float zoomMax;

	public float baseWidthDivision = 2f;

	public Vector2 defaultScale = Vector2.one;

	public bool showRemaining;

	public Vector2 remainingScale = Vector2.one;

	public List<HealthScaleConfigUI> divisions;

	public float SmallStepThickness = 0.1f;

	public Vector2 GetScaleByDivision(int division, int total)
	{
		if (divisions.IsNullOrEmpty())
		{
			return defaultScale;
		}
		HealthScaleConfigUI healthScaleConfigUI = (from it in divisions
			where division % it.division == 0
			orderby it.division descending
			select it).FirstOrDefault();
		if (healthScaleConfigUI != null)
		{
			return healthScaleConfigUI.scale;
		}
		if (showRemaining)
		{
			int num = divisions.Min((HealthScaleConfigUI it) => total % it.division);
			if (num > 0 && division > total - num)
			{
				return remainingScale;
			}
		}
		return Vector2.zero;
	}
}
