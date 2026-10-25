using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO;

public class HeroBattleStatDTO
{
	public record BattleStat
	{
		public int Current;

		public Sprite Icon;

		public bool HighScore;

		public string NameKey;
	}

	public string CharacterID;

	public BattleStat Kills;

	public BattleStat DamageDealt;

	public BattleStat DamageReceived;

	public BattleStat HealingDone;

	public BattleStat LootCollected;

	public BattleStat ChestsLooted;

	public BattleStat XPEarned;

	public BattleStat ItemsUsed;

	public IEnumerable<BattleStat> GetBattleStats()
	{
		yield return Kills;
		yield return DamageDealt;
		yield return DamageReceived;
		yield return HealingDone;
		yield return LootCollected;
		yield return ChestsLooted;
		yield return XPEarned;
		yield return ItemsUsed;
	}

	public BattleStat GetStatByCategory(BattleResultDTO.AchievementBadge.BadgeCategory cat)
	{
		return cat switch
		{
			BattleResultDTO.AchievementBadge.BadgeCategory.EnemiesKilled => Kills, 
			BattleResultDTO.AchievementBadge.BadgeCategory.DamageDealt => DamageDealt, 
			BattleResultDTO.AchievementBadge.BadgeCategory.DamageTaken => DamageReceived, 
			BattleResultDTO.AchievementBadge.BadgeCategory.DamageHealed => HealingDone, 
			BattleResultDTO.AchievementBadge.BadgeCategory.LootCollected => LootCollected, 
			BattleResultDTO.AchievementBadge.BadgeCategory.ChestsLooted => ChestsLooted, 
			BattleResultDTO.AchievementBadge.BadgeCategory.ExperienceEarned => XPEarned, 
			BattleResultDTO.AchievementBadge.BadgeCategory.ItemsUsed => ItemsUsed, 
			BattleResultDTO.AchievementBadge.BadgeCategory.Consolation => null, 
			_ => null, 
		};
	}

	public static HeroBattleStatDTO Populate(string characterID, StatsState stats, List<CPlayerStatsScenario> playerStats)
	{
		HeroBattleStatDTO heroBattleStatDTO = new HeroBattleStatDTO();
		CPlayerStatsScenario cPlayerStatsScenario = playerStats.FirstOrDefault((CPlayerStatsScenario scenario) => scenario.CharacterID == characterID);
		heroBattleStatDTO.CharacterID = characterID;
		int num = playerStats.Max((CPlayerStatsScenario s) => s.Kills.Count);
		int num2 = cPlayerStatsScenario?.Kills.Count ?? 0;
		heroBattleStatDTO.Kills = new BattleStat
		{
			Current = num2,
			HighScore = (num2 != 0 && num2 == num),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("EnemiesDefeated"),
			NameKey = "GUI_RESULTS_ENEMIES"
		};
		int num3 = playerStats.Max((CPlayerStatsScenario s) => s.DamageDealt.Sum((CPlayerStatsDamage it) => it.FinalDamageAmount));
		int num4 = cPlayerStatsScenario?.DamageDealt.Sum((CPlayerStatsDamage it) => it.FinalDamageAmount) ?? 0;
		heroBattleStatDTO.DamageDealt = new BattleStat
		{
			Current = num4,
			HighScore = (num4 != 0 && num4 == num3),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("DamageDealt"),
			NameKey = "GUI_RESULTS_DAMAGE_DONE"
		};
		int num5 = playerStats.Max((CPlayerStatsScenario s) => s.DamageReceived.Sum((CPlayerStatsDamage it) => it.FinalDamageAmount));
		int num6 = cPlayerStatsScenario?.DamageReceived.Sum((CPlayerStatsDamage it2) => it2.FinalDamageAmount) ?? 0;
		heroBattleStatDTO.DamageReceived = new BattleStat
		{
			Current = num6,
			HighScore = (num6 != 0 && num6 == num5),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("DamageTaken"),
			NameKey = "GUI_RESULTS_DAMAGE_TAKEN"
		};
		int num7 = playerStats.Max((CPlayerStatsScenario s) => s.Heals.Sum((CPlayerStatsHeal it) => it.HealAmount));
		int num8 = cPlayerStatsScenario?.Heals.Sum((CPlayerStatsHeal it2) => it2.HealAmount) ?? 0;
		heroBattleStatDTO.HealingDone = new BattleStat
		{
			Current = num8,
			HighScore = (num8 != 0 && num8 == num7),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("DamageHealed"),
			NameKey = "GUI_RESULTS_HEALING"
		};
		int num9 = (stats.m_GoldPilesLootedLastScenarioByCharacter.Any() ? stats.m_GoldPilesLootedLastScenarioByCharacter.Max((KeyValuePair<string, int> c) => c.Value) : 0);
		int valueOrDefault = stats.m_GoldPilesLootedLastScenarioByCharacter.GetValueOrDefault(characterID, 0);
		heroBattleStatDTO.LootCollected = new BattleStat
		{
			Current = valueOrDefault,
			HighScore = (valueOrDefault != 0 && valueOrDefault == num9),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("ItemsLooted"),
			NameKey = "GUI_RESULTS_LOOTCOLECTED"
		};
		int num10 = (stats.m_ChestsLootedLastScenarioByCharacter.Any() ? stats.m_ChestsLootedLastScenarioByCharacter.Max((KeyValuePair<string, int> c) => c.Value) : 0);
		int valueOrDefault2 = stats.m_ChestsLootedLastScenarioByCharacter.GetValueOrDefault(characterID, 0);
		heroBattleStatDTO.ChestsLooted = new BattleStat
		{
			Current = valueOrDefault2,
			HighScore = (valueOrDefault2 != 0 && valueOrDefault2 == num10),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("ChestsLooted"),
			NameKey = "GUI_RESULTS_CHESTS"
		};
		int num11 = (stats.m_TotalXPEarnedLastScenarioByCharacter.Any() ? stats.m_TotalXPEarnedLastScenarioByCharacter.Max((KeyValuePair<string, int> c) => c.Value) : 0);
		int valueOrDefault3 = stats.m_TotalXPEarnedLastScenarioByCharacter.GetValueOrDefault(characterID, 0);
		heroBattleStatDTO.XPEarned = new BattleStat
		{
			Current = valueOrDefault3,
			HighScore = (valueOrDefault3 != 0 && valueOrDefault3 == num11),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("XPGained"),
			NameKey = "GUI_RESULTS_XP"
		};
		int num12 = (stats.m_ItemsUsedLastScenarioByCharacter.Any() ? stats.m_ItemsUsedLastScenarioByCharacter.Max((KeyValuePair<string, int> c) => c.Value) : 0);
		int valueOrDefault4 = stats.m_ItemsUsedLastScenarioByCharacter.GetValueOrDefault(characterID, 0);
		heroBattleStatDTO.ItemsUsed = new BattleStat
		{
			Current = valueOrDefault4,
			HighScore = (valueOrDefault4 != 0 && valueOrDefault4 == num12),
			Icon = UIInfoTools.Instance.GetBattleStatIcon("ItemsUsed"),
			NameKey = "GUI_RESULTS_ITEMS_SPENT"
		};
		return heroBattleStatDTO;
	}
}
