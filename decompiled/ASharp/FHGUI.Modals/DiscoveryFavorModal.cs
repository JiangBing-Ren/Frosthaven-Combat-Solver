using System;
using FHGUI.Common.Elements;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modals;

public class DiscoveryFavorModal : FHMapViewModule
{
	public class Params
	{
		public int CurrentPoints;

		public uint MaxPoints;

		public UIPicker.UIPickerData PickerData;
	}

	[SerializeField]
	private FHStacksCounter _FavorPoints;

	[SerializeField]
	private UIPicker _picker;

	[SerializeField]
	private FHButton _confirmButton;

	private Params _data;

	private Action _onClose;

	private Action _onConfirmAction;

	private Action<FHGUI.Common.Elements.UIPickerSlot, bool> _onSelect;

	private Predicate<FHGUI.Common.Elements.UIPickerSlot> _validateSelect;

	private void Awake()
	{
		_confirmButton.onClick.AddListener(Confirm);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_data = null;
		_onClose = null;
		_onConfirmAction = null;
	}

	public void Init(Params initParams, Action onClose, Action onConfirm, Action<FHGUI.Common.Elements.UIPickerSlot, bool> onSelect, Predicate<FHGUI.Common.Elements.UIPickerSlot> validateSelect, bool canConfirm)
	{
		_data = initParams;
		_onClose = onClose;
		_onSelect = onSelect;
		_onConfirmAction = onConfirm;
		_validateSelect = validateSelect;
		_FavorPoints.Init(initParams.CurrentPoints, (int)initParams.MaxPoints);
		_picker.Init(initParams.PickerData, (UIPicker.UIPickerResult _) => false, ValidateSelect);
		_confirmButton.interactable = canConfirm;
	}

	private bool ValidateSelect(FHGUI.Common.Elements.UIPickerSlot slot)
	{
		Predicate<FHGUI.Common.Elements.UIPickerSlot> validateSelect = _validateSelect;
		if (validateSelect != null && !validateSelect(slot))
		{
			return false;
		}
		_onSelect?.Invoke(slot, !slot.Selected);
		return false;
	}

	private void Confirm()
	{
		_onConfirmAction?.Invoke();
	}

	public void Close()
	{
		_onClose?.Invoke();
	}
}
