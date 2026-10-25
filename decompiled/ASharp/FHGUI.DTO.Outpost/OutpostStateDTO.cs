using FHRL;

namespace FHGUI.DTO.Outpost;

public struct OutpostStateDTO(OutpostState state, CampaignState campaignState, uint inspiration)
{
	public OutpostResources Resources = campaignState.TotalPartyResources;

	public uint Gold = campaignState.TotalPartyGold;

	public uint Prosperity = state.Prosperity;

	public uint TownLevel = state.ProsperityLevel;

	public int Morale = state.Morale;

	public uint Inspiration = inspiration;
}
