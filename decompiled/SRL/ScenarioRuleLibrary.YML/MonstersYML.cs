using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonstersYML : BinarySerializableYMLProcessor<MonsterYMLData>
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<MonsterYMLData> LoadedYML { get; private set; }

	public MonstersYML()
	{
		LoadedYML = new ConcurrentQueue<MonsterYMLData>();
	}

	public override bool AddToDataSource(MonsterYMLData data)
	{
		string fileName = data.FileName;
		bool flag = true;
		if (data.ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
			flag = false;
		}
		if (flag)
		{
			LoadedYML.Enqueue(data);
		}
		return flag;
	}

	public override bool ProcessRawData(MonsterYMLData data, string rawData)
	{
		string fileName = data.FileName;
		bool result = true;
		try
		{
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(rawData);
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value5))
						{
							if (value5.Length <= 64)
							{
								data.ID = value5;
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Monster ID exceeds max allowed length (64) in file " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "OnlyValidInLevels":
					{
						if (YMLShared.GetStringList(entry.Value, entry.Key.ToString(), fileName, out var values4))
						{
							data.OnlyValidInLevels = values4;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "LocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocKey", fileName, out var value17))
						{
							data.LocKey = CardProcessingShared.GetLookupValue(value17);
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "MonsterType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MonsterType", fileName, out var monsterTypeString))
						{
							EMonsterType eMonsterType = MonsterYMLData.MonsterTypes.Single((EMonsterType x) => x.ToString() == monsterTypeString);
							if (eMonsterType != EMonsterType.None)
							{
								data.MonsterType = eMonsterType;
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
						continue;
					}
					case "Model":
					{
						if (YMLShared.GetStringList(entry.Value, "Model", fileName, out var values))
						{
							data.Models = new List<string>();
							foreach (string item in values)
							{
								if (GetNPCModelEnumFromSpaceName(item) != CClass.ENPCModel.None)
								{
									if (!data.Models.Contains(item))
									{
										data.Models.Add(item);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Model '" + item + "' specified in file " + fileName);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "NonEliteVariant":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NonEliteVariant", fileName, out var value10))
						{
							data.NonEliteVariant = value10;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "CardsPoolClass":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CardsPoolClass", fileName, out var value13))
						{
							data.CardsPoolClass = value13;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "PredominantlyMelee":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PredominantlyMelee", fileName, out var value15))
						{
							data.PredominantlyMelee = value15;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "AlwaysShowInnateImmunities":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "AlwaysShowInnateImmunities", fileName, out var value7))
						{
							data.AlwaysShowInnateImmunities = value7;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "AbilityCards":
					{
						data.AddRawYML(entry);
						if (YMLShared.GetIntList(entry.Value, "AbilityCards", fileName, out var values3))
						{
							foreach (int cardID in values3)
							{
								MonsterCardYMLData monsterCardYMLData = ScenarioRuleClient.SRLYML.MonsterCards.FirstOrDefault((MonsterCardYMLData s) => s.ID == cardID);
								if (monsterCardYMLData != null)
								{
									if (data.AbilityCards == null)
									{
										data.AbilityCards = new List<MonsterCardYMLData>();
									}
									data.AbilityCards.Add(monsterCardYMLData);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster Card with ID " + cardID + " in file " + fileName);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "AddNumberOfPlayersTo":
					{
						if (YMLShared.GetStringList(entry.Value, "AddNumberOfPlayersTo", fileName, out var values6))
						{
							foreach (string addProp in values6)
							{
								EMonsterBaseStats eMonsterBaseStats = MonsterYMLData.MonsterBaseStatsEnums.FirstOrDefault((EMonsterBaseStats s) => s.ToString() == addProp);
								if (eMonsterBaseStats != EMonsterBaseStats.None)
								{
									if (data.AddNumberOfPlayersTo == null)
									{
										data.AddNumberOfPlayersTo = new List<EMonsterBaseStats>();
									}
									data.AddNumberOfPlayersTo.Add(eMonsterBaseStats);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid value specified for AddNumberOfPlayersTo in file " + fileName);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "MonsterClassIDToActImmediatelyBefore":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MonsterClassIDToActImmediatelyBefore", fileName, out var value12))
						{
							data.MonsterClassIDToActImmediatelyBefore = value12;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "StandeeLimit":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "StandeeLimit", fileName, out var value8))
						{
							data.StandeeLimit = value8;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "TreatAsNonMonster":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatAsNonMonster", fileName, out var value3))
						{
							data.TreatAsNonMonster = value3;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "IgnoreCardPoolStandeeLimit":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreCardPoolStandeeLimit", fileName, out var value4))
						{
							data.IgnoreCardPoolStandeeLimit = value4;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "MiniBoss":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "MiniBoss", fileName, out var value16))
						{
							data.MiniBoss = value16;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "FocusingInitiative":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "FocusingInitiative", fileName, out var value2))
						{
							data.FocusingInitiative = value2;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "UseNormalSizeAvatarForBoss":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseNormalSizeAvatarForBoss", fileName, out var value11))
						{
							data.UseNormalSizeAvatarForBoss = value11;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "Colour":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Colour", fileName, out var value))
						{
							if (value.Length == 3 || value.Length == 4 || value.Length == 6 || value.Length == 8)
							{
								bool flag = true;
								string text = value;
								foreach (char c in text)
								{
									if ((c < 'A' || c > 'F') && (c < '0' || c > '9'))
									{
										flag = false;
										break;
									}
								}
								if (flag)
								{
									value = (data.ColourHTML = "#" + value);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Incorrect Colour format. Use only hexadecimal characters in file " + fileName);
								result = false;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Colour property has to be exactly 3, 4, 6 or 8 characters long in file " + fileName);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "ChunkModifier":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "ChunkModifier", fileName, out var value14))
						{
							if (value14 >= 0f && value14 <= 1f)
							{
								data.Fatness = value14;
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "ChunkModifier is out of range. It has to be a value between 0 and 1 in file " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "Vertex":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "Vertex", fileName, out var value9))
						{
							if (value9 >= -0.5f && value9 <= 0.5f)
							{
								data.VertexAnimIntensity = value9;
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Vertex is out of range. It has to be a value between -0.5 and 0.5 in file " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "CustomConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomConfig", fileName, out var value6))
						{
							data.CustomConfig = value6;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "Flags":
					{
						if (YMLShared.GetStringList(entry.Value, "Flags", fileName, out var values5))
						{
							EMonsterFlag eMonsterFlag = EMonsterFlag.None;
							foreach (string item2 in values5)
							{
								if (Enum.TryParse<EMonsterFlag>(item2, out var result2))
								{
									eMonsterFlag |= result2;
								}
							}
							data.Flags = eMonsterFlag;
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "CanMoveOnActorClassID":
					{
						if (YMLShared.GetStringList(entry.Value, "CanMoveOnActorClassID", fileName, out var values2))
						{
							foreach (string item3 in values2)
							{
								if (data.CanMoveOnActorClassID == null)
								{
									List<string> list = (data.CanMoveOnActorClassID = new List<string>());
								}
								data.CanMoveOnActorClassID.Add(item3);
							}
						}
						else
						{
							result = false;
						}
						continue;
					}
					case "Parser":
						continue;
					}
					data.AddRawYML(entry);
					if (entry.Key.ToString().Length > 4 && entry.Key.ToString().Substring(0, 5) == "Level")
					{
						try
						{
							if (int.TryParse(entry.Key.ToString()[5].ToString(), out var result3))
							{
								if (result3 >= 0 && result3 < 8 && entry.Value is Mapping)
								{
									if (!GetBaseStats(entry, result3, out var baseStats, fileName, data.MonsterType == EMonsterType.Object))
									{
										return false;
									}
									if (data.MonsterBaseStats == null)
									{
										data.MonsterBaseStats = new List<BaseStats>();
									}
									data.MonsterBaseStats.Add(baseStats);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Base Stats entry " + entry.Key.ToString() + " in file " + fileName);
									result = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Base Stats entry " + entry.Key.ToString() + " in file " + fileName);
								result = false;
							}
						}
						catch
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Base Stats entry " + entry.Key.ToString() + " in file " + fileName);
							result = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						result = false;
					}
				}
				return result;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}

	private bool GetBaseStats(MappingEntry parentMapEntry, int level, out BaseStats baseStats, string fileName, bool isObject = false)
	{
		bool flag = true;
		baseStats = null;
		int num = int.MaxValue;
		int num2 = ((!isObject) ? int.MaxValue : 0);
		int num3 = ((!isObject) ? int.MaxValue : 0);
		int num4 = ((!isObject) ? int.MaxValue : 0);
		int num5 = ((!isObject) ? int.MaxValue : 0);
		int shield = 0;
		int retaliate = 0;
		int retaliateRange = 0;
		int pierce = 0;
		int push = 0;
		bool flying = false;
		bool advantage = false;
		bool attackersGainDisadvantage = false;
		bool invulnerable = false;
		bool pierceInvulnerablility = false;
		bool untargetable = false;
		bool immuneToForcedMovement = false;
		bool doesNotBlock = false;
		bool isWaterMonster = false;
		bool ignoreActorCollision = false;
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		List<CAbility.EAbilityType> list2 = new List<CAbility.EAbilityType>();
		Dictionary<CAbility.EAbilityType, int> dictionary = new Dictionary<CAbility.EAbilityType, int>();
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>();
		List<CAbility> onDeathAbilities = new List<CAbility>();
		bool canActivatePressurePlates = false;
		if (isObject)
		{
			list2.AddRange(new List<CAbility.EAbilityType>
			{
				CAbility.EAbilityType.Shield,
				CAbility.EAbilityType.Retaliate,
				CAbility.EAbilityType.Invisible,
				CAbility.EAbilityType.Poison,
				CAbility.EAbilityType.Wound,
				CAbility.EAbilityType.Muddle,
				CAbility.EAbilityType.Immobilize,
				CAbility.EAbilityType.Disarm,
				CAbility.EAbilityType.Curse,
				CAbility.EAbilityType.Stun,
				CAbility.EAbilityType.Strengthen,
				CAbility.EAbilityType.Bless,
				CAbility.EAbilityType.Push,
				CAbility.EAbilityType.Pull,
				CAbility.EAbilityType.Teleport2,
				CAbility.EAbilityType.ControlActor,
				CAbility.EAbilityType.StopFlying,
				CAbility.EAbilityType.Swap,
				CAbility.EAbilityType.Sleep,
				CAbility.EAbilityType.Bane,
				CAbility.EAbilityType.Regenerate,
				CAbility.EAbilityType.Ward,
				CAbility.EAbilityType.Brittle
			});
		}
		if (YMLShared.GetMapping(parentMapEntry, fileName, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "Health":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Health", fileName, out var value2))
					{
						num = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Move":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Move", fileName, out var value14))
					{
						num2 = value14;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Attack":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Attack", fileName, out var value9))
					{
						num3 = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Range":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Range", fileName, out var value7))
					{
						num4 = value7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Target":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Target", fileName, out var value20))
					{
						num5 = value20;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Shield":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Shield", fileName, out var value4))
					{
						shield = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Retaliate":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Retaliate", fileName, out var value21))
					{
						retaliate = value21;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RetaliateRange":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "RetaliateRange", fileName, out var value12))
					{
						retaliateRange = value12;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Pierce":
				{
					string value17;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", fileName, out var value16, suppressErrors: true))
					{
						pierce = value16;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Pierce", fileName, out value17))
					{
						if (value17 == "All")
						{
							pierce = 99999;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Pierce value " + value17 + " in file " + fileName);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Push":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Push", fileName, out var value11))
					{
						push = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Flying":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value5))
					{
						flying = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Advantage":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Advantage", fileName, out var value23))
					{
						advantage = value23;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AttackersGainDisadvantage":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AttackersGainDisadvantage", fileName, out var value18))
					{
						attackersGainDisadvantage = value18;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Invulnerable":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Invulnerable", fileName, out var value13))
					{
						invulnerable = value13;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PierceInvulnerability":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "PierceInvulnerability", fileName, out var value10))
					{
						pierceInvulnerablility = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Untargetable":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Untargetable", fileName, out var value6))
					{
						untargetable = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ImmuneToForcedMovement":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ImmuneToForcedMovement", fileName, out var value3))
					{
						immuneToForcedMovement = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "DoesNotBlock":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "DoesNotBlock", fileName, out var value22))
					{
						doesNotBlock = value22;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IsWaterMonster":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsWaterMonster", fileName, out var value19))
					{
						isWaterMonster = value19;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreActorCollision":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreActorCollision", fileName, out var value15))
					{
						ignoreActorCollision = value15;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Conditions":
				{
					if (YMLShared.GetStringList(entry.Value, "AttackersGainDisadvantage", fileName, out var values2))
					{
						foreach (string negString in values2)
						{
							CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.FirstOrDefault((CCondition.ENegativeCondition s) => s.ToString() == negString);
							if (eNegativeCondition != CCondition.ENegativeCondition.NA)
							{
								list.Add(eNegativeCondition);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid condition '" + negString + "' specified for Condtions in file " + fileName);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Immunities":
				{
					if (YMLShared.GetStringList(entry.Value, "Immunities", fileName, out var values))
					{
						foreach (string abilityTypeString in values)
						{
							CAbility.EAbilityType eAbilityType2 = CAbility.AbilityTypes.FirstOrDefault((CAbility.EAbilityType s) => s.ToString() == abilityTypeString);
							if (eAbilityType2 != CAbility.EAbilityType.None)
							{
								list2.Add(eAbilityType2);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ability type '" + abilityTypeString + "' specified for Immunities in file " + fileName);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Specials":
				{
					if (YMLShared.GetMapping(entry, fileName, out var mapping2))
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							if (YMLShared.GetStringPropertyValue(entry2.Key, "Specials/" + entry.Key, fileName, out var abilityString) && YMLShared.GetIntPropertyValue(entry2.Value, "Specials/" + entry.Key, fileName, out var value8))
							{
								CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.FirstOrDefault((CAbility.EAbilityType s) => s.ToString() == abilityString);
								if (eAbilityType != CAbility.EAbilityType.None)
								{
									dictionary.Add(eAbilityType, value8);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry '" + abilityString + "' specified for Specials in file " + fileName);
								flag = false;
							}
							else
							{
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
				case "StatIsBasedOnX":
				{
					if (AbilityData.ParseStatIsBasedOnX(entry, fileName, out var statIsBasedOnXEntries2, isBaseStats: true))
					{
						statIsBasedOnXEntries = statIsBasedOnXEntries2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "OnDeathAbilities":
				{
					if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out var abilities))
					{
						onDeathAbilities = abilities;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CanActivatePressurePlates":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanActivatePressurePlates", fileName, out var value))
					{
						canActivatePressurePlates = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in file " + fileName);
					flag = false;
					break;
				}
			}
		}
		else
		{
			flag = false;
		}
		if (num == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Health specified for monster in file " + fileName);
			flag = false;
		}
		if (!isObject)
		{
			if (num2 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Move specified for monster in file " + fileName);
				flag = false;
			}
			if (num3 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Attack specified for monster in file " + fileName);
				flag = false;
			}
			if (num4 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Range specified for monster in file " + fileName);
				flag = false;
			}
			if (num5 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Target specified for monster in file " + fileName);
				flag = false;
			}
		}
		if (flag)
		{
			baseStats = new BaseStats(level, num, num2, num3, num4, num5, shield, retaliate, retaliateRange, pierce, push, flying, advantage, attackersGainDisadvantage, invulnerable, pierceInvulnerablility, untargetable, doesNotBlock, isWaterMonster, ignoreActorCollision, list, list2, dictionary, statIsBasedOnXEntries, onDeathAbilities, canActivatePressurePlates, immuneToForcedMovement);
		}
		return flag;
	}

	public static CClass.ENPCModel GetNPCModelEnumFromSpaceName(string spacedName)
	{
		string noSpaces = spacedName.Replace(" ", string.Empty);
		return CClass.NPCModels.FirstOrDefault((CClass.ENPCModel s) => s.ToString() == noSpaces);
	}
}
