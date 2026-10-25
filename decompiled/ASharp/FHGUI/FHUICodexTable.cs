using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FHGUI;

public class FHUICodexTable : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private FHUICodexTableRow rowPrefab;

	[SerializeField]
	private RectTransform headerContainer;

	[Header("Config")]
	[SerializeField]
	private float _rowHeight = 24f;

	public TableData Data = new TableData();

	private readonly List<FHUICodexTableRow> _rowPool = new List<FHUICodexTableRow>();

	public FHUICodexTableCell GetCellTextPrototype => _rowPool.FirstOrDefault()?.GetCellPrototype;

	private void Awake()
	{
		InitDummyDataIfEmpty();
		UpdateRows();
	}

	private void InitDummyDataIfEmpty()
	{
		if (Data.Columns.Count == 0)
		{
			Data.Columns.Add(new ColumnDefinition
			{
				Id = "1",
				Width = 68f
			});
		}
		if (Data.Rows.Count != 0)
		{
			return;
		}
		RowData rowData = new RowData
		{
			Height = _rowHeight
		};
		foreach (ColumnDefinition column in Data.Columns)
		{
			_ = column;
			rowData.Cells.Add(CellData.Empty());
		}
		Data.Rows.Add(rowData);
	}

	private void UpdateRows()
	{
		UIHelperUtil.UpdateUIList(Data.Rows.Count, _rowPool, rowPrefab, content.transform, setRowData);
		void setRowData(int i, FHUICodexTableRow row)
		{
			RowData row2 = Data.Rows[i];
			row.Init(row2, Data.Columns);
		}
	}

	public void Init(TableData data)
	{
		Data = data;
		AdjustColumnsWidth(data);
		UpdateRows();
	}

	private void AdjustColumnsWidth(TableData data)
	{
		FHUICodexTableCell getCellTextPrototype = GetCellTextPrototype;
		if (getCellTextPrototype == null)
		{
			return;
		}
		foreach (RowData row in data.Rows)
		{
			row.PrepairMerge();
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < data.Columns.Count; i++)
		{
			float num3 = data.Columns[i].Width;
			for (int j = 0; j < data.Rows.Count; j++)
			{
				CellData cellData = data.Rows[j].Cells[i];
				if (!cellData.ExcludeForSizeCalculations)
				{
					float num4 = getCellTextPrototype.CalcDesiredWidth(cellData);
					if (num3 < num4)
					{
						num3 = num4;
					}
				}
			}
			data.Columns[i].Width = num3;
			num += data.Columns[i].Width;
			num2 += data.Columns[i].MinWidth;
		}
		if (num > 0f && num2 > 0f && data.Columns.Count > 0)
		{
			int num5 = 3;
			while (num5 > 0 && num > data.MaxWidth && num >= num2)
			{
				num5--;
				float num6 = data.MaxWidth / num;
				num = 0f;
				foreach (ColumnDefinition column in data.Columns)
				{
					column.Width = Mathf.Max(column.MinWidth, column.Width * num6);
					num += column.Width;
				}
			}
		}
		foreach (RowData row2 in data.Rows)
		{
			for (int k = 0; k < data.Columns.Count; k++)
			{
				row2.Cells[k].Width = data.Columns[k].Width;
			}
			row2.ApplyMergeCells();
		}
	}
}
