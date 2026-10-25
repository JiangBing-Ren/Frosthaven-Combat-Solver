using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class PerksYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<PerksYMLData> LoadedYML { get; private set; }

	public PerksYML()
	{
		LoadedYML = new ConcurrentQueue<PerksYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			PerksYMLData perksYMLData = new PerksYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value2))
						{
							perksYMLData.ID = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value5))
						{
							perksYMLData.Name = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Description", fileName, out var value))
						{
							perksYMLData.Description = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CharacterID", fileName, out var charID))
						{
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == charID) != null)
							{
								perksYMLData.CharacterID = charID;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {charID} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Available":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Available", fileName, out var value4))
						{
							perksYMLData.Available = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Cost":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Cost", fileName, out var value3))
						{
							perksYMLData.Cost = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CardsToAdd":
					{
						if (YMLShared.GetStringList(entry.Value, "CardsToAdd", fileName, out var values3))
						{
							foreach (string card2 in values3)
							{
								if (ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData a) => a.Name == card2) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {card2} in {fileName}");
									flag = false;
								}
							}
							if (flag)
							{
								perksYMLData.CardsToAddStrings = values3;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CardsToRemove":
					{
						if (YMLShared.GetStringList(entry.Value, "CardsToRemove", fileName, out var values))
						{
							foreach (string card in values)
							{
								if (ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData a) => a.Name == card) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {card} in {fileName}");
									flag = false;
								}
							}
							if (flag)
							{
								perksYMLData.CardsToRemoveStrings = values;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Flags":
					{
						if (YMLShared.GetStringList(entry.Value, "Flags", fileName, out var values2))
						{
							EPerkFlag ePerkFlag = EPerkFlag.None;
							foreach (string item in values2)
							{
								if (Enum.TryParse<EPerkFlag>(item, out var result))
								{
									ePerkFlag |= result;
								}
							}
							perksYMLData.Flags = ePerkFlag;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Abilities":
					{
						if (CardProcessingShared.GetAbilities(entry, perksYMLData.Name.GetHashCode(), isMonster: false, fileName, out var abilities))
						{
							perksYMLData.Abilities = abilities;
							foreach (CAbility item2 in abilities)
							{
								item2.PerkID = perksYMLData.ID;
								if (item2 is CAbilityAddActiveBonus { AddAbility: not null } cAbilityAddActiveBonus)
								{
									cAbilityAddActiveBonus.AddAbility.PerkID = perksYMLData.ID;
								}
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Overrides":
					{
						perksYMLData.Overrides = new List<CAbilityOverride>();
						if (YMLShared.GetMapping(entry, fileName, out var mapping))
						{
							foreach (MappingEntry entry2 in mapping.Entries)
							{
								CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry2, 0, isMonster: false, fileName);
								if (cAbilityOverride != null)
								{
									perksYMLData.Overrides.Add(cAbilityOverride);
								}
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CompareAbility":
					{
						if (CAbilityCompare.GetAbilityCompare(entry, fileName, out var compareAbility))
						{
							perksYMLData.CompareAbility = compareAbility;
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
					case "Parser":
						break;
					}
				}
				if (perksYMLData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					LoadedYML.Enqueue(perksYMLData);
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
}
