using System.Collections.Generic;
using ScenarioRuleLibrary;

public abstract class ElementPickController : PickController
{
	protected UIElementPicker picker;

	protected List<ElementInfusionBoardManager.EElement> excludedElements;

	protected ElementPickController(UIElementPicker picker)
	{
		this.picker = picker;
	}

	public ElementPickController SetExcludedElements(List<ElementInfusionBoardManager.EElement> excludedElements)
	{
		this.excludedElements = excludedElements;
		return this;
	}

	public override bool IsSelecting()
	{
		return picker.IsOpen;
	}

	public override void ClosePicker()
	{
		picker.Hide();
	}

	public void OnDestroy()
	{
	}
}
