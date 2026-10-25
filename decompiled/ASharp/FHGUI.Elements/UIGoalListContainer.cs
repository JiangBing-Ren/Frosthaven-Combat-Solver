using System;
using System.Collections.Generic;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIGoalListContainer : ListContainerBase<UIGoalSummary, UIGoalListContainer.GoalListParams, GoalDTO>
{
	public class GoalListParams
	{
		public string InfoLiteral;

		public Action<GoalDTO> OnListElementClicked;

		public string InteractionHints;
	}

	[Header("Info Field")]
	[SerializeField]
	private TMP_Text m_TextInfo;

	protected override void InitList(GoalListParams listParams, IList<GoalDTO> listElementParams)
	{
		base.InitList(listParams, listElementParams);
		if (!(m_TextInfo == null))
		{
			m_TextInfo.text = listParams.InfoLiteral;
		}
	}

	protected override void InitElement(UIGoalSummary listElement, GoalDTO elementParams, GoalListParams listParams)
	{
		UIGoalSummary.Params obj = new UIGoalSummary.Params
		{
			DTO = elementParams,
			OnClickedCallback = () =>
			{
				listParams.OnListElementClicked?.Invoke(elementParams);
			},
			IgnoreLocalization = false,
			HintsLiteral = listParams.InteractionHints
		};
		listElement.Init(obj);
	}
}
