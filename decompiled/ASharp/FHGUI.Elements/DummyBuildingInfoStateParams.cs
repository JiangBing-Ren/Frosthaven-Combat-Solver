using FHGUI.DTO;

namespace FHGUI.Elements;

public struct DummyBuildingInfoStateParams : IBuildingInfoStateParams
{
	public BuildDonationActionData DonationData { get; }

	public bool IsPartyLeader { get; }

	public bool CanStartProject { get; }

	public string StartProjectTooltip { get; set; }
}
