using System.Collections.Generic;
using System.Linq;
using FHGUI.Services;
using FHRL;

namespace FHGUI.DTO;

public class OutpostAttackEventSummeryEntryDTO
{
	public BuildingStateDTO Building;

	public List<AttackModifierDTO> DrawnCards;

	public bool InProgress;

	public bool Defended;

	public bool Wrecked;

	public uint Threat;

	public uint FinalThreat;

	public int Defense;

	public int FinalDefense;

	public uint SoldersSpent;

	public uint SoldersAvailable;

	public int Index;

	public bool Advantage;

	public bool Disadvantage;

	public OutpostResources ResourcesSpent;

	public uint MoraleSpent;

	public uint InspirationSpent;

	public bool IsRepaired
	{
		get
		{
			if (!Defended)
			{
				return !Wrecked;
			}
			return false;
		}
	}

	public int FinalAttackValue => (int)(FinalDefense - FinalThreat);

	public int InitialAttackValue => (int)(Defense - Threat);

	public bool HasSuccessDrawn => DrawnCards.Any((AttackModifierDTO dto) => dto.IsSuccess && dto.Usage == OutpostAttackState.DrawnCardUsageSummary.CardUsageState.Used);

	public bool IsEmpty()
	{
		return Building.Def == null;
	}

	public string GetRepairCostLocalized()
	{
		string text = string.Empty;
		if (MoraleSpent != 0)
		{
			text += string.Format("{0}{1}", MoraleSpent, FHUILocFormatting.Sprite("Morale"));
		}
		if (InspirationSpent != 0)
		{
			text += string.Format("{0}{1}", InspirationSpent, FHUILocFormatting.Sprite("Inspiration"));
		}
		if (ResourcesSpent.Total > 0)
		{
			text += CraftingViewService.LocalizeCost(ResourcesSpent, ResourcesSpent);
		}
		return text;
	}
}
