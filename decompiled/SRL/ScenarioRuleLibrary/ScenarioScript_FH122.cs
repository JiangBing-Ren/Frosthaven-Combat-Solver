using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH122 : ScenarioScript
{
	[Serializable]
	public class Storage122
	{
		public bool HungryMawReachedHalfHealth;
	}

	private const string HungryMawClassID = "TheMawID";

	private static readonly List<string> ImpClasses = new List<string> { "SnowImpID", "SnowImpEliteID" };

	private CObjectDoor Door1;

	private CObjectDoor Door2;

	private CObjectDoor Door3;

	private CObjectDoor Door4;

	private CObjectDoor Door5;

	private CObjectDoor Door6;

	private CMap Room2;

	private CMap Room3;

	private CMap Room4;

	private CMap Room5;

	private CMap Room6;

	private CSpawner SpawnerC;

	private CSpawner SpawnerD;

	private CSpawner SpawnerE;

	private CSpawner SpawnerF;

	private CObjective_Scripted OpenEitherDoor;

	private CObjective_KillAllBosses KillTheHungryMaw;

	private CObjective_LootX LootMammothBark;

	private CObjective_BringItemsToArea BringMammothBarkToPot;

	private CScenarioModifier Hint1;

	private CScenarioModifier Hint2;

	private CScenarioModifier Hint3;

	private CObjectChest ChestA;

	private CObjectChest ChestB;

	private Storage122 serializedProperties;

	private CEnemyActor HungryMaw => ScenarioManager.Scenario.AllEnemies.SingleOrDefault((CEnemyActor x) => x.Class.ID == "TheMawID");

	private bool HungryMawReachedHalfHealth
	{
		get
		{
			return serializedProperties.HungryMawReachedHalfHealth;
		}
		set
		{
			serializedProperties.HungryMawReachedHalfHealth = value;
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage122();
		Door1 = ScenarioScript.FindDoor(5, 21);
		Door2 = ScenarioScript.FindDoor(5, 16);
		Room2 = ScenarioScript.FindRoom("Room_2");
		Room3 = ScenarioScript.FindRoom("Room_3");
		Door3 = ScenarioScript.FindDoor(8, 10);
		Door4 = ScenarioScript.FindDoor(2, 7);
		Room4 = ScenarioScript.FindRoom("Room_4");
		Room5 = ScenarioScript.FindRoom("Room_5");
		Room6 = ScenarioScript.FindRoom("Room_6");
		Door5 = ScenarioScript.FindDoor(12, 7);
		Door6 = ScenarioScript.FindDoor(9, 5);
		SpawnerC = ScenarioScript.FindSpawner(9, 10);
		SpawnerD = ScenarioScript.FindSpawner(11, 12);
		SpawnerE = ScenarioScript.FindSpawner(2, 10);
		SpawnerF = ScenarioScript.FindSpawner(4, 15);
		OpenEitherDoor = GetObjective<CObjective_Scripted>("OpenEitherDoor");
		OpenEitherDoor.Total = 1;
		KillTheHungryMaw = GetObjective<CObjective_KillAllBosses>("KillTheHungryMaw");
		LootMammothBark = GetObjective<CObjective_LootX>("LootMammothBark");
		BringMammothBarkToPot = GetObjective<CObjective_BringItemsToArea>("BringMammothBarkToPot");
		Hint1 = GetScenarioModifier("Hint1");
		Hint2 = GetScenarioModifier("Hint2");
		Hint3 = GetScenarioModifier("Hint3");
		ChestA = ScenarioScript.FindChest(5, 8);
		ChestB = ScenarioScript.FindChest(12, 0);
		return base.Initialize();
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			UpdateHungryMawDamageImmunity();
			return;
		}
		serializedProperties = (Storage122)JsonConvert.DeserializeObject(storage, typeof(Storage122));
		UpdateHungryMawDamageImmunity();
		UpdateTileHighlights();
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		base.TickTakenDamageInternal(damageTaken, damageReducedByShields, actualDamage);
		if (_tickActor.Class.ID == "TheMawID" && (double)_tickActor.Health <= Math.Floor((float)_tickActor.MaxHealth / 2f) && !HungryMawReachedHalfHealth)
		{
			HungryMawReachedHalfHealth = true;
			if (_tickActor.Health < 0)
			{
				ScenarioScript.HealTo(_tickActor, 1);
			}
			ScenarioScript.Heal(_tickActor, ScenarioManager.CurrentScenarioState.Level * ScenarioManager.Scenario.AllPlayers.Count, removeAllTokens: true);
			Door2.ForceActivate(_tickActor);
			UpdateHungryMawDamageImmunity();
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			if (_tickActor.Class.ID == "TheMawID")
			{
				UpdateHungryMawDamageImmunity();
			}
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			TickPhaseEndTurnLoot();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			Door1.UnlockLockedDoorWithoutOpening();
			Door2.UnlockLockedDoorWithoutOpening();
			Door3.UnlockLockedDoorWithoutOpening();
			Door4.UnlockLockedDoorWithoutOpening();
			Door5.UnlockLockedDoorWithoutOpening();
			Door6.UnlockLockedDoorWithoutOpening();
			break;
		}
	}

	private void UpdateHungryMawDamageImmunity()
	{
		CEnemyActor hungryMaw = HungryMaw;
		if (hungryMaw != null)
		{
			hungryMaw.IsImmuneToDamage = HungryMawReachedHalfHealth && ScenarioManager.Scenario.Enemies.Any((CEnemyActor x) => ImpClasses.Contains(x.Class.ID) && !x.IsDead);
			Hint2.SetDeactivated(!HungryMaw.IsImmuneToDamage || !HungryMawReachedHalfHealth);
		}
	}

	protected override void OnStartRevealingRoom(bool wasRevealed)
	{
		base.OnStartRevealingRoom(wasRevealed);
		if (wasRevealed)
		{
			return;
		}
		if (_tickRoomRevealed == Room3)
		{
			if (Room2.Revealed)
			{
				AddMonsterOnRoomReveal("PolarBearID", 5, 15, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
				AddMonsterOnRoomReveal("SnowImpID", 5, 11, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.Normal
					}
				});
				AddMonsterOnRoomReveal("SnowImpID", 6, 8, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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
			}
			else
			{
				AddMonsterOnRoomReveal("SnowImpID", 5, 15, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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
				AddMonsterOnRoomReveal("SnowImpID", 3, 10, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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
						ScenarioManager.EPerPartySizeConfig.ToElite
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.Normal
					}
				});
			}
		}
		else if (_tickRoomRevealed == Room6)
		{
			if (Room4.Revealed)
			{
				AddMonsterOnRoomReveal("SnowImpID", 13, 1, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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
						ScenarioManager.EPerPartySizeConfig.Normal
					},
					{
						4,
						ScenarioManager.EPerPartySizeConfig.ToElite
					}
				});
			}
			else
			{
				AddMonsterOnRoomReveal("SnowImpID", 13, 1, new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
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

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed == Room2)
		{
			Hint1.SetDeactivated(deactivate: true);
			OpenEitherDoor.Current = 1;
			OpenEitherDoor.ScriptedIsComplete = true;
			KillTheHungryMaw.SetActivation(active: true);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			Door2.SetHasExtraLock(hasExtraLock: true);
		}
		else if (_tickRoomRevealed == Room3)
		{
			if (Room2.Revealed)
			{
				Door3.SetHasExtraLock(hasExtraLock: true);
				Door4.SetHasExtraLock(hasExtraLock: true);
				GameState.TriggerCampaignSection("FS_122_The_Eternal_Crave_Special_3_1");
				return;
			}
			ChestA.DestroyProp();
			Hint1.SetDeactivated(deactivate: true);
			OpenEitherDoor.Current = 1;
			OpenEitherDoor.ScriptedIsComplete = true;
			LootMammothBark.SetActivation(active: true);
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			Door1.SetHasExtraLock(hasExtraLock: true);
			GameState.TriggerCampaignSection("FS_122_The_Eternal_Crave_Special_4_1");
		}
		else if (_tickRoomRevealed == Room4)
		{
			Door3.SetHasExtraLock(hasExtraLock: true);
		}
		else if (_tickRoomRevealed == Room5)
		{
			Door4.SetHasExtraLock(hasExtraLock: true);
		}
		else
		{
			if (_tickRoomRevealed != Room6)
			{
				return;
			}
			CTile tile = ScenarioManager.GetTile(11, 5);
			CObjectQuestItem cObjectQuestItem = new CObjectQuestItem(ESpecificPropType.MammothBark.ToString(), ScenarioManager.ObjectImportType.CarryableQuestItem, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
			cObjectQuestItem.PropCustomLocKey = "FH_Scenario_Campaign_122_SCENARIO_OBJECT_1";
			cObjectQuestItem.PropCustomDescriptionLocKey = "FH_Scenario_Campaign_122_SCENARIO_MODIFIER_3";
			tile.SpawnProp(cObjectQuestItem);
			if (Room4.Revealed)
			{
				Door5.SetHasExtraLock(hasExtraLock: true);
				return;
			}
			SpawnerC.SetActive(active: true, forceDontSpawn: true);
			SpawnerD.SetActive(active: true, forceDontSpawn: true);
			foreach (SpawnRoundEntry spawnRoundEntry in SpawnerC.SpawnRoundEntries)
			{
				_ = spawnRoundEntry;
				SpawnerC.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			}
			foreach (SpawnRoundEntry spawnRoundEntry2 in SpawnerD.SpawnRoundEntries)
			{
				_ = spawnRoundEntry2;
				SpawnerD.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			}
			SpawnerC.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			SpawnerD.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			Door6.SetHasExtraLock(hasExtraLock: true);
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (ImpClasses.Contains(_tickActor.Class.ID))
		{
			UpdateHungryMawDamageImmunity();
		}
	}

	private void TickPhaseEndTurnLoot()
	{
		CObjectProp lootedChest = _tickActor.CarriedQuestItems?.FirstOrDefault();
		AfterLootGoalChest(lootedChest);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (_tickAbility is CAbilityLoot { PropsLooted: var propsLooted })
		{
			CObjectProp lootedChest = propsLooted.SingleOrDefault((CObjectProp x) => x is CObjectQuestItem);
			AfterLootGoalChest(lootedChest);
		}
	}

	private void AfterLootGoalChest(CObjectProp lootedChest)
	{
		if (lootedChest == null || BringMammothBarkToPot.IsActive)
		{
			return;
		}
		BringMammothBarkToPot.SetActivation(active: true);
		Hint3.SetDeactivated(deactivate: false);
		SpawnerE.SetActive(active: true, forceDontSpawn: true);
		SpawnerF.SetActive(active: true, forceDontSpawn: true);
		foreach (SpawnRoundEntry spawnRoundEntry in SpawnerE.SpawnRoundEntries)
		{
			_ = spawnRoundEntry;
			SpawnerE.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		}
		foreach (SpawnRoundEntry spawnRoundEntry2 in SpawnerF.SpawnRoundEntries)
		{
			_ = spawnRoundEntry2;
			SpawnerF.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
		}
		SpawnerE.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		SpawnerF.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		UpdateTileHighlights();
		GameState.TriggerCampaignSection("FS_122_The_Eternal_Crave_Special_1_1");
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor.Type == CActor.EType.Player && BringMammothBarkToPot.IsActive)
		{
			List<TileIndex> tiles = BringMammothBarkToPot.AreaPositions;
			if (tiles.Contains(new TileIndex(_tickActorTile.m_ArrayIndex)) && ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor player) => tiles.Any((TileIndex tile) => tile.ToPoint() == player.ArrayIndex) && player.CarriedQuestItems.Count > 0))
			{
				CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.Find((CPlayerActor player) => player.CarriedQuestItems.Count > 0);
				CObjectProp cObjectProp = cPlayerActor.CarriedQuestItems[0];
				cPlayerActor.CarriedQuestItems.Clear();
				cObjectProp?.DestroyProp();
				GameState.TriggerCampaignSection("FS_122_The_Eternal_Crave_Special_2");
			}
		}
		BringMammothBarkToPot.CheckObjectiveComplete(ScenarioManager.Scenario.AllPlayers.Count);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void UpdateTileHighlights()
	{
		if (BringMammothBarkToPot.IsActive)
		{
			ScenarioManager.ToggleTileHighlights(BringMammothBarkToPot.AreaPositions.Select((TileIndex x) => x.ToPoint()), enabled: true);
		}
	}

	protected override int GetConclusionIndexInternal()
	{
		if (BringMammothBarkToPot.IsComplete)
		{
			return 1;
		}
		return base.GetConclusionIndexInternal();
	}

	public override bool CanActorLootQuestItems(CActor actor)
	{
		if (actor is CHeroSummonActor)
		{
			return true;
		}
		return base.CanActorLootQuestItems(actor);
	}
}
