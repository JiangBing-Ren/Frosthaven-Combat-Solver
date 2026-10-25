using System;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class EventDistributionViewState : FHMapViewState
{
	private const string SYNC_EVENT_DISTRIBUTION_COMPLETE = "EVENT_DISTRIBUTION_COMPLETE";

	private readonly ResourceAllocationService ResourceAllocationService;

	private CampaignUnresolvedAction _unresolvedAction;

	private DistributionsDTO _distributions;

	private bool _readyCheckPassed;

	public Action OnResolved;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private DistributionModalModule DistributionModalModule => GetModule<DistributionModalModule>();

	[Preserve]
	private DistributionModule DistributionModule => View.Modules.DistributionModule;

	public EventDistributionViewState(CampaignUnresolvedAction unresolvedAction)
	{
		_unresolvedAction = unresolvedAction;
		ResourceAllocationService = View.GetService<ResourceAllocationService>();
		DistributionModalModule.OnResourceDistributionChanged = ResourceAllocationService.OnResourceDistributionChanged;
		DistributionModalModule.OnGoldDistributionChanged = ResourceAllocationService.OnGoldDistributionChanged;
		DistributionModalModule.OnItemDistributionChanged = ResourceAllocationService.OnItemDistributionChanged;
		DistributionModalModule.OnBuildingDistributionChanged = ResourceAllocationService.OnBuildingDistributionChanged;
		DistributionModalModule.OnPetDistributionChanged = ResourceAllocationService.OnPetDistributionChanged;
	}

	public override void OnShow()
	{
		DistributionModalModule.OnDone = SyncAndDone;
		_distributions = new DistributionsDTO();
		PlayerDTO playerDTO = new PlayerDTO(this);
		DistributionModalModule.gameObject.SetActive(value: false);
		DistributionModule.Params data = new DistributionModule.Params
		{
			Title = "GUI_RESOURCE_ALLOCATION_CAPTION",
			ReadyUsers = playerDTO.ReadyUsers,
			HasDistributionAuthority = ResourceAllocationService.HasDistributionAuthority(playerDTO),
			OnDone = SyncAndDone,
			ReadyCheckPassed = _readyCheckPassed
		};
		ResourceAllocationService.Populate_EventDistribution(ref data, _unresolvedAction, _distributions, playerDTO.IsReady);
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
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("EVENT_DISTRIBUTION_COMPLETE", OnDone, () =>
		{
			_readyCheckPassed = false;
		});
	}

	private void OnDone()
	{
		if (View.IsPartyLeader)
		{
			ResourceAllocationService.FinalizeEventDistributionLegacy(_unresolvedAction, _distributions);
			PopThisState();
			OnResolved?.Invoke();
		}
	}
}
