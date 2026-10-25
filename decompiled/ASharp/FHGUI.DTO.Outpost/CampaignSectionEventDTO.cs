using System.Linq;
using FHRL;
using GLOOM;
using UnityEngine;

namespace FHGUI.DTO.Outpost;

public struct CampaignSectionEventDTO
{
	public CampaignSectionID ID;

	public string Name;

	public string Desc;

	public Sprite Icon;

	public CampaignSectionEventDTO(CampaignSectionDef def, bool passed)
	{
		ID = def.ID;
		Name = ((def.ShowAsUnknownInCalendar && !passed) ? LocalizationManager.GetTranslation("GUI_UNKNOWN_EVENT") : LocalizationManager.GetTranslation($"Event_{def.ID}_Name"));
		Desc = "";
		if (!def.ShowAsUnknownInCalendar && def.Effects.Any((CampaignEffect x) => x.Type == CampaignEffectType.EnableTravellingMerchantThisWeek))
		{
			Icon = UIInfoTools.Instance.TravellingMerchantCalendarEventSprite;
		}
		else
		{
			Icon = (def.ShowAsUnknownInCalendar ? UIInfoTools.Instance.UnknownCalendarEventSprite : UIInfoTools.Instance.CalendarEventSprite);
		}
	}
}
