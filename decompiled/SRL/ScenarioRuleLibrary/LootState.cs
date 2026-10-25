using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class LootState : ISerializable
{
	public int LootTokens;

	public Dictionary<LootCard.EType, int> DrawnLootCardTypes = new Dictionary<LootCard.EType, int>();

	public Dictionary<LootCard.EType, int> ResourceTypeToCount = new Dictionary<LootCard.EType, int>();

	public List<int> ItemIds = new List<int>();

	public List<int> SoldItemIds = new List<int>();

	public int NumEmptyDeckDraws;

	public int NumEmptyRandomItemDraws;

	public List<uint> LootedItemBlueprintIDs = new List<uint>();

	public uint LootedRandomSideScenarioID;

	public uint LootedScenarioID;

	public int GoldFromEmptyRandomItemDraws;

	public int NumEmptyRandomScenarioDraws;

	public int NumEmptyItemBlueprintDraws;

	public uint PerkChecks;

	public List<string> CampaignStickers = new List<string>();

	public List<string> TriggeredCampaignSections = new List<string>();

	public List<Tuple<string, int>> DelayedCampaignSections = new List<Tuple<string, int>>();

	public Dictionary<string, int> XpFromSources = new Dictionary<string, int>();

	public List<CCondition.ENegativeCondition> NegativeConditions = new List<CCondition.ENegativeCondition>();

	public uint LastLootedBlueprintID
	{
		get
		{
			if (LootedItemBlueprintIDs.Count == 0)
			{
				return 0u;
			}
			return LootedItemBlueprintIDs.Last();
		}
	}

	public LootState()
	{
	}

	public IEnumerable<Reward> GetRewards()
	{
		foreach (int itemId in ItemIds)
		{
			yield return new Reward(itemId, 1, ETreasureType.Item, null, EGiveToCharacterType.Give);
		}
		foreach (uint lootedItemBlueprintID in LootedItemBlueprintIDs)
		{
			yield return new Reward((int)lootedItemBlueprintID, 1, ETreasureType.ItemBlueprint, null, EGiveToCharacterType.Give);
		}
		foreach (int soldItemId in SoldItemIds)
		{
			yield return new Reward(soldItemId, 1, ETreasureType.SoldItem, null, EGiveToCharacterType.Give);
		}
		if (LootedRandomSideScenarioID != 0)
		{
			yield return new Reward(ETreasureType.UnlockLocation, LootedRandomSideScenarioID.ToString());
		}
		if (LootedScenarioID != 0)
		{
			yield return new Reward(ETreasureType.UnlockLocation, LootedScenarioID.ToString());
		}
		int num = GoldFromEmptyRandomItemDraws + ResourceTypeToCount.Where((KeyValuePair<LootCard.EType, int> pair) => pair.Key == LootCard.EType.Money).Sum((KeyValuePair<LootCard.EType, int> pair) => pair.Value);
		if (num > 0)
		{
			yield return new Reward(ETreasureType.Gold, num, ETreasureDistributionType.Combined, null);
		}
		foreach (KeyValuePair<LootCard.EType, int> item in ResourceTypeToCount.OrderBy((KeyValuePair<LootCard.EType, int> p) => p.Key))
		{
			if (item.Key != LootCard.EType.Money && item.Key != LootCard.EType.RandomItem && item.Value != 0)
			{
				yield return new Reward(item.Key, item.Value, ETreasureDistributionType.Combined, null);
			}
		}
		if (NumEmptyDeckDraws > 0)
		{
			yield return new Reward(ETreasureType.EmptyLootDeck, NumEmptyDeckDraws);
		}
		if (NumEmptyRandomScenarioDraws > 0)
		{
			yield return new Reward(ETreasureType.EmptyScenarioDeck, NumEmptyRandomScenarioDraws);
		}
		if (NumEmptyItemBlueprintDraws > 0)
		{
			yield return new Reward(ETreasureType.EmptyBlueprintDeck, NumEmptyItemBlueprintDraws);
		}
		if (NumEmptyRandomItemDraws > 0)
		{
			yield return new Reward(ETreasureType.EmpyRandomItemDeck, GoldFromEmptyRandomItemDraws);
		}
		if (PerkChecks != 0)
		{
			yield return new Reward(ETreasureType.PerkCheck, (int)PerkChecks);
		}
	}

	public void AddItemBlueprint(uint id)
	{
		if (id != 0)
		{
			LootedItemBlueprintIDs.Add(id);
		}
		else
		{
			NumEmptyItemBlueprintDraws++;
		}
	}

	public void AddRandomSideScenario(uint id)
	{
		if (id != 0)
		{
			LootedRandomSideScenarioID = id;
		}
		else
		{
			NumEmptyRandomScenarioDraws++;
		}
	}

	public void AddScenario(uint id)
	{
		if (id != 0)
		{
			LootedScenarioID = id;
		}
	}

	public void GainPerkChecks(uint amount)
	{
		PerkChecks += amount;
	}

	public void GainCampaignSticker(string sticker)
	{
		CampaignStickers.Add(sticker);
	}

	public void AddCampaignSection(string section, int delayWeeks = 0)
	{
		if (delayWeeks > 0)
		{
			DelayedCampaignSections.Add(Tuple.Create(section, delayWeeks));
		}
		else
		{
			TriggeredCampaignSections.Add(section);
		}
	}

	public int AddResource(LootCard.EType resourceType, int amount, bool applyModifiers = false)
	{
		if (applyModifiers && resourceType == LootCard.EType.Money)
		{
			amount *= (int)ScenarioManager.GoldConversion;
		}
		ResourceTypeToCount.TryGetValue(resourceType, out var value);
		ResourceTypeToCount[resourceType] = value + amount;
		return amount;
	}

	public int ResourceAmount(LootCard.EType resourceType)
	{
		if (ResourceTypeToCount.TryGetValue(resourceType, out var value))
		{
			return value;
		}
		return 0;
	}

	public void AddCardTypeDrawn(LootCard.EType type, int amount = 1)
	{
		DrawnLootCardTypes.TryGetValue(type, out var value);
		DrawnLootCardTypes[type] = value + amount;
	}

	public void AddXPFromSource(string source, int amount = 1)
	{
		XpFromSources.TryGetValue(source, out var value);
		XpFromSources[source] = value + amount;
	}

	public void AccumulateLootFrom(LootState otherLoot)
	{
		LootTokens += otherLoot.LootTokens;
		foreach (KeyValuePair<LootCard.EType, int> item in otherLoot.ResourceTypeToCount)
		{
			AddResource(item.Key, item.Value);
		}
		foreach (KeyValuePair<LootCard.EType, int> drawnLootCardType in otherLoot.DrawnLootCardTypes)
		{
			AddCardTypeDrawn(drawnLootCardType.Key, drawnLootCardType.Value);
		}
		foreach (KeyValuePair<string, int> xpFromSource in otherLoot.XpFromSources)
		{
			AddXPFromSource(xpFromSource.Key, xpFromSource.Value);
		}
		foreach (int itemId in otherLoot.ItemIds)
		{
			ItemIds.Add(itemId);
		}
		foreach (int soldItemId in otherLoot.SoldItemIds)
		{
			SoldItemIds.Add(soldItemId);
		}
		NumEmptyDeckDraws += otherLoot.NumEmptyDeckDraws;
		NumEmptyRandomItemDraws += otherLoot.NumEmptyRandomItemDraws;
		NumEmptyItemBlueprintDraws += otherLoot.NumEmptyItemBlueprintDraws;
		NumEmptyRandomScenarioDraws += otherLoot.NumEmptyRandomScenarioDraws;
		LootedItemBlueprintIDs.AddRange(otherLoot.LootedItemBlueprintIDs);
		if (otherLoot.LootedRandomSideScenarioID != 0)
		{
			LootedRandomSideScenarioID = otherLoot.LootedRandomSideScenarioID;
		}
		if (otherLoot.LootedScenarioID != 0)
		{
			LootedScenarioID = otherLoot.LootedScenarioID;
		}
		GoldFromEmptyRandomItemDraws += otherLoot.GoldFromEmptyRandomItemDraws;
		PerkChecks += otherLoot.PerkChecks;
		CampaignStickers.AddRange(otherLoot.CampaignStickers);
		NegativeConditions.AddRange(otherLoot.NegativeConditions);
		DelayedCampaignSections.AddRange(otherLoot.DelayedCampaignSections);
		TriggeredCampaignSections.AddRange(otherLoot.TriggeredCampaignSections);
	}

	public void MigrateFromOldData(byte[] oldData)
	{
		int i = 0;
		for (int num = LootCard.AllTypes.Length; i < num; i++)
		{
			int num2 = oldData[i];
			if (num2 > 0)
			{
				LootCard.EType resourceType = (LootCard.EType)i;
				AddResource(resourceType, num2);
			}
		}
	}

	public LootState(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "LootTokens":
					LootTokens = info.GetInt32("LootTokens");
					break;
				case "ResourceTypeToCount":
					ResourceTypeToCount = (Dictionary<LootCard.EType, int>)info.GetValue("ResourceTypeToCount", typeof(Dictionary<LootCard.EType, int>));
					break;
				case "DrawnLootCardTypes":
					DrawnLootCardTypes = (Dictionary<LootCard.EType, int>)info.GetValue("DrawnLootCardTypes", typeof(Dictionary<LootCard.EType, int>));
					break;
				case "XpFromSources":
					XpFromSources = (Dictionary<string, int>)info.GetValue("XpFromSources", typeof(Dictionary<string, int>));
					break;
				case "ItemIds":
					ItemIds = (List<int>)info.GetValue("ItemIds", typeof(List<int>));
					break;
				case "SoldItemIds":
					SoldItemIds = (List<int>)info.GetValue("SoldItemIds", typeof(List<int>));
					break;
				case "NumEmptyDeckDraws":
					NumEmptyDeckDraws = info.GetInt32("NumEmptyDeckDraws");
					break;
				case "NumEmptyRandomItemDraws":
					NumEmptyRandomItemDraws = info.GetInt32("NumEmptyRandomItemDraws");
					break;
				case "GoldFromEmptyRandomItemDraws":
					GoldFromEmptyRandomItemDraws = info.GetInt32("GoldFromEmptyRandomItemDraws");
					break;
				case "LootedItemBlueprintIDs":
					LootedItemBlueprintIDs = (List<uint>)info.GetValue("LootedItemBlueprintIDs", typeof(List<uint>));
					break;
				case "LootedRandomSideScenarioID":
					LootedRandomSideScenarioID = info.GetUInt32("LootedRandomSideScenarioID");
					break;
				case "LootedScenarioID":
					LootedScenarioID = info.GetUInt32("LootedScenarioID");
					break;
				case "NumEmptyRandomScenarioDraws":
					NumEmptyRandomScenarioDraws = info.GetInt32("NumEmptyRandomScenarioDraws");
					break;
				case "NumEmptyRandomItemBlueprintDraws":
					NumEmptyItemBlueprintDraws = info.GetInt32("NumEmptyRandomItemBlueprintDraws");
					break;
				case "PerkChecks":
					PerkChecks = info.GetUInt32("PerkChecks");
					break;
				case "CampaignStickers":
					CampaignStickers = (List<string>)info.GetValue("CampaignStickers", typeof(List<string>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "DelayedCampaignSections":
					DelayedCampaignSections = (List<Tuple<string, int>>)info.GetValue("DelayedCampaignSections", typeof(List<Tuple<string, int>>));
					break;
				case "TriggeredCampaignSections":
					TriggeredCampaignSections = (List<string>)info.GetValue("TriggeredCampaignSections", typeof(List<string>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize LootState entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw;
			}
		}
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("LootTokens", LootTokens);
		info.AddValue("ResourceTypeToCount", ResourceTypeToCount);
		info.AddValue("ItemIds", ItemIds);
		info.AddValue("SoldItemIds", SoldItemIds);
		info.AddValue("NumEmptyDeckDraws", NumEmptyDeckDraws);
		info.AddValue("NumEmptyRandomItemDraws", NumEmptyRandomItemDraws);
		info.AddValue("GoldFromEmptyRandomItemDraws", GoldFromEmptyRandomItemDraws);
		info.AddValue("LootedItemBlueprintIDs", LootedItemBlueprintIDs);
		info.AddValue("LootedRandomSideScenarioID", LootedRandomSideScenarioID);
		info.AddValue("LootedScenarioID", LootedScenarioID);
		info.AddValue("NumEmptyRandomScenarioDraws", NumEmptyRandomScenarioDraws);
		info.AddValue("NumEmptyRandomItemBlueprintDraws", NumEmptyItemBlueprintDraws);
		info.AddValue("PerkChecks", PerkChecks);
		info.AddValue("CampaignStickers", CampaignStickers);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("DelayedCampaignSections", DelayedCampaignSections);
		info.AddValue("TriggeredCampaignSections", TriggeredCampaignSections);
		info.AddValue("DrawnLootCardTypes", DrawnLootCardTypes);
		info.AddValue("XpFromSources", XpFromSources);
	}
}
