using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIDebriefingTable : MonoBehaviour
{
	public struct Params
	{
		public DebriefingTableDTO TableData;
	}

	[Header("Container")]
	[SerializeField]
	private RectTransform ColumHeadersContainer;

	[SerializeField]
	private RectTransform RowHeadersContainer;

	[SerializeField]
	private GridLayoutGroup CellContainer;

	private List<UIDebriefingTableHeader> m_ColumnHeaders = new List<UIDebriefingTableHeader>();

	private List<UIDebriefingTableHeader> m_RowHeaders = new List<UIDebriefingTableHeader>();

	private List<UIDebriefingTableCell> m_Cells = new List<UIDebriefingTableCell>();

	private bool m_Initiazled;

	private void InitCache()
	{
		if (!m_Initiazled)
		{
			m_Initiazled = true;
			m_ColumnHeaders.AddRange(ColumHeadersContainer.GetComponentsInChildren<UIDebriefingTableHeader>(includeInactive: true));
			m_RowHeaders.AddRange(RowHeadersContainer.GetComponentsInChildren<UIDebriefingTableHeader>(includeInactive: true));
			m_Cells.AddRange(CellContainer.GetComponentsInChildren<UIDebriefingTableCell>(includeInactive: true));
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		if (@params.TableData == null || !@params.TableData.IsValid())
		{
			Clear();
			return;
		}
		int length = @params.TableData.Cells.GetLength(1);
		int length2 = @params.TableData.Cells.GetLength(0);
		int length3 = @params.TableData.Cells.Length;
		HelperTools.NormalizePool(ref m_ColumnHeaders, m_ColumnHeaders[0].gameObject, ColumHeadersContainer.transform, length);
		for (int i = 0; i < length; i++)
		{
			m_ColumnHeaders[i].Init(@params.TableData.ColumHeaders[i]);
		}
		HelperTools.NormalizePool(ref m_RowHeaders, m_RowHeaders[0].gameObject, RowHeadersContainer.transform, length2);
		for (int j = 0; j < length2; j++)
		{
			m_RowHeaders[j].Init(@params.TableData.RowHeaders[j]);
		}
		CellContainer.constraintCount = length2;
		HelperTools.NormalizePool(ref m_Cells, m_Cells[0].gameObject, CellContainer.transform, length3);
		for (int k = 0; k < length3; k++)
		{
			m_Cells[k].Init(@params.TableData.Cells[k % length2, k / length2]);
		}
	}

	private void Clear()
	{
		foreach (UIDebriefingTableHeader columnHeader in m_ColumnHeaders)
		{
			columnHeader.gameObject.SetActive(value: false);
		}
		foreach (UIDebriefingTableHeader rowHeader in m_RowHeaders)
		{
			rowHeader.gameObject.SetActive(value: false);
		}
		foreach (UIDebriefingTableCell cell in m_Cells)
		{
			cell.gameObject.SetActive(value: false);
		}
	}
}
