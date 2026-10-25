using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class TreasureTableEntry
{
	public const int ArraySize = 8;

	public ETreasureType Type;

	public List<int> Chance;

	public List<int> MinAmount;

	public List<int> MaxAmount;

	public int ItemID = -1;

	public RewardCondition Condition;

	public RewardCondition EnemyCondition;

	public ElementInfusionBoardManager.EElement? Infuse;

	public EEnhancement? Enhancement;

	public PerksYMLData Perk;

	public string UnlockCharacterID;

	public string UnlockAchievement;

	public string UnlockChapter;

	public string UnlockLocation;

	public string UnlockQuest;

	public string CityEvent;

	public string RoadEvent;

	public bool UnlockEnhancer;

	public bool UnlockMerchant;

	public bool UnlockTemple;

	public bool UnlockTrainer;

	public bool UnlockPartyUI;

	public bool UnlockMultiplayer;

	public bool UnlockProsperityItem;

	public bool UnlockProsperityItemStock;

	public bool RandomDesign;

	public ETreasureDistributionType TreasureDistributionType;

	public List<int> Chapter;

	public int ChapterFilter;

	public int SubChapterFilter;

	public string GiveToCharacterID;

	public string ConsumeSlot;

	public Dictionary<string, int> Modifiers;

	public ETreasureDistributionRestrictionType TreasureDistributionRestrictionType;

	public TreasureTableEntry(ETreasureType type, List<int> chance = null, List<int> minAmount = null, List<int> maxAmount = null, int itemID = -1, RewardCondition condition = null, RewardCondition enemyCondition = null, ElementInfusionBoardManager.EElement? infuse = null, EEnhancement? enhancement = null, PerksYMLData perk = null, string unlockAchievement = null, string unlockCharacterID = null, string unlockLocation = null, string unlockQuest = null, ETreasureDistributionType treasureDistributionType = ETreasureDistributionType.Combined, List<int> chapter = null, int chapterFilter = int.MaxValue, int subChapterFilter = int.MaxValue, string giveToCharacterID = null, string cityEvent = null, string roadEvent = null, string consumeSlot = null, Dictionary<string, int> modifiers = null, ETreasureDistributionRestrictionType distributionRestrictionType = ETreasureDistributionRestrictionType.None)
	{
		Type = type;
		Chance = chance ?? new List<int> { 100, 100, 100, 100, 100, 100, 100, 100 };
		MinAmount = null;
		MaxAmount = null;
		TreasureDistributionType = treasureDistributionType;
		ChapterFilter = chapterFilter;
		SubChapterFilter = subChapterFilter;
		GiveToCharacterID = giveToCharacterID;
		TreasureDistributionRestrictionType = distributionRestrictionType;
		switch (type)
		{
		case ETreasureType.Gold:
		case ETreasureType.XP:
		case ETreasureType.Damage:
		case ETreasureType.Prosperity:
		case ETreasureType.EnhancementSlots:
		case ETreasureType.PerkPoint:
		case ETreasureType.Reputation:
		case ETreasureType.PerkCheck:
		case ETreasureType.Discard:
			MinAmount = minAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			MaxAmount = maxAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			break;
		case ETreasureType.Item:
		case ETreasureType.ItemStock:
		case ETreasureType.LoseItem:
		case ETreasureType.ItemBlueprint:
			MinAmount = minAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			MaxAmount = maxAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			ItemID = itemID;
			break;
		case ETreasureType.Condition:
			Condition = condition;
			break;
		case ETreasureType.EnemyCondition:
			EnemyCondition = enemyCondition;
			break;
		case ETreasureType.Infuse:
			Infuse = infuse;
			break;
		case ETreasureType.Enhancement:
			MinAmount = minAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			MaxAmount = maxAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			Enhancement = enhancement;
			break;
		case ETreasureType.Perk:
			Perk = perk;
			break;
		case ETreasureType.UnlockCharacter:
			UnlockCharacterID = unlockCharacterID;
			break;
		case ETreasureType.UnlockLocation:
			UnlockLocation = unlockLocation;
			break;
		case ETreasureType.UnlockQuest:
			UnlockQuest = unlockQuest;
			break;
		case ETreasureType.CityEvent:
			CityEvent = cityEvent;
			break;
		case ETreasureType.RoadEvent:
			RoadEvent = roadEvent;
			break;
		case ETreasureType.ConsumeItem:
			ConsumeSlot = consumeSlot;
			break;
		case ETreasureType.AddModifiers:
			Modifiers = modifiers;
			break;
		case ETreasureType.UnlockEnhancer:
			UnlockEnhancer = true;
			break;
		case ETreasureType.UnlockMerchant:
			UnlockMerchant = true;
			break;
		case ETreasureType.UnlockTemple:
			UnlockTemple = true;
			break;
		case ETreasureType.UnlockTrainer:
			UnlockTrainer = true;
			break;
		case ETreasureType.UnlockPartyUI:
			UnlockPartyUI = true;
			break;
		case ETreasureType.ItemDesign:
			RandomDesign = true;
			break;
		case ETreasureType.UnlockChapter:
			Chapter = chapter;
			break;
		case ETreasureType.UnlockProsperityItem:
			UnlockProsperityItem = true;
			ItemID = itemID;
			MinAmount = minAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			MaxAmount = maxAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			break;
		case ETreasureType.UnlockProsperityItemStock:
			UnlockProsperityItemStock = true;
			ItemID = itemID;
			MinAmount = minAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			MaxAmount = maxAmount ?? new List<int> { 1, 1, 1, 1, 1, 1, 1, 1 };
			break;
		case ETreasureType.UnlockMultiplayer:
			UnlockMultiplayer = true;
			break;
		default:
			throw new Exception("Error: Unsupported Treasure Type: " + type);
		}
		if (Chance.Count != 8)
		{
			throw new Exception("Invalid index size for Chance array");
		}
		if (MinAmount != null && MinAmount.Count != 8)
		{
			throw new Exception("Invalid index size for MinAmount array");
		}
		if (MaxAmount != null && MaxAmount.Count != 8)
		{
			throw new Exception("Invalid index size for MaxAmount array");
		}
		for (int i = 0; i < 8; i++)
		{
			if (MinAmount != null && MaxAmount != null && MinAmount[i] > MaxAmount[i])
			{
				int value = MinAmount[i];
				MinAmount[i] = MaxAmount[i];
				MaxAmount[i] = value;
			}
		}
	}

	public void ChangeAmount(int value)
	{
		for (int i = 0; i < MinAmount.Count; i++)
		{
			MinAmount[i] = Math.Max(0, MinAmount[i] + value);
		}
		for (int j = 0; j < MaxAmount.Count; j++)
		{
			MaxAmount[j] = Math.Max(0, MaxAmount[j] + value);
		}
	}

	public TreasureTableEntry Copy()
	{
		return new TreasureTableEntry(Type, Chance, MinAmount, MaxAmount, ItemID, Condition, EnemyCondition, Infuse, Enhancement, Perk, UnlockAchievement, UnlockCharacterID, UnlockLocation, UnlockQuest, TreasureDistributionType, Chapter, ChapterFilter, SubChapterFilter, GiveToCharacterID, CityEvent, RoadEvent, null, null, TreasureDistributionRestrictionType);
	}
}
