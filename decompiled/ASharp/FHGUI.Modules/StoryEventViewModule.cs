using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements.Notifications;
using UnityEngine;

namespace FHGUI.Modules;

public class StoryEventViewModule : FHMapViewModule
{
	[SerializeField]
	private UIStoryEventList _listContainer;

	[SerializeField]
	private FHButton _closeButton;

	private Action _onClose;

	private void Awake()
	{
		_closeButton.onClick.AddListener(Close);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_onClose = null;
	}

	public void Init(IList<StoryEventDTO> events, Action<StoryEventDTO> onClick, Action onClose)
	{
		gameObject.SetActive(value: true);
		_onClose = onClose;
		_listContainer.Init(onClick, events);
	}

	public void Close()
	{
		_onClose?.Invoke();
		gameObject.SetActive(value: false);
	}
}
