using FHGUI.DTO.Outpost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Notifications;

[RequireComponent(typeof(FHButton))]
public class UIOutpostNotificationDescriptionElement : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private Image _icon;

	private FHButton _button;

	private OutpostNotificationEvent _onDismiss;

	public OutpostNotificationDTO Data { get; private set; }

	private void Awake()
	{
		_button = GetComponent<FHButton>();
		_button.onClick.AddListener(OnClick);
		_button.onRightClick.AddListener(OnRightClick);
	}

	private void OnDisable()
	{
		_onDismiss = null;
		Data = null;
	}

	private void OnRightClick()
	{
		_onDismiss?.Invoke(Data);
	}

	private void OnClick()
	{
		Data.OnClick?.Invoke();
	}

	public void Init(OutpostNotificationDTO dto, OutpostNotificationEvent onDismissNotification)
	{
		Data = dto;
		_description.text = dto.Description;
		_icon.enabled = dto.Icon != null;
		_icon.sprite = dto.Icon;
		_onDismiss = onDismissNotification;
	}
}
