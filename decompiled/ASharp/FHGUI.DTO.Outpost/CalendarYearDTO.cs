using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FHGUI.DTO.Outpost;

public record CalendarYearDTO : IEnumerable<CalendarWeekDTO>, IEnumerable
{
	private readonly CalendarWeekDTO[] _weeks = new CalendarWeekDTO[CalendarUtils.WeeksInYear];

	public CalendarWeekDTO this[int index]
	{
		get
		{
			if (index <= CalendarUtils.WeeksInYear)
			{
				return _weeks[index];
			}
			return default;
		}
	}

	public CalendarWeekDTO this[uint index] => this[(int)index];

	public uint Year { get; }

	public static CalendarYearDTO Empty(uint year)
	{
		return new CalendarYearDTO(year, from w in Enumerable.Range(0, (int)CalendarUtils.WeeksInYear)
			select new CalendarWeekDTO(CalendarUtils.ToAbsoluteWeek(year, (uint)w)));
	}

	public CalendarYearDTO(uint yearNum, CalendarWeekDTO[] weeks)
	{
		Year = yearNum;
		_weeks = weeks;
	}

	public CalendarYearDTO(uint yearNum, IEnumerable<CalendarWeekDTO> weeks)
	{
		Year = yearNum;
		_weeks = weeks.ToArray();
	}

	public IEnumerator<CalendarWeekDTO> GetEnumerator()
	{
		return (IEnumerator<CalendarWeekDTO>)_weeks.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _weeks.GetEnumerator();
	}

	[CompilerGenerated]
	protected virtual bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Year = ");
		builder.Append(Year.ToString());
		return true;
	}
}
