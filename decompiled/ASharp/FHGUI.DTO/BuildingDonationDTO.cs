using FHRL;

namespace FHGUI.DTO;

public struct BuildingDonationDTO
{
	public CharacterID Character;

	public bool IsPartyLeader;

	public OutpostResources CurrentResources;

	public uint Gold;

	public uint Inspiration;

	public BuildingMaterialDistribution Distribution;

	public bool IsDone;
}
