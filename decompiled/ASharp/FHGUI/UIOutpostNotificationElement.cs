using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using FHGUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI;

public class UIOutpostNotificationElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public struct OutpostNotificationElementData
	{
		private readonly List<OutpostNotificationDTO> _notifications;

		public string TooltipText { get; private set; }

		public Sprite Icon { get; private set; }

		public Sprite SelectedIcon { get; private set; }

		public Sprite Background { get; private set; }

		public IReadOnlyCollection<OutpostNotificationDTO> Notifications => _notifications;

		public int Count => _notifications.Count();

		public OutpostNotificationType NotificationType { get; private set; }

		public OutpostNotificationElementData(OutpostNotificationType notificationType, IEnumerable<OutpostNotificationDTO> notifications, string tooltipText, Sprite icon, Sprite selectedIcon, Sprite background)
		{
			NotificationType = notificationType;
			_notifications = new List<OutpostNotificationDTO>(notifications);
			TooltipText = tooltipText;
			Icon = icon;
			SelectedIcon = selectedIcon;
			Background = background;
		}
	}

	public struct OutpostNotificationParams
	{
		public OutpostNotificationTypeEvent OnClickGroup;

		public OutpostNotificationTypeEvent OnDismissGroup;

		public OutpostNotificationEvent OnDismiss;

		public Predicate<OutpostNotificationDTO> CanDismiss;

		public Func<OutpostNotificationType, IEnumerable<OutpostNotificationDTO>, string> GetTooltip;
	}

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _selectedIcon;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _selectedBackground;

	[SerializeField]
	private TMP_Text _stacks;

	[SerializeField]
	private GameObject _stacksContainer;

	[Header("Notification Info")]
	[SerializeField]
	private UIOutpostNotificationDescriptionContainer _descriptionContainer;

	private Action<UIOutpostNotificationElement> _onClick;

	private OutpostNotificationElementData _data;

	private readonly List<OutpostNotificationDTO> _notifications = new List<OutpostNotificationDTO>();

	private OutpostNotificationParams? _notificationParams;

	public OutpostNotificationType NotificationType => _data.NotificationType;

	public IEnumerable<OutpostNotificationDTO> Notifications => _data.Notifications;

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
		_button.onRightClick.AddListener(OnRightClick);
	}

	private void OnDisable()
	{
		_onClick = null;
		_notificationParams = null;
	}

	public void Init(OutpostNotificationElementData data, OutpostNotificationParams notificationParams, Action<UIOutpostNotificationElement> onClick)
	{
		_data = data;
		_icon.sprite = data.Icon;
		if (_selectedIcon != null)
		{
			_selectedIcon.sprite = data.SelectedIcon;
		}
		_background.sprite = data.Background;
		_background.enabled = data.Background != null;
		_button.interactable = data.Notifications.Any();
		OnDeselect();
		_onClick = onClick;
		_notificationParams = notificationParams;
		int count = _data.Count;
		_stacks.text = count.ToString();
		_stacksContainer.SetActive(count > 1);
		_notifications.Clear();
		_notifications.AddRange(data.Notifications);
		_descriptionContainer.Init(notificationParams, _notifications);
	}

	public void ShowCarousel(bool immediate = false)
	{
		_descriptionContainer.ShowCarousel(immediate);
	}

	public void HideCarousel(bool immediate = false)
	{
		_descriptionContainer.HideCarousel(immediate);
	}

	public void ToggleCarousel(bool immediate = false)
	{
		_descriptionContainer.ToggleCarousel(immediate);
	}

	private void OnClick()
	{
		_onClick?.Invoke(this);
		_notificationParams?.OnClickGroup?.Invoke(NotificationType);
	}

	private void OnRightClick()
	{
		_notificationParams?.OnDismissGroup?.Invoke(NotificationType);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnSelect();
		if (_data.Notifications.Any())
		{
			ShowCarousel();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnDeselect();
		HideCarousel();
	}

	private void OnSelect()
	{
		if (_selectedIcon != null)
		{
			_selectedIcon.enabled = true;
		}
		if (_selectedBackground != null)
		{
			_selectedBackground.enabled = true;
		}
	}

	private void OnDeselect()
	{
		if (_selectedIcon != null)
		{
			_selectedIcon.enabled = false;
		}
		if (_selectedBackground != null)
		{
			_selectedBackground.enabled = false;
		}
	}
}
