using System;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;

namespace FHGUI.Common.Elements;

public class FHInteractableIconContainer : ListContainerBase<FHInteractableSimpleIcon, UISimpleInteractionEvents, Tuple<SpriteWithTooltipDTO, object>>
{
	protected override void InitElement(FHInteractableSimpleIcon listElement, Tuple<SpriteWithTooltipDTO, object> elementParams, UISimpleInteractionEvents interactions = null)
	{
		listElement.Init(elementParams.Item1, interactions, elementParams.Item2);
	}
}
