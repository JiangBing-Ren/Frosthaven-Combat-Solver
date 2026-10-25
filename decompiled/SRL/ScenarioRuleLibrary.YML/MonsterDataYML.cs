using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonsterDataYML
{
	public List<MonsterThreatValuesEntry> MonsterThreatValues { get; private set; }

	public List<MonsterGroup> MonsterGroups { get; private set; }

	public List<MonsterFamily> MonsterFamilies { get; private set; }

	public bool IsLoaded { get; private set; }

	public string FileName { get; private set; }

	public MonsterDataYML()
	{
		MonsterThreatValues = new List<MonsterThreatValuesEntry>();
		MonsterGroups = new List<MonsterGroup>();
		MonsterFamilies = new List<MonsterFamily>();
		IsLoaded = false;
	}

	public bool Validate()
	{
		bool result = true;
		if (MonsterThreatValues.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterThreatValues were loaded " + FileName);
			result = false;
		}
		else if (MonsterGroups.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterGroups were loaded " + FileName);
			result = false;
		}
		else if (MonsterFamilies.Count <= 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No MonsterFamilies were loaded " + FileName);
			result = false;
		}
		return result;
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool result = true;
			FileName = fileName;
			IsLoaded = true;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Threats":
						if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping4))
							{
								foreach (MappingEntry entry2 in mapping4.Entries)
								{
									if (entry2 == null)
									{
										continue;
									}
									int value2 = 0;
									int value3 = 0;
									if (YMLShared.GetSequence(entry2.Value, entry2.Key.ToString(), FileName, out var sequence2))
									{
										if (sequence2.Entries.Count != 2)
										{
											SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid threat values entry for monster " + entry2.Key.ToString() + ", two values expected in sequence. File:\n" + FileName);
											result = false;
										}
										else
										{
											YMLShared.GetIntPropertyValue(sequence2.Entries[0], entry2.Key.ToString(), FileName, out value2);
											YMLShared.GetIntPropertyValue(sequence2.Entries[1], entry2.Key.ToString(), FileName, out value3);
										}
									}
									MonsterThreatValues.Add(new MonsterThreatValuesEntry(entry2.Key.ToString(), value2, value3));
								}
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					case "MonsterGroups":
						if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping5))
							{
								foreach (MappingEntry entry3 in mapping5.Entries)
								{
									if (entry3 != null)
									{
										List<MonsterSpawnRates> monsters = MonsterSpawnRates.Process(entry3, FileName);
										MonsterGroups.Add(new MonsterGroup(entry3.Key.ToString(), monsters));
									}
								}
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					case "MonsterFamilies":
						if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping))
							{
								foreach (MappingEntry entry4 in mapping.Entries)
								{
									if (entry4 == null)
									{
										continue;
									}
									Dictionary<string, List<string>> dictionary = null;
									List<MonsterSpawnRates> list = null;
									if (entry4.Value is Mapping)
									{
										if (YMLShared.GetMapping(entry4, fileName, out var mapping2))
										{
											foreach (MappingEntry entry5 in mapping2.Entries)
											{
												if (entry5 == null)
												{
													continue;
												}
												string text = entry5.Key.ToString();
												if (!(text == "Descriptions"))
												{
													if (text == "DefaultMonsterGroup")
													{
														list = MonsterSpawnRates.Process(entry5, FileName);
													}
													else
													{
														SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid MonsterFamilies entry " + entry4.Key.ToString() + " in File:\n" + FileName);
													}
													continue;
												}
												dictionary = new Dictionary<string, List<string>>();
												if (entry5.Value is Mapping)
												{
													if (YMLShared.GetMapping(entry5, fileName, out var mapping3))
													{
														foreach (MappingEntry entry6 in mapping3.Entries)
														{
															if (entry6 == null)
															{
																continue;
															}
															List<string> list2 = new List<string>();
															if (entry6.Value is Sequence)
															{
																if (YMLShared.GetSequence(entry6.Value, entry6.Key.ToString(), FileName, out var sequence))
																{
																	foreach (DataItem entry7 in sequence.Entries)
																	{
																		list2.Add((entry7 as Scalar).Text);
																	}
																}
															}
															else
															{
																YMLShared.GetStringPropertyValue(entry6.Value, entry6.Key.ToString(), FileName, out var value);
																list2.Add(value);
															}
															dictionary.Add(entry6.Key.ToString(), list2);
														}
													}
													else
													{
														result = false;
													}
												}
												else
												{
													result = false;
												}
											}
											if (dictionary == null || dictionary.Count == 0)
											{
												SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing/Invalid Descriptions entry in scenario file:\n" + fileName);
											}
											if (list == null || list.Count == 0)
											{
												SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing/Invalid DefaultMonsterGroup entry in scenario file:\n" + fileName);
											}
											MonsterFamilies.Add(new MonsterFamily(entry4.Key.ToString(), dictionary, list));
										}
										else
										{
											result = false;
										}
									}
									else
									{
										result = false;
									}
								}
							}
							else
							{
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid root level entry " + entry.Key.ToString() + " in File:\n" + FileName);
						break;
					case "Parser":
						break;
					}
				}
				return result;
			}
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to parse yml. File:\n" + FileName);
			foreach (string item in yamlParser.Errors.Select((Pair<int, string> x) => x.Right))
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, item);
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}
}
