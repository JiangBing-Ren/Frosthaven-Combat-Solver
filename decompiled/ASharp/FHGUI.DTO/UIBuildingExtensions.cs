using FHRL;

namespace FHGUI.DTO;

public static class UIBuildingExtensions
{
	public static BuildingTypeID ToBuildingTypeID(this BuildingId id)
	{
		return new BuildingTypeID((uint)id);
	}

	public static bool IsEqual(this BuildingId id1, BuildingTypeID id2)
	{
		return id1 == (BuildingId)id2.ID;
	}

	public static bool IsEqual(this BuildingTypeID id1, BuildingId id2)
	{
		return id1.ID == (uint)id2;
	}

	public static BuildingId ToBuildingID(this BuildingTypeID id)
	{
		return (BuildingId)id.ID;
	}
}
