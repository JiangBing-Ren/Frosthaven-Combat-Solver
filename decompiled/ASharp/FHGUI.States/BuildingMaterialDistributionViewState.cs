using System;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class BuildingMaterialDistributionViewState : FHMapViewState
{
	private string Title;

	private readonly BuildingState _buildingState;

	private readonly DistributionsDTO _distributions;

	private readonly OutpostResourceChange _change;

	public Action<BuildingMaterialDistribution> OnConfirm;

	[Preserve]
	private DistributionModalModule DistributionModalModule => GetModule<DistributionModalModule>();

	[Preserve]
	private DistributionModule DistributionModule => View.Modules.DistributionModule;

	private ResourceAllocationService ResourceAllocationService => View.GetService<ResourceAllocationService>();

	public BuildingMaterialDistributionViewState(BuildingState state)
	{
		_buildingState = state;
		_distributions = ResourceAllocationService.GetBuildingRepairDistribution(_buildingState, out _change);
	}

	public override void OnShow()
	{
		bool flag = false;
		DistributionModalModule.gameObject.SetActive(flag);
		DistributionModule.gameObject.SetActive(!flag);
		if (flag)
		{
			DistributionModalModule.Init_BuildingDistribution(Title, _distributions.BuildingMaterial, 0u, _change, GameState, new PlayerDTO(this));
			DistributionModalModule.OnDone = () =>
			{
				if (_distributions.BuildingMaterial.IsValid)
				{
					OnConfirm?.Invoke(_distributions.BuildingMaterial);
				}
				OnDone();
			};
			DistributionModalModule.OnCancel = PopThisState;
			return;
		}
		DistributionModule.Params data = new DistributionModule.Params
		{
			Title = "GUI_RESOURCE_ALLOCATION_CAPTION",
			OnDone = () =>
			{
				if (_distributions.BuildingMaterial.IsValid)
				{
					OnConfirm?.Invoke(_distributions.BuildingMaterial);
				}
				OnDone();
			},
			OnCancel = PopThisState,
			HasDistributionAuthority = true,
			DoneButtonKey = "GUI_CONFIRM",
			CancelButtonKey = "GUI_CANCEL"
		};
		ResourceAllocationService.Populate_BuildingDistribution(ref data, _distributions.BuildingMaterial, 0u, _change, OnShow);
		DistributionModule.Init(data);
	}

	private void OnDone()
	{
		PopThisState();
	}
}
