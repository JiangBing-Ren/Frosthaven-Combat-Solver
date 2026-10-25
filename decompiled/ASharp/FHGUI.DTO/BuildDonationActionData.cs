using FHRL;

namespace FHGUI.DTO;

public class BuildDonationActionData
{
	public BuildingAction Start;

	public BuildingAction Cancel;

	public BuildingAction Donate;

	public BuildingAction FinalizeDonations;

	public BuildDonationActionData()
	{
		Start = (BuildingTypeID id) =>
		{
		};
		Cancel = (BuildingTypeID id) =>
		{
		};
		Donate = (BuildingTypeID id) =>
		{
		};
		FinalizeDonations = (BuildingTypeID id) =>
		{
		};
	}

	public BuildDonationActionData(BuildingAction start, BuildingAction cancel, BuildingAction donate, BuildingAction finalize)
	{
		Start = start;
		Cancel = cancel;
		Donate = donate;
		FinalizeDonations = finalize;
	}

	public void BindActions(BuildingAction start, BuildingAction cancel, BuildingAction donate, BuildingAction finalize)
	{
		Start = start;
		Cancel = cancel;
		Donate = donate;
		FinalizeDonations = finalize;
	}

	public void BindActions(BuildDonationActionData data)
	{
		Start = data.Start;
		Cancel = data.Cancel;
		Donate = data.Donate;
		FinalizeDonations = data.FinalizeDonations;
	}

	public void Clear()
	{
		Start = null;
		Cancel = null;
		Donate = null;
		FinalizeDonations = null;
	}
}
