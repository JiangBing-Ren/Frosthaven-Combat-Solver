using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO.Outpost;

public struct CalendarWeekDTO(uint weekNum, IEnumerable<CampaignSectionEventDTO> events = null)
{
	public List<CampaignSectionEventDTO> CampaignEvents = ((events != null) ? new List<CampaignSectionEventDTO>(events) : new List<CampaignSectionEventDTO>());

	public CalendarSeason Season = CalendarUtils.GetSeason(weekNum);

	public uint WeekNum = weekNum;
}
