using FHRL;

namespace FHGUI.DTO.Outpost;

public struct BuildingCostDTO
{
	public OutpostResources Resources;

	public OutpostResources RawResources;

	public uint Gold;

	public uint Morale;

	public uint Inspiration;

	public uint? TownLevel;

	public OutpostResources CurrentResources;

	public uint CurrentGold;

	public int CurrentMorale;

	public uint CurrentInspiration;

	public uint? CurrentTownLevel;
}
