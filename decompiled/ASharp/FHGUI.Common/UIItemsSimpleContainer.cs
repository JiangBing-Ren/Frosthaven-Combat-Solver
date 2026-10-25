using System.Collections.Generic;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using FHGUI.Elements;

namespace FHGUI.Common;

public class UIItemsSimpleContainer : ListContainerBase<UIItemElement, object, ItemDataDTO>
{
	protected override void InitElement(UIItemElement listElement, ItemDataDTO item, object listParams)
	{
		listElement.Init(item, new FHItemTooltip.ItemTooltipParams
		{
			ItemData = item,
			FlipItem = ItemDTOUtilities.GetFlipItem(item),
			Data = default
		});
	}

	public void Init(IList<ItemDataDTO> items)
	{
		Init(null, items);
	}
}
