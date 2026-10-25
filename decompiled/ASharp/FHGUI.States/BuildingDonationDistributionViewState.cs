using System.Linq;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class BuildingDonationDistributionViewState : FHMapViewState
{
	private BuildingTypeDef _buildingDef;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private DistributionModule DistributionModule => View.Modules.DistributionModule;

	private ResourceAllocationService ResourceAllocationService => View.GetService<ResourceAllocationService>();

	public BuildingDonationDistributionViewState(BuildingTypeDef buildingDef)
	{
		_buildingDef = buildingDef;
	}

	public override void OnShow()
	{
		BuildingState buildingState = View.GameState.Outpost.Buildings.FirstOrDefault((BuildingState b) => b.Def == _buildingDef);
		if (buildingState?.donationDistribution == null)
		{
			PopThisState();
			return;
		}
		DistributionModule.Params data = new DistributionModule.Params
		{
			Table = null,
			Title = "GUI_BUNDLING_DONATION_CAPTION",
			Description = "GUI_BUNDLING_DONATION",
			IsDone = () => true,
			OnDone = OnDone,
			HasDistributionAuthority = true,
			DoneButtonKey = "GUI_CLOSE"
		};
		ResourceAllocationService.Populate_BuildingDonation(ref data, buildingState);
		DistributionModule.Init(data);
	}

	private void OnDone()
	{
		PopThisState();
	}
}
