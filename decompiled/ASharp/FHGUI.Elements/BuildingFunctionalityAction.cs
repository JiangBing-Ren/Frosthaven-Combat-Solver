namespace FHGUI.Elements;

public struct BuildingFunctionalityAction
{
	public BuildingFunctionalityActionHandler OnClick;

	public bool IsInteractable;

	public string ActionText;

	public bool Localize;

	public string TooltipText;

	public int Charges;

	public int MaxCharges;
}
