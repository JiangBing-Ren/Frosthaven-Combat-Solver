using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using FHGUI.Elements.Notifications;
using UnityEngine;

namespace FHGUI.Modules;

public class OutpostNotificationModule : FHMapViewModule
{
	[SerializeField]
	private UIOutpostNotificationElementContainer _notificationsContainer;

	[SerializeField]
	private UIStoryEventsNotification _storyEvents;

	private List<UIOutpostNotificationElement.OutpostNotificationElementData> _notificationsCache = new List<UIOutpostNotificationElement.OutpostNotificationElementData>();

	private Action _openStoryEvents;

	private readonly OutpostNotificationType[] _order = new OutpostNotificationType[13]
	{
		OutpostNotificationType.ConstructionProjectAvailable,
		OutpostNotificationType.EmptyGardenPlot,
		OutpostNotificationType.FullStables,
		OutpostNotificationType.BuildingWrecked,
		OutpostNotificationType.NewCraftingBlueprints,
		OutpostNotificationType.NewTraderItems,
		OutpostNotificationType.TownGuardHasUnspentPoints,
		OutpostNotificationType.NewJobPosting,
		OutpostNotificationType.HeroUnselectedCards,
		OutpostNotificationType.HeroUnspentPerkPoints,
		OutpostNotificationType.HeroLeveledUp,
		OutpostNotificationType.HeroReadyForRetirement,
		OutpostNotificationType.HeroRetired
	};

	protected override bool PlaySoundOnEnable => false;

	protected override void OnHide()
	{
		base.OnHide();
		_openStoryEvents = null;
	}

	public void Init(IEnumerable<OutpostNotificationDTO> notifications, UIOutpostNotificationElement.OutpostNotificationParams notificationParams, UIStoryEventsNotification.StoryEventParams storyEventParams)
	{
		UIInfoTools uiInfoTools = UIInfoTools.Instance;
		IOrderedEnumerable<UIOutpostNotificationElement.OutpostNotificationElementData> collection = from t in notifications.ToLookup((OutpostNotificationDTO dto) => dto.OutpostNotificationType)
			where t.Key != OutpostNotificationType.StoryEvents
			select new UIOutpostNotificationElement.OutpostNotificationElementData(t.Key, t, string.Empty, uiInfoTools.GetNotificationGroupImage(t.Key), uiInfoTools.GetNotificationGroupSelectedImage(t.Key), uiInfoTools.GetNotificationGroupBackground(t.Key)) into n
			orderby _order.IndexOf(n.NotificationType)
			select n;
		_notificationsCache.Clear();
		_notificationsCache.AddRange(collection);
		_notificationsContainer.Init(notificationParams, _notificationsCache);
		_storyEvents.Init(storyEventParams);
	}

	private void OnStoryEventsClick()
	{
		_openStoryEvents?.Invoke();
	}
}
