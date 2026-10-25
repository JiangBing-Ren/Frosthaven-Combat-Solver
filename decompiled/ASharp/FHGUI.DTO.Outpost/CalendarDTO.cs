using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO.Outpost;

public record CalendarDTO
{
	public readonly uint CurrentWeek;

	public readonly CalendarSeason Season;

	private Dictionary<uint, CalendarYearDTO> _years = new Dictionary<uint, CalendarYearDTO>();

	public IReadOnlyDictionary<uint, CalendarYearDTO> Years => _years;

	public uint CurrentYear => CalendarUtils.GetYear(CurrentWeek);

	public uint WeekIndexInYear => CalendarUtils.GetWeekIndex(CurrentWeek);

	public CalendarYearDTO GetYearData(uint year)
	{
		if (!_years.TryGetValue(year, out var value))
		{
			return CalendarYearDTO.Empty(year);
		}
		return value;
	}

	public CalendarDTO(uint week, CalendarSeason season, Dictionary<uint, CalendarYearDTO> years)
	{
		CurrentWeek = week;
		Season = season;
		_years = years;
	}
}
