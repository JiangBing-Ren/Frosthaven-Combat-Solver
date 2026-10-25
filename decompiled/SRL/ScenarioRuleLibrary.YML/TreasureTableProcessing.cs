using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary;

namespace ScenarioRuleLibrary.YML;

public class TreasureTableProcessing
{
	public static List<RewardGroup> RollTreasureTables(SharedLibrary.Random rng, List<TreasureTable> treasureTables, int level, int chapter, int subChapter)
	{
		if (treasureTables == null || treasureTables.Count == 0)
		{
			return new List<RewardGroup>();
		}
		List<RewardGroup> list = new List<RewardGroup>();
		if (level < 0)
		{
			level = 0;
		}
		else if (level >= 8)
		{
			level = 7;
		}
		foreach (TreasureTable treasureTable in treasureTables)
		{
			List<Reward> list2 = new List<Reward>();
			foreach (TreasureTableEntry entry in treasureTable.Entries)
			{
				try
				{
					if (((rng != null) ? (rng.Next(100) + 1) : 0) <= entry.Chance[level])
					{
						Reward reward = GetReward(rng, entry, level);
						if (reward != null)
						{
							list2.Add(reward);
						}
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport(ex.Message + "\n" + ex.StackTrace);
				}
			}
			bool flag = false;
			foreach (TreasureTableGroup group in treasureTable.Groups)
			{
				try
				{
					for (int i = 0; i < Math.Max(1, group.RepeatCount); i++)
					{
						if (((rng != null) ? (rng.Next(100) + 1) : 0) <= group.GroupChance[level])
						{
							list2.AddRange(ProcessGroup(rng, group, level, chapter, subChapter));
							flag = true;
						}
					}
				}
				catch (Exception ex2)
				{
					Log.ErrorWithReport("An exception occurred processing a Treasure Group in table" + treasureTable.Name + ".\n" + ex2.Message + "\n" + ex2.StackTrace);
				}
			}
			if (flag)
			{
				list2.Reverse();
			}
			list.Add(new RewardGroup(list2, treasureTable.Name));
		}
		return list;
	}

	private static List<Reward> ProcessGroup(SharedLibrary.Random rng, TreasureTableGroup group, int level, int chapter, int subChapter)
	{
		List<Reward> list = new List<Reward>();
		if (group.AndLogic)
		{
			foreach (TreasureTableEntry entry in group.Entries)
			{
				if (((rng != null) ? (rng.Next(100) + 1) : 0) <= entry.Chance[level])
				{
					Reward reward = GetReward(rng, entry, level, group.GiveToCharacterID, group.GiveToCharacterType, group.GiveToCharacterRequirementType, group.GiveToCharacterRequirementCheckID);
					if (reward != null)
					{
						list.Add(reward);
					}
				}
			}
			foreach (TreasureTableGroup group2 in group.Groups)
			{
				for (int i = 0; i < Math.Max(1, group.RepeatCount); i++)
				{
					if (((rng != null) ? (rng.Next(100) + 1) : 0) <= group.GroupChance[level])
					{
						list.AddRange(ProcessGroup(rng, group2, level, chapter, subChapter));
					}
				}
			}
		}
		else
		{
			int maxValue = group.Entries.Sum((TreasureTableEntry x) => x.Chance[level]) + group.Groups.Sum((TreasureTableGroup x) => x.GroupChance[level] * x.RepeatCount);
			int num = rng?.Next(maxValue) ?? 0;
			int num2 = 0;
			bool flag = false;
			foreach (TreasureTableEntry entry2 in group.Entries)
			{
				num2 += entry2.Chance[level];
				if (num <= num2)
				{
					Reward reward2 = GetReward(rng, entry2, level, group.GiveToCharacterID, group.GiveToCharacterType, group.GiveToCharacterRequirementType, group.GiveToCharacterRequirementCheckID);
					if (reward2 != null)
					{
						list.Add(reward2);
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				foreach (TreasureTableGroup group3 in group.Groups)
				{
					for (int num3 = 0; num3 < Math.Max(1, group.RepeatCount); num3++)
					{
						num2 += group3.GroupChance[level];
						if (num <= num2)
						{
							list.AddRange(ProcessGroup(rng, group3, level, chapter, subChapter));
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	private static Reward GetReward(SharedLibrary.Random rng, TreasureTableEntry entry, int scenarioLevel, string giveToCharacterID = null, EGiveToCharacterType giveToCharacterType = EGiveToCharacterType.None, EGiveToCharacterRequirement giveToCharacterRequirementType = EGiveToCharacterRequirement.None, string giveToCharacterRequirementCheckID = null)
	{
		int num = 1;
		if (entry.MinAmount != null && entry.MaxAmount != null && entry.MinAmount[scenarioLevel] <= entry.MaxAmount[scenarioLevel])
		{
			num = rng?.Next(entry.MinAmount[scenarioLevel], entry.MaxAmount[scenarioLevel] + 1) ?? (entry.MaxAmount[scenarioLevel] + 1);
		}
		if (entry.GiveToCharacterID != null)
		{
			giveToCharacterID = entry.GiveToCharacterID;
		}
		switch (entry.Type)
		{
		case ETreasureType.Gold:
		case ETreasureType.XP:
			if (num == 0)
			{
				return null;
			}
			return new Reward(entry.Type, num, entry.TreasureDistributionType, giveToCharacterID, entry.TreasureDistributionRestrictionType);
		case ETreasureType.Item:
			return new Reward(entry.ItemID, num, entry.Type, giveToCharacterID, giveToCharacterType, giveToCharacterRequirementType, giveToCharacterRequirementCheckID);
		case ETreasureType.ItemStock:
			return new Reward(entry.ItemID, num, entry.Type, giveToCharacterID, giveToCharacterType);
		case ETreasureType.ItemBlueprint:
			return new Reward(entry.ItemID, num, entry.Type, giveToCharacterID, giveToCharacterType);
		case ETreasureType.LoseItem:
			return new Reward(entry.ItemID, num, entry.Type, giveToCharacterID, giveToCharacterType)
			{
				TreasureDistributionType = entry.TreasureDistributionType
			};
		case ETreasureType.Condition:
			if (entry.Condition != null)
			{
				return new Reward(new List<RewardCondition> { entry.Condition }, forEnemy: false, entry.TreasureDistributionType);
			}
			break;
		case ETreasureType.EnemyCondition:
			if (entry.EnemyCondition != null)
			{
				return new Reward(new List<RewardCondition> { entry.EnemyCondition }, forEnemy: true);
			}
			break;
		case ETreasureType.Enhancement:
			if (entry.Enhancement.HasValue)
			{
				return new Reward(entry.Enhancement.Value);
			}
			break;
		case ETreasureType.Perk:
			if (entry.Perk != null)
			{
				return new Reward(entry.Perk);
			}
			break;
		case ETreasureType.Infuse:
			if (entry.Infuse.HasValue)
			{
				return new Reward(new List<ElementInfusionBoardManager.EElement> { entry.Infuse.Value });
			}
			break;
		case ETreasureType.Damage:
		case ETreasureType.Discard:
			if (num <= 0)
			{
				return null;
			}
			return new Reward(entry.Type, num, entry.TreasureDistributionType, giveToCharacterID);
		case ETreasureType.Prosperity:
		case ETreasureType.Reputation:
		case ETreasureType.PerkCheck:
			return new Reward(entry.Type, num, entry.TreasureDistributionType, giveToCharacterID);
		case ETreasureType.UnlockMerchant:
			return new Reward(entry.Type, entry.UnlockMerchant);
		case ETreasureType.UnlockEnhancer:
			return new Reward(entry.Type, entry.UnlockEnhancer);
		case ETreasureType.UnlockTrainer:
			return new Reward(entry.Type, entry.UnlockTrainer);
		case ETreasureType.UnlockTemple:
			return new Reward(entry.Type, entry.UnlockTemple);
		case ETreasureType.UnlockLocation:
			return new Reward(entry.Type, entry.UnlockLocation);
		case ETreasureType.UnlockQuest:
			return new Reward(entry.Type, entry.UnlockQuest);
		case ETreasureType.UnlockChapter:
			if (entry.Chapter == null)
			{
				return null;
			}
			return new Reward(entry.Type, entry.Chapter[0], entry.Chapter[1]);
		case ETreasureType.UnlockCharacter:
			if (entry.UnlockCharacterID != null)
			{
				return new Reward(entry.UnlockCharacterID);
			}
			break;
		case ETreasureType.EnhancementSlots:
			if (num <= 0)
			{
				return null;
			}
			return new Reward(entry.Type, num, entry.TreasureDistributionType, giveToCharacterID);
		case ETreasureType.UnlockPartyUI:
			return new Reward(entry.Type, entry.UnlockPartyUI);
		case ETreasureType.UnlockMultiplayer:
			return new Reward(entry.Type, entry.UnlockMultiplayer);
		case ETreasureType.PerkPoint:
			if (num <= 0)
			{
				return null;
			}
			return new Reward(entry.Type, num, entry.TreasureDistributionType, giveToCharacterID);
		case ETreasureType.UnlockProsperityItem:
			return new Reward(entry.ItemID, num, entry.Type, giveToCharacterID, giveToCharacterType, giveToCharacterRequirementType, giveToCharacterRequirementCheckID);
		case ETreasureType.UnlockProsperityItemStock:
			return new Reward(entry.ItemID, num, entry.Type, giveToCharacterID, giveToCharacterType);
		case ETreasureType.CityEvent:
			return new Reward(entry.Type, entry.CityEvent);
		case ETreasureType.RoadEvent:
			return new Reward(entry.Type, entry.RoadEvent);
		case ETreasureType.ConsumeItem:
			return new Reward(entry.Type, entry.ConsumeSlot, entry.TreasureDistributionType);
		case ETreasureType.AddModifiers:
			return new Reward(entry.Type, entry.Modifiers, entry.TreasureDistributionType);
		default:
			Log.ErrorWithReport("Error: Invalid TreasureType " + entry.Type);
			break;
		}
		Log.ErrorWithReport("Error: Invalid Reward of type  " + entry.Type);
		return null;
	}
}
