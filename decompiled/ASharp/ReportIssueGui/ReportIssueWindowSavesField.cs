using System;
using System.Collections.Generic;
using SETUtil;
using UnityEngine;
using UnityEngine.UI;

namespace ReportIssueGui;

public class ReportIssueWindowSavesField : MonoBehaviour
{
	public enum SpecialState
	{
		None,
		ClearAll,
		SelectAll
	}

	public class OptionContent
	{
		public string Name;

		public bool ToggleValue;

		public Func<OptionContent, bool> GetToggleStateMethod;

		public SpecialState SpecialState;

		public Action<ReportIssueRecordSavesFieldItem> OnSelect;
	}

	[Bind]
	public GameObject _sfItemSelectionDropdownObject;

	[Bind]
	public GameObject _sfItemTemplate;

	[Bind]
	public Button _savesFilterDropdown;

	[Bind]
	public Text _sfDropdownLabel;

	private bool _interactable;

	public Action<Dictionary<OptionContent, bool>> onValueChanged;

	private Dictionary<OptionContent, bool> Values = new Dictionary<OptionContent, bool>();

	private List<ReportIssueRecordSavesFieldItem> UIItems = new List<ReportIssueRecordSavesFieldItem>();

	public bool Interactable
	{
		get
		{
			return _interactable;
		}
		set
		{
			_interactable = value;
			_savesFilterDropdown.interactable = value;
			if (!_interactable)
			{
				_sfItemSelectionDropdownObject.SetActive(value: false);
			}
		}
	}

	private void Awake()
	{
		Binder.BindFields(this);
		_sfItemSelectionDropdownObject.SetActive(value: false);
		_sfItemTemplate.SetActive(value: false);
		_savesFilterDropdown.onClick.AddListener(() =>
		{
			_sfItemSelectionDropdownObject.SetActive(!_sfItemSelectionDropdownObject.activeSelf);
		});
	}

	public void SetOptions(List<OptionContent> options)
	{
		ClearOptions();
		Transform parent = _sfItemTemplate.transform.parent;
		foreach (OptionContent option in options)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_sfItemTemplate, parent);
			gameObject.SetActive(value: true);
			ReportIssueRecordSavesFieldItem component = gameObject.GetComponent<ReportIssueRecordSavesFieldItem>();
			component.Init(new Dictionary<string, object>
			{
				{
					"OnInteract",
					new Action<ReportIssueRecordSavesFieldItem, bool>(OnItemInteract)
				},
				{ "Content", option }
			});
			UIItems.Add(component);
		}
	}

	internal void UpdateLabel(string text)
	{
		_sfDropdownLabel.text = text;
	}

	private void OnItemInteract(ReportIssueRecordSavesFieldItem item, bool state)
	{
		if (Interactable)
		{
			if (!Values.ContainsKey(item.OptionContent))
			{
				Values.Add(item.OptionContent, value: false);
			}
			_sfDropdownLabel.text = item.OptionName;
			Values[item.OptionContent] = state;
			item.OptionContent.OnSelect?.Invoke(item);
			onValueChanged(Values);
			_sfItemSelectionDropdownObject.SetActive(value: false);
		}
	}

	public void ClearOptions()
	{
		foreach (ReportIssueRecordSavesFieldItem uIItem in UIItems)
		{
			SceneUtil.SmartDestroy(uIItem.gameObject);
		}
		Values.Clear();
		UIItems.Clear();
	}

	public void LateUpdateState()
	{
		foreach (ReportIssueRecordSavesFieldItem uIItem in UIItems)
		{
			bool flag = uIItem.OptionContent.GetToggleStateMethod(uIItem.OptionContent);
			Values[uIItem.OptionContent] = flag;
			uIItem.SetStateNoCallbacks(flag);
		}
	}
}
