using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseActiveBonusTooltip : MonoBehaviour
{
	private enum ETooltipType
	{
		Item,
		Bonus
	}

	[SerializeField]
	private UIActiveAbility activeAbilityTooltip;

	[SerializeField]
	private UIItemTooltipTarget itemTooltip;

	[SerializeField]
	private RectTransform itemTooltipAnchor;

	private ETooltipType type;

	public void Init(CActiveBonus bonus)
	{
		Reset();
		if (bonus.BaseCard is CItem item)
		{
			type = ETooltipType.Item;
			if (itemTooltip != null)
			{
				ItemDTOUtilities.GetFullItemInfo(item, out var itemData, out var flipItem);
				itemTooltip.Initialize(new FHItemTooltip.ItemTooltipParams(itemData, flipItem), new TooltipPositionParams
				{
					Anchor = TextAnchor.LowerLeft,
					TransformAnchor = itemTooltipAnchor
				}, manualShowHide: true);
			}
		}
		else
		{
			type = ETooltipType.Bonus;
			activeAbilityTooltip.Initialize(bonus, null, 1, forceGeneration: true);
		}
	}

	private void Reset()
	{
		if (itemTooltip != null)
		{
			itemTooltip.HideTooltip();
		}
		activeAbilityTooltip.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		if (type == ETooltipType.Item)
		{
			if (itemTooltip != null)
			{
				itemTooltip.ShowManually();
			}
		}
		else if (type == ETooltipType.Bonus)
		{
			activeAbilityTooltip.gameObject.SetActive(value: true);
			SummonContainer componentInChildren = activeAbilityTooltip.GetComponentInChildren<SummonContainer>();
			if (componentInChildren != null)
			{
				componentInChildren.SummonLT.SetActive(value: false);
				componentInChildren.SummonLB.SetActive(value: false);
				componentInChildren.SummonMT.SetActive(value: false);
				componentInChildren.SummonMB.SetActive(value: false);
				componentInChildren.GetComponentInChildren<LayoutElement>().flexibleWidth = 0f;
			}
		}
	}

	public void Hide()
	{
		if (itemTooltip != null)
		{
			itemTooltip.HideTooltip();
		}
		UIManager.Instance.UnhighlightElement(itemTooltip.gameObject, unlockUI: false);
		UIManager.Instance.UnhighlightElement(activeAbilityTooltip.gameObject, unlockUI: false);
		activeAbilityTooltip.gameObject.SetActive(value: false);
		SummonContainer componentInChildren = activeAbilityTooltip.GetComponentInChildren<SummonContainer>();
		if (componentInChildren != null)
		{
			componentInChildren.SummonLT.SetActive(value: true);
			componentInChildren.SummonLB.SetActive(value: true);
			componentInChildren.SummonMT.SetActive(value: true);
			componentInChildren.SummonMB.SetActive(value: true);
			componentInChildren.GetComponentInChildren<LayoutElement>().flexibleWidth = 1f;
		}
	}

	public void Clear()
	{
		if (itemTooltip != null)
		{
			itemTooltip.HideTooltip();
		}
	}
}
