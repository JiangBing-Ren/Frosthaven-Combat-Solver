using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ScenarioYML
{
	public const string DEFAULT_MONSTER_GROUP_KEY = "DefaultMonsterGroup";

	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<ScenarioDefinition> LoadedYML { get; private set; }

	public ScenarioYML()
	{
		LoadedYML = new ConcurrentQueue<ScenarioDefinition>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool result = true;
			YamlParser yamlParser = new YamlParser();
			TextInput textInput = new TextInput(fileStream.ReadToEnd());
			string inputText = textInput.InputText;
			YamlStream yamlStream = yamlParser.ParseYamlStream(textInput, out var success);
			if (success)
			{
				string text = null;
				string bossID = null;
				int bossCount = 1;
				string description = string.Empty;
				string text2 = ScenarioRuleClient.SRLYML.MonsterData.MonsterFamilies[0].FamilyID;
				List<string> rewardTreasureTables = new List<string>();
				List<string> chestTreasureTables = null;
				List<string> scenarioMeshes = null;
				ScenarioMessage startMessage = null;
				ScenarioMessage completeMessage = null;
				ScenarioLayout scenarioLayout = null;
				ApparanceStyle apparanceStyle = null;
				ScenarioPossibleRoom.EBiome eBiome = ScenarioPossibleRoom.EBiome.Default;
				ScenarioPossibleRoom.ESubBiome subBiome = ScenarioPossibleRoom.ESubBiome.Default;
				ScenarioPossibleRoom.ETheme theme = ScenarioPossibleRoom.ETheme.Default;
				ScenarioPossibleRoom.ESubTheme subTheme = ScenarioPossibleRoom.ESubTheme.Default;
				ScenarioPossibleRoom.ETone tone = ScenarioPossibleRoom.ETone.Default;
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value10))
						{
							text = value10;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Description", fileName, out var value11))
						{
							description = value11;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "BaseBiome":
					{
						ScenarioPossibleRoom.EBiome eBiome3 = ScenarioPossibleRoom.Biomes.SingleOrDefault((ScenarioPossibleRoom.EBiome x) => x.ToString() == entry.Value.ToString());
						if (eBiome3 != ScenarioPossibleRoom.EBiome.Inherit)
						{
							eBiome = eBiome3;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "BaseBiome " + entry.Value.ToString() + " is not a defined Biome type. File: " + fileName);
						result = false;
						break;
					}
					case "BaseSubBiome":
					{
						ScenarioPossibleRoom.ESubBiome eSubBiome2 = ScenarioPossibleRoom.SubBiomes.SingleOrDefault((ScenarioPossibleRoom.ESubBiome x) => x.ToString() == entry.Value.ToString());
						if (eSubBiome2 != ScenarioPossibleRoom.ESubBiome.Inherit)
						{
							subBiome = eSubBiome2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "BaseSubBiome " + entry.Value.ToString() + " is not a defined SubBiome type, using Default instead. File: " + fileName);
						result = false;
						break;
					}
					case "BaseTheme":
					{
						ScenarioPossibleRoom.ETheme eTheme2 = ScenarioPossibleRoom.Themes.SingleOrDefault((ScenarioPossibleRoom.ETheme x) => x.ToString() == entry.Value.ToString());
						if (eTheme2 != ScenarioPossibleRoom.ETheme.Inherit)
						{
							theme = eTheme2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "BaseTheme " + entry.Value.ToString() + " is not a defined Theme type, using Default instead. File: " + fileName);
						result = false;
						break;
					}
					case "BaseSubTheme":
					{
						ScenarioPossibleRoom.ESubTheme eSubTheme2 = ScenarioPossibleRoom.SubThemes.SingleOrDefault((ScenarioPossibleRoom.ESubTheme x) => x.ToString() == entry.Value.ToString());
						if (eSubTheme2 != ScenarioPossibleRoom.ESubTheme.Inherit)
						{
							subTheme = eSubTheme2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "BaseSubTheme " + entry.Value.ToString() + " is not a defined SubTheme type, using Default instead. File: " + fileName);
						result = false;
						break;
					}
					case "BaseTone":
					{
						ScenarioPossibleRoom.ETone eTone2 = ScenarioPossibleRoom.Tones.SingleOrDefault((ScenarioPossibleRoom.ETone x) => x.ToString() == entry.Value.ToString());
						if (eTone2 != ScenarioPossibleRoom.ETone.Inherit)
						{
							tone = eTone2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "BaseTone " + entry.Value.ToString() + " is not a defined Tone type, using Default instead. File: " + fileName);
						result = false;
						break;
					}
					case "MonsterFamily":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var monsterFamilyValue))
						{
							if (ScenarioRuleClient.SRLYML.MonsterData.MonsterFamilies.Any((MonsterFamily x) => x.FamilyID == monsterFamilyValue))
							{
								text2 = monsterFamilyValue;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "MonsterFamily " + monsterFamilyValue + " not found in MonsterFamilies in Monsters yml. File: " + fileName);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Boss":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value9))
						{
							bossID = value9;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "BossCount":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), entry.Key.ToString(), fileName, out var value12))
						{
							bossCount = value12;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "RewardTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "RewardTreasureTables", fileName, out var values3))
						{
							foreach (string name5 in values3)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => t.Name == name5) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name5} in {fileName}");
									result = false;
								}
							}
							rewardTreasureTables = values3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ChestTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "ChestTreasureTables", fileName, out var values2))
						{
							foreach (string name4 in values2)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => t.Name == name4) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name4} in {fileName}");
									result = false;
								}
							}
							chestTreasureTables = values2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ScenarioMeshes":
					{
						if (YMLShared.GetStringList(entry.Value, entry.Key.ToString(), fileName, out var values4))
						{
							scenarioMeshes = values4;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Meshes property in File " + fileName);
						result = false;
						break;
					}
					case "ScenarioStartMessage":
					{
						if (ScenarioMessage.ParseMessage(entry, fileName, out var message3))
						{
							startMessage = message3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ScenarioCompleteMessage":
					{
						if (ScenarioMessage.ParseMessage(entry, fileName, out var message2))
						{
							completeMessage = message2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ScenarioLayout":
					{
						Mapping mapping;
						if (text2 == null || eBiome == ScenarioPossibleRoom.EBiome.Inherit)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "MonsterFamily and BaseBiome values must be assigned prior to the ScenarioLayouts section. File: " + fileName);
							result = false;
						}
						else if (YMLShared.GetMapping(entry, fileName, out mapping))
						{
							string text3 = text;
							List<CObjective> list = new List<CObjective>();
							List<CObjective> list2 = new List<CObjective>();
							List<CScenarioModifier> list3 = new List<CScenarioModifier>();
							List<ScenarioRoomEntry> list4 = null;
							foreach (MappingEntry entry2 in mapping.Entries)
							{
								if (entry2 == null)
								{
									continue;
								}
								switch (entry2.Key.ToString())
								{
								case "Objectives":
								{
									if (GetObjectives(entry2, fileName, out var objectives))
									{
										list.AddRange(objectives.Where((CObjective o) => o.Result == EObjectiveResult.Win));
										list2.AddRange(objectives.Where((CObjective o) => o.Result == EObjectiveResult.Lose));
									}
									else
									{
										result = false;
									}
									break;
								}
								case "ScenarioModifiers":
								{
									int num2 = -1;
									if (YMLShared.GetMapping(entry2, fileName, out var mapping7))
									{
										foreach (MappingEntry entry3 in mapping7.Entries)
										{
											num2++;
											string name3 = entry3.Key.ToString();
											EScenarioModifierType eScenarioModifierType = EScenarioModifierType.None;
											EScenarioModifierTriggerPhase eScenarioModifierTriggerPhase = EScenarioModifierTriggerPhase.None;
											CObjectiveFilter scenarioModFilter = null;
											bool applyToEachActorOnce = false;
											bool isPositive = false;
											List<ElementInfusionBoardManager.EElement> list11 = new List<ElementInfusionBoardManager.EElement>();
											List<ElementInfusionBoardManager.EElement> list12 = new List<ElementInfusionBoardManager.EElement>();
											List<ElementInfusionBoardManager.EElement> list13 = new List<ElementInfusionBoardManager.EElement>();
											List<CCondition.EPositiveCondition> list14 = new List<CCondition.EPositiveCondition>();
											List<CCondition.ENegativeCondition> list15 = new List<CCondition.ENegativeCondition>();
											EConditionDecTrigger conditionDecrementTrigger = EConditionDecTrigger.Turns;
											string scenarioAbilityID = string.Empty;
											if (entry3.Value is Scalar)
											{
												if (YMLShared.GetStringPropertyValue(entry3.Value, "ScenarioModifiers", fileName, out var triggerString))
												{
													eScenarioModifierTriggerPhase = CScenarioModifier.ScenarioModifierTriggerPhases.SingleOrDefault((EScenarioModifierTriggerPhase s) => s.ToString() == triggerString);
												}
												else
												{
													result = false;
												}
											}
											else if (entry3.Value is Mapping)
											{
												if (YMLShared.GetMapping(entry3, fileName, out var mapping8))
												{
													foreach (MappingEntry entry4 in mapping8.Entries)
													{
														switch (entry4.Key.ToString())
														{
														case "Type":
														{
															if (YMLShared.GetStringPropertyValue(entry4.Value, "Type", fileName, out var typeValue))
															{
																eScenarioModifierType = CScenarioModifier.ScenarioModifierTypes.SingleOrDefault((EScenarioModifierType s) => s.ToString() == typeValue);
															}
															else
															{
																result = false;
															}
															break;
														}
														case "TriggerPhase":
														{
															if (YMLShared.GetStringPropertyValue(entry4.Value, "ScenarioModifiers", fileName, out var triggerString2))
															{
																eScenarioModifierTriggerPhase = CScenarioModifier.ScenarioModifierTriggerPhases.SingleOrDefault((EScenarioModifierTriggerPhase s) => s.ToString() == triggerString2);
															}
															else
															{
																result = false;
															}
															break;
														}
														case "Filter":
														{
															if (GetObjectiveFilter(entry4, fileName, out var filter))
															{
																scenarioModFilter = filter;
															}
															else
															{
																result = false;
															}
															break;
														}
														case "StrongElements":
														{
															if (CardProcessingShared.GetElements(entry4.Value, "StrongElements", fileName, out var elements))
															{
																list11.AddRange(elements);
															}
															else
															{
																result = false;
															}
															break;
														}
														case "WaningElements":
														{
															if (CardProcessingShared.GetElements(entry4.Value, "WaningElements", fileName, out var elements2))
															{
																list12.AddRange(elements2);
															}
															else
															{
																result = false;
															}
															break;
														}
														case "InertElements":
														{
															if (CardProcessingShared.GetElements(entry4.Value, "InertElements", fileName, out var elements3))
															{
																list13.AddRange(elements3);
															}
															else
															{
																result = false;
															}
															break;
														}
														case "ScenarioAbilityID":
														{
															if (YMLShared.GetStringPropertyValue(entry4.Value, "ScenarioAbilityID", fileName, out var outScenarioAbilityID))
															{
																if (ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData s) => outScenarioAbilityID == s.ScenarioAbilityID) != null)
																{
																	scenarioAbilityID = outScenarioAbilityID;
																	break;
																}
																SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outScenarioAbilityID} in {fileName}");
																result = false;
															}
															else
															{
																result = false;
															}
															break;
														}
														case "Condition":
														case "Conditions":
														{
															if (AbilityData.GetConditions(entry4, fileName, out var negativeConditions, out var positiveConditions))
															{
																list15 = negativeConditions;
																list14 = positiveConditions;
															}
															else
															{
																result = false;
															}
															break;
														}
														case "ConditionDecrementTrigger":
														{
															if (YMLShared.GetStringPropertyValue(entry4.Value, "ConditionDecrementTrigger", fileName, out var conditionDecString))
															{
																conditionDecrementTrigger = CAbilityCondition.ConditionDecTriggers.SingleOrDefault((EConditionDecTrigger s) => s.ToString() == conditionDecString);
															}
															else
															{
																result = false;
															}
															break;
														}
														case "ApplyToEachActorOnce":
														{
															if (YMLShared.GetBoolPropertyValue(entry4.Value, "ApplyToEachActorOnce", fileName, out var value8))
															{
																applyToEachActorOnce = value8;
															}
															else
															{
																result = false;
															}
															break;
														}
														case "IsPositive":
														{
															if (YMLShared.GetBoolPropertyValue(entry4.Value, "IsPositive", fileName, out var value7))
															{
																isPositive = value7;
															}
															else
															{
																result = false;
															}
															break;
														}
														default:
															SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier property " + entry4.Key.ToString() + ". File: " + fileName);
															result = false;
															break;
														}
													}
												}
												else
												{
													result = false;
												}
											}
											switch (eScenarioModifierType)
											{
											case EScenarioModifierType.None:
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier. File: " + fileName);
												result = false;
												continue;
											default:
												if (eScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.None)
												{
													SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier Trigger Phase. File: " + fileName);
													result = false;
													continue;
												}
												break;
											case EScenarioModifierType.AddConditionsToAbilities:
												break;
											}
											if ((eScenarioModifierType == EScenarioModifierType.TriggerAbility || eScenarioModifierType == EScenarioModifierType.ApplyActiveBonusToActor) && string.IsNullOrEmpty(scenarioAbilityID))
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier Scenario Ability ID. File: " + fileName);
												result = false;
												continue;
											}
											if ((eScenarioModifierType == EScenarioModifierType.TriggerAbility || eScenarioModifierType == EScenarioModifierType.ApplyActiveBonusToActor) && ScenarioRuleClient.SRLYML.ScenarioAbilities.SingleOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == scenarioAbilityID) == null)
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier Scenario Ability ID, no matching ID found in ScenarioAbilities. File: " + fileName);
												result = false;
												continue;
											}
											if (eScenarioModifierType == EScenarioModifierType.AddConditionsToAbilities && list14.Count <= 0 && list15.Count <= 0)
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier - AddConditionsToAbilities has no positive or negative conditions. File: " + fileName);
												result = false;
												continue;
											}
											if (eScenarioModifierType == EScenarioModifierType.ApplyConditionToActor && list14.Count <= 0 && list15.Count <= 0)
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier - ApplyConditionToActor has no positive or negative conditions. File: " + fileName);
												result = false;
												continue;
											}
											switch (eScenarioModifierType)
											{
											case EScenarioModifierType.AddModifierCards:
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Scenario Modifier - AddModifierCards is not currently supported by YML. File: " + fileName);
												result = false;
												break;
											case EScenarioModifierType.SetElements:
												list3.Add(new CScenarioModifierSetElements(name3, num2, 0, eScenarioModifierTriggerPhase, applyToEachActorOnce, scenarioModFilter, isPositive, list11, list12, list13, 1, 0));
												break;
											case EScenarioModifierType.TriggerAbility:
												list3.Add(new CScenarioModifierTriggerAbility(name3, num2, 0, eScenarioModifierTriggerPhase, applyToEachActorOnce, scenarioModFilter, isPositive, scenarioAbilityID, CScenarioModifierTriggerAbility.ETriggerAbilityStackingType.None, null, new List<TileIndex>()));
												break;
											case EScenarioModifierType.AddConditionsToAbilities:
												list3.Add(new CScenarioModifierAddConditionsToAbilities(name3, num2, 0, eScenarioModifierTriggerPhase, applyToEachActorOnce, scenarioModFilter, isPositive, list14, list15));
												break;
											case EScenarioModifierType.ApplyConditionToActor:
												list3.Add(new CScenarioModifierApplyConditionToActor(name3, num2, 0, eScenarioModifierTriggerPhase, applyToEachActorOnce, scenarioModFilter, isPositive, list14, list15, EScenarioModifierActivationType.None, null, conditionDecrementTrigger));
												break;
											case EScenarioModifierType.ApplyActiveBonusToActor:
												list3.Add(new CScenarioModifierApplyActiveBonusToActor(name3, num2, 0, eScenarioModifierTriggerPhase, applyToEachActorOnce, scenarioModFilter, isPositive, scenarioAbilityID, null));
												break;
											}
										}
									}
									else
									{
										result = false;
									}
									break;
								}
								case "Rooms":
								{
									list4 = new List<ScenarioRoomEntry>();
									if (YMLShared.GetMapping(entry2, fileName, out var mapping2))
									{
										foreach (MappingEntry entry5 in mapping2.Entries)
										{
											Mapping mapping3;
											if (entry5 == null)
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null room entry in File: " + fileName);
												result = false;
											}
											else if (YMLShared.GetMapping(entry5, fileName, out mapping3))
											{
												string text4 = entry5.Key.ToString();
												int num = int.MinValue;
												string parentRoom = string.Empty;
												bool flag = false;
												ScenarioMessage roomRevealedMessage = null;
												List<ScenarioPossibleRoom> list5 = null;
												bool flag2 = false;
												bool flag3 = false;
												foreach (MappingEntry entry6 in mapping3.Entries)
												{
													if (entry6 == null)
													{
														SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null room property entry in File: " + fileName);
														result = false;
														continue;
													}
													switch (entry6.Key.ToString())
													{
													case "Threat":
													{
														if (YMLShared.ParseIntValue(entry6.Value.ToString(), "Threat", fileName, out var value6))
														{
															num = value6;
														}
														else
														{
															result = false;
														}
														break;
													}
													case "ParentRoom":
													{
														if (YMLShared.GetStringPropertyValue(entry6.Value, "ParentRoom", fileName, out var value2))
														{
															parentRoom = value2;
														}
														else
														{
															result = false;
														}
														break;
													}
													case "BossInHere":
													{
														if (YMLShared.GetBoolPropertyValue(entry6.Value, "BossInHere", fileName, out var value3))
														{
															if (!flag)
															{
																flag = value3;
															}
														}
														else
														{
															result = false;
														}
														break;
													}
													case "RoomRevealedMessage":
													{
														if (ScenarioMessage.ParseMessage(entry6, fileName, out var message))
														{
															roomRevealedMessage = message;
														}
														else
														{
															result = false;
														}
														break;
													}
													case "PossibleRooms":
													{
														list5 = new List<ScenarioPossibleRoom>();
														if (!YMLShared.GetSequence(entry6.Value, entry6.Key.ToString(), fileName, out var sequence))
														{
															break;
														}
														foreach (DataItem entry7 in sequence.Entries)
														{
															string roomName = null;
															string text5 = "DefaultMonsterGroup";
															ScenarioPossibleRoom.EBiome eBiome2 = eBiome;
															ScenarioPossibleRoom.ESubBiome eSubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
															ScenarioPossibleRoom.ETheme eTheme = ScenarioPossibleRoom.ETheme.Inherit;
															ScenarioPossibleRoom.ESubTheme eSubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
															ScenarioPossibleRoom.ETone eTone = ScenarioPossibleRoom.ETone.Inherit;
															List<int> oneHexObstacles = null;
															List<int> twoHexObstacles = null;
															List<int> threeHexObstacles = null;
															List<int> traps = null;
															List<int> pressurePlates = null;
															List<int> terrainHotCoals = null;
															List<int> terrainWater = null;
															List<int> terrainThorns = null;
															List<int> terrainRubble = null;
															List<int> goldPiles = null;
															List<int> treasureChestChance = null;
															List<string> chestTreasureTables2 = null;
															List<List<string>> list6 = new List<List<string>>();
															List<SpawnerData> list7 = new List<SpawnerData>();
															List<Tuple<string, List<int>>> list8 = new List<Tuple<string, List<int>>>();
															List<Tuple<string, List<int>>> list9 = new List<Tuple<string, List<int>>>();
															List<Tuple<string, List<int>>> list10 = new List<Tuple<string, List<int>>>();
															if (entry7 is Scalar)
															{
																roomName = (entry7 as Scalar).Text;
															}
															else if (entry7 is Mapping)
															{
																MappingEntry mappingEntry = (entry7 as Mapping).Entries[0];
																roomName = mappingEntry.Key.ToString();
																Mapping mapping4;
																if (mappingEntry.Value is Scalar)
																{
																	text5 = (mappingEntry.Value as Scalar).Text;
																}
																else if (YMLShared.GetMapping(mappingEntry, fileName, out mapping4))
																{
																	foreach (MappingEntry possibleRoomPropertiesEntry in mapping4.Entries)
																	{
																		if (possibleRoomPropertiesEntry == null)
																		{
																			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null possible room entry in File: " + fileName);
																			result = false;
																			continue;
																		}
																		List<string> values;
																		switch (possibleRoomPropertiesEntry.Key.ToString())
																		{
																		case "MonsterGroup":
																		{
																			if (YMLShared.GetStringPropertyValue(possibleRoomPropertiesEntry.Value, text3 + "/" + text4 + "/" + possibleRoomPropertiesEntry.Key.ToString(), fileName, out var value4))
																			{
																				text5 = value4;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "Biome":
																			eBiome2 = ScenarioPossibleRoom.Biomes.SingleOrDefault((ScenarioPossibleRoom.EBiome x) => x.ToString() == possibleRoomPropertiesEntry.Value.ToString());
																			if (eBiome2 == ScenarioPossibleRoom.EBiome.Inherit && possibleRoomPropertiesEntry.Value.ToString() != ScenarioPossibleRoom.EBiome.Inherit.ToString())
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid " + possibleRoomPropertiesEntry.Key.ToString() + " entry in " + text3 + "/" + text4 + ", using Inherit instead. File: " + fileName);
																				result = false;
																			}
																			break;
																		case "SubBiome":
																			eSubBiome = ScenarioPossibleRoom.SubBiomes.SingleOrDefault((ScenarioPossibleRoom.ESubBiome x) => x.ToString() == possibleRoomPropertiesEntry.Value.ToString());
																			if (eSubBiome == ScenarioPossibleRoom.ESubBiome.Inherit && possibleRoomPropertiesEntry.Value.ToString() != ScenarioPossibleRoom.ESubBiome.Inherit.ToString())
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid " + possibleRoomPropertiesEntry.Key.ToString() + " entry in " + text3 + "/" + text4 + ", using Inherit instead. File: " + fileName);
																				result = false;
																			}
																			break;
																		case "Theme":
																			eTheme = ScenarioPossibleRoom.Themes.SingleOrDefault((ScenarioPossibleRoom.ETheme x) => x.ToString() == possibleRoomPropertiesEntry.Value.ToString());
																			if (eTheme == ScenarioPossibleRoom.ETheme.Inherit && possibleRoomPropertiesEntry.Value.ToString() != ScenarioPossibleRoom.ETheme.Inherit.ToString())
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid " + possibleRoomPropertiesEntry.Key.ToString() + " entry in " + text3 + "/" + text4 + " using Inherit instead., File: " + fileName);
																				result = false;
																			}
																			break;
																		case "SubTheme":
																			eSubTheme = ScenarioPossibleRoom.SubThemes.SingleOrDefault((ScenarioPossibleRoom.ESubTheme x) => x.ToString() == possibleRoomPropertiesEntry.Value.ToString());
																			if (eSubTheme == ScenarioPossibleRoom.ESubTheme.Inherit && possibleRoomPropertiesEntry.Value.ToString() != ScenarioPossibleRoom.ESubTheme.Inherit.ToString())
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid " + possibleRoomPropertiesEntry.Key.ToString() + " entry in " + text3 + "/" + text4 + ", using Inherit instead. File: " + fileName);
																				result = false;
																			}
																			break;
																		case "Tone":
																			eTone = ScenarioPossibleRoom.Tones.SingleOrDefault((ScenarioPossibleRoom.ETone x) => x.ToString() == possibleRoomPropertiesEntry.Value.ToString());
																			if (eTone == ScenarioPossibleRoom.ETone.Inherit && possibleRoomPropertiesEntry.Value.ToString() != ScenarioPossibleRoom.ETone.Inherit.ToString())
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid " + possibleRoomPropertiesEntry.Key.ToString() + " entry in " + text3 + "/" + text4 + ", using Inherit instead. File: " + fileName);
																				result = false;
																			}
																			break;
																		case "OneHexObstacles":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables10))
																			{
																				oneHexObstacles = spawnables10;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "TwoHexObstacles":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables6))
																			{
																				twoHexObstacles = spawnables6;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "ThreeHexObstacles":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables))
																			{
																				threeHexObstacles = spawnables;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "Traps":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables11))
																			{
																				traps = spawnables11;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "Spawner":
																			if (list7.Count == 0)
																			{
																				if (YMLShared.GetMapping(possibleRoomPropertiesEntry, fileName, out var mapping5))
																				{
																					if (GetSpawnerData(mapping5.Entries, fileName, out var spawnerData))
																					{
																						list7.Add(spawnerData);
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
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Single spawner already specified for ScenarioYML, use Spawners instead to specify multiple in a list. File " + fileName);
																				result = false;
																			}
																			break;
																		case "Spawners":
																		{
																			if (YMLShared.GetSequence(possibleRoomPropertiesEntry, fileName, out var sequence3))
																			{
																				foreach (DataItem entry8 in sequence3.Entries)
																				{
																					if (entry8 is Mapping mapping6)
																					{
																						if (GetSpawnerData(mapping6.Entries, fileName, out var spawnerData2))
																						{
																							list7.Add(spawnerData2);
																						}
																						else
																						{
																							result = false;
																						}
																					}
																					else
																					{
																						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected Spawners entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
																						result = false;
																					}
																				}
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected Spawners entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
																				result = false;
																			}
																			break;
																		}
																		case "PressurePlates":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables9))
																			{
																				pressurePlates = spawnables9;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "TerrainHotCoals":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables7))
																			{
																				terrainHotCoals = spawnables7;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "TerrainWater":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables8))
																			{
																				terrainWater = spawnables8;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "TerrainThorns":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables2))
																			{
																				terrainThorns = spawnables2;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "TerrainRubble":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables4))
																			{
																				terrainRubble = spawnables4;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "GoldPiles":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables5))
																			{
																				goldPiles = spawnables5;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "TreasureChestChance":
																		{
																			if (ScenarioRoomsYML.ProcessSpawnables(possibleRoomPropertiesEntry, fileName, out var spawnables3))
																			{
																				treasureChestChance = spawnables3;
																			}
																			else
																			{
																				result = false;
																			}
																			break;
																		}
																		case "ChestTreasureTables":
																			if (YMLShared.GetStringList(possibleRoomPropertiesEntry.Value, "ChestTreasureTables", fileName, out values))
																			{
																				foreach (string name in values)
																				{
																					if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable tt) => tt.Name == name) == null)
																					{
																						SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name} in {fileName}");
																						result = false;
																					}
																				}
																				chestTreasureTables2 = values;
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse Treasure Table list room override.  File:" + fileName);
																				result = false;
																			}
																			break;
																		case "GoalChest":
																			if (list6.Count == 0)
																			{
																				if (YMLShared.GetStringList(possibleRoomPropertiesEntry.Value, "GoalChest", fileName, out values))
																				{
																					foreach (string name2 in values)
																					{
																						TreasureTable treasureTable = ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable tt) => tt.Name == name2);
																						ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData i) => i.StringID == name2.Trim('£'));
																						if (treasureTable == null && itemCardYMLData == null)
																						{
																							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name2} in {fileName}");
																							result = false;
																						}
																					}
																					list6.Add(values);
																				}
																				else
																				{
																					result = false;
																				}
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Single GoalChest already specified for ScenarioYML, use GoalChests instead to specify multiple in a list. File " + fileName);
																				result = false;
																			}
																			break;
																		case "GoalChests":
																		{
																			if (YMLShared.GetSequence(possibleRoomPropertiesEntry, fileName, out var sequence4))
																			{
																				foreach (DataItem entry9 in sequence4.Entries)
																				{
																					if (entry9 is Mapping)
																					{
																						if (YMLShared.GetStringList(entry9, "GoalChest", fileName, out values))
																						{
																							list6.Add(values);
																						}
																						else
																						{
																							result = false;
																						}
																					}
																					else
																					{
																						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected GoalChests entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
																						result = false;
																					}
																				}
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected GoalChests entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
																				result = false;
																			}
																			break;
																		}
																		case "AllyMonsters":
																			list8 = new List<Tuple<string, List<int>>>();
																			if (possibleRoomPropertiesEntry.Value is Sequence)
																			{
																				Sequence sequence6 = possibleRoomPropertiesEntry.Value as Sequence;
																				if (sequence6.Entries[0] is Sequence)
																				{
																					foreach (DataItem entry10 in sequence6.Entries)
																					{
																						if (YMLShared.GetTupleStringListInt(entry10, "AllyMonsters", fileName, out var outAllyMonstersTuple))
																						{
																							if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData m) => m.ID == outAllyMonstersTuple.Item1) != null)
																							{
																								list8.Add(outAllyMonstersTuple);
																								continue;
																							}
																							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outAllyMonstersTuple.Item1} in {fileName}");
																							result = false;
																						}
																						else
																						{
																							result = false;
																						}
																					}
																				}
																				else if (sequence6.Entries.Count == 5)
																				{
																					if (YMLShared.GetTupleStringListInt(sequence6, "AllyMonsters", fileName, out var outAllyMonstersTuple2))
																					{
																						if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData m) => m.ID == outAllyMonstersTuple2.Item1) != null)
																						{
																							list8.Add(outAllyMonstersTuple2);
																							break;
																						}
																						SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outAllyMonstersTuple2.Item1} in {fileName}");
																						result = false;
																					}
																					else
																					{
																						result = false;
																					}
																				}
																				else
																				{
																					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid AllyMonsters entry, must be list of [string, int, int, int, int] pairs. File: " + fileName);
																					result = false;
																				}
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid AllyMonsters entry, must be list of [string, int, int, int, int] pairs. File: " + fileName);
																				result = false;
																			}
																			break;
																		case "Enemy2Monsters":
																			list9 = new List<Tuple<string, List<int>>>();
																			if (possibleRoomPropertiesEntry.Value is Sequence)
																			{
																				Sequence sequence5 = possibleRoomPropertiesEntry.Value as Sequence;
																				if (sequence5.Entries[0] is Sequence)
																				{
																					foreach (DataItem entry11 in sequence5.Entries)
																					{
																						if (YMLShared.GetTupleStringListInt(entry11, "Enemy2Monsters", fileName, out var outEnemy2MonstersTuple))
																						{
																							if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData m) => m.ID == outEnemy2MonstersTuple.Item1) != null)
																							{
																								list9.Add(outEnemy2MonstersTuple);
																								continue;
																							}
																							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outEnemy2MonstersTuple.Item1} in {fileName}");
																							result = false;
																						}
																						else
																						{
																							result = false;
																						}
																					}
																				}
																				else if (sequence5.Entries.Count == 5)
																				{
																					if (YMLShared.GetTupleStringListInt(sequence5, "Enemy2Monsters", fileName, out var outEnemy2MonstersTuple2))
																					{
																						if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData m) => m.ID == outEnemy2MonstersTuple2.Item1) != null)
																						{
																							list9.Add(outEnemy2MonstersTuple2);
																							break;
																						}
																						SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outEnemy2MonstersTuple2.Item1} in {fileName}");
																						result = false;
																					}
																					else
																					{
																						result = false;
																					}
																				}
																				else
																				{
																					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Enemy2Monsters entry, must be list of [string, int, int, int, int] pairs. File: " + fileName);
																					result = false;
																				}
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Enemy2Monsters entry, must be list of [string, int, int, int, int] pairs. File: " + fileName);
																				result = false;
																			}
																			break;
																		case "Objects":
																			list10 = new List<Tuple<string, List<int>>>();
																			if (possibleRoomPropertiesEntry.Value is Sequence)
																			{
																				Sequence sequence2 = possibleRoomPropertiesEntry.Value as Sequence;
																				if (sequence2.Entries[0] is Sequence)
																				{
																					foreach (DataItem entry12 in sequence2.Entries)
																					{
																						if (YMLShared.GetTupleStringListInt(entry12, "Objects", fileName, out var outObjectsTuple))
																						{
																							if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData o) => o.ID == outObjectsTuple.Item1) != null)
																							{
																								list10.Add(outObjectsTuple);
																								continue;
																							}
																							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outObjectsTuple.Item1} in {fileName}");
																							result = false;
																						}
																						else
																						{
																							result = false;
																						}
																					}
																				}
																				else if (sequence2.Entries.Count == 5)
																				{
																					if (YMLShared.GetTupleStringListInt(sequence2, "Objects", fileName, out var outObjectsTuple2))
																					{
																						if (ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData o) => o.ID == outObjectsTuple2.Item1) != null)
																						{
																							list10.Add(outObjectsTuple2);
																							break;
																						}
																						SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outObjectsTuple2.Item1} in {fileName}");
																						result = false;
																					}
																					else
																					{
																						result = false;
																					}
																				}
																				else
																				{
																					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objects entry, must be list of [string, int, int, int, int] pairs. File: " + fileName);
																					result = false;
																				}
																			}
																			else
																			{
																				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objects entry, must be list of [string, int, int, int, int] pairs. File: " + fileName);
																				result = false;
																			}
																			break;
																		default:
																			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + possibleRoomPropertiesEntry.Key.ToString() + " in " + text3 + "/" + text4 + ", File: " + fileName);
																			result = false;
																			break;
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
																SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid PossibleRooms entry in Rooms entry " + text4 + " in scenario layout " + text3 + ", must be Scalar or Mapping. File: " + fileName);
																result = false;
															}
															if (!ScenarioRuleClient.SRLYML.ScenarioRooms.Rooms.Any((ScenarioRoomsYML.RoguelikeRoom x) => x.RoomID == roomName))
															{
																SharedClient.ValidationRecord.RecordParseFailure(fileName, "No rooms ymls exist with requested room name " + roomName + " in " + text3 + "/" + text4 + ". File: " + fileName);
																result = false;
															}
															else if (text5 == "")
															{
																SharedClient.ValidationRecord.RecordParseFailure(fileName, "Monster group is invalid");
																result = false;
															}
															else
															{
																list5.Add(new ScenarioPossibleRoom(roomName, text5, eBiome2, eSubBiome, eTheme, eSubTheme, eTone, oneHexObstacles, twoHexObstacles, threeHexObstacles, traps, pressurePlates, terrainHotCoals, terrainWater, terrainThorns, terrainRubble, goldPiles, treasureChestChance, chestTreasureTables2, list6, list7, list8, list9, list10));
															}
														}
														break;
													}
													case "IsDungeonExitRoom":
													{
														if (YMLShared.GetBoolPropertyValue(entry6.Value, "IsDungeonExitRoom", fileName, out var value5))
														{
															if (!flag2)
															{
																flag2 = value5;
															}
														}
														else
														{
															result = false;
														}
														break;
													}
													case "IsAdditionalDungeonEntrance":
													{
														if (YMLShared.GetBoolPropertyValue(entry6.Value, "IsAdditionalDungeonEntrance", fileName, out var value))
														{
															if (!flag3)
															{
																flag3 = value;
															}
														}
														else
														{
															result = false;
														}
														break;
													}
													default:
														SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry6.Key.ToString() + "in room " + text4 + ", File: " + fileName);
														result = false;
														break;
													}
												}
												if (num == int.MinValue)
												{
													SharedClient.ValidationRecord.RecordParseFailure(fileName, "Threat value not assigned in " + text3 + ", Room: " + text4 + ", File:" + fileName);
													result = false;
												}
												else if (list5 == null || list5.Count == 0)
												{
													SharedClient.ValidationRecord.RecordParseFailure(fileName, "Possible rooms not assigned in " + text3 + ", Room: " + text4 + ", File:" + fileName);
													result = false;
												}
												else
												{
													list4.Add(new ScenarioRoomEntry(text4, num, parentRoom, flag, roomRevealedMessage, list5, flag2, flag3));
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
									break;
								}
								default:
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry2.Key.ToString() + " in " + entry.Key.ToString() + "/" + entry2.Key.ToString() + ", File: " + fileName);
									result = false;
									break;
								}
							}
							if (list4 == null || list4.Count == 0)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Rooms entry in ScenarioLayout " + text3 + ", File:" + fileName);
								result = false;
							}
							else
							{
								scenarioLayout = new ScenarioLayout(text3, list, list2, list3, list4);
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry.Key.ToString() + " in ScenarioYML, File: " + fileName);
						result = false;
						break;
					case "Parser":
						break;
					}
				}
				if (text == null || text == string.Empty)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ID in scenario file: " + fileName);
					result = false;
				}
				else
				{
					apparanceStyle = new ApparanceStyle(eBiome, subBiome, theme, subTheme, tone);
					LoadedYML.Enqueue(new ScenarioDefinition(text, description, apparanceStyle, bossID, bossCount, text2, scenarioMeshes, rewardTreasureTables, chestTreasureTables, startMessage, completeMessage, scenarioLayout, fileName, inputText));
				}
				return result;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "", ex);
		}
		return false;
	}

	public static bool GetObjectives(MappingEntry mappingEntry, string fileName, out List<CObjective> objectives)
	{
		bool result = true;
		objectives = new List<CObjective>();
		if (YMLShared.GetMapping(mappingEntry, fileName, out var mapping))
		{
			foreach (MappingEntry objectiveME in mapping.Entries)
			{
				EObjectiveType eObjectiveType = CObjective.ObjectiveTypes.SingleOrDefault((EObjectiveType s) => s.ToString() == objectiveME.Key.ToString());
				EObjectiveResult eObjectiveResult = EObjectiveResult.None;
				CObjectiveFilter objectiveFilter = new CObjectiveFilter();
				bool hideCounter = false;
				bool activeFromStart = true;
				int? activateOnRound = null;
				int num = int.MaxValue;
				List<int> list = null;
				List<int> list2 = null;
				List<int> list3 = null;
				List<int> list4 = null;
				List<int> list5 = null;
				List<int> list6 = null;
				string customLoc = null;
				if (objectiveME.Value is Scalar)
				{
					if (YMLShared.GetStringPropertyValue(objectiveME.Value, "Objectives", fileName, out var resultString))
					{
						eObjectiveResult = CObjective.ObjectiveResults.SingleOrDefault((EObjectiveResult s) => s.ToString() == resultString);
					}
					else
					{
						result = false;
					}
				}
				else if (objectiveME.Value is Mapping)
				{
					if (YMLShared.GetMapping(objectiveME, fileName, out var mapping2))
					{
						foreach (MappingEntry entry in mapping2.Entries)
						{
							switch (entry.Key.ToString())
							{
							case "HideCounter":
							{
								if (YMLShared.GetBoolPropertyValue(entry.Value, "Objectives", fileName, out var value3))
								{
									hideCounter = value3;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "Result":
							{
								if (YMLShared.GetStringPropertyValue(entry.Value, "Objectives", fileName, out var resultString2))
								{
									eObjectiveResult = CObjective.ObjectiveResults.SingleOrDefault((EObjectiveResult s) => s.ToString() == resultString2);
								}
								else
								{
									result = false;
								}
								break;
							}
							case "Filter":
							{
								if (GetObjectiveFilter(entry, fileName, out var filter))
								{
									objectiveFilter = filter;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "ActivationRound":
							{
								if (YMLShared.GetIntPropertyValue(entry.Value, "Objectives", fileName, out var value4))
								{
									activateOnRound = value4;
									activeFromStart = false;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "ReachRound":
							{
								if (YMLShared.GetIntPropertyValue(entry.Value, "Objectives", fileName, out var value2))
								{
									num = value2;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "KillAmount":
							{
								if (YMLShared.GetIntList(entry.Value, "KillAmount", fileName, out var values6))
								{
									list = values6;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "LootAmount":
							{
								if (YMLShared.GetIntList(entry.Value, "LootAmount", fileName, out var values3))
								{
									list2 = values3;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "DestroyAmount":
							{
								if (YMLShared.GetIntList(entry.Value, "DestroyAmount", fileName, out var values))
								{
									list3 = values;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "ActivateAmount":
							{
								if (YMLShared.GetIntList(entry.Value, "ActivateAmount", fileName, out var values5))
								{
									list4 = values5;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "ActorAmount":
							{
								if (YMLShared.GetIntList(entry.Value, "ActorAmount", fileName, out var values4))
								{
									list5 = values4;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "DamageAmount":
							{
								if (YMLShared.GetIntList(entry.Value, "DamageAmount", fileName, out var values2))
								{
									list6 = values2;
								}
								else
								{
									result = false;
								}
								break;
							}
							case "ObjectiveDescription":
							{
								if (YMLShared.GetStringPropertyValue(entry.Value, "ObjectiveDescription", fileName, out var value))
								{
									customLoc = value;
								}
								else
								{
									result = false;
								}
								break;
							}
							}
						}
					}
					else
					{
						result = false;
					}
				}
				if (eObjectiveType == EObjectiveType.None)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective. File: " + fileName);
					result = false;
					continue;
				}
				if (eObjectiveResult == EObjectiveResult.None)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective Result. File: " + fileName);
					result = false;
					continue;
				}
				if (eObjectiveType == EObjectiveType.ReachRound && num == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective ReachRound value. File: " + fileName);
					result = false;
					continue;
				}
				if (eObjectiveType == EObjectiveType.XCharactersDie && (list == null || list.Count != 4))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective XCharactersDie kill amount value - must have 4 entries: " + fileName);
					result = false;
					continue;
				}
				if (eObjectiveType == EObjectiveType.LootX && (list2 == null || list2.Count != 4))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective LootX loot amount value - must have 4 entries: " + fileName);
					result = false;
					continue;
				}
				if (eObjectiveType == EObjectiveType.DestroyXObjects && (list3 == null || list3.Count != 4))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective DestroyXObjects destroy amount value - must have 4 entries: " + fileName);
					result = false;
					continue;
				}
				if ((eObjectiveType == EObjectiveType.ActorReachPosition || eObjectiveType == EObjectiveType.AnyActorReachPosition || eObjectiveType == EObjectiveType.BringItemsToArea || eObjectiveType == EObjectiveType.ActorsEscaped) && (list5 == null || list5.Count != 4))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective ActorReachPosition/AnyActorReachPosition/ActorsEscaped actor amount value - must have 4 entries: " + fileName);
					result = false;
					continue;
				}
				if (eObjectiveType == EObjectiveType.DealXDamage && (list6 == null || list6.Count != 4))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective DealXDamage damage amount value - must have 4 entries: " + fileName);
					result = false;
					continue;
				}
				switch (eObjectiveType)
				{
				case EObjectiveType.CustomTrigger:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "YML-driven Custom Trigger objectives are not currently supported: " + fileName);
					result = false;
					continue;
				case EObjectiveType.ActivateXPressurePlates:
				case EObjectiveType.DeactivateXSpawners:
				case EObjectiveType.ActivateXSpawners:
					if (list4 == null || list4.Count != 4)
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Objective ActivatePressurePlateX activate amount value - must have 4 entries: " + fileName);
						result = false;
						continue;
					}
					break;
				}
				switch (eObjectiveType)
				{
				case EObjectiveType.KillAllEnemies:
					objectives.Add(new CObjective_KillAllEnemies(eObjectiveResult, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.KillAllBosses:
					objectives.Add(new CObjective_KillAllBosses(eObjectiveResult, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.ReachRound:
					objectives.Add(new CObjective_ReachRound(eObjectiveResult, objectiveFilter, num, countFromActivationRound: false, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.ActorReachPosition:
					objectives.Add(new CObjective_ActorReachPosition(eObjectiveResult, objectiveFilter, list5, new List<TileIndex>(), hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.XCharactersDie:
					objectives.Add(new CObjective_XCharactersDie(eObjectiveResult, objectiveFilter, list, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.LootX:
					objectives.Add(new CObjective_LootX(eObjectiveResult, objectiveFilter, list2, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.ActivateXPressurePlates:
					objectives.Add(new CObjective_ActivatePressurePlateX(eObjectiveResult, objectiveFilter, list4, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.DestroyXObjects:
					objectives.Add(new CObjective_DestroyXObjects(eObjectiveResult, objectiveFilter, list3, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.RevealAllRooms:
					objectives.Add(new CObjective_RevealAllRooms(eObjectiveResult, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.AllCharactersMustLoot:
					objectives.Add(new CObjective_AllCharactersMustLoot(eObjectiveResult, objectiveFilter, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.AnyActorReachPosition:
					objectives.Add(new CObjective_AnyActorReachPosition(eObjectiveResult, objectiveFilter, list5, new List<TileIndex>(), hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.DeactivateXSpawners:
					objectives.Add(new CObjective_DeactivateXSpawners(eObjectiveResult, objectiveFilter, list4, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.ActivateXSpawners:
					objectives.Add(new CObjective_ActivateXSpawners(eObjectiveResult, objectiveFilter, list4, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.ActorsEscaped:
					objectives.Add(new CObjective_ActorsEscaped(eObjectiveResult, objectiveFilter, list5, new List<TileIndex>(), hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.BringItemsToArea:
					objectives.Add(new CObjective_BringItemsToArea(eObjectiveResult, objectiveFilter, list5, new List<TileIndex>(), hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				case EObjectiveType.DealXDamage:
					objectives.Add(new CObjective_DealXDamage(eObjectiveResult, objectiveFilter, list6, hideCounter, activeFromStart, activateOnRound, customLoc));
					break;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	public static bool GetObjectiveFilter(MappingEntry keyMappingEntry, string filename, out CObjectiveFilter filter)
	{
		bool result = true;
		filter = new CObjectiveFilter();
		if (keyMappingEntry.Value is Scalar || keyMappingEntry.Value is Sequence)
		{
			if (YMLShared.GetStringList(keyMappingEntry.Value, "Filter/EnemyType", filename, out var values))
			{
				foreach (string ftype in values)
				{
					CAbilityFilter.EFilterEnemy eFilterEnemy = CAbilityFilter.FilterEnemyTypes.SingleOrDefault((CAbilityFilter.EFilterEnemy x) => x.ToString() == ftype);
					if (eFilterEnemy != CAbilityFilter.EFilterEnemy.None)
					{
						filter.FilterEnemyType |= eFilterEnemy;
						continue;
					}
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Enemy Type '" + ftype + "'.  File: " + filename);
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		else if (keyMappingEntry.Value is Mapping)
		{
			if (YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
			{
				foreach (MappingEntry entry in mapping.Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ActorType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/ActorType", filename, out var values5))
						{
							foreach (string ftype4 in values5)
							{
								CAbilityFilter.EFilterActorType eFilterActorType = CAbilityFilter.FilterActorTypes.SingleOrDefault((CAbilityFilter.EFilterActorType x) => x.ToString() == ftype4);
								if (eFilterActorType != CAbilityFilter.EFilterActorType.None)
								{
									filter.FilterActorType |= eFilterActorType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Actor Type '" + ftype4 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "EnemyType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/EnemyType", filename, out var values3))
						{
							foreach (string ftype2 in values3)
							{
								CAbilityFilter.EFilterEnemy eFilterEnemy2 = CAbilityFilter.FilterEnemyTypes.SingleOrDefault((CAbilityFilter.EFilterEnemy x) => x.ToString() == ftype2);
								if (eFilterEnemy2 != CAbilityFilter.EFilterEnemy.None)
								{
									filter.FilterEnemyType |= eFilterEnemy2;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Enemy Type '" + ftype2 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "LootType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/LootType", filename, out var values4))
						{
							foreach (string ftype3 in values4)
							{
								CAbilityFilter.ELootType eLootType = CAbilityFilter.FilterLootTypes.SingleOrDefault((CAbilityFilter.ELootType x) => x.ToString() == ftype3);
								if (eLootType != CAbilityFilter.ELootType.None)
								{
									filter.FilterLootType |= eLootType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Loot Type '" + ftype3 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AbilityType":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/AbilityType", filename, out var values7))
						{
							foreach (string ftype5 in values7)
							{
								CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == ftype5);
								if (eAbilityType != CAbility.EAbilityType.None)
								{
									filter.FilterAbilityType |= eAbilityType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Ability Type '" + ftype5 + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "PlayerClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/PlayerClasses", filename, out var values9))
						{
							filter.FilterPlayerClassIDs = values9.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Player Classes '" + values9?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "EnemyClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/EnemyClasses", filename, out var values6))
						{
							filter.FilterEnemyClassIDs = values6;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Enemy Classes '" + values6?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "HeroSummonClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/HeroSummonClasses", filename, out var values8))
						{
							filter.FilterHeroSummonClassIDs = values8.Distinct().ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Hero Summon Classes '" + values8?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					case "ObjectClasses":
					{
						if (YMLShared.GetStringList(entry.Value, "Filter/ObjectClasses", filename, out var values2))
						{
							filter.FilterObjectClassIDs = values2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter Object Classes '" + values2?.ToString() + "'.  File: " + filename);
						result = false;
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Filter entry '" + entry.Key.ToString() + "'.  File: " + filename);
						result = false;
						break;
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
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ObjectiveFilter entry.  File: " + filename);
			result = false;
		}
		return result;
	}

	public static bool GetSpawnerData(List<MappingEntry> entries, string fileName, out SpawnerData spawnerData)
	{
		bool flag = true;
		spawnerData = null;
		if (entries != null)
		{
			SpawnerData.ESpawnerTriggerType eSpawnerTriggerType = SpawnerData.ESpawnerTriggerType.None;
			SpawnerData.ESpawnerActivationType spawnerActivationType = SpawnerData.ESpawnerActivationType.ScenarioStart;
			int spawnStartRound = 2;
			bool loopSpawnPattern = true;
			bool shouldCountTowardsKillAllEnemies = false;
			List<int> list = null;
			Dictionary<string, List<SpawnRoundEntry>> dictionary = new Dictionary<string, List<SpawnRoundEntry>>();
			string spawnerHoverNameLoc = string.Empty;
			foreach (MappingEntry entry in entries)
			{
				switch (entry.Key.ToString())
				{
				case "SpawnerTriggerType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "SpawnTriggerType", fileName, out var triggerString))
					{
						SpawnerData.ESpawnerTriggerType eSpawnerTriggerType2 = SpawnerData.SpawnerTriggerTypes.SingleOrDefault((SpawnerData.ESpawnerTriggerType s) => s.ToString() == triggerString);
						if (eSpawnerTriggerType2 != SpawnerData.ESpawnerTriggerType.None)
						{
							eSpawnerTriggerType = eSpawnerTriggerType2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid SpawnTriggerType " + triggerString + " specified in File: " + fileName);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SpawnerActivationType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "SpawnerActivationType", fileName, out var activationString))
					{
						SpawnerData.ESpawnerActivationType eSpawnerActivationType = SpawnerData.SpawnerActivationTypes.SingleOrDefault((SpawnerData.ESpawnerActivationType s) => s.ToString() == activationString);
						if (eSpawnerActivationType != SpawnerData.ESpawnerActivationType.None)
						{
							spawnerActivationType = eSpawnerActivationType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid SpawnerActivationType " + activationString + " specified in File: " + fileName);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SpawnStartRound":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "SpawnStartRound", fileName, out var value3))
					{
						spawnStartRound = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "LoopSpawnPattern":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "LoopSpawnPattern", fileName, out var value4))
					{
						loopSpawnPattern = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ShouldCountTowardsKillAllEnemies":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShouldCountTowardsKillAllEnemies", fileName, out var value))
					{
						shouldCountTowardsKillAllEnemies = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SpawnRoundInterval":
				{
					if (YMLShared.GetIntList(entry.Value, "SpawnRoundInterval", fileName, out var values))
					{
						list = values;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SpawnerHoverNameLoc":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "SpawnerHoverNameLoc", fileName, out var value2))
					{
						spawnerHoverNameLoc = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SpawnRoundEntries":
				{
					if (YMLShared.GetMapping(entry, fileName, out var mapping))
					{
						foreach (MappingEntry spawnRoundMappingEntry in mapping.Entries)
						{
							List<SpawnRoundEntry> list2 = new List<SpawnRoundEntry>();
							if (spawnRoundMappingEntry == null)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null SpawnRoundEntries specified in File: " + fileName);
								flag = false;
							}
							else if (SpawnerData.SpawnerEntryDifficulties.Any((SpawnerData.ESpawnerEntryDifficulty s) => s.ToString() == spawnRoundMappingEntry.Key.ToString()) && !dictionary.Keys.Contains(spawnRoundMappingEntry.Key.ToString()))
							{
								if (YMLShared.GetSequence(spawnRoundMappingEntry, fileName, out var sequence))
								{
									foreach (DataItem entry2 in sequence.Entries)
									{
										if (entry2 is Mapping mapping2)
										{
											if (GetSpawnRoundEntries(mapping2.Entries, fileName, out var spawnRoundEntries))
											{
												list2.AddRange(spawnRoundEntries);
											}
											else
											{
												flag = false;
											}
										}
										else
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected SpawnRoundEntries entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
											flag = false;
										}
									}
								}
								else
								{
									flag = false;
								}
								if (list2.Count > 0)
								{
									dictionary.Add(spawnRoundMappingEntry.Key.ToString(), list2);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid difficulty for SpawnRoundEntries, either already exists or is not a valid difficulty. File: " + fileName);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				}
			}
			if (eSpawnerTriggerType == SpawnerData.ESpawnerTriggerType.None)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Spawner Trigger Type. File: " + fileName);
				flag = false;
			}
			else if (list == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No SpawnRoundInterval specified. File: " + fileName);
				flag = false;
			}
			else if (list.Count != 4)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "SpawnRoundInterval count must be 4. File: " + fileName);
				flag = false;
			}
			else if (dictionary.Count < 4 && !dictionary.ContainsKey("Default"))
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "SpawnRoundEntries must contain a definition for each difficulty or a Default entry. File: " + fileName);
				flag = false;
			}
			if (flag)
			{
				spawnerData = new SpawnerData(eSpawnerTriggerType, spawnerActivationType, spawnStartRound, loopSpawnPattern, shouldCountTowardsKillAllEnemies, list, dictionary, spawnerHoverNameLoc);
			}
		}
		return flag;
	}

	public static bool GetSpawnRoundEntries(List<MappingEntry> entries, string fileName, out List<SpawnRoundEntry> spawnRoundEntries)
	{
		spawnRoundEntries = new List<SpawnRoundEntry>();
		bool flag = true;
		List<string> list = null;
		foreach (MappingEntry entry in entries)
		{
			if (entry.Key.ToString() == "SpawnClasses")
			{
				if (YMLShared.GetStringList(entry.Value, "SpawnClasses", fileName, out var values))
				{
					list = values;
				}
				else
				{
					flag = false;
				}
			}
		}
		if (list == null || list.Count < 4)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ClassName. File: " + fileName);
			flag = false;
		}
		foreach (string spawnClass in list)
		{
			if (spawnClass.ToLower() != "empty" && ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == spawnClass) == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster Class: " + spawnClass + " in File: " + fileName + " ensure name is spaced.");
				flag = false;
			}
		}
		if (flag)
		{
			SpawnRoundEntry item = new SpawnRoundEntry(list);
			spawnRoundEntries.Add(item);
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid SpawnRoundEntry. File: " + fileName);
		}
		return flag;
	}
}
