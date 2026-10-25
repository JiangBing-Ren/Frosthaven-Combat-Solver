using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReportIssueGui;

[RequireComponent(typeof(Toggle))]
public class ReportIssueRecordSavesFieldItem : MonoBehaviour
{
	private Toggle _toggleComponent;

	[SerializeField]
	private Text Label;

	internal ReportIssueWindowSavesField.OptionContent OptionContent;

	private Action<ReportIssueRecordSavesFieldItem, bool> OnInteract;

	internal string OptionName => OptionContent.Name;

	internal void Init(Dictionary<string, object> initParams)
	{
		OptionContent = (ReportIssueWindowSavesField.OptionContent)initParams["Content"];
		Label.text = OptionName;
		OnInteract = (Action<ReportIssueRecordSavesFieldItem, bool>)initParams["OnInteract"];
		_toggleComponent = GetComponent<Toggle>();
		_toggleComponent.isOn = OptionContent.ToggleValue;
		_toggleComponent.onValueChanged.AddListener((bool state) =>
		{
			OnInteract(this, state);
		});
	}

	public void SetStateNoCallbacks(bool state)
	{
		_toggleComponent.onValueChanged.RemoveAllListeners();
		_toggleComponent.isOn = state;
		_toggleComponent.onValueChanged.AddListener((bool s) =>
		{
			OnInteract(this, s);
		});
	}
}
