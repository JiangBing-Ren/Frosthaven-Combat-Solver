using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Tooltips;

public class UIItemTooltipTarget : FHTooltipTarget<FHItemTooltip, FHItemTooltip.ItemTooltipParams>
{
	[SerializeField]
	private FHItemTooltip _overrideTooltipPrefab;

	private FHItemTooltip.ItemTooltipParams _itemTooltipParams;

	public override FHItemTooltip TooltipIPrefab
	{
		get
		{
			if (!(_overrideTooltipPrefab != null))
			{
				return UIInfoTools.Instance.ItemTooltip;
			}
			return _overrideTooltipPrefab;
		}
	}

	protected override void OnInitialize(FHItemTooltip.ItemTooltipParams data, TooltipPositionParams? positionParams = null)
	{
		_itemTooltipParams = data;
	}

	protected override bool CanShowTooltip()
	{
		return _itemTooltipParams != null;
	}

	protected override void OnShowInternal()
	{
		if (_itemTooltipParams == null)
		{
			HideTooltip();
			return;
		}
		_itemTooltipParams.AnchorTarget = AnchorTarget;
		TooltipInstance.Show(_itemTooltipParams);
		UIUtility.ForceRecalclulateLayouts(TooltipInstance.gameObject.transform as RectTransform);
	}

	protected override void OnHideInternal()
	{
		TooltipInstance.Hide();
	}
}
