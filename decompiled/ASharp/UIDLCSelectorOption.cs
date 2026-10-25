using System;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIDLCSelectorOption : LocalizedListener
{
	[Serializable]
	public class DLCSelectorEvent : UnityEvent<DLCRegistry.EDLCKey, bool>
	{
	}

	[SerializeField]
	private UITab toggle;

	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	public DLCSelectorEvent OnToggledDLC;

	public DLCRegistry.EDLCKey DLC { get; private set; }

	private void Awake()
	{
		toggle.onValueChanged.AddListener(OnToggled);
		toggle.OnSelected.AddListener(() =>
		{
			ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.selectHandler);
		});
		toggle.OnDeselected.AddListener(() =>
		{
			ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.deselectHandler);
		});
	}

	private void OnToggled(bool isOn)
	{
		SetSelected(isOn);
		OnToggledDLC?.Invoke(DLC, isOn);
	}

	private void SetDLC(DLCRegistry.EDLCKey dlc)
	{
		DLC = dlc;
		title.SetTextKey(dlc.ToString());
		description.text = string.Format(LocalizationManager.GetTranslation("GUI_DLC_SELECTOR_DESCR"), LocalizationManager.GetTranslation(dlc.ToString()));
		SetSelected(selected: false);
	}

	public void SetUnavailableDLC(DLCRegistry.EDLCKey dlc)
	{
		toggle.interactable = false;
		SetDLC(dlc);
	}

	public void SetAvailableDLC(DLCRegistry.EDLCKey dlc)
	{
		toggle.interactable = true;
		SetDLC(dlc);
	}

	public void SetSelected(bool selected)
	{
		toggle.SetValue(selected);
		description.color = (selected ? UIInfoTools.DefInstance.mainColor : UIInfoTools.DefInstance.greyedOutTextColor);
		UITextTooltipTarget uITextTooltipTarget = tooltip;
		string term;
		if (toggle.interactable)
		{
			term = (selected ? "GUI_DLC_SELECTOR_ENABLED_TOOLTIP" : "GUI_DLC_SELECTOR_DISABLED_TOOLTIP");
		}
		else
		{
			term = "GUI_DLC_SELECTOR_UNAVAILABLE_TOOLTIP";
		}
		uITextTooltipTarget.SetText(string.Format(LocalizationManager.GetTranslation(term), LocalizationManager.GetTranslation(DLC.ToString())), refreshTooltip: true);
	}

	protected override void OnLanguageChanged()
	{
		description.text = string.Format(LocalizationManager.GetTranslation("GUI_DLC_SELECTOR_DESCR"), LocalizationManager.GetTranslation(DLC.ToString()));
		UITextTooltipTarget uITextTooltipTarget = tooltip;
		string term;
		if (toggle.interactable)
		{
			term = (toggle.isOn ? "GUI_DLC_SELECTOR_ENABLED_TOOLTIP" : "GUI_DLC_SELECTOR_DISABLED_TOOLTIP");
		}
		else
		{
			term = "GUI_DLC_SELECTOR_UNAVAILABLE_TOOLTIP";
		}
		uITextTooltipTarget.SetText(string.Format(LocalizationManager.GetTranslation(term), LocalizationManager.GetTranslation(DLC.ToString())), refreshTooltip: true);
	}
}
