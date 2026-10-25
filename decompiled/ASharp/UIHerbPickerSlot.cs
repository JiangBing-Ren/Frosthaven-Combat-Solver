using System;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHerbPickerSlot : MonoBehaviour
{
	public struct UIHerbData
	{
		public OutpostHerbType Herb;

		public int Quantity;

		public bool Locked;

		public bool IsEmptySelectable;
	}

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _quantity;

	[SerializeField]
	private GameObject _selected;

	[SerializeField]
	private GameObject _locked;

	[SerializeField]
	private Color _normalColor;

	[SerializeField]
	private Color _unavailableColor;

	[SerializeField]
	private Color _lockedColor;

	private Action<OutpostHerbType, bool> _onSelect;

	private bool _isSelected;

	private UIHerbData? _herbData;

	public UIHerbData HerbData => _herbData.GetValueOrDefault();

	public FHButton Button => _button;

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
	}

	private void OnDisable()
	{
		_isSelected = false;
		_herbData = null;
	}

	private void OnClick()
	{
		_isSelected = !_isSelected;
		_onSelect?.Invoke(HerbData.Herb, _isSelected);
		Init(HerbData, _isSelected);
	}

	public void Init(UIHerbData herbData, bool selected = false)
	{
		_herbData = herbData;
		bool flag = herbData.Quantity > 0;
		bool locked = herbData.Locked;
		bool interactable = !locked && (flag || _herbData.Value.IsEmptySelectable);
		_isSelected = selected;
		_selected.SetActive(selected);
		_locked.SetActive(locked);
		_quantity.text = herbData.Quantity.ToString();
		if (locked)
		{
			_icon.color = _lockedColor;
			_quantity.color = _lockedColor;
		}
		else if (!flag)
		{
			_icon.color = _unavailableColor;
			_quantity.color = _unavailableColor;
		}
		else
		{
			_icon.color = Color.white;
			_quantity.color = _normalColor;
		}
		_button.interactable = interactable;
	}
}
