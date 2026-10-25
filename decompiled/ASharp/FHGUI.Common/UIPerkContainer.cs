using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Common;

public class UIPerkContainer : ListContainerBase<UIPerkElement, object, PerkDTO>
{
	[SerializeField]
	private GameObject _tooltipPrototype;

	protected override void InitElement(UIPerkElement listElement, PerkDTO perk, object listParams)
	{
		if (_tooltipPrototype != null && !string.IsNullOrEmpty(perk.DescriptionLocalized))
		{
			new UIPrefabTooltipTarget.TooltipInitParams
			{
				Prefab = _tooltipPrototype,
				OnObjectCreatedCallback = (GameObject go) =>
				{
					UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
					{
						Icon = perk.Icon,
						CaptionLiteral = perk.NameLocalized,
						DescriptionLiteral = perk.DescriptionLocalized,
						Hints = null
					};
					go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
				}
			};
		}
		listElement.Init(perk, null);
	}
}
