using System;
using System.Collections.Generic;
using FHGUI.DTO.Outpost;
using FHGUI.Elements.Notifications;
using UnityEngine;

namespace FHGUI.Modules;

public class OutpostEventLogModule : FHMapViewModule
{
	[SerializeField]
	private UIOutpostEventLogContainer _eventLogContainer;

	private Action _onClose;

	public void Close()
	{
		_onClose?.Invoke();
		gameObject.SetActive(value: false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_onClose = null;
	}

	public void Init(IList<EventLogDTO> eventLogTexts, UIOutpostEventLogContainer.EventLogListParams @params, Action onClose)
	{
		_eventLogContainer.Init(@params, eventLogTexts);
		_onClose = onClose;
	}
}
