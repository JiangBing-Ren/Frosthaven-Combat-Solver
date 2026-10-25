using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityTooltip : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Image titleIcon;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Transform descriptionContainer;

	[SerializeField]
	private UIItemTooltipTarget itemTooltip;

	[SerializeField]
	private Transform itemContainer;

	public void Initialize(IAbility ability, CPlayerActor player)
	{
		Clear();
		if (ability.DescriptionItem != null)
		{
			ItemDTOUtilities.GetFullItemInfo(ability.DescriptionItem, out var itemData, out var flipItem);
			if (itemTooltip != null)
			{
				itemTooltip.Initialize(new FHItemTooltip.ItemTooltipParams(itemData, flipItem), new TooltipPositionParams
				{
					Anchor = TextAnchor.LowerLeft,
					TransformAnchor = (itemContainer as RectTransform)
				}, manualShowHide: true);
				itemTooltip.enabled = true;
			}
			descriptionContainer.gameObject.SetActive(value: false);
			itemContainer.gameObject.SetActive(value: true);
			return;
		}
		titleText.text = ability.DescriptionTitle;
		if (player != null && ability.DescriptionTitle.IsNOTNullOrEmpty())
		{
			titleIcon.sprite = UIInfoTools.Instance.GetCharacterConfigUI(player.CharacterClass.CharacterModel, useDefault: true, player.CharacterClass.CharacterYML.CustomCharacterConfig).iconClass;
		}
		titleIcon.gameObject.SetActive(titleIcon.sprite);
		descriptionText.text = ability.DescriptionText;
		descriptionContainer.gameObject.SetActive(value: true);
		itemContainer.gameObject.SetActive(value: false);
		if (itemTooltip != null)
		{
			itemTooltip.enabled = false;
			itemTooltip.HideTooltip();
		}
	}

	public void Clear()
	{
		if (itemTooltip != null)
		{
			itemTooltip.HideTooltip();
		}
	}

	public void Show()
	{
		if (itemTooltip != null && itemTooltip.enabled)
		{
			itemTooltip.ShowManually();
		}
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		if (itemTooltip != null)
		{
			itemTooltip.HideTooltip();
		}
		gameObject.SetActive(value: false);
	}
}
