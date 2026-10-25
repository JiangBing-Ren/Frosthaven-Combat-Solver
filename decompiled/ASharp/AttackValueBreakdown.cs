using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class AttackValueBreakdown : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI displayText;

	private bool isInitialized;

	private bool followMouse;

	private void Update()
	{
		if (followMouse)
		{
			transform.position = InputFHManager.CursorPosition;
		}
	}

	public void Init(CAttackSummary.TargetSummary attackSummary, int abilityAttackValue)
	{
		string text = "<size=80%><sprite name=Attack><size=100%> " + abilityAttackValue;
		if (attackSummary.Parent.AttackAbility != null)
		{
			foreach (CActiveBonus thisTargetAttackActiveBonuse in attackSummary.ThisTargetAttackActiveBonuses)
			{
				int num = thisTargetAttackActiveBonuse.ReferenceAbilityStrengthScalar(attackSummary.Parent.AttackAbility, attackSummary.Actor);
				if (num != 1)
				{
					text = "(" + text + ") x " + num;
				}
			}
		}
		foreach (CAttackSummary.ConsumeData strengthBuffConsume in attackSummary.Parent.StrengthBuffConsumes)
		{
			text += " + ";
			foreach (ElementInfusionBoardManager.EElement element in strengthBuffConsume.Elements)
			{
				text = text + "<sprite name=" + element.ToString() + ">";
			}
			text = text + " " + strengthBuffConsume.BuffAmount;
		}
		if (attackSummary.Poison)
		{
			text += " + <sprite name=Poison>1";
		}
		if (attackSummary.Parent.AttackAbility != null)
		{
			foreach (CActiveBonus thisTargetAttackActiveBonuse2 in attackSummary.ThisTargetAttackActiveBonuses)
			{
				int num2 = thisTargetAttackActiveBonuse2.ReferenceStrength(attackSummary.Parent.AttackAbility, attackSummary.Actor);
				if (num2 != 0)
				{
					text = text + string.Format(((num2 > 0) ? " + " : " - ") + "<sprite name=\"{0}\" color=white> ", UIInfoTools.Instance.GetActiveAbilityIconName(thisTargetAttackActiveBonuse2)) + Mathf.Abs(num2);
				}
			}
		}
		foreach (int conditionalOverridesStrengthBuff in attackSummary.ConditionalOverridesStrengthBuffs)
		{
			text = text + ((conditionalOverridesStrengthBuff > 0) ? " + " : " - ") + "<sprite name=\"Attack\" color=white> " + Mathf.Abs(conditionalOverridesStrengthBuff);
		}
		if (attackSummary.Parent.AttackAbility != null)
		{
			foreach (CActiveBonus thisTargetAttackActiveBonuse3 in attackSummary.ThisTargetAttackActiveBonuses)
			{
				int num3 = thisTargetAttackActiveBonuse3.ReferenceStrengthScalar(attackSummary.Parent.AttackAbility, attackSummary.Actor);
				if (num3 != 1)
				{
					text = "(" + text + ") x " + num3;
				}
			}
		}
		if (attackSummary.Shield > 0)
		{
			text = text + " - <size=80%><sprite name=Shield><size=100%> " + attackSummary.Shield;
			if (attackSummary.Pierce > 0)
			{
				text = text + " + <size=80%><sprite name=Pierce><size=100%> " + attackSummary.Pierce;
			}
		}
		text = "= " + text;
		displayText.text = text;
		isInitialized = true;
		followMouse = false;
	}

	public void Init(List<CActiveBonus> applicableActiveAttackBonuses, List<CActionAugmentation> currentActionSelectedAugmentations, CAction action)
	{
		int strength = action.FindType(CAbility.EAbilityType.Attack).Strength;
		int num = strength;
		string text = "<sprite name=Attack> " + strength;
		isInitialized = false;
		int num2 = 0;
		if (applicableActiveAttackBonuses != null)
		{
			foreach (CActiveBonus applicableActiveAttackBonuse in applicableActiveAttackBonuses)
			{
				int num3 = 0;
				foreach (CAbility ability in action.Abilities)
				{
					if (ability.AbilityType == CAbility.EAbilityType.Attack && ability is CAbilityAttack { TargetingActor: not null })
					{
						num3 += applicableActiveAttackBonuse.ReferenceStrength((CAbilityAttack)ability, null);
					}
				}
				if (num3 != 0)
				{
					text = text + string.Format(((num3 > 0) ? " + " : " - ") + "<sprite name=\"{0}\" color=white> ", UIInfoTools.Instance.GetActiveAbilityIconName(applicableActiveAttackBonuse)) + Mathf.Abs(num3);
					isInitialized = true;
				}
				num2 += num3;
			}
		}
		num += num2;
		foreach (CActionAugmentation currentActionSelectedAugmentation in currentActionSelectedAugmentations)
		{
			if (currentActionSelectedAugmentation.ActionID != action.ID)
			{
				continue;
			}
			int num4 = 0;
			foreach (CActionAugmentationOp augmentationOp in currentActionSelectedAugmentation.AugmentationOps)
			{
				if (augmentationOp.ParentAbilityType == CAbility.EAbilityType.Attack && augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride && augmentationOp.AbilityOverride.Strength.HasValue)
				{
					num4 += augmentationOp.AbilityOverride.Strength.Value;
				}
			}
			num += num4;
			if (num4 <= 0)
			{
				continue;
			}
			text += " + ";
			foreach (ElementInfusionBoardManager.EElement element in currentActionSelectedAugmentation.Elements)
			{
				text = text + "<sprite name=" + element.ToString() + ">";
			}
			text = text + " " + num4;
			isInitialized = true;
		}
		if (isInitialized)
		{
			text = text + " = <sprite name=Attack> " + num;
			displayText.text = text;
			transform.position = InputFHManager.CursorPosition;
			followMouse = true;
		}
	}

	public void Show()
	{
		if (isInitialized)
		{
			gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}
