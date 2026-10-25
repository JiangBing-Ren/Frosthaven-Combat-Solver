using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ItemCardYML
{
	public const int MinimumFilesRequired = 1;

	public ConcurrentQueue<ItemCardYMLData> LoadedYML { get; private set; }

	public ItemCardYML()
	{
		LoadedYML = new ConcurrentQueue<ItemCardYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			ItemCardYMLData itemCardYMLData = new ItemCardYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out var success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "PetUnlockedBy":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PetUnlockedBy", fileName, out var value9))
						{
							itemCardYMLData.PetUnlockedBy = CardProcessingShared.GetLookupValue(value9);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ID":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ID", fileName, out var value6))
						{
							itemCardYMLData.ID = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StringID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "StringID", fileName, out var value16))
						{
							itemCardYMLData.StringID = CardProcessingShared.GetLookupValue(value16);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value15))
						{
							itemCardYMLData.Name = CardProcessingShared.GetLookupValue(value15);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Description", fileName, out var value4))
						{
							itemCardYMLData.Description = CardProcessingShared.GetLookupValue(value4);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Art":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Art", fileName, out var value12))
						{
							itemCardYMLData.Art = value12;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TotalInGame":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "TotalInGame", fileName, out var value10))
						{
							itemCardYMLData.TotalInGame = value10;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Cost":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "Cost", fileName, out var value18))
						{
							itemCardYMLData.Cost = value18;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Slot":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Slot", fileName, out var slotString))
						{
							CItem.EItemSlot eItemSlot = CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotString);
							if (eItemSlot != CItem.EItemSlot.None)
							{
								itemCardYMLData.Slot = eItemSlot;
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
					case "Usage":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Usage", fileName, out var usageString))
						{
							CItem.EUsageType eUsageType = CItem.UsageTypes.SingleOrDefault((CItem.EUsageType x) => x.ToString() == usageString);
							if (eUsageType != CItem.EUsageType.None)
							{
								itemCardYMLData.Usage = eUsageType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Usage " + usageString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "FlipItemID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "FlipItemID", fileName, out var value13))
						{
							itemCardYMLData.FlipItemID = CardProcessingShared.GetLookupValue(value13);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "FlipEndOfTurn":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "FlipItemID", fileName, out var value8))
						{
							itemCardYMLData.FlipEndOfTurn = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "FlipItemSide":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "FlipItemSide", fileName, out var stringFlipItemSide))
						{
							CItem.EFlipItemSide eFlipItemSide = CItem.FlipItemSides.SingleOrDefault((CItem.EFlipItemSide x) => x.ToString() == stringFlipItemSide);
							if (eFlipItemSide != CItem.EFlipItemSide.None)
							{
								itemCardYMLData.FlipItemSide = eFlipItemSide;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid FlipItemSide " + stringFlipItemSide + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UsedWhenEquipped":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UsedWhenEquipped", fileName, out var value17))
						{
							itemCardYMLData.UsedWhenEquipped = value17;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PersistentBonus":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PersistentBonus", fileName, out var value14))
						{
							itemCardYMLData.PersistentBonus = value14;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RoundBonus":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "RoundBonus", fileName, out var value11))
						{
							itemCardYMLData.RoundBonus = value11;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Rarity":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Rarity", fileName, out var rarityString))
						{
							CItem.EItemRarity eItemRarity = CItem.ItemRarities.SingleOrDefault((CItem.EItemRarity x) => x.ToString() == rarityString);
							if (eItemRarity != CItem.EItemRarity.None)
							{
								itemCardYMLData.Rarity = eItemRarity;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Rarity " + rarityString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ProsperityRequirement":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "ProsperityRequirement", fileName, out var value3))
						{
							itemCardYMLData.ProsperityRequirement = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Trigger":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Trigger", fileName, out var triggerString))
						{
							CItem.EItemTrigger eItemTrigger = CItem.ItemTriggers.SingleOrDefault((CItem.EItemTrigger x) => x.ToString() == triggerString);
							if (eItemTrigger != CItem.EItemTrigger.None)
							{
								itemCardYMLData.Trigger = eItemTrigger;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Trigger " + triggerString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Consumes":
					{
						if (YMLShared.GetStringList(entry.Value, "Consume", fileName, out var values2))
						{
							foreach (string consumeValue in values2)
							{
								try
								{
									ElementInfusionBoardManager.EElement item = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == consumeValue);
									itemCardYMLData.Consumes.Add(item);
								}
								catch
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid consume element " + consumeValue + " in file " + fileName);
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
					case "Data":
					{
						ItemData itemData = new ItemData();
						if (itemData.ProcessEntry(entry, itemCardYMLData.ID, fileName))
						{
							itemCardYMLData.Data = itemData;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Layout":
					{
						if (YMLShared.GetSequence(entry.Value, "Layout", fileName, out var sequence))
						{
							itemCardYMLData.CardLayout = new CardLayout(sequence, itemCardYMLData.Data.Abilities, null, itemCardYMLData.Name, DiscardType.None, fileName, itemCardYMLData.Data.Overrides);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ValidEquipCharacterClassIDs":
					{
						if (YMLShared.GetStringList(entry.Value, "ValidEquipCharacterClassIDs", fileName, out var values))
						{
							itemCardYMLData.ValidEquipCharacterClassIDs = values.Distinct().ToList();
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Tradeable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Tradeable", fileName, out var value7))
						{
							itemCardYMLData.Tradeable = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IgnoreImpair":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreImpair", fileName, out var value5))
						{
							itemCardYMLData.IgnoreImpair = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MoraleCost":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "MoraleCost", fileName, out var value2))
						{
							itemCardYMLData.MoraleCost = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PermanentlyConsumed":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PermanentlyConsumed", fileName, out var value))
						{
							itemCardYMLData.PermanentlyConsumed = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (itemCardYMLData.ID == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					if (itemCardYMLData.Data.Abilities != null && itemCardYMLData.Data.Abilities.Count > 0)
					{
						itemCardYMLData.ItemType = CItem.EItemType.Ability;
					}
					if (itemCardYMLData.Data.Overrides != null && itemCardYMLData.Data.Overrides.Count > 0)
					{
						if (itemCardYMLData.ItemType == CItem.EItemType.Ability)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Both Abilities and Overrides can't be set on the same item card.  File: " + fileName);
							return false;
						}
						itemCardYMLData.ItemType = CItem.EItemType.Override;
					}
					if (itemCardYMLData.Data.Vars != null && itemCardYMLData.Data.Vars.Count > 0 && itemCardYMLData.ItemType == CItem.EItemType.None)
					{
						itemCardYMLData.ItemType = CItem.EItemType.Vars;
					}
					if (itemCardYMLData.Data.Abilities != null)
					{
						foreach (CAbility ability in itemCardYMLData.Data.Abilities)
						{
							if (!(ability is CAbilityConsumeElement))
							{
								continue;
							}
							foreach (ElementInfusionBoardManager.EElement item2 in (ability as CAbilityConsumeElement).ElementsToConsume)
							{
								itemCardYMLData.Consumes.Add(item2);
							}
						}
					}
					if (flag)
					{
						LoadedYML.Enqueue(itemCardYMLData);
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
}
