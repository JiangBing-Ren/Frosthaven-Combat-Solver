using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOnAttackCondition : MonoBehaviour
{
	[SerializeField]
	private TMP_Text conditionName;

	[SerializeField]
	private Image conditionIcon;

	public void Initialize(CCondition.ENegativeCondition condition, ElementInfusionBoardManager.EElement? infuse = null, string onDeath = null)
	{
		if (condition != CCondition.ENegativeCondition.NA)
		{
			conditionName.text = CreateLayout.LocaliseText("$AttacksApply$ $" + condition.ToString() + "$");
			conditionIcon.sprite = UIInfoTools.Instance.GetIconNegativeCondition(condition);
		}
		else if (infuse.HasValue)
		{
			conditionName.text = CreateLayout.LocaliseText("$AttacksInfuse$ ");
			conditionIcon.sprite = UIInfoTools.Instance.GetElementIcon(infuse.Value);
		}
		else
		{
			conditionName.text = CreateLayout.LocaliseText(onDeath);
			conditionIcon.sprite = UIInfoTools.Instance.DamageSprite;
		}
	}
}
