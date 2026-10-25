using System;
using System.Collections.Generic;
using System.IO;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class RemoveYML
{
	public List<RemoveYMLData> LoadedYML { get; private set; }

	public RemoveYML()
	{
		LoadedYML = new List<RemoveYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			RemoveYMLData removeYMLData = new RemoveYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					if (text == "Parser")
					{
						continue;
					}
					if (text == "FilesToRemove")
					{
						if (YMLShared.GetStringList(entry.Value, "RewardTreasureTables", fileName, out var values))
						{
							foreach (string item in values)
							{
								if (Path.GetExtension(item) != ".yml")
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Can only remove .yml files in " + fileName);
									flag = false;
								}
							}
							removeYMLData.FilesToRemove = values;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
					}
				}
				if (flag)
				{
					LoadedYML.Add(removeYMLData);
				}
				return flag;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}
}
