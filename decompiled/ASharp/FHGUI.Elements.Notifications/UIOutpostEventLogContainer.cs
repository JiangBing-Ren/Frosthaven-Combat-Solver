using System.Linq;
using FHGUI.DTO.Outpost;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Notifications;

public class UIOutpostEventLogContainer : ListContainerBase<UIOutpostEventLogRow, UIOutpostEventLogContainer.EventLogListParams, EventLogDTO>
{
	public class EventLogListParams
	{
		public uint CurrentWeek;
	}

	[SerializeField]
	private ScrollRect _scroll;

	protected override void InitElement(UIOutpostEventLogRow row, EventLogDTO data, EventLogListParams listParams)
	{
		row.Init(data);
	}

	protected override void AfterInit_Impl(EventLogListParams listParams)
	{
		base.AfterInit_Impl(listParams);
		UIOutpostEventLogRow uIOutpostEventLogRow = Elements.FirstOrDefault((UIOutpostEventLogRow e) => e.Data.IsNewWeek && e.Data.Week == listParams.CurrentWeek);
		if (uIOutpostEventLogRow != null)
		{
			_scroll.ScrollVerticallyTo(uIOutpostEventLogRow.transform as RectTransform);
		}
	}
}
