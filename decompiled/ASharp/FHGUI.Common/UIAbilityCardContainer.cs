using System.Collections.Generic;
using FHGUI.Elements;
using FHGUI.Modules;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Common;

public class UIAbilityCardContainer : ListContainerBase<UIAbilityCardSlot, UIAbilityCardSlot.UIAbilityCardSlotContext, CAbilityCard>
{
	[SerializeField]
	private RectTransform _tooltipContainer;

	public void Init(IList<CAbilityCard> cards, CPlayerActor playerActor, ECharacter fallback = ECharacter.None, UIScenarioCardInteractionEvents events = null, List<CAbilityCard> selectedCard = null)
	{
		Init(new UIAbilityCardSlot.UIAbilityCardSlotContext
		{
			Events = events,
			PlayerActor = playerActor,
			Fallback = fallback,
			SelectedCard = selectedCard
		}, cards);
	}

	public void Clear()
	{
		foreach (UIAbilityCardSlot listElement in m_ListElements)
		{
			listElement.Clear();
		}
	}

	protected override void InitElement(UIAbilityCardSlot listElement, CAbilityCard elementParams, UIAbilityCardSlot.UIAbilityCardSlotContext listParams)
	{
		listElement.Init(elementParams, listParams, _tooltipContainer);
	}

	public void SetTooltipContainer(RectTransform rectTransform)
	{
		_tooltipContainer = rectTransform;
	}
}
