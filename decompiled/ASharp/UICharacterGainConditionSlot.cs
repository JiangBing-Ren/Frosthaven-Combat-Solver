using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterGainConditionSlot : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	[TextArea]
	private string tooltipTitleFormat = "<size=+2>{0}</size>";

	public void SetCondition(CCondition.EPositiveCondition condition)
	{
		icon.sprite = UIInfoTools.Instance.GetIconPositiveCondition(condition);
		tooltip.SetText(string.Format(tooltipTitleFormat, LocalizationManager.GetTranslation($"GUI_GIFT_{condition}")), refreshTooltip: false, LocalizationManager.GetTranslation($"Glossary_{condition}"));
	}

	public void SetCondition(CCondition.ENegativeCondition condition, int strength = 1)
	{
		icon.sprite = UIInfoTools.Instance.GetIconNegativeCondition(condition);
		tooltip.SetText(string.Format(tooltipTitleFormat, LocalizationManager.GetTranslation($"GUI_GIFT_{condition}"), strength), refreshTooltip: false, LocalizationManager.GetTranslation($"Glossary_{condition}"));
	}
}
