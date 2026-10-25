using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class RetirementMaterialDistributionViewState : FHMapViewState
{
	public Action<List<ResourceDistribution>> OnConfirm;

	private DistributionsDTO _distributions;

	private DistributionChangeDTO _resourceChange;

	private CharacterState _characterState;

	private string _selectedAdditionalQuestID;

	[Preserve]
	private DistributionModule DistributionModule => View.Modules.DistributionModule;

	[Preserve]
	private DistributionModalModule DistributionModalModule => GetModule<DistributionModalModule>();

	private ResourceAllocationService ResourceAllocationService => View.GetService<ResourceAllocationService>();

	public RetirementMaterialDistributionViewState(CharacterState character, string questId = null)
	{
		_characterState = character;
		_selectedAdditionalQuestID = questId;
		_distributions = new DistributionsDTO();
		_distributions.Resources = View.GetService<RosterViewService>().GetRetireDistribution(_characterState);
		_resourceChange = new DistributionChangeDTO
		{
			Resources = new OutpostResourceChange
			{
				Resources = _characterState.Resources.ToMaterialResources()
			},
			ChangeType = DistributionChangeType.Gain
		};
	}

	public override void OnShow()
	{
		DistributionModalModule.gameObject.SetActive(value: false);
		DistributionModule.Params obj = new DistributionModule.Params
		{
			Table = ResourceAllocationService.GetDefaultResourceTable(_resourceChange, _distributions, OnShow),
			Title = "GUI_RESOURCE_ALLOCATION_CAPTION",
			Description = "GUI_RETIRE_DISTRIBUTE_RESOURCES_DESCRIPTION",
			IsDone = () =>
			{
				OutpostResources resources = _resourceChange.Resources.Resources;
				int num = _resourceChange.Resources.Total;
				foreach (ResourceDistribution resource in _distributions.Resources)
				{
					resources -= resource.Resources;
					num -= resource.Resources.Total;
				}
				return num == 0;
			},
			OnDone = OnDone,
			HasDistributionAuthority = true,
			DoneButtonKey = "GUI_CONFIRM",
			CancelButtonKey = "GUI_CANCEL"
		};
		DistributionModule.Init(obj);
	}

	private void OnResourceDistributionChanged(ResourceDistribution distr)
	{
		OnShow();
	}

	private void OnDone()
	{
		PopThisState();
		OnConfirm?.Invoke(_distributions.Resources);
	}
}
