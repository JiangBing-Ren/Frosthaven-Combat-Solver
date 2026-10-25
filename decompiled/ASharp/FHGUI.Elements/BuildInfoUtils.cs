using System.Linq;
using FHGUI.DTO;
using FHGUI.DTO.Outpost;
using FHRL;
using GLOOM;

namespace FHGUI.Elements;

public static class BuildInfoUtils
{
	public static string GetDescription(BuildingStateDTO building)
	{
		BuildingConfigUI buildingConfig = UIInfoTools.Instance.GetBuildingConfig(building.Def);
		if (!building.IsConstructed)
		{
			return ValidateText(buildingConfig.FlavorTextKey);
		}
		if (building.IsWrecked || building.IsRebuilding)
		{
			return ValidateText(buildingConfig.GetWreckedEffect((!building.IsConstructed) ? 1u : building.Level));
		}
		return ValidateText(buildingConfig.GetEffect(building.Level));
	}

	public static string GetUpgradeEffect(BuildingTypeDef def, uint level)
	{
		return ValidateText(UIInfoTools.Instance.GetBuildingConfig(def).GetUpgradeEffect(level));
	}

	public static string GetConstructEffect(BuildingTypeDef def)
	{
		return ValidateText(UIInfoTools.Instance.GetBuildingConfig(def).GetConstructEffect());
	}

	public static string ValidateText(string locKey)
	{
		if (!locKey.IsNullOrEmpty())
		{
			return locKey;
		}
		return "UNDEFINED <color=\"red\"" + locKey + ">";
	}

	public static void ColorizeDescription(TextLocalizedListener textComponent, BuildingStateDTO building)
	{
		if (building.IsWrecked || building.IsRebuilding)
		{
			string translation = LocalizationManager.GetTranslation(GetDescription(building));
			translation = $"<style=\"Wrecked\">{translation}</style>";
			textComponent.Text.text = translation;
		}
		else
		{
			textComponent.ClearText();
			textComponent.SetTextKey(GetDescription(building));
		}
	}

	public static void InitCost(BuildingStateDTO.Cost? cost, OutpostResources? donatedResources, OutpostResources partyResources, uint? donatedGold, uint partyGold, OutpostStateDTO state, UIBuildingResourceCostContainer costContainer)
	{
		BuildingTypeDef.Cost? cost2 = cost?.Spendable;
		OutpostResources resources = cost2?.Resources ?? default(OutpostResources);
		OutpostResources valueOrDefault = (cost?.RawResourcesCost).GetValueOrDefault();
		uint townLevelRequirement = cost?.RequiredProsperityLevel ?? 0;
		costContainer.Init(new BuildingCostDTO
		{
			Resources = resources,
			RawResources = valueOrDefault,
			Gold = (cost2?.Gold ?? 0),
			Morale = (cost?.TotalMoraleCost ?? 0),
			Inspiration = 0u,
			TownLevel = cost?.RequiredProsperityLevel,
			CurrentResources = donatedResources.GetValueOrDefault(),
			CurrentGold = donatedGold.GetValueOrDefault(),
			CurrentMorale = state.Morale,
			CurrentInspiration = state.Inspiration,
			CurrentTownLevel = state.TownLevel
		}, townLevelRequirement, state.TownLevel, partyResources, partyGold);
	}

	public static int GetProsperityReward(BuildingStateDTO building)
	{
		if (building.Level == 0)
		{
			return building.ConstructEffects?.FirstOrDefault((CampaignEffect e) => e.Type == CampaignEffectType.AddProsperity).GetInt(0) ?? 0;
		}
		return building.UpgradeConstructEffects?.FirstOrDefault((CampaignEffect e) => e.Type == CampaignEffectType.AddProsperity).GetInt(0) ?? 0;
	}
}
