using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class TreasureTablesYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<TreasureTable> LoadedYML { get; private set; }

	public TreasureTablesYML()
	{
		LoadedYML = new ConcurrentQueue<TreasureTable>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					string name = entry.Key.ToString();
					string giveToCharacterID = null;
					List<TreasureTableEntry> entries = new List<TreasureTableEntry>();
					List<TreasureTableGroup> list = new List<TreasureTableGroup>();
					new Dictionary<string, int>();
					if (YMLShared.GetMapping(entry, fileName, out var mapping))
					{
						foreach (MappingEntry entry2 in mapping.Entries)
						{
							if (entry2 == null)
							{
								continue;
							}
							switch (entry2.Key.ToString())
							{
							case "Description":
							{
								if (!YMLShared.GetStringPropertyValue(entry2.Value, "Description", fileName, out var _))
								{
									flag = false;
								}
								continue;
							}
							case "RemoveTable":
							{
								if (!YMLShared.GetStringList(entry2.Value, "RemoveTable", fileName, out var _))
								{
									flag = false;
								}
								continue;
							}
							case "GiveToCharacter":
							{
								if (YMLShared.GetStringPropertyValue(entry2.Value, "GiveToCharacter", fileName, out var characterValue))
								{
									if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == characterValue) != null)
									{
										giveToCharacterID = characterValue;
										continue;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {characterValue} in {fileName}");
									flag = false;
								}
								else
								{
									flag = false;
								}
								continue;
							}
							case "Group":
							{
								if (ProcessGroupEntry(entry2, fileName, out var treasureTableGroup))
								{
									list.Add(treasureTableGroup);
								}
								else
								{
									flag = false;
								}
								continue;
							}
							case "Items":
							case "ItemStocks":
							case "UnlockProsperityItems":
							case "UnlockProsperityItemStocks":
								if (!AddItemEntry(entry2, ref entries, fileName))
								{
									flag = false;
								}
								continue;
							case "Enhancements":
							{
								if (!(entry2.Value is Sequence))
								{
									continue;
								}
								if (YMLShared.GetStringList(entry2.Value, "Enhancements", fileName, out var values2))
								{
									foreach (string enhancementName in values2)
									{
										EEnhancement eEnhancement = CEnhancement.Enhancements.SingleOrDefault((EEnhancement s) => s.ToString() == enhancementName);
										if (eEnhancement != EEnhancement.NoEnhancement)
										{
											EEnhancement? enhancement = eEnhancement;
											TreasureTableEntry treasureTableEntry = new TreasureTableEntry(ETreasureType.Enhancement, null, null, null, -1, null, null, null, enhancement);
											if (treasureTableEntry != null)
											{
												entries.Add(treasureTableEntry);
											}
										}
										else
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Treasure Enhancement not found: " + enhancementName + ".  File: " + fileName);
											flag = false;
										}
									}
								}
								else
								{
									flag = false;
								}
								continue;
							}
							case "Parser":
								continue;
							}
							string key = entry2.Key.ToString();
							ETreasureType eTreasureType = Reward.TreasureTypes.SingleOrDefault((ETreasureType x) => x.ToString() == key);
							TreasureTableEntry treasureTableEntry2 = null;
							if (eTreasureType != ETreasureType.None || key == "Nothing")
							{
								if (ProcessEntry(entry2, eTreasureType, fileName, out var treasureTableEntry3))
								{
									treasureTableEntry2 = treasureTableEntry3;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								CItem item = GetItem(key, fileName);
								if (item != null)
								{
									if (ProcessEntry(entry2, ETreasureType.Item, fileName, out var treasureTableEntry4, item))
									{
										treasureTableEntry2 = treasureTableEntry4;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "TreasureType not found: " + key + ".  File: " + fileName);
									flag = false;
								}
							}
							if (treasureTableEntry2 != null)
							{
								entries.Add(treasureTableEntry2);
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					if (flag)
					{
						LoadedYML.Enqueue(new TreasureTable(name, giveToCharacterID, entries, list, fileName));
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

	private bool ProcessEntry(MappingEntry entryMap, ETreasureType type, string fileName, out TreasureTableEntry treasureTableEntry, CItem item = null)
	{
		treasureTableEntry = null;
		bool flag = true;
		try
		{
			List<int> chance = null;
			List<int> list = null;
			List<int> maxAmount = null;
			RewardCondition condition = null;
			RewardCondition enemyCondition = null;
			string conditionName = string.Empty;
			string enemyConditionName = string.Empty;
			RewardCondition.EConditionMapDuration eConditionMapDuration = RewardCondition.EConditionMapDuration.NextScenario;
			int roundDuration = 1;
			ElementInfusionBoardManager.EElement? infuse = null;
			EEnhancement? enhancement = null;
			PerksYMLData perksYMLData = null;
			string unlockAchievement = null;
			string unlockCharacterID = null;
			string unlockLocation = null;
			string unlockQuest = null;
			string cityEvent = null;
			string roadEvent = null;
			string consumeSlot = null;
			ETreasureDistributionType treasureDistributionType = ETreasureDistributionType.Combined;
			List<int> chapter = null;
			int chapterFilter = int.MaxValue;
			int subChapterFilter = int.MaxValue;
			string giveToCharacterID = null;
			Dictionary<string, int> dictionary = null;
			ETreasureDistributionRestrictionType distributionRestrictionType = ETreasureDistributionRestrictionType.None;
			if (type == ETreasureType.Condition)
			{
				treasureDistributionType = ETreasureDistributionType.None;
			}
			string nameValue;
			if (entryMap.Value is Mapping)
			{
				if (YMLShared.GetMapping(entryMap, fileName, out var mapping))
				{
					foreach (MappingEntry entry in mapping.Entries)
					{
						switch (entry.Key.ToString())
						{
						case "Name":
							if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out nameValue))
							{
								switch (type)
								{
								case ETreasureType.Item:
								case ETreasureType.ItemStock:
								case ETreasureType.UnlockProsperityItem:
								case ETreasureType.UnlockProsperityItemStock:
								case ETreasureType.LoseItem:
								case ETreasureType.ItemBlueprint:
									if (ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData i) => i.StringID == nameValue) != null)
									{
										item = GetItem(nameValue, fileName);
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {nameValue} in {fileName}");
									flag = false;
									break;
								case ETreasureType.Condition:
									conditionName = nameValue;
									break;
								case ETreasureType.EnemyCondition:
									enemyConditionName = nameValue;
									break;
								case ETreasureType.Infuse:
									try
									{
										infuse = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == nameValue);
									}
									catch (Exception ex)
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Element " + nameValue + ".  File:\n" + fileName, ex);
									}
									break;
								case ETreasureType.Enhancement:
									enhancement = CEnhancement.Enhancements.SingleOrDefault((EEnhancement s) => s.ToString() == nameValue);
									break;
								case ETreasureType.Perk:
									perksYMLData = ScenarioRuleClient.SRLYML.Perks.SingleOrDefault((PerksYMLData s) => s.Name == nameValue);
									if (perksYMLData == null)
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {nameValue} in {fileName}");
										flag = false;
									}
									break;
								case ETreasureType.UnlockCharacter:
									if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == nameValue) != null)
									{
										unlockCharacterID = nameValue;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {nameValue} in {fileName}");
									flag = false;
									break;
								case ETreasureType.UnlockLocation:
									unlockLocation = nameValue;
									break;
								case ETreasureType.UnlockQuest:
									unlockQuest = nameValue;
									break;
								case ETreasureType.CityEvent:
									cityEvent = nameValue;
									break;
								case ETreasureType.RoadEvent:
									roadEvent = nameValue;
									break;
								default:
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Name entry not supported for TreasureType " + type.ToString() + ". File: " + fileName);
									flag = false;
									break;
								}
							}
							else
							{
								flag = false;
							}
							break;
						case "Slot":
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "Slot", fileName, out var slotString))
							{
								if (CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotString) != CItem.EItemSlot.None)
								{
									consumeSlot = slotString;
									break;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Slot " + slotString + " in file " + fileName);
								flag = false;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "Chance":
						{
							if (YMLShared.GetIntList(entry.Value, "Chance", fileName, out var values, allowScalar: true, 8))
							{
								chance = values;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "MinAmount":
						{
							if (YMLShared.GetIntList(entry.Value, "MinAmount", fileName, out var values2, allowScalar: true, 8))
							{
								list = values2;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "MaxAmount":
						{
							if (YMLShared.GetIntList(entry.Value, "MaxAmount", fileName, out var values4, allowScalar: true, 8))
							{
								maxAmount = values4;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "Amount":
						{
							if (YMLShared.GetIntList(entry.Value, "Amount", fileName, out var values3, allowScalar: true, 8))
							{
								list = values3;
								maxAmount = list;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "Duration":
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "Duration", fileName, out var conditionDurationValue))
							{
								RewardCondition.EConditionMapDuration eConditionMapDuration2 = RewardCondition.ConditionDurations.SingleOrDefault((RewardCondition.EConditionMapDuration x) => x.ToString() == conditionDurationValue);
								if (eConditionMapDuration2 != RewardCondition.EConditionMapDuration.None)
								{
									eConditionMapDuration = eConditionMapDuration2;
									break;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find condition duration " + conditionDurationValue + ". File: " + fileName);
								flag = false;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "RoundDuration":
						{
							if (YMLShared.GetIntPropertyValue(entry.Value, "RoundDuration", fileName, out var value2))
							{
								roundDuration = value2;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "TreasureDistributionType":
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "TreasureDistributionType", fileName, out var treasureDistributionValue))
							{
								ETreasureDistributionType eTreasureDistributionType = Reward.TreasureDistributionTypes.SingleOrDefault((ETreasureDistributionType x) => x.ToString() == treasureDistributionValue);
								if (eTreasureDistributionType != ETreasureDistributionType.None)
								{
									treasureDistributionType = eTreasureDistributionType;
									break;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find treasure distribution " + treasureDistributionValue + ". File: " + fileName);
								flag = false;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "ChapterFilter":
						{
							if (YMLShared.GetIntPropertyValue(entry.Value, "ChapterFilter", fileName, out var value3))
							{
								chapterFilter = value3;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "SubChapterFilter":
						{
							if (YMLShared.GetIntPropertyValue(entry.Value, "SubChapterFilter", fileName, out var value))
							{
								subChapterFilter = value;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "GiveForCharacter":
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "GiveForCharacter", fileName, out var characterValue))
							{
								if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == characterValue) != null)
								{
									giveToCharacterID = characterValue;
									break;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {characterValue} in {fileName}");
								flag = false;
							}
							else
							{
								flag = false;
							}
							break;
						}
						case "Modifiers":
							if (entry.Value is Sequence)
							{
								foreach (DataItem entry2 in (entry.Value as Sequence).Entries)
								{
									if (entry2 is Sequence)
									{
										dictionary = new Dictionary<string, int>();
										Sequence sequence = entry2 as Sequence;
										if (sequence.Entries.Count == 2)
										{
											if (sequence.Entries[0] is Scalar && sequence.Entries[1] is Scalar)
											{
												string card = (sequence.Entries[0] as Scalar).Text;
												if (int.TryParse((sequence.Entries[1] as Scalar).Text, out var result))
												{
													if (ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == card) != null)
													{
														dictionary.Add(card, result);
														continue;
													}
													SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid attack modifier card name '" + card + "' in file " + fileName);
													flag = false;
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
						case "TreasureDistributionRestrictionType":
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "TreasureDistributionRestrictionType", fileName, out var distributionRestrictionValue))
							{
								ETreasureDistributionRestrictionType eTreasureDistributionRestrictionType = Reward.TreasureDistributionRestrictionTypes.SingleOrDefault((ETreasureDistributionRestrictionType x) => x.ToString() == distributionRestrictionValue);
								if (eTreasureDistributionRestrictionType != ETreasureDistributionRestrictionType.None)
								{
									distributionRestrictionType = eTreasureDistributionRestrictionType;
									break;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find treasure restriction distribution " + distributionRestrictionValue + ". File: " + fileName);
								flag = false;
							}
							else
							{
								flag = false;
							}
							break;
						}
						default:
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Treasure Table entry property " + entry.Key?.ToString() + " not found. File: " + fileName);
							flag = false;
							break;
						}
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Treasure Table Mapping entry " + entryMap.Key?.ToString() + " is not Mapping. File: " + fileName);
					flag = false;
				}
			}
			else if (entryMap.Value is Scalar)
			{
				if (type == ETreasureType.Gold || type == ETreasureType.XP || type == ETreasureType.Damage || type == ETreasureType.Prosperity || type == ETreasureType.EnhancementSlots || type == ETreasureType.PerkPoint || type == ETreasureType.Reputation || type == ETreasureType.PerkCheck || type == ETreasureType.Discard)
				{
					if (YMLShared.GetIntList(entryMap.Value, "MinAmount", fileName, out var values5, allowScalar: true, 8))
					{
						list = values5;
						maxAmount = values5;
					}
					else
					{
						flag = false;
					}
				}
				else if (type == ETreasureType.None || ((type == ETreasureType.Item || type == ETreasureType.ItemStock || type == ETreasureType.ItemBlueprint || type == ETreasureType.UnlockProsperityItem || type == ETreasureType.UnlockProsperityItemStock) && item != null))
				{
					if (YMLShared.GetIntList(entryMap.Value, "Chance", fileName, out var values6, allowScalar: true, 8))
					{
						chance = values6;
					}
					else
					{
						flag = false;
					}
				}
				else if (YMLShared.GetStringPropertyValue(entryMap.Value, entryMap.Key.ToString(), fileName, out nameValue))
				{
					switch (type)
					{
					case ETreasureType.Item:
					case ETreasureType.ItemStock:
					case ETreasureType.UnlockProsperityItem:
					case ETreasureType.UnlockProsperityItemStock:
					case ETreasureType.LoseItem:
					case ETreasureType.ItemBlueprint:
						if (ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData i) => i.StringID == nameValue) != null)
						{
							item = GetItem(nameValue, fileName);
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {nameValue} in {fileName}");
						flag = false;
						break;
					case ETreasureType.Condition:
						conditionName = nameValue;
						eConditionMapDuration = RewardCondition.EConditionMapDuration.NextScenario;
						break;
					case ETreasureType.EnemyCondition:
						enemyConditionName = nameValue;
						break;
					case ETreasureType.Infuse:
						try
						{
							infuse = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == nameValue);
						}
						catch (Exception ex2)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Element " + nameValue + ".  File:\n" + fileName, ex2);
							flag = false;
						}
						break;
					case ETreasureType.Enhancement:
						enhancement = CEnhancement.Enhancements.SingleOrDefault((EEnhancement s) => s.ToString() == nameValue);
						break;
					case ETreasureType.Perk:
					{
						PerksYMLData perksYMLData2 = ScenarioRuleClient.SRLYML.Perks.SingleOrDefault((PerksYMLData s) => s.Name == nameValue);
						if (perksYMLData != null)
						{
							perksYMLData = perksYMLData2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Perk " + nameValue + ".  File:\n" + fileName);
						flag = false;
						break;
					}
					case ETreasureType.UnlockCharacter:
						if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == nameValue) != null)
						{
							unlockCharacterID = nameValue;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {nameValue} in {fileName}");
						flag = false;
						break;
					case ETreasureType.UnlockLocation:
						unlockLocation = nameValue;
						break;
					case ETreasureType.UnlockQuest:
						unlockQuest = nameValue;
						break;
					case ETreasureType.CityEvent:
						cityEvent = nameValue;
						break;
					case ETreasureType.RoadEvent:
						roadEvent = nameValue;
						break;
					case ETreasureType.ConsumeItem:
						consumeSlot = nameValue;
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Scalar form Name property entry not supported for TreasureType " + type.ToString() + ". File: " + fileName);
						flag = false;
						break;
					case ETreasureType.UnlockMerchant:
					case ETreasureType.UnlockEnhancer:
					case ETreasureType.UnlockTrainer:
					case ETreasureType.UnlockTemple:
					case ETreasureType.UnlockPartyUI:
					case ETreasureType.UnlockMultiplayer:
					case ETreasureType.ItemDesign:
						break;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Scalar entry not supported for TreasureType " + type.ToString() + ". File: " + fileName);
					flag = false;
				}
			}
			else if (entryMap.Value is Sequence)
			{
				if (type == ETreasureType.Gold || type == ETreasureType.XP || type == ETreasureType.Damage || type == ETreasureType.Prosperity || type == ETreasureType.EnhancementSlots || type == ETreasureType.PerkPoint || type == ETreasureType.Reputation || type == ETreasureType.PerkCheck || type == ETreasureType.Discard)
				{
					if (YMLShared.GetIntList(entryMap.Value, "MinAmount", fileName, out var values7))
					{
						list = values7;
						maxAmount = values7;
					}
					else
					{
						flag = false;
					}
				}
				else if (type == ETreasureType.None || ((type == ETreasureType.Item || type == ETreasureType.ItemStock || type == ETreasureType.ItemBlueprint || type == ETreasureType.UnlockProsperityItem || type == ETreasureType.UnlockProsperityItemStock) && item != null))
				{
					if (YMLShared.GetIntList(entryMap.Value, "Chance", fileName, out var values8))
					{
						chance = values8;
					}
					else
					{
						flag = false;
					}
				}
				else if (type == ETreasureType.UnlockChapter)
				{
					if (YMLShared.GetIntList(entryMap.Value, "Chapter", fileName, out var values9, allowScalar: false))
					{
						chapter = values9;
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry type " + type.ToString() + ".  Can't specify value as a sequence for this type (or item not found).  File: " + fileName);
					flag = false;
				}
			}
			if (type == ETreasureType.Item && item == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Treasure table entry was of Item type but no item was found.  File: " + fileName);
				flag = false;
			}
			if (type == ETreasureType.None)
			{
				type = ETreasureType.Gold;
				list = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
				maxAmount = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
			}
			if (eConditionMapDuration != RewardCondition.EConditionMapDuration.None && conditionName != string.Empty)
			{
				CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == conditionName);
				if (eNegativeCondition != CCondition.ENegativeCondition.NA)
				{
					condition = new RewardCondition(eConditionMapDuration, eNegativeCondition, roundDuration);
				}
				else
				{
					CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == conditionName);
					if (ePositiveCondition != CCondition.EPositiveCondition.NA)
					{
						condition = new RewardCondition(eConditionMapDuration, ePositiveCondition, roundDuration);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find condition with name " + conditionName + ". File: " + fileName);
						flag = false;
					}
				}
			}
			if (enemyConditionName != string.Empty)
			{
				CCondition.ENegativeCondition eNegativeCondition2 = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == enemyConditionName);
				if (eNegativeCondition2 != CCondition.ENegativeCondition.NA)
				{
					enemyCondition = new RewardCondition(eConditionMapDuration, eNegativeCondition2, roundDuration);
				}
				else
				{
					CCondition.EPositiveCondition ePositiveCondition2 = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == enemyConditionName);
					if (ePositiveCondition2 != CCondition.EPositiveCondition.NA)
					{
						enemyCondition = new RewardCondition(eConditionMapDuration, ePositiveCondition2, roundDuration);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find condition with name " + conditionName + ". File: " + fileName);
						flag = false;
					}
				}
			}
			if (flag)
			{
				treasureTableEntry = new TreasureTableEntry(type, chance, list, maxAmount, item?.ID ?? (-1), condition, enemyCondition, infuse, enhancement, perksYMLData, unlockAchievement, unlockCharacterID, unlockLocation, unlockQuest, treasureDistributionType, chapter, chapterFilter, subChapterFilter, giveToCharacterID, cityEvent, roadEvent, consumeSlot, dictionary, distributionRestrictionType);
			}
		}
		catch (Exception ex3)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex3);
			flag = false;
		}
		return flag;
	}

	private bool ProcessGroupEntry(MappingEntry entryMap, string fileName, out TreasureTableGroup treasureTableGroup)
	{
		treasureTableGroup = null;
		bool flag = true;
		try
		{
			List<TreasureTableEntry> entries = new List<TreasureTableEntry>();
			List<TreasureTableGroup> list = new List<TreasureTableGroup>();
			List<int> groupChance = null;
			int repeatCount = 1;
			bool andLogic = false;
			string giveToCharacterID = null;
			EGiveToCharacterType eGiveToCharacterType = EGiveToCharacterType.None;
			EGiveToCharacterRequirement giveToCharacterRequirementType = EGiveToCharacterRequirement.None;
			string giveToCharacterRequirementCheckID = null;
			string groupName = null;
			bool updateGroup = false;
			if (YMLShared.GetMapping(entryMap, fileName, out var mapping))
			{
				foreach (MappingEntry entry in mapping.Entries)
				{
					if (entry == null)
					{
						continue;
					}
					switch (entry.Key.ToString())
					{
					case "Chance":
					{
						if (YMLShared.GetIntList(entry.Value, "Chance", fileName, out var values2, allowScalar: true, 8))
						{
							groupChance = values2;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "Group":
					{
						if (ProcessGroupEntry(entry, fileName, out var treasureTableGroup2))
						{
							list.Add(treasureTableGroup2);
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "RepeatCount":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "RepeatCount", fileName, out var value2))
						{
							repeatCount = value2;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "And":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "And", fileName, out var value4))
						{
							andLogic = value4;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "Items":
					case "ItemStocks":
					case "UnlockProsperityItems":
					case "UnlockProsperityItemStocks":
						if (!AddItemEntry(entry, ref entries, fileName))
						{
							flag = false;
						}
						continue;
					case "Enhancements":
						if (entry.Value is Sequence)
						{
							if (YMLShared.GetStringList(entry.Value, "Enhancements", fileName, out var values))
							{
								foreach (string enhancementName in values)
								{
									EEnhancement eEnhancement = CEnhancement.Enhancements.SingleOrDefault((EEnhancement s) => s.ToString() == enhancementName);
									if (eEnhancement != EEnhancement.NoEnhancement)
									{
										List<TreasureTableEntry> list2 = entries;
										EEnhancement? enhancement = eEnhancement;
										list2.Add(new TreasureTableEntry(ETreasureType.Enhancement, null, null, null, -1, null, null, null, enhancement));
									}
									else
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Treasure Enhancement not found: " + enhancementName + ".  File: " + fileName);
										flag = false;
									}
								}
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid enhancements property.  Expected a Sequence.  File: " + fileName);
							flag = false;
						}
						continue;
					case "GiveToCharacter":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "GiveToCharacter", fileName, out var characterValue))
						{
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == characterValue) != null)
							{
								giveToCharacterID = characterValue;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {characterValue} in {fileName}");
								flag = false;
							}
							if (eGiveToCharacterType == EGiveToCharacterType.None)
							{
								eGiveToCharacterType = EGiveToCharacterType.Give;
							}
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "GiveToCharacterType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "GiveToCharacterType", fileName, out var giveToCharacterTypeValue))
						{
							EGiveToCharacterType eGiveToCharacterType2 = Reward.GiveToCharacterTypes.SingleOrDefault((EGiveToCharacterType x) => x.ToString() == giveToCharacterTypeValue);
							if (eGiveToCharacterType2 != EGiveToCharacterType.None)
							{
								eGiveToCharacterType = eGiveToCharacterType2;
							}
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "GiveToCharacterRequirementType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "GiveToCharacterRequirementType", fileName, out var giveToCharacterRequirementTypeValue))
						{
							EGiveToCharacterRequirement eGiveToCharacterRequirement = Reward.GiveToCharacterRequirementTypes.SingleOrDefault((EGiveToCharacterRequirement x) => x.ToString() == giveToCharacterRequirementTypeValue);
							if (eGiveToCharacterRequirement != EGiveToCharacterRequirement.None)
							{
								giveToCharacterRequirementType = eGiveToCharacterRequirement;
							}
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "GiveToCharacterRequirementCheckID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "GiveToCharacterRequirementCheckID", fileName, out var value3))
						{
							giveToCharacterRequirementCheckID = value3;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "GroupName":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "GroupName", fileName, out var value))
						{
							groupName = value;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "Update":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Update", fileName, out var value5))
						{
							updateGroup = value5;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					}
					string key = entry.Key.ToString();
					ETreasureType eTreasureType = Reward.TreasureTypes.SingleOrDefault((ETreasureType x) => x.ToString() == key);
					TreasureTableEntry treasureTableEntry = null;
					if (eTreasureType != ETreasureType.None || key == "Nothing")
					{
						if (ProcessEntry(entry, eTreasureType, fileName, out var treasureTableEntry2))
						{
							treasureTableEntry = treasureTableEntry2;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						CItem item = GetItem(key, fileName);
						if (item != null)
						{
							if (ProcessEntry(entry, ETreasureType.Item, fileName, out var treasureTableEntry3, item))
							{
								treasureTableEntry = treasureTableEntry3;
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "TreasureType not found: " + key + ".  File: " + fileName);
							flag = false;
						}
					}
					if (treasureTableEntry != null)
					{
						entries.Add(treasureTableEntry);
					}
				}
			}
			else
			{
				flag = false;
			}
			if ((flag && entries.Count > 0) || list.Count > 0)
			{
				treasureTableGroup = new TreasureTableGroup(entries, list, groupChance, repeatCount, andLogic, giveToCharacterID, eGiveToCharacterType, giveToCharacterRequirementType, giveToCharacterRequirementCheckID, groupName, updateGroup);
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No entries found in treasure table " + entryMap.Key?.ToString() + ".  File: " + fileName);
				flag = false;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
			flag = false;
		}
		return flag;
	}

	public static CItem GetItem(string itemID, string fileName)
	{
		ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData x) => x.StringID == itemID);
		if (itemCardYMLData != null)
		{
			return itemCardYMLData.GetItem;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Item " + itemID + ". File:" + fileName);
		return null;
	}

	private bool AddItemEntry(MappingEntry entry, ref List<TreasureTableEntry> entries, string fileName)
	{
		bool result = true;
		if (entry.Value is Sequence)
		{
			if (YMLShared.GetStringList(entry.Value, entry.Key.ToString(), fileName, out var values))
			{
				foreach (string item3 in values)
				{
					CItem item = GetItem(item3, fileName);
					if (item != null)
					{
						ETreasureType treasureType = ETreasureType.None;
						switch (entry.Key.ToString())
						{
						case "Items":
							treasureType = ETreasureType.Item;
							break;
						case "ItemStocks":
							treasureType = ETreasureType.ItemStock;
							break;
						case "ItemBlueprint":
							treasureType = ETreasureType.ItemBlueprint;
							break;
						case "UnlockProsperityItems":
							treasureType = ETreasureType.UnlockProsperityItem;
							break;
						case "UnlockProsperityItemStocks":
							treasureType = ETreasureType.UnlockProsperityItemStock;
							break;
						}
						if (treasureType != ETreasureType.None)
						{
							TreasureTableEntry treasureTableEntry = entries.SingleOrDefault((TreasureTableEntry a) => a.Type == treasureType && a.ItemID == item.ID);
							if (treasureTableEntry != null)
							{
								treasureTableEntry.ChangeAmount(1);
								entries.Remove(treasureTableEntry);
								entries.Add(treasureTableEntry);
							}
							else
							{
								TreasureTableEntry item2 = new TreasureTableEntry(treasureType, null, null, null, item.ID);
								entries.Add(item2);
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid treasure table entry.  " + entry.Key.ToString() + ".  Must be Items or ItemStocks. File: " + fileName);
							result = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Treasure Item not found: " + item3 + ".  File: " + fileName);
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
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid treasure table entry.  " + entry.Key.ToString() + " must be a sequence. File: " + fileName);
			result = false;
		}
		return result;
	}
}
