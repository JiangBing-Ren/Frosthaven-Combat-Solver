using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI;

public class UICampaignSettingToggle : UICampaignSetting
{
	[Header("Components")]
	[SerializeField]
	private ExtendedToggle Button_Toggle;

	[SerializeField]
	private TMP_Text Label_Title;

	[SerializeField]
	private TextLocalizedListener Label_ToggleButton;

	[SerializeField]
	private UITextTooltipTarget TooltipTarget;

	private bool m_Initiazled;

	private void InitCache()
	{
		if (!m_Initiazled)
		{
			m_Initiazled = true;
			Button_Toggle.onValueChanged.AddListener(HandleOnToggle);
		}
	}

	public override void Init(CampaignSettingDTO dto, ValueChangeCallback changeCallback)
	{
		InitCache();
		Data = dto;
		OnChange = changeCallback;
		interactable = dto.Interactable;
		Button_Toggle.interactable = dto.Interactable;
		Button_Toggle.gameObject.SetActive(dto.Interactable);
		Label_Title.text = Data.NameLocalized;
		Button_Toggle.SetIsOnWithoutNotify((bool)Data.Value.Current);
		TooltipTarget.SetText(dto.TooltipLocalized);
		UpdateToggleLabel();
	}

	private void UpdateToggleLabel()
	{
		Label_ToggleButton.SetTextKey(Button_Toggle.isOn ? "GUI_Active" : "GUI_Inactive");
	}

	private void HandleOnToggle(bool value)
	{
		UpdateToggleLabel();
		OnChange(Data, value);
	}
}
