using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioBriefingSuggestibleList : ListContainerBase<UIScenarioBriefingSuggestibleSlot, UIScenarioBriefingSuggestibleList.ListParams, SuggestibleSlotDTO>, IUIViewModule
{
	public class ListParams
	{
		public UISuggestibleSlotInteractionEvents InteractionEvents;

		public Func<string, bool, UIPrefabTooltipTarget.TooltipInitParams> tooltipInitParams;

		public int CurrentCurrent;

		public int MaxCharges;
	}

	[SerializeField]
	private FHStacksCounter m_Charges;

	public GameObject GameObject => gameObject;

	protected override void InitList(ListParams listParams, IList<SuggestibleSlotDTO> listElementParams)
	{
		if ((bool)m_Charges)
		{
			m_Charges.Init(listParams.MaxCharges - listParams.CurrentCurrent, listParams.MaxCharges);
		}
		base.InitList(listParams, listElementParams);
	}

	protected override void InitElement(UIScenarioBriefingSuggestibleSlot listElement, SuggestibleSlotDTO elementParams, ListParams listParams)
	{
		listElement.Init(elementParams, listParams.InteractionEvents, listParams.tooltipInitParams?.Invoke(elementParams.Id, arg2: false));
	}
}
