using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.DTO;

public class DebriefingTableDTO
{
	public struct Header
	{
		public Sprite Icon;

		public string Text;
	}

	public struct Cell
	{
		public Sprite Icon;

		public string Text;

		public bool IsHighlighted;
	}

	public List<Header> ColumHeaders;

	public List<Header> RowHeaders;

	public Cell[,] Cells;

	public bool IsValid()
	{
		if (ColumHeaders == null || ColumHeaders.Count == 0)
		{
			return false;
		}
		if (RowHeaders == null || RowHeaders.Count == 0)
		{
			return false;
		}
		if (Cells == null || ColumHeaders.Count == 0)
		{
			return false;
		}
		return true;
	}
}
