using FHRL;
using UnityEngine;

namespace FHGUI.DTO.Outpost;

public static class CalendarUtils
{
	public static uint WeeksInYear => 20u;

	public static uint GetYear(uint week)
	{
		return (uint)Mathf.CeilToInt((float)week / (float)WeeksInYear);
	}

	public static uint ToAbsoluteWeek(uint year, uint weekIndexInYear)
	{
		return (year - 1) * WeeksInYear + weekIndexInYear + 1;
	}

	public static uint GetWeekIndex(uint absoluteWeek)
	{
		return (absoluteWeek - 1) % WeeksInYear;
	}

	public static void ConvertFromAbsoluteWeek(uint absoluteWeek, out uint year, out uint weekNum)
	{
		year = GetYear(absoluteWeek);
		weekNum = GetWeekIndex(absoluteWeek);
	}

	public static CalendarSeason GetSeason(int week)
	{
		if ((week - 1) / 10 % 2 != 0)
		{
			return CalendarSeason.Winter;
		}
		return CalendarSeason.Summer;
	}

	public static CalendarSeason GetSeason(uint week)
	{
		return GetSeason((int)week);
	}
}
