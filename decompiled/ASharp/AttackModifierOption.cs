using ScenarioRuleLibrary.YML;
using UnityEngine;

internal class AttackModifierOption : IOption
{
	public AttackModifierYMLData ability { get; private set; }

	public string ID
	{
		get
		{
			if (ability != null)
			{
				return ability.Name;
			}
			return "None";
		}
	}

	public AttackModifierOption(AttackModifierYMLData ability)
	{
		this.ability = ability;
	}

	public string GetPickerText()
	{
		if (ability != null)
		{
			return ability.Card.AttackModifierLogString(null, skipLoc: true);
		}
		return "-";
	}

	public Sprite GetPickerIcon()
	{
		if (ability == null)
		{
			return null;
		}
		return UIInfoTools.Instance.GetPremadeAttackModifierIcon(ability);
	}

	public string GetSelectedText()
	{
		return GetPickerText();
	}
}
