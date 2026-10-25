using System.Collections.Generic;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Modules;

public class RadialEquipmentViewModule : FHMapViewModule
{
	[Header("Slots")]
	[SerializeField]
	private RadialEquipmentContainerElement _container;

	public void Init(IEnumerable<ItemDataDTO> items, uint characterLevel, uint unlockedSmallItemCount, uint maxSmallItems, uint maxOneHandSlots, FHItemTooltip.ItemTooltipParamsFunc tooltipData = null, UIItemInteractionEvents interactionEvents = null)
	{
		_container.Init(items, characterLevel, unlockedSmallItemCount, maxSmallItems, maxOneHandSlots, tooltipData, interactionEvents);
	}
}
