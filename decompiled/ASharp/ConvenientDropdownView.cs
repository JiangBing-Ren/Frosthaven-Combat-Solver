using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConvenientDropdownView : MonoBehaviour
{
	private const string _ppKey = "KR_Alphasort_Dropdowns";

	[SerializeField]
	private GameObject _optionPrefab;

	[SerializeField]
	private Transform _optionContainer;

	[SerializeField]
	private GameObject _canvasParent;

	[SerializeField]
	private TMP_InputField _searchInput;

	[SerializeField]
	private Text _nameLabel;

	[SerializeField]
	private Toggle _notifyValueChange;

	[SerializeField]
	private Toggle _alphabeticalSort;

	private string _search = string.Empty;

	private ConvenientDropdown _dropdown;

	public void Show(ConvenientDropdown dropdown)
	{
		_search = string.Empty;
		_dropdown = dropdown;
		_nameLabel.text = dropdown.nameLabel;
		_searchInput.Select();
		_notifyValueChange.SetValue(value: true);
		if (!PlayerPrefs.HasKey("KR_Alphasort_Dropdowns"))
		{
			PlayerPrefs.SetInt("KR_Alphasort_Dropdowns", 0);
		}
		_alphabeticalSort.SetValue(PlayerPrefs.GetInt("KR_Alphasort_Dropdowns") == 1);
		_alphabeticalSort.onValueChanged.RemoveAllListeners();
		_alphabeticalSort.onValueChanged.AddListener((bool b) =>
		{
			PlayerPrefs.SetInt("KR_Alphasort_Dropdowns", b ? 1 : 0);
			Refresh();
		});
		_searchInput.onValueChanged.Invoke(_searchInput.text);
		Refresh();
	}

	public void Hide()
	{
		_canvasParent.SetActive(value: false);
	}

	private void Awake()
	{
		_canvasParent.SetActive(value: false);
		_searchInput.onValueChanged.RemoveAllListeners();
		_searchInput.onValueChanged.AddListener((string s) =>
		{
			_search = s;
			Refresh();
		});
	}

	private void Refresh()
	{
		foreach (Transform item in _optionContainer)
		{
			Object.Destroy(item.gameObject);
		}
		_canvasParent.SetActive(value: true);
		List<ConvenientDropdown.Option> options = _dropdown.options;
		int value = _dropdown.value;
		List<(GameObject, string)> list = new List<(GameObject, string)>();
		for (int i = 0; i < options.Count; i++)
		{
			if (string.IsNullOrEmpty(_search) || options[i].text.ToLower().Contains(_search.ToLower()))
			{
				ConvenientDropdown.Option option = options[i];
				GameObject gameObject = Object.Instantiate(_optionPrefab, _optionContainer);
				list.Add((gameObject, option.text));
				gameObject.GetComponentInChildren<TMP_Text>().text = option.text;
				Button component = gameObject.GetComponent<Button>();
				component.colors = new ColorBlock
				{
					normalColor = ((i == value) ? Color.green : GetStandardColor(options[i])),
					highlightedColor = ((i == value) ? Color.green : Color.white),
					pressedColor = ((i == value) ? Color.green : Color.white),
					selectedColor = ((i == value) ? Color.green : Color.white),
					disabledColor = ((i == value) ? Color.green : Color.white),
					colorMultiplier = 1f,
					fadeDuration = 0.1f
				};
				int i2 = i;
				component.onClick.AddListener(() =>
				{
					_canvasParent.SetActive(value: false);
					_dropdown.SetValue(i2, _notifyValueChange.isOn);
				});
			}
		}
		if (_alphabeticalSort.isOn)
		{
			list.Sort(((GameObject instance, string option) a, (GameObject instance, string option) b) => a.option.CompareTo(b.option));
			for (int num = 0; num < list.Count; num++)
			{
				list[num].Item1.transform.SetSiblingIndex(num);
			}
		}
	}

	private Color GetStandardColor(ConvenientDropdown.Option option)
	{
		if (option.text.StartsWith("FH_"))
		{
			return new Color(0.8f, 0.8f, 1f);
		}
		if (option.text.StartsWith("Inherit") || option.text.StartsWith("Default"))
		{
			return new Color(1f, 1f, 0.8f);
		}
		return Color.white;
	}
}
