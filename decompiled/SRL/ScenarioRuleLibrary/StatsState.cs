using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class StatsState : ISerializable
{
	public int m_ChestsLootedLastScenario;

	public int m_GoldPilesLootedLastScenario;

	public int m_TotalDamageDoneLastScenario;

	public int m_TotalDamageTakenLastScenario;

	public int m_TotalHealingDoneLastScenario;

	public int m_TotalXPEarnedLastScenario;

	public int m_ItemsUsedLastScenario;

	public int m_ScenarioGoldLastScenario;

	public int m_ScenarioChestsLastScenario;

	public int m_EnemiesKilledLastScenario;

	public Dictionary<string, int> m_ChestsLootedLastScenarioByCharacter;

	public Dictionary<string, int> m_GoldPilesLootedLastScenarioByCharacter;

	public Dictionary<string, int> m_TotalXPEarnedLastScenarioByCharacter;

	public Dictionary<string, int> m_ItemsUsedLastScenarioByCharacter;

	public Dictionary<string, CPlayerStatsScenario> m_PlayerStats;

	public CPlayerStatsScenario m_HeroSummonStats;

	public CPlayerStatsScenario m_MonsterStats;

	public Dictionary<string, int> m_RandomItemBlueprintsLootedLastScenarioByCharacter;

	public List<CTrapActorStats> m_TrapActorStats;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ChestsLootedLastScenario", m_ChestsLootedLastScenario);
		info.AddValue("GoldPilesLootedLastScenario", m_GoldPilesLootedLastScenario);
		info.AddValue("TotalDamageDoneLastScenario", m_TotalDamageDoneLastScenario);
		info.AddValue("TotalDamageTakenLastScenario", m_TotalDamageTakenLastScenario);
		info.AddValue("TotalHealingDoneLastScenario", m_TotalHealingDoneLastScenario);
		info.AddValue("TotalXPEarnedLastScenario", m_TotalXPEarnedLastScenario);
		info.AddValue("ItemsUsedLastScenario", m_ItemsUsedLastScenario);
		info.AddValue("ScenarioGoldLastScenario", m_ScenarioGoldLastScenario);
		info.AddValue("ScenarioChestsLastScenario", m_ScenarioChestsLastScenario);
		info.AddValue("EnemiesKilledLastScenario", m_EnemiesKilledLastScenario);
		info.AddValue("ChestsLootedLastScenarioByCharacter", m_ChestsLootedLastScenarioByCharacter);
		info.AddValue("GoldPilesLootedLastScenarioByCharacter", m_GoldPilesLootedLastScenarioByCharacter);
		info.AddValue("TotalXPEarnedLastScenarioByCharacter", m_TotalXPEarnedLastScenarioByCharacter);
		info.AddValue("ItemsUsedLastScenarioByCharacter", m_ItemsUsedLastScenarioByCharacter);
		info.AddValue("PlayerStats", m_PlayerStats);
		info.AddValue("HeroSummonStats", m_HeroSummonStats);
		info.AddValue("MonsterStats", m_MonsterStats);
		info.AddValue("m_RandomItemBlueprintsLootedLastScenarioByCharacter", m_RandomItemBlueprintsLootedLastScenarioByCharacter);
		info.AddValue("m_TrapActorStats", m_TrapActorStats);
	}

	public StatsState(List<PlayerState> players)
	{
		m_PlayerStats = new Dictionary<string, CPlayerStatsScenario>();
		if (players != null)
		{
			foreach (PlayerState player in players)
			{
				if (CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == player.ClassID) != null)
				{
					m_PlayerStats.Add(player.ClassID, new CPlayerStatsScenario(player.ClassID));
				}
				else
				{
					Log.ErrorWithReport("Unable to find character with class ID " + player.ClassID);
				}
			}
		}
		m_HeroSummonStats = new CPlayerStatsScenario((string)null);
		m_MonsterStats = new CPlayerStatsScenario((string)null);
		m_ChestsLootedLastScenarioByCharacter = new Dictionary<string, int>();
		m_GoldPilesLootedLastScenarioByCharacter = new Dictionary<string, int>();
		m_TotalXPEarnedLastScenarioByCharacter = new Dictionary<string, int>();
		m_ItemsUsedLastScenarioByCharacter = new Dictionary<string, int>();
		m_RandomItemBlueprintsLootedLastScenarioByCharacter = new Dictionary<string, int>();
		m_TrapActorStats = new List<CTrapActorStats>();
	}

	public StatsState(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ChestsLootedLastScenario":
					m_ChestsLootedLastScenario = info.GetInt32("ChestsLootedLastScenario");
					break;
				case "GoldPilesLootedLastScenario":
					m_GoldPilesLootedLastScenario = info.GetInt32("GoldPilesLootedLastScenario");
					break;
				case "TotalDamageDoneLastScenario":
					m_TotalDamageDoneLastScenario = info.GetInt32("TotalDamageDoneLastScenario");
					break;
				case "TotalDamageTakenLastScenario":
					m_TotalDamageTakenLastScenario = info.GetInt32("TotalDamageTakenLastScenario");
					break;
				case "TotalHealingDoneLastScenario":
					m_TotalHealingDoneLastScenario = info.GetInt32("TotalHealingDoneLastScenario");
					break;
				case "TotalXPEarnedLastScenario":
					m_TotalXPEarnedLastScenario = info.GetInt32("TotalXPEarnedLastScenario");
					break;
				case "ItemsUsedLastScenario":
					m_ItemsUsedLastScenario = info.GetInt32("ItemsUsedLastScenario");
					break;
				case "ScenarioGoldLastScenario":
					m_ScenarioGoldLastScenario = info.GetInt32("ScenarioGoldLastScenario");
					break;
				case "ScenarioChestsLastScenario":
					m_ScenarioChestsLastScenario = info.GetInt32("ScenarioChestsLastScenario");
					break;
				case "EnemiesKilledLastScenario":
					m_EnemiesKilledLastScenario = info.GetInt32("EnemiesKilledLastScenario");
					break;
				case "ChestsLootedLastScenarioByCharacter":
					m_ChestsLootedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("ChestsLootedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "GoldPilesLootedLastScenarioByCharacter":
					m_GoldPilesLootedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("GoldPilesLootedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "TotalXPEarnedLastScenarioByCharacter":
					m_TotalXPEarnedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("TotalXPEarnedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "ItemsUsedLastScenarioByCharacter":
					m_ItemsUsedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("ItemsUsedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "PlayerStats":
					m_PlayerStats = (Dictionary<string, CPlayerStatsScenario>)info.GetValue("PlayerStats", typeof(Dictionary<string, CPlayerStatsScenario>));
					break;
				case "HeroSummonStats":
					m_HeroSummonStats = (CPlayerStatsScenario)info.GetValue("HeroSummonStats", typeof(CPlayerStatsScenario));
					break;
				case "MonsterStats":
					m_MonsterStats = (CPlayerStatsScenario)info.GetValue("MonsterStats", typeof(CPlayerStatsScenario));
					break;
				case "m_RandomItemBlueprintsLootedLastScenarioByCharacter":
					m_RandomItemBlueprintsLootedLastScenarioByCharacter = (Dictionary<string, int>)info.GetValue("m_RandomItemBlueprintsLootedLastScenarioByCharacter", typeof(Dictionary<string, int>));
					break;
				case "m_TrapActorStats":
					m_TrapActorStats = (List<CTrapActorStats>)info.GetValue("m_TrapActorStats", typeof(List<CTrapActorStats>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize StatsDataStorage entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public StatsState(StatsState original)
	{
		m_ChestsLootedLastScenario = original.m_ChestsLootedLastScenario;
		m_GoldPilesLootedLastScenario = original.m_GoldPilesLootedLastScenario;
		m_TotalDamageDoneLastScenario = original.m_TotalDamageDoneLastScenario;
		m_TotalDamageTakenLastScenario = original.m_TotalDamageTakenLastScenario;
		m_TotalHealingDoneLastScenario = original.m_TotalHealingDoneLastScenario;
		m_TotalXPEarnedLastScenario = original.m_TotalXPEarnedLastScenario;
		m_ItemsUsedLastScenario = original.m_ItemsUsedLastScenario;
		m_ScenarioGoldLastScenario = original.m_ScenarioGoldLastScenario;
		m_ScenarioChestsLastScenario = original.m_ScenarioChestsLastScenario;
		m_EnemiesKilledLastScenario = original.m_EnemiesKilledLastScenario;
		if (original.m_ChestsLootedLastScenarioByCharacter != null)
		{
			m_ChestsLootedLastScenarioByCharacter = new Dictionary<string, int>(original.m_ChestsLootedLastScenarioByCharacter);
		}
		if (original.m_GoldPilesLootedLastScenarioByCharacter != null)
		{
			m_GoldPilesLootedLastScenarioByCharacter = new Dictionary<string, int>(original.m_GoldPilesLootedLastScenarioByCharacter);
		}
		if (original.m_TotalXPEarnedLastScenarioByCharacter != null)
		{
			m_TotalXPEarnedLastScenarioByCharacter = new Dictionary<string, int>(original.m_TotalXPEarnedLastScenarioByCharacter);
		}
		if (original.m_ItemsUsedLastScenarioByCharacter != null)
		{
			m_ItemsUsedLastScenarioByCharacter = new Dictionary<string, int>(original.m_ItemsUsedLastScenarioByCharacter);
		}
		if (original.m_PlayerStats != null)
		{
			m_PlayerStats = new Dictionary<string, CPlayerStatsScenario>(original.m_PlayerStats.Count);
			foreach (KeyValuePair<string, CPlayerStatsScenario> playerStat in original.m_PlayerStats)
			{
				CPlayerStatsScenario value = new CPlayerStatsScenario(playerStat.Value);
				m_PlayerStats.Add(playerStat.Key, value);
			}
		}
		if (original.m_HeroSummonStats != null)
		{
			m_HeroSummonStats = new CPlayerStatsScenario(original.m_HeroSummonStats);
		}
		if (original.m_MonsterStats != null)
		{
			m_MonsterStats = new CPlayerStatsScenario(original.m_MonsterStats);
		}
		if (original.m_RandomItemBlueprintsLootedLastScenarioByCharacter != null)
		{
			m_RandomItemBlueprintsLootedLastScenarioByCharacter = new Dictionary<string, int>(original.m_RandomItemBlueprintsLootedLastScenarioByCharacter);
		}
		if (original.m_TrapActorStats != null)
		{
			m_TrapActorStats = new List<CTrapActorStats>(original.m_TrapActorStats);
		}
	}
}
