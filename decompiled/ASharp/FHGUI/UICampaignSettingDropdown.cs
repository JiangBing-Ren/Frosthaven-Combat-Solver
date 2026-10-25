using System.Collections.Generic;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI;

public class UICampaignSettingDropdown : UICampaignSetting
{
	[Header("Components")]
	[SerializeField]
	private TMP_Dropdown Dropdown_Options;

	[SerializeField]
	private TMP_Text Label_Title;

	private bool m_Initiazled;

	private void InitCache()
	{
		if (!m_Initiazled)
		{
			m_Initiazled = true;
			Dropdown_Options.onValueChanged.AddListener(HandleOnValueChange);
		}
	}

	public override void Init(CampaignSettingDTO dto, ValueChangeCallback changeCallback)
	{
		InitCache();
		Data = dto;
		OnChange = changeCallback;
		interactable = dto.Interactable;
		Dropdown_Options.interactable = dto.Interactable;
		Label_Title.text = Data.NameLocalized;
		Dropdown_Options.ClearOptions();
		Dropdown_Options.AddOptions(ExtractOptions(Data));
		Dropdown_Options.SetValueWithoutNotify(GetActiveOptionIndex(Data));
	}

	private int GetActiveOptionIndex(CampaignSettingDTO data)
	{
		for (int i = 0; i < data.Options.Length; i++)
		{
			if (data.Options[i].Value.Current == data.Value.Current)
			{
				return i;
			}
		}
		return 0;
	}

	private List<TMP_Dropdown.OptionData> ExtractOptions(CampaignSettingDTO data)
	{
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>(data.Options.Length);
		for (int i = 0; i < data.Options.Length; i++)
		{
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData
			{
				text = data.Options[i].NameLocalized
			};
			list.Add(item);
		}
		return list;
	}

	private void HandleOnValueChange(int value)
	{
		OnChange(Data, Data.Options[value]);
	}
}
