using System;
using System.Linq;
using FHGUI.DTO.Outpost;
using UnityEngine;

namespace FHGUI.DataSO;

[CreateAssetMenu(fileName = "NotificationUIDataSO", menuName = "Def/NotificationUIDataSO")]
public class NotificationUIDataSO : ScriptableObject
{
	[Serializable]
	public struct NotificationIcon
	{
		public OutpostNotificationType NotificationType;

		public Sprite Icon;

		public Sprite SelectedIcon;
	}

	[Header("Visuals")]
	public Sprite OutpostNotificationBackground;

	public Sprite PlayerNotificationBackground;

	public NotificationIcon[] Icons;

	public Sprite GetIcon(OutpostNotificationType notificationType)
	{
		return Icons?.FirstOrDefault((NotificationIcon t) => t.NotificationType == notificationType).Icon ?? null;
	}

	public Sprite GetSelectedIcon(OutpostNotificationType notificationType)
	{
		return Icons?.FirstOrDefault((NotificationIcon t) => t.NotificationType == notificationType).SelectedIcon ?? null;
	}
}
