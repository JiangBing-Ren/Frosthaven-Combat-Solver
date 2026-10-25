using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;

namespace FHGUI.Elements;

public class UIEquipmentContainerElement : MonoBehaviour
{
	[Header("Slots")]
	[SerializeField]
	protected UIOutpostEquipmentItemElement _headSlot;

	[SerializeField]
	protected UIOutpostEquipmentItemElement _feetSlot;

	[SerializeField]
	protected UIOutpostEquipmentItemElement _bodySlot;

	[SerializeField]
	protected List<UIOutpostEquipmentItemElement> _oneHandSlots;

	[SerializeField]
	protected UIOutpostEquipmentItemElement _twoHandSlot;

	[Space]
	[SerializeField]
	protected RectTransform _equippedContainer;

	[SerializeField]
	protected RectTransform _smallItemsContainer;

	[SerializeField]
	protected UIOutpostEquipmentItemElement _smallItemPrototype;

	[Header("Tooltips")]
	[SerializeField]
	protected GameObject _tooltipOverridePosition;

	protected List<UIOutpostEquipmentItemElement> _smallItems;

	protected List<UIOutpostEquipmentItemElement> _equipmentSlots;

	private UIItemInteractionEvents _interactionEvents;

	private FHItemTooltip.ItemTooltipParamsFunc _tooltipData;

	private bool _setup;

	public UIOutpostEquipmentItemElement Selected { get; private set; }

	public IReadOnlyList<UIOutpostEquipmentItemElement> SmallItems => _smallItems.AsReadOnly();

	public IReadOnlyList<UIOutpostEquipmentItemElement> EquipmentSlots => _equipmentSlots.AsReadOnly();

	protected virtual void InitCache()
	{
		_smallItems = _smallItemsContainer.GetComponentsInChildren<UIOutpostEquipmentItemElement>().ToList();
		_setup = true;
		_equipmentSlots = new List<UIOutpostEquipmentItemElement> { _headSlot, _feetSlot, _bodySlot, _twoHandSlot };
		_equipmentSlots.AddRange(_oneHandSlots);
	}

	public virtual void Init(IEnumerable<ItemDataDTO> items, uint characterLevel, uint unlockedSmallItemCount, uint maxSmallItems, uint maxOneHandSlots, FHItemTooltip.ItemTooltipParamsFunc tooltipDataFunc = null, UIItemInteractionEvents interactionEvents = null)
	{
		if (!_setup)
		{
			InitCache();
		}
		_interactionEvents = interactionEvents;
		_tooltipData = tooltipDataFunc;
		IEnumerable<ItemDataDTO> enumerable = items.Where((ItemDataDTO i) => i.IsEquipment);
		List<ItemDataDTO> source = items.Where((ItemDataDTO i) => i.IsWeapon).ToList();
		List<UIOutpostEquipmentItemElement> usedSlots = new List<UIOutpostEquipmentItemElement>();
		foreach (ItemDataDTO item2 in enumerable)
		{
			UIOutpostEquipmentItemElement slot = GetSlot(item2.ItemType);
			if (slot == null)
			{
				Debug.LogError("Cannot find slot for item " + item2.NameKey);
				continue;
			}
			slot.SetItem(item2, OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(item2), GetTooltipParams(slot));
			usedSlots.Add(slot);
		}
		for (int num = 0; num < _oneHandSlots.Count; num++)
		{
			if (num < maxOneHandSlots)
			{
				_oneHandSlots[num].SetActive(active: true);
			}
			else
			{
				_oneHandSlots[num].SetActive(active: false);
			}
		}
		List<ItemDataDTO> list = source.ToList();
		if (list.Count != 0)
		{
			ItemDataDTO itemDataDTO = list.FirstOrDefault((ItemDataDTO w) => w.ItemType == ItemEquipType.TwoHandWeapon);
			if (itemDataDTO != null)
			{
				UIOutpostEquipmentItemElement slot2 = GetSlot(ItemEquipType.TwoHandWeapon);
				slot2.SetItem(itemDataDTO, OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(itemDataDTO), GetTooltipParams(slot2), shared: false);
				usedSlots.Add(slot2);
				UIOutpostEquipmentItemElement slot3 = GetSlot(ItemEquipType.OneHandWeapon);
				slot3.SetItem(itemDataDTO, OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(itemDataDTO), GetTooltipParams(slot2));
				usedSlots.Add(slot3);
				list.Remove(itemDataDTO);
			}
			else
			{
				if (list.Count >= 1)
				{
					UIOutpostEquipmentItemElement slot4 = GetSlot(ItemEquipType.OneHandWeapon);
					slot4.SetItem(list[0], OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(list[0]), GetTooltipParams(slot4));
					list.Remove(list[0]);
					usedSlots.Add(slot4);
				}
				if (list.Count >= 1)
				{
					UIOutpostEquipmentItemElement slot5 = GetSlot(ItemEquipType.TwoHandWeapon);
					slot5.SetItem(list[0], OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(list[0]), GetTooltipParams(slot5));
					list.Remove(list[0]);
					usedSlots.Add(slot5);
				}
			}
			if (list.Count >= 1)
			{
				int num2 = 1;
				foreach (ItemDataDTO item3 in list)
				{
					UIOutpostEquipmentItemElement slot6 = GetSlot(ItemEquipType.OneHandWeapon, num2);
					slot6.SetItem(item3, OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(item3), GetTooltipParams(slot6));
					num2++;
					usedSlots.Add(slot6);
				}
			}
		}
		List<ItemDataDTO> smallItems = items.Where((ItemDataDTO item) => item.ItemType == ItemEquipType.Small).ToList();
		int count = (int)Math.Max(smallItems.Count, maxSmallItems);
		int num3 = (int)Math.Ceiling((float)characterLevel / 2f);
		int extraSmallItems = (int)unlockedSmallItemCount - num3;
		UIHelperUtil.UpdateUIList(count, _smallItems, _smallItemPrototype, _smallItemsContainer, setElement);
		foreach (UIOutpostEquipmentItemElement item4 in _equipmentSlots.Where((UIOutpostEquipmentItemElement s) => !usedSlots.Contains(s)))
		{
			item4.SetEmpty(isAvailable: true, GetSlotType(item4), 0, OnItemSlotClicked, OnHover, OnDoubleClick, OnRightClick);
		}
		void setElement(int index, UIOutpostEquipmentItemElement el)
		{
			if (index < smallItems.Count)
			{
				ItemDataDTO item = smallItems[index];
				el.SetItem(item, OnItemSlotClicked, OnHover, OnDoubleClick, null, _tooltipData?.Invoke(item), GetTooltipParams(el));
				usedSlots.Add(el);
			}
			else
			{
				int levelRequirement = 2 * Mathf.Min(index - extraSmallItems, (int)(maxSmallItems - 1)) + 1;
				el.SetEmpty(index + 1 <= unlockedSmallItemCount, ItemEquipType.Small, levelRequirement, OnItemSlotClicked, OnHover, OnDoubleClick);
			}
		}
	}

	private TooltipPositionParams GetTooltipParams(UIOutpostEquipmentItemElement slot)
	{
		return new TooltipPositionParams
		{
			TransformAnchor = ((_tooltipOverridePosition != null) ? (_tooltipOverridePosition.transform as RectTransform) : (slot.transform as RectTransform)),
			Anchor = TextAnchor.UpperRight
		};
	}

	private ItemEquipType GetSlotType(UIOutpostEquipmentItemElement slot)
	{
		if (slot == _headSlot)
		{
			return ItemEquipType.Head;
		}
		if (slot == _feetSlot)
		{
			return ItemEquipType.Feet;
		}
		if (slot == _bodySlot)
		{
			return ItemEquipType.Body;
		}
		if (slot == _twoHandSlot)
		{
			return ItemEquipType.TwoHandWeapon;
		}
		if (_oneHandSlots.Contains(slot))
		{
			return ItemEquipType.OneHandWeapon;
		}
		return ItemEquipType.Small;
	}

	private void OnDisable()
	{
		_interactionEvents = null;
		Selected?.Deselect();
		Selected = null;
		_tooltipData = null;
	}

	protected UIOutpostEquipmentItemElement GetSlot(ItemEquipType itemEquipType, int oneHandSlotIndex = 0)
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
			result = _oneHandSlots[oneHandSlotIndex];
			break;
		case ItemEquipType.TwoHandWeapon:
			result = _twoHandSlot;
			break;
		}
		return result;
	}

	protected void OnItemSlotClicked(UIOutpostEquipmentItemElement itemSlot)
	{
		if (itemSlot.Data != null)
		{
			_interactionEvents?.OnClick?.Invoke(itemSlot.Data);
		}
		Selected?.Deselect();
		Selected = itemSlot;
		Selected?.Select();
	}

	protected void OnHover(UIOutpostEquipmentItemElement itemSlot, bool hover)
	{
		if (itemSlot.Data != null && !(Selected == itemSlot))
		{
			if (hover)
			{
				_interactionEvents?.OnHover?.Invoke(itemSlot.Data, isFlagRaised: true);
			}
			else
			{
				_interactionEvents?.OnHover?.Invoke(itemSlot.Data, isFlagRaised: false);
			}
		}
	}

	protected void OnDoubleClick(UIOutpostEquipmentItemElement item)
	{
		if (!(item == null) && item.Data != null)
		{
			_interactionEvents?.OnDoubleClick?.Invoke(item.Data);
		}
	}

	protected void OnRightClick(UIOutpostEquipmentItemElement item)
	{
		if (!(item == null) && item.Data != null)
		{
			_interactionEvents?.OnRightClick?.Invoke(item.Data);
		}
	}
}
