using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class DistributionModule : FHMapViewModule
{
	public struct Params
	{
		public DistributionTableDTO Table;

		public string Title;

		public string Description;

		public string DescriptionLiteral;

		public List<UIMultiplayerUser.Data> ReadyUsers;

		public Action OnDone;

		public Action OnCancel;

		public Func<bool> IsDone;

		public Func<bool> IsValid;

		public bool HasDistributionAuthority;

		public string DoneButtonKey;

		public string CancelButtonKey;

		public bool ReadyCheckPassed;
	}

	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener Title;

	[SerializeField]
	private TextLocalizedListener Description;

	[SerializeField]
	private RectTransform ColumnContainer;

	[SerializeField]
	private RectTransform RowHeaderContainer;

	[SerializeField]
	private FHButton DoneButton;

	[SerializeField]
	private FHButton CancelButton;

	[Header("Hints")]
	[SerializeField]
	private GameObject ActionHintsContainer;

	[SerializeField]
	private GameObject PartyLeaderOnlyAuthorityHint;

	[Header("Layout")]
	[SerializeField]
	private LayoutElement m_TableLayoutElement;

	[SerializeField]
	private int m_MaxTableHeight;

	[SerializeField]
	private int m_DataRowHeight;

	[SerializeField]
	private int m_DataRowWidth;

	[SerializeField]
	private int m_ColumnHeaderHeight;

	[SerializeField]
	private int m_ColumnWidth;

	[Header("Multiplayer")]
	[SerializeField]
	private UIUsersListContainer m_ReadyUsersListContainer;

	private List<UIDistributionColumn> m_Columns = new List<UIDistributionColumn>();

	private List<UIDistributionRowHeader> m_RowHeaders = new List<UIDistributionRowHeader>();

	private bool m_Initialized;

	public Params Data { get; private set; }

	private void InitCache()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		m_Columns.AddRange(ColumnContainer.GetComponentsInChildren<UIDistributionColumn>(includeInactive: true));
		foreach (UIDistributionColumn column in m_Columns)
		{
			column.OnFocusGain = (Action<UIDistributionColumn>)Delegate.Combine(column.OnFocusGain, (Action<UIDistributionColumn>)((UIDistributionColumn c) =>
			{
				UpdateHitAreas();
			}));
		}
		m_RowHeaders.AddRange(RowHeaderContainer.GetComponentsInChildren<UIDistributionRowHeader>(includeInactive: true));
		DoneButton.onClick.AddListener(() =>
		{
			Data.OnDone?.Invoke();
		});
		CancelButton.onClick.AddListener(() =>
		{
			Data.OnCancel?.Invoke();
		});
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		m_ReadyUsersListContainer.Init(Data.ReadyUsers);
		Rebuild();
	}

	public void Rebuild()
	{
		if (Title != null)
		{
			Title.SetTextKey(Data.Title);
		}
		if (Description != null)
		{
			if (!string.IsNullOrEmpty(Data.DescriptionLiteral))
			{
				Description.enabled = false;
				Description.Text.text = Data.DescriptionLiteral;
			}
			else
			{
				Description.enabled = true;
				Description.SetTextKey(Data.Description);
			}
		}
		if ((bool)ActionHintsContainer)
		{
			ActionHintsContainer.SetActive(Data.HasDistributionAuthority);
		}
		if ((bool)PartyLeaderOnlyAuthorityHint)
		{
			PartyLeaderOnlyAuthorityHint.SetActive(!Data.HasDistributionAuthority);
		}
		PopulateButtons();
		PopulateColumns();
		AdjustSize();
		UpdateHitAreas();
	}

	private void AdjustSize()
	{
		int valueOrDefault = (Data.Table?.Rows?.Count).GetValueOrDefault();
		int valueOrDefault2 = (Data.Table?.Columns?.Count).GetValueOrDefault();
		m_TableLayoutElement.minHeight = Mathf.Min(m_ColumnHeaderHeight + m_DataRowHeight * valueOrDefault, m_MaxTableHeight);
		m_TableLayoutElement.preferredWidth = Mathf.Max(m_DataRowWidth + m_ColumnWidth * valueOrDefault2, m_TableLayoutElement.minWidth);
	}

	private void UpdateHitAreas()
	{
		for (int i = 0; i < m_Columns.Count; i++)
		{
			UIDistributionColumn uIDistributionColumn = m_Columns[i];
			if (uIDistributionColumn.gameObject.activeInHierarchy)
			{
				int currentFocusedIndex = int.MaxValue;
				if (uIDistributionColumn.IsHighlighted)
				{
					currentFocusedIndex = 0;
				}
				else if (i > 0 && m_Columns[i - 1].IsHighlighted)
				{
					currentFocusedIndex = -1;
				}
				else if (i < m_Columns.Count - 1 && m_Columns[i + 1].IsHighlighted)
				{
					currentFocusedIndex = 1;
				}
				uIDistributionColumn.UpdateHitArea(currentFocusedIndex);
			}
		}
	}

	private void PopulateButtons()
	{
		DoneButton.interactable = Data.IsDone != null && !Data.ReadyCheckPassed && Data.IsDone();
		DoneButton.SetTextKey(Data.DoneButtonKey ?? "GUI_DONE");
		CancelButton.gameObject.SetActive(Data.OnCancel != null);
		CancelButton.SetTextKey(Data.CancelButtonKey ?? "GUI_CANCEL");
	}

	private void PopulateColumns()
	{
		DistributionTableDTO table = Data.Table;
		int valueOrDefault = (table?.Rows?.Count).GetValueOrDefault();
		HelperTools.NormalizePool(ref m_RowHeaders, m_RowHeaders[0].gameObject, RowHeaderContainer, valueOrDefault);
		for (int i = 0; i < valueOrDefault; i++)
		{
			m_RowHeaders[i].Init(table.Rows[i].HeaderData);
		}
		int valueOrDefault2 = (table?.Columns?.Count).GetValueOrDefault();
		HelperTools.NormalizePool(ref m_Columns, m_Columns[0].gameObject, ColumnContainer, valueOrDefault2);
		for (int j = 0; j < valueOrDefault2; j++)
		{
			DistributionTableDTO.ColumnData columnData = table.Columns[j];
			m_Columns[j].Init(new UIDistributionColumn.Params
			{
				Cells = table.GetCells(j),
				Character = columnData.Character,
				CharacterResources = columnData.CharacterResources,
				TownResources = columnData.TownResources
			});
		}
	}
}
