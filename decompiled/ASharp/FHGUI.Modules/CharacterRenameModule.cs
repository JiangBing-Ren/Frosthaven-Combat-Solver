using System;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class CharacterRenameModule : FHMapViewModule
{
	[SerializeField]
	private TMP_InputField _nameField;

	private Action<string> _onValueChanged;

	private Action<string> _onSubmit;

	[Header("Colors")]
	[SerializeField]
	private Color _normalColor;

	[SerializeField]
	private Color _activeColor;

	protected override bool PlaySoundOnEnable => false;

	private void Awake()
	{
		_nameField.onValueChanged.AddListener(OnValueChanged);
		_nameField.onSubmit.AddListener(OnSubmit);
	}

	public void Show(string setName, int maxLength, Action<string> onValueChanged, Action<string> onSubmit)
	{
		gameObject.SetActive(value: true);
		_nameField.SetValue(setName);
		_nameField.characterLimit = maxLength;
		_onValueChanged = onValueChanged;
		_onSubmit = onSubmit;
		_nameField.Select();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public string GetInputName()
	{
		return _nameField.text;
	}

	private void OnValueChanged(string value)
	{
		_onValueChanged?.Invoke(value);
	}

	private void OnSubmit(string value)
	{
		_onSubmit?.Invoke(value);
	}
}
