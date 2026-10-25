using System.Collections.Generic;
using FHGUI.Common.Elements;
using FHGUI.Elements;
using FHGUI.Modules;
using UnityEngine;

namespace FHGUI.Common;

public class UISummonListContainer : ListContainerBase<FHInteractableSimpleIcon, UISimpleInteractionEvents, HeroSummonGeneralDTO>
{
	[SerializeField]
	private GameObject _tooltipPrototype;

	private string _hints;

	public void Init(UISimpleInteractionEvents events, IList<HeroSummonGeneralDTO> summons, string hints = null)
	{
		_hints = hints;
		base.Init(events, summons);
	}

	protected override void InitElement(FHInteractableSimpleIcon listElement, HeroSummonGeneralDTO summon, UISimpleInteractionEvents events)
	{
		if (_tooltipPrototype != null)
		{
			UIPrefabTooltipTarget component = listElement.GetComponent<UIPrefabTooltipTarget>();
			if (component != null)
			{
				component.Initialize(_tooltipPrototype, (GameObject go) =>
				{
					go.GetComponent<UISummonTooltip>()?.Init(new UISummonTooltip.TooltipParams
					{
						Summon = summon,
						Hints = _hints
					});
				});
			}
		}
		listElement.Init(summon.ToSimpleIconDTO(), events, summon);
	}
}
