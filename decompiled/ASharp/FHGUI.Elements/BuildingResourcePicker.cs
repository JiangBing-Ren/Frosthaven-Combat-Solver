using System;
using FHGUI.Common.Elements;
using FHRL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class BuildingResourcePicker : MonoBehaviour
{
	public struct ResourcePickerData
	{
		public uint MaterialAmount;

		public OutpostResources CurrentResources;

		public OutpostMaterialType PickedMaterialType;

		public Action<OutpostMaterialType> OnPickMaterialType;
	}

	[Header("Picker")]
	[SerializeField]
	private FHButton _openPickerButton;

	[SerializeField]
	private Image _openPickerButtonIcon;

	[SerializeField]
	private Sprite _allMaterialsIcon;

	[SerializeField]
	private UIPicker _picker;

	[SerializeField]
	private TMP_Text _resourceAmountValue;

	private ResourcePickerData _data;

	public bool Interactable
	{
		get
		{
			return _openPickerButton.interactable;
		}
		set
		{
			_openPickerButton.interactable = value;
		}
	}

	public bool IsVisible => _picker.IsVisible();

	private void Awake()
	{
		_openPickerButton.onClick.AddListener(OpenPicker);
	}

	private void OnDisable()
	{
		_picker.Hide();
	}

	public void Init(ResourcePickerData data)
	{
		_data = data;
		OutpostMaterialType pickedMaterialType = data.PickedMaterialType;
		bool flag = pickedMaterialType != OutpostMaterialType.All && pickedMaterialType != OutpostMaterialType.None;
		_openPickerButtonIcon.sprite = (flag ? UIInfoTools.Instance.GetResourceConfig(data.PickedMaterialType).BigIcon : _allMaterialsIcon);
		_resourceAmountValue.text = _data.MaterialAmount.ToString();
	}

	private void OpenPicker()
	{
		UIPicker.UIPickerData materialPicker = UIPicker.UIPickerData.GetMaterialPicker(_data.CurrentResources, _data.PickedMaterialType);
		_picker.Init(materialPicker, OnPickerResult);
	}

	private bool OnPickerResult(UIPicker.UIPickerResult obj)
	{
		FHGUI.Common.Elements.UIPickerSlot.UIPickerSlotDTO[] selected = obj.Selected;
		OutpostMaterialType obj2 = ((selected != null && selected.Length > 0) ? ((OutpostMaterialType)obj.Selected[0].SlotData) : OutpostMaterialType.None);
		_data.OnPickMaterialType?.Invoke(obj2);
		return true;
	}

	public void RefreshPicker()
	{
		if (IsVisible)
		{
			if (Interactable)
			{
				OpenPicker();
			}
			else
			{
				_picker.Hide();
			}
		}
	}
}
