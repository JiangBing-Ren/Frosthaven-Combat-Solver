using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ItemData
{
	public List<CAbility> Abilities { get; private set; }

	public List<CAbilityOverride> Overrides { get; private set; }

	public CAbilityFilter CharacterFilter { get; private set; }

	public CAbilityCompare CompareAbility { get; private set; }

	public CAbilityRequirements ItemRequirements { get; private set; }

	public List<string> CompareConditions { get; private set; }

	public int ShieldValue { get; private set; }

	public int RetaliateValue { get; private set; }

	public int RetaliateRange { get; private set; }

	public int SmallSlots { get; private set; }

	public Dictionary<AttackModifierYMLData, int> AdditionalModifiers { get; private set; }

	public Dictionary<AttackModifierYMLData, int> RemoveModifiers { get; private set; }

	public List<(string key, TValue value)> Vars { get; private set; }

	public bool ProcessEntry(MappingEntry entry, int cardID, string fileName)
	{
		try
		{
			bool result = true;
			AdditionalModifiers = new Dictionary<AttackModifierYMLData, int>();
			RemoveModifiers = new Dictionary<AttackModifierYMLData, int>();
			RetaliateRange = 1;
			if (YMLShared.GetMapping(entry, fileName, out var mapping))
			{
				foreach (MappingEntry entry2 in mapping.Entries)
				{
					if (entry2 == null)
					{
						continue;
					}
					switch (entry2.Key.ToString())
					{
					case "Vars":
					{
						if (AbilityData.GetVars(entry2, fileName, out var vars))
						{
							Vars = vars;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Abilities":
					{
						if (CardProcessingShared.GetAbilities(entry2, cardID, isMonster: false, fileName, out var abilities))
						{
							Abilities = abilities;
							foreach (CAbility ability in Abilities)
							{
								ability.IsItemAbility = true;
								if (ability.SubAbilities.Count > 0)
								{
									foreach (CAbility subAbility in ability.SubAbilities)
									{
										subAbility.IsItemAbility = true;
									}
								}
								if (ability.ActiveBonusData.AbilityData != null)
								{
									ability.ActiveBonusData.AbilityData.IsItemAbility = true;
								}
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Overrides":
					{
						Overrides = new List<CAbilityOverride>();
						if (YMLShared.GetMapping(entry2, fileName, out var mapping2))
						{
							foreach (MappingEntry entry3 in mapping2.Entries)
							{
								CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry3, cardID, isMonster: false, fileName);
								if (cAbilityOverride != null)
								{
									Overrides.Add(cAbilityOverride);
								}
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CharacterFilter":
					{
						if (CardProcessingShared.GetAbilityFilter(entry2, fileName, out var filter))
						{
							CharacterFilter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CompareAbility":
					{
						if (CAbilityCompare.GetAbilityCompare(entry2, fileName, out var compareAbility))
						{
							CompareAbility = compareAbility;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ItemRequirements":
					{
						if (AbilityData.GetAbilityRequirements(entry2, fileName, out var abilityRequirements))
						{
							ItemRequirements = abilityRequirements;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Conditions":
					{
						if (YMLShared.GetStringList(entry2.Value, "Conditions", fileName, out var values))
						{
							CompareConditions = values;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ShieldValue":
					{
						if (YMLShared.GetIntPropertyValue(entry2.Value, "ShieldValue", fileName, out var value))
						{
							ShieldValue = value;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "RetaliateValue":
					{
						if (YMLShared.GetIntPropertyValue(entry2.Value, "RetaliateValue", fileName, out var value4))
						{
							RetaliateValue = value4;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "RetaliateRange":
					{
						if (YMLShared.GetIntPropertyValue(entry2.Value, "RetaliateRange", fileName, out var value3))
						{
							RetaliateRange = value3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "SmallSlots":
					{
						if (YMLShared.GetIntPropertyValue(entry2.Value, "SmallSlots", fileName, out var value2))
						{
							SmallSlots = value2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "PreventDamage":
						ShieldValue = int.MaxValue;
						break;
					case "AddModifiers":
						if (entry2.Value is Sequence)
						{
							foreach (DataItem entry4 in (entry2.Value as Sequence).Entries)
							{
								if (entry4 is Sequence)
								{
									Sequence sequence2 = entry4 as Sequence;
									if (sequence2.Entries.Count == 2)
									{
										if (sequence2.Entries[0] is Scalar && sequence2.Entries[1] is Scalar)
										{
											string card2 = (sequence2.Entries[0] as Scalar).Text;
											if (int.TryParse((sequence2.Entries[1] as Scalar).Text, out var result3))
											{
												AttackModifierYMLData attackModifierYMLData2 = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == card2);
												if (attackModifierYMLData2 != null)
												{
													AdditionalModifiers.Add(attackModifierYMLData2, result3);
													continue;
												}
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid attack modifier card name '" + card2 + "' in file " + fileName);
												result = false;
											}
											else
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Number of cards must be an integer. File: " + fileName);
												result = false;
											}
										}
										else
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Sequence entries must be Scalar. File: " + fileName);
											result = false;
										}
									}
									else
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Sequence must be length 2. File: " + fileName);
										result = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Must be a sequence. File: " + fileName);
									result = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Must be a sequence. File: " + fileName);
							result = false;
						}
						break;
					case "RemoveModifiers":
						if (entry2.Value is Sequence)
						{
							foreach (DataItem entry5 in (entry2.Value as Sequence).Entries)
							{
								if (entry5 is Sequence)
								{
									Sequence sequence = entry5 as Sequence;
									if (sequence.Entries.Count == 2)
									{
										if (sequence.Entries[0] is Scalar && sequence.Entries[1] is Scalar)
										{
											string card = (sequence.Entries[0] as Scalar).Text;
											if (int.TryParse((sequence.Entries[1] as Scalar).Text, out var result2))
											{
												AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == card);
												if (attackModifierYMLData != null)
												{
													RemoveModifiers.Add(attackModifierYMLData, result2);
													continue;
												}
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid attack modifier card name '" + card + "' in file " + fileName);
												result = false;
											}
											else
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Number of cards must be an integer. File: " + fileName);
												result = false;
											}
										}
										else
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Sequence entries must be Scalar. File: " + fileName);
											result = false;
										}
									}
									else
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Sequence must be length 2. File: " + fileName);
										result = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Must be a sequence. File: " + fileName);
									result = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Must be a sequence. File: " + fileName);
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid key " + entry2.Key.ToString() + " in YML file " + fileName);
						result = false;
						break;
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Data entry.  File:\n" + fileName, ex);
		}
		return false;
	}
}
