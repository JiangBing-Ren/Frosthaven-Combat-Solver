using ScenarioRuleLibrary;
using UnityEngine;

internal class AbilityOption : IOption
{
	private CAbility ability;

	public string ID => ability.Name;

	public AbilityOption(CAbility ability)
	{
		this.ability = ability;
	}

	public string GetPickerText()
	{
		return PreviewEffectGenerator.GenerateDescription(ability);
	}

	public Sprite GetPickerIcon()
	{
		if (!PreviewEffectGenerator.TryGetAbilityOverrideIcon(ability, out var icon))
		{
			return null;
		}
		return icon;
	}

	public string GetSelectedText()
	{
		return "<sprite name=\"" + PreviewEffectGenerator.GetIcon(ability.AbilityType) + "\">";
	}

	public Color GetSelectedTextColor()
	{
		return UIInfoTools.Instance.White;
	}
}
