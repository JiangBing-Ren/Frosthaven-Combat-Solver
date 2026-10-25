namespace FHGUI.Elements.Buildings;

public class UIBuildingActionListContainer : ListContainerBase<UIBuildInfoActionElement, object, BuildingFunctionalityAction>
{
	protected override void InitElement(UIBuildInfoActionElement listElement, BuildingFunctionalityAction elementParams, object listParams)
	{
		listElement.Init(elementParams);
	}
}
