using System;
using FHGUI.Elements;
using FHRL;
using UnityEngine;

namespace FHGUI.DTO;

[Serializable]
public struct BuildingStateDTO : IBuildingInfoParams
{
	public struct Cost
	{
		public BuildingTypeDef.Cost? Spendable;

		public OutpostResources? RawResourcesCost;

		public uint RequiredProsperityLevel;

		public uint ExtraMoralePenalty;

		public uint TotalMoraleCost => ExtraMoralePenalty + (Spendable?.Morale ?? 0);
	}

	public BuildingTypeDef Def;

	public uint Level;

	public string NameLocKey;

	public string DescriptionLocKey;

	public Sprite Icon;

	public Sprite IconTintable;

	public BuildingConstructionType ConstructionState;

	public bool IsWrecked;

	public bool IsRepaired;

	public bool IsDamaged;

	public bool IsConstructed;

	public bool CanUpgrade;

	public bool CanBuild;

	public bool CanRebuild;

	public bool IsExtraConstructionProject;

	public int AvailableMorale;

	public uint AvailableProsperity;

	public bool HasStartedProject;

	public bool IsDonationComplete;

	public bool CanFinalizeDonation;

	public string FinalizeDonationTT;

	public string MissingBuildRequirement;

	public Cost? ConstructionCost;

	public Cost? RebuildCost;

	public Cost? UpgradeCost;

	public CampaignEffect[] ConstructEffects;

	public CampaignEffect[] ActiveEffects;

	public CampaignEffect[] ApplyEffects;

	public CampaignEffect[] UpgradeEffects;

	public CampaignEffect[] UpgradeConstructEffects;

	public BuildingTypeID TypeID => Def.ID;

	public BuildingId ID => (BuildingId)Def.ID.ID;

	public bool IsRebuilding => ConstructionState == BuildingConstructionType.Rebuilding;

	public bool IsUnderConstruction
	{
		get
		{
			if (!IsActive)
			{
				return !HasStartedProject;
			}
			return false;
		}
	}

	public bool IsActive => ConstructionState == BuildingConstructionType.None;

	public bool HasMoreUpgrades => Level < MaxLevels;

	public bool HasUpgrades
	{
		get
		{
			if (!ReleaseStage.IsDemo())
			{
				return MaxLevels > 1;
			}
			return CampaignSettings.MaxValidBuildingLevel(Def.ID, Def.Levels) > 1;
		}
	}

	public bool CanBuyUpgrades => !Def.CannotBuyUpgrades;

	public uint MaxLevels => Def.Levels;

	public bool HasBuildConstructionRequirements
	{
		get
		{
			if (ConstructionCost.HasValue && ConstructionCost.Value.RequiredProsperityLevel <= AvailableProsperity)
			{
				return ConstructionCost.Value.Spendable?.Morale <= AvailableMorale;
			}
			return false;
		}
	}

	public bool HasRebuildRequirements
	{
		get
		{
			if (RebuildCost.HasValue && RebuildCost.Value.RequiredProsperityLevel <= AvailableProsperity)
			{
				return RebuildCost.Value.Spendable?.Morale <= AvailableMorale;
			}
			return false;
		}
	}

	public bool HasUpgradeRequirements
	{
		get
		{
			if (UpgradeCost.HasValue && UpgradeCost.Value.RequiredProsperityLevel <= AvailableProsperity)
			{
				return UpgradeCost.Value.Spendable?.Morale <= AvailableMorale;
			}
			return false;
		}
	}

	public override string ToString()
	{
		return string.Format("[{0}, {1}]", GetType(), (Def == null) ? "Emprty" : Def.Name);
	}
}
