using System;
using System.Collections.Generic;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPartyListRowElement : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text m_PartyName;

	[SerializeField]
	private RectTransform m_ClassIconContainer;

	[SerializeField]
	private Image m_ClassIconPrototype;

	[SerializeField]
	private FHToggle m_Toggle;

	private List<UISelectableChildElement> m_SelectableElements = new List<UISelectableChildElement>();

	private Action<ImportPartyDTO> m_OnSelectCallback;

	private List<Image> m_Icons = new List<Image>();

	private bool m_Initialized;

	public ImportPartyDTO PartyDto { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_ClassIconContainer.GetComponentsInChildren(m_Icons);
			m_Toggle.Selected += () =>
			{
				m_OnSelectCallback(PartyDto);
			};
			gameObject.GetComponentsInChildren(includeInactive: true, m_SelectableElements);
			m_Toggle.onValueChanged.AddListener(RefreshLabels);
		}
	}

	public void Init(ImportPartyDTO importPartyDto, Action<ImportPartyDTO> onSelect)
	{
		InitCache();
		m_OnSelectCallback = onSelect;
		PartyDto = importPartyDto;
		Populate(importPartyDto);
	}

	private void Populate(ImportPartyDTO importPartyDto)
	{
		m_PartyName.text = importPartyDto.Name;
		UIHelperUtil.UpdateUIList(importPartyDto.Members?.Count ?? 0, m_Icons, m_ClassIconPrototype, m_ClassIconContainer, setRowData);
		void setRowData(int i, Image item)
		{
			ECharacter eCharacter = CrossReference.ClassIdToECharacterModel(importPartyDto.Members[i].Class.ID.ID);
			item.sprite = UIInfoTools.Instance.GetCharacterClassIcon(eCharacter.ToString());
		}
	}

	private void RefreshLabels(bool isOn)
	{
		if (m_SelectableElements != null && m_SelectableElements.Count != 0)
		{
			for (int i = 0; i < m_SelectableElements.Count; i++)
			{
				m_SelectableElements[i].RefreshState();
			}
		}
	}

	public void Select(bool notify = false)
	{
		m_Toggle.Select();
	}
}
