using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class StoryEventLogViewState : FHMapViewState
{
	private List<StoryEventDTO> _events = new List<StoryEventDTO>();

	public override StateCategory Category => StateCategory.Outpost;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private StoryEventViewModule StoryEventModule => View.Modals.StoryEventModule;

	private OutpostNotificationViewService _notificationService => View.GetService<OutpostNotificationViewService>();

	public override void OnShow()
	{
		base.OnShow();
		_events.Clear();
		_events.AddRange(_notificationService.GetStoryEvents().Reverse());
		StoryEventModule.Init(_events, OnClick, OnClose);
	}

	private void OnClose()
	{
		PopThisState();
	}

	private void OnClick(StoryEventDTO data)
	{
		_notificationService.MarkStoryEventAsSeen(data);
		_notificationService.OpenCampaignSectionView(data);
	}
}
