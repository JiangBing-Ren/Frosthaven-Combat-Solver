using System.Collections.Generic;
using System.Linq;
using FHGUI.Modules;
using FHRL;

namespace FHGUI.DTO;

public class DistributionTableDTO
{
	public class DistributionRowData
	{
		public UIDistributionRowHeader.Params HeaderData;

		public List<UIDistributionCell.Params> CellData;
	}

	public class ColumnData
	{
		public PartySlotDTO Character;

		public OutpostResources CharacterResources;

		public OutpostResources TownResources;

		public bool IsTown => Character == null;
	}

	public List<DistributionRowData> Rows;

	public List<ColumnData> Columns;

	public List<UIDistributionCell.Params> GetCells(int columnIndex)
	{
		return Rows.Select((DistributionRowData row) => row.CellData[columnIndex]).ToList();
	}
}
