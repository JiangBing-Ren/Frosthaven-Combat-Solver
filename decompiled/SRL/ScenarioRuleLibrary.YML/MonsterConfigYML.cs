using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonsterConfigYML
{
	public ConcurrentBag<MonsterConfigYMLData> LoadedYML { get; set; }

	public MonsterConfigYML()
	{
		LoadedYML = new ConcurrentBag<MonsterConfigYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			MonsterConfigYMLData monsterConfigYMLData = new MonsterConfigYMLData(fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value3))
						{
							monsterConfigYMLData.ID = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Avatar":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Avatar", fileName, out var value2))
						{
							monsterConfigYMLData.Avatar = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Portrait":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Portrait", fileName, out var value))
						{
							monsterConfigYMLData.Portrait = value;
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
					case "Parser":
						break;
					}
				}
			}
			if (flag)
			{
				LoadedYML.Add(monsterConfigYMLData);
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (SystemException ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}
}
