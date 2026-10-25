using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using UnityEngine;

namespace FHGUI.Elements;

public class RadialEquipmentContainerElement : UIEquipmentContainerElement
{
	public override void Init(IEnumerable<ItemDataDTO> items, uint characterLevel, uint unlockedSmallItemCount, uint maxSmallItems, uint maxOneHandSlots, FHItemTooltip.ItemTooltipParamsFunc tooltipDataFunc = null, UIItemInteractionEvents interactionEvents = null)
	{
		base.Init(items, characterLevel, unlockedSmallItemCount, maxSmallItems, maxOneHandSlots, tooltipDataFunc, interactionEvents);
		List<ItemDataDTO> smallItems = items.Where((ItemDataDTO item) => item.ItemType == ItemEquipType.Small).ToList();
		int count = (int)Math.Max(smallItems.Count, maxSmallItems);
		int num = (int)Math.Ceiling((float)characterLevel / 2f);
		int extraSmallItems = (int)unlockedSmallItemCount - num;
		float radius = ((RectTransform)transform).rect.size.x / 2f + ((RectTransform)_smallItemPrototype.transform).rect.size.x / 2f;
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
		void setElement(int index, UIOutpostEquipmentItemElement el)
		{
			if (index < smallItems.Count)
			{
				el.SetItem(smallItems[index], OnItemSlotClicked, OnHover, OnDoubleClick);
			}
			else
			{
				int levelRequirement = 2 * Mathf.Min(index - extraSmallItems, (int)(maxSmallItems - 1)) + 1;
				el.SetEmpty(index + 1 <= unlockedSmallItemCount, ItemEquipType.Small, levelRequirement, OnItemSlotClicked, OnHover, OnDoubleClick);
			}
		}
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
}
