using System;
using System.Collections.Generic;
using FHGUI.Elements;
using UnityEngine;

namespace FHGUI.Common;

public class UIConditionEffectListContainer : ListContainerBase<UIConditionEffectElement, UIPrefabTooltipTarget.TooltipInitParams, StatusEffectDTO>
{
	[SerializeField]
	private GameObject _tooltipPrototype;

	private int _keepAtlLeast;

	private List<StatusEffectDTO> _statuses = new List<StatusEffectDTO>();

	public void Init(IList<StatusEffectDTO> statuses, int keepAtlLeast = 0)
	{
		_keepAtlLeast = keepAtlLeast;
		_statuses.Clear();
		if (statuses != null)
		{
			_statuses.AddRange(statuses);
		}
		int num = statuses?.Count ?? 0;
		if (num < _keepAtlLeast)
		{
			for (int i = num; i < _keepAtlLeast; i++)
			{
				_statuses.Add(null);
			}
		}
		Init(null, _statuses);
	}

	protected override void InitElement(UIConditionEffectElement listElement, StatusEffectDTO status, UIPrefabTooltipTarget.TooltipInitParams tooltipInitParams)
	{
		if (status == null)
		{
			listElement.SetEmpty();
			return;
		}
		UIPrefabTooltipTarget.TooltipInitParams tooltipInitParams2 = null;
		if (_tooltipPrototype != null)
		{
			tooltipInitParams2 = new UIPrefabTooltipTarget.TooltipInitParams
			{
				Prefab = _tooltipPrototype,
				OnObjectCreatedCallback = (GameObject go) =>
				{
					UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
					{
						Icon = status.Icon,
						CaptionLiteral = status.Name,
						DescriptionLiteral = status.Description,
						Hints = null
					};
					go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
				}
			};
		}
		listElement.Init(status, tooltipInitParams2);
	}

	private Color GetColor(StatusEffectDTO.StatusEffectType effectType)
	{
		return effectType switch
		{
			StatusEffectDTO.StatusEffectType.Positive => UIInfoTools.DefInstance.positiveTextColor, 
			StatusEffectDTO.StatusEffectType.Negative => UIInfoTools.DefInstance.negativeTextColor, 
			StatusEffectDTO.StatusEffectType.Permanent => UIInfoTools.DefInstance.basicTextColor, 
			_ => throw new ArgumentOutOfRangeException("effectType", effectType, null), 
		};
	}
}
