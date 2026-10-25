using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[Serializable]
public class CMap : ISerializable, IDumpState
{
	public static readonly EMapType[] MapTypes = (EMapType[])Enum.GetValues(typeof(EMapType));

	private bool m_ReveledInternal;

	private bool m_TreatAsIce;

	public string MapGuid { get; private set; }

	public int IslandID { get; private set; }

	public string RoomName { get; set; }

	public EMapType MapType { get; set; }

	[StateDump(Skip = true)]
	public List<CMapTile> MapTiles { get; private set; }

	public bool Revealed { get; set; }

	public bool RevealedAndNotDestroyed
	{
		get
		{
			if (Revealed)
			{
				return !Destroyed;
			}
			return false;
		}
	}

	public bool Destroyed { get; set; }

	public string ParentName { get; set; }

	public List<string> Children { get; private set; }

	public string ScenarioRoomName { get; private set; }

	public ScenarioPossibleRoom SelectedPossibleRoom { get; private set; }

	public CVector3 Position { get; set; }

	public CVector3 Rotation { get; set; }

	public CVector3 Centre { get; set; }

	public CVector3 ClosestTileIdentityPosition { get; set; }

	public float Angle { get; set; }

	public CVector3 DefaultLevelFlowNormal { get; set; }

	public string EntranceDoor { get; set; }

	public List<string> ExitDoors { get; private set; }

	public string DungeonEntranceDoor { get; set; }

	public string DungeonExitDoor { get; set; }

	public bool IsDungeonExitRoom { get; private set; }

	public bool IsAdditionalDungeonEntranceRoom { get; private set; }

	public bool FailedToLoad { get; set; }

	public bool CanTeleportToFromAnotherMap { get; set; } = true;

	public bool TreatAsWater { get; set; }

	public bool TreatAsIce
	{
		get
		{
			return m_TreatAsIce;
		}
		set
		{
			SetTreatAsIce(value);
		}
	}

	public List<PlayerState> Players => ScenarioManager.CurrentScenarioState?.Players.Where((PlayerState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<PlayerState>();

	public List<EnemyState> Monsters => ScenarioManager.CurrentScenarioState?.Monsters.Where((EnemyState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<EnemyState>();

	public List<EnemyState> AllyMonsters => ScenarioManager.CurrentScenarioState?.AllyMonsters.Where((EnemyState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<EnemyState>();

	public List<EnemyState> Enemy2Monsters => ScenarioManager.CurrentScenarioState?.Enemy2Monsters.Where((EnemyState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<EnemyState>();

	public List<EnemyState> NeutralMonsters => ScenarioManager.CurrentScenarioState?.NeutralMonsters.Where((EnemyState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<EnemyState>();

	public List<HeroSummonState> HeroSummons => ScenarioManager.CurrentScenarioState?.HeroSummons.Where((HeroSummonState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<HeroSummonState>();

	public List<ObjectState> Objects => ScenarioManager.CurrentScenarioState?.Objects.Where((ObjectState w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<ObjectState>();

	public List<CObjectProp> Props => ScenarioManager.CurrentScenarioState?.Props.Where((CObjectProp w) => w.StartingMapGuid == MapGuid).ToList() ?? new List<CObjectProp>();

	public List<CObjectProp> DoorProps => ScenarioManager.CurrentScenarioState?.Props.Where((CObjectProp w) => w.ObjectType == ScenarioManager.ObjectImportType.Door && ((w as CObjectDoor).HexMap == this || (w as CObjectDoor).Hex2Map == this)).ToList() ?? new List<CObjectProp>();

	public List<CSpawner> Spawners => ScenarioManager.CurrentScenarioState?.Spawners.Where((CSpawner w) => w.CurrentMapGuid == MapGuid).ToList() ?? new List<CSpawner>();

	public string MapInstanceName => MapType.ToString() + ":" + MapGuid;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("MapGuid", MapGuid);
		info.AddValue("IslandID", IslandID);
		info.AddValue("RoomName", RoomName);
		info.AddValue("MapType", MapType);
		info.AddValue("MapTiles", MapTiles);
		info.AddValue("Revealed", Revealed);
		info.AddValue("Destroyed", Destroyed);
		info.AddValue("ParentName", ParentName);
		info.AddValue("Children", Children);
		info.AddValue("ScenarioRoomName", ScenarioRoomName);
		info.AddValue("SelectedPossibleRoom", SelectedPossibleRoom);
		info.AddValue("Position", Position);
		info.AddValue("Rotation", Rotation);
		info.AddValue("Centre", Centre);
		info.AddValue("ClosestTileIdentityPosition", ClosestTileIdentityPosition);
		info.AddValue("Angle", Angle);
		info.AddValue("DefaultLevelFlowNormal", DefaultLevelFlowNormal);
		info.AddValue("EntranceDoor", EntranceDoor);
		info.AddValue("ExitDoors", ExitDoors);
		info.AddValue("DungeonEntranceDoor", DungeonEntranceDoor);
		info.AddValue("DungeonExitDoor", DungeonExitDoor);
		info.AddValue("IsDungeonExitRoom", IsDungeonExitRoom);
		info.AddValue("IsAdditionalDungeonEntranceRoom", IsAdditionalDungeonEntranceRoom);
		info.AddValue("FailedToLoad", FailedToLoad);
	}

	public CMap(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "MapGuid":
					MapGuid = info.GetString("MapGuid");
					break;
				case "IslandID":
					IslandID = info.GetInt32("IslandID");
					break;
				case "RoomName":
					RoomName = info.GetString("RoomName");
					break;
				case "MapType":
					MapType = (EMapType)info.GetValue("MapType", typeof(EMapType));
					break;
				case "MapTiles":
					MapTiles = (List<CMapTile>)info.GetValue("MapTiles", typeof(List<CMapTile>));
					break;
				case "Revealed":
					Revealed = info.GetBoolean("Revealed");
					break;
				case "Destroyed":
					Destroyed = info.GetBoolean("Destroyed");
					break;
				case "ParentName":
					ParentName = info.GetString("ParentName");
					break;
				case "Children":
					Children = (List<string>)info.GetValue("Children", typeof(List<string>));
					break;
				case "ScenarioRoomName":
					ScenarioRoomName = info.GetString("ScenarioRoomName");
					break;
				case "SelectedPossibleRoom":
					SelectedPossibleRoom = (ScenarioPossibleRoom)info.GetValue("SelectedPossibleRoom", typeof(ScenarioPossibleRoom));
					break;
				case "Position":
					Position = (CVector3)info.GetValue("Position", typeof(CVector3));
					break;
				case "Rotation":
					Rotation = (CVector3)info.GetValue("Rotation", typeof(CVector3));
					break;
				case "Centre":
					Centre = (CVector3)info.GetValue("Centre", typeof(CVector3));
					break;
				case "ClosestTileIdentityPosition":
					ClosestTileIdentityPosition = (CVector3)info.GetValue("ClosestTileIdentityPosition", typeof(CVector3));
					break;
				case "Angle":
					Angle = (float)info.GetDouble("Angle");
					break;
				case "DefaultLevelFlowNormal":
					DefaultLevelFlowNormal = (CVector3)info.GetValue("DefaultLevelFlowNormal", typeof(CVector3));
					break;
				case "EntranceDoor":
					EntranceDoor = info.GetString("EntranceDoor");
					break;
				case "ExitDoors":
					ExitDoors = (List<string>)info.GetValue("ExitDoors", typeof(List<string>));
					break;
				case "DungeonEntranceDoor":
					DungeonEntranceDoor = info.GetString("DungeonEntranceDoor");
					break;
				case "DungeonExitDoor":
					DungeonExitDoor = info.GetString("DungeonExitDoor");
					break;
				case "IsDungeonExitRoom":
					IsDungeonExitRoom = info.GetBoolean("IsDungeonExitRoom");
					break;
				case "IsAdditionalDungeonEntranceRoom":
					IsAdditionalDungeonEntranceRoom = info.GetBoolean("IsAdditionalDungeonEntranceRoom");
					break;
				case "FailedToLoad":
					FailedToLoad = info.GetBoolean("FailedToLoad");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CMap entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (string.IsNullOrEmpty(ParentName) && string.IsNullOrEmpty(DungeonEntranceDoor) && !string.IsNullOrEmpty(EntranceDoor))
		{
			DungeonEntranceDoor = EntranceDoor;
			EntranceDoor = null;
		}
	}

	public override string ToString()
	{
		return "[" + MF.RefStr(this) + "] " + GetType().Name + "(" + MapInstanceName + ")";
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return GetType().Name + "(" + MapInstanceName + ")";
	}

	public CMap(string roomName, EMapType mapType, string scenarioRoomName, ScenarioPossibleRoom possibleRoom, string parentName)
	{
		MapGuid = Guid.NewGuid().ToString();
		IslandID = MapGuid.GetHashCode();
		RoomName = roomName;
		MapType = mapType;
		ScenarioRoomName = scenarioRoomName;
		SelectedPossibleRoom = possibleRoom;
		ParentName = parentName;
		MapTiles = new List<CMapTile>();
		Children = new List<string>();
		ExitDoors = new List<string>();
		Revealed = false;
	}

	public CMap(Guid mapGuid, string roomName, EMapType mapType, string scenarioRoomName, ScenarioPossibleRoom possibleRoom, string parentName, bool isDungeonExitRoom, bool isAdditionalDungeonEntranceRoom)
	{
		MapGuid = mapGuid.ToString();
		IslandID = MapGuid.GetHashCode();
		RoomName = roomName;
		MapType = mapType;
		ScenarioRoomName = scenarioRoomName;
		SelectedPossibleRoom = possibleRoom;
		ParentName = parentName;
		MapTiles = new List<CMapTile>();
		Children = new List<string>();
		ExitDoors = new List<string>();
		IsDungeonExitRoom = isDungeonExitRoom;
		IsAdditionalDungeonEntranceRoom = isAdditionalDungeonEntranceRoom;
		Revealed = false;
		Destroyed = false;
	}

	private void SetTreatAsIce(bool treatAsIce)
	{
		m_TreatAsIce = treatAsIce;
		if (treatAsIce)
		{
			ScenarioManager.PathFinder.HasIceTerrain = true;
		}
	}

	public void Prepare()
	{
		RevealProps();
	}

	public void Reveal(bool initial, bool noIDRegen = false, bool forLevelEditor = false, bool fromActorOpeningDoor = false)
	{
		bool reveledInternal = m_ReveledInternal;
		foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			scenarioScript.SetRevealedRoom(this);
		}
		if (!m_ReveledInternal)
		{
			foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnRoomRevealed))
			{
				if (item is CScenarioModifierApplyActiveBonusToActor cScenarioModifierApplyActiveBonusToActor)
				{
					cScenarioModifierApplyActiveBonusToActor.GetScenarioScriptAtRuntime(out var script);
					if (script is ScenarioScript_FH006)
					{
						foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
						{
							if (!map.Revealed)
							{
								continue;
							}
							foreach (CObjectProp item2 in map.Props.Where((CObjectProp p) => p.PropHealthDetails?.CustomLocKey == "Snowdrift" && !p.IsDestroyed))
							{
								Log.Info(item2.PrefabName + " (" + item2.PropGuid + ") destroyed");
								item2.RuntimeAttachedActor.Damaged(item2.RuntimeAttachedActor.Health, fromAttackAbility: false, item2.RuntimeAttachedActor, null);
								GameState.ActorHealthCheck(item2.RuntimeAttachedActor, item2.RuntimeAttachedActor);
							}
						}
					}
				}
				item.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			foreach (PlayerState item3 in Players.Where((PlayerState p) => !p.HiddenAtStart).ToList())
			{
				CPlayerActor cPlayerActor = ScenarioManager.Scenario.AddPlayer(item3, initial);
				if (cPlayerActor == null)
				{
					ScenarioManager.CurrentScenarioState.Players.Remove(item3);
				}
				else if (!forLevelEditor && !initial)
				{
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cPlayerActor, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
				}
			}
			foreach (EnemyState enemyState in Monsters)
			{
				ScenarioManager.EPerPartySizeConfig configForPartySize = enemyState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
				if (!forLevelEditor && configForPartySize == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				if (!enemyState.HasShiftedForm && !forLevelEditor && configForPartySize == ScenarioManager.EPerPartySizeConfig.ToElite && !enemyState.IsElite)
				{
					CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == enemyState.ClassID);
					bool num = enemyState.Health == cMonsterClass.Health() && enemyState.MaxHealth == cMonsterClass.Health();
					enemyState.IncreaseToElite();
					if (num)
					{
						cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == enemyState.ClassID);
						enemyState.Health = cMonsterClass.Health();
						enemyState.MaxHealth = cMonsterClass.Health();
					}
				}
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial, noIDRegen);
				if (cEnemyActor == null)
				{
					ScenarioManager.CurrentScenarioState.Monsters.Remove(enemyState);
				}
				else if (!forLevelEditor && !initial)
				{
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cEnemyActor, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
				}
			}
			foreach (HeroSummonState heroSummon in HeroSummons)
			{
				CHeroSummonActor cHeroSummonActor = ScenarioManager.Scenario.AddHeroSummon(heroSummon, initial, noIDRegen);
				if (cHeroSummonActor == null)
				{
					ScenarioManager.CurrentScenarioState.HeroSummons.Remove(heroSummon);
				}
				else if (!forLevelEditor && !initial)
				{
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cHeroSummonActor, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
				}
			}
			foreach (EnemyState allyMonsterState in AllyMonsters)
			{
				ScenarioManager.EPerPartySizeConfig configForPartySize2 = allyMonsterState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
				if (!forLevelEditor && configForPartySize2 == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				if (!allyMonsterState.HasShiftedForm && !forLevelEditor && configForPartySize2 == ScenarioManager.EPerPartySizeConfig.ToElite && !allyMonsterState.IsElite)
				{
					CMonsterClass cMonsterClass2 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == allyMonsterState.ClassID);
					bool num2 = allyMonsterState.Health == cMonsterClass2.Health() && allyMonsterState.MaxHealth == cMonsterClass2.Health();
					allyMonsterState.IncreaseToElite();
					if (num2)
					{
						cMonsterClass2 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == allyMonsterState.ClassID);
						allyMonsterState.Health = cMonsterClass2.Health();
						allyMonsterState.MaxHealth = cMonsterClass2.Health();
					}
				}
				CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.AddAllyMonster(allyMonsterState, initial, noIDRegen);
				if (cEnemyActor2 == null)
				{
					ScenarioManager.CurrentScenarioState.AllyMonsters.Remove(allyMonsterState);
				}
				else if (!forLevelEditor && !initial)
				{
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cEnemyActor2, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
				}
			}
			foreach (EnemyState enemy2MonsterState in Enemy2Monsters)
			{
				ScenarioManager.EPerPartySizeConfig configForPartySize3 = enemy2MonsterState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
				if (!forLevelEditor && configForPartySize3 == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				if (!enemy2MonsterState.HasShiftedForm && !forLevelEditor && configForPartySize3 == ScenarioManager.EPerPartySizeConfig.ToElite && !enemy2MonsterState.IsElite)
				{
					CMonsterClass cMonsterClass3 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == enemy2MonsterState.ClassID);
					bool num3 = enemy2MonsterState.Health == cMonsterClass3.Health() && enemy2MonsterState.MaxHealth == cMonsterClass3.Health();
					enemy2MonsterState.IncreaseToElite();
					if (num3)
					{
						cMonsterClass3 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == enemy2MonsterState.ClassID);
						enemy2MonsterState.Health = cMonsterClass3.Health();
						enemy2MonsterState.MaxHealth = cMonsterClass3.Health();
					}
				}
				CEnemyActor cEnemyActor3 = ScenarioManager.Scenario.AddEnemy2Monster(enemy2MonsterState, initial, noIDRegen);
				if (cEnemyActor3 == null)
				{
					ScenarioManager.CurrentScenarioState.Enemy2Monsters.Remove(enemy2MonsterState);
				}
				else if (!forLevelEditor && !initial)
				{
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cEnemyActor3, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
				}
			}
			foreach (EnemyState neutralMonsterState in NeutralMonsters)
			{
				ScenarioManager.EPerPartySizeConfig configForPartySize4 = neutralMonsterState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
				if (!forLevelEditor && configForPartySize4 == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				if (!neutralMonsterState.HasShiftedForm && !forLevelEditor && configForPartySize4 == ScenarioManager.EPerPartySizeConfig.ToElite && !neutralMonsterState.IsElite)
				{
					CMonsterClass cMonsterClass4 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == neutralMonsterState.ClassID);
					bool num4 = neutralMonsterState.Health == cMonsterClass4.Health() && neutralMonsterState.MaxHealth == cMonsterClass4.Health();
					neutralMonsterState.IncreaseToElite();
					if (num4)
					{
						cMonsterClass4 = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == neutralMonsterState.ClassID);
						neutralMonsterState.Health = cMonsterClass4.Health();
						neutralMonsterState.MaxHealth = cMonsterClass4.Health();
					}
				}
				CEnemyActor cEnemyActor4 = ScenarioManager.Scenario.AddNeutralMonster(neutralMonsterState, initial, noIDRegen);
				if (cEnemyActor4 == null)
				{
					ScenarioManager.CurrentScenarioState.NeutralMonsters.Remove(neutralMonsterState);
				}
				else if (!forLevelEditor && !initial)
				{
					ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cEnemyActor4, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
				}
			}
			foreach (ObjectState @object in Objects)
			{
				ScenarioManager.EPerPartySizeConfig configForPartySize5 = @object.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
				if ((forLevelEditor || configForPartySize5 != ScenarioManager.EPerPartySizeConfig.Hidden) && @object.Object == null)
				{
					CObjectActor cObjectActor = ScenarioManager.Scenario.AddObject(@object, initial, noIDRegen, @object.IsAttachedToProp);
					if (cObjectActor == null)
					{
						ScenarioManager.CurrentScenarioState.Objects.Remove(@object);
					}
					else if (!forLevelEditor && !initial)
					{
						ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cObjectActor, ScenarioManager.CurrentScenarioState.Players.Count, forLevelEditor);
					}
				}
			}
			CreateHealthPropDummies(initial, noIDRegen, forLevelEditor);
			foreach (CSpawner spawner in Spawners)
			{
				if (spawner.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				if ((!ScenarioManager.IsLoading || ScenarioManager.CurrentScenarioState.IsFirstLoad) && !spawner.IsActive && (spawner.SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.RoomRevealed || ((spawner.SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.RoomOpen) & fromActorOpeningDoor)))
				{
					spawner.SetActive(active: true);
					if (spawner.SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartRound && spawner.ArrayIndex != null)
					{
						spawner.OnRoomOpened(ScenarioManager.CurrentScenarioState.Players.Count, initial);
					}
				}
				if (spawner is CInteractableSpawner && spawner.Prop != null && !initial)
				{
					CSpawn_MessageData message = new CSpawn_MessageData(null)
					{
						m_SpawnDelay = 0f,
						m_Prop = spawner.Prop
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			if (!initial && !fromActorOpeningDoor)
			{
				foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
				{
					if (scenarioModifier.ShouldActivateWhenOpeningRoom(MapGuid))
					{
						scenarioModifier.SetDeactivated(deactivate: false);
					}
					if (scenarioModifier.ShouldTriggerWhenOpeningRoom(MapGuid))
					{
						scenarioModifier.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
					}
				}
			}
			else if (!initial & fromActorOpeningDoor)
			{
				foreach (CScenarioModifier scenarioModifier2 in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
				{
					if (scenarioModifier2.ShouldActivateWhenOpeningRoom(MapGuid))
					{
						scenarioModifier2.SetDeactivated(deactivate: false);
					}
					if (scenarioModifier2.ShouldTriggerWhenOpeningRoom(MapGuid))
					{
						scenarioModifier2.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
					}
				}
			}
			Revealed = true;
			m_ReveledInternal = true;
		}
		else if (fromActorOpeningDoor)
		{
			foreach (CSpawner spawner2 in Spawners)
			{
				if (spawner2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				if (!spawner2.IsActive && spawner2.SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.RoomOpen && spawner2.ActivationsCount == 0)
				{
					spawner2.SetActive(active: true);
					if (spawner2.SpawnerData.SpawnerTriggerType == SpawnerData.ESpawnerTriggerType.StartRound && spawner2.ArrayIndex != null)
					{
						spawner2.OnRoomOpened(ScenarioManager.CurrentScenarioState.Players.Count, initial);
					}
				}
				if (spawner2.Prop != null)
				{
					CSpawn_MessageData message2 = new CSpawn_MessageData(null)
					{
						m_SpawnDelay = 0f,
						m_Prop = spawner2.Prop
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
		}
		foreach (ScenarioScript scenarioScript2 in ScenarioManager.CurrentScenarioState.ScenarioScripts)
		{
			scenarioScript2.ResetRevealedRoom(reveledInternal);
		}
		if (!reveledInternal)
		{
			foreach (CSpawner spawner3 in Spawners)
			{
				if (spawner3.IsActive && spawner3.SpawnerData.SpawnerTriggerType != SpawnerData.ESpawnerTriggerType.TriggeredExternally)
				{
					spawner3.UpdateWillSpawnNextRoundIndicator(ScenarioManager.CurrentScenarioState.Players.Count);
				}
			}
			CVars cVars = new CVars();
			cVars.Set("TriggerMap", new TValue(this));
			CVarsContext context = new CVarsContext(cVars);
			foreach (CPlayerActor player in ScenarioManager.Scenario.AllPlayers)
			{
				if (player.IsDeadForObjectives)
				{
					continue;
				}
				List<CAbility> list = (from a in player.CharacterClass.Perks.Where((PerksYMLData p) => (p.Flags & EPerkFlag.ExecuteAbilitiesAfterRoomRevealed) != 0).SelectMany((PerksYMLData p) => p.Abilities)
					select CAbility.CopyAbility(a, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, context)).ToList();
				if (list.Count <= 0)
				{
					continue;
				}
				foreach (CAbility item4 in list)
				{
					item4.SetMiscVar("ProcessWhilePhasedOut", true);
				}
				PhaseManager.StartAbilities(list.Select((CAbility x) => ((CAbility x, CActor))(x: x, player)).ToList(), stackInsteadOfInject: true);
			}
			CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericRoomRevealed, null, GameState.CurrentActor, GameState.CurrentActor, ("RoomRevealed", new TValue(this)));
		}
		CPlayEnvAudio_MessageData message3 = new CPlayEnvAudio_MessageData(MapInstanceName);
		ScenarioRuleClient.MessageHandler(message3);
	}

	private void RevealProps()
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile == null || (cTile.m_HexMap != this && cTile.m_Hex2Map != this))
			{
				continue;
			}
			foreach (CObjectProp prop2 in cTile.m_Props)
			{
				if (prop2 is CObjectDoor prop)
				{
					CRevealDoor_MessageData message = new CRevealDoor_MessageData(null)
					{
						m_Prop = prop
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				else if (!prop2.Activated || prop2.ObjectType == ScenarioManager.ObjectImportType.PressurePlate)
				{
					CSpawn_MessageData message2 = new CSpawn_MessageData(null)
					{
						m_SpawnDelay = 0f,
						m_Prop = prop2
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
		}
	}

	public static bool TryCreatePropWithHealthDummy(CObjectProp prop, int health, CTile tile, bool initial, bool noIDRegen, bool performSpawnedModifiers, out CObjectActor dummy)
	{
		if (prop.PropHealthDetails == null)
		{
			dummy = null;
			return false;
		}
		prop.PropHealthDetails.CurrentHealth = health;
		ObjectState objectState = prop.PropHealthDetails.CreateStateForPropWithHealth(tile, prop.PropHealthDetails.CurrentHealth);
		ScenarioManager.CurrentScenarioState.Objects.Add(objectState);
		dummy = ScenarioManager.Scenario.AddObject(objectState, initial, noIDRegen, dummyObjectForProp: true);
		if (dummy != null)
		{
			objectState.InitialisePropAttachment(prop, dummy);
			if (performSpawnedModifiers && !initial)
			{
				ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(dummy, ScenarioManager.CurrentScenarioState.Objects.Count);
			}
			return true;
		}
		return false;
	}

	private void CreateHealthPropDummies(bool initial, bool noIDRegen = false, bool forLevelEditor = false)
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile == null || (cTile.m_HexMap != this && cTile.m_Hex2Map != this))
			{
				continue;
			}
			foreach (CObjectProp prop in cTile.m_Props)
			{
				if (forLevelEditor || prop.PropHealthDetails == null || !prop.PropHealthDetails.HasHealth)
				{
					continue;
				}
				if (!prop.PropActorHasBeenAssigned)
				{
					if (prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count) != ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						int propStartingHealth = prop.PropHealthDetails.GetPropStartingHealth();
						TryCreatePropWithHealthDummy(prop, propStartingHealth, cTile, initial, noIDRegen, !forLevelEditor, out var _);
					}
					continue;
				}
				ObjectState existingObjectState = ScenarioManager.CurrentScenarioState.Objects.SingleOrDefault((ObjectState x) => x.PropGuidAttachedTo == prop.PropGuid);
				if (existingObjectState?.Object == null && !ScenarioManager.Scenario.InitialObjects.Any((CObjectActor x) => x.ActorGuid == existingObjectState.ActorGuid) && ScenarioManager.Scenario.AddObject(existingObjectState, initial, noIDRegen, existingObjectState.IsAttachedToProp) == null)
				{
					ScenarioManager.CurrentScenarioState.Objects.Remove(existingObjectState);
				}
			}
		}
	}

	public void Save(bool saveHiddenUnits = false)
	{
		foreach (PlayerState player in Players)
		{
			player.Save(initial: false, saveHiddenUnits);
		}
		foreach (HeroSummonState heroSummon in HeroSummons)
		{
			heroSummon.Save(initial: false);
		}
		foreach (EnemyState monster in Monsters)
		{
			monster.Save(initial: false, saveHiddenUnits);
		}
		foreach (EnemyState allyMonster in AllyMonsters)
		{
			allyMonster.Save(initial: false, saveHiddenUnits);
		}
		foreach (EnemyState enemy2Monster in Enemy2Monsters)
		{
			enemy2Monster.Save(initial: false, saveHiddenUnits);
		}
		foreach (EnemyState neutralMonster in NeutralMonsters)
		{
			neutralMonster.Save(initial: false, saveHiddenUnits);
		}
		foreach (ObjectState @object in Objects)
		{
			@object.Save(initial: false, saveHiddenUnits);
		}
	}

	public void Load(bool forLevelEditor)
	{
		if (!(Revealed | forLevelEditor))
		{
			return;
		}
		foreach (PlayerState item in Players.Where((PlayerState p) => !p.HiddenAtStart))
		{
			item.Load();
		}
		foreach (PlayerState item2 in Players.Where((PlayerState p) => !p.HiddenAtStart))
		{
			item2.LoadAbiltyDeck();
		}
		foreach (HeroSummonState heroSummon in HeroSummons)
		{
			heroSummon.Load();
		}
		foreach (EnemyState monster in Monsters)
		{
			ScenarioManager.EPerPartySizeConfig configForPartySize = monster.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
			if (forLevelEditor || configForPartySize != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				monster.Load();
			}
		}
		foreach (EnemyState allyMonster in AllyMonsters)
		{
			ScenarioManager.EPerPartySizeConfig configForPartySize2 = allyMonster.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
			if (forLevelEditor || configForPartySize2 != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				allyMonster.Load();
			}
		}
		foreach (EnemyState enemy2Monster in Enemy2Monsters)
		{
			ScenarioManager.EPerPartySizeConfig configForPartySize3 = enemy2Monster.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
			if (forLevelEditor || configForPartySize3 != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				enemy2Monster.Load();
			}
		}
		foreach (EnemyState neutralMonster in NeutralMonsters)
		{
			ScenarioManager.EPerPartySizeConfig configForPartySize4 = neutralMonster.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
			if (forLevelEditor || configForPartySize4 != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				neutralMonster.Load();
			}
		}
		foreach (ObjectState @object in Objects)
		{
			ScenarioManager.EPerPartySizeConfig configForPartySize5 = @object.GetConfigForPartySize(ScenarioManager.CurrentScenarioState.Players.Count);
			if (forLevelEditor || configForPartySize5 != ScenarioManager.EPerPartySizeConfig.Hidden)
			{
				@object.Load();
			}
		}
	}

	public void Reset()
	{
		m_ReveledInternal = false;
	}

	public static void SetChildren(List<CMap> allMaps)
	{
		CMap cMap = null;
		foreach (CMap map in allMaps)
		{
			if (map.ParentName != null && map.ParentName != string.Empty)
			{
				CMap cMap2 = allMaps.SingleOrDefault((CMap x) => x.RoomName == map.ParentName);
				if (cMap2 != null)
				{
					cMap2.Children.Add(map.MapGuid);
					continue;
				}
				Log.ErrorWithReport("Unable to find map parent with name " + map.ParentName + ".  Map will be added linearly.");
			}
			if (cMap != null)
			{
				map.ParentName = cMap.RoomName;
				cMap.Children.Add(map.MapGuid);
			}
			cMap = map;
		}
	}

	public static List<Tuple<int, string>> Compare(CMap map1, CMap map2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (map1.IslandID != map2.IslandID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2901, "Map IslandID does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"IslandID",
						map1.IslandID.ToString(),
						map2.IslandID.ToString()
					}
				});
			}
			if (map1.RoomName != map2.RoomName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2902, "Map RoomName does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName }
				});
			}
			if (map1.MapType != map2.MapType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2903, "Map MapType does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName }
				});
			}
			if (map1.Revealed != map2.Revealed)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2904, "Map Revealed state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Revealed",
						map1.Revealed.ToString(),
						map2.Revealed.ToString()
					}
				});
			}
			if (map1.Destroyed != map2.Destroyed)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2904, "Map Destroyed state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Destroyed",
						map1.Destroyed.ToString(),
						map2.Destroyed.ToString()
					}
				});
			}
			if (map1.ParentName != map2.ParentName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2905, "Map ParentName state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3] { "ParentName", map1.ParentName, map2.ParentName }
				});
			}
			if (map1.ScenarioRoomName != map2.ScenarioRoomName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2906, "Map ScenarioRoomName state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3] { "ScenarioRoomName", map1.ScenarioRoomName, map2.ScenarioRoomName }
				});
			}
			if ((map1.SelectedPossibleRoom == null && map2.SelectedPossibleRoom != null) || (map1.SelectedPossibleRoom != null && map2.SelectedPossibleRoom == null))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2907, "Map SelectedPossibleRoom null state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"SelectedPossibleRoom",
						(map1.SelectedPossibleRoom == null) ? "is null" : "is not null",
						(map2.SelectedPossibleRoom == null) ? "is null" : "is not null"
					}
				});
			}
			else if (map1.SelectedPossibleRoom != null && map2.SelectedPossibleRoom != null && map1.SelectedPossibleRoom.Name != map2.SelectedPossibleRoom.Name)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2908, "Map SelectedPossibleRoom Name state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"SelectedPossibleRoom",
						map1.SelectedPossibleRoom.Name,
						map2.SelectedPossibleRoom.Name
					}
				});
			}
			if (!CVector3.Compare(map1.Position, map2.Position))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2909, "Map Position state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Position",
						map1.Position.ToString(),
						map2.Position.ToString()
					}
				});
			}
			if (!CVector3.Compare(map1.Rotation, map2.Rotation))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2910, "Map Rotation state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Rotation",
						map1.Rotation.ToString(),
						map2.Rotation.ToString()
					}
				});
			}
			if (!CVector3.Compare(map1.Centre, map2.Centre))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2911, "Map Centre state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Centre",
						map1.Centre.ToString(),
						map2.Centre.ToString()
					}
				});
			}
			if (!CVector3.Compare(map1.ClosestTileIdentityPosition, map2.ClosestTileIdentityPosition))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2912, "Map ClosestTileIdentityPosition state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"ClosestTileIdentityPosition",
						map1.ClosestTileIdentityPosition.ToString(),
						map2.ClosestTileIdentityPosition.ToString()
					}
				});
			}
			if (!CVector3.CompareFloats(map1.Angle, map2.Angle))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2913, "Map Angle state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Angle",
						map1.Angle.ToString(),
						map2.Angle.ToString()
					}
				});
			}
			if (isMPCompare)
			{
				if ((map1.DefaultLevelFlowNormal == null && map2.DefaultLevelFlowNormal != null) || (map1.DefaultLevelFlowNormal != null && map2.DefaultLevelFlowNormal == null))
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2914, "Map DefaultLevelFlowNormal null state does not match.", new List<string[]>
					{
						new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
						new string[3]
						{
							"Map Type",
							map1.MapType.ToString(),
							map2.MapType.ToString()
						},
						new string[3] { "RoomName", map1.RoomName, map2.RoomName },
						new string[3]
						{
							"DefaultLevelFlowNormal",
							(map1.DefaultLevelFlowNormal == null) ? "is null" : "is not null",
							(map2.DefaultLevelFlowNormal == null) ? "is null" : "is not null"
						}
					});
				}
				else if (map1.DefaultLevelFlowNormal != null && map2.DefaultLevelFlowNormal != null && !CVector3.Compare(map1.DefaultLevelFlowNormal, map2.DefaultLevelFlowNormal))
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2915, "Map DefaultLevelFlowNormal Name state does not match.", new List<string[]>
					{
						new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
						new string[3]
						{
							"Map Type",
							map1.MapType.ToString(),
							map2.MapType.ToString()
						},
						new string[3] { "RoomName", map1.RoomName, map2.RoomName },
						new string[3]
						{
							"DefaultLevelFlowNormal",
							map1.DefaultLevelFlowNormal.ToString(),
							map2.DefaultLevelFlowNormal.ToString()
						}
					});
				}
			}
			if (map1.EntranceDoor != map2.EntranceDoor)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2916, "Map EntranceDoor state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3] { "EntranceDoor", map1.EntranceDoor, map2.EntranceDoor }
				});
			}
			if (map1.ExitDoors.Count != map2.ExitDoors.Count)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2917, "Map ExitDoors Count does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"ExitDoors Count",
						map1.ExitDoors.Count.ToString(),
						map2.ExitDoors.Count.ToString()
					}
				});
			}
			else
			{
				foreach (string exitDoor in map1.ExitDoors)
				{
					if (!map2.ExitDoors.Contains(exitDoor))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2918, "Map ExitDoors on " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing an exit door from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
							new string[3]
							{
								"Map Type",
								map1.MapType.ToString(),
								map2.MapType.ToString()
							},
							new string[3] { "RoomName", map1.RoomName, map2.RoomName },
							new string[3]
							{
								"ExitDoors",
								string.Join(", ", map1.ExitDoors),
								string.Join(", ", map2.ExitDoors)
							}
						});
					}
				}
				foreach (string exitDoor2 in map2.ExitDoors)
				{
					if (!map1.ExitDoors.Contains(exitDoor2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2918, "Map ExitDoors on " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing an exit door from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
							new string[3]
							{
								"Map Type",
								map1.MapType.ToString(),
								map2.MapType.ToString()
							},
							new string[3] { "RoomName", map1.RoomName, map2.RoomName },
							new string[3]
							{
								"ExitDoors",
								string.Join(", ", map1.ExitDoors),
								string.Join(", ", map2.ExitDoors)
							}
						});
					}
				}
			}
			if (map1.Children.Count != map2.Children.Count)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2919, "Map Children Count does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"Children Count",
						map1.Children.Count.ToString(),
						map2.Children.Count.ToString()
					}
				});
			}
			else
			{
				foreach (string child in map1.Children)
				{
					if (!map2.Children.Contains(child))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2920, "Map Children on " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a child from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
							new string[3]
							{
								"Map Type",
								map1.MapType.ToString(),
								map2.MapType.ToString()
							},
							new string[3] { "RoomName", map1.RoomName, map2.RoomName },
							new string[3]
							{
								"Children",
								string.Join(", ", map1.Children),
								string.Join(", ", map2.Children)
							}
						});
					}
				}
				foreach (string child2 in map2.Children)
				{
					if (!map1.Children.Contains(child2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2920, "Map Children on " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a child from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
							new string[3]
							{
								"Map Type",
								map1.MapType.ToString(),
								map2.MapType.ToString()
							},
							new string[3] { "RoomName", map1.RoomName, map2.RoomName },
							new string[3]
							{
								"Children",
								string.Join(", ", map1.Children),
								string.Join(", ", map2.Children)
							}
						});
					}
				}
			}
			if (map1.DungeonEntranceDoor != map2.DungeonEntranceDoor)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2921, "Map DungeonEntranceDoor state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3] { "DungeonEntranceDoor", map1.DungeonEntranceDoor, map2.DungeonEntranceDoor }
				});
			}
			if (map1.DungeonExitDoor != map2.DungeonExitDoor)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2922, "Map DungeonExitDoor state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3] { "DungeonExitDoor", map1.DungeonExitDoor, map2.DungeonExitDoor }
				});
			}
			if (map1.IsDungeonExitRoom != map2.IsDungeonExitRoom)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2923, "Map IsDungeonExitRoom state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"IsDungeonExitRoom",
						map1.IsDungeonExitRoom.ToString(),
						map2.IsDungeonExitRoom.ToString()
					}
				});
			}
			if (map1.IsAdditionalDungeonEntranceRoom != map2.IsAdditionalDungeonEntranceRoom)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2924, "Map IsAdditionalDungeonEntranceRoom state does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", map1.MapGuid, map2.MapGuid },
					new string[3]
					{
						"Map Type",
						map1.MapType.ToString(),
						map2.MapType.ToString()
					},
					new string[3] { "RoomName", map1.RoomName, map2.RoomName },
					new string[3]
					{
						"IsAdditionalDungeonEntranceRoom",
						map1.IsAdditionalDungeonEntranceRoom.ToString(),
						map2.IsAdditionalDungeonEntranceRoom.ToString()
					}
				});
			}
			bool flag = false;
			foreach (CMapTile mapTile1 in map1.MapTiles)
			{
				if (map1.MapTiles.Where((CMapTile w) => w.TileGuid == mapTile1.TileGuid).Count() > 1)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2925, "Duplicate entry in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " state Map Tiles list.", new List<string[]>
					{
						new string[3] { "Map GUID", map1.MapGuid, "NA" },
						new string[3]
						{
							"Map Type",
							map1.MapType.ToString(),
							"NA"
						},
						new string[3] { "RoomName", map1.RoomName, "NA" },
						new string[3] { "Map Tile", mapTile1.TileGuid, "NA" },
						new string[3]
						{
							"Map Tile Position",
							mapTile1.Position.ToString(),
							"NA"
						},
						new string[3]
						{
							"Duplicate Count",
							map1.MapTiles.Where((CMapTile w) => w.TileGuid == mapTile1.TileGuid).Count().ToString(),
							"NA"
						}
					});
					flag = true;
				}
			}
			foreach (CMapTile mapTile2 in map2.MapTiles)
			{
				if (map2.MapTiles.Where((CMapTile w) => w.TileGuid == mapTile2.TileGuid).Count() > 1)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2925, "Duplicate entry in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " state Map Tiles list.", new List<string[]>
					{
						new string[3] { "Map GUID", "NA", map2.MapGuid },
						new string[3]
						{
							"Map Type",
							"NA",
							map2.MapType.ToString()
						},
						new string[3] { "RoomName", "NA", map2.RoomName },
						new string[3] { "Map Tile", "NA", mapTile2.TileGuid },
						new string[3]
						{
							"Map Tile Position",
							"NA",
							mapTile2.Position.ToString()
						},
						new string[3]
						{
							"Duplicate Count",
							"NA",
							map2.MapTiles.Where((CMapTile w) => w.TileGuid == mapTile2.TileGuid).Count().ToString()
						}
					});
					flag = true;
				}
			}
			if (map1.MapTiles.Count != map2.MapTiles.Count)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2926, "Number of Map Tiles does not match.", new List<string[]>
				{
					new string[3]
					{
						"Map Tiles Count",
						map1.MapTiles.Count.ToString(),
						map2.MapTiles.Count.ToString()
					},
					new string[3]
					{
						"Map Tiles",
						string.Join(", ", map1.MapTiles.Select((CMapTile s) => s.TileGuid + " (" + s.Position.ToString() + ") ")),
						string.Join(", ", map2.MapTiles.Select((CMapTile s) => s.TileGuid + " (" + s.Position.ToString() + ") "))
					}
				});
			}
			else if (!flag)
			{
				bool flag2 = false;
				foreach (CMapTile mapTile3 in map1.MapTiles)
				{
					if (!map2.MapTiles.Exists((CMapTile e) => e.TileGuid == mapTile3.TileGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2927, "Map Tile in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " state could not be found in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " state.", new List<string[]>
						{
							new string[3] { "Map GUID", map1.MapGuid, "NA" },
							new string[3]
							{
								"Map Type",
								map1.MapType.ToString(),
								"NA"
							},
							new string[3] { "RoomName", map1.RoomName, "NA" },
							new string[3] { "Map Tile", mapTile3.TileGuid, "NA" },
							new string[3]
							{
								"Map Tile Position",
								mapTile3.Position.ToString(),
								"NA"
							}
						});
						flag2 = true;
					}
				}
				foreach (CMapTile mapTile4 in map2.MapTiles)
				{
					if (!map1.MapTiles.Exists((CMapTile e) => e.TileGuid == mapTile4.TileGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2927, "Map Tile in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " state could not be found in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " state.", new List<string[]>
						{
							new string[3] { "Map GUID", "NA", map2.MapGuid },
							new string[3]
							{
								"Map Type",
								"NA",
								map2.MapType.ToString()
							},
							new string[3] { "RoomName", "NA", map2.RoomName },
							new string[3] { "Map Tile", "NA", mapTile4.TileGuid },
							new string[3]
							{
								"Map Tile Position",
								"NA",
								mapTile4.Position.ToString()
							}
						});
						flag2 = true;
					}
				}
				if (!flag2)
				{
					foreach (CMapTile mapTile5 in map1.MapTiles)
					{
						try
						{
							CMapTile mapTile6 = map2.MapTiles.Single((CMapTile s) => s.TileGuid == mapTile5.TileGuid);
							list.AddRange(CMapTile.Compare(mapTile5, mapTile6, map1.MapGuid, map1.MapType, map1.RoomName, isMPCompare));
						}
						catch (Exception ex)
						{
							list.Add(new Tuple<int, string>(2928, "Exception during map tile compare.\n" + ex.Message + "\n" + ex.StackTrace));
						}
					}
				}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(2999, "Exception during map compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}

	public void SetRoguelikeScenarioPossibleRoom(ScenarioPossibleRoom possibleRoom)
	{
		SelectedPossibleRoom = possibleRoom;
	}
}
