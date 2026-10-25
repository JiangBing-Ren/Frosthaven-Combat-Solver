using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MapConfigYML
{
	public ConcurrentBag<MapConfigYMLData> LoadedYML { get; set; }

	public MapConfigYML()
	{
		LoadedYML = new ConcurrentBag<MapConfigYMLData>();
	}

	private bool IsTGA(string file)
	{
		if (file.EndsWith(".tga"))
		{
			return true;
		}
		return false;
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			MapConfigYMLData mapConfigYMLData = new MapConfigYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "NorthWest":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NorthWest", fileName, out var value3))
						{
							if (IsTGA(value3))
							{
								mapConfigYMLData.NorthWest = value3;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .tga " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "NorthEast":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NorthEast", fileName, out var value4))
						{
							if (IsTGA(value4))
							{
								mapConfigYMLData.NorthEast = value4;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .tga " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "SouthWest":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "SouthWest", fileName, out var value2))
						{
							if (IsTGA(value2))
							{
								mapConfigYMLData.SouthWest = value2;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .tga " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "SouthEast":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "SouthEast", fileName, out var value))
						{
							if (IsTGA(value))
							{
								mapConfigYMLData.SouthEast = value;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Has to be .tga " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
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
				LoadedYML.Add(mapConfigYMLData);
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
