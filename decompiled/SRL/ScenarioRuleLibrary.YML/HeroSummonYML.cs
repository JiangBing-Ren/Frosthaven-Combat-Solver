using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class HeroSummonYML : BinarySerializableYMLProcessor<HeroSummonYMLData>
{
	public ConcurrentQueue<HeroSummonYMLData> LoadedYML { get; private set; }

	public HeroSummonYML()
	{
		LoadedYML = new ConcurrentQueue<HeroSummonYMLData>();
	}

	public override bool AddToDataSource(HeroSummonYMLData summonData)
	{
		string fileName = summonData.FileName;
		bool flag = true;
		if (summonData.ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
			flag = false;
		}
		if (flag)
		{
			LoadedYML.Enqueue(summonData);
		}
		return flag;
	}

	public override bool ProcessRawData(HeroSummonYMLData summonData, string rawData)
	{
		string fileName = summonData.FileName;
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
					bool useSpecialBaseState;
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value26))
						{
							if (value26.Length <= 64)
							{
								summonData.ID = value26;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Summon ID exceeds max allowed length (64) in file " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "LocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocKey", fileName, out var value11))
						{
							summonData.LocKey = value11;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Model":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Model", fileName, out var value8))
						{
							CClass.ENPCModel nPCModelEnumFromSpaceName = GetNPCModelEnumFromSpaceName(value8);
							if (nPCModelEnumFromSpaceName != CClass.ENPCModel.None)
							{
								summonData.Model = value8;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Summon Model " + nPCModelEnumFromSpaceName.ToString() + " File:\n" + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Attack":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Attack", fileName, out int amount2, out useSpecialBaseState, out List<EEnhancementMarkType> enhancementSlots2, out ElementInfusionBoardManager.EElement? infuse, false))
						{
							summonData.Attack = amount2;
							summonData.AttackEnhancements = enhancementSlots2;
							summonData.AttackInfuse = infuse;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Range":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Range", fileName, out var amount4, out useSpecialBaseState, out var enhancementSlots4))
						{
							summonData.Range = amount4;
							summonData.RangeEnhancements = enhancementSlots4;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Move":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Move", fileName, out var amount, out useSpecialBaseState, out var enhancementSlots))
						{
							summonData.Move = amount;
							summonData.MoveEnhancements = enhancementSlots;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Health":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Health", fileName, out var amount3, out useSpecialBaseState, out var enhancementSlots3))
						{
							summonData.HealthTable = new int[10] { amount3, amount3, amount3, amount3, amount3, amount3, amount3, amount3, amount3, amount3 };
							summonData.HealthEnhancements = enhancementSlots3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "HealthTable":
					{
						if (YMLShared.GetIntArray(entry.Value, "Health", fileName, 10, out var outArray, ignorePositionZero: true))
						{
							summonData.HealthTable = outArray;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Shield":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Shield", fileName, out var value19))
						{
							summonData.Shield = value19;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Retaliate":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Retaliate", fileName, out var value5))
						{
							summonData.Retaliate = value5;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "RetaliateRange":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "RetaliateRange", fileName, out var value3))
						{
							summonData.RetaliateRange = value3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Pierce":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", fileName, out var value25))
						{
							summonData.Pierce = value25;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Flying":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value17))
						{
							summonData.Flying = value17;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Invulnerable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Invulnerable", fileName, out var value9))
						{
							summonData.Invulnerable = value9;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "PierceInvulnerability":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PierceInvulnerability", fileName, out var value31))
						{
							summonData.PierceInvulnerability = value31;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Untargetable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Untargetable", fileName, out var value29))
						{
							summonData.Untargetable = value29;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "DoesNotBlock":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "DoesNotBlock", fileName, out var value22))
						{
							summonData.DoesNotBlock = value22;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "PlayerControlled":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PlayerControlled", fileName, out var value23))
						{
							summonData.PlayerControlled = value23;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "IgnoreActorCollision":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreActorCollision", fileName, out var value16))
						{
							summonData.IgnoreActorCollision = value16;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TreatAsTrap":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatAsTrap", fileName, out var value10))
						{
							summonData.TreatAsTrap = value10;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AttackAnimOverload":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AttackAnimOverload", fileName, out var value12))
						{
							if (value12.ToLower() == "null")
							{
								summonData.NullAttackAnimOverload = true;
							}
							else
							{
								summonData.AttackAnimOverload = value12;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AttackOnDeathAnimOverload":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AttackOnDeathAnimOverload", fileName, out var value28))
						{
							if (value28.ToLower() == "null")
							{
								summonData.NullAttackOnDeathAnimOverload = true;
							}
							else
							{
								summonData.AttackOnDeathAnimOverload = value28;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AdjacentAttackOnDeath":
					{
						int value21;
						if (YMLShared.GetStringPropertyValue(entry.Value, "AdjacentAttackOnDeath", fileName, out var value20) && value20.ToLower() == "null")
						{
							summonData.NullAdjacentAttackOnDeathNullable = true;
						}
						else if (YMLShared.GetIntPropertyValue(entry.Value, "AdjacentAttackOnDeath", fileName, out value21))
						{
							summonData.AdjacentAttackOnDeath = value21;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "StatIsBasedOnX":
					{
						List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "StatIsBasedOnX", fileName, out var value15) && value15.ToLower() == "null")
						{
							summonData.NullStatIsBasedOnXEntries = true;
						}
						else if (AbilityData.ParseStatIsBasedOnX(entry, fileName, out statIsBasedOnXEntries, isBaseStats: true))
						{
							summonData.StatIsBasedOnXEntriesRaw = entry.GetYML();
							summonData.StatIsBasedOnXEntries = statIsBasedOnXEntries;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "OnAttackCondition":
					case "OnAttackConditions":
					{
						List<CCondition.ENegativeCondition> negativeConditions;
						List<CCondition.EPositiveCondition> positiveConditions;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "OnAttackCondition", fileName, out var value2) && value2.ToLower() == "null")
						{
							summonData.NullOnAttackConditions = true;
						}
						else if (AbilityData.GetConditions(entry, fileName, out negativeConditions, out positiveConditions))
						{
							summonData.OnAttackConditions = negativeConditions;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "OnSummonAbilities":
					{
						List<CAbility> abilities3;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "OnSummonAbilities", fileName, out var value27) && value27.ToLower() == "null")
						{
							summonData.NullOnSummonAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out abilities3))
						{
							summonData.AddRawYML(entry);
							summonData.OnSummonAbilities = abilities3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "OnDeathAbilities":
					{
						List<CAbility> abilities2;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "OnDeathAbilities", fileName, out var value24) && value24.ToLower() == "null")
						{
							summonData.NullOnDeathAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out abilities2))
						{
							summonData.AddRawYML(entry);
							summonData.OnDeathAbilities = abilities2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "SpecialAbilities":
					{
						List<CAbility> abilities;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "SpecialAbilities", fileName, out var value14) && value14.ToLower() == "null")
						{
							summonData.NullSpecialAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out abilities))
						{
							summonData.AddRawYML(entry);
							summonData.SpecialAbilities = abilities;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "MoveOverride":
					{
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "MoveOverride", fileName, out var value6) && value6.ToLower() == "null")
						{
							summonData.NullMoveOverride = true;
							break;
						}
						summonData.AddRawYML(entry);
						summonData.MoveOverride = CAbilityOverride.CreateAbilityOverride(entry, 0, isMonster: false, fileName, "", isConsume: false, "DefaultMove");
						if (summonData.MoveOverride == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "MoveOverride was null.  File: " + fileName);
							result = false;
						}
						break;
					}
					case "AttackOverride":
					{
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "AttackOverride", fileName, out var value30) && value30.ToLower() == "null")
						{
							summonData.NullAttackOverride = true;
							break;
						}
						summonData.AddRawYML(entry);
						summonData.AttackOverride = CAbilityOverride.CreateAbilityOverride(entry, 0, isMonster: false, fileName, "", isConsume: false, "DefaultAttack");
						if (summonData.AttackOverride == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "AttackOverride was null.  File: " + fileName);
							result = false;
						}
						break;
					}
					case "AIFocusOverride":
					{
						CAIFocusOverrideDetails aiFocusOverride = null;
						if (AbilityData.ParseAIFocusOverride(entry.Value, fileName, ref aiFocusOverride))
						{
							summonData.AIFocusOverrideDetails = aiFocusOverride;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Colour":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Colour", fileName, out var value18))
						{
							if (value18.Length == 3 || value18.Length == 4 || value18.Length == 6 || value18.Length == 8)
							{
								bool flag = true;
								string text = value18;
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
									value18 = (summonData.ColourHTML = "#" + value18);
									break;
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
						break;
					}
					case "ChunkModifier":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "ChunkModifier", fileName, out var value13))
						{
							if (value13 >= 0f && value13 <= 1f)
							{
								summonData.Fatness = value13;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "ChunkModifier is out of range. It has to be a value between 0 and 1 in file " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Vertex":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "Vertex", fileName, out var value7))
						{
							if (value7 >= -0.5f && value7 <= 0.5f)
							{
								summonData.VertexAnimIntensity = value7;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Vertex is out of range. It has to be a value between -0.5 and 0.5 in file " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CustomConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomConfig", fileName, out var value4))
						{
							summonData.CustomConfig = value4;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CanMoveOnActorClassID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CanMoveOnActorClassID", fileName, out var value))
						{
							summonData.CanMoveOnActorClassID = value;
						}
						else
						{
							result = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry.Key.ToString() + " File:\n" + fileName);
						result = false;
						break;
					}
				}
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return result;
	}

	private static CClass.ENPCModel GetNPCModelEnumFromSpaceName(string spacedName)
	{
		string noSpaces = spacedName.Replace(" ", string.Empty);
		return CClass.NPCModels.FirstOrDefault((CClass.ENPCModel s) => s.ToString() == noSpaces);
	}
}
