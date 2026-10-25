using FHGUI.DTO;

namespace FHGUI.Elements;

public class IconListContainer : ListContainerBase<FHSimpleIcon, IconListContainer.ListParams, SpriteWithTooltipDTO>
{
	public class ListParams
	{
	}

	protected override void InitElement(FHSimpleIcon listElement, SpriteWithTooltipDTO elementParams, ListParams listParams)
	{
		listElement.Init(elementParams);
	}
}
