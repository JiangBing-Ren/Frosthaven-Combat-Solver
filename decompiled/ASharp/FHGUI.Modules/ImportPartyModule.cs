using System;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.States;
using UnityEngine;

namespace FHGUI.Modules;

public class ImportPartyModule : FHMapViewModule
{
	[SerializeField]
	private UIPartyListContainerElement m_PartyList;

	[SerializeField]
	private UIPartyInfoElement m_PartyInfo;

	[SerializeField]
	private FHButton m_Improt;

	[SerializeField]
	private FHButton m_Back;

	private Action m_OnBackCallback;

	private Action<string> m_OnImportCallback;

	private Action<CharacterDataDTO> m_OnCharacterInspect;

	private string m_LastSelectedfPartyId;

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			m_Improt.onClick.AddListener(HandleOnImport);
			m_Back.onClick.AddListener(HandleOnBack);
		}
	}

	public void Init(ImportFromPartyViewState.ImportPartyParams data)
	{
		InitCache();
		m_OnBackCallback = data.onBack;
		m_OnImportCallback = data.onImport;
		m_OnCharacterInspect = data.onCharacterInspect;
		m_PartyList.Init(data.partyData, HandleOnPartySelected);
		if (!TryReselectParty(data))
		{
			SelectFirstParty(data);
		}
		m_Improt.interactable = true;
	}

	private void SelectFirstParty(ImportFromPartyViewState.ImportPartyParams data)
	{
		ImportPartyDTO party = ((data != null && data.partyData != null && data.partyData.Count > 0) ? data.partyData[0] : null);
		SelectParty(party);
	}

	private void SelectParty(ImportPartyDTO party)
	{
		m_PartyInfo.Init(party, m_OnCharacterInspect);
		m_LastSelectedfPartyId = party?.ID;
		m_PartyList.SelectParty(party, notify: false);
	}

	private bool TryReselectParty(ImportFromPartyViewState.ImportPartyParams data)
	{
		if (data == null)
		{
			return false;
		}
		foreach (ImportPartyDTO partyDatum in data.partyData)
		{
			if (!(partyDatum.ID != m_LastSelectedfPartyId))
			{
				SelectParty(partyDatum);
				return true;
			}
		}
		return false;
	}

	private void HandleOnPartySelected(ImportPartyDTO party)
	{
		SelectParty(party);
	}

	private void HandleOnBack()
	{
		m_OnBackCallback?.Invoke();
	}

	private void HandleOnImport()
	{
		if (m_PartyInfo.Data != null)
		{
			m_OnImportCallback?.Invoke(m_PartyInfo.Data.ID);
		}
	}
}
