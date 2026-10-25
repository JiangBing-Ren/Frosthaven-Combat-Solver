using System;
using FHGUI.DTO;
using UnityEngine.EventSystems;

namespace FHGUI;

public abstract class UICampaignSetting : FHButton
{
	public delegate void ValueChangeCallback(CampaignSettingDTO dto, object newValue);

	[NonSerialized]
	public Action<UICampaignSetting> OnFocusGain;

	[NonSerialized]
	public Action<UICampaignSetting> OnFocusLoss;

	[NonSerialized]
	public Action<UICampaignSetting> OnSelected;

	protected ValueChangeCallback OnChange;

	public CampaignSettingDTO Data { get; protected set; }

	public abstract void Init(CampaignSettingDTO dto, ValueChangeCallback changeCallback);

	public override void Select()
	{
		base.Select();
		OnFocusGain(this);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		OnFocusGain(this);
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		OnFocusLoss(this);
		base.OnPointerExit(eventData);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		OnSelected(this);
		base.OnSelect(eventData);
	}

	protected override void OnDisable()
	{
		OnFocusGain = null;
		OnFocusLoss = null;
		base.OnDisable();
	}
}
