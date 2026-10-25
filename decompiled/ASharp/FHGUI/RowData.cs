using System.Collections.Generic;
using System.Linq;

namespace FHGUI;

public class RowData
{
	public List<CellData> Cells = new List<CellData>();

	public float Height = 24f;

	public bool Highlighted;

	private List<(int, int)> MergeData;

	public void MergeCells(int cellStartIndex, int count)
	{
		if (cellStartIndex >= 0 && count >= 2)
		{
			if (MergeData == null)
			{
				MergeData = new List<(int, int)>();
			}
			MergeData.Add((cellStartIndex, count));
		}
	}

	public void PrepairMerge()
	{
		if (MergeData == null)
		{
			return;
		}
		foreach (var mergeDatum in MergeData)
		{
			if (mergeDatum.Item1 + mergeDatum.Item2 <= Cells.Count())
			{
				Cells[mergeDatum.Item1].ExcludeForSizeCalculations = true;
				for (int i = mergeDatum.Item1 + 1; i < mergeDatum.Item1 + mergeDatum.Item2; i++)
				{
					Cells[i].Merged = true;
				}
			}
		}
	}

	public void ApplyMergeCells()
	{
		if (MergeData == null)
		{
			return;
		}
		foreach (var mergeDatum in MergeData)
		{
			if (mergeDatum.Item1 + mergeDatum.Item2 <= Cells.Count())
			{
				float num = Cells[mergeDatum.Item1].Width;
				for (int i = mergeDatum.Item1 + 1; i < mergeDatum.Item1 + mergeDatum.Item2; i++)
				{
					num += Cells[i].Width;
				}
				Cells[mergeDatum.Item1].Width = num;
			}
		}
	}
}
