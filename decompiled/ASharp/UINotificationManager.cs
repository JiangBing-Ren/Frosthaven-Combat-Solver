using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;
using UnityEngine.UI;

public class UINotificationManager : Singleton<UINotificationManager>
{
	public class NotificationData
	{
		public string titleLoc;

		public string message;

		public GameObject content;

		public Sprite icon;

		public NotificationDataButton dataButton1;

		public NotificationDataButton dataButton2;

		public string audioItem;
	}

	public class NotificationDataButton
	{
		public string buttonTextLoc;

		public Action clickCallback;

		public bool hideOnClick = true;

		public NotificationDataButton(Action clickCallback, string buttonTextLoc)
		{
			this.clickCallback = clickCallback;
			this.buttonTextLoc = buttonTextLoc;
		}
	}

	private class Notification : INotificationControl
	{
		public delegate void NotificationEventHandler(Notification notification);

		public readonly string ID;

		private readonly UINotification notification;

		public readonly string Tag;

		public event NotificationEventHandler OnHide;

		private event BasicEventHandler onHidden;

		public Notification(string id, UINotification uiNotification, string tag = null)
		{
			ID = id;
			notification = uiNotification;
			Tag = tag;
			notification.OnHidden.AddListener(OnHidden);
		}

		public void Hide(bool instant = false)
		{
			notification.Hide(instant);
			OnHide?.Invoke(this);
		}

		private void OnHidden()
		{
			notification.OnHidden.RemoveListener(OnHidden);
			onHidden?.Invoke();
		}

		public void RegisterToOnHidden(Action callback)
		{
			onHidden += () =>
			{
				callback();
			};
		}
	}

	public interface INotificationControl
	{
		void Hide(bool instant = false);

		void RegisterToOnHidden(Action callback);
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UINotification notificationPrefab;

	[SerializeField]
	private RectTransform notificationContainer;

	[SerializeField]
	private List<UINotification> poolNotifications = new List<UINotification>();

	private Dictionary<string, Notification> idNotifications = new Dictionary<string, Notification>();

	private List<Notification> activeNotifications = new List<Notification>();

	private HashSet<Component> hideRequests = new HashSet<Component>();

	protected override void AwakeInit()
	{
		for (int i = 0; i < poolNotifications.Count; i++)
		{
			UINotification notification = poolNotifications[i];
			notification.OnHidden.AddListener(() =>
			{
				OnHiddenNotification(notification);
			});
		}
	}

	public void Hide(Component request)
	{
		hideRequests.Add(request);
		window.Hide();
	}

	public void Show(Component request)
	{
		hideRequests.Remove(request);
		if (hideRequests.Count == 0)
		{
			window.Show();
		}
	}

	public INotificationControl ShowNotification(string titleLoc, string text, float durationDisplay, string id = null, string notificationTag = null)
	{
		return ShowNotification(new NotificationData
		{
			titleLoc = titleLoc,
			message = text
		}, durationDisplay, id, notificationTag);
	}

	public INotificationControl ShowNotification(string titleLoc, GameObject content, float durationDisplay, string id = null, string notificationTag = null)
	{
		return ShowNotification(new NotificationData
		{
			titleLoc = titleLoc,
			content = content
		}, durationDisplay, id, notificationTag);
	}

	public INotificationControl ShowNotification(string titleLoc, string text, Action buttonCallback, string buttonTextLoc, float durationDisplay, string id = null, string notificationTag = null)
	{
		return ShowNotification(new NotificationData
		{
			titleLoc = titleLoc,
			message = text,
			dataButton1 = ((buttonCallback == null) ? null : new NotificationDataButton(buttonCallback, buttonTextLoc))
		}, durationDisplay, id, notificationTag);
	}

	public INotificationControl ShowNotification(NotificationData data, float durationDisplay, string id = null, string notificationTag = null)
	{
		if (string.IsNullOrEmpty(id) || !idNotifications.ContainsKey(id))
		{
			UINotification notification = GetNotification();
			Notification notification2 = new Notification(id, notification, notificationTag);
			if (data.dataButton1 != null && data.dataButton1.hideOnClick)
			{
				Action buttonClickCallback = data.dataButton1.clickCallback;
				data.dataButton1.clickCallback = () =>
				{
					notification2.Hide();
					buttonClickCallback?.Invoke();
				};
			}
			if (data.dataButton2 != null && data.dataButton2.hideOnClick)
			{
				Action buttonClickCallback2 = data.dataButton2.clickCallback;
				data.dataButton2.clickCallback = () =>
				{
					notification2.Hide();
					buttonClickCallback2?.Invoke();
				};
			}
			notification.Show(data, id);
			if (durationDisplay > 0f)
			{
				Coroutine waitCoroutine = StartCoroutine(WaitNotification(() =>
				{
					notification2.Hide();
				}, durationDisplay));
				notification2.OnHide += (Notification n) =>
				{
					StopCoroutine(waitCoroutine);
				};
			}
			notification2.OnHide += OnHideNotification;
			if (!string.IsNullOrEmpty(id))
			{
				idNotifications[id] = notification2;
			}
			activeNotifications.Add(notification2);
			return notification2;
		}
		return idNotifications[id];
	}

	private void OnHideNotification(Notification notification)
	{
		if (!string.IsNullOrEmpty(notification.ID))
		{
			idNotifications.Remove(notification.ID);
		}
		activeNotifications.Remove(notification);
	}

	public INotificationControl ShowPermanentNotification(string id, string titleLoc, string text, string notificationTag = null)
	{
		return ShowPermanentNotification(id, titleLoc, text, null, null, notificationTag);
	}

	public INotificationControl ShowPermanentNotification(string id, string titleLoc, string text, Action buttonCallback, string buttonTextLoc, string notificationTag = null)
	{
		return ShowPermanentNotification(id, new NotificationData
		{
			titleLoc = titleLoc,
			message = text,
			dataButton1 = ((buttonCallback == null) ? null : new NotificationDataButton(buttonCallback, buttonTextLoc))
		}, notificationTag);
	}

	public INotificationControl ShowPermanentNotification(string id, NotificationData data, string notificationTag = null)
	{
		return ShowNotification(data, -1f, id, notificationTag);
	}

	public void HideNotification(string id, bool instant = false)
	{
		if (!string.IsNullOrEmpty(id) && idNotifications.ContainsKey(id))
		{
			idNotifications[id].Hide(instant);
		}
	}

	public void HideNotificationsByTag(string notificationTag, bool instant = true)
	{
		if (string.IsNullOrEmpty(notificationTag))
		{
			return;
		}
		foreach (Notification item in activeNotifications.FindAll((Notification it) => it.Tag == notificationTag))
		{
			item.Hide(instant);
		}
	}

	public void ClearNotifications(bool instant = true)
	{
		StopAllCoroutines();
		while (activeNotifications.Count > 0)
		{
			activeNotifications[0].Hide(instant);
		}
	}

	private IEnumerator WaitNotification(Action callback, float waitTime)
	{
		float time = 0f;
		while (time < waitTime)
		{
			if (window.IsOpen)
			{
				time += Timekeeper.instance.m_GlobalClock.deltaTime;
			}
			yield return null;
		}
		callback();
	}

	private UINotification GetNotification()
	{
		UINotification notification;
		if (poolNotifications.Count > 0)
		{
			notification = poolNotifications[poolNotifications.Count - 1];
			poolNotifications.RemoveAt(poolNotifications.Count - 1);
			notification.transform.SetAsLastSibling();
		}
		else
		{
			notification = UnityEngine.Object.Instantiate(notificationPrefab, notificationContainer);
			notification.OnHidden.AddListener(() =>
			{
				OnHiddenNotification(notification);
			});
		}
		return notification;
	}

	private void OnHiddenNotification(UINotification notification)
	{
		poolNotifications.Add(notification);
	}
}
