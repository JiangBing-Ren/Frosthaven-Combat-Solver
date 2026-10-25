using FHGUI.DTO.Outpost;

namespace FHGUI.Elements;

public class CalendarEventsDescListContainer : ListContainerBase<CalendarEventDescElement, object, CampaignSectionEventDTO>
{
	protected override void InitElement(CalendarEventDescElement listElement, CampaignSectionEventDTO section, object listParams)
	{
		listElement.Init(section);
	}
}
