using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary;

public class CAbilityCompare
{
	public CAbility.EAbilityType? AbilityType { get; private set; }

	public int? Strength { get; private set; }

	public int? Range { get; private set; }

	public bool? IsRanged { get; private set; }

	public bool? IsMelee { get; private set; }

	public int? NumberOfTargets { get; private set; }

	public bool? HasAreaEffect { get; private set; }

	public List<CCondition.EPositiveCondition> HasPositiveConditions { get; private set; }

	public List<CCondition.ENegativeCondition> HasNegativeConditions { get; private set; }

	public CAbility.EAbilityTargeting? Targeting { get; private set; }

	public List<ElementInfusionBoardManager.EElement> InfusesElements { get; private set; }

	public bool? IsTargetedAbility { get; private set; }

	public bool? HasActiveBonus { get; private set; }

	public bool? Jump { get; private set; }

	public bool? Fly { get; private set; }

	public bool? IgnoreDifficultTerrain { get; private set; }

	public bool? IgnoreHazardousTerrain { get; private set; }

	public bool? MultiPassAttack { get; private set; }

	public bool? ExploitVulnerability { get; private set; }

	public int? Pierce { get; private set; }

	public CAbilityFilter AbilityFilter { get; private set; }

	public bool? Adjacent { get; private set; }

	public bool? HasCardWithHigherThanCurrentInitiative { get; private set; }

	public int? TargetShield { get; private set; }

	public int? AbilityActorHealth { get; private set; }

	public CAbilityCompare(CAbility.EAbilityType? abilityType = null, int? strength = null, int? range = null, bool? isRanged = null, bool? isMelee = null, int? numberOfTargets = null, bool? hasAreaEffect = null, List<CCondition.EPositiveCondition> hasPositiveConditions = null, List<CCondition.ENegativeCondition> hasNegativeConditions = null, CAbility.EAbilityTargeting? targeting = null, List<ElementInfusionBoardManager.EElement> infusesElements = null, bool? isTargetedAbility = null, bool? hasActiveBonus = null, bool? jump = null, bool? fly = null, bool? multipassAttack = null, int? pierce = null, CAbilityFilter abilityFilter = null, bool? hasCardWithHigherThanCurrentInitiative = null, bool? exploitVulnerability = null, bool? adjacent = null, int? targetShield = null, int? abilityActorHealth = null)
	{
		AbilityType = abilityType;
		Strength = strength;
		Range = range;
		IsRanged = isRanged;
		IsMelee = isMelee;
		NumberOfTargets = numberOfTargets;
		HasAreaEffect = hasAreaEffect;
		HasPositiveConditions = hasPositiveConditions;
		HasNegativeConditions = hasNegativeConditions;
		Targeting = targeting;
		HasActiveBonus = hasActiveBonus;
		InfusesElements = infusesElements;
		IsTargetedAbility = isTargetedAbility;
		Jump = jump;
		Fly = fly;
		MultiPassAttack = multipassAttack;
		ExploitVulnerability = exploitVulnerability;
		Pierce = pierce;
		AbilityFilter = abilityFilter;
		HasCardWithHigherThanCurrentInitiative = hasCardWithHigherThanCurrentInitiative;
		Adjacent = adjacent;
		TargetShield = targetShield;
		AbilityActorHealth = abilityActorHealth;
	}

	public bool CompareAbility(CAbility compareAbility, CActor targetActor = null, bool checkActorsToTarget = false)
	{
		if (compareAbility is CAbilityMerged cAbilityMerged)
		{
			compareAbility = cAbilityMerged.ActiveAbility;
		}
		if (compareAbility != null && compareAbility.NoBonuses)
		{
			return false;
		}
		AbilityData.MiscAbilityData miscAbilityData = compareAbility.MiscAbilityData;
		bool flag;
		int range;
		if (miscAbilityData != null && miscAbilityData.UseParentRangeType.HasValue)
		{
			AbilityData.MiscAbilityData miscAbilityData2 = compareAbility.MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.UseParentRangeType.Value)
			{
				CAbility cAbility = ((compareAbility.ParentAbility is CAbilityMerged cAbilityMerged2) ? cAbilityMerged2.GetMergedWithAbility(compareAbility) : compareAbility.ParentAbility);
				AbilityData.MiscAbilityData miscAbilityData3 = cAbility.MiscAbilityData;
				flag = miscAbilityData3 != null && miscAbilityData3.TreatAsMelee.HasValue && (cAbility.MiscAbilityData?.TreatAsMelee.Value ?? false);
				range = cAbility.Range;
				goto IL_0141;
			}
		}
		AbilityData.MiscAbilityData miscAbilityData4 = compareAbility.MiscAbilityData;
		flag = miscAbilityData4 != null && miscAbilityData4.TreatAsMelee.HasValue && (compareAbility.MiscAbilityData?.TreatAsMelee.Value ?? false);
		range = compareAbility.Range;
		goto IL_0141;
		IL_0141:
		if (Adjacent.HasValue && targetActor != null && compareAbility?.TargetingActor != null && Adjacent.Value != compareAbility.TargetingActor.IsAdjacentTo(targetActor))
		{
			return false;
		}
		if (AbilityActorHealth.HasValue && compareAbility?.TargetingActor != null && compareAbility.TargetingActor.Health < AbilityActorHealth.Value)
		{
			return false;
		}
		if (TargetShield.HasValue && targetActor != null && ((GameState.TotalShieldDuringLastAttack > 0) ? GameState.TotalShieldDuringLastAttack : targetActor.CalculateShield(compareAbility)) < TargetShield.Value)
		{
			return false;
		}
		if (AbilityType.HasValue && AbilityType.Value != compareAbility.AbilityType)
		{
			return false;
		}
		if (Strength.HasValue && Strength.Value != compareAbility.Strength)
		{
			return false;
		}
		if (Range.HasValue && Range.Value != compareAbility.Range)
		{
			return false;
		}
		if (IsRanged.HasValue && IsRanged.Value != (range > 1 && !flag))
		{
			return false;
		}
		if (IsMelee.HasValue && IsMelee.Value != (range <= 1 || flag))
		{
			return false;
		}
		if (NumberOfTargets.HasValue || HasAreaEffect.HasValue)
		{
			int num = ((compareAbility.OriginalAreaEffect?.Hexes != null) ? compareAbility.OriginalAreaEffect.Hexes.Count((CAreaEffect.CAreaEffectHex x) => x.Flags.HasFlag(CAreaEffect.EHexFlag.Enemy)) : 0);
			if (NumberOfTargets.HasValue && (NumberOfTargets.Value != compareAbility.OriginalTargetCount || compareAbility.AllTargetsOnMovePath || compareAbility.AllTargetsOnAttackPath || compareAbility.AllTargetsOnMovePathSameStartAndEnd || (num > 0 && NumberOfTargets.Value != num)))
			{
				return false;
			}
			if (HasAreaEffect.HasValue)
			{
				if (HasAreaEffect.Value && compareAbility.OriginalAreaEffect == null && (!NumberOfTargets.HasValue || NumberOfTargets.Value != num))
				{
					return false;
				}
				if (!HasAreaEffect.Value && compareAbility.OriginalAreaEffect != null)
				{
					return false;
				}
			}
		}
		if (HasPositiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition hasPositiveCondition in HasPositiveConditions)
			{
				if (!compareAbility.PositiveConditions.Keys.Contains(hasPositiveCondition))
				{
					return false;
				}
			}
		}
		if (HasNegativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition hasNegativeCondition in HasNegativeConditions)
			{
				if (!compareAbility.NegativeConditions.Keys.Contains(hasNegativeCondition))
				{
					return false;
				}
			}
		}
		if (Targeting.HasValue && Targeting.Value != compareAbility.Targeting)
		{
			return false;
		}
		if (InfusesElements != null)
		{
			if (!(compareAbility is CAbilityInfuse))
			{
				return false;
			}
			foreach (ElementInfusionBoardManager.EElement infusesElement in InfusesElements)
			{
				if (!(compareAbility as CAbilityInfuse).ElementsToInfuse.Contains(infusesElement))
				{
					return false;
				}
			}
		}
		if (IsTargetedAbility.HasValue && IsTargetedAbility.Value != compareAbility.IsTargetedAbility)
		{
			return false;
		}
		bool flag2 = compareAbility.ActiveBonusData != null && compareAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA;
		if (flag2 && !HasActiveBonus.HasValue)
		{
			return false;
		}
		if (HasActiveBonus.HasValue && HasActiveBonus.Value != flag2)
		{
			return false;
		}
		if (Jump.HasValue)
		{
			if (!(compareAbility is CAbilityMove))
			{
				return false;
			}
			if (Jump.Value != (compareAbility as CAbilityMove).Jump)
			{
				return false;
			}
		}
		if (Fly.HasValue)
		{
			if (!(compareAbility is CAbilityMove))
			{
				return false;
			}
			if (Fly.Value != (compareAbility as CAbilityMove).Fly)
			{
				return false;
			}
		}
		if (IgnoreDifficultTerrain.HasValue)
		{
			if (!(compareAbility is CAbilityMove))
			{
				return false;
			}
			if (IgnoreDifficultTerrain.Value != (compareAbility as CAbilityMove).IgnoreDifficultTerrain)
			{
				return false;
			}
		}
		if (IgnoreHazardousTerrain.HasValue)
		{
			if (!(compareAbility is CAbilityMove))
			{
				return false;
			}
			if (IgnoreHazardousTerrain.Value != (compareAbility as CAbilityMove).IgnoreHazardousTerrain)
			{
				return false;
			}
		}
		if (MultiPassAttack.HasValue)
		{
			if (!(compareAbility is CAbilityAttack))
			{
				return false;
			}
			if (MultiPassAttack.Value != (compareAbility as CAbilityAttack).MultiPassAttack)
			{
				return false;
			}
		}
		if (ExploitVulnerability.HasValue)
		{
			if (!(compareAbility is CAbilityAttack))
			{
				return false;
			}
			if (ExploitVulnerability.Value != (compareAbility as CAbilityAttack).ExploitVulnerability)
			{
				return false;
			}
		}
		if (Pierce.HasValue)
		{
			if (!(compareAbility is CAbilityAttack))
			{
				return false;
			}
			if (Pierce.Value != (compareAbility as CAbilityAttack).Pierce)
			{
				return false;
			}
		}
		if (AbilityFilter != null)
		{
			if (!compareAbility.AbilityFilter.HasTargetTypeFlag(AbilityFilter.FilterTargetType))
			{
				return false;
			}
			if (targetActor != null && !AbilityFilter.IsValidTarget(targetActor, compareAbility.TargetingActor, IsTargetedAbility == true, useTargetOriginalType: false, false))
			{
				return false;
			}
			if (checkActorsToTarget && (compareAbility.ActorsToTarget == null || compareAbility.ActorsToTarget.Count == 0 || !compareAbility.ActorsToTarget.All((CActor x) => AbilityFilter.IsValidTarget(x, compareAbility.TargetingActor, IsTargetedAbility == true, useTargetOriginalType: false, false))))
			{
				return false;
			}
		}
		return true;
	}

	public bool CompareActor(CActor compareActor)
	{
		if (HasCardWithHigherThanCurrentInitiative.HasValue && compareActor is CPlayerActor cPlayerActor)
		{
			int num = cPlayerActor.Initiative();
			bool flag = false;
			foreach (CAbilityCard handAbilityCard in cPlayerActor.CharacterClass.HandAbilityCards)
			{
				if (handAbilityCard.Initiative > num)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public static bool GetAbilityCompare(MappingEntry compareParentEntry, string filename, out CAbilityCompare compareAbility)
	{
		bool flag = true;
		compareAbility = null;
		CAbility.EAbilityType? abilityType = null;
		int? strength = null;
		int? range = null;
		bool? isRanged = null;
		bool? isMelee = null;
		int? numberOfTargets = null;
		bool? hasAreaEffect = null;
		List<CCondition.EPositiveCondition> list = null;
		List<CCondition.ENegativeCondition> list2 = null;
		CAbility.EAbilityTargeting? targeting = null;
		List<ElementInfusionBoardManager.EElement> list3 = null;
		bool? isTargetedAbility = null;
		bool? hasActiveBonus = null;
		bool? jump = null;
		bool? fly = null;
		bool? multipassAttack = null;
		int? pierce = null;
		CAbilityFilter abilityFilter = null;
		bool? hasCardWithHigherThanCurrentInitiative = null;
		bool? adjacent = null;
		int? targetShield = null;
		int? abilityActorHealth = null;
		if (YMLShared.GetMapping(compareParentEntry, filename, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "AbilityType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "AbilityType", filename, out var abilityTypeString))
					{
						CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == abilityTypeString);
						if (eAbilityType != CAbility.EAbilityType.None)
						{
							abilityType = eAbilityType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Ability Type " + abilityTypeString + " supplied to compare ability in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Strength":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Strength", filename, out var value4))
					{
						strength = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Range":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Range", filename, out var value10))
					{
						range = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IsRanged":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsRanged", filename, out var value16))
					{
						isRanged = value16;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IsMelee":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsMelee", filename, out var value7))
					{
						isMelee = value7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Target":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Target", filename, out var value17))
					{
						numberOfTargets = value17;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "HasAreaEffect":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "HasAreaEffect", filename, out var value14))
					{
						hasAreaEffect = value14;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "HasPositiveConditions":
				{
					if (YMLShared.GetStringList(entry.Value, "HasPositiveConditions", filename, out var values))
					{
						list = new List<CCondition.EPositiveCondition>();
						foreach (string posConString in values)
						{
							CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition s) => s.ToString() == posConString);
							if (ePositiveCondition != CCondition.EPositiveCondition.NA)
							{
								list.Add(ePositiveCondition);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid positive condition " + posConString + " specified in Compare Ability entry in file " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "HasNegativeConditions":
				{
					if (YMLShared.GetStringList(entry.Value, "HasNegativeConditions", filename, out var values2))
					{
						list2 = new List<CCondition.ENegativeCondition>();
						foreach (string negConString in values2)
						{
							CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition s) => s.ToString() == negConString);
							if (eNegativeCondition != CCondition.ENegativeCondition.NA)
							{
								list2.Add(eNegativeCondition);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid positive condition " + negConString + " specified in Compare Ability entry in file " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Targeting":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Targeting", filename, out var targetingString))
					{
						CAbility.EAbilityTargeting eAbilityTargeting = CAbility.AbilityTargetingTypes.SingleOrDefault((CAbility.EAbilityTargeting s) => s.ToString() == targetingString);
						if (eAbilityTargeting != CAbility.EAbilityTargeting.None)
						{
							targeting = eAbilityTargeting;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Targeting value " + targetingString + " specified in Compare Ability entry in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "InfusesElements":
				{
					if (YMLShared.GetStringList(entry.Value, "InfusesElements", filename, out var values3))
					{
						list3 = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString in values3)
						{
							try
							{
								ElementInfusionBoardManager.EElement item = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement s) => s.ToString() == elementString);
								list3.Add(item);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element " + elementString + " specified in Compare Ability entry in file " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IsTargetedAbility":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsTargetedAbility", filename, out var value15))
					{
						isTargetedAbility = value15;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "HasActiveBonus":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "HasActiveBonus", filename, out var value11))
					{
						hasActiveBonus = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Jump":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Jump", filename, out var value9))
					{
						jump = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Fly":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Fly", filename, out var value5))
					{
						fly = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MultiPassAttack":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "MultiPassAttack", filename, out var value2))
					{
						multipassAttack = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Pierce":
				{
					string value13;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", filename, out var value12, suppressErrors: true))
					{
						pierce = value12;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Pierce", filename, out value13))
					{
						if (value13 == "All")
						{
							pierce = 99999;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value " + value13 + " specified for Pierce entry in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Filter":
				{
					if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter))
					{
						abilityFilter = filter;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "HasCardWithHigherThanCurrentInitiative":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "HasCardWithHigherThanCurrentInitiative", filename, out var value8))
					{
						hasCardWithHigherThanCurrentInitiative = value8;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Adjacent":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Adjacent", filename, out var value6))
					{
						adjacent = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TargetShield":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "TargetShield", filename, out var value3))
					{
						targetShield = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityActorHealth":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "AbilityActorHealth", filename, out var value))
					{
						abilityActorHealth = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					Log.ErrorWithReport("Invalid Compare entry " + entry.Key.ToString() + " in file " + filename);
					flag = false;
					break;
				}
			}
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			compareAbility = new CAbilityCompare(abilityType, strength, range, isRanged, isMelee, numberOfTargets, hasAreaEffect, list, list2, targeting, list3, isTargetedAbility, hasActiveBonus, jump, fly, multipassAttack, pierce, abilityFilter, hasCardWithHigherThanCurrentInitiative, null, adjacent, targetShield, abilityActorHealth);
		}
		return flag;
	}
}
