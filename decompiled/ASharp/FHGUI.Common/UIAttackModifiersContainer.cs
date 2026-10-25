using FHGUI.Elements;
using GLOOM;
using UnityEngine;

namespace FHGUI.Common;

public class UIAttackModifiersContainer : ListContainerBase<UIAttackModifierElement, object, AttackModifierInfo>
{
	[SerializeField]
	private GameObject _tooltipPrototype;

	protected override void InitElement(UIAttackModifierElement listElement, AttackModifierInfo mod, object data)
	{
		if (_tooltipPrototype != null)
		{
			UIPrefabTooltipTarget component = listElement.GetComponent<UIPrefabTooltipTarget>();
			if (component != null)
			{
				component.Initialize(_tooltipPrototype, (GameObject go) =>
				{
					go.GetComponent<UIGenericTooltipRenderer>()?.Init(new UIGenericTooltipRenderer.Params
					{
						Icon = mod.Icon,
						CaptionLiteral = LocalizationManager.GetTranslation("GUI_ATTACKMODIFIERS_LABEL"),
						DescriptionLiteral = LocalizationManager.GetTranslation(mod.DescriptionLocKey)
					});
				});
			}
		}
		listElement.Init(mod);
	}
}
