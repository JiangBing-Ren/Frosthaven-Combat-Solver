using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConvenientDropdown : MonoBehaviour
{
	public class Option
	{
		public string text;

		public Option(string text)
		{
			this.text = text;
		}
	}

	[SerializeField]
	private ConvenientDropdownView _dropdownView;

	[SerializeField]
	private TMP_Text _valueLabel;

	[SerializeField]
	private Text _nameLabel;

	public UnityEvent<int> OnValueChanged = new UnityEvent<int>();

	public List<Option> options = new List<Option>();

	private int _value;

	public string nameLabel => _nameLabel.text;

	public int value
	{
		get
		{
			return _value;
		}
		set
		{
			SetValue(value, notify: true);
		}
	}

	private void Awake()
	{
		if (_dropdownView == null)
		{
			_dropdownView = Object.FindObjectOfType<ConvenientDropdownView>();
		}
		Button componentInChildren = GetComponentInChildren<Button>();
		componentInChildren.onClick.RemoveAllListeners();
		componentInChildren.onClick.AddListener(() =>
		{
			_dropdownView.Show(this);
		});
	}

	public void AddOptions(IEnumerable<string> options)
	{
		this.options.AddRange(options.Select((string o) => new Option(o)));
		SetValueWithoutNotify(value);
	}

	public void ClearOptions()
	{
		options.Clear();
		SetValueWithoutNotify(0);
	}

	internal void SetValueWithoutNotify(int v)
	{
		SetValue(v, notify: false);
	}

	internal void SetName(string v)
	{
		_nameLabel.text = v;
	}

	internal void SetValue(int value, bool notify)
	{
		_value = value;
		if (options.Count <= value)
		{
			_valueLabel.text = "N/A";
			return;
		}
		if (notify)
		{
			OnValueChanged?.Invoke(value);
		}
		_valueLabel.text = options[value].text;
	}
}
