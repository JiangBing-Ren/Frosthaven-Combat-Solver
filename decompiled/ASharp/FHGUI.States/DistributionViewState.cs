using System;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class DistributionViewState : FHMapViewState
{
	private const string SYNC_DISTRIBUTION_COMPLETE = "DISTRIBUTION_COMPLETE";

	private DistributionChangeDTO _change;

	private CampaignUnresolvedAction _action;

	private DistributionsDTO _distributions;

	private bool _readyCheckPassed;

	public Action OnResolved;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private DistributionModalModule DistributionModalModuleLegacy => View.Modules.DistributionModuleLegacy;

	[Preserve]
	private DistributionModule DistributionModule => View.Modules.DistributionModule;

	private ResourceAllocationService ResourceAllocationService => View.GetService<ResourceAllocationService>();

	public DistributionViewState(DistributionChangeDTO change, CampaignUnresolvedAction action = default(CampaignUnresolvedAction))
	{
		_change = change;
		_action = action;
		DistributionModalModuleLegacy.OnItemDistributionChanged = ResourceAllocationService.OnItemDistributionChanged;
		DistributionModalModuleLegacy.OnResourceDistributionChanged = ResourceAllocationService.OnResourceDistributionChanged;
		DistributionModalModuleLegacy.OnGoldDistributionChanged = ResourceAllocationService.OnGoldDistributionChanged;
	}

	public override void OnShow()
	{
		DistributionModalModuleLegacy.OnDone = SyncAndDone;
		DistributionModalModuleLegacy.gameObject.SetActive(value: false);
		DistributionModule.gameObject.SetActive(value: true);
		PlayerDTO playerDTO = new PlayerDTO(this);
		_distributions = ResourceAllocationService.PopulateDistributionLegacy(DistributionModalModuleLegacy, _change, new PlayerDTO(this));
		_distributions = ResourceAllocationService.GetDistributions(_change);
		DistributionModule.Params data = new DistributionModule.Params
		{
			Title = "GUI_RESOURCE_ALLOCATION_CAPTION",
			ReadyUsers = playerDTO.ReadyUsers,
			HasDistributionAuthority = ResourceAllocationService.HasDistributionAuthority(playerDTO),
			OnDone = SyncAndDone,
			ReadyCheckPassed = _readyCheckPassed
		};
		if (_change.Gold != 0)
		{
			ResourceAllocationService.Populate_DefaultGoldDistribution(ref data, _change, _distributions, playerDTO.IsReady);
		}
		if (_change.Resources.AnyMaterial > 0 || _change.Resources.Resources.HasMaterials)
		{
			ResourceAllocationService.Populate_DefaultMaterialDistribution(ref data, _change, _distributions, playerDTO.IsReady);
		}
		else if (_change.Resources.AnyHerb > 0)
		{
			ResourceAllocationService.Populate_DefaultMaterialDistribution(ref data, _change, _distributions, playerDTO.IsReady);
		}
		if (_change.Items != null || _change.LoseItem > 0)
		{
			ResourceAllocationService.Populate_DefaultItemDistribution(ref data, _change, _distributions, playerDTO.IsReady);
		}
		DistributionModule.Init(data);
		if (View.IsReadyCheckRunning && View.IsPartyLeader)
		{
			Func<bool> isValid = data.IsValid;
			if (isValid != null && !isValid())
			{
				View.GetService<MultiplayerSyncViewService>().CancelSyncPoint();
			}
		}
	}

	private void SyncAndDone()
	{
		_readyCheckPassed = true;
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("DISTRIBUTION_COMPLETE", OnDone, () =>
		{
			_readyCheckPassed = false;
		});
	}

	private void OnDone()
	{
		if (View.IsPartyLeader)
		{
			ResourceAllocationService.FinalizeDistribution(_change, _distributions);
			OnResolved?.Invoke();
		}
	}
}
