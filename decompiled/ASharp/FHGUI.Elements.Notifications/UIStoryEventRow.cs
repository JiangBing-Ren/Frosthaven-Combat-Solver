using System;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements.Notifications;

public class UIStoryEventRow : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener _weekText;

	[SerializeField]
	private TMP_Text _eventName;

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private GameObject _newIndicator;

	private StoryEventDTO? _data;

	private Action<StoryEventDTO> _onClick;

	private void Awake()
	{
		_button.onClick.AddListener(OnStoryEventClick);
	}

	private void OnDisable()
	{
		_data = null;
		_onClick = null;
	}

	public void Init(StoryEventDTO dto, Action<StoryEventDTO> onEventClick)
	{
		_weekText.SetParam("WEEK", dto.WeekTriggered);
		_eventName.text = dto.Name;
		_onClick = onEventClick;
		_newIndicator.SetActive(dto.IsNew);
		_data = dto;
	}

	private void OnStoryEventClick()
	{
		if (_data.HasValue)
		{
			_onClick?.Invoke(_data.Value);
		}
	}
}
