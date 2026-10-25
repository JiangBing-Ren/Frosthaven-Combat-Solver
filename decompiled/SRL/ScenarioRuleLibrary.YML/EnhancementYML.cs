using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class EnhancementYML
{
	public Dictionary<EEnhancement, int> EnhancementClassPersistentBaseCosts { get; private set; }

	public List<int> ClassPersistentLevelOfAbilityCardCost { get; private set; }

	public List<int> ClassPersistentNumberOfPreviousEnhancementsCost { get; private set; }

	public int ClassPersistentMultiTargetMultiplier { get; private set; }

	public float ClassPersistentSellPricePercentage { get; private set; }

	public bool ClassPersistentAllowSell { get; private set; }

	public Dictionary<EEnhancement, int> EnhancementCharacterPersistentBaseCosts { get; private set; }

	public List<int> CharacterPersistentLevelOfAbilityCardCost { get; private set; }

	public List<int> CharacterPersistentNumberOfPreviousEnhancementsCost { get; private set; }

	public int CharacterPersistentMultiTargetMultiplier { get; private set; }

	public float CharacterPersistentSellPricePercentage { get; private set; }

	public bool CharacterPersistentAllowSell { get; private set; }

	public bool IsLoaded { get; private set; }

	public string FileName { get; private set; }

	public EnhancementYML()
	{
		EnhancementClassPersistentBaseCosts = null;
		ClassPersistentLevelOfAbilityCardCost = null;
		ClassPersistentNumberOfPreviousEnhancementsCost = null;
		ClassPersistentMultiTargetMultiplier = int.MaxValue;
		ClassPersistentSellPricePercentage = float.MaxValue;
		ClassPersistentAllowSell = false;
		EnhancementCharacterPersistentBaseCosts = null;
		CharacterPersistentLevelOfAbilityCardCost = null;
		CharacterPersistentNumberOfPreviousEnhancementsCost = null;
		CharacterPersistentMultiTargetMultiplier = int.MaxValue;
		CharacterPersistentSellPricePercentage = float.MaxValue;
		CharacterPersistentAllowSell = false;
		IsLoaded = false;
	}

	public bool Validate()
	{
		bool result = true;
		if (EnhancementClassPersistentBaseCosts.Count <= 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Class Persistent Enhancements were loaded " + FileName);
			result = false;
		}
		else if (CEnhancement.Enhancements.Count() != EnhancementClassPersistentBaseCosts.Count)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing cost for required Class Persistent Enhancements " + string.Join("\n", from s in CEnhancement.Enhancements
				where !EnhancementClassPersistentBaseCosts.Keys.Contains(s)
				select s.ToString()) + " File: " + FileName);
			result = false;
		}
		else if (ClassPersistentLevelOfAbilityCardCost == null || ClassPersistentLevelOfAbilityCardCost.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ClassPersistentLevelOfAbilityCardCost was loaded " + FileName);
			result = false;
		}
		else if (ClassPersistentLevelOfAbilityCardCost.Count != 9)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "ClassPersistentLevelOfAbilityCardCost must have exactly 9 entires " + FileName);
			result = false;
		}
		else if (ClassPersistentNumberOfPreviousEnhancementsCost == null || ClassPersistentNumberOfPreviousEnhancementsCost.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ClassPersistentNumberOfPreviousEnhancementsCost was loaded " + FileName);
			result = false;
		}
		else if (ClassPersistentMultiTargetMultiplier == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ClassPersistentMultiTargetMultiplier was loaded " + FileName);
			result = false;
		}
		else if (ClassPersistentSellPricePercentage == float.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ClassPersistentSellPricePercentage was loaded " + FileName);
			result = false;
		}
		else if (EnhancementCharacterPersistentBaseCosts.Count <= 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Character Persistent Enhancements were loaded " + FileName);
			result = false;
		}
		else if (CEnhancement.Enhancements.Count() != EnhancementCharacterPersistentBaseCosts.Count)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Missing cost for required Character Persistent Enhancements " + string.Join("\n", from s in CEnhancement.Enhancements
				where !EnhancementClassPersistentBaseCosts.Keys.Contains(s)
				select s.ToString()) + " File: " + FileName);
			result = false;
		}
		else if (CharacterPersistentLevelOfAbilityCardCost == null || CharacterPersistentLevelOfAbilityCardCost.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CharacterPersistentLevelOfAbilityCardCost was loaded " + FileName);
			result = false;
		}
		else if (CharacterPersistentLevelOfAbilityCardCost.Count != 9)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "CharacterPersistentLevelOfAbilityCardCost must have exactly 9 entires " + FileName);
			result = false;
		}
		else if (CharacterPersistentNumberOfPreviousEnhancementsCost == null || CharacterPersistentNumberOfPreviousEnhancementsCost.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CharacterPersistentNumberOfPreviousEnhancementsCost was loaded " + FileName);
			result = false;
		}
		else if (CharacterPersistentMultiTargetMultiplier == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CharacterPersistentMultiTargetMultiplier was loaded " + FileName);
			result = false;
		}
		else if (CharacterPersistentSellPricePercentage == float.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CharacterPersistentSellPricePercentage was loaded " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(Dictionary<EEnhancement, int> enhancementClassPersistentBaseCosts, List<int> classPersistentLevelOfAbilityCardCost, List<int> classPersistentNumberOfPreviousEnhancementsCost, int classPersistentMultiTargetMultiplier, float classPersistentSellPricePercentage, bool classPersistentAllowSell, Dictionary<EEnhancement, int> enhancementCharacterPersistentBaseCosts, List<int> characterPersistentLevelOfAbilityCardCost, List<int> characterPersistentNumberOfPreviousEnhancementsCost, int characterPersistentMultiTargetMultiplier, float characterPersistentSellPricePercentage, bool characterPersistentAllowSell)
	{
		foreach (KeyValuePair<EEnhancement, int> enhancementClassPersistentBaseCost in enhancementClassPersistentBaseCosts)
		{
			if (EnhancementClassPersistentBaseCosts.ContainsKey(enhancementClassPersistentBaseCost.Key))
			{
				EnhancementClassPersistentBaseCosts[enhancementClassPersistentBaseCost.Key] = enhancementClassPersistentBaseCost.Value;
			}
			else
			{
				EnhancementClassPersistentBaseCosts.Add(enhancementClassPersistentBaseCost.Key, enhancementClassPersistentBaseCost.Value);
			}
		}
		if (classPersistentLevelOfAbilityCardCost != null)
		{
			ClassPersistentLevelOfAbilityCardCost = classPersistentLevelOfAbilityCardCost;
		}
		if (classPersistentNumberOfPreviousEnhancementsCost != null)
		{
			ClassPersistentNumberOfPreviousEnhancementsCost = classPersistentNumberOfPreviousEnhancementsCost;
		}
		if (classPersistentMultiTargetMultiplier != int.MaxValue)
		{
			ClassPersistentMultiTargetMultiplier = classPersistentMultiTargetMultiplier;
		}
		if (classPersistentSellPricePercentage != float.MaxValue)
		{
			ClassPersistentSellPricePercentage = classPersistentSellPricePercentage;
		}
		ClassPersistentAllowSell = classPersistentAllowSell;
		foreach (KeyValuePair<EEnhancement, int> enhancementCharacterPersistentBaseCost in enhancementCharacterPersistentBaseCosts)
		{
			if (EnhancementCharacterPersistentBaseCosts.ContainsKey(enhancementCharacterPersistentBaseCost.Key))
			{
				EnhancementCharacterPersistentBaseCosts[enhancementCharacterPersistentBaseCost.Key] = enhancementCharacterPersistentBaseCost.Value;
			}
			else
			{
				EnhancementCharacterPersistentBaseCosts.Add(enhancementCharacterPersistentBaseCost.Key, enhancementCharacterPersistentBaseCost.Value);
			}
		}
		if (characterPersistentLevelOfAbilityCardCost != null)
		{
			CharacterPersistentLevelOfAbilityCardCost = characterPersistentLevelOfAbilityCardCost;
		}
		if (characterPersistentNumberOfPreviousEnhancementsCost != null)
		{
			CharacterPersistentNumberOfPreviousEnhancementsCost = characterPersistentNumberOfPreviousEnhancementsCost;
		}
		if (characterPersistentMultiTargetMultiplier != int.MaxValue)
		{
			CharacterPersistentMultiTargetMultiplier = characterPersistentMultiTargetMultiplier;
		}
		if (characterPersistentSellPricePercentage != float.MaxValue)
		{
			CharacterPersistentSellPricePercentage = characterPersistentSellPricePercentage;
		}
		CharacterPersistentAllowSell = characterPersistentAllowSell;
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			FileName = fileName;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				Dictionary<EEnhancement, int> dictionary = new Dictionary<EEnhancement, int> { 
				{
					EEnhancement.NoEnhancement,
					0
				} };
				List<int> classPersistentLevelOfAbilityCardCost = null;
				List<int> classPersistentNumberOfPreviousEnhancementsCost = null;
				int classPersistentMultiTargetMultiplier = int.MaxValue;
				float classPersistentSellPricePercentage = float.MaxValue;
				bool classPersistentAllowSell = false;
				Dictionary<EEnhancement, int> dictionary2 = new Dictionary<EEnhancement, int> { 
				{
					EEnhancement.NoEnhancement,
					0
				} };
				List<int> characterPersistentLevelOfAbilityCardCost = null;
				List<int> characterPersistentNumberOfPreviousEnhancementsCost = null;
				int characterPersistentMultiTargetMultiplier = int.MaxValue;
				float characterPersistentSellPricePercentage = float.MaxValue;
				bool characterPersistentAllowSell = false;
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ClassPersistentEnhancements":
						if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping2))
							{
								foreach (MappingEntry enhanceEntry2 in mapping2.Entries)
								{
									EEnhancement eEnhancement2 = CEnhancement.Enhancements.SingleOrDefault((EEnhancement s) => s.ToString() == enhanceEntry2.Key.ToString());
									if (eEnhancement2 != EEnhancement.NoEnhancement && YMLShared.GetIntPropertyValue(enhanceEntry2.Value, "ClassPersistentEnhancements", fileName, out var value7))
									{
										dictionary.Add(eEnhancement2, value7);
										continue;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry in ClassPersistentEnhancements of layout file " + FileName);
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					case "ClassPersistentLevelOfAbilityCardCost":
					{
						if (YMLShared.GetIntList(entry.Value, "ClassPersistentLevelOfAbilityCardCost", fileName, out var values2))
						{
							classPersistentLevelOfAbilityCardCost = values2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ClassPersistentNumberOfPreviousEnhancementsCost":
					{
						if (YMLShared.GetIntList(entry.Value, "ClassPersistentNumberOfPreviousEnhancementsCost", fileName, out var values4))
						{
							classPersistentNumberOfPreviousEnhancementsCost = values4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ClassPersistentMultiTargetMultiplier":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "ClassPersistentMultiTargetMultiplier", fileName, out var value4))
						{
							classPersistentMultiTargetMultiplier = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ClassPersistentSellPricePercentage":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "ClassPersistentSellPricePercentage", fileName, out var value8))
						{
							classPersistentSellPricePercentage = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ClassPersistentAllowSell":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ClassPersistentAllowSell", fileName, out var value2))
						{
							classPersistentAllowSell = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterPersistentEnhancements":
						if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping))
							{
								foreach (MappingEntry enhanceEntry in mapping.Entries)
								{
									EEnhancement eEnhancement = CEnhancement.Enhancements.SingleOrDefault((EEnhancement s) => s.ToString() == enhanceEntry.Key.ToString());
									if (eEnhancement != EEnhancement.NoEnhancement && YMLShared.GetIntPropertyValue(enhanceEntry.Value, "CharacterPersistentEnhancements", fileName, out var value5))
									{
										dictionary2.Add(eEnhancement, value5);
										continue;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry in CharacterPersistentEnhancements of layout file " + FileName);
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					case "CharacterPersistentLevelOfAbilityCardCost":
					{
						if (YMLShared.GetIntList(entry.Value, "CharacterPersistentLevelOfAbilityCardCost", fileName, out var values))
						{
							characterPersistentLevelOfAbilityCardCost = values;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterPersistentNumberOfPreviousEnhancementsCost":
					{
						if (YMLShared.GetIntList(entry.Value, "CharacterPersistentNumberOfPreviousEnhancementsCost", fileName, out var values3))
						{
							characterPersistentNumberOfPreviousEnhancementsCost = values3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterPersistentMultiTargetMultiplier":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "CharacterPersistentMultiTargetMultiplier", fileName, out var value6))
						{
							characterPersistentMultiTargetMultiplier = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterPersistentSellPricePercentage":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "CharacterPersistentSellPricePercentage", fileName, out var value3))
						{
							characterPersistentSellPricePercentage = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterPersistentAllowSell":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "CharacterPersistentAllowSell", fileName, out var value))
						{
							characterPersistentAllowSell = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of Enhancements file " + FileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (flag)
				{
					if (!IsLoaded)
					{
						EnhancementClassPersistentBaseCosts = dictionary;
						ClassPersistentLevelOfAbilityCardCost = classPersistentLevelOfAbilityCardCost;
						ClassPersistentNumberOfPreviousEnhancementsCost = classPersistentNumberOfPreviousEnhancementsCost;
						ClassPersistentMultiTargetMultiplier = classPersistentMultiTargetMultiplier;
						ClassPersistentSellPricePercentage = classPersistentSellPricePercentage;
						ClassPersistentAllowSell = classPersistentAllowSell;
						EnhancementCharacterPersistentBaseCosts = dictionary2;
						CharacterPersistentLevelOfAbilityCardCost = characterPersistentLevelOfAbilityCardCost;
						CharacterPersistentNumberOfPreviousEnhancementsCost = characterPersistentNumberOfPreviousEnhancementsCost;
						CharacterPersistentMultiTargetMultiplier = characterPersistentMultiTargetMultiplier;
						CharacterPersistentSellPricePercentage = characterPersistentSellPricePercentage;
						CharacterPersistentAllowSell = characterPersistentAllowSell;
						IsLoaded = true;
					}
					else
					{
						UpdateData(dictionary, classPersistentLevelOfAbilityCardCost, classPersistentNumberOfPreviousEnhancementsCost, classPersistentMultiTargetMultiplier, classPersistentSellPricePercentage, classPersistentAllowSell, dictionary2, characterPersistentLevelOfAbilityCardCost, characterPersistentNumberOfPreviousEnhancementsCost, characterPersistentMultiTargetMultiplier, characterPersistentSellPricePercentage, characterPersistentAllowSell);
					}
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

	public Dictionary<EEnhancement, int> EnhancementBaseCosts(bool classPersistent = false)
	{
		if (classPersistent)
		{
			return EnhancementClassPersistentBaseCosts;
		}
		return EnhancementCharacterPersistentBaseCosts;
	}

	public List<int> LevelOfAbilityCardCost(bool classPersistent = false)
	{
		if (classPersistent)
		{
			return ClassPersistentLevelOfAbilityCardCost;
		}
		return CharacterPersistentLevelOfAbilityCardCost;
	}

	public List<int> NumberOfPreviousEnhancementsCost(bool classPersistent = false)
	{
		if (classPersistent)
		{
			return ClassPersistentNumberOfPreviousEnhancementsCost;
		}
		return CharacterPersistentNumberOfPreviousEnhancementsCost;
	}

	public int MultiTargetMultiplier(bool classPersistent = false)
	{
		if (classPersistent)
		{
			return ClassPersistentMultiTargetMultiplier;
		}
		return CharacterPersistentMultiTargetMultiplier;
	}

	public float SellPricePercentage(bool classPersistent = false)
	{
		if (classPersistent)
		{
			return ClassPersistentSellPricePercentage;
		}
		return CharacterPersistentSellPricePercentage;
	}

	public bool AllowSell(bool classPersistent = false)
	{
		if (classPersistent)
		{
			return ClassPersistentAllowSell;
		}
		return CharacterPersistentAllowSell;
	}
}
