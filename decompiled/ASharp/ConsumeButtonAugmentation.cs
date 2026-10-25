using System.Collections.Generic;
using ScenarioRuleLibrary;

public class ConsumeButtonAugmentation : AugmentationHolder
{
	public ConsumeButton ConsumeButton { get; private set; }

	public ConsumeButtonAugmentation(ConsumeButton button)
		: base(button.ID, button.AbilityConsume.ConsumeData)
	{
		ConsumeButton = button;
	}

	public override void DisactiveAugment()
	{
		base.DisactiveAugment();
		ConsumeButton.ClearSelection();
	}

	public override void ActiveAugment(List<ElementInfusionBoardManager.EElement> elements)
	{
		ConsumeButton.ShowSelected(elements);
		base.ActiveAugment(elements);
	}
}
