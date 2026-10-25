using System;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPartyInfoRowElement : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text m_CharacterName;

	[SerializeField]
	private TMP_Text m_CharacterLevel;

	[SerializeField]
	private Image m_CharacterIcon;

	[SerializeField]
	private FHButton m_CharacterInspectButton;

	[SerializeField]
	private ResourceElement m_Gold;

	[SerializeField]
	private ResourceElement m_Lumber;

	[SerializeField]
	private ResourceElement m_Metal;

	[SerializeField]
	private ResourceElement m_Hide;

	[SerializeField]
	private UIEquipmentContainerElement m_EquipmentContainer;

	private Action<CharacterDataDTO> m_OnCharacterInspectCallback;

	private CharacterDataDTO m_Character;

	private bool m_Initialized;

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_CharacterInspectButton.onClick.AddListener(HandleOnCharacterInspect);
		}
	}

	public void Init(CharacterDataDTO character, Action<CharacterDataDTO> onCharacterInspectCallback)
	{
		InitCache();
		m_OnCharacterInspectCallback = onCharacterInspectCallback;
		m_Character = character;
		m_CharacterName.text = character.Name;
		m_CharacterIcon.sprite = UIInfoTools.GetCharacterBannerPortrait(CrossReference.ClassDefToECharacterModel(character.Class).ToString());
		m_CharacterLevel.text = character.Level.ToString();
		if ((bool)m_Gold)
		{
			m_Gold.Init((int)character.Gold);
		}
		if ((bool)m_Lumber)
		{
			m_Lumber.Init(character.Resources.Lumber);
		}
		if ((bool)m_Metal)
		{
			m_Metal.Init(character.Resources.Metal);
		}
		if ((bool)m_Hide)
		{
			m_Hide.Init(character.Resources.Hide);
		}
		if ((bool)m_EquipmentContainer)
		{
			m_EquipmentContainer.Init(character.Items, character.Level, character.UnlockedSmallItemCount, character.MaxSmallItems, character.MaxOneHandSlots);
		}
	}

	private void HandleOnCharacterInspect()
	{
		m_OnCharacterInspectCallback(m_Character);
	}

	private void OnDisable()
	{
		m_Character = null;
	}
}
