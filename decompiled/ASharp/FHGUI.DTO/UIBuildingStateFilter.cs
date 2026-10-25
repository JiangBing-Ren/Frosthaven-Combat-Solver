using System;

namespace FHGUI.DTO;

[Flags]
public enum UIBuildingStateFilter
{
	None = 0,
	Active = 1,
	Construct = 2,
	Upgradeable = 4,
	Wrecked = 8,
	All = Active | Construct | Upgradeable | Wrecked
}
