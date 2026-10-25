using FHGUI.Elements;

namespace FHGUI.Common;

public class UILootListContainer : ListContainerBase<UILootElement, object, LootDTO>
{
	protected override void InitElement(UILootElement listElement, LootDTO elementParams, object listParams)
	{
		listElement.Init(elementParams);
	}
}
