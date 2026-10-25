using FHGUI.DTO;

namespace FHGUI.Elements;

public interface IBuildingInfoStateParams
{
	BuildDonationActionData DonationData { get; }

	bool IsPartyLeader { get; }

	bool CanStartProject { get; }

	string StartProjectTooltip { get; }
}
