using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;

namespace FHGUI.Elements;

public class UIOutpostEquipmentItemContainer : MonoBehaviour
{
	[Header("Slots")]
	[SerializeField]
	private UIOutpostEquipmentItemElement _headSlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _feetSlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _bodySlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _oneHandedSlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _twoHandedSlot;

	[Space]
	[SerializeField]
	private RectTransform _smallItemsContainer;

	[SerializeField]
	private UIOutpostEquipmentItemElement _smallItemPrototype;

	private List<UIOutpostEquipmentItemElement> _smallItems;

	private List<UIOutpostEquipmentItemElement> _slots;

	private Action<UIOutpostEquipmentItemElement> _onItemSlotClicked;

	private Action<UIOutpostEquipmentItemElement, bool> _onItemSlotHover;

	private OutpostItemEvent _onItemSlotDoubleClick;

	private FHItemTooltip.ItemTooltipParamsFunc _tooltipData;

	private bool _setup;

	private const int MIN_SMALL_ITEMS = 5;

	public UIOutpostEquipmentItemElement Selected { get; private set; }

	private void InitCache()
	{
		_smallItems = _smallItemsContainer.GetComponentsInChildren<UIOutpostEquipmentItemElement>().ToList();
		_setup = true;
		_slots = new List<UIOutpostEquipmentItemElement> { _headSlot, _feetSlot, _bodySlot, _oneHandedSlot, _twoHandedSlot };
	}

	public void Init(IEnumerable<ItemDataDTO> items, uint characterLevel, Action<UIOutpostEquipmentItemElement> onItemClick = null, Action<UIOutpostEquipmentItemElement, bool> onHover = null, OutpostItemEvent onDoubleClick = null, FHItemTooltip.ItemTooltipParamsFunc tooltipData = null)
	{
		if (!_setup)
		{
			InitCache();
		}
		IEnumerable<ItemDataDTO> enumerable = items.Where((ItemDataDTO i) => i.IsEquipment);
		List<ItemDataDTO> list = items.Where((ItemDataDTO i) => i.IsWeapon).ToList();
		foreach (UIOutpostEquipmentItemElement slot2 in _slots)
		{
			slot2.SetEmpty(isAvailable: true, ItemEquipType.Small, 0, OnItemSlotClicked, OnHover, OnDoubleClick);
		}
		foreach (ItemDataDTO item in enumerable)
		{
			UIOutpostEquipmentItemElement slot = GetSlot(item.ItemType);
			if (slot == null)
			{
				Debug.LogError("Cannot find slot for item " + item.NameKey);
			}
			else
			{
				slot.SetItem(item, OnItemSlotClicked, OnHover, OnDoubleClick);
			}
		}
		if (list.Count != 0)
		{
			if (list.Count > 1)
			{
				_oneHandedSlot.SetItem(list[0], OnItemSlotClicked, OnHover, OnDoubleClick);
				_twoHandedSlot.SetItem(list[1], OnItemSlotClicked, OnHover, OnDoubleClick);
			}
			else
			{
				ItemDataDTO itemDataDTO = list[0];
				GetSlot(itemDataDTO.ItemType).SetItem(itemDataDTO, OnItemSlotClicked, OnHover, OnDoubleClick);
				if (itemDataDTO.ItemType == ItemEquipType.TwoHandWeapon)
				{
					_oneHandedSlot.SetEmpty(isAvailable: false, ItemEquipType.TwoHandWeapon);
				}
			}
		}
		List<ItemDataDTO> smallItems = items.Where((ItemDataDTO item) => item.ItemType == ItemEquipType.Small).ToList();
		int count = Mathf.Max(smallItems.Count, 5);
		float radius = (transform as RectTransform).rect.size.x / 2f + (_smallItemPrototype.transform as RectTransform).rect.size.x / 2f;
		UIHelperUtil.UpdateUIList(count, _smallItems, _smallItemPrototype, _smallItemsContainer, setElement, null, (int i, UIOutpostEquipmentItemElement el) =>
		{
			Transform transform = new GameObject("Pivot", typeof(RectTransform)).transform;
			transform.SetParent(_smallItemsContainer, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			el.transform.SetParent(transform, worldPositionStays: false);
			el.transform.localPosition = new Vector3(radius, 0f, 0f);
		});
		PositionSmallItems();
		_onItemSlotClicked = onItemClick;
		_onItemSlotHover = onHover;
		_onItemSlotDoubleClick = onDoubleClick;
		_tooltipData = tooltipData;
		void setElement(int index, UIOutpostEquipmentItemElement el)
		{
			if (index < smallItems.Count)
			{
				el.SetItem(smallItems[index], OnItemSlotClicked, OnHover, OnDoubleClick);
			}
			else
			{
				int num = 2 * Mathf.Min(index, 4) + 1;
				el.SetEmpty(num <= characterLevel, ItemEquipType.Small, num, OnItemSlotClicked, OnHover, OnDoubleClick);
			}
		}
	}

	private void OnDisable()
	{
		_onItemSlotClicked = null;
		_onItemSlotHover = null;
		_onItemSlotDoubleClick = null;
		Selected?.Deselect();
		Selected = null;
		_tooltipData = null;
	}

	private UIOutpostEquipmentItemElement GetSlot(ItemEquipType itemEquipType)
	{
		UIOutpostEquipmentItemElement result = null;
		switch (itemEquipType)
		{
		case ItemEquipType.Head:
			result = _headSlot;
			break;
		case ItemEquipType.Feet:
			result = _feetSlot;
			break;
		case ItemEquipType.Body:
			result = _bodySlot;
			break;
		case ItemEquipType.OneHandWeapon:
			result = _oneHandedSlot;
			break;
		case ItemEquipType.TwoHandWeapon:
			result = _twoHandedSlot;
			break;
		}
		return result;
	}

	private void PositionSmallItems()
	{
		float num = 25f;
		float num2 = 45f;
		float num3 = (180f - (num2 + num)) / (float)(Mathf.Max(_smallItems.Count, 5) - 1);
		float num4 = 90f - num;
		for (int i = 0; i < _smallItems.Count; i++)
		{
			UIOutpostEquipmentItemElement uIOutpostEquipmentItemElement = _smallItems[i];
			if (uIOutpostEquipmentItemElement.isActiveAndEnabled)
			{
				float z = num4 - (float)i * num3;
				uIOutpostEquipmentItemElement.transform.parent.rotation = Quaternion.Euler(0f, 0f, z);
				uIOutpostEquipmentItemElement.transform.rotation = Quaternion.identity;
			}
		}
	}

	private void OnItemSlotClicked(UIOutpostEquipmentItemElement itemSlot)
	{
		_onItemSlotClicked?.Invoke(itemSlot);
		Selected?.Deselect();
		Selected = itemSlot;
		Selected?.Select();
		if (itemSlot.Data != null)
		{
			ShowTooltip(itemSlot.Data, itemSlot.transform.position);
		}
		else
		{
			HideTooltip();
		}
	}

	private void ShowTooltip(ItemDataDTO itemData, Vector3 pos, bool showActions = true)
	{
	}

	private void HideTooltip(bool immediate = false)
	{
	}

	private void OnHover(UIOutpostEquipmentItemElement itemSlot, bool hover)
	{
		if (itemSlot.Data != null && !(Selected == itemSlot))
		{
			if (hover)
			{
				_onItemSlotHover?.Invoke(itemSlot, arg2: true);
				ShowTooltip(itemSlot.Data, itemSlot.transform.position, showActions: false);
			}
			else
			{
				_onItemSlotHover?.Invoke(itemSlot, arg2: false);
				HideTooltip();
			}
		}
	}

	private void OnDoubleClick(UIOutpostEquipmentItemElement item)
	{
		if (!(item == null) && item.Data != null)
		{
			_onItemSlotDoubleClick?.Invoke(item.Data.ID);
		}
	}
}
