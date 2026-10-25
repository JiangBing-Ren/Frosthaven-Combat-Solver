using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AStar;
using Libs.GameConsole;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CScenario
{
	public bool SetEnemyHealthToMaxOnPlay;

	public bool EnemyMaxHealthBasedOnPartyLevel;

	public SEventActorFinishedScenario.EScenarioResult CurrentScenarioResult;

	private string m_ID;

	private string m_Description;

	[StateDump(Skip = true)]
	private List<CPlayerActor> m_InitialPlayers = new List<CPlayerActor>();

	[StateDump(Skip = true)]
	private List<CEnemyActor> m_InitialEnemies = new List<CEnemyActor>();

	[StateDump(Skip = true)]
	private List<CHeroSummonActor> m_InitialHeroSummons = new List<CHeroSummonActor>();

	[StateDump(Skip = true)]
	private List<CEnemyActor> m_InitialAllyMonsters = new List<CEnemyActor>();

	[StateDump(Skip = true)]
	private List<CEnemyActor> m_InitialEnemy2Monsters = new List<CEnemyActor>();

	[StateDump(Skip = true)]
	private List<CEnemyActor> m_InitialNeutralMonsters = new List<CEnemyActor>();

	[StateDump(Skip = true)]
	private List<CObjectActor> m_InitialObjects = new List<CObjectActor>();

	private List<CPlayerActor> m_Players = new List<CPlayerActor>();

	private List<CEnemyActor> m_Enemies = new List<CEnemyActor>();

	private List<CHeroSummonActor> m_HeroSummons = new List<CHeroSummonActor>();

	private List<CEnemyActor> m_AllyMonsters = new List<CEnemyActor>();

	private List<CEnemyActor> m_Enemy2Monsters = new List<CEnemyActor>();

	private List<CEnemyActor> m_NeutralMonsters = new List<CEnemyActor>();

	private List<CMap> m_Maps = new List<CMap>();

	private CVectorInt3 m_PositiveSpaceOffset;

	[ConsoleVariable(Alias = "alerted_by_adjacent_enemy")]
	public static bool AlertedByAdjacentEnemy;

	[ConsoleVariable(Alias = "alerted_by_enemy_in_same_room")]
	public static bool AlertedByEnemyInSameRoom;

	public string ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			m_ID = value;
		}
	}

	public string Description => m_Description;

	public List<CPlayerActor> PlayerActors => m_Players;

	public List<CPlayerActor> ExhaustedPlayers { get; private set; }

	public List<CEnemyActor> Enemies => m_Enemies;

	public List<CEnemyActor> DeadEnemies { get; private set; }

	public List<CHeroSummonActor> HeroSummons => m_HeroSummons;

	public List<CHeroSummonActor> DeadHeroSummons { get; private set; }

	public List<CEnemyActor> AllyMonsters => m_AllyMonsters;

	public List<CEnemyActor> DeadAllyMonsters { get; private set; }

	public List<CEnemyActor> Enemy2Monsters => m_Enemy2Monsters;

	public List<CEnemyActor> DeadEnemy2Monsters { get; private set; }

	public List<CEnemyActor> NeutralMonsters => m_NeutralMonsters;

	public List<CEnemyActor> DeadNeutralMonsters { get; private set; }

	public List<CMap> Maps => m_Maps;

	public CVectorInt3 PositiveSpaceOffset
	{
		get
		{
			return m_PositiveSpaceOffset;
		}
		set
		{
			m_PositiveSpaceOffset = value;
		}
	}

	public List<CPlayerActor> InitialPlayers => m_InitialPlayers;

	public List<CEnemyActor> InitialEnemies => m_InitialEnemies;

	public List<CHeroSummonActor> InitialHeroSummons => m_InitialHeroSummons;

	public List<CEnemyActor> InitialAllyMonsters => m_InitialAllyMonsters;

	public List<CEnemyActor> InitialEnemy2Monsters => m_InitialEnemy2Monsters;

	public List<CEnemyActor> InitialNeutralMonsters => m_InitialNeutralMonsters;

	public List<CObjectActor> InitialObjects => m_InitialObjects;

	public List<CObjectActor> Objects => m_Objects;

	public List<CObjectActor> DeadObjects { get; private set; }

	public List<CPlayerActor> AllPlayers => PlayerActors.Concat(ExhaustedPlayers).ToList();

	public int PartyGold => AllPlayers.Sum((CPlayerActor s) => s.Gold) + HeroSummons.Where((CHeroSummonActor it) => it.IsCompanionSummon).Sum((CHeroSummonActor it) => it.Gold);

	public List<CEnemyActor> AllEnemies => Enemies.Concat(DeadEnemies).ToList();

	public List<CHeroSummonActor> AllHeroSummons => HeroSummons.Concat(DeadHeroSummons).ToList();

	public List<CEnemyActor> AllAllyMonsters => AllyMonsters.Concat(DeadAllyMonsters).ToList();

	public List<CEnemyActor> AllEnemy2Monsters => Enemy2Monsters.Concat(DeadEnemy2Monsters).ToList();

	public List<CEnemyActor> AllNeutralMonsters => NeutralMonsters.Concat(DeadNeutralMonsters).ToList();

	public List<CEnemyActor> AllAliveMonsters => Enemies.Concat(AllyMonsters).Concat(Enemy2Monsters).Concat(NeutralMonsters)
		.ToList();

	public List<CEnemyActor> AllMonsters => AllEnemies.Concat(AllAllyMonsters).Concat(AllEnemy2Monsters).Concat(AllNeutralMonsters)
		.ToList();

	public List<CObjectActor> AllObjects => Objects.Concat(DeadObjects).ToList();

	public List<CEnemyActor> AllEnemyMonsters => Enemies.Concat(Enemy2Monsters).ToList();

	public List<CEnemyActor> AllEnemyMonstersAndObjects => Enemies.Concat(Enemy2Monsters).Concat(Objects).ToList();

	public List<CActor> AllActors
	{
		get
		{
			try
			{
				List<CActor> list = new List<CActor>();
				list.AddRange(AllPlayers);
				list.AddRange(AllEnemies);
				list.AddRange(AllHeroSummons);
				list.AddRange(AllAllyMonsters);
				list.AddRange(AllEnemy2Monsters);
				list.AddRange(AllNeutralMonsters);
				list.AddRange(AllObjects);
				return list;
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Error accessing all actors list.\n" + ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}
	}

	public List<CActor> AllActorsThreadSafe
	{
		get
		{
			List<CActor> allActors = CActiveBonus.ActiveBonusCalculationScope.GetAllActors();
			if (allActors != null)
			{
				return allActors;
			}
			return AllActors;
		}
	}

	public List<CActor> AllAliveActors
	{
		get
		{
			List<CActor> list = new List<CActor>();
			list.AddRange(PlayerActors);
			list.AddRange(Enemies);
			list.AddRange(HeroSummons);
			list.AddRange(AllyMonsters);
			list.AddRange(Enemy2Monsters);
			list.AddRange(NeutralMonsters);
			list.AddRange(Objects);
			return list;
		}
	}

	public List<CActor> AllAliveActorsThreadSafe
	{
		get
		{
			List<CActor> allAliveActors = CActiveBonus.ActiveBonusCalculationScope.GetAllAliveActors();
			if (allAliveActors != null)
			{
				return allAliveActors;
			}
			return AllAliveActors;
		}
	}

	public List<CActor> AllDeadActors
	{
		get
		{
			List<CActor> list = new List<CActor>();
			list.AddRange(ExhaustedPlayers);
			list.AddRange(DeadEnemies);
			list.AddRange(DeadHeroSummons);
			list.AddRange(DeadAllyMonsters);
			list.AddRange(DeadEnemy2Monsters);
			list.AddRange(DeadNeutralMonsters);
			list.AddRange(DeadObjects);
			return list;
		}
	}

	public Dictionary<string, int> GetAllEnemyCountByType
	{
		get
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (EnemyState item in m_Maps.SelectMany((CMap m) => m.Monsters).ToList())
			{
				if (!dictionary.ContainsKey(item.ClassID))
				{
					dictionary[item.ClassID] = 0;
				}
				dictionary[item.ClassID]++;
			}
			return dictionary;
		}
	}

	public List<CObjectActor> m_Objects { get; private set; } = new List<CObjectActor>();

	public CScenario(string id, string name, CVectorInt3 positiveSpaceOffset)
	{
		m_ID = id;
		m_Description = name;
		m_PositiveSpaceOffset = positiveSpaceOffset;
		CurrentScenarioResult = SEventActorFinishedScenario.EScenarioResult.None;
		ExhaustedPlayers = new List<CPlayerActor>();
		DeadEnemies = new List<CEnemyActor>();
		DeadHeroSummons = new List<CHeroSummonActor>();
		DeadAllyMonsters = new List<CEnemyActor>();
		DeadEnemy2Monsters = new List<CEnemyActor>();
		DeadNeutralMonsters = new List<CEnemyActor>();
		DeadObjects = new List<CObjectActor>();
	}

	public void Reset(List<CMap> newMaps = null)
	{
		m_InitialPlayers.Clear();
		m_InitialEnemies.Clear();
		m_InitialHeroSummons.Clear();
		m_InitialAllyMonsters.Clear();
		m_InitialEnemy2Monsters.Clear();
		m_InitialNeutralMonsters.Clear();
		m_InitialNeutralMonsters.Clear();
		m_InitialObjects.Clear();
		m_Objects.Clear();
		m_Players.Clear();
		m_Enemies.Clear();
		m_HeroSummons.Clear();
		m_AllyMonsters.Clear();
		m_Enemy2Monsters.Clear();
		m_NeutralMonsters.Clear();
		ExhaustedPlayers.Clear();
		DeadEnemies.Clear();
		DeadAllyMonsters.Clear();
		DeadEnemy2Monsters.Clear();
		DeadNeutralMonsters.Clear();
		DeadObjects.Clear();
		m_Maps.Clear();
		if (newMaps != null)
		{
			m_Maps.AddRange(newMaps);
		}
	}

	public void SortLists()
	{
		ExhaustedPlayers.Sort(GameState.CompareActorsByGUID);
		Enemies.Sort(GameState.CompareActorsByGUID);
		DeadEnemies.Sort(GameState.CompareActorsByGUID);
		HeroSummons.Sort(GameState.CompareActorsByGUID);
		DeadHeroSummons.Sort(GameState.CompareActorsByGUID);
		AllyMonsters.Sort(GameState.CompareActorsByGUID);
		DeadAllyMonsters.Sort(GameState.CompareActorsByGUID);
		Enemy2Monsters.Sort(GameState.CompareActorsByGUID);
		DeadEnemy2Monsters.Sort(GameState.CompareActorsByGUID);
		NeutralMonsters.Sort(GameState.CompareActorsByGUID);
		DeadNeutralMonsters.Sort(GameState.CompareActorsByGUID);
		Objects.Sort(GameState.CompareActorsByGUID);
		DeadObjects.Sort(GameState.CompareActorsByGUID);
	}

	public void AddMap(CMap map)
	{
		m_Maps.Add(map);
	}

	public CPlayerActor AddPlayer(PlayerState playerState, bool initial)
	{
		CCharacterClass cCharacterClass = CharacterClassManager.Find(playerState.ClassID);
		if (cCharacterClass != null)
		{
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (Tuple<int, int> selectedAbilityCardIDAndInstanceID in playerState.AbilityDeck.SelectedAbilityCardIDsAndInstanceIDs)
			{
				list.Add(CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard x) => x.ID == selectedAbilityCardIDAndInstanceID.Item1 && x.CardInstanceID == selectedAbilityCardIDAndInstanceID.Item2));
			}
			List<CAbilityCard> ownedAbilityCards = playerState.AbilityDeck.OwnedAbilityCardIDs.Select((int id) => CharacterClassManager.AllAbilityCardInstances.SingleOrDefault((CAbilityCard x) => x.ID == id)).ToList();
			cCharacterClass.SetHand(list, ownedAbilityCards);
			CPlayerActor cPlayerActor = new CPlayerActor(new Point(playerState.Location), cCharacterClass, playerState.Health, playerState.MaxHealth, playerState.Level, playerState.ActorGuid, playerState.ChosenModelIndex, playerState.CharacterName, playerState.TimeTokens, playerState.Agility, playerState.Form, playerState.PrevForm, playerState.PendingForm, playerState.NextSummonID, playerState.TrophyTokens, playerState.ResonanceTokens);
			cPlayerActor.CompletedMasteries = playerState.CompletedMasteries;
			if (playerState.PhasedOut)
			{
				cPlayerActor.PhasedOut = true;
				cPlayerActor.CauseOfDeath = playerState.CauseOfDeath;
			}
			if (playerState.IsDead && !playerState.PhasedOut)
			{
				ScenarioManager.Scenario.ExhaustedPlayers.Add(cPlayerActor);
				cPlayerActor.CauseOfDeath = playerState.CauseOfDeath;
			}
			else if (initial)
			{
				if (cPlayerActor.Class.ID == "MetalMosaicID")
				{
					cPlayerActor.Pressure = CPlayerActor.EPressure.Regular;
				}
				m_InitialPlayers.Add(cPlayerActor);
			}
			else
			{
				m_Players.Add(cPlayerActor);
			}
			if (playerState.ActorGuid == null)
			{
				playerState.ActorGuid = cPlayerActor.ActorGuid;
				playerState.IsRevealed = true;
				playerState.Save(initial);
			}
			if (!initial)
			{
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
				CActiveBonus.RefreshAllSongActiveBonuses(cPlayerActor);
				CActiveBonus.RefreshAllAuraActiveBonuses();
				CActiveBonus.RefreshOverhealActiveBonuses();
			}
			return cPlayerActor;
		}
		Log.ErrorWithReport("Cannot find Hero: " + playerState.ClassID);
		return null;
	}

	private void CalcHealth(ActorState actorState, int monsterClassHealth)
	{
		if (actorState.ActorGuid == null)
		{
			int num = monsterClassHealth;
			if (actorState.MinInitialMaxHealth != -1)
			{
				num = Math.Max(num, actorState.MinInitialMaxHealth);
			}
			if (actorState.Health == 0)
			{
				actorState.Health = num;
			}
			if (actorState.MaxHealth == 0)
			{
				actorState.MaxHealth = num;
			}
			return;
		}
		int num2 = monsterClassHealth;
		if (actorState.MinInitialMaxHealth != -1)
		{
			num2 = Math.Max(num2, actorState.MinInitialMaxHealth);
		}
		if (EnemyMaxHealthBasedOnPartyLevel)
		{
			actorState.MaxHealth = num2;
		}
		if (SetEnemyHealthToMaxOnPlay)
		{
			actorState.Health = actorState.MaxHealth;
		}
		actorState.Health = Math.Min(actorState.Health, actorState.MaxHealth);
	}

	public CEnemyActor AddEnemy(EnemyState actorState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(actorState.ClassID);
		if (cMonsterClass == null)
		{
			Log.ErrorWithReport("Cannot find Monster: " + actorState.ClassID);
			return null;
		}
		if (!actorState.IsRevealed & resetHealth)
		{
			int monsterClassHealth = cMonsterClass.Health();
			CalcHealth(actorState, monsterClassHealth);
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || actorState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					Log.Warning("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = actorState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(actorState.Location), cMonsterClass, CActor.EType.Enemy, actorState.IsSummon, actorState.SummonerGuid, actorState.ActorGuid, actorState.Health, actorState.MaxHealth, actorState.Level, num, initial, actorState.ChosenModelIndex, actorState.CustomLocKey);
		}
		catch
		{
			Log.ErrorWithReport("Could not create Enemy Monster: " + actorState.ClassID);
			return null;
		}
		if (actorState.IsDead && !actorState.PhasedOut)
		{
			ScenarioManager.Scenario.DeadEnemies.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialEnemies.Add(cEnemyActor);
		}
		else
		{
			m_Enemies.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				Log.SimpleInfo(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		actorState.IsRevealed = true;
		if (actorState.ActorGuid == null)
		{
			actorState.ActorGuid = cEnemyActor.ActorGuid;
			actorState.Save(initial);
		}
		if (!initial)
		{
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CEnemyActor AddAllyMonster(EnemyState actorState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(actorState.ClassID);
		if (cMonsterClass == null)
		{
			Log.ErrorWithReport("Cannot find Monster: " + actorState.ClassID);
			return null;
		}
		if (!actorState.IsRevealed & resetHealth)
		{
			int monsterClassHealth = cMonsterClass.Health();
			CalcHealth(actorState, monsterClassHealth);
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || actorState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					Log.Warning("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = actorState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(actorState.Location), cMonsterClass, actorState.Type, actorState.IsSummon, actorState.SummonerGuid, actorState.ActorGuid, actorState.Health, actorState.MaxHealth, actorState.Level, num, initial, actorState.ChosenModelIndex, actorState.CustomLocKey);
		}
		catch
		{
			Log.ErrorWithReport("Could not create Ally Monster: " + actorState.ClassID);
			return null;
		}
		if (actorState.IsDead && !actorState.PhasedOut)
		{
			ScenarioManager.Scenario.DeadAllyMonsters.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialAllyMonsters.Add(cEnemyActor);
		}
		else
		{
			m_AllyMonsters.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				Log.SimpleInfo(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		actorState.IsRevealed = true;
		if (actorState.ActorGuid == null)
		{
			actorState.ActorGuid = cEnemyActor.ActorGuid;
			actorState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CEnemyActor AddEnemy2Monster(EnemyState actorState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(actorState.ClassID);
		if (cMonsterClass == null)
		{
			Log.ErrorWithReport("Cannot find Monster: " + actorState.ClassID);
			return null;
		}
		if (!actorState.IsRevealed & resetHealth)
		{
			int monsterClassHealth = cMonsterClass.Health();
			CalcHealth(actorState, monsterClassHealth);
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || actorState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					Log.Warning("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = actorState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(actorState.Location), cMonsterClass, CActor.EType.Enemy2, actorState.IsSummon, actorState.SummonerGuid, actorState.ActorGuid, actorState.Health, actorState.MaxHealth, actorState.Level, num, initial, actorState.ChosenModelIndex, actorState.CustomLocKey);
		}
		catch
		{
			Log.ErrorWithReport("Could not create Enemy 2 Monster: " + actorState.ClassID);
			return null;
		}
		if (actorState.IsDead && !actorState.PhasedOut)
		{
			ScenarioManager.Scenario.DeadEnemy2Monsters.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialEnemy2Monsters.Add(cEnemyActor);
		}
		else
		{
			m_Enemy2Monsters.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				Log.SimpleInfo(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		actorState.IsRevealed = true;
		if (actorState.ActorGuid == null)
		{
			actorState.ActorGuid = cEnemyActor.ActorGuid;
			actorState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public CEnemyActor AddNeutralMonster(EnemyState actorState, bool initial, bool noIDRegen = false, bool fromSpawnerAtRoundStart = false, bool resetHealth = true)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(actorState.ClassID);
		if (cMonsterClass == null)
		{
			Log.ErrorWithReport("Cannot find Monster: " + actorState.ClassID);
			return null;
		}
		if (!actorState.IsRevealed & resetHealth)
		{
			int monsterClassHealth = cMonsterClass.Health();
			CalcHealth(actorState, monsterClassHealth);
		}
		CEnemyActor cEnemyActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || actorState.ID == 0)
			{
				num = cMonsterClass.GetNextID();
				if (num == -1)
				{
					Log.Warning("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = actorState.ID;
			}
			cEnemyActor = new CEnemyActor(new Point(actorState.Location), cMonsterClass, CActor.EType.Neutral, actorState.IsSummon, actorState.SummonerGuid, actorState.ActorGuid, actorState.Health, actorState.MaxHealth, actorState.Level, num, initial, actorState.ChosenModelIndex, actorState.CustomLocKey);
		}
		catch
		{
			Log.ErrorWithReport("Could not create Neutral Monster: " + actorState.ClassID);
			return null;
		}
		if (actorState.IsDead && !actorState.PhasedOut)
		{
			ScenarioManager.Scenario.DeadNeutralMonsters.Add(cEnemyActor);
		}
		else if (initial)
		{
			m_InitialNeutralMonsters.Add(cEnemyActor);
		}
		else
		{
			m_NeutralMonsters.Add(cEnemyActor);
		}
		if (!initial && !fromSpawnerAtRoundStart)
		{
			cMonsterClass.SelectRoundAbilityCard();
			if (cMonsterClass.RoundAbilityCard != null)
			{
				Log.SimpleInfo(cMonsterClass.LocKey + " selected card " + cMonsterClass.RoundAbilityCard.Name);
			}
		}
		actorState.IsRevealed = true;
		if (actorState.ActorGuid == null)
		{
			actorState.ActorGuid = cEnemyActor.ActorGuid;
			actorState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cEnemyActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		return cEnemyActor;
	}

	public void OnFinishedHeroSummon(HeroSummonState heroSummonState)
	{
		CHeroSummonActor cHeroSummonActor = HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == heroSummonState.ActorGuid);
		if (cHeroSummonActor == null)
		{
			return;
		}
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			scenarioScript.OnHeroSummonFinished(cHeroSummonActor);
		}
	}

	public CHeroSummonActor AddHeroSummon(HeroSummonState heroSummonState, bool initial, bool noIDRegen = false, CAbility ability = null)
	{
		if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == heroSummonState.ClassID) == null)
		{
			Log.ErrorWithReport("Cannot find YML for Hero Summon: " + heroSummonState.ClassID);
			return null;
		}
		CHeroSummonClass cHeroSummonClass = CharacterClassManager.FindHeroSummonClass(heroSummonState.ClassID);
		if (cHeroSummonClass == null)
		{
			Log.ErrorWithReport("Cannot find Class for Hero Summon: " + heroSummonState.ClassID);
			return null;
		}
		CHeroSummonActor cHeroSummonActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || heroSummonState.ID == 0)
			{
				num = PlayerActors.First((CPlayerActor a) => a.ActorGuid == heroSummonState.Summoner).GetNextSummonId(cHeroSummonClass);
				if (num == -1)
				{
					Log.Warning("Maximum number of hero summon of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = heroSummonState.ID;
			}
			float baseHealthModifier = 1f;
			if (ScenarioManager.CurrentScenarioState != null)
			{
				baseHealthModifier = ScenarioManager.CurrentScenarioState.SummonHealthModifier;
			}
			int enhancementHealth = 0;
			if (ability?.AbilityEnhancements != null && ability.AbilityEnhancements.Find((CEnhancement e) => e.Enhancement == EEnhancement.SummonHP) != null)
			{
				enhancementHealth = 1;
			}
			cHeroSummonActor = new CHeroSummonActor(new Point(heroSummonState.Location), heroSummonState.Summoner, heroSummonState.Level, num, cHeroSummonClass, heroSummonState.HeroSummonData, heroSummonState.ActorGuid, heroSummonState.ChosenModelIndex, baseHealthModifier, enhancementHealth);
		}
		catch
		{
			Log.ErrorWithReport("Could not create Hero Summon: " + heroSummonState.ClassID);
			return null;
		}
		if (heroSummonState.IsDead && !heroSummonState.PhasedOut)
		{
			DeadHeroSummons.Add(cHeroSummonActor);
		}
		else
		{
			if (initial)
			{
				m_InitialHeroSummons.Add(cHeroSummonActor);
			}
			else
			{
				m_HeroSummons.Add(cHeroSummonActor);
			}
			CPlayerActor.LogHeroSummonMoveChanged(cHeroSummonActor);
		}
		if (heroSummonState.ActorGuid == null)
		{
			heroSummonState.ActorGuid = cHeroSummonActor.ActorGuid;
			heroSummonState.IsRevealed = true;
			heroSummonState.Save(initial);
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			CActiveBonus.RefreshAllSongActiveBonuses(cHeroSummonActor);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			CActiveBonus.RefreshOverhealActiveBonuses();
		}
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			scenarioScript.OnHeroSummonSpawned(cHeroSummonActor);
		}
		return cHeroSummonActor;
	}

	public CObjectActor AddObject(ObjectState objectState, bool initial, bool noIDRegen = false, bool dummyObjectForProp = false)
	{
		CObjectClass cObjectClass = MonsterClassManager.FindObjectClass(objectState.ClassID);
		if (cObjectClass == null)
		{
			Log.ErrorWithReport("Cannot find Monster: " + objectState.ClassID);
			return null;
		}
		if (!objectState.IsRevealed)
		{
			if ((objectState.ActorGuid == null) | dummyObjectForProp)
			{
				if (objectState.Health == 0)
				{
					objectState.Health = cObjectClass.Health();
				}
				if (objectState.MaxHealth == 0)
				{
					objectState.MaxHealth = cObjectClass.Health();
				}
			}
			else
			{
				if (EnemyMaxHealthBasedOnPartyLevel)
				{
					objectState.MaxHealth = cObjectClass.Health();
				}
				if (SetEnemyHealthToMaxOnPlay)
				{
					objectState.Health = objectState.MaxHealth;
				}
				objectState.Health = Math.Min(objectState.Health, objectState.MaxHealth);
			}
		}
		CObjectActor cObjectActor;
		try
		{
			int num;
			if ((initial && !noIDRegen) || objectState.ID == 0)
			{
				num = cObjectClass.GetNextID();
				if (num == -1)
				{
					Log.Warning("Maximum number of enemies of this type has been reached.  Will not spawn another.");
					return null;
				}
			}
			else
			{
				num = objectState.ID;
			}
			cObjectActor = new CObjectActor(new Point(objectState.Location), cObjectClass, objectState.Type, objectState.ActorGuid, objectState.Health, objectState.MaxHealth, objectState.Level, num, initial, objectState.ChosenModelIndex);
		}
		catch
		{
			Log.ErrorWithReport("Could not create Enemy Monster: " + objectState.ClassID);
			return null;
		}
		if (objectState.IsDead && !objectState.PhasedOut)
		{
			ScenarioManager.Scenario.DeadObjects.Add(cObjectActor);
		}
		else if (initial)
		{
			m_InitialObjects.Add(cObjectActor);
		}
		else
		{
			m_Objects.Add(cObjectActor);
		}
		if (!initial)
		{
			cObjectClass.SelectRoundAbilityCard();
		}
		objectState.IsRevealed = true;
		if (objectState.ActorGuid == null)
		{
			objectState.ActorGuid = cObjectActor.ActorGuid;
			objectState.Save(initial);
		}
		if (objectState.IsAttachedToProp)
		{
			CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp p) => p.PropGuid == objectState.PropGuidAttachedTo) ?? ScenarioManager.CurrentScenarioState.DestroyedProps.FirstOrDefault((CObjectProp p) => p.PropGuid == objectState.PropGuidAttachedTo) ?? ScenarioManager.CurrentScenarioState.ActivatedProps.FirstOrDefault((CObjectProp p) => p.PropGuid == objectState.PropGuidAttachedTo);
			if (cObjectProp != null)
			{
				cObjectActor.SetAttachedToProp(cObjectProp);
				cObjectProp.SetActorAttachedAtRuntime(cObjectActor);
			}
		}
		if (!initial)
		{
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
		return cObjectActor;
	}

	public bool AnyMapsHiddenAndStillHaveEnemies()
	{
		foreach (CMap map in m_Maps)
		{
			if (!map.Revealed && map.Monsters.Count() > 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<CActor> FindAllActorsWithActiveBonusClass(CClass classToFind)
	{
		List<CPlayerActor> list = m_Players.FindAll((CPlayerActor x) => x.ActiveBonusClass() == classToFind);
		List<CHeroSummonActor> list2 = m_HeroSummons.FindAll((CHeroSummonActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list3 = m_Enemies.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list4 = m_AllyMonsters.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list5 = m_Enemy2Monsters.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CEnemyActor> list6 = m_NeutralMonsters.FindAll((CEnemyActor x) => x.ActiveBonusClass() == classToFind);
		List<CObjectActor> list7 = m_Objects.FindAll((CObjectActor x) => x.ActiveBonusClass() == classToFind);
		List<CActor> list8 = new List<CActor>();
		foreach (CPlayerActor item in list)
		{
			list8.Add(item);
		}
		foreach (CHeroSummonActor item2 in list2)
		{
			list8.Add(item2);
		}
		foreach (CEnemyActor item3 in list3)
		{
			list8.Add(item3);
		}
		foreach (CEnemyActor item4 in list4)
		{
			list8.Add(item4);
		}
		foreach (CEnemyActor item5 in list5)
		{
			list8.Add(item5);
		}
		foreach (CEnemyActor item6 in list6)
		{
			list8.Add(item6);
		}
		foreach (CObjectActor item7 in list7)
		{
			list8.Add(item7);
		}
		return list8;
	}

	public CActor FindActorAt(int x, int y, CActor actorToIgnore = null, bool includeInitial = false, bool isRetry = false)
	{
		return FindActorAt(new Point(x, y), actorToIgnore, includeInitial, isRetry);
	}

	public CActor FindActorAt(Point position, CActor actorToIgnore = null, bool includeInitial = false, bool isRetry = false)
	{
		try
		{
			CActor cActor = m_Players.Find((CPlayerActor x) => IsValidFindActor(x));
			if (cActor == null)
			{
				cActor = m_Enemies.Find((CEnemyActor x) => IsValidFindActor(x));
			}
			if (cActor == null)
			{
				cActor = m_HeroSummons.Find((CHeroSummonActor x) => IsValidFindActor(x));
			}
			if (cActor == null)
			{
				cActor = m_AllyMonsters.Find((CEnemyActor x) => IsValidFindActor(x));
			}
			if (cActor == null)
			{
				cActor = m_Enemy2Monsters.Find((CEnemyActor x) => IsValidFindActor(x));
			}
			if (cActor == null)
			{
				cActor = m_NeutralMonsters.Find((CEnemyActor x) => IsValidFindActor(x));
			}
			if (cActor == null)
			{
				cActor = m_Objects.Find((CObjectActor x) => IsValidFindActor(x));
			}
			if (cActor == null)
			{
				CTile propTile = new CTile();
				CObjectObstacle objectObstacle = CObjectProp.FindPropWithPathingBlocker(position, ref propTile);
				if (objectObstacle != null)
				{
					cActor = m_Objects.Find((CObjectActor x) => x.AttachedProp?.PropGuid == objectObstacle.PropGuid);
				}
			}
			if (includeInitial)
			{
				if (cActor == null)
				{
					cActor = m_InitialPlayers.Find((CPlayerActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					cActor = m_InitialEnemies.Find((CEnemyActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					cActor = m_InitialHeroSummons.Find((CHeroSummonActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					cActor = m_InitialAllyMonsters.Find((CEnemyActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					cActor = m_InitialEnemy2Monsters.Find((CEnemyActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					cActor = m_InitialNeutralMonsters.Find((CEnemyActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					cActor = m_InitialObjects.Find((CObjectActor x) => IsValidFindActor(x));
				}
				if (cActor == null)
				{
					CTile propTile2 = new CTile();
					CObjectObstacle objectObstacle2 = CObjectProp.FindPropWithPathingBlocker(position, ref propTile2);
					if (objectObstacle2 != null)
					{
						cActor = m_InitialObjects.Find((CObjectActor x) => x.AttachedProp?.PropGuid == objectObstacle2.PropGuid);
					}
				}
			}
			return cActor;
		}
		catch (Exception ex)
		{
			if (!isRetry)
			{
				Log.Info("Caught exception at CScenario.FindActorAt - Sleeping Thread");
				Thread.Sleep(50);
				return FindActorAt(position, actorToIgnore, includeInitial, isRetry: true);
			}
			string text = "Error occurred trying to FindActorAt position: " + position.ToString() + ".\n";
			if (ex is InvalidOperationException)
			{
				text += "Invalid Operation Exception, a threading issue.\n";
			}
			if (m_Players == null)
			{
				text += "m_Players list is null.\n";
			}
			if (m_Players.Any((CPlayerActor x) => x == null))
			{
				text += "m_Players contains a null actor.\n";
			}
			if (m_Players.Any((CPlayerActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Players contains an actor with a null array index.\n";
			}
			if (m_Enemies == null)
			{
				text += "m_Enemies list is null.\n";
			}
			if (m_Enemies.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemies contains a null actor.\n";
			}
			if (m_Enemies.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemies contains an actor with a null array index.\n";
			}
			if (m_HeroSummons == null)
			{
				text += "m_HeroSummons list is null.\n";
			}
			if (m_HeroSummons.Any((CHeroSummonActor x) => x == null))
			{
				text += "m_HeroSummons contains a null actor.\n";
			}
			if (m_HeroSummons.Any((CHeroSummonActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_HeroSummons contains an actor with a null array index.\n";
			}
			if (m_AllyMonsters == null)
			{
				text += "m_AllyMonsters list is null.\n";
			}
			if (m_AllyMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_AllyMonsters contains a null actor.\n";
			}
			if (m_AllyMonsters.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_AllyMonsters contains an actor with a null array index.\n";
			}
			if (m_Enemy2Monsters == null)
			{
				text += "m_Enemy2Monsters list is null.\n";
			}
			if (m_Enemy2Monsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemy2Monsters contains a null actor.\n";
			}
			if (m_Enemy2Monsters.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemy2Monsters contains an actor with a null array index.\n";
			}
			if (m_NeutralMonsters == null)
			{
				text += "m_NeutralMonsters list is null.\n";
			}
			if (m_NeutralMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_NeutralMonsters contains a null actor.\n";
			}
			if (m_NeutralMonsters.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_NeutralMonsters contains an actor with a null array index.\n";
			}
			if (m_Objects == null)
			{
				text += "m_Objects list is null.\n";
			}
			if (m_Objects.Any((CObjectActor x) => x == null))
			{
				text += "m_Objects contains a null actor.\n";
			}
			if (m_Objects.Any((CObjectActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Objects contains an actor with a null array index.\n";
			}
			if (includeInitial)
			{
				if (m_InitialPlayers == null)
				{
					text += "m_InitialPlayers list is null.\n";
				}
				if (m_InitialPlayers.Any((CPlayerActor x) => x == null))
				{
					text += "m_InitialPlayers contains a null actor.\n";
				}
				if (m_InitialPlayers.Any((CPlayerActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialPlayers contains an actor with a null array index.\n";
				}
				if (m_InitialEnemies == null)
				{
					text += "m_InitialEnemies list is null.\n";
				}
				if (m_InitialEnemies.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemies contains a null actor.\n";
				}
				if (m_InitialEnemies.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemies contains an actor with a null array index.\n";
				}
				if (m_InitialHeroSummons == null)
				{
					text += "m_InitialHeroSummons list is null.\n";
				}
				if (m_InitialHeroSummons.Any((CHeroSummonActor x) => x == null))
				{
					text += "m_InitialHeroSummons contains a null actor.\n";
				}
				if (m_InitialHeroSummons.Any((CHeroSummonActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialHeroSummons contains an actor with a null array index.\n";
				}
				if (m_InitialAllyMonsters == null)
				{
					text += "m_InitialAllyMonsters list is null.\n";
				}
				if (m_InitialAllyMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialAllyMonsters contains a null actor.\n";
				}
				if (m_InitialAllyMonsters.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialAllyMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialEnemy2Monsters == null)
				{
					text += "m_InitialEnemy2Monsters list is null.\n";
				}
				if (m_InitialEnemy2Monsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemy2Monsters contains a null actor.\n";
				}
				if (m_InitialEnemy2Monsters.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemy2Monsters contains an actor with a null array index.\n";
				}
				if (m_InitialNeutralMonsters == null)
				{
					text += "m_InitialNeutralMonsters list is null.\n";
				}
				if (m_InitialNeutralMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialNeutralMonsters contains a null actor.\n";
				}
				if (m_InitialNeutralMonsters.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialNeutralMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialObjects == null)
				{
					text += "m_InitialObjects list is null.\n";
				}
				if (m_InitialObjects.Any((CObjectActor x) => x == null))
				{
					text += "m_InitialObjects contains a null actor.\n";
				}
				if (m_InitialObjects.Any((CObjectActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialObjects contains an actor with a null array index.\n";
				}
			}
			Log.ErrorWithReport(text + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
		bool IsValidFindActor(CActor actor)
		{
			if ((actor.RealArrayIndex == position || (actor.MultiHexArrayIndexes != null && actor.MultiHexArrayIndexes.Contains(position))) && !actor.PhasedOut)
			{
				return actor != actorToIgnore;
			}
			return false;
		}
	}

	public List<CActor> FindActorsAt(Point position, CActor actorToIgnore = null, bool includeInitial = false, bool isRetry = false, bool useRealPositions = true)
	{
		try
		{
			List<CActor> list = new List<CActor>();
			CTile propTile = new CTile();
			CObjectObstacle objectObstacle = CObjectProp.FindPropWithPathingBlocker(position, ref propTile);
			list.AddRange(m_Players.FindAll((CPlayerActor x) => IsValidFindActor(x)));
			list.AddRange(m_Enemies.FindAll((CEnemyActor x) => IsValidFindActor(x)));
			list.AddRange(m_HeroSummons.FindAll((CHeroSummonActor x) => IsValidFindActor(x)));
			list.AddRange(m_AllyMonsters.FindAll((CEnemyActor x) => IsValidFindActor(x)));
			list.AddRange(m_Enemy2Monsters.FindAll((CEnemyActor x) => IsValidFindActor(x)));
			list.AddRange(m_NeutralMonsters.FindAll((CEnemyActor x) => IsValidFindActor(x)));
			list.AddRange(m_Objects.FindAll((CObjectActor x) => IsValidFindActor(x)));
			if (objectObstacle != null)
			{
				list.AddRange(m_Objects.FindAll((CObjectActor x) => x.AttachedProp?.PropGuid == objectObstacle.PropGuid));
			}
			if (includeInitial)
			{
				list.AddRange(m_InitialPlayers.FindAll((CPlayerActor x) => IsValidFindActor(x)));
				list.AddRange(m_InitialEnemies.FindAll((CEnemyActor x) => IsValidFindActor(x)));
				list.AddRange(m_InitialHeroSummons.FindAll((CHeroSummonActor x) => IsValidFindActor(x)));
				list.AddRange(m_InitialAllyMonsters.FindAll((CEnemyActor x) => IsValidFindActor(x)));
				list.AddRange(m_InitialEnemy2Monsters.FindAll((CEnemyActor x) => IsValidFindActor(x)));
				list.AddRange(m_InitialNeutralMonsters.FindAll((CEnemyActor x) => IsValidFindActor(x)));
				list.AddRange(m_InitialObjects.FindAll((CObjectActor x) => IsValidFindActor(x)));
				if (objectObstacle != null)
				{
					list.AddRange(m_InitialObjects.FindAll((CObjectActor x) => x.AttachedProp?.PropGuid == objectObstacle.PropGuid));
				}
			}
			return list;
		}
		catch (Exception ex)
		{
			if (!isRetry)
			{
				Log.Info("Caught exception at CScenario.FindActorsAt - Sleeping Thread");
				Thread.Sleep(50);
				return FindActorsAt(position, actorToIgnore, includeInitial, isRetry: true);
			}
			string text = "Error occurred trying to FindActorsAt position: " + position.ToString() + ".\n";
			if (ex is InvalidOperationException)
			{
				text += "Invalid Operation Exception, a threading issue.\n";
			}
			if (m_Players == null)
			{
				text += "m_Players list is null.\n";
			}
			if (m_Players.Any((CPlayerActor x) => x == null))
			{
				text += "m_Players contains a null actor.\n";
			}
			if (m_Players.Any((CPlayerActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Players contains an actor with a null array index.\n";
			}
			if (m_Enemies == null)
			{
				text += "m_Enemies list is null.\n";
			}
			if (m_Enemies.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemies contains a null actor.\n";
			}
			if (m_Enemies.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemies contains an actor with a null array index.\n";
			}
			if (m_HeroSummons == null)
			{
				text += "m_HeroSummons list is null.\n";
			}
			if (m_HeroSummons.Any((CHeroSummonActor x) => x == null))
			{
				text += "m_HeroSummons contains a null actor.\n";
			}
			if (m_HeroSummons.Any((CHeroSummonActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_HeroSummons contains an actor with a null array index.\n";
			}
			if (m_AllyMonsters == null)
			{
				text += "m_AllyMonsters list is null.\n";
			}
			if (m_AllyMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_AllyMonsters contains a null actor.\n";
			}
			if (m_AllyMonsters.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_AllyMonsters contains an actor with a null array index.\n";
			}
			if (m_Enemy2Monsters == null)
			{
				text += "m_Enemy2Monsters list is null.\n";
			}
			if (m_Enemy2Monsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_Enemy2Monsters contains a null actor.\n";
			}
			if (m_Enemy2Monsters.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Enemy2Monsters contains an actor with a null array index.\n";
			}
			if (m_NeutralMonsters == null)
			{
				text += "m_NeutralMonsters list is null.\n";
			}
			if (m_NeutralMonsters.Any((CEnemyActor x) => x == null))
			{
				text += "m_NeutralMonsters contains a null actor.\n";
			}
			if (m_NeutralMonsters.Any((CEnemyActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_NeutralMonsters contains an actor with a null array index.\n";
			}
			if (m_Objects == null)
			{
				text += "m_Objects list is null.\n";
			}
			if (m_Objects.Any((CObjectActor x) => x == null))
			{
				text += "m_Objects contains a null actor.\n";
			}
			if (m_Objects.Any((CObjectActor x) =>
			{
				_ = x.ArrayIndex;
				return false;
			}))
			{
				text += "m_Objects contains an actor with a null array index.\n";
			}
			if (includeInitial)
			{
				if (m_InitialPlayers == null)
				{
					text += "m_InitialPlayers list is null.\n";
				}
				if (m_InitialPlayers.Any((CPlayerActor x) => x == null))
				{
					text += "m_InitialPlayers contains a null actor.\n";
				}
				if (m_InitialPlayers.Any((CPlayerActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialPlayers contains an actor with a null array index.\n";
				}
				if (m_InitialEnemies == null)
				{
					text += "m_InitialEnemies list is null.\n";
				}
				if (m_InitialEnemies.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemies contains a null actor.\n";
				}
				if (m_InitialEnemies.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemies contains an actor with a null array index.\n";
				}
				if (m_InitialHeroSummons == null)
				{
					text += "m_InitialHeroSummons list is null.\n";
				}
				if (m_InitialHeroSummons.Any((CHeroSummonActor x) => x == null))
				{
					text += "m_InitialHeroSummons contains a null actor.\n";
				}
				if (m_InitialHeroSummons.Any((CHeroSummonActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialHeroSummons contains an actor with a null array index.\n";
				}
				if (m_InitialAllyMonsters == null)
				{
					text += "m_InitialAllyMonsters list is null.\n";
				}
				if (m_InitialAllyMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialAllyMonsters contains a null actor.\n";
				}
				if (m_InitialAllyMonsters.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialAllyMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialEnemy2Monsters == null)
				{
					text += "m_InitialEnemy2Monsters list is null.\n";
				}
				if (m_InitialEnemy2Monsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialEnemy2Monsters contains a null actor.\n";
				}
				if (m_InitialEnemy2Monsters.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialEnemy2Monsters contains an actor with a null array index.\n";
				}
				if (m_InitialNeutralMonsters == null)
				{
					text += "m_InitialNeutralMonsters list is null.\n";
				}
				if (m_InitialNeutralMonsters.Any((CEnemyActor x) => x == null))
				{
					text += "m_InitialNeutralMonsters contains a null actor.\n";
				}
				if (m_InitialNeutralMonsters.Any((CEnemyActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialNeutralMonsters contains an actor with a null array index.\n";
				}
				if (m_InitialObjects == null)
				{
					text += "m_InitialObjects list is null.\n";
				}
				if (m_InitialObjects.Any((CObjectActor x) => x == null))
				{
					text += "m_InitialObjects contains a null actor.\n";
				}
				if (m_InitialObjects.Any((CObjectActor x) =>
				{
					_ = x.ArrayIndex;
					return false;
				}))
				{
					text += "m_InitialObjects contains an actor with a null array index.\n";
				}
			}
			Log.ErrorWithReport(text + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
		bool IsValidFindActor(CActor actor)
		{
			if ((actor.GetArrayIndex(useRealPositions) == position || (actor.MultiHexArrayIndexes != null && actor.MultiHexArrayIndexes.Contains(position))) && !actor.PhasedOut && !actor.Deactivated)
			{
				return actor != actorToIgnore;
			}
			return false;
		}
	}

	public CPlayerActor FindPlayerAt(Point position)
	{
		return m_Players.Find((CPlayerActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindEnemyAt(Point position)
	{
		return m_Enemies.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CHeroSummonActor FindHeroSummonAt(Point position)
	{
		return m_HeroSummons.Find((CHeroSummonActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindAllyMonsterAt(Point position)
	{
		return m_AllyMonsters.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindEnemy2MonsterAt(Point position)
	{
		return m_Enemy2Monsters.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CEnemyActor FindNeutralMonsterAt(Point position)
	{
		return m_NeutralMonsters.Find((CEnemyActor x) => x.ArrayIndex == position);
	}

	public CObjectActor FindObjectActorAt(Point position)
	{
		return m_Objects.Find((CObjectActor x) => x.ArrayIndex == position);
	}

	public void RemoveMonster(CEnemyActor monster)
	{
		if (m_Enemies.Contains(monster))
		{
			RemoveEnemy(monster);
		}
		else if (m_AllyMonsters.Contains(monster))
		{
			RemoveAllyMonsters(monster);
		}
		else if (m_Enemy2Monsters.Contains(monster))
		{
			RemoveEnemy2Monster(monster);
		}
		else if (m_NeutralMonsters.Contains(monster))
		{
			RemoveNeutralMonster(monster);
		}
		else if (m_Objects.Contains(monster))
		{
			RemoveObjectActor((CObjectActor)monster);
		}
	}

	public void RemoveEnemy(CEnemyActor enemyActor)
	{
		m_Enemies.Remove(enemyActor);
		if (!DeadEnemies.Exists((CEnemyActor x) => x.ActorGuid == enemyActor.ActorGuid))
		{
			DeadEnemies.Add(enemyActor);
		}
	}

	public void RemovePlayer(CPlayerActor playerActor)
	{
		m_Players.Remove(playerActor);
		if (!ExhaustedPlayers.Exists((CPlayerActor x) => x.ActorGuid == playerActor.ActorGuid))
		{
			ExhaustedPlayers.Add(playerActor);
		}
	}

	public void RemoveHeroSummon(CHeroSummonActor heroSummonActor)
	{
		m_HeroSummons.Remove(heroSummonActor);
		if (!DeadHeroSummons.Exists((CHeroSummonActor x) => x.ActorGuid == heroSummonActor.ActorGuid))
		{
			DeadHeroSummons.Add(heroSummonActor);
		}
	}

	public void RemoveAllyMonsters(CEnemyActor allyMonster)
	{
		m_AllyMonsters.Remove(allyMonster);
		if (!DeadAllyMonsters.Exists((CEnemyActor x) => x.ActorGuid == allyMonster.ActorGuid))
		{
			DeadAllyMonsters.Add(allyMonster);
		}
	}

	public void RemoveEnemy2Monster(CEnemyActor enemy2monster)
	{
		m_Enemy2Monsters.Remove(enemy2monster);
		if (!DeadEnemy2Monsters.Exists((CEnemyActor x) => x.ActorGuid == enemy2monster.ActorGuid))
		{
			DeadEnemy2Monsters.Add(enemy2monster);
		}
	}

	public void RemoveNeutralMonster(CEnemyActor neutralMonster)
	{
		m_NeutralMonsters.Remove(neutralMonster);
		if (!DeadNeutralMonsters.Exists((CEnemyActor x) => x.ActorGuid == neutralMonster.ActorGuid))
		{
			DeadNeutralMonsters.Add(neutralMonster);
		}
	}

	public void RemoveObjectActor(CObjectActor objectActor)
	{
		m_Objects.Remove(objectActor);
		if (!DeadObjects.Exists((CObjectActor x) => x.ActorGuid == objectActor.ActorGuid))
		{
			DeadObjects.Add(objectActor);
		}
	}

	public bool HasActor(CActor actor)
	{
		if (actor.Class is CCharacterClass)
		{
			return m_Players.Contains((CPlayerActor)actor);
		}
		if (actor.Class is CMonsterClass)
		{
			if (actor.Class is CObjectClass)
			{
				return m_Objects.Contains((CObjectActor)actor);
			}
			return AllAliveMonsters.Contains((CEnemyActor)actor);
		}
		return m_HeroSummons.Contains((CHeroSummonActor)actor);
	}

	public CActor FindActor(string actorGuid)
	{
		object obj = m_Players.SingleOrDefault((CPlayerActor s) => s.ActorGuid == actorGuid) ?? ExhaustedPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == actorGuid);
		if (obj == null)
		{
			obj = m_InitialPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialHeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_Enemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialEnemies.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_AllyMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadAllyMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_Enemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadEnemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialEnemy2Monsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_NeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadNeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialNeutralMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_Objects.SingleOrDefault((CObjectActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = DeadObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == actorGuid);
		}
		if (obj == null)
		{
			obj = m_InitialObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == actorGuid);
		}
		return (CActor)obj;
	}

	public bool CalcActorAlertedState(CActor actor)
	{
		if (!(actor is CPlayerActor))
		{
			return false;
		}
		if (AlertedByAdjacentEnemy)
		{
			foreach (CEnemyActor allEnemyMonster in AllEnemyMonsters)
			{
				if (!allEnemyMonster.IsDead && ScenarioManager.GetTileDistance(allEnemyMonster.ArrayIndex.X, allEnemyMonster.ArrayIndex.Y, actor.ArrayIndex.X, actor.ArrayIndex.Y) == 1)
				{
					return true;
				}
			}
			return false;
		}
		if (AlertedByEnemyInSameRoom)
		{
			CMap hexMap = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y].m_HexMap;
			foreach (CEnemyActor allEnemyMonster2 in AllEnemyMonsters)
			{
				if (!allEnemyMonster2.IsDead && ScenarioManager.Tiles[allEnemyMonster2.ArrayIndex.X, allEnemyMonster2.ArrayIndex.Y].m_HexMap == hexMap)
				{
					return true;
				}
			}
			return false;
		}
		foreach (CEnemyActor allEnemyMonster3 in AllEnemyMonsters)
		{
			if (!allEnemyMonster3.IsDead)
			{
				return true;
			}
		}
		foreach (CObjectActor allObject in AllObjects)
		{
			if (!allObject.IsDead && CActor.AreActorsEnemies(actor.Type, allObject.Type, actor, allObject))
			{
				return true;
			}
		}
		return false;
	}
}
