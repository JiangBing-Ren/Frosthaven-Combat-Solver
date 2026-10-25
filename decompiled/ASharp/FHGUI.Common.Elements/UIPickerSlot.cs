using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class UIPickerSlot : MonoBehaviour
{
	public record UIPickerSlotDTO
	{
		public Sprite Icon;

		public int? Quantity;

		public bool Available = true;

		public bool Selected;

		public bool Locked;

		public string Tooltip;

		public object SlotData;
	}

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _quantity;

	[SerializeField]
	private GameObject _locked;

	[SerializeField]
	private RectTransform _quantityContainer;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[Header("State")]
	[SerializeField]
	private Color _disabledColor = new Color(1f, 0f, 0f, 0.5f);

	[SerializeField]
	private GameObject[] _selectedObjects;

	private Action<UIPickerSlot, bool> _onClick;

	private Predicate<UIPickerSlot> _validateSelect;

	public UIPickerSlotDTO Data { get; private set; }

	public bool Selected => Data?.Selected ?? false;

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
	}

	private void OnDisable()
	{
		Data = null;
		_onClick = null;
		_validateSelect = null;
		SetSelected(selected: false);
	}

	private void OnClick()
	{
		if (!(Data == null) && Data.Available)
		{
			Predicate<UIPickerSlot> validateSelect = _validateSelect;
			if (validateSelect == null || validateSelect(this))
			{
				Data.Selected = true;
				_onClick?.Invoke(this, Selected);
			}
		}
	}

	private void SetSelected(bool selected)
	{
		if (selected)
		{
			_button.Select();
		}
		else
		{
			_button.OnDeselect(null);
		}
		GameObject[] selectedObjects = _selectedObjects;
		for (int i = 0; i < selectedObjects.Length; i++)
		{
			selectedObjects[i].SetActive(selected);
		}
	}

	public void Init(UIPickerSlotDTO data, Action<UIPickerSlot, bool> onClick, Predicate<UIPickerSlot> validateSelect = null)
	{
		Data = data;
		_icon.sprite = data.Icon;
		bool hasValue = data.Quantity.HasValue;
		_quantityContainer.gameObject.SetActive(hasValue);
		_quantity.text = data.Quantity?.ToString() ?? "";
		_button.enabled = !hasValue || data.Quantity > 0;
		_button.interactable = data.Available;
		_icon.color = (data.Available ? Color.white : _disabledColor);
		_locked.SetActive(data.Locked);
		SetSelected(data.Selected);
		_tooltip.tooltipText = data.Tooltip;
		_onClick = onClick;
		_validateSelect = validateSelect;
	}
}
