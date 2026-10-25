using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AttackModifiersYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<AttackModifierYMLData> LoadedYML { get; private set; }

	public AttackModifiersYML()
	{
		LoadedYML = new ConcurrentQueue<AttackModifierYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AttackModifierYMLData attackModifierYMLData = new AttackModifierYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			attackModifierYMLData.IsConditionalModifier = false;
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value8))
						{
							attackModifierYMLData.Name = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CustomCombatLogDescriptionLocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomCombatLogDescriptionLocKey", fileName, out var value6))
						{
							attackModifierYMLData.CustomCombatLogDescriptionLocKey = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DescriptionForToggleAlternate":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "DescriptionForToggleAlternate", fileName, out var value14))
						{
							attackModifierYMLData.DescriptionForToggleAlternate = value14;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MathModifier":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MathModifier", fileName, out var value3))
						{
							if (value3.Contains(" "))
							{
								value3 = value3.Replace(" ", string.Empty);
							}
							if (int.TryParse(value3.Substring(1), out var result))
							{
								if (result < -2 || result > 2)
								{
									attackModifierYMLData.IsConditionalModifier = true;
								}
								else
								{
									attackModifierYMLData.IsConditionalModifier = false;
								}
								attackModifierYMLData.MathModifier = value3;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to Parse MathModifier " + value3 + " in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AlternateModifier":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AlternateModifier", fileName, out var value10))
						{
							if (value10.Contains(" "))
							{
								value10 = value10.Replace(" ", string.Empty);
							}
							if (int.TryParse(value10.Substring(1), out var result2))
							{
								if (result2 < -2 || result2 > 2)
								{
									attackModifierYMLData.IsConditionalModifier = true;
								}
								else
								{
									attackModifierYMLData.IsConditionalModifier = false;
								}
								attackModifierYMLData.AlternateMathModifier = value10;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to Parse AlternateMathModifier " + value10 + " in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AlternateFilter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry, fileName, out var filter))
						{
							attackModifierYMLData.AlternateFilter = filter;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Shuffle":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Shuffle", fileName, out var value11))
						{
							attackModifierYMLData.Shuffle = value11;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Rolling":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Rolling", fileName, out var value7))
						{
							attackModifierYMLData.Rolling = value7;
							attackModifierYMLData.IsConditionalModifier = true;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DamageIfTargetMoves":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "DamageIfTargetMoves", fileName, out var value5))
						{
							attackModifierYMLData.DamageIfTargetMoves = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TryToPerformAsSummoner":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TryToPerformAsSummoner", fileName, out var value13))
						{
							attackModifierYMLData.TryToPerformAsSummoner = value13;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ControlTargetingActor":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ControlTargetingActor", fileName, out var value12))
						{
							attackModifierYMLData.ControlTargetingActor = value12;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AddTarget":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "AddTarget", fileName, out var value9))
						{
							attackModifierYMLData.AddTarget = value9;
							attackModifierYMLData.IsConditionalModifier = true;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IsBless":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsBless", fileName, out var value4))
						{
							attackModifierYMLData.IsBless = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IsCurse":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsCurse", fileName, out var value2))
						{
							attackModifierYMLData.IsCurse = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Abilities":
					{
						List<CAbility> abilities;
						if (entry.Value is Scalar scalar2 && scalar2.Text.ToLower() == "null")
						{
							attackModifierYMLData.NullAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, attackModifierYMLData.Name.GetHashCode(), isMonster: false, fileName, out abilities))
						{
							attackModifierYMLData.Abilities = abilities;
							attackModifierYMLData.IsConditionalModifier = true;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AbilityOverrides":
						if (entry.Value is Scalar scalar && scalar.Text.ToLower() == "null")
						{
							attackModifierYMLData.NullOverrides = true;
						}
						else if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping))
							{
								foreach (MappingEntry entry2 in mapping.Entries)
								{
									CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry2, attackModifierYMLData.Name.GetHashCode(), isMonster: false, fileName, string.Empty);
									if (cAbilityOverride != null)
									{
										if (attackModifierYMLData.Overrides == null)
										{
											attackModifierYMLData.Overrides = new List<CAbilityOverride>();
										}
										attackModifierYMLData.Overrides.Add(cAbilityOverride);
										attackModifierYMLData.IsConditionalModifier = true;
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
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry for AbilityOverrides.  Entry must be mapping.  File: " + fileName);
							flag = false;
						}
						break;
					case "UseGenericIcon":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseGenericIcon", fileName, out var value))
						{
							attackModifierYMLData.UseGenericIcon = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
						break;
					}
				}
				if (attackModifierYMLData.Name == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Name specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					LoadedYML.Enqueue(attackModifierYMLData);
				}
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}

	public static AttackModifierYMLData CreateCurse()
	{
		string curseName = (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness) ? "Reduced_Randomness_Curse" : "Curse");
		AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData f) => f.Name == curseName)?.Copy();
		if (attackModifierYMLData == null)
		{
			attackModifierYMLData = new AttackModifierYMLData(CCondition.ENegativeCondition.Curse.ToString(), "*0", isCurse: true, isBless: false);
			ScenarioRuleClient.SRLYML.Data.AttackModifiers.LoadedYML.Enqueue(attackModifierYMLData);
		}
		return attackModifierYMLData;
	}

	public static AttackModifierYMLData CreateBless()
	{
		string blessName = (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness) ? "Reduced_Randomness_Bless" : "Bless");
		AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData f) => f.Name == blessName)?.Copy();
		if (attackModifierYMLData == null)
		{
			attackModifierYMLData = new AttackModifierYMLData(CCondition.EPositiveCondition.Bless.ToString(), "*2", isCurse: false, isBless: true);
			ScenarioRuleClient.SRLYML.Data.AttackModifiers.LoadedYML.Enqueue(attackModifierYMLData);
		}
		return attackModifierYMLData;
	}
}
