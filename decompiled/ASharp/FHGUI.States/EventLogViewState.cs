using FHGUI.Elements.Notifications;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class EventLogViewState : FHMapViewState
{
	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private OutpostEventLogModule EventLogModule => View.Modules.EventLogModule;

	private OutpostNotificationViewService NotificationViewService => View.GetService<OutpostNotificationViewService>();

	public override void OnShow()
	{
		EventLogModule.Init(NotificationViewService.GetFullEventLog(), new UIOutpostEventLogContainer.EventLogListParams
		{
			CurrentWeek = GameController.State.Outpost.Week
		}, OnClose);
	}

	private void OnClose()
	{
		PopThisState();
	}
}
