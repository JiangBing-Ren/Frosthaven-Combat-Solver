using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AttackModifierDecksYML
{
	public const int MinimumFilesRequired = 2;

	public ConcurrentQueue<AttackModifierDeckYMLData> LoadedYML { get; private set; }

	public AttackModifierDecksYML()
	{
		LoadedYML = new ConcurrentQueue<AttackModifierDeckYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AttackModifierDeckYMLData attackModifierDeckYMLData = new AttackModifierDeckYMLData(fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value2))
						{
							attackModifierDeckYMLData.Name = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IsMonsterDeck":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsMonsterDeck", fileName, out var value))
						{
							attackModifierDeckYMLData.IsMonsterDeckNullable = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IsReducedRandomnessDeck":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsReducedRandomnessDeck", fileName, out var value3))
						{
							attackModifierDeckYMLData.IsReducedRandomnessDeckNullable = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AttackModifierCards":
						if (entry.Value is Sequence)
						{
							foreach (DataItem entry2 in (entry.Value as Sequence).Entries)
							{
								if (entry2 is Sequence)
								{
									Sequence sequence = entry2 as Sequence;
									if (sequence.Entries.Count == 2)
									{
										if (sequence.Entries[0] is Scalar && sequence.Entries[1] is Scalar)
										{
											string card = (sequence.Entries[0] as Scalar).Text;
											if (int.TryParse((sequence.Entries[1] as Scalar).Text, out var result))
											{
												AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == card);
												if (attackModifierYMLData != null)
												{
													if (attackModifierDeckYMLData.AttackModifierDeck == null)
													{
														attackModifierDeckYMLData.AttackModifierDeck = new Dictionary<AttackModifierYMLData, int>();
													}
													attackModifierDeckYMLData.AttackModifierDeck.Add(attackModifierYMLData, result);
												}
												else
												{
													SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid attack modifier card name '" + card + "' in file " + fileName);
													flag = false;
												}
											}
											else
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Number of cards must be an integer. File: " + fileName);
												flag = false;
											}
										}
										else
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Sequence entries must be Scalar. File: " + fileName);
											flag = false;
										}
									}
									else
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Sequence must be length 2. File: " + fileName);
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Must be a sequence. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry.  Must be a sequence. File: " + fileName);
							flag = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (attackModifierDeckYMLData.Name == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Name specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					LoadedYML.Enqueue(attackModifierDeckYMLData);
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
