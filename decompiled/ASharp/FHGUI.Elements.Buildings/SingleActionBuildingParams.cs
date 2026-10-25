using System;

namespace FHGUI.Elements.Buildings;

public struct SingleActionBuildingParams : IBuildingInfoParams
{
	public Action Action;

	public string ActionLocKey;

	public string ActionHintLocKey;

	public bool IsInteractable;
}
