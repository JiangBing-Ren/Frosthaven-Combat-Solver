using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH101 : ScenarioScript
{
	[Serializable]
	private struct SerializationData
	{
		public List<Point> LootedGoalChestsPoints { get; set; }

		public int AbsorbedSwarms { get; set; }
	}

	private const string GOAL_CHEST_LOC_KEY = "FH/FH_Scenario_Campaign_101_SCENARIO_OBJECT_2";

	private const string GOAL_CHEST_DESC_LOC_KEY = "FH_101_Swarm_Receptacle_D";

	private const string ZU_ID = "ZuID";

	private const string SWARM_ID = "SwarmID";

	private readonly List<Point> _firstRoomSwarmSpawnPoints = new List<Point>
	{
		new Point(13, 6),
		new Point(13, 12)
	};

	private readonly List<Point> _secondRoomSwarmSpawnPoints = new List<Point>
	{
		new Point(11, 12),
		new Point(8, 6),
		new Point(2, 8),
		new Point(2, 10)
	};

	private readonly List<Point> _thirdRoomSwarmSpawnPoints = new List<Point>
	{
		new Point(16, 4),
		new Point(14, 2)
	};

	private readonly List<Point> _fourthRoomSwarmSpawnPoints = new List<Point>
	{
		new Point(10, 15)
	};

	private readonly List<Point> _allSwarmSpawnPoints = new List<Point>();

	private readonly HashSet<CObjectProp> _goalChestsBuffer = new HashSet<CObjectProp>();

	private readonly HashSet<CObjectProp> _goalChestsBeforeLoot = new HashSet<CObjectProp>();

	private readonly HashSet<CObjectProp> _goalChestsAfterLoot = new HashSet<CObjectProp>();

	private CObjective_Scripted _enterNextRoomObjective;

	private CObjective_Scripted _helpZuObjective;

	private CScenarioModifier _nonInteractableScenarioModifier;

	private CScenarioModifier _lootSwarmReceptaclesModifier;

	private CScenarioModifier _swarmsApplyPoisonModifier;

	private CScenarioModifier _absorbSwarmsModifier;

	private CMap _secondRoom;

	private CMap _thirdRoom;

	private CMap _fourthRoom;

	private List<Point> _lootedGoalChestsPoints = new List<Point>();

	private int AbsorbedSwarms { get; set; }

	private int MaxAbsorbedSwarms => ScenarioScript.PartySize + 4;

	protected override bool Initialize()
	{
		_allSwarmSpawnPoints.AddRange(_firstRoomSwarmSpawnPoints);
		_allSwarmSpawnPoints.AddRange(_secondRoomSwarmSpawnPoints);
		_allSwarmSpawnPoints.AddRange(_thirdRoomSwarmSpawnPoints);
		_allSwarmSpawnPoints.AddRange(_fourthRoomSwarmSpawnPoints);
		_enterNextRoomObjective = GetObjective<CObjective_Scripted>("EnterNextRoom");
		_helpZuObjective = GetObjective<CObjective_Scripted>("HelpZu");
		_nonInteractableScenarioModifier = GetScenarioModifier("NonInteractable");
		_lootSwarmReceptaclesModifier = GetScenarioModifier("LootSwarmReceptacles");
		_swarmsApplyPoisonModifier = GetScenarioModifier("SwarmsApplyPoison");
		_absorbSwarmsModifier = GetScenarioModifier("AbsorbSwarms");
		_secondRoom = ScenarioScript.FindRoom("Room_2");
		_thirdRoom = ScenarioScript.FindRoom("Room_3");
		_fourthRoom = ScenarioScript.FindRoom("Room_4");
		return true;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(new SerializationData
		{
			LootedGoalChestsPoints = _lootedGoalChestsPoints,
			AbsorbedSwarms = AbsorbedSwarms
		});
	}

	protected override void DeserializeInternal(string storage)
	{
		if (string.IsNullOrEmpty(storage))
		{
			return;
		}
		SerializationData serializationData = JsonConvert.DeserializeObject<SerializationData>(storage);
		_lootedGoalChestsPoints = serializationData.LootedGoalChestsPoints;
		AbsorbedSwarms = serializationData.AbsorbedSwarms;
		List<Point> list = new List<Point>();
		foreach (Point allSwarmSpawnPoint in _allSwarmSpawnPoints)
		{
			if (!_lootedGoalChestsPoints.Contains(allSwarmSpawnPoint) && GetGoalChest(allSwarmSpawnPoint) != null)
			{
				list.Add(allSwarmSpawnPoint);
			}
		}
		ScenarioManager.ToggleTileHighlights(list, enabled: true);
		RefreshUntargetable();
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Loot)
		{
			GetGoalChests(_goalChestsBeforeLoot);
		}
		else
		{
			if (_tickAbility.AbilityType != CAbility.EAbilityType.Move)
			{
				return;
			}
			if (_tickActor.Class.ID == "ZuID")
			{
				CAbilityMove cAbilityMove = (CAbilityMove)_tickAbility;
				CEnemyActor cEnemyActor = FindNearestSwarmToZu();
				if (cEnemyActor != null)
				{
					cAbilityMove.AIFocusOverride.ForcedUseTileIndex = cEnemyActor.ArrayIndex.ToTileIndex();
				}
			}
			else if (_tickActor.Class.ID == "SwarmID")
			{
				CAbilityMove cAbilityMove2 = (CAbilityMove)_tickAbility;
				CEnemyActor zu = GetZu();
				if (zu != null)
				{
					cAbilityMove2.AIFocusOverride.ForcedUseTileIndex = zu.ArrayIndex.ToTileIndex();
				}
			}
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Loot)
		{
			GetGoalChests(_goalChestsAfterLoot);
			List<CObjectProp> goalChests = FindLootedChests(_goalChestsBeforeLoot, _goalChestsAfterLoot);
			OnGoalChestsLooted(goalChests);
		}
	}

	protected override void TickRepositionInternal()
	{
		foreach (CEnemyActor item in GetSwarmsOverlappingWithZu())
		{
			GameState.KillActor(item, item, CActor.ECauseOfDeath.Undetermined, out var _);
			AbsorbedSwarms++;
			CPlayCharacterAnimation_MessageData message = new CPlayCharacterAnimation_MessageData("Interact", GetZu());
			ScenarioRuleClient.MessageHandler(message);
		}
		if (GetSwarmAt(_tickActor.ArrayIndex) != null)
		{
			foreach (CActor livingActor in _tickActor.Tile.LivingActors)
			{
				ApplyPoisonToActor(livingActor);
			}
		}
		_helpZuObjective.Current = AbsorbedSwarms;
		_helpZuObjective.Total = MaxAbsorbedSwarms;
		_helpZuObjective.ScriptedIsComplete = _helpZuObjective.Current >= _helpZuObjective.Total;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickRoomRevealed();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 203);
			break;
		}
	}

	private void TickEndTurn()
	{
		if (_tickActor.Type == CActor.EType.Player)
		{
			GetGoalChests(_goalChestsBuffer, (CObjectProp chest) => chest.ArrayIndex.ToPoint() == _tickActor.ArrayIndex);
			OnGoalChestsLooted(_goalChestsBuffer);
		}
	}

	private void TickRoomRevealed()
	{
		if (_tickRoomRevealed == _secondRoom)
		{
			_enterNextRoomObjective.ScriptedIsComplete = true;
			_helpZuObjective.SetActivation(active: true);
			_nonInteractableScenarioModifier.Show();
			_lootSwarmReceptaclesModifier.Show();
			_swarmsApplyPoisonModifier.Show();
			_absorbSwarmsModifier.Show();
			ScenarioScript._css.UpdateScenarioModifiersDescriptions();
			SpawnGoalChests(_firstRoomSwarmSpawnPoints);
			SpawnGoalChests(_secondRoomSwarmSpawnPoints);
		}
		else if (_tickRoomRevealed == _thirdRoom)
		{
			SpawnGoalChests(_thirdRoomSwarmSpawnPoints);
		}
		else if (_tickRoomRevealed == _fourthRoom)
		{
			SpawnGoalChests(_fourthRoomSwarmSpawnPoints);
		}
	}

	private void OnGoalChestsLooted(ICollection<CObjectProp> goalChests)
	{
		foreach (CObjectProp goalChest in goalChests)
		{
			Point point = goalChest.ArrayIndex.ToPoint();
			GetSpawner(point).SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: true);
			_lootedGoalChestsPoints.Add(point);
		}
		ScenarioManager.ToggleTileHighlights(goalChests.Select((CObjectProp x) => x.ArrayIndex.ToPoint()), enabled: false);
		goalChests.Clear();
	}

	private void SpawnGoalChests(List<Point> swarmSpawnPoints)
	{
		ScenarioManager.ToggleTileHighlights(swarmSpawnPoints, enabled: true);
		foreach (Point swarmSpawnPoint in swarmSpawnPoints)
		{
			if (GetGoalChest(swarmSpawnPoint) == null)
			{
				CTile tile = ScenarioManager.GetTile(swarmSpawnPoint);
				CObjectChest cObjectChest = new CObjectChest(ESpecificPropType.ReceptacleContainer.ToString(), ScenarioManager.ObjectImportType.GoalChest, swarmSpawnPoint.ToTileIndex(), null, null, null, tile.m_HexMap.MapGuid);
				cObjectChest.PropCustomLocKey = "FH/FH_Scenario_Campaign_101_SCENARIO_OBJECT_2";
				cObjectChest.PropCustomDescriptionLocKey = "FH_101_Swarm_Receptacle_D";
				tile.SpawnProp(cObjectChest);
			}
		}
	}

	private CObjectChest GetGoalChest(Point arrayIndex)
	{
		return ScenarioManager.CurrentScenarioState.ChestProps.FirstOrDefault((CObjectProp x) => x.ArrayIndex.ToPoint() == arrayIndex) as CObjectChest;
	}

	private void GetGoalChests(ICollection<CObjectProp> outGoalChests, Func<CObjectProp, bool> predicate = null)
	{
		outGoalChests.Clear();
		foreach (CObjectProp chestProp in ScenarioManager.CurrentScenarioState.ChestProps)
		{
			if ((predicate == null || predicate(chestProp)) && _allSwarmSpawnPoints.Contains(chestProp.ArrayIndex.ToPoint()))
			{
				outGoalChests.Add((CObjectChest)chestProp);
			}
		}
	}

	private List<CObjectProp> FindLootedChests(ICollection<CObjectProp> chestsBeforeLoot, ICollection<CObjectProp> chestsAfterLoot)
	{
		List<CObjectProp> list = new List<CObjectProp>();
		foreach (CObjectProp item in chestsBeforeLoot)
		{
			if (!chestsAfterLoot.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private CEnemyActor GetZu()
	{
		return ScenarioManager.Scenario.AllAllyMonsters.FirstOrDefault((CEnemyActor x) => x.Class.ID == "ZuID");
	}

	private List<CEnemyActor> GetSwarms()
	{
		return ScenarioManager.Scenario.AllAllyMonsters.Where((CEnemyActor x) => x.Class.ID == "SwarmID" && !x.IsDead).ToList();
	}

	private void RefreshUntargetable()
	{
		CEnemyActor zu = GetZu();
		if (zu != null)
		{
			zu.ForcedUntargetable = true;
		}
		foreach (CEnemyActor swarm in GetSwarms())
		{
			swarm.ForcedUntargetable = true;
		}
	}

	private CEnemyActor GetSwarmAt(Point arrayIndex)
	{
		return ScenarioManager.Scenario.AllyMonsters.FirstOrDefault((CEnemyActor x) => x.Class.ID == "SwarmID" && x.ArrayIndex == arrayIndex);
	}

	private CEnemyActor FindNearestSwarmToZu()
	{
		CEnemyActor zu = GetZu();
		List<CEnemyActor> swarms = GetSwarms();
		int num = int.MaxValue;
		CEnemyActor result = null;
		foreach (CEnemyActor item in swarms)
		{
			int num2 = Point.HexDist(zu.ArrayIndex, item.ArrayIndex);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	private List<CEnemyActor> GetSwarmsOverlappingWithZu()
	{
		List<CEnemyActor> list = new List<CEnemyActor>();
		CEnemyActor zu = GetZu();
		foreach (CEnemyActor swarm in GetSwarms())
		{
			if (swarm.ArrayIndex == zu.ArrayIndex)
			{
				list.Add(swarm);
			}
		}
		return list;
	}

	private void ApplyPoisonToActor(CActor actor)
	{
		if (actor != null && !(actor.Class.ID == "ZuID") && !(actor.Class.ID == "SwarmID"))
		{
			CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _swarmsApplyPoisonModifier.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		RefreshUntargetable();
	}

	public override void OnSpawnerHasSpawned(CSpawner spawner, CActor actor)
	{
		base.OnSpawnerHasSpawned(spawner, actor);
		RefreshUntargetable();
	}
}
