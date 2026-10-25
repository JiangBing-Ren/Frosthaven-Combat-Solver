using System;
using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPartyListContainerElement : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private UIPartyListRowElement Prototype_PartyRow;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private ToggleGroup m_ToggleGroup;

	private List<UIPartyListRowElement> m_PartyRows = new List<UIPartyListRowElement>();

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_Container.GetComponentsInChildren(m_PartyRows);
			m_ToggleGroup = (m_ToggleGroup ? m_ToggleGroup : m_Container.GetComponent<ToggleGroup>());
		}
	}

	public void Init(List<ImportPartyDTO> partyData, Action<ImportPartyDTO> onSelect)
	{
		InitCache();
		UIHelperUtil.UpdateUIList(partyData.Count, m_PartyRows, Prototype_PartyRow, m_Container, onShow);
		void onShow(int i, UIPartyListRowElement item)
		{
			item.GetComponent<FHToggle>().group = m_ToggleGroup;
			item.Init(partyData[i], onSelect);
		}
	}

	public void SelectParty(ImportPartyDTO party, bool notify = true)
	{
		for (int i = 0; i < m_PartyRows.Count; i++)
		{
			UIPartyListRowElement uIPartyListRowElement = m_PartyRows[i];
			if (uIPartyListRowElement.PartyDto == party)
			{
				uIPartyListRowElement.Select();
				break;
			}
		}
	}
}
