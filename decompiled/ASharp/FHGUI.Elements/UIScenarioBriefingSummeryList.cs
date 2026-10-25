using System;
using System.Collections.Generic;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIScenarioBriefingSummeryList : ListContainerBase<UIScenarioBriefingSuggestibleSlot, UIScenarioBriefingSummeryList.ListParams, SuggestibleSlotDTO>, IUIViewModule
{
	public class ListParams
	{
		public string CaptionLiteral;

		public IEnumerable<FHInputEventContainer.Params> Interactions;

		public Func<string, bool, UIPrefabTooltipTarget.TooltipInitParams> tooltipInitParams;
	}

	[SerializeField]
	private TMP_Text Text_Caption;

	[SerializeField]
	private TMP_Text Text_Empty;

	[SerializeField]
	private FHButton Button_GroupAction;

	[Header("Interaction Hints")]
	[SerializeField]
	private FHInputEventContainer m_InteractionHints;

	public GameObject GameObject => gameObject;

	protected override void InitList(ListParams listParams, IList<SuggestibleSlotDTO> listElementParams)
	{
		if ((bool)Text_Caption)
		{
			Text_Caption.text = listParams.CaptionLiteral;
		}
		if ((bool)Text_Empty)
		{
			Text_Empty.enabled = listElementParams.Count == 0;
		}
		if (m_InteractionHints != null)
		{
			m_InteractionHints.Init(listParams.Interactions);
		}
		base.InitList(listParams, listElementParams);
	}

	protected override void InitElement(UIScenarioBriefingSuggestibleSlot listElement, SuggestibleSlotDTO elementParams, ListParams listParams)
	{
		listElement.Init(elementParams, null, listParams.tooltipInitParams?.Invoke(elementParams.Id, arg2: true));
	}
}
