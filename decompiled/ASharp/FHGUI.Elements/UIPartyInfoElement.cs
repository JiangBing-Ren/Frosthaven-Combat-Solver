using System;
using System.Collections.Generic;
using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements;

public class UIPartyInfoElement : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private UIPartyInfoRowElement Prototype_CharacterRow;

	[SerializeField]
	private RectTransform m_Container;

	private List<UIPartyInfoRowElement> m_CharacterRows = new List<UIPartyInfoRowElement>();

	private bool m_Initialized;

	public ImportPartyDTO Data { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if (Prototype_CharacterRow.transform.parent == m_Container)
			{
				m_CharacterRows.Add(Prototype_CharacterRow);
			}
			Prototype_CharacterRow.gameObject.SetActive(value: false);
		}
	}

	public void Init(ImportPartyDTO importPartyDto, Action<CharacterDataDTO> onCharacterInspectCallback)
	{
		InitCache();
		Data = importPartyDto;
		UIHelperUtil.UpdateUIList((importPartyDto?.Members?.Count).GetValueOrDefault(), m_CharacterRows, Prototype_CharacterRow, m_Container, setRowData);
		void setRowData(int i, UIPartyInfoRowElement item)
		{
			item.gameObject.SetActive(value: true);
			item.Init(importPartyDto.Members[i], onCharacterInspectCallback);
		}
	}

	private void OnDisable()
	{
		Data = null;
		foreach (UIPartyInfoRowElement characterRow in m_CharacterRows)
		{
			characterRow.gameObject.SetActive(value: false);
		}
	}
}
