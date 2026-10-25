using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH078 : ScenarioScript
{
	[Serializable]
	public class Storage78
	{
		public List<string> MonsterGuidsInRoom13B = new List<string>();

		public bool MonstersInRoom13BKilled;

		public string Room4AEliteLurkerSoldierGUID;

		public List<MonsterOrderInfo> MonsterKillOrder = new List<MonsterOrderInfo>();

		public List<MonsterOrderInfo> KilledOrder = new List<MonsterOrderInfo>();
	}

	[Serializable]
	public class MonsterOrderInfo
	{
		public string ActorGUID;

		public string CounterGUID;

		[JsonIgnore]
		public CActor Actor => ScenarioManager.FindActor(ActorGUID);

		[JsonIgnore]
		public CObjectProp Prop
		{
			get
			{
				if (CounterGUID != null)
				{
					return ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp x) => x.PropGuid == CounterGUID);
				}
				return null;
			}
			set
			{
				CounterGUID = value?.PropGuid;
			}
		}

		public bool Valid => CounterGUID != null;
	}

	private Storage78 serializedProperties;

	private CMap Room2C;

	private CObjectDoor Door1;

	private CObjectDoor Door2;

	private CObjectDoor Door4;

	private CMap Room10B;

	private CObjective_Scripted ExploreCaves;

	private CMap Room13B;

	private CMap Room13D;

	private CObjectDoor Door3;

	private CObjectDoor DoorA;

	private CMap Room2J;

	private CMap Room4A;

	private CSpawner SpawnerB;

	private CObjectDoor Door5;

	private CObjectDoor Door6;

	private CObjective_Scripted KillEnemiesObjective;

	private CObjective_Scripted KillEnemiesAscending;

	private CScenarioModifier SpawnerModifier;

	public List<MonsterOrderInfo> MonsterKillOrder => serializedProperties?.MonsterKillOrder;

	public List<MonsterOrderInfo> KilledOrder => serializedProperties?.KilledOrder;

	public List<string> MonsterGuidsInRoom13B
	{
		get
		{
			return serializedProperties.MonsterGuidsInRoom13B;
		}
		private set
		{
			serializedProperties.MonsterGuidsInRoom13B = value;
		}
	}

	public List<CEnemyActor> MonstersInRoom13B => ScenarioManager.Scenario.AllMonsters.Where((CEnemyActor x) => MonsterGuidsInRoom13B.Contains(x.ActorGuid)).ToList();

	public bool MonstersInRoom13BKilled
	{
		get
		{
			return serializedProperties.MonstersInRoom13BKilled;
		}
		private set
		{
			serializedProperties.MonstersInRoom13BKilled = value;
		}
	}

	public string Room4AEliteLurkerSoldierGUID
	{
		get
		{
			return serializedProperties.Room4AEliteLurkerSoldierGUID;
		}
		private set
		{
			serializedProperties.Room4AEliteLurkerSoldierGUID = value;
		}
	}

	public CEnemyActor Room4AEliteLurkerSoldier
	{
		get
		{
			return ScenarioManager.Scenario.AllMonsters.SingleOrDefault((CEnemyActor x) => x.ActorGuid == Room4AEliteLurkerSoldierGUID);
		}
		private set
		{
			Room4AEliteLurkerSoldierGUID = value?.ActorGuid ?? string.Empty;
		}
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			return;
		}
		serializedProperties = (Storage78)JsonConvert.DeserializeObject(storage, typeof(Storage78));
		for (int i = 0; i < MonsterKillOrder.Count; i++)
		{
			MonsterOrderInfo monsterOrderInfo = MonsterKillOrder[i];
			if (monsterOrderInfo.Valid)
			{
				CChangePropVisualState_MessageData message = new CChangePropVisualState_MessageData
				{
					m_prop = monsterOrderInfo.Prop,
					m_stateName = "State_" + (i + 1)
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage78();
		Room2C = ScenarioScript.FindRoom("Room_2_C");
		Door1 = ScenarioScript.FindDoor(10, 7);
		Door2 = ScenarioScript.FindDoor(8, 5);
		Door4 = ScenarioScript.FindDoor(4, 7);
		Room10B = ScenarioScript.FindRoom("Room_10_B");
		Room13B = ScenarioScript.FindRoom("Room_13_B");
		Room13D = ScenarioScript.FindRoom("Room_13_D");
		Door3 = ScenarioScript.FindDoor(7, 11);
		DoorA = ScenarioScript.FindDoor(10, 15);
		Room2J = ScenarioScript.FindRoom("Room_2_J");
		Door5 = ScenarioScript.FindDoor(4, 15);
		Door6 = ScenarioScript.FindDoor(7, 17);
		Room4A = ScenarioScript.FindRoom("Room_4_A");
		SpawnerB = ScenarioScript.FindSpawner(3, 16);
		SpawnerModifier = GetScenarioModifier("SpawnerModifier");
		KillEnemiesObjective = GetObjective<CObjective_Scripted>("KillAllEnemies");
		KillEnemiesAscending = GetObjective<CObjective_Scripted>("KillEnemiesAscending");
		ExploreCaves = GetObjective<CObjective_Scripted>("ExploreCaves");
		ExploreCaves.Total = 1;
		return true;
	}

	private void UpdateKillObjective()
	{
		List<CEnemyActor> list = ScenarioManager.Scenario.AllEnemies.ToList();
		KillEnemiesObjective.Total = list.Count;
		KillEnemiesObjective.Current = list.Count((CEnemyActor x) => x.IsDead);
		KillEnemiesObjective.ScriptedIsComplete = KillEnemiesObjective.Current == KillEnemiesObjective.Total;
		CheckObjectivesComplete();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
		{
			UpdateKillObjective();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario)
		{
			SetupInitialDoorLocks();
		}
	}

	private void SetupInitialDoorLocks()
	{
		Door1.UnlockLockedDoorWithoutOpening();
		Door2.UnlockLockedDoorWithoutOpening();
		Door5.UnlockLockedDoorWithoutOpening();
		Door6.UnlockLockedDoorWithoutOpening();
	}

	protected override void TickDeathInternal()
	{
		CacheKillOrder();
		CheckRoom13BMonstersDead();
		CheckRoom4AEliteLurkerSoldierDead();
		UpdateKillObjective();
	}

	private void CacheKillOrder()
	{
		CActor tickActor = _tickActor;
		foreach (MonsterOrderInfo item in MonsterKillOrder)
		{
			if (item.ActorGUID == tickActor.ActorGuid)
			{
				KilledOrder.Add(new MonsterOrderInfo
				{
					ActorGUID = item.ActorGUID,
					CounterGUID = item.CounterGUID
				});
				item.Prop?.DestroyProp();
				item.Prop = null;
				break;
			}
		}
	}

	private void CheckRoom13BMonstersDead()
	{
		if (MonstersInRoom13BKilled)
		{
			return;
		}
		CActor tickActor = _tickActor;
		if (!MonstersInRoom13B.Contains(tickActor))
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < Math.Min(MonsterKillOrder.Count, KilledOrder.Count); i++)
		{
			MonsterOrderInfo monsterOrderInfo = MonsterKillOrder[i];
			MonsterOrderInfo monsterOrderInfo2 = KilledOrder[i];
			if (monsterOrderInfo.ActorGUID != monsterOrderInfo2.ActorGUID)
			{
				flag = false;
				break;
			}
		}
		KillEnemiesAscending.Current = KilledOrder.Count;
		KillEnemiesAscending.ScriptedUnableToComplete = !flag;
		if (flag)
		{
			KillEnemiesAscending.ScriptedIsComplete = KillEnemiesAscending.Current >= KillEnemiesAscending.Total;
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		if (MonstersInRoom13B.All((CEnemyActor x) => x.IsDead))
		{
			MonstersInRoom13BKilled = true;
			if (flag)
			{
				DoorA.UnlockLockedDoorWithoutOpening();
				GameState.TriggerCampaignSection("FS_078_The_Lurker_Problem_Special_1_1");
			}
			else
			{
				Door3.UnlockLockedDoorWithoutOpening();
				GameState.TriggerCampaignSection("FS_078_The_Lurker_Problem_Special_2_1");
			}
		}
	}

	private void CheckRoom4AEliteLurkerSoldierDead()
	{
		CActor tickActor = _tickActor;
		CEnemyActor room4AEliteLurkerSoldier = Room4AEliteLurkerSoldier;
		if (tickActor == room4AEliteLurkerSoldier)
		{
			SpawnerB.SetActive(active: false);
			SpawnerModifier.SetDeactivated(deactivate: true);
		}
	}

	protected override int GetConclusionIndexInternal()
	{
		if (Door5.DoorIsOpen)
		{
			return 1;
		}
		return 0;
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == Room13B)
		{
			Door2.SetHasExtraLock(hasExtraLock: true);
			Door3.SetHasExtraLock(hasExtraLock: true);
			Door4.SetHasExtraLock(hasExtraLock: true);
			IEnumerable<CEnemyActor> source = ScenarioManager.Scenario.AllAliveMonsters.Where((CEnemyActor x) => x.Tile.m_HexMap.MapGuid == Room13B.MapGuid);
			MonsterGuidsInRoom13B.AddRange(source.Select((CEnemyActor x) => x.ActorGuid));
			List<CEnemyActor> list = (from x in ScenarioManager.Scenario.Enemies.Concat(ScenarioManager.Scenario.Enemy2Monsters)
				where x.Tile.m_HexMap == Room13B
				select x).ToList();
			list.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
			for (int num = 0; num < list.Count; num++)
			{
				CEnemyActor cEnemyActor = list[num];
				CTile tile = cEnemyActor.Tile;
				CObjectProp cObjectProp = SpawnVFXProp(tile, EVFXPropType.NumberCounterActor.ToString());
				CChangePropVisualState_MessageData message = new CChangePropVisualState_MessageData
				{
					m_prop = cObjectProp,
					m_stateName = "State_" + (num + 1)
				};
				ScenarioRuleClient.MessageHandler(message);
				MonsterKillOrder.Add(new MonsterOrderInfo
				{
					ActorGUID = cEnemyActor.ActorGuid,
					CounterGUID = cObjectProp.PropGuid
				});
			}
			KillEnemiesAscending.SetActivation(active: true);
			KillEnemiesAscending.Total = MonsterKillOrder.Count;
		}
		else if (_tickRoomRevealed == Room10B)
		{
			Door1.SetHasExtraLock(hasExtraLock: true);
		}
		else if (_tickRoomRevealed == Room13D)
		{
			if (Room13B.Revealed)
			{
				GameState.TriggerCampaignSection("FS_078_The_Lurker_Problem_Special_Room_13_D_Door_3");
				return;
			}
			Door3.SetHasExtraLock(hasExtraLock: true);
			GameState.TriggerCampaignSection("FS_078_The_Lurker_Problem_Special_Room_13_D_Door_4");
		}
		else if (_tickRoomRevealed == Room4A)
		{
			ExploreCaves.Current = 1;
			ExploreCaves.ScriptedIsComplete = true;
			KillEnemiesObjective.SetActivation(active: true);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			if (Room2J.Revealed)
			{
				SpawnerB.SetActive(active: true, forceDontSpawn: true);
				SpawnerB.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
				SpawnerModifier.SetDeactivated(deactivate: false);
				Room4AEliteLurkerSoldier = ScenarioScript.FindActor(4, 20) as CEnemyActor;
				Door5.SetHasExtraLock(hasExtraLock: true);
				GameState.TriggerCampaignSection("FS_078_The_Lurker_Problem_Special_Room_4_A_Door_6");
			}
			else
			{
				Door6.SetHasExtraLock(hasExtraLock: true);
				GameState.TriggerCampaignSection("FS_078_The_Lurker_Problem_Special_Room_4_A_Door_5");
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		CTile tile = _tickActor.Tile;
		foreach (MonsterOrderInfo item in MonsterKillOrder)
		{
			if (item.ActorGUID == _tickActor.ActorGuid)
			{
				CObjectProp prop = item.Prop;
				if (prop != null)
				{
					prop.PropTile.DelProp(prop);
					prop.SetLocation(new TileIndex(tile.m_ArrayIndex), null, prop.Rotation);
					tile.AddProp(prop);
					prop.SetNewStartingMapGuid(tile.m_HexMap.MapGuid);
				}
			}
		}
	}

	protected override void OnStartRevealingRoom(bool wasRevealed)
	{
		base.OnStartRevealingRoom(wasRevealed);
		if (!wasRevealed && _tickRoomRevealed == Room13D)
		{
			if (Room13B.Revealed)
			{
				ModifyMonsterSetup(4, 14, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(5, 13, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(6, 13, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(7, 10, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(4, 11, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
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
				});
				ModifyMonsterSetup(3, 10, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.Normal
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.Normal
					}
				});
				ModifyMonsterSetup(4, 9, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.Normal
					}
				});
				ModifyMonsterSetup(6, 8, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
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
				});
			}
			else
			{
				ModifyMonsterSetup(4, 14, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.Normal
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.Normal
					}
				});
				ModifyMonsterSetup(5, 13, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
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
				});
				ModifyMonsterSetup(6, 13, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.Normal
					}
				});
				ModifyMonsterSetup(7, 10, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
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
				});
				ModifyMonsterSetup(4, 11, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(3, 10, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(4, 9, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				ModifyMonsterSetup(6, 8, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
				{
					{
						1,
						ScenarioManager.EPerPartySizeConfig.Hidden
					},
					{
						2,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						3,
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
			}
		}
	}
}
