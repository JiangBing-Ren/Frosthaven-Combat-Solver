using System;

namespace FHGUI.DTO;

public class QuickAccessActionsDTO
{
	public BuildingFlagAction OnBuildingSelected;

	public Action<UIBuildingStateFilter> OnBuildingFilterClicked;

	public Action OnBuildButtonClicked;

	public Action OnOutpostButtonClicked;

	public Action OnWorldMapButtonClicked;
}
