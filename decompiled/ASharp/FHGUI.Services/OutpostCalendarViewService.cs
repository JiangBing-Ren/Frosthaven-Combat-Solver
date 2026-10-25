using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using FHRL;
using Libs.Utils;

namespace FHGUI.Services;

public class OutpostCalendarViewService(FHMapView view) : FHMapViewService(view)
{
	private readonly Dictionary<uint, CalendarYearDTO> _calendarCache = new Dictionary<uint, CalendarYearDTO>();

	private IEnumerable<CalendarWeekDTO> GetWeeksDataInYear(uint year)
	{
		for (uint i = 0u; i < CalendarUtils.WeeksInYear; i++)
		{
			yield return GetWeekData(year, i);
		}
	}

	private CalendarWeekDTO GetWeekData(uint absoluteWeek)
	{
		IEnumerable<CampaignSectionEventDTO> events = from e in GameController.State.Outpost.CalendarEvents
			where e.SectionID.Valid && e.TriggerAtWeek == absoluteWeek
			select new CampaignSectionEventDTO(DefRepo.CampaignSections[e.SectionID], e.TriggerAtWeek <= _view.GameState.Outpost.Week);
		return new CalendarWeekDTO(absoluteWeek, events);
	}

	private CalendarWeekDTO GetWeekData(uint year, uint weekIndex)
	{
		return GetWeekData(CalendarUtils.ToAbsoluteWeek(year, weekIndex));
	}

	private CalendarYearDTO GetYearData(uint year)
	{
		return new CalendarYearDTO(year, GetWeeksDataInYear(year));
	}

	public CalendarDTO GetCalendarData()
	{
		Calendar calendar = GameController.State.Outpost.Calendar;
		uint currentYear = calendar.Year;
		CalendarYearDTO value = new CalendarYearDTO(currentYear, GetWeeksDataInYear(currentYear));
		_calendarCache.Remove(currentYear);
		_calendarCache.Add(currentYear, value);
		for (uint num = 1u; num < currentYear; num++)
		{
			if (!_calendarCache.ContainsKey(num))
			{
				_calendarCache.Add(num, GetYearData(num));
			}
		}
		_calendarCache.RemoveWhere((KeyValuePair<uint, CalendarYearDTO> pair) => pair.Key > currentYear);
		List<CalendarEventState> calendarEvents = GameController.State.Outpost.CalendarEvents;
		if (calendarEvents.Any())
		{
			uint year = CalendarUtils.GetYear(calendarEvents.Max((CalendarEventState e) => e.TriggerAtWeek));
			for (uint num2 = currentYear + 1; num2 <= year; num2++)
			{
				_calendarCache.Add(num2, GetYearData(num2));
			}
		}
		return new CalendarDTO(calendar.Week, calendar.Season, _calendarCache);
	}
}
