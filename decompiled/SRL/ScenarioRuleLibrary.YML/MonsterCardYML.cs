using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonsterCardYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<MonsterCardYMLData> LoadedYML { get; private set; }

	public MonsterCardYML()
	{
		LoadedYML = new ConcurrentQueue<MonsterCardYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			MonsterCardYMLData monsterCardYMLData = new MonsterCardYMLData(fileName);
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
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ID", fileName, out var value2))
						{
							monsterCardYMLData.ID = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value4))
						{
							monsterCardYMLData.Name = CardProcessingShared.GetLookupValue(value4);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Initiative":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "Initiative", fileName, out var value3))
						{
							monsterCardYMLData.Initiative = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Shuffle":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Shuffle", fileName, out var value))
						{
							monsterCardYMLData.Shuffle = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Data":
						if (entry.Value is Mapping)
						{
							if (AbilityData.GetAction(entry.Value as Mapping, monsterCardYMLData.ID, fileName, DiscardType.Discard, out var action, out var _, isMonster: true))
							{
								monsterCardYMLData.CardAction = action;
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Child of Data entry is not a mapping. File:\n" + fileName + "\nDataItem:\n" + entry.Value.ToString());
							flag = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (monsterCardYMLData.ID == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					LoadedYML.Enqueue(monsterCardYMLData);
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
