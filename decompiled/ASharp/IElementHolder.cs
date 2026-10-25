using ScenarioRuleLibrary;

public interface IElementHolder
{
	ElementInfusionBoardManager.EElement RequiredElement { get; }

	ElementInfusionBoardManager.EElement? SelectedElement { get; }

	void SetSelectedElement(ElementInfusionBoardManager.EElement? element);
}
