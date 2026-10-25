using System.Collections.Generic;
using FHGUI.Services;
using FHRL;

namespace FHGUI.DTO;

public class OutpostAttackEventDTO
{
	public BuildingStateDTO CurrentTargetBuilding;

	public int TotalTargetCount;

	public int CurrentTargetIndex;

	public OutpostAttackState.AttackState CurrentState;

	public bool CanUseSoldiers;

	public uint LastUsedSoldiers;

	public bool IsSuccessfulDefend;

	public uint ThreatValue;

	public int BaseDefenceValue;

	public int MoralDefenceValue;

	public int BonusDefenceValue;

	public int PerSolderDefenseValueModifier;

	public int DefenceFromSolder;

	public int TotalDefenceValue;

	public int TotalAttackValue;

	public int AvailableMorale;

	public List<AttackModifierDTO> DrawnCards;

	public List<AttackModifierDTO> ToChooseCards;

	public TownGuardDrawCardActionType ActionType;

	public int LoseMoraleInsteadOfDamagingBuildings;

	public int LoseSoldiersInsteadOfDamagingBuildings;

	public uint InitialSolderCount;

	public uint MaxSolderCount;

	public OutpostResources ResourcesSpentOnRepair;

	public uint MoraleSpentOnRepair;

	public uint InspirationSpentOnRepair;

	public int TotalDefenseValue => BaseDefenceValue + DefenceFromSolder + BonusDefenceValue + MoralDefenceValue;

	public bool WasRepaired
	{
		get
		{
			if (CurrentState >= OutpostAttackState.AttackState.AttackFinished)
			{
				if (MoraleSpentOnRepair == 0 && ResourcesSpentOnRepair.Total <= 0)
				{
					return InspirationSpentOnRepair != 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool WaitingForModifierChoice
	{
		get
		{
			if (CurrentState == OutpostAttackState.AttackState.AttackStarted && ToChooseCards != null)
			{
				return ToChooseCards.Count > 0;
			}
			return false;
		}
	}

	public string GetRepairCostLocalized()
	{
		string text = string.Empty;
		if (MoraleSpentOnRepair != 0)
		{
			text += string.Format("{0}{1} ", MoraleSpentOnRepair, FHUILocFormatting.Sprite("Morale"));
		}
		if (InspirationSpentOnRepair != 0)
		{
			text += string.Format("{0}{1} ", InspirationSpentOnRepair, FHUILocFormatting.Sprite("Inspiration"));
		}
		if (ResourcesSpentOnRepair.Total > 0)
		{
			text += CraftingViewService.LocalizeCost(ResourcesSpentOnRepair, ResourcesSpentOnRepair);
		}
		return text;
	}
}
