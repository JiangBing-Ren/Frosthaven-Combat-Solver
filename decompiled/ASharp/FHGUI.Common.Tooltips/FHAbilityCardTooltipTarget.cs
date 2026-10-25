using UnityEngine;

namespace FHGUI.Common.Tooltips;

public class FHAbilityCardTooltipTarget : FHTooltipTarget<UIAbilityCardTooltip, UIAbilityCardTooltip.FHAbilityCardTooltipParams>
{
	[SerializeField]
	private UIAbilityCardTooltip _overrideTooltipPrefab;

	private UIAbilityCardTooltip.FHAbilityCardTooltipParams _tooltipParams;

	public override UIAbilityCardTooltip TooltipIPrefab
	{
		get
		{
			if (!(_overrideTooltipPrefab != null))
			{
				return UIInfoTools.Instance.CardTooltip;
			}
			return _overrideTooltipPrefab;
		}
	}

	protected override void OnInitialize(UIAbilityCardTooltip.FHAbilityCardTooltipParams data, TooltipPositionParams? positionParams = null)
	{
		_tooltipParams = data;
		_positionParams = positionParams;
	}

	protected override bool CanShowTooltip()
	{
		UIAbilityCardTooltip.FHAbilityCardTooltipParams tooltipParams = _tooltipParams;
		if ((object)tooltipParams == null)
		{
			return false;
		}
		return tooltipParams.AbilityCardID > 0;
	}

	protected override void OnShowInternal()
	{
		if (_tooltipParams == null)
		{
			HideTooltip();
		}
		else
		{
			TooltipInstance.Show(_tooltipParams);
		}
	}

	protected override void OnHideInternal()
	{
		TooltipInstance.Hide();
	}
}
