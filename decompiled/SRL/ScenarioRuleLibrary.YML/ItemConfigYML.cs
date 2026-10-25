using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ItemConfigYML
{
	private SpinLock _lock;

	public ConcurrentBag<ItemConfigYMLData> LoadedYML { get; set; }

	public ItemConfigYML()
	{
		LoadedYML = new ConcurrentBag<ItemConfigYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			ItemConfigYMLData data = new ItemConfigYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ItemName":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ItemName", fileName, out var value3))
						{
							data.ItemName = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Icon":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Icon", fileName, out var value2))
						{
							data.Icon = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Background":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Background", fileName, out var value))
						{
							data.Background = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Audio":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Audio", fileName, out var audioString))
						{
							ItemConfigYMLData.EItemAudioToggle eItemAudioToggle = ItemConfigYMLData.ItemAudioToggles.SingleOrDefault((ItemConfigYMLData.EItemAudioToggle s) => s.ToString() == audioString);
							if (eItemAudioToggle != ItemConfigYMLData.EItemAudioToggle.None)
							{
								data.Audio = eItemAudioToggle;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Audio " + entry.Key.ToString() + " in file " + fileName);
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
				bool lockTaken = false;
				_lock.Enter(ref lockTaken);
				ItemConfigYMLData itemConfigYMLData = LoadedYML.FirstOrDefault((ItemConfigYMLData s) => s.ItemName == data.ItemName);
				if (itemConfigYMLData == null)
				{
					LoadedYML.Add(data);
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Cannot update existing ItemConfigYMLData: " + data.ItemName);
					itemConfigYMLData.UpdateData(data);
				}
				_lock.Exit();
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
