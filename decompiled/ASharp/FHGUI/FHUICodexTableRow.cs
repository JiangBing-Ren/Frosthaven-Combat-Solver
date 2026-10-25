using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexTableRow : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private FHUICodexTableCell m_CellPrototype;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private GameObject m_Highlight;

	public RectTransform Container;

	private List<FHUICodexTableCell> Cells = new List<FHUICodexTableCell>();

	public FHUICodexTableCell GetCellPrototype => Cells.FirstOrDefault() ?? m_CellPrototype;

	public void Init(RowData row, List<ColumnDefinition> columns)
	{
		if (Container != null)
		{
			Container.sizeDelta = new Vector2(Container.sizeDelta.x, row.Height);
		}
		if ((bool)m_Highlight)
		{
			m_Highlight.gameObject.SetActive(row.Highlighted);
		}
		UIHelperUtil.UpdateUIList(row.Cells.Count, Cells, m_CellPrototype, Container.transform, setCellData);
		void setCellData(int i, FHUICodexTableCell cell)
		{
			CellData data = row.Cells[i];
			cell.Init(data, columns[i], row);
		}
	}
}
