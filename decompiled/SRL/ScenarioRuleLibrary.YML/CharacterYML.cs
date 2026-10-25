using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class CharacterYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<CharacterYMLData> LoadedYML { get; private set; }

	public CharacterYML()
	{
		LoadedYML = new ConcurrentQueue<CharacterYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool success = true;
			CharacterYMLData characterYMLData = new CharacterYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success2);
			if (success2)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					switch (text)
					{
					case "StartScenarioAbilities":
					{
						if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out var abilities))
						{
							characterYMLData.StartScenarioAbilities.AddRange(abilities);
						}
						else
						{
							success = false;
						}
						break;
					}
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value21))
						{
							if (value21.Length <= 64)
							{
								characterYMLData.ID = value21;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Character ID exceeds max allowed length (64) in file " + fileName);
							success = false;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "LocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value20))
						{
							characterYMLData.LocKey = CardProcessingShared.GetLookupValue(value20);
						}
						else
						{
							success = false;
						}
						break;
					}
					case "OverrideAbilityCardArt":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value17))
						{
							characterYMLData.OverrideAbilityCardArt = value17;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Model":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var modelString))
						{
							ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == modelString);
							if (eCharacter != ECharacter.None)
							{
								characterYMLData.Model = eCharacter;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid model name " + modelString + " in file " + fileName);
							success = false;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "DefaultSkin":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, text, fileName, out var value15))
						{
							characterYMLData.DefaultSkin = value15;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "HealthTable":
					{
						if (YMLShared.GetIntArray(entry.Value, text, fileName, 10, out var outArray, ignorePositionZero: true))
						{
							characterYMLData.HealthTable = outArray;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "NumberAbilityCardsInBattle":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, text, fileName, out var value5))
						{
							characterYMLData.NumberAbilityCardsInBattle = value5;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "AttackModifierDeck":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var amdValue))
						{
							if (ScenarioRuleClient.SRLYML.AttackModifierDecks.SingleOrDefault((AttackModifierDeckYMLData a) => a.Name == amdValue) != null)
							{
								characterYMLData.AttackModifierDeckString = amdValue;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {amdValue} in {fileName}");
							success = false;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "CompanionSummonData":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var summonID))
						{
							if (summonID.ToLower() == "null")
							{
								characterYMLData.NullCompanionSummonID = true;
								break;
							}
							if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData h) => h.ID == summonID) != null)
							{
								characterYMLData.CompanionSummonID = summonID;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {summonID} in {fileName}");
							success = false;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value9))
						{
							characterYMLData.Description = value9;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Role":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value7))
						{
							characterYMLData.Role = value7;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Difficulty":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value2))
						{
							characterYMLData.Difficulty = value2;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Strengths":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value10))
						{
							characterYMLData.Strengths = value10;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Weaknesses":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value13))
						{
							characterYMLData.Weaknesses = value13;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Adventure_Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value12))
						{
							characterYMLData.Adventure_Description = value12;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Colour":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value18))
						{
							if (value18.Length == 3 || value18.Length == 4 || value18.Length == 6 || value18.Length == 8)
							{
								bool flag = true;
								string text2 = value18;
								foreach (char c in text2)
								{
									if ((c < 'A' || c > 'F') && (c < '0' || c > '9'))
									{
										flag = false;
										break;
									}
								}
								if (flag)
								{
									value18 = (characterYMLData.ColourHTML = "#" + value18);
									break;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Incorrect Colour format. Use only hexadecimal characters in file " + fileName);
								success = false;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Colour property has to be exactly 3, 4, 6 or 8 characters long in file " + fileName);
								success = false;
							}
						}
						else
						{
							success = false;
						}
						break;
					}
					case "ChunkModifier":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, text, fileName, out var value11))
						{
							if (value11 >= 0f && value11 <= 1f)
							{
								characterYMLData.Fatness = value11;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "ChunkModifier is out of range. It has to be a value between 0 and 1 in file " + fileName);
							success = false;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Vertex":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, text, fileName, out var value6))
						{
							if (value6 >= -0.5f && value6 <= 0.5f)
							{
								characterYMLData.VertexAnimIntensity = value6;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Vertex is out of range. It has to be a value between -0.5 and 0.5 in file " + fileName);
							success = false;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "CustomCharacterConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value4))
						{
							characterYMLData.CustomCharacterConfig = value4;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "TimeTokens":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, text, fileName, out var value))
						{
							characterYMLData.TimeTokens = value;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "TrophyTokens":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, text, fileName, out var value19))
						{
							characterYMLData.TrophyTokens = value19;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "ResonanceTokens":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, text, fileName, out var value16))
						{
							characterYMLData.ResonanceTokens = value16;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Race":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value14))
						{
							characterYMLData.Race = value14;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Traits":
					{
						if (YMLShared.GetStringList(entry.Value, text, fileName, out var values4))
						{
							characterYMLData.Traits = values4.ToArray();
						}
						else
						{
							success = false;
						}
						break;
					}
					case "OnlyValidInLevels":
					{
						if (YMLShared.GetStringList(entry.Value, text, fileName, out var values3))
						{
							characterYMLData.OnlyValidInLevels = values3;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "Complexity":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, text, fileName, out var value8))
						{
							if (VerifyHeroStatValue(value8, text, fileName, ref success, 1, 5))
							{
								characterYMLData.Complexity = value8;
							}
						}
						else
						{
							success = false;
						}
						break;
					}
					case "ElementalAffinities":
					{
						if (YMLShared.GetStringList(entry.Value, text, fileName, out var values2))
						{
							characterYMLData.ElementalAffinities = values2.ToArray();
						}
						else
						{
							success = false;
						}
						break;
					}
					case "MasteryIDs":
					{
						if (YMLShared.GetStringList(entry.Value, text, fileName, out var values))
						{
							characterYMLData.MasteryIDs = values.ToArray();
						}
						else
						{
							success = false;
						}
						break;
					}
					case "RetireCampaignSection":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, text, fileName, out var value3))
						{
							characterYMLData.RetireCampaignSection = value3;
						}
						else
						{
							success = false;
						}
						break;
					}
					case "HeroStats":
					{
						if (TryGetHeroStats(entry, fileName, out var heroStats))
						{
							characterYMLData.HeroStats = heroStats;
						}
						else
						{
							success = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						success = false;
						break;
					case "Parser":
						break;
					}
				}
				if (characterYMLData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					success = false;
				}
				if (success)
				{
					LoadedYML.Enqueue(characterYMLData);
				}
				return success;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}

	private bool TryGetHeroStats(MappingEntry statsEntry, string fileName, out HeroClassStats heroStats)
	{
		heroStats = new HeroClassStats();
		if (!YMLShared.GetMapping(statsEntry, fileName, out var mapping))
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Invalid Hero Stats entry {statsEntry.Key} in file {fileName}");
			return false;
		}
		bool success = true;
		foreach (MappingEntry entry2 in mapping.Entries)
		{
			MappingEntry entry = entry2;
			string entryName = entry.Key.ToString();
			switch (entryName)
			{
			case "Melee":
				setStatValue(ref heroStats.Melee);
				break;
			case "Ranged":
				setStatValue(ref heroStats.Ranged);
				break;
			case "Mobility":
				setStatValue(ref heroStats.Mobility);
				break;
			case "Support":
				setStatValue(ref heroStats.Support);
				break;
			case "Defense":
				setStatValue(ref heroStats.Defense);
				break;
			case "Control":
				setStatValue(ref heroStats.Control);
				break;
			}
			void setStatValue(ref int stat)
			{
				if (!YMLShared.GetIntPropertyValue(entry.Value, entryName, fileName, out var value))
				{
					success = false;
				}
				else if (VerifyHeroStatValue(value, entryName, fileName, ref success))
				{
					stat = value;
				}
			}
		}
		return success;
	}

	private static bool VerifyHeroStatValue(int value, string entryName, string fileName, ref bool success, int min = 0, int max = 6)
	{
		if (value >= min && value <= max)
		{
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, $"{entryName} is out of range. It has to be a value between {min} and {max} in file {fileName}");
		success = false;
		return false;
	}
}
