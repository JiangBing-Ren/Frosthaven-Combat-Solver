using ScenarioRuleLibrary;
using UnityEngine;

public class AugmentationOption : AugmentationHolder, IOption
{
	public AugmentationOption(CActionAugmentation augmentation)
		: base(augmentation.Name, augmentation)
	{
	}

	public Sprite GetPickerIcon()
	{
		return null;
	}

	public string GetPickerText()
	{
		return PreviewEffectGenerator.GenerateDescription(Augmentation);
	}

	public string GetSelectedText()
	{
		if (Augmentation.CostAbility == null)
		{
			return GetPickerText();
		}
		return PreviewEffectGenerator.GenerateDescription(Augmentation.CostAbility);
	}
}
