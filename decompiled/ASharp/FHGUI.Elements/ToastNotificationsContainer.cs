using System.Collections.Generic;
using DG.Tweening;
using FHGUI.Elements.Notifications;
using FHGUI.Services;
using Libs.GameConsole;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class ToastNotificationsContainer : MonoBehaviour
{
	private class ToastNotificationData
	{
		public ToastNotification UIElement;

		public float TimeLeft;
	}

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private ToastNotification _notificationPrefab;

	[Header("Settings")]
	[SerializeField]
	private float _duration = 5f;

	[SerializeField]
	private float _spacing = 10f;

	[Header("Animation")]
	[SerializeField]
	private float _slideDuration = 0.35f;

	[SerializeField]
	private float _fadeDuration = 0.25f;

	private const int _maxNotifications = 3;

	private const string POOL_NAME = "TOAST_NOTIFICATION";

	private readonly List<ToastNotificationData> _activeNotifications = new List<ToastNotificationData>(3);

	private readonly Queue<string> _notifications = new Queue<string>(50);

	public void ShowNotification(string notificationText)
	{
		_notifications.Enqueue(notificationText);
	}

	private void OnDisable()
	{
		Clear();
	}

	public void Clear()
	{
		for (int num = _activeNotifications.Count - 1; num >= 0; num--)
		{
			ToastNotificationData toastNotificationData = _activeNotifications[num];
			DOTween.Kill(toastNotificationData.UIElement.gameObject);
			ObjectPool.Recycle(toastNotificationData.UIElement.gameObject, _notificationPrefab.gameObject, "TOAST_NOTIFICATION");
		}
		_activeNotifications.Clear();
		_notifications.Clear();
	}

	private void HideNotification(ToastNotification notification)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(notification.CanvasGroup.DOFade(0f, _fadeDuration));
		sequence.Join(notification.RectTransform.DOAnchorPosY(notification.RectTransform.anchoredPosition.y + 20f, _fadeDuration));
		sequence.OnComplete(() =>
		{
			ObjectPool.Recycle(notification.gameObject, _notificationPrefab.gameObject, "TOAST_NOTIFICATION");
			UpdatePositions();
		});
	}

	public void Update()
	{
		for (int num = _activeNotifications.Count - 1; num >= 0; num--)
		{
			ToastNotificationData toastNotificationData = _activeNotifications[num];
			toastNotificationData.TimeLeft -= Time.deltaTime;
			if (toastNotificationData.TimeLeft <= 0f)
			{
				_activeNotifications.RemoveAt(num);
				HideNotification(toastNotificationData.UIElement);
			}
		}
		bool flag = false;
		string result;
		while (_activeNotifications.Count < 3 && _notifications.TryPeek(out result))
		{
			string t = _notifications.Dequeue();
			ToastNotification toastNotification = ObjectPool.Spawn(_notificationPrefab, _container, forceInstantiate: true);
			toastNotification.Init(t);
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(toastNotification.RectTransform);
			float num2 = (1f - _container.pivot.y) * _container.rect.height;
			for (int i = 0; i < _activeNotifications.Count; i++)
			{
				float height = _activeNotifications[i].UIElement.Height;
				num2 = num2 - (1f - toastNotification.RectTransform.pivot.y) * height - ((1f - toastNotification.RectTransform.pivot.y) * height + _spacing);
			}
			toastNotification.RectTransform.anchoredPosition = new Vector2(toastNotification.RectTransform.anchoredPosition.x, num2 - ((1f - toastNotification.RectTransform.pivot.y) * toastNotification.Height + _spacing));
			toastNotification.CanvasGroup.DOFade(1f, _fadeDuration);
			_activeNotifications.Add(new ToastNotificationData
			{
				UIElement = toastNotification,
				TimeLeft = _duration
			});
			flag = true;
		}
		if (flag)
		{
			UpdatePositions();
		}
	}

	private void UpdatePositions()
	{
		float num = (1f - _container.pivot.y) * _container.rect.height;
		for (int i = 0; i < _activeNotifications.Count; i++)
		{
			ToastNotification uIElement = _activeNotifications[i].UIElement;
			float height = uIElement.Height;
			float num2 = num - (1f - uIElement.RectTransform.pivot.y) * height;
			uIElement.RectTransform.DOAnchorPosY(num2, _slideDuration).SetEase(Ease.OutCubic);
			num = num2 - ((1f - uIElement.RectTransform.pivot.y) * height + _spacing);
		}
	}

	[ConsoleCommand(Command = "test_toast")]
	public static void DebugAddNotifications(IConsole console, string t)
	{
		Singleton<FHMapChoreographer>.InstanceFast.View.GetService<OutpostNotificationViewService>().ShowToastNotification(t);
	}
}
