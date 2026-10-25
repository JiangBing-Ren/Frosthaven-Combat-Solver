using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements.Notifications;

public class UIStoryEventsNotification : MonoBehaviour
{
	public record StoryEventParams
	{
		public Action OpenStoryEvents;

		public IList<StoryEventDTO> Events = new List<StoryEventDTO>();

		public bool HasNew;
	}

	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private RectTransform _stacksContainer;

	[SerializeField]
	private TMP_Text _stacks;

	[SerializeField]
	private GameObject _newEventsIndicator;

	private StoryEventParams _params;

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
	}

	public void Init(StoryEventParams @params)
	{
		gameObject.SetActive(@params.Events.Any());
		int num = @params.Events.Count((StoryEventDTO e) => e.IsNew);
		_stacks.text = num.ToString();
		_stacksContainer.gameObject.SetActive(num > 0);
		_newEventsIndicator.SetActive(@params.HasNew);
		_params = @params;
	}

	private void OnClick()
	{
		_params?.OpenStoryEvents?.Invoke();
	}
}
