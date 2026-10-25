using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AStar;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class CardProcessingShared
{
	public const int IDMaxLength = 64;

	public const string TREASURE_TABLE_SHORTCUT_CHAR = "£";

	public static string GetLookupValue(string lookup)
	{
		if (lookup.Contains('$'))
		{
			return CardLayoutRow.GetKey(lookup, '$');
		}
		return lookup;
	}

	public static bool GetSingleAbilityFilter(MappingEntry keyMappingEntry, string filename, out CAbilityFilterContainer filter)
	{
		List<CAbilityFilter> list = new List<CAbilityFilter>();
		bool result = true;
		try
		{
			if (GetAbilityFilter(keyMappingEntry, filename, out var filter2))
			{
				list.Add(filter2);
			}
			else
			{
				result = false;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing an Ability Filter Container.\nFile: " + filename, ex);
			result = false;
		}
		filter = new CAbilityFilterContainer(list);
		return result;
	}

	public static bool GetAbilityFilterContainer(MappingEntry keyMappingEntry, string filename, out CAbilityFilterContainer filter)
	{
		List<CAbilityFilter> list = new List<CAbilityFilter>();
		bool andLogic = false;
		bool result = true;
		try
		{
			if (YMLShared.GetMapping(keyMappingEntry, filename, out var mapping, suppressErrors: true))
			{
				foreach (MappingEntry entry in mapping.Entries)
				{
					CAbilityFilter filter2;
					if (entry.Key.ToString() == "AndLogic")
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "AndLogic", filename, out var value))
						{
							andLogic = value;
						}
					}
					else if (GetAbilityFilter(entry, filename, out filter2))
					{
						list.Add(filter2);
					}
					else
					{
						result = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing an Ability Filter Container.\nFile: " + filename, ex);
			result = false;
		}
		filter = new CAbilityFilterContainer(list)
		{
			AndLogic = andLogic
		};
		return result;
	}

	public static bool GetAbilityFilter(MappingEntry keyMappingEntry, string filename, out CAbilityFilter filter)
	{
		filter = new CAbilityFilter();
		bool result = true;
		try
		{
			Mapping mapping;
			if (keyMappingEntry.Value is Scalar || keyMappingEntry.Value is Sequence)
			{
				if (YMLShared.GetStringList(keyMappingEntry.Value, "Filter/TargetType", filename, out var values))
				{
					foreach (string ftype in values)
					{
						CAbilityFilter.EFilterTargetType eFilterTargetType = CAbilityFilter.FilterTargetTypes.SingleOrDefault((CAbilityFilter.EFilterTargetType x) => x.ToString() == ftype);
						if (eFilterTargetType != CAbilityFilter.EFilterTargetType.None)
						{
							filter.FilterTargetType |= eFilterTargetType;
							continue;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Target Type '" + ftype + "'.  File: " + filename);
						result = false;
					}
				}
				else
				{
					result = false;
				}
			}
			else if (YMLShared.GetMapping(keyMappingEntry, filename, out mapping))
			{
				foreach (MappingEntry entry in mapping.Entries)
				{
					switch (entry.Key.ToString())
					{
					case "TargetType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/TargetType", filename, out var values18))
						{
							foreach (string ftype4 in values18)
							{
								CAbilityFilter.EFilterTargetType eFilterTargetType2 = CAbilityFilter.FilterTargetTypes.SingleOrDefault((CAbilityFilter.EFilterTargetType x) => x.ToString() == ftype4);
								if (eFilterTargetType2 != CAbilityFilter.EFilterTargetType.None)
								{
									filter.FilterTargetType |= eFilterTargetType2;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Target Type '" + ftype4 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "EnemyType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/EnemyType", filename, out var values12))
						{
							foreach (string enemyFilter in values12)
							{
								CAbilityFilter.EFilterEnemy eFilterEnemy = CAbilityFilter.FilterEnemyTypes.SingleOrDefault((CAbilityFilter.EFilterEnemy x) => x.ToString() == enemyFilter);
								if (eFilterEnemy != CAbilityFilter.EFilterEnemy.None)
								{
									filter.FilterEnemy |= eFilterEnemy;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter EnemyType '" + enemyFilter + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ActorType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/ActorType", filename, out var values17))
						{
							foreach (string ftype3 in values17)
							{
								CAbilityFilter.EFilterActorType eFilterActorType = CAbilityFilter.FilterActorTypes.SingleOrDefault((CAbilityFilter.EFilterActorType x) => x.ToString() == ftype3);
								if (eFilterActorType != CAbilityFilter.EFilterActorType.None)
								{
									filter.FilterActorType |= eFilterActorType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Actor Type '" + ftype3 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AllyType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/AllyType", filename, out var values5))
						{
							foreach (string ftype2 in values5)
							{
								CAbilityFilter.EFilterAllyType eFilterAllyType = CAbilityFilter.EFilterAllyTypes.SingleOrDefault((CAbilityFilter.EFilterAllyType x) => x.ToString() == ftype2);
								if (eFilterAllyType != CAbilityFilter.EFilterAllyType.Ally)
								{
									filter.FilterAllyType |= eFilterAllyType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Ally Type '" + ftype2 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Vars":
					{
						if (AbilityData.GetVars(entry, filename, out var vars))
						{
							filter.FilterVars = vars;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "PlayerClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/PlayerClasses", filename, out var values4))
						{
							filter.FilterPlayerClasses = values4.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Player Classes '" + values4?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "EnemyClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/EnemyClasses", filename, out var values11))
						{
							filter.FilterEnemyClasses = values11;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Enemy Classes '" + values11?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "HeroSummonClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/HeroSummonClasses", filename, out var values15))
						{
							filter.FilterHeroSummonClasses = values15.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Hero Summon Classes '" + values15?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "ObjectClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/ObjectClasses", filename, out var values19))
						{
							filter.FilterObjectClasses = values19.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Object Classes '" + values19?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "Summoner":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/Summoner", filename, out var values7))
						{
							filter.FilterSummonerClasses = values7.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Summoner Classes '" + values7?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "Health":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/Health", filename, out var value7))
						{
							if (GetEqualityFilter(value7, "Health", filename, out var equalityFilter5))
							{
								filter.FilterHealth = equalityFilter5;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "HealthSelf":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/HealthSelf", filename, out var value16))
						{
							if (GetEqualityFilter(value16, "HealthSelf", filename, out var equalityFilter9))
							{
								filter.FilterHealthSelf = equalityFilter9;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentActors":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentActors", filename, out var value31))
						{
							if (GetEqualityFilter(value31, "TargetAdjacentActors", filename, out var equalityFilter21))
							{
								filter.FilterTargetAdjacentActors = equalityFilter21;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentEnemies":
					case "TargetAdjacentEnemiesOfCaster":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentEnemies", filename, out var value23))
						{
							if (GetEqualityFilter(value23, "TargetAdjacentEnemies", filename, out var equalityFilter13))
							{
								filter.FilterTargetAdjacentEnemies = equalityFilter13;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentAllies":
					case "TargetAdjacentAlliesOfCaster":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentAllies", filename, out var value26))
						{
							if (GetEqualityFilter(value26, "TargetAdjacentAllies", filename, out var equalityFilter16))
							{
								filter.FilterTargetAdjacentAllies = equalityFilter16;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentSummonsOfCaster":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentSummonsOfCaster", filename, out var value3))
						{
							if (GetEqualityFilter(value3, "TargetAdjacentSummonsOfCaster", filename, out var equalityFilter2))
							{
								filter.FilterTargetAdjacentSummonsOfCaster = equalityFilter2;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentBannerSummonsOfCaster":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentBannerSummonsOfCaster", filename, out var value32))
						{
							if (GetEqualityFilter(value32, "TargetAdjacentBannerSummonsOfCaster", filename, out var equalityFilter22))
							{
								filter.FilterTargetAdjacentSummonsOfCaster = equalityFilter22;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentAlliesOfTarget":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentAlliesOfTarget", filename, out var value21))
						{
							if (GetEqualityFilter(value21, "TargetAdjacentAlliesOfTarget", filename, out var equalityFilter12))
							{
								filter.FilterTargetAdjacentAlliesOfTarget = equalityFilter12;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterAdjacentEnemies":
					case "CasterAdjacentEnemiesOfCaster":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterAdjacentEnemies", filename, out var value15))
						{
							if (GetEqualityFilter(value15, "CasterAdjacentEnemies", filename, out var equalityFilter8))
							{
								filter.FilterCasterAdjacentEnemies = equalityFilter8;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterAdjacentAllies":
					case "CasterAdjacentAlliesOfCaster":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterAdjacentAllies", filename, out var value4))
						{
							if (GetEqualityFilter(value4, "CasterAdjacentAllies", filename, out var equalityFilter3))
							{
								filter.FilterCasterAdjacentAllies = equalityFilter3;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentToWalls":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentToWalls", filename, out var value28))
						{
							if (GetEqualityFilter(value28, "TargetAdjacentToWalls", filename, out var equalityFilter18))
							{
								filter.FilterTargetAdjacentToWalls = equalityFilter18;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterAdjacentToWalls":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterAdjacentToWalls", filename, out var value24))
						{
							if (GetEqualityFilter(value24, "CasterAdjacentToWalls", filename, out var equalityFilter14))
							{
								filter.FilterCasterAdjacentToWalls = equalityFilter14;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetInWater":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetInWater", filename, out var value18))
						{
							if (GetEqualityFilter(value18, "TargetInWater", filename, out var equalityFilter10))
							{
								filter.FilterTargetInWater = equalityFilter10;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentValidTiles":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetAdjacentValidTiles", filename, out var value13))
						{
							if (GetEqualityFilter(value13, "TargetAdjacentValidTiles", filename, out var equalityFilter7))
							{
								filter.FilterTargetAdjacentValidTiles = equalityFilter7;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAdjacentValidTilesFilter":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/TargetAdjacentValidTilesFilter", filename, out var values6))
						{
							filter.FilterTargetAdjacentValidTilesFilterList = new List<CAbilityFilter.EFilterTile>();
							foreach (string stringValue in values6)
							{
								CAbilityFilter.EFilterTile item = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
								filter.FilterTargetAdjacentValidTilesFilterList.Add(item);
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterAdjacentValidTiles":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterAdjacentValidTiles", filename, out var value))
						{
							if (GetEqualityFilter(value, "CasterAdjacentValidTiles", filename, out var equalityFilter))
							{
								filter.FilterCasterAdjacentValidTiles = equalityFilter;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterAdjacentValidTilesFilter":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/CasterAdjacentValidTilesFilter", filename, out var values20))
						{
							filter.FilterCasterAdjacentValidTilesFilterList = new List<CAbilityFilter.EFilterTile>();
							foreach (string stringValue2 in values20)
							{
								CAbilityFilter.EFilterTile item2 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue2);
								filter.FilterCasterAdjacentValidTilesFilterList.Add(item2);
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetHasNegativeCondition":
					case "TargetHasNegativeConditions":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/TargetHasNegativeConditions", filename, out var values16))
						{
							List<CCondition.ENegativeCondition> list6 = new List<CCondition.ENegativeCondition>();
							foreach (string negConditionString2 in values16)
							{
								CCondition.ENegativeCondition eNegativeCondition2 = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == negConditionString2);
								if (eNegativeCondition2 == CCondition.ENegativeCondition.NA)
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid condition in Filter/TargetHasNegativeConditions '" + negConditionString2 + "' File: " + filename);
									result = false;
								}
								else
								{
									list6.Add(eNegativeCondition2);
								}
							}
							if (list6.Count > 0)
							{
								filter.FilterTargetHasNegativeConditions = list6;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterHasNegativeCondition":
					case "CasterHasNegativeConditions":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/CasterHasNegativeCondition", filename, out var values13))
						{
							List<CCondition.ENegativeCondition> list4 = new List<CCondition.ENegativeCondition>();
							foreach (string negConditionString in values13)
							{
								CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == negConditionString);
								if (eNegativeCondition == CCondition.ENegativeCondition.NA)
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid condition in Filter/CasterHasNegativeCondition '" + negConditionString + "' File: " + filename);
									result = false;
								}
								else
								{
									list4.Add(eNegativeCondition);
								}
							}
							if (list4.Count > 0)
							{
								filter.FilterCasterHasNegativeConditions = list4;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetHasPositiveCondition":
					case "TargetHasPositiveConditions":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/TargetHasPositiveConditions", filename, out var values10))
						{
							List<CCondition.EPositiveCondition> list3 = new List<CCondition.EPositiveCondition>();
							foreach (string posConditionString2 in values10)
							{
								CCondition.EPositiveCondition ePositiveCondition2 = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == posConditionString2);
								if (ePositiveCondition2 == CCondition.EPositiveCondition.NA)
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid condition in Filter/TargetHasPositiveConditions '" + posConditionString2 + "' File: " + filename);
									result = false;
								}
								else
								{
									list3.Add(ePositiveCondition2);
								}
							}
							if (list3.Count > 0)
							{
								filter.FilterTargetHasPositiveConditions = list3;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterHasPositiveCondition":
					case "CasterHasPositiveConditions":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/CasterHasPositiveConditions", filename, out var values8))
						{
							List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
							foreach (string posConditionString in values8)
							{
								CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == posConditionString);
								if (ePositiveCondition == CCondition.EPositiveCondition.NA)
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid condition in Filter/CasterHasPositiveConditions '" + posConditionString + "' File: " + filename);
									result = false;
								}
								else
								{
									list.Add(ePositiveCondition);
								}
							}
							if (list.Count > 0)
							{
								filter.FilterCasterHasPositiveConditions = list;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetNegativeConditionCount":
					case "TargetNegativeConditionsCount":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetNegativeConditionsCount", filename, out var value6))
						{
							if (GetEqualityFilter(value6, "TargetNegativeConditionsCount", filename, out var equalityFilter4))
							{
								filter.FilterTargetNegativeConditionCount = equalityFilter4;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterNegativeConditionCount":
					case "CasterNegativeConditionsCount":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterNegativeConditionsCount", filename, out var value34))
						{
							if (GetEqualityFilter(value34, "CasterNegativeConditionsCount", filename, out var equalityFilter23))
							{
								filter.FilterCasterNegativeConditionCount = equalityFilter23;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetPositiveConditionCount":
					case "TargetPositiveConditionsCount":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetPositiveConditionsCount", filename, out var value30))
						{
							if (GetEqualityFilter(value30, "TargetPositiveConditionsCount", filename, out var equalityFilter20))
							{
								filter.FilterTargetPositiveConditionCount = equalityFilter20;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterPositiveConditionCount":
					case "CasterPositiveConditionsCount":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterPositiveConditionsCount", filename, out var value25))
						{
							if (GetEqualityFilter(value25, "CasterPositiveConditionsCount", filename, out var equalityFilter15))
							{
								filter.FilterCasterPositiveConditionCount = equalityFilter15;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetHasImmunities":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/TargetHasImmunities", filename, out var values14))
						{
							List<CAbility.EAbilityType> list5 = new List<CAbility.EAbilityType>();
							foreach (string abilityTypeString2 in values14)
							{
								CAbility.EAbilityType eAbilityType2 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == abilityTypeString2);
								if (eAbilityType2 == CAbility.EAbilityType.None)
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type in Filter/TargetHasImmunities '" + abilityTypeString2 + "' File: " + filename);
									result = false;
								}
								else
								{
									list5.Add(eAbilityType2);
								}
							}
							if (list5.Count > 0)
							{
								filter.FilterTargetHasImmunities = list5;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetImmunitiesCount":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetImmunitiesCount", filename, out var value19))
						{
							if (GetEqualityFilter(value19, "TargetImmunitiesCount", filename, out var equalityFilter11))
							{
								filter.FilterCasterImmunitiesCount = equalityFilter11;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterHasImmunities":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/CasterHasImmunities", filename, out var values9))
						{
							List<CAbility.EAbilityType> list2 = new List<CAbility.EAbilityType>();
							foreach (string abilityTypeString in values9)
							{
								CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == abilityTypeString);
								if (eAbilityType == CAbility.EAbilityType.None)
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type in Filter/CasterHasImmunities '" + abilityTypeString + "' File: " + filename);
									result = false;
								}
								else
								{
									list2.Add(eAbilityType);
								}
							}
							if (list2.Count > 0)
							{
								filter.FilterCasterHasImmunities = list2;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterImmunitiesCount":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CasterImmunitiesCount", filename, out var value11))
						{
							if (GetEqualityFilter(value11, "CasterImmunitiesCount", filename, out var equalityFilter6))
							{
								filter.FilterCasterImmunitiesCount = equalityFilter6;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Invert":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/Invert", filename, out var value10))
						{
							filter.Invert = value10;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CanTargetUntargetable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/CanTargetUntargetable", filename, out var value8))
						{
							filter.CanTargetUntargetable = value8;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "UseTargetOriginalType":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/UseTargetOriginalType", filename, out var value2))
						{
							filter.UseTargetOriginalType = value2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "SpecificAbilityNames":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/SpecificAbilityNames", filename, out var values3))
						{
							filter.SpecificAbilityNames = values3.ToList();
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CheckAdjacentRange":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Filter/CheckAdjacentRange", filename, out var value33))
						{
							filter.CheckAdjacentRange = value33;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CompareTargetHPToYourMissingHP":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/CompareTargetHPToYourMissingHP", filename, out var value29))
						{
							if (GetEqualityFilter(value29, "CompareTargetHPToYourMissingHP", filename, out var equalityFilter19))
							{
								filter.FilterCompareTargetHPToYourMissingHP = equalityFilter19;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetMissingHP":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Filter/TargetMissingHP", filename, out var value27))
						{
							if (GetEqualityFilter(value27, "TargetMissingHP", filename, out var equalityFilter17))
							{
								filter.FilterTargetMissingHP = equalityFilter17;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "IsDoomed":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/IsDoomed", filename, out var value22))
						{
							if (value22)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.IsDoomed;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.IsDoomed;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CarryingQuestItem":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/CarryingQuestItem", filename, out var value20))
						{
							if (value20)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.CarryingQuestItem;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.CarryingQuestItem;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "LootedGoalChest":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/LootedGoalChest", filename, out var value17))
						{
							if (value17)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.LootedGoalChest;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.LootedGoalChest;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetAttackedByCasterThisRound":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/TargetAttackedByCasterThisRound", filename, out var value14))
						{
							if (value14)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.TargetAttackedByCasterThisRound;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.TargetAttackedByCasterThisRound;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetTargetedByCasterPreviousAbility":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/TargetTargetedByCasterPreviousAbility", filename, out var value12))
						{
							if (value12)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.TargetTargetedByCasterPreviousAbility;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.TargetTargetedByCasterPreviousAbility;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetTargetedByAllPreviousAbilitiesInAction":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/TargetTargetedByAllPreviousAbilitiesInAction", filename, out var value9))
						{
							if (value9)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.TargetTargetedByAllPreviousAbilitiesInAction;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.TargetTargetedByAllPreviousAbilitiesInAction;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CasterPreviousMovementEachHexCloserToTarget":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Filter/CasterPreviousMovementEachHexCloserToTarget", filename, out var value5))
						{
							if (value5)
							{
								filter.FilterFlags |= CAbilityFilter.EFilterFlags.CasterPreviousMovementEachHexCloserToTarget;
							}
							else
							{
								filter.FilterFlags &= ~CAbilityFilter.EFilterFlags.CasterPreviousMovementEachHexCloserToTarget;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TargetHasCharacterResource":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/TargetHasCharacterResource", filename, out var values2))
						{
							filter.FilterTargetHasCharacterResource = values2.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter TargetHasCharacterResource '" + values2?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter entry '" + entry.Key.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
				}
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter entry.  File: " + filename);
				result = false;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing an Ability Filter.", ex);
			result = false;
		}
		return result;
	}

	public static bool GetEqualityFilter(string equalityString, string filterName, string filename, out CEqualityFilter equalityFilter)
	{
		bool flag = true;
		equalityFilter = null;
		try
		{
			string[] array = equalityString.Split(' ');
			if (array.Length != 2 && array.Length != 3)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid " + filterName + " Filter " + equalityString + " in file " + filename);
				flag = false;
			}
			if (!CEqualityFilter.IsValidEqualityString(array[0]))
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Equality value in " + filterName + " Filter " + equalityString + " in file " + filename);
				flag = false;
			}
			if (flag)
			{
				bool valueIsPercentage = false;
				if (array[1].EndsWith("%"))
				{
					valueIsPercentage = true;
					array[1] = array[1].Replace("%", "");
				}
				if (int.TryParse(array[1], out var result))
				{
					if (array.Length == 3)
					{
						equalityFilter = new CEqualityFilter(array[0], result, valueIsPercentage, level: true);
					}
					else
					{
						equalityFilter = new CEqualityFilter(array[0], result, valueIsPercentage);
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value in " + filterName + " Filter " + equalityString + " in file " + filename);
					flag = false;
				}
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Exception while getting equality filter.\n", ex);
			flag = false;
		}
		return flag;
	}

	public static CBaseCard.ECardPile GetCardPile(DiscardType discardType)
	{
		return discardType switch
		{
			DiscardType.Discard => CBaseCard.ECardPile.Discarded, 
			DiscardType.Lost => CBaseCard.ECardPile.Lost, 
			DiscardType.PermanentlyLost => CBaseCard.ECardPile.PermanentlyLost, 
			_ => CBaseCard.ECardPile.Discarded, 
		};
	}

	public static CAreaEffect CreateExactRangeAsAreaEffect(int exactRange, string actionName, string filename)
	{
		int num = exactRange * 2;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("0,0,G");
		for (int i = -exactRange; i <= exactRange; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				if ((Math.Abs(i) == exactRange && Math.Abs(j) <= exactRange && Math.Abs(j) % 2 == exactRange % 2) || Math.Abs(j) + Math.Abs(i) == num)
				{
					stringBuilder.Append("|");
					stringBuilder.Append(j);
					stringBuilder.Append(",");
					stringBuilder.Append(i);
					stringBuilder.Append(",R");
				}
			}
		}
		return CreateArea(stringBuilder.ToString(), actionName, filename);
	}

	public static CAreaEffect CreateArea(string areaString, string actionName, string filename)
	{
		List<CAreaEffect.CAreaEffectHex> list = new List<CAreaEffect.CAreaEffectHex>();
		bool melee = false;
		int offsetX = 0;
		int offsetY = 0;
		string[] array = areaString.Split('|');
		bool canFlip = true;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text == "CantFlip")
			{
				continue;
			}
			string[] array3 = text.Split(',');
			if (array3.Length != 3)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Area Effect specified for Action " + actionName + ". File:\n" + filename);
			}
			else if (array3[2] == "G")
			{
				if (int.TryParse(array3[0], out var result) && int.TryParse(array3[1], out var result2))
				{
					offsetX = result * -1;
					offsetY = result2 * -1;
				}
				break;
			}
		}
		array2 = array;
		foreach (string text2 in array2)
		{
			if (text2 == "CantFlip")
			{
				canFlip = false;
				continue;
			}
			string[] array4 = text2.Split(',');
			if (array4.Length == 3)
			{
				CAreaEffect.EHexFlag flags = CAreaEffect.EHexFlag.None;
				switch (array4[2])
				{
				case "G":
					melee = true;
					break;
				case "R":
					flags = CAreaEffect.EHexFlag.Enabled | CAreaEffect.EHexFlag.Enemy;
					break;
				case "B":
					flags = CAreaEffect.EHexFlag.Enabled | CAreaEffect.EHexFlag.Ally;
					break;
				case "S":
					flags = CAreaEffect.EHexFlag.Enabled | CAreaEffect.EHexFlag.Space;
					break;
				case "E":
					flags = CAreaEffect.EHexFlag.Enemy;
					break;
				}
				if (CreateHex(array4[0], array4[1], flags, out var hex, filename, offsetX, offsetY))
				{
					list.Add(hex);
				}
			}
		}
		CAreaEffect cAreaEffect = new CAreaEffect(actionName + "Area", melee, list, canFlip);
		if (Point.AreHexesInAStraightLine(cAreaEffect.Hexes.Select((CAreaEffect.CAreaEffectHex x) => x.ArrayIndex)))
		{
			cAreaEffect.CanFlip = false;
		}
		return cAreaEffect;
	}

	public static bool CreateHex(string xs, string ys, CAreaEffect.EHexFlag flags, out CAreaEffect.CAreaEffectHex hex, string filename, int offsetX, int offsetY)
	{
		if (int.TryParse(xs, out var result) && int.TryParse(ys, out var result2))
		{
			result += offsetX;
			result2 += offsetY;
			result = ((result % 2 != 0) ? ((result - 1) / 2) : (result / 2));
			hex = new CAreaEffect.CAreaEffectHex(result, result2, flags);
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in area effect string. File:\n" + filename);
		hex = null;
		return false;
	}

	public static bool GetElements(DataItem di, string entryName, string filename, out List<ElementInfusionBoardManager.EElement> elements)
	{
		bool result = true;
		elements = new List<ElementInfusionBoardManager.EElement>();
		List<string> values = new List<string>();
		if (YMLShared.GetStringList(di, "Elements", filename, out values))
		{
			foreach (string elementString in values)
			{
				try
				{
					elements.Add(ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString));
				}
				catch
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "No such Element '" + elementString + "' found.  Entry: " + entryName + " File: " + filename);
					result = false;
				}
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element or element sequence.  Entry: " + entryName + " File: " + filename);
			result = false;
		}
		return result;
	}

	public static CardLayout ProcessAbilityLayout(MappingEntry mapping, string key, List<CAbility> linkedAbilities, string abilityCardName, DiscardType discard, string filename, List<AbilityConsume> consumes = null)
	{
		try
		{
			if (YMLShared.GetMapping(mapping, filename, out var mapping2))
			{
				MappingEntry mappingEntry = mapping2.Entries.SingleOrDefault((MappingEntry x) => x.Key.ToString() == key);
				if (mappingEntry != null)
				{
					DataItem value = mappingEntry.Value;
					if (value is Scalar)
					{
						return new CardLayout((value as Scalar).Text, linkedAbilities, abilityCardName, discard, filename);
					}
					if (value is Sequence)
					{
						return new CardLayout(value as Sequence, linkedAbilities, consumes, abilityCardName, discard, filename);
					}
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Child of Layout entry " + key + " is not a sequence.  File:\n" + filename + "\nDataItem:\n" + value.ToString());
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Layout does not contain " + key + " entry.  File:\n" + filename);
				}
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while processing the Ability layout " + key + " File:\n" + filename, ex);
		}
		return null;
	}

	public static List<TreasureTable> GetTreasureTables(List<string> tableNames, string fileName)
	{
		List<TreasureTable> list = new List<TreasureTable>();
		int i;
		for (i = 0; i < tableNames.Count; i++)
		{
			TreasureTable treasureTable = null;
			treasureTable = ((!tableNames[i].StartsWith("£") || !tableNames[i].EndsWith("£")) ? ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable x) => x.Name == tableNames[i]) : new TreasureTable(Guid.NewGuid().ToString(), null, new List<TreasureTableEntry>
			{
				new TreasureTableEntry(ETreasureType.Item, null, null, null, TreasureTablesYML.GetItem(tableNames[i].Substring(1, tableNames[i].Length - 2), fileName).ID)
			}, new List<TreasureTableGroup>(), "Generated"));
			if (treasureTable != null)
			{
				list.Add(treasureTable);
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find treasure table " + tableNames[i] + " in YML. File: " + fileName);
			}
		}
		return list;
	}

	public static bool GetAbilities(MappingEntry actionEntry, int cardID, bool isMonster, string filename, out List<CAbility> abilities, bool isMergedAbility = false)
	{
		bool result = true;
		abilities = new List<CAbility>();
		if (YMLShared.GetMapping(actionEntry, filename, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				if (YMLShared.GetMapping(entry, filename, out var mapping2))
				{
					Mapping abilityMapping = mapping2;
					string name = entry.Key.ToString();
					bool isMonster2 = isMonster;
					bool isMergedAbility2 = isMergedAbility;
					if (AbilityData.ProcessAbilityEntry(filename, abilityMapping, name, cardID, isMonster2, out var ability, isSubAbility: false, isConsumeAbility: false, null, null, null, null, null, null, null, null, null, null, isMergedAbility2))
					{
						abilities.Add(ability);
					}
					else
					{
						result = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse Abilities.  Property must be Mapping. File: " + filename);
					result = false;
				}
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse Abilities.  Property must be Mapping. File: " + filename);
			result = false;
		}
		return result;
	}
}
