using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AbilityCardYML
{
	public const int LongRestID = -1;

	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<AbilityCardYMLData> LoadedYML { get; private set; }

	public AbilityCardYML()
	{
		LoadedYML = new ConcurrentQueue<AbilityCardYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AbilityCardYMLData abilityCardYMLData = new AbilityCardYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value5))
						{
							abilityCardYMLData.Name = CardProcessingShared.GetLookupValue(value5);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Character":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Character", fileName, out var characterID))
						{
							if (characterID == "All")
							{
								abilityCardYMLData.CharacterID = characterID;
								break;
							}
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == characterID) != null)
							{
								abilityCardYMLData.CharacterID = characterID;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {characterID} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ID":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ID", fileName, out var value10))
						{
							abilityCardYMLData.ID = value10;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Level":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Level", fileName, out var value8))
						{
							abilityCardYMLData.Level = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "SupplyCard":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "SupplyCard", fileName, out var value3))
						{
							abilityCardYMLData.SupplyCard = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Initiative":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "Initiative", fileName, out var value7))
						{
							abilityCardYMLData.Initiative = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ProgressionOrder":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ProgressionOrder", fileName, out var value9))
						{
							abilityCardYMLData.ProgressionOrder = value9;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Discard":
					{
						if (YMLShared.GetSequence(entry.Value, "Discard", fileName, out var sequence))
						{
							string text = (sequence.Entries[0] as Scalar).Text;
							string text2 = (sequence.Entries[1] as Scalar).Text;
							switch (text)
							{
							case "Discard":
								abilityCardYMLData.TopDiscardType = DiscardType.Discard;
								break;
							case "Lost":
								abilityCardYMLData.TopDiscardType = DiscardType.Lost;
								break;
							case "Permanently Lost":
								abilityCardYMLData.TopDiscardType = DiscardType.PermanentlyLost;
								break;
							default:
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse Top Discard property in file " + fileName);
								break;
							}
							switch (text2)
							{
							case "Discard":
								abilityCardYMLData.BottomDiscardType = DiscardType.Discard;
								break;
							case "Lost":
								abilityCardYMLData.BottomDiscardType = DiscardType.Lost;
								break;
							case "Permanently Lost":
								abilityCardYMLData.BottomDiscardType = DiscardType.PermanentlyLost;
								break;
							default:
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse Bottom Discard property in file " + fileName);
								break;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Prefab":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Prefab", fileName, out var value2))
						{
							abilityCardYMLData.Prefab = value2;
						}
						break;
					}
					case "CustomImage":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomImage", fileName, out var value4))
						{
							abilityCardYMLData.CustomImage = value4;
						}
						break;
					}
					case "MiniCardImage":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MiniCardImage", fileName, out var value6))
						{
							abilityCardYMLData.MiniCardImage = value6;
						}
						break;
					}
					case "Data":
					{
						if (YMLShared.GetMapping(entry, fileName, out var mapping))
						{
							MappingEntry mappingEntry = mapping.Entries.SingleOrDefault((MappingEntry x) => x.Key.ToString() == "Top");
							if (mappingEntry != null)
							{
								if (ProcessActionData(mappingEntry, abilityCardYMLData.ID, abilityCardYMLData.TopDiscardType, fileName, out var action, out var consumes))
								{
									abilityCardYMLData.TopActionCardData = action;
									abilityCardYMLData.TopConsumes.AddRange(consumes);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Top Action Data in file " + fileName);
								flag = false;
							}
							MappingEntry mappingEntry2 = mapping.Entries.SingleOrDefault((MappingEntry x) => x.Key.ToString() == "Bottom");
							if (mappingEntry2 != null)
							{
								if (ProcessActionData(mappingEntry2, abilityCardYMLData.ID, abilityCardYMLData.BottomDiscardType, fileName, out var action2, out var consumes2))
								{
									abilityCardYMLData.BottomActionCardData = action2;
									abilityCardYMLData.BottomConsumes.AddRange(consumes2);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Top Bottom Data in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Layout":
					{
						Mapping mapping2;
						if (abilityCardYMLData.ID == -1 && entry.Value is Sequence)
						{
							abilityCardYMLData.TopActionFullLayout = new CardLayout(entry.Value as Sequence, null, null, abilityCardYMLData.Name, DiscardType.None, fileName);
						}
						else if (YMLShared.GetMapping(entry, fileName, out mapping2))
						{
							abilityCardYMLData.TopActionFullLayout = CardProcessingShared.ProcessAbilityLayout(entry, "Top", abilityCardYMLData.TopActionCardData?.Abilities?.Concat(abilityCardYMLData?.TopActionCardData?.ModeAbilities).ToList(), abilityCardYMLData.Name, abilityCardYMLData.TopDiscardType, fileName, abilityCardYMLData.TopConsumes);
							abilityCardYMLData.BottomActionFullLayout = CardProcessingShared.ProcessAbilityLayout(entry, "Bottom", abilityCardYMLData.BottomActionCardData?.Abilities?.Concat(abilityCardYMLData?.BottomActionCardData?.ModeAbilities).ToList(), abilityCardYMLData.Name, abilityCardYMLData.BottomDiscardType, fileName, abilityCardYMLData.BottomConsumes);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RequiredForm":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "RequiredForm", fileName, out var value))
						{
							abilityCardYMLData.RequiredForm = Enum.Parse<CPlayerActor.EForm>(value);
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
				if (abilityCardYMLData.ID == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					LoadedYML.Enqueue(abilityCardYMLData);
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

	private bool ProcessActionData(MappingEntry mappingEntry, int cardID, DiscardType discardType, string fileName, out CAction action, out List<AbilityConsume> consumes)
	{
		consumes = null;
		action = null;
		if (mappingEntry.Value is Mapping)
		{
			if (AbilityData.GetAction(mappingEntry.Value as Mapping, cardID, fileName, discardType, out var action2, out consumes))
			{
				action = action2;
				return true;
			}
			return false;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Child of Data entry is not a mapping. File:\n" + fileName + "\nDataItem:\n" + mappingEntry.Value.ToString());
		return false;
	}
}
