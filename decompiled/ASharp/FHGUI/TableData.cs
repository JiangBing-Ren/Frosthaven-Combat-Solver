using System.Collections.Generic;

namespace FHGUI;

public class TableData
{
	public List<ColumnDefinition> Columns = new List<ColumnDefinition>();

	public List<RowData> Rows = new List<RowData>();

	public float MaxWidth = 900f;

	public RowData AddRow(List<CellData> cells, float height, bool highlighted = false)
	{
		RowData rowData = new RowData
		{
			Cells = cells,
			Height = height,
			Highlighted = highlighted
		};
		Rows.Add(rowData);
		return rowData;
	}
}
