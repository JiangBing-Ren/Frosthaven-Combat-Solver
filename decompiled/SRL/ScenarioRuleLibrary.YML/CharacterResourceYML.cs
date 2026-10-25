using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class CharacterResourceYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<CharacterResourceData> LoadedYML { get; private set; }

	public CharacterResourceYML()
	{
		LoadedYML = new ConcurrentQueue<CharacterResourceData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			string value = string.Empty;
			bool value2 = false;
			int value3 = 0;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				if (yamlStream.Documents.Count > 0)
				{
					foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
					{
						if (entry.Key.ToString() == "Parser")
						{
							continue;
						}
						if (YMLShared.GetMapping(entry, fileName, out var mapping))
						{
							CharacterResourceData characterResourceData = new CharacterResourceData(fileName);
							foreach (MappingEntry entry2 in mapping.Entries)
							{
								switch (entry2.Key.ToString())
								{
								case "ID":
									if (YMLShared.GetStringPropertyValue(entry2.Value, "ID", fileName, out value))
									{
										characterResourceData.ID = value;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.ID '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "Sprite":
									if (YMLShared.GetStringPropertyValue(entry2.Value, "Sprite", fileName, out value))
									{
										characterResourceData.Sprite = value;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.Sprite '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "LocKey":
									if (YMLShared.GetStringPropertyValue(entry2.Value, "LocKey", fileName, out value))
									{
										characterResourceData.LocKey = value;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.LocKey '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "ResourceModel":
									if (YMLShared.GetStringPropertyValue(entry2.Value, "ResourceModel", fileName, out value))
									{
										characterResourceData.ResourceModel = value;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.ResourceModel '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "DropOnDeath":
									if (YMLShared.GetBoolPropertyValue(entry2.Value, "DropOnDeath", fileName, out value2))
									{
										characterResourceData.DropOnDeath = value2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.DropOnDeath '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "Hidden":
									if (YMLShared.GetBoolPropertyValue(entry2.Value, "Hidden", fileName, out value2))
									{
										characterResourceData.Hidden = value2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.Hidden '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "MaxAmount":
									if (YMLShared.GetIntPropertyValue(entry2.Value, "MaxAmount", fileName, out value3))
									{
										characterResourceData.MaxAmount = value3;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.MaxAmount '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "NumTimesCanBeLooted":
									if (YMLShared.GetIntPropertyValue(entry2.Value, "NumTimesCanBeLooted", fileName, out value3))
									{
										characterResourceData.NumTimesCanBeLooted = value3;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.NumTimesCanBeLooted '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "StayActiveAfterLooted":
									if (YMLShared.GetBoolPropertyValue(entry2.Value, "StayActiveAfterLooted", fileName, out value2))
									{
										characterResourceData.StayActiveAfterLooted = value2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData.StayActiveAfterLooted '" + value + "'.  File: " + fileName);
									flag = false;
									break;
								case "CanLootFilter":
								{
									if (ScenarioYML.GetObjectiveFilter(entry2, fileName, out var filter))
									{
										characterResourceData.CanLootFilter = filter;
									}
									else
									{
										flag = false;
									}
									break;
								}
								default:
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterResourceData entry '" + entry.Key?.ToString() + "'. File:\n" + fileName);
									break;
								}
							}
							if (flag)
							{
								LoadedYML.Enqueue(characterResourceData);
							}
						}
						else
						{
							flag = false;
						}
					}
					return flag;
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
