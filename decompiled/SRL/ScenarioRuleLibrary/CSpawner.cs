using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

[Serializable]
[StateDump(UseToString = true)]
public class CSpawner : ISerializable
{
	private bool forceIndicatorVisible;

	public string SpawnerGuid { get; private set; }

	public SpawnerData SpawnerData { get; private set; }

	public TileIndex ArrayIndex { get; private set; }

	public bool IsActive { get; private set; }

	public int SpawnRoundCounter { get; private set; }

	public int CurrentSpawnRoundEntryIndex { get; set; }

	public string StartingMapGuid { get; set; }

	public CMap StartingMap => ScenarioManager.Scenario.Maps.SingleOrDefault((CMap x) => x.MapGuid == StartingMapGuid);

	public CMap CurrentMap => ScenarioManager.GetTile(ArrayIndex.X, ArrayIndex.Y).m_HexMap;

	public string CurrentMapGuid => CurrentMap.MapGuid;

	[StateDump(Skip = true)]
	public CObjectProp Prop { get; protected set; }

	public CActor.EType ActorTypeToSpawn { get; protected set; }

	public SpawnerData.ESpawnerEntryDifficulty SpawnerEntryDifficulty { get; private set; }

	public CAIFocusOverrideDetails AIFocusOverride { get; private set; }

	public Dictionary<int, ScenarioManager.EPerPartySizeConfig> ConfigPerPartySize { get; private set; }

	public bool HasSpawned { get; private set; }

	public int ActivationsCount { get; private set; }

	public bool HasIndicator => Prop != null;

	[StateDump(Skip = true)]
	public bool WasActiveBeforeSave { get; set; }

	public List<SpawnRoundEntry> SpawnRoundEntries
	{
		get
		{
			if (SpawnerData.SpawnRoundEntryDictionary.TryGetValue(SpawnerEntryDifficulty.ToString(), out var value))
			{
				return value;
			}
			return SpawnerData.SpawnRoundEntryDictionary["Default"];
		}
	}

	public List<TileIndex> PathingBlockers
	{
		get
		{
			if (Prop != null && Prop is CObjectObstacle cObjectObstacle)
			{
				return cObjectObstacle.PathingBlockers;
			}
			return new List<TileIndex>();
		}
	}

	public CTile GetTile()
	{
		return ScenarioManager.GetTile(ArrayIndex.ToPoint());
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SpawnerGuid", SpawnerGuid);
		info.AddValue("SpawnerData", SpawnerData);
		info.AddValue("ArrayIndex", ArrayIndex);
		info.AddValue("IsActive", IsActive);
		info.AddValue("SpawnRoundCounter", SpawnRoundCounter);
		info.AddValue("CurrentSpawnRoundEntryIndex", CurrentSpawnRoundEntryIndex);
		info.AddValue("StartingMapGuid", StartingMapGuid);
		info.AddValue("WasActiveBeforeSave", WasActiveBeforeSave);
		info.AddValue("AIFocusOverride", AIFocusOverride);
		info.AddValue("ConfigPerPartySize", ConfigPerPartySize);
		info.AddValue("ActorTypeToSpawn", ActorTypeToSpawn);
		info.AddValue("HasSpawned", HasSpawned);
		info.AddValue("ActivationsCount", ActivationsCount);
		info.AddValue("forceIndicatorVisible", forceIndicatorVisible);
	}

	public CSpawner(SerializationInfo info, StreamingContext context)
	{
		bool flag = false;
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "SpawnerGuid":
					SpawnerGuid = info.GetString("SpawnerGuid");
					break;
				case "SpawnerData":
					SpawnerData = (SpawnerData)info.GetValue("SpawnerData", typeof(SpawnerData));
					break;
				case "ArrayIndex":
					ArrayIndex = (TileIndex)info.GetValue("ArrayIndex", typeof(TileIndex));
					break;
				case "IsActive":
					IsActive = info.GetBoolean("IsActive");
					break;
				case "WasActiveBeforeSave":
					WasActiveBeforeSave = info.GetBoolean("WasActiveBeforeSave");
					break;
				case "SpawnRoundCounter":
					SpawnRoundCounter = info.GetInt32("SpawnRoundCounter");
					break;
				case "CurrentSpawnRoundEntryIndex":
					CurrentSpawnRoundEntryIndex = info.GetInt32("CurrentSpawnRoundEntryIndex");
					break;
				case "StartingMapGuid":
					StartingMapGuid = info.GetString("StartingMapGuid");
					break;
				case "AIFocusOverride":
					AIFocusOverride = (CAIFocusOverrideDetails)info.GetValue("AIFocusOverride", typeof(CAIFocusOverrideDetails));
					break;
				case "ConfigPerPartySize":
					ConfigPerPartySize = (Dictionary<int, ScenarioManager.EPerPartySizeConfig>)info.GetValue("ConfigPerPartySize", typeof(Dictionary<int, ScenarioManager.EPerPartySizeConfig>));
					break;
				case "ActorTypeToSpawn":
					flag = true;
					ActorTypeToSpawn = (CActor.EType)info.GetValue("ActorTypeToSpawn", typeof(CActor.EType));
					break;
				case "HasSpawned":
					HasSpawned = info.GetBoolean("HasSpawned");
					break;
				case "ActivationsCount":
					ActivationsCount = info.GetInt32("ActivationsCount");
					break;
				case "forceIndicatorVisible":
					forceIndicatorVisible = info.GetBoolean("forceIndicatorVisible");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CSpawner entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (!flag)
		{
			ActorTypeToSpawn = CActor.EType.Enemy;
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (ConfigPerPartySize == null)
		{
			ConfigPerPartySize = GetDefaultConfigPerPartySize();
		}
	}

	public override string ToString()
	{
		return $"{GetType().Name}({SpawnerGuid} at {ArrayIndex})";
	}

	public CSpawner(SpawnerData spawnerData, TileIndex arrayIndex, string startingMapGuid, string spawnerGuid = null, SpawnerData.ESpawnerEntryDifficulty spawnerEntryDifficulty = SpawnerData.ESpawnerEntryDifficulty.Default, CActor.EType typeToSpawn = CActor.EType.Enemy)
	{
		SpawnerGuid = spawnerGuid ?? Guid.NewGuid().ToString();
		StartingMapGuid = startingMapGuid;
		SpawnerData = spawnerData;
		ArrayIndex = arrayIndex;
		SpawnRoundCounter = 0;
		CurrentSpawnRoundEntryIndex = 0;
		IsActive = SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.ScenarioStart;
		SpawnerEntryDifficulty = spawnerEntryDifficulty;
		ConfigPerPartySize = GetDefaultConfigPerPartySize();
		ActorTypeToSpawn = typeToSpawn;
		HasSpawned = false;
		ActivationsCount = 0;
	}

	public CSpawner Clone(string newGUID = null)
	{
		return new CSpawner(SpawnerData.Clone(), new TileIndex(ArrayIndex.X, ArrayIndex.Y), StartingMapGuid, newGUID, SpawnerEntryDifficulty, ActorTypeToSpawn)
		{
			AIFocusOverride = AIFocusOverride,
			ConfigPerPartySize = ConfigPerPartySize
		};
	}

	public void SetLocation(TileIndex arrayIndex)
	{
		bool flag = false;
		if (Prop != null)
		{
			flag = true;
			HideSpawnerProp();
		}
		ScenarioManager.GetTile(ArrayIndex.X, ArrayIndex.Y).m_Spawners.Remove(this);
		ArrayIndex = arrayIndex;
		ScenarioManager.GetTile(ArrayIndex.X, ArrayIndex.Y).m_Spawners.AddOrdered(this, ScenarioManager.CurrentScenarioState.Spawners);
		if (flag)
		{
			UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		}
	}

	public void SetAIFocusOverride(CAIFocusOverrideDetails aiFocusOverride)
	{
		AIFocusOverride = aiFocusOverride;
	}

	public void SetSpawnType(CActor.EType actorTypeToSpawn)
	{
		ActorTypeToSpawn = actorTypeToSpawn;
	}

	public virtual void SetActive(bool active, bool forceDontSpawn = false)
	{
		bool isActive = IsActive;
		IsActive = active;
		if (IsActive && !isActive)
		{
			ActivationsCount++;
		}
		if (!IsActive && Prop != null)
		{
			HideSpawnerProp();
		}
		else if (IsActive && !isActive && !forceDontSpawn && SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.ActivatedExternally)
		{
			TrySpawnUnit(ScenarioManager.CurrentScenarioState.Players.Count);
		}
		else
		{
			UpdateWillSpawnNextRoundIndicator(ScenarioManager.Scenario.InitialPlayers.Count);
		}
	}

	public bool ConvertMultiEntrySpawnerToSeparateSpawners()
	{
		List<SpawnRoundEntry> spawnRoundEntries = SpawnRoundEntries;
		if (spawnRoundEntries == null || spawnRoundEntries.Count < 2)
		{
			return false;
		}
		int count = spawnRoundEntries.Count;
		int num = ScenarioManager.CurrentScenarioState.Spawners.IndexOf(this);
		CTile tile = ScenarioManager.GetTile(ArrayIndex.X, ArrayIndex.Y);
		int num2 = tile.m_Spawners.IndexOf(this);
		for (int i = 1; i < count; i++)
		{
			CSpawner cSpawner = Clone($"{SpawnerGuid}-{i}");
			RemoveEntriesButOne(cSpawner, i);
			if (num >= 0)
			{
				ScenarioManager.CurrentScenarioState.Spawners.Insert(num + i, cSpawner);
			}
			if (num2 >= 0)
			{
				tile.m_Spawners.Insert(num2 + i, cSpawner);
			}
		}
		RemoveEntriesButOne(this, 0);
		return true;
		static void RemoveAllButOne(List<SpawnRoundEntry> lst, int idx)
		{
			if (idx >= lst.Count)
			{
				lst.Clear();
			}
			else
			{
				SpawnRoundEntry item = lst[idx];
				lst.Clear();
				lst.Add(item);
			}
		}
		static void RemoveEntriesButOne(CSpawner spawner, int idx)
		{
			foreach (KeyValuePair<string, List<SpawnRoundEntry>> item2 in spawner.SpawnerData.SpawnRoundEntryDictionary)
			{
				RemoveAllButOne(item2.Value, idx);
			}
		}
	}

	public static bool ConvertMultiEntrySpawnersToSeparateSpawners()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		bool result = false;
		for (int num = spawners.Count - 1; num >= 0; num--)
		{
			if (spawners[num].ConvertMultiEntrySpawnerToSeparateSpawners())
			{
				result = true;
			}
		}
		return result;
	}

	public static CObjectProp CreateBaseSpawnerProp(int x, int y)
	{
		CTile cTile = ScenarioManager.Tiles[x, y];
		return CreateBaseSpawnerProp(new TileIndex(x, y), cTile.m_HexMap.MapGuid, null);
	}

	public static CObjectProp CreateBaseSpawnerProp(TileIndex arrayIndex, string startingMapGuid, CSpawner spawner)
	{
		CObjectProp cObjectProp = new CObjectProp(EPropType.Spawner.ToString(), ScenarioManager.ObjectImportType.Spawner, arrayIndex, null, null, null, startingMapGuid, (spawner != null) ? ("spawner_prop_" + spawner.SpawnerGuid) : null);
		cObjectProp.PropTile.SpawnProp(cObjectProp, notifyClient: true, 0.001f);
		return cObjectProp;
	}

	public virtual void CreateSpawnerProp(bool forceKeepItVisible = false)
	{
		if (!IsActive && !forceKeepItVisible)
		{
			return;
		}
		if (forceKeepItVisible)
		{
			forceIndicatorVisible = true;
		}
		if (Prop == null)
		{
			Prop = CreateBaseSpawnerProp(ArrayIndex, CurrentMapGuid, this);
			Prop.RuntimeAttachedSpawner = this;
			if (!GameState.IsInLevelEditor)
			{
				WasActiveBeforeSave = true;
			}
		}
	}

	public virtual void HideSpawnerProp(bool forced = false)
	{
		if (!forceIndicatorVisible || forced)
		{
			forceIndicatorVisible = false;
			if (Prop != null)
			{
				Prop.DestroyProp();
				Prop = null;
				WasActiveBeforeSave = false;
			}
		}
	}

	public void OnScenarioStart(bool firstLoad = false)
	{
		if (firstLoad)
		{
			IsActive = SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.ScenarioStart;
		}
	}

	public bool IsActiveAndRevealed()
	{
		bool revealed = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y].m_HexMap.Revealed;
		return IsActive & revealed;
	}

	public void StartRoundReset()
	{
		if (SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartRound)
		{
			HasSpawned = false;
		}
	}

	public void OnStartRound(int partySize, int roundNumber)
	{
		if ((!HasSpawned || SpawnerData.SpawnerTriggerType != SpawnerData.ESpawnerTriggerType.StartRound) && SpawnerData.SpawnerTriggerType != SpawnerData.ESpawnerTriggerType.TriggeredExternally)
		{
			if (IsActive && roundNumber >= SpawnerData.SpawnStartRound)
			{
				SpawnRoundCounter++;
			}
			if (IsActiveAndRevealed() && roundNumber >= SpawnerData.SpawnStartRound && (SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartRound || SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.Both))
			{
				TrySpawnUnit(partySize, treatAsSummon: false, initial: false, startRound: true, addIntoInitiativeOrder: true);
			}
			UpdateWillSpawnNextRoundIndicator(partySize);
		}
	}

	public void OnEndRound(int partySize, int roundNumber)
	{
		if (IsActiveAndRevealed() && roundNumber >= SpawnerData.SpawnStartRound && (SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.EndRound || SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.Both))
		{
			TrySpawnUnit(partySize);
		}
	}

	public void OnStartTurn(int partySize, int roundNumber)
	{
		if (IsActiveAndRevealed() && roundNumber >= SpawnerData.SpawnStartRound && SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartTurn)
		{
			TrySpawnUnit(partySize);
		}
	}

	public void OnEndTurn(int partySize, int roundNumber)
	{
		if (IsActiveAndRevealed() && roundNumber >= SpawnerData.SpawnStartRound && SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.EndTurn)
		{
			TrySpawnUnit(partySize);
		}
	}

	public void OnRoomOpened(int partySize, bool initial = false)
	{
		if (!initial && IsActive)
		{
			UpdateWillSpawnNextRoundIndicator(ScenarioManager.CurrentScenarioState.Players.Count);
		}
	}

	public void OnPressurePlateTriggered(int partySize, bool addIntoInitiativeOrder)
	{
		if (IsActiveAndRevealed())
		{
			TrySpawnUnit(partySize, treatAsSummon: false, initial: false, startRound: false, addIntoInitiativeOrder);
		}
	}

	private void TrySpawnUnit(int partySize, bool treatAsSummon = false, bool initial = false, bool startRound = false, bool addIntoInitiativeOrder = false)
	{
		int num = SpawnerData.SpawnRoundInterval[partySize - 1];
		if (num != 0 && SpawnRoundCounter % num == 0)
		{
			SpawnUnit(partySize, treatAsSummon, initial, startRound, forceUseAnyEntry: false, addIntoInitiativeOrder);
		}
	}

	public static bool SpawnUnitAtTile(int partySize, CActor.EType actorType, string classId, int tileX, int tileY, bool treatAsSummon = false, bool initial = false, bool startRound = false, CAIFocusOverrideDetails aiFocusOverride = null, bool addIntoInitiativeOrder = false, int spawnDelay = 0)
	{
		CActor resultActor;
		return SpawnUnitAtTile(partySize, actorType, classId, tileX, tileY, out resultActor, treatAsSummon, initial, startRound, aiFocusOverride, addIntoInitiativeOrder, spawnDelay);
	}

	public static bool SpawnUnitAtTile(int partySize, CActor.EType actorType, string classId, int tileX, int tileY, out CActor resultActor, bool treatAsSummon = false, bool initial = false, bool startRound = false, CAIFocusOverrideDetails aiFocusOverride = null, bool addIntoInitiativeOrder = false, int spawnDelay = 0)
	{
		CTile cTile = ScenarioManager.Tiles[tileX, tileY];
		bool flag = classId == "PROP_BearTrap";
		bool result = false;
		resultActor = null;
		if (flag)
		{
			CObjectTrap cObjectTrap = new CObjectTrap(ESpecificPropType.BearTrap.ToString(), ScenarioManager.ObjectImportType.Trap, new TileIndex(cTile.m_ArrayIndex), null, null, null, cTile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, 0, 0, 0, new List<CCondition.ENegativeCondition>(), null, 0);
			cTile.SpawnProp(cObjectTrap, notifyClient: true, spawnDelay);
			Log.SimpleInfo("Spawned Trap: " + ESpecificPropType.BearTrap.ToString() + " at Array Index: " + cTile.m_ArrayIndex.ToString());
			CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
			if (cActor != null)
			{
				cObjectTrap.Activate(cActor);
			}
			resultActor = cActor;
			result = true;
		}
		else
		{
			CClass cClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.ID == classId);
			if (cClass != null)
			{
				if (actorType == CActor.EType.Player || actorType == CActor.EType.HeroSummon || actorType == CActor.EType.Unknown)
				{
					Log.Warning("Unable to spawn Unit with ID " + classId + " because the \"" + actorType.ToString() + "\" Type is unsupported for Spawners");
				}
				else
				{
					int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cClass.Models.Count);
					EnemyState enemyState = new EnemyState(cClass.ID, chosenModelIndex, null, cTile.m_HexMap.MapGuid, new TileIndex(cTile.m_ArrayIndex), cClass.Health(), cClass.Health(), partySize, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, treatAsSummon, null, 1, actorType);
					if (aiFocusOverride != null)
					{
						enemyState.SetAIFocusOverride(aiFocusOverride);
					}
					CEnemyActor cEnemyActor = null;
					switch (actorType)
					{
					case CActor.EType.Enemy:
						cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial, noIDRegen: false, startRound);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
						}
						break;
					case CActor.EType.Ally:
						cEnemyActor = ScenarioManager.Scenario.AddAllyMonster(enemyState, initial, noIDRegen: false, startRound);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
						}
						break;
					case CActor.EType.Enemy2:
						cEnemyActor = ScenarioManager.Scenario.AddEnemy2Monster(enemyState, initial, noIDRegen: false, startRound);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
						}
						break;
					case CActor.EType.Neutral:
						cEnemyActor = ScenarioManager.Scenario.AddNeutralMonster(enemyState, initial, noIDRegen: false, startRound);
						if (cEnemyActor != null)
						{
							ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
						}
						break;
					}
					if (cEnemyActor != null)
					{
						result = true;
						resultActor = cEnemyActor;
						if (!initial)
						{
							ScenarioManager.CurrentScenarioState.Update();
							CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
							{
								m_SpawnTile = cTile,
								m_SpawnEnemy = cEnemyActor,
								m_SpawnDelay = spawnDelay,
								m_TreatAsSummon = treatAsSummon
							};
							ScenarioRuleClient.MessageHandler(message);
							ScenarioManager.CurrentScenarioState.Update();
							ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cEnemyActor, partySize);
							ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
							if (PhaseManager.PhaseType != CPhase.PhaseType.StartRoundEffects && !startRound)
							{
								cEnemyActor.RecalcLockedInitiative();
							}
							if (addIntoInitiativeOrder)
							{
								GameState.AddActorIntoInitiativeOrder(cEnemyActor);
							}
						}
						Log.SimpleInfo("Spawned Actor: " + cEnemyActor.Class.ID + cEnemyActor.ID + " at Array Index: " + cEnemyActor.ArrayIndex.ToString());
					}
				}
			}
			else
			{
				Log.Warning("Unable to find unitClass with ID " + classId);
			}
		}
		return result;
	}

	public void SpawnUnit(int partySize, bool treatAsSummon = false, bool initial = false, bool startRound = false, bool forceUseAnyEntry = false, bool addIntoInitiativeOrder = false, bool spawnFarthestOnAdjacent = false, bool useFirstRoundStartingTile = false, int spawnDelay = 0)
	{
		SpawnRoundEntry spawnRoundEntry = null;
		if (CurrentSpawnRoundEntryIndex < SpawnRoundEntries.Count)
		{
			spawnRoundEntry = SpawnRoundEntries[CurrentSpawnRoundEntryIndex];
		}
		CurrentSpawnRoundEntryIndex++;
		if (SpawnerData.LoopSpawnPattern && CurrentSpawnRoundEntryIndex >= SpawnRoundEntries.Count)
		{
			CurrentSpawnRoundEntryIndex = 0;
		}
		if ((spawnRoundEntry == null) & forceUseAnyEntry)
		{
			if (SpawnRoundEntries.Count > CurrentSpawnRoundEntryIndex)
			{
				spawnRoundEntry = SpawnRoundEntries[CurrentSpawnRoundEntryIndex];
				CurrentSpawnRoundEntryIndex++;
			}
			else if (SpawnRoundEntries.Count > 0)
			{
				CurrentSpawnRoundEntryIndex = 0;
				spawnRoundEntry = SpawnRoundEntries[CurrentSpawnRoundEntryIndex];
				CurrentSpawnRoundEntryIndex++;
			}
			if (spawnRoundEntry == null)
			{
				Log.Warning("Unable to find any entry from which to spawn on the spawner, nothing getting spawned");
			}
		}
		if (spawnRoundEntry == null)
		{
			return;
		}
		string text = spawnRoundEntry.SpawnClass[partySize - 1];
		bool flag = text == "PROP_BearTrap";
		if (!useFirstRoundStartingTile && PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest && PhaseManager.PhaseType != CPhase.PhaseType.Autosave && PhaseManager.PhaseType != CPhase.PhaseType.StartScenarioEffects && PhaseManager.PhaseType != CPhase.PhaseType.StartRoundEffects)
		{
			useFirstRoundStartingTile = true;
		}
		CMapTile cMapTile = FindSpawnTile(initial, !flag, text, useFirstRoundStartingTile);
		if (cMapTile != null && (!spawnFarthestOnAdjacent || ArrayIndex.Equals(cMapTile.ArrayIndex) || ScenarioManager.IsTileAdjacent(ArrayIndex, cMapTile.ArrayIndex)))
		{
			HasSpawned = SpawnUnitAtTile(partySize, ActorTypeToSpawn, text, cMapTile.ArrayIndex.X, cMapTile.ArrayIndex.Y, out var resultActor, treatAsSummon, initial, startRound, null, addIntoInitiativeOrder, spawnDelay);
			if (HasSpawned && ScenarioScript.current != null)
			{
				ScenarioScript.current.OnSpawnerHasSpawned(this, resultActor);
			}
		}
		else
		{
			Log.Warning("Unable to find spawn tile to spawn unit from Spawner SpawnerGUID: " + SpawnerGuid + " StartingMapGuid: " + StartingMapGuid);
		}
	}

	private CMapTile FindSpawnTile(bool initial, bool needEmptyTile = true, string classID = null, bool useFirstRoundStartingTile = false)
	{
		if (needEmptyTile)
		{
			CMapTile cMapTile = null;
			List<CMapTile> mapTiles = FindClosestEmptyTileList(initial, classID, useFirstRoundStartingTile);
			cMapTile = FindEmptyTileInList(mapTiles, initial, classID);
			if (cMapTile == null && ScenarioManager.Scenario.FindActorAt(new Point(ArrayIndex)) == null)
			{
				cMapTile = CurrentMap.MapTiles.FirstOrDefault((CMapTile t) => t.ArrayIndex.X == ArrayIndex.X && t.ArrayIndex.Y == ArrayIndex.Y);
			}
			return cMapTile;
		}
		return CurrentMap.MapTiles.FirstOrDefault((CMapTile t) => t.ArrayIndex.X == ArrayIndex.X && t.ArrayIndex.Y == ArrayIndex.Y);
	}

	public static bool IsEmptyTile(int x, int y, bool initial = false, string classID = null)
	{
		return IsEmptyTile(ScenarioManager.Tiles[x, y], initial, classID);
	}

	public static bool IsEmptyTile(CTile clientTile, bool initial = false, string classID = null)
	{
		CTile propTile = null;
		IEnumerable<CTile> enumerable = ScenarioScript.current?.TilesCannotStopOnTopOf();
		if (enumerable != null)
		{
			foreach (CTile item in enumerable)
			{
				if (item == clientTile)
				{
					return false;
				}
			}
		}
		if (CAbilityFilter.IsValidTile(clientTile, CAbilityFilter.EFilterTile.EmptyHex, initial, classID))
		{
			return CObjectProp.FindPropWithPathingBlocker(clientTile.m_ArrayIndex, ref propTile) == null;
		}
		return false;
	}

	private CMapTile FindEmptyTileInList(List<CMapTile> mapTiles, bool initial, string classID = null)
	{
		CMapTile cMapTile = null;
		while (mapTiles.Count > 0 && cMapTile == null)
		{
			CMapTile cMapTile2 = mapTiles[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(mapTiles.Count)];
			if (IsEmptyTile(ScenarioManager.Tiles[cMapTile2.ArrayIndex.X, cMapTile2.ArrayIndex.Y], initial, classID))
			{
				cMapTile = cMapTile2;
			}
			else
			{
				mapTiles.Remove(cMapTile2);
			}
		}
		return cMapTile;
	}

	private List<CMapTile> FindClosestEmptyTileList(bool initial, string classID = null, bool canUseFirstRoundStartingTile = false)
	{
		CTile spawnerTile = ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];
		List<CMapTile> list = new List<CMapTile>();
		CTile propTile = null;
		CNode cNode = ScenarioManager.PathFinder.Nodes[spawnerTile.m_ArrayIndex.X, spawnerTile.m_ArrayIndex.Y];
		if (!(this is CInteractableSpawner) && cNode.Walkable && !cNode.Blocked && CAbilityFilter.IsValidTile(spawnerTile, CAbilityFilter.EFilterTile.EmptyHex, initial, classID) && ((ScenarioManager.CurrentScenarioState.RoundNumber > 1 || !ScenarioManager.StartingTiles.Contains(spawnerTile)) | canUseFirstRoundStartingTile))
		{
			CMapTile item = CurrentMap.MapTiles.SingleOrDefault((CMapTile x) => x.ArrayIndex.X == spawnerTile.m_ArrayIndex.X && x.ArrayIndex.Y == spawnerTile.m_ArrayIndex.Y);
			list.Add(item);
		}
		else
		{
			string text = "(SpawnerGUID: " + SpawnerGuid + " Spawner Tile Blocked):\n";
			string text2 = "";
			CActor cActor = ScenarioManager.Scenario.FindActorAt(spawnerTile.m_ArrayIndex);
			if (cActor != null)
			{
				text2 = cActor.Class.ID + ((cActor is CPlayerActor) ? "" : cActor.ID.ToString());
			}
			else
			{
				for (int num = 0; num < spawnerTile.m_Props.Count; num++)
				{
					CObjectProp cObjectProp = spawnerTile.m_Props[num];
					text2 = text2 + cObjectProp.InstanceName + ((num < spawnerTile.m_Props.Count - 1) ? ", " : "");
				}
				if (ScenarioManager.CurrentScenarioState.RoundNumber <= 1 && ScenarioManager.StartingTiles.Contains(spawnerTile))
				{
					text2 += (string.IsNullOrEmpty(text2) ? "" : ", Starting Tile on Round 1");
				}
			}
			text = text + "Tile " + spawnerTile.m_ArrayIndex.ToString() + " Blocked By: " + text2;
			Log.SimpleInfo(text);
			for (int num2 = 0; num2 < 10; num2++)
			{
				List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(spawnerTile, num2 + 1);
				allUnblockedTilesFromOrigin.Remove(spawnerTile);
				List<CTile> list2 = new List<CTile>();
				List<CTile> list3 = new List<CTile>();
				foreach (CTile item2 in allUnblockedTilesFromOrigin)
				{
					if (ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y].Walkable && CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex, initial, classID) && CObjectProp.FindPropWithPathingBlocker(item2.m_ArrayIndex, ref propTile) == null && (ScenarioManager.CurrentScenarioState.RoundNumber > 1 || !ScenarioManager.StartingTiles.Contains(item2)))
					{
						list2.Add(item2);
					}
					else
					{
						list3.Add(item2);
					}
				}
				string text3 = "(SpawnerGUID: " + SpawnerGuid + " Check Spawner Ring " + (num2 + 1) + "):\nEmpty Tiles Found:";
				for (int num3 = 0; num3 < list2.Count; num3++)
				{
					CTile cTile = list2[num3];
					text3 = text3 + "\nTile " + cTile.m_ArrayIndex.ToString();
				}
				text3 += "\nBlocked Tiles Found:";
				for (int num4 = 0; num4 < list3.Count; num4++)
				{
					CTile cTile2 = list3[num4];
					text2 = "";
					CActor cActor2 = ScenarioManager.Scenario.FindActorAt(cTile2.m_ArrayIndex);
					if (cActor2 != null)
					{
						text2 = cActor2.Class.ID + ((cActor2 is CPlayerActor) ? "" : cActor2.ID.ToString());
					}
					else
					{
						for (int num5 = 0; num5 < cTile2.m_Props.Count; num5++)
						{
							CObjectProp cObjectProp2 = cTile2.m_Props[num5];
							text2 = text2 + cObjectProp2.InstanceName + ((num5 < cTile2.m_Props.Count - 1) ? ", " : "");
						}
						if (ScenarioManager.CurrentScenarioState.RoundNumber <= 1 && ScenarioManager.StartingTiles.Contains(cTile2))
						{
							text2 += (string.IsNullOrEmpty(text2) ? "" : ", Starting Tile on Round 1");
						}
					}
					text3 = text3 + "\nTile " + cTile2.m_ArrayIndex.ToString() + " Blocked By: " + text2;
				}
				Log.SimpleInfo(text3);
				if (list2.Count <= 0)
				{
					continue;
				}
				foreach (CTile emptyRingTile in list2)
				{
					CMapTile cMapTile = CurrentMap.MapTiles.SingleOrDefault((CMapTile x) => x.ArrayIndex.X == emptyRingTile.m_ArrayIndex.X && x.ArrayIndex.Y == emptyRingTile.m_ArrayIndex.Y);
					if (cMapTile != null)
					{
						list.Add(cMapTile);
						continue;
					}
					Log.SimpleInfo("Empty tile: " + emptyRingTile.m_ArrayIndex.ToString() + " is not on the same room map as (SpawnerGUID: " + SpawnerGuid + ")");
				}
				break;
			}
		}
		return list;
	}

	public bool UpdateWillSpawnNextRoundIndicator(int partySize)
	{
		if (partySize == 0)
		{
			return true;
		}
		int roundNumber = ScenarioManager.CurrentScenarioState.RoundNumber;
		if (GetTile()?.m_HexMap?.Revealed == true && (IsActive || SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.TriggeredExternally) && roundNumber + 1 >= SpawnerData.SpawnStartRound && (!HasSpawned || SpawnerData.LoopSpawnPattern))
		{
			int num = SpawnerData.SpawnRoundInterval[partySize - 1];
			if (num != 0)
			{
				bool flag = false;
				switch (SpawnerData.SpawnerTriggerType)
				{
				case SpawnerData.ESpawnerTriggerType.EndRound:
					flag = SpawnRoundCounter % num == 0;
					break;
				case SpawnerData.ESpawnerTriggerType.StartRound:
					flag = (SpawnRoundCounter + 1) % num == 0;
					break;
				case SpawnerData.ESpawnerTriggerType.Both:
					flag = SpawnRoundCounter % num == 0 || (SpawnRoundCounter + 1) % num == 0;
					break;
				case SpawnerData.ESpawnerTriggerType.TriggeredExternally:
					flag = true;
					break;
				}
				if (flag)
				{
					CClass cClass = null;
					if (CurrentSpawnRoundEntryIndex < SpawnRoundEntries.Count)
					{
						cClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.ID == SpawnRoundEntries[CurrentSpawnRoundEntryIndex].SpawnClass[partySize - 1]);
					}
					if (cClass != null)
					{
						if (Prop == null)
						{
							CreateSpawnerProp();
						}
						return true;
					}
				}
			}
		}
		HideSpawnerProp();
		return false;
	}

	public CMonsterClass GetNextMonsterClassToSpawn(int partySize)
	{
		CMonsterClass cMonsterClass = null;
		if (forceIndicatorVisible || UpdateWillSpawnNextRoundIndicator(partySize))
		{
			for (int i = CurrentSpawnRoundEntryIndex; i < SpawnRoundEntries.Count; i++)
			{
				SpawnRoundEntry currentEntry = SpawnRoundEntries[i];
				cMonsterClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.ID == currentEntry.SpawnClass[partySize - 1]);
				if (cMonsterClass != null)
				{
					break;
				}
			}
			if (cMonsterClass == null && SpawnerData.LoopSpawnPattern)
			{
				for (int num = 0; num < SpawnRoundEntries.Count; num++)
				{
					SpawnRoundEntry currentEntry2 = SpawnRoundEntries[num];
					cMonsterClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.ID == currentEntry2.SpawnClass[partySize - 1]);
					if (cMonsterClass != null)
					{
						break;
					}
				}
			}
		}
		return cMonsterClass;
	}

	public int TotalMonstersLeftToSpawn()
	{
		if (!SpawnerData.LoopSpawnPattern)
		{
			return Math.Max(0, SpawnRoundEntries.Count - CurrentSpawnRoundEntryIndex);
		}
		return int.MaxValue;
	}

	public void SetConfigForPartySize(int partySize, ScenarioManager.EPerPartySizeConfig configToSet)
	{
		ConfigPerPartySize[partySize] = configToSet;
	}

	public ScenarioManager.EPerPartySizeConfig GetConfigForPartySize(int partySize)
	{
		if (ConfigPerPartySize == null || !ConfigPerPartySize.ContainsKey(partySize))
		{
			return ScenarioManager.EPerPartySizeConfig.Normal;
		}
		return ConfigPerPartySize[partySize];
	}

	private static Dictionary<int, ScenarioManager.EPerPartySizeConfig> GetDefaultConfigPerPartySize()
	{
		return new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
		{
			{
				1,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				2,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				3,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				4,
				ScenarioManager.EPerPartySizeConfig.Normal
			}
		};
	}

	public static List<Tuple<int, string>> Compare(CSpawner state1, CSpawner state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.SpawnerGuid != state2.SpawnerGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2301, "CSpawner SpawnerGuid does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					}
				});
			}
			if (!TileIndex.Compare(state1.ArrayIndex, state2.ArrayIndex))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2302, "CSpawner ArrayIndex does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					}
				});
			}
			if (state1.IsActive != state2.IsActive)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2303, "CSpawner IsActive does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"IsActive",
						state1.IsActive.ToString(),
						state2.IsActive.ToString()
					}
				});
			}
			if (state1.SpawnRoundCounter != state2.SpawnRoundCounter)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2304, "CSpawner SpawnRoundCounter does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"SpawnRoundCounter",
						state1.SpawnRoundCounter.ToString(),
						state2.SpawnRoundCounter.ToString()
					}
				});
			}
			if (state1.CurrentSpawnRoundEntryIndex != state2.CurrentSpawnRoundEntryIndex)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2305, "CSpawner CurrentSpawnRoundEntryIndex does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"CurrentSpawnRoundEntryIndex",
						state1.CurrentSpawnRoundEntryIndex.ToString(),
						state2.CurrentSpawnRoundEntryIndex.ToString()
					}
				});
			}
			if (state1.StartingMapGuid != state2.StartingMapGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2306, "CSpawner StartingMapGuid does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3] { "StartingMapGuid", state1.StartingMapGuid, state2.StartingMapGuid }
				});
			}
			list.AddRange(SpawnerData.Compare(state1.SpawnerData, state2.SpawnerData, state1.SpawnerGuid, state1.ArrayIndex, isMPCompare));
			if (StateShared.CheckNullsMatch(state1.AIFocusOverride, state2.AIFocusOverride) == StateShared.ENullStatus.Mismatch)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2310, "CSpawner AIFocusOverride null state does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"Array Index",
						state1.ArrayIndex.ToString(),
						state2.ArrayIndex.ToString()
					},
					new string[3]
					{
						"AIFocusOverride",
						(state1.AIFocusOverride == null) ? "is null" : "is not null",
						(state2.AIFocusOverride == null) ? "is null" : "is not null"
					}
				});
			}
			switch (StateShared.CheckNullsMatch(state1.ConfigPerPartySize, state2.ConfigPerPartySize))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2311, "CSpawner ConfigPerPartySize null state does not match.", new List<string[]>
				{
					new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
					new string[3]
					{
						"ConfigPerPartySize",
						(state1.ConfigPerPartySize == null) ? "is null" : "is not null",
						(state2.ConfigPerPartySize == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.ConfigPerPartySize.Count != state2.ConfigPerPartySize.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2312, "CSpawner total ConfigPerPartySize Count does not match.", new List<string[]>
					{
						new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
						new string[3]
						{
							"ConfigPerPartySize Count",
							state1.ConfigPerPartySize.Count.ToString(),
							state2.ConfigPerPartySize.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in state1.ConfigPerPartySize)
				{
					if (!state2.ConfigPerPartySize.ContainsKey(item.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2313, "CSpawner ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item.Key.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item2 in state2.ConfigPerPartySize)
				{
					if (!state1.ConfigPerPartySize.ContainsKey(item2.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2314, "CSpawner ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
							new string[3]
							{
								"ConfigPerPartySize Key",
								"Missing",
								item2.Key.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item3 in state1.ConfigPerPartySize)
				{
					if (item3.Value != state2.ConfigPerPartySize[item3.Key])
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2315, "CSpawner ConfigPerPartySize has key with differing values.", new List<string[]>
						{
							new string[3] { "Spawner GUID", state1.SpawnerGuid, state2.SpawnerGuid },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item3.Key.ToString(),
								item3.Key.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Value",
								item3.Value.ToString(),
								state2.ConfigPerPartySize[item3.Key].ToString()
							}
						});
						flag = true;
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2399, "Exception during CSpawner compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
