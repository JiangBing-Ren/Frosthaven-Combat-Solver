using System;
using FHGUI.DTO;

namespace FHGUI.Elements.Notifications;

public class UIStoryEventList : ListContainerBase<UIStoryEventRow, Action<StoryEventDTO>, StoryEventDTO>
{
	protected override void InitElement(UIStoryEventRow listElement, StoryEventDTO elementParams, Action<StoryEventDTO> listParams)
	{
		listElement.Init(elementParams, listParams);
	}
}
