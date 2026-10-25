using System;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
	[Serializable]
	public class UI
	{
		public GameObject onLayer;

		public GameObject offLayer;
	}

	private const string _documentation = "<b>ToggleButton</b> requires a <b>Button</b> and displays <b>onLayer</b> or <b>offLayer</b> according to <b>IsOn</b> property";

	[SerializeField]
	private UI _ui;

	private bool _isOn;

	public bool IsOn
	{
		get
		{
			return _isOn;
		}
		set
		{
			_isOn = value;
			_UpdateUI();
		}
	}

	private void _UpdateUI()
	{
		_ui.onLayer?.SetActive(_isOn);
		_ui.offLayer?.SetActive(!_isOn);
	}

	private void OnEnable()
	{
		_UpdateUI();
	}

	public void OnButtonClicked()
	{
		IsOn = !IsOn;
	}

	public void setActive()
	{
		IsOn = !IsOn;
	}
}
