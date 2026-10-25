using System;
using UnityEngine;

namespace FHGUI;

public class CellData
{
	public string TextValue;

	public Sprite Icon;

	public string Tooltip;

	public CellStyle Style;

	public bool Hidden;

	public bool ExcludeForSizeCalculations;

	public bool Merged;

	public float Width;

	public Action OnClickAction;

	public static CellData Empty()
	{
		return new CellData
		{
			TextValue = "-"
		};
	}
}
