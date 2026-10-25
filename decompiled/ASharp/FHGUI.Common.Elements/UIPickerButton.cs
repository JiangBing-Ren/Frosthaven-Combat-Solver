using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class UIPickerButton : MonoBehaviour
{
	[Header("Picker")]
	[SerializeField]
	private FHButton _openPickerButton;

	[SerializeField]
	private Image _openPickerButtonIcon;

	[SerializeField]
	private UIPicker _picker;

	[SerializeField]
	private TMP_Text _amountValue;

	[SerializeField]
	private Sprite _defaultAllIcon;

	[SerializeField]
	private GameObject _defaultAllObject;

	[SerializeField]
	private bool _openPickerOverButton;

	private UIPicker.UIPickerData _pickerData;

	private Action<UIPickerSlot, bool> _onSelect;

	private Predicate<UIPickerSlot> _validateSelect;

	public UIPicker.UIPickerData PickerData => _pickerData;

	public int SelectedIndex => _pickerData?.PickerElements.FindIndex((UIPickerSlot.UIPickerSlotDTO p) => p.Selected) ?? (-1);

	public FHButton Button => _openPickerButton;

	public UIPicker Picker => _picker;

	private void Awake()
	{
		_openPickerButton.onClick.AddListener(OnButtonClick);
	}

	private void OnButtonClick()
	{
		if (_picker.isActiveAndEnabled)
		{
			_picker.gameObject.SetActive(value: false);
		}
		else
		{
			Show();
		}
	}

	private void OnDisable()
	{
		_pickerData = null;
		_picker.gameObject.SetActive(value: false);
	}

	public void Init(UIPicker.UIPickerData data, Action<UIPickerSlot, bool> onSelect, Predicate<UIPickerSlot> validateSelect, Sprite unselectedIcon = null)
	{
		_picker.gameObject.SetActive(value: false);
		_pickerData = data;
		_onSelect = onSelect;
		_validateSelect = validateSelect;
		_defaultAllIcon = unselectedIcon ?? _defaultAllIcon;
		SetState();
	}

	private void SetState()
	{
		int selectedIndex = SelectedIndex;
		UIPickerSlot.UIPickerSlotDTO uIPickerSlotDTO = ((selectedIndex == -1) ? null : _pickerData.PickerElements.ElementAt(selectedIndex));
		if (_defaultAllObject != null)
		{
			_defaultAllObject.SetActive(selectedIndex == -1);
		}
		_openPickerButtonIcon.sprite = ((selectedIndex == -1) ? _defaultAllIcon : (uIPickerSlotDTO?.Icon ?? _defaultAllIcon));
		_amountValue.text = ((selectedIndex == -1) ? string.Empty : uIPickerSlotDTO?.Quantity.ToString());
	}

	public void Show()
	{
		_picker.Init(_pickerData, OnResult, ValidateSelect);
		if (_pickerData.targetPoint != null)
		{
			UIHelperUtil.TrySetPositionConstraint(_picker.gameObject, _pickerData.targetPoint, 1f, createConstraintIfMissing: true);
		}
		else if (_openPickerOverButton)
		{
			UIHelperUtil.TrySetPositionConstraint(_picker.gameObject, transform, 1f, createConstraintIfMissing: true);
		}
		else
		{
			_picker.gameObject.transform.localPosition = Vector3.zero;
		}
	}

	private bool OnResult(UIPicker.UIPickerResult result)
	{
		throw new NotImplementedException();
	}

	private bool ValidateSelect(UIPickerSlot slot)
	{
		Predicate<UIPickerSlot> validateSelect = _validateSelect;
		if (validateSelect != null && !validateSelect(slot))
		{
			return false;
		}
		_onSelect?.Invoke(slot, !slot.Selected);
		SetState();
		_picker.Hide();
		return false;
	}
}
