using System.Collections.Generic;
using GLOOM;
using Libs.Utils;
using UnityEngine;

namespace FH;

public class OtherSettings : LocalizedListener
{
	public ExtendedToggle TelemetryToggle;

	public ExtendedDropdown NetworkRegionDropdown;

	public GameObject[] RegionObjects;

	private SelectorWrapper<string> _networkRegionSelector;

	private void Awake()
	{
		TelemetryToggle.onValueChanged.AddListener(OnTelemetryChange);
		_networkRegionSelector = new SelectorWrapper<string>(NetworkRegionDropdown, new List<SelectorOptData<string>>());
		_networkRegionSelector.OnValuedChanged.AddListener(SetNetworkRegion);
	}

	private void Start()
	{
		RegionObjects.ForEach((GameObject o) =>
		{
			o.SetActive(value: false);
		});
	}

	private void SetNetworkRegion(string regionKey)
	{
	}

	private new void OnEnable()
	{
		TelemetryToggle.SetIsOnWithoutNotify(TelemetryAndEulaPolicy.IsTelemetryEnabled);
		NetworkRegionDropdown.ClearOptions();
		NetworkRegionDropdown.AddOptions(new List<string> { LocalizationManager.GetTranslation("GUI_NETWORK_REGION_LOADING") });
		NetworkRegionDropdown.SetValueWithoutNotify(0);
		NetworkRegionDropdown.interactable = false;
	}

	private new void OnDisable()
	{
	}

	private void Update()
	{
	}

	protected override void OnLanguageChanged()
	{
		_networkRegionSelector.RefreshTexts();
	}

	private void OnTelemetryChange(bool isOn)
	{
		Debug.Log($"Telemetry: {isOn}");
		TelemetryAndEulaPolicy.SetTelemetryEnabled(isOn);
		SaveData.Instance.SaveGlobalData();
	}
}
