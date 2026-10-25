using FHGUI.DTO.Outpost;
using FHRL;

namespace FHGUI.DTO;

public class MainHeaderDTO
{
	public OutpostResources OutpostResources;

	public RangedValueDTO Prosperity;

	public int ProsperityLevel;

	public RangedValueDTO Morale;

	public int Inspiration;

	public int TotalDefense;

	public RangedValueDTO Soldiers;

	public CharacterDataDTO SelectedHero;

	public uint Week;

	public CalendarSeason Season;

	public CalendarDTO Calendar;

	public bool HasNewCodexEntry;
}
