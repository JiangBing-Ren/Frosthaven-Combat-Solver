using FHRL;

namespace FHGUI.Elements.Buildings;

public struct ResourceBuildingData : IBuildingInfoParams
{
	public OutpostMaterialType Material;

	public UIBuildInfoResourceActionElement.ResourceBuildingActionData? BuyAction;

	public UIBuildInfoResourceActionElement.ResourceBuildingActionData? SellAction;

	public UIBuildInfoResourceActionElement.ResourceBuildingActionData? SellAtHighPriceAction;
}
