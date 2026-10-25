using ScenarioRuleLibrary;

public interface IInfuseElement
{
	ElementInfusionBoardManager.EElement SelectedElement { get; }

	bool IsAnyElement { get; }

	void PickElement(ElementInfusionBoardManager.EElement element);

	void ResetElementToInitial();
}
