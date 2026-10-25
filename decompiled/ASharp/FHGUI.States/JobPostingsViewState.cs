using FHGUI.DTO;
using FHGUI.Elements.JobPostings;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class JobPostingsViewState : FHMapViewState
{
	public override StateCategory Category => StateCategory.WorldMap;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private JobPostingsViewModule JobPostingModule => View.Modules.WorldMap.JobPostingsModule;

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private BuildingViewService BuildingViewService => View.GetService<BuildingViewService>();

	public override void OnShow()
	{
		QuickAccessBarService.Setup(MainViewMainCategories.WorldMap, MainViewSubCategories.JobPostings);
		JobPostingModule.Init(BuildingViewService.GetAllJobs(), new UIJobPostingInfo.JobPostingContext
		{
			AcceptJob = AcceptJob,
			IsHost = View.IsPartyLeader
		}, OnClose);
	}

	private void OnClose()
	{
		PopThisState();
	}

	private void AcceptJob(CampaignSectionID id)
	{
		if (BuildingViewService.AcceptJobPosting(id))
		{
			JobPostingModule.Init(BuildingViewService.GetAllJobs(), new UIJobPostingInfo.JobPostingContext
			{
				AcceptJob = AcceptJob,
				IsHost = View.IsPartyLeader
			}, OnClose);
		}
	}
}
