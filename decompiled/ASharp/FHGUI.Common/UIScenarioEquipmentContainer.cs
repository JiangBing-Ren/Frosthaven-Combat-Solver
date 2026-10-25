using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;
using UnityEngine.Pool;

namespace FHGUI.Common;

public class UIScenarioEquipmentContainer : MonoBehaviour
{
	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private UIOutpostEquipmentItemElement _itemPrefab;

	[Header("Slots")]
	[SerializeField]
	private UIOutpostEquipmentItemElement _headSlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _bodySlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _feetSlot;

	[SerializeField]
	private UIOutpostEquipmentItemElement _weaponSlot1;

	[SerializeField]
	private UIOutpostEquipmentItemElement _weaponSlot2;

	[SerializeField]
	private UIOutpostEquipmentItemElement _weaponSlot3;

	[SerializeField]
	private UIOutpostEquipmentItemElement _smallItem;

	private List<UIOutpostEquipmentItemElement> _itemSlots = new List<UIOutpostEquipmentItemElement>();

	private List<ItemDataDTO> _items = new List<ItemDataDTO>();

	private void InitCache()
	{
		if (_itemSlots.Count <= 0)
		{
			_container.GetComponentsInChildren(_itemSlots);
		}
	}

	public void Init(IList<ItemDataDTO> items)
	{
		InitCache();
		_items.Clear();
		int num = 7;
		List<ItemDataDTO> value;
		using (CollectionPool<List<ItemDataDTO>, ItemDataDTO>.Get(out value))
		{
			value.AddRange(items);
			SetItemInSlot(items, _headSlot, ItemEquipType.Head, out var item);
			SetItemInSlot(items, _bodySlot, ItemEquipType.Body, out var item2);
			SetItemInSlot(items, _feetSlot, ItemEquipType.Feet, out var item3);
			value.Remove(item);
			value.Remove(item2);
			value.Remove(item3);
			_items.Add(item);
			_items.Add(item2);
			_items.Add(item3);
			if (SetItemInSlot(items, _weaponSlot1, ItemEquipType.TwoHandWeapon, out var item4))
			{
				SetItem(item4, _weaponSlot2, ItemEquipType.TwoHandWeapon, isAvailable: false);
				ItemDataDTO itemDataDTO = items.FirstOrDefault((ItemDataDTO itemDataDTO5) => itemDataDTO5.ItemType == ItemEquipType.OneHandWeapon);
				if (itemDataDTO != null)
				{
					SetItem(itemDataDTO, _weaponSlot3, ItemEquipType.OneHandWeapon);
				}
				else
				{
					_weaponSlot3.SetEmpty(isAvailable: true, ItemEquipType.OneHandWeapon);
				}
				value.Remove(item4);
				value.Remove(itemDataDTO);
				_items.Add(item4);
				_items.Add(item4);
				_items.Add(itemDataDTO);
			}
			else
			{
				IEnumerable<ItemDataDTO> source = items.Where((ItemDataDTO itemDataDTO5) => itemDataDTO5.ItemType == ItemEquipType.OneHandWeapon);
				ItemDataDTO item5 = source.ElementAtOrDefault(0);
				ItemDataDTO item6 = source.ElementAtOrDefault(1);
				ItemDataDTO itemDataDTO2 = source.ElementAtOrDefault(2);
				SetItem(item5, _weaponSlot1, ItemEquipType.OneHandWeapon);
				SetItem(item6, _weaponSlot2, ItemEquipType.OneHandWeapon);
				if (itemDataDTO2 != null)
				{
					SetItem(itemDataDTO2, _weaponSlot3, ItemEquipType.OneHandWeapon);
				}
				else
				{
					_weaponSlot3.SetEmpty(isAvailable: true, ItemEquipType.OneHandWeapon);
				}
				value.Remove(item5);
				value.Remove(item6);
				value.Remove(itemDataDTO2);
				_items.Add(item5);
				_items.Add(item6);
				_items.Add(itemDataDTO2);
			}
			ItemDataDTO itemDataDTO3 = items.FirstOrDefault((ItemDataDTO itemDataDTO5) => itemDataDTO5.ItemType == ItemEquipType.Small);
			SetItem(itemDataDTO3, _smallItem, ItemEquipType.Small);
			value.Remove(itemDataDTO3);
			if (itemDataDTO3 != null)
			{
				_items.Add(itemDataDTO3);
			}
			_items.AddRange(value);
		}
		int num2 = Mathf.Max(num, _items.Count);
		HelperTools.NormalizePool(ref _itemSlots, _itemPrefab.gameObject, _container, num2);
		for (int num3 = num; num3 < num2; num3++)
		{
			ItemDataDTO itemDataDTO4 = _items[num3];
			SetItem(itemDataDTO4, _itemSlots[num3], itemDataDTO4.ItemType);
		}
		if (_weaponSlot3.Data == null)
		{
			_weaponSlot3.gameObject.SetActive(value: false);
		}
	}

	private bool SetItemInSlot(IList<ItemDataDTO> items, UIOutpostEquipmentItemElement slot, ItemEquipType slotType, out ItemDataDTO item, bool isAvailable = true)
	{
		item = items.FirstOrDefault((ItemDataDTO itemDataDTO) => itemDataDTO.ItemType == slotType);
		return SetItem(item, slot, slotType, isAvailable);
	}

	private bool SetItem(ItemDataDTO item, UIOutpostEquipmentItemElement slot, ItemEquipType slotType, bool isAvailable = true)
	{
		if (item == null)
		{
			slot.SetEmpty(isAvailable, slotType);
			return false;
		}
		FHItemTooltip.ItemTooltipParams tooltipParams = new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemDTOUtilities.GetFlipItem(item),
			Data = default
		};
		bool shared = isAvailable;
		slot.SetItem(item, null, null, null, null, tooltipParams, null, shared);
		return true;
	}
}
