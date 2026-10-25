using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO;

public record QuickAccessBarDTO
{
	public MainViewMainCategories Context;

	public MainViewSubCategories SelectedCategory;

	public UIBuildingStateFilter BuildingFilter;

	public BuildingTypeID SelectedBuildingId;

	public IEnumerable<(MainViewSubCategories, QuickAccessTabDTO)> AvailableTabs;

	public bool HasSled;

	public bool HasBoat;

	public bool HasClimbingGear;

	public bool HasActiveScenario;
}
