using System;
using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;

namespace FHGUI;

public class UICampaignSettingSlider : UICampaignSetting
{
	[Header("Components")]
	[SerializeField]
	private FHSlider Slider_SettingValue;

	[SerializeField]
	private TMP_Text Label_Title;

	[SerializeField]
	private TMP_Text Label_Ammount_Foreground;

	[SerializeField]
	private TMP_Text Label_Ammount_Background;

	private bool m_IgnoreEvents;

	private bool m_Initiazled;

	private void InitCache()
	{
		if (!m_Initiazled)
		{
			m_Initiazled = true;
			Slider_SettingValue.onValueChanged.AddListener(HandleOnSliderValueChange);
		}
	}

	public override void Init(CampaignSettingDTO dto, ValueChangeCallback changeCallback)
	{
		InitCache();
		Data = dto;
		OnChange = changeCallback;
		interactable = dto.Interactable;
		Slider_SettingValue.interactable = dto.Interactable;
		m_IgnoreEvents = true;
		Slider_SettingValue.minValue = (float)Convert.ToDouble(dto.Value.Min);
		Slider_SettingValue.maxValue = (float)Convert.ToDouble(dto.Value.Max);
		Slider_SettingValue.SetValueWithoutNotify((float)Convert.ToDouble(dto.Value.Current));
		Label_Title.text = Data.NameLocalized;
		m_IgnoreEvents = false;
		UpdateValueLabels();
	}

	private void UpdateValueLabels()
	{
		string text = $"{Data.Value.Current:F0}/{Data.Value.Max:F0}";
		Label_Ammount_Foreground.text = text;
		Label_Ammount_Background.text = text;
	}

	private void HandleOnSliderValueChange(float value)
	{
		if (!m_IgnoreEvents)
		{
			OnChange(Data, value);
			UpdateValueLabels();
		}
	}
}
