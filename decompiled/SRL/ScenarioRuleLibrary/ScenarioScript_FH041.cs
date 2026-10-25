using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH041 : ScenarioScript
{
	private CObjective_BringItemsToArea bringShardsToAltarObjective;

	private CObjectObstacle _shadowWall;

	private List<CSpawner> _spawners = new List<CSpawner>();

	private List<string> _spawnedQuestActors = new List<string>();

	private CObjectProp _pedestalProp;

	private int ReturnedItems => bringShardsToAltarObjective?.CurrentItemsAmount ?? 0;

	protected override bool Initialize()
	{
		bringShardsToAltarObjective = GetObjective<CObjective_BringItemsToArea>("FixAltar");
		_spawners.Clear();
		_spawners.Add(ScenarioManager.CurrentScenarioState.Spawners.Find((CSpawner spawner) => spawner.ArrayIndex.X == 8 && spawner.ArrayIndex.Y == 4));
		_spawners.Add(ScenarioManager.CurrentScenarioState.Spawners.Find((CSpawner spawner) => spawner.ArrayIndex.X == 5 && spawner.ArrayIndex.Y == 4));
		_spawners.Add(ScenarioManager.CurrentScenarioState.Spawners.Find((CSpawner spawner) => spawner.ArrayIndex.X == 2 && spawner.ArrayIndex.Y == 4));
		UpdatePedestalProp();
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_spawnedQuestActors);
		Trace("[ffa000]Serialize: " + text + "[-]", null, 39);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 45);
		List<CObjectProp> props = ScenarioManager.CurrentScenarioState.Props;
		_pedestalProp = props.SingleOrDefault((CObjectProp x) => x.PropTile.m_ArrayIndex.X == 14 && x.PropTile.m_ArrayIndex.Y == 4);
		foreach (CObjectProp item in props)
		{
			if (item is CObjectObstacle { PropType: EPropType.ThreeHexCurvedObstacle } cObjectObstacle)
			{
				_shadowWall = cObjectObstacle;
				Trace("Acquired shadow wall.", null, 52);
				break;
			}
		}
		if (_shadowWall == null)
		{
			props = ScenarioManager.CurrentScenarioState.DestroyedProps.OrderBy((CObjectProp prop) => prop.ArrayIndex.X).ToList();
			foreach (CObjectProp item2 in props)
			{
				if (item2 is CObjectObstacle { PropType: EPropType.ThreeHexCurvedObstacle } cObjectObstacle2)
				{
					_shadowWall = cObjectObstacle2;
					Trace("Acquired shadow wall.", null, 62);
					break;
				}
			}
		}
		if (_shadowWall != null)
		{
			_shadowWall.SetIgnoreFlyAndJump(ignoresFlyAndJump: true);
			UpdateShadowWallPF();
		}
		foreach (CObjectProp item3 in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => prop.SpecificPropType == ESpecificPropType.ShadowWallResidue))
		{
			item3.PropCustomLocKey = "FH_Scenario_Campaign_041_SCENARIO_OBJECT_1";
			item3.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
		}
		if (!string.IsNullOrEmpty(s))
		{
			_spawnedQuestActors.Clear();
			_spawnedQuestActors.AddRange(JsonConvert.DeserializeObject<List<string>>(s));
		}
	}

	private void UpdateShadowWallPF()
	{
		if (_shadowWall != null && !_shadowWall.IsDestroyed)
		{
			CTile propTile = _shadowWall.PropTile;
			UpdateShadowWallPF(propTile, valid: false);
		}
	}

	private void UpdateShadowWallPF(CTile tile, bool valid)
	{
		UpdatePF(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y + 1, valid);
		UpdatePF(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y - 1, valid);
		UpdatePF(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, valid);
	}

	private void UpdatePF(int x, int y, bool valid)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[x, y];
		cNode.Blocked = !valid;
		cNode.Walkable = valid;
	}

	protected override void TickPhaseInternal()
	{
		Log.SimpleInfo("TickPhaseInternal");
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemies.Find((CEnemyActor actor) => actor.ArrayIndex.X == 8 && actor.ArrayIndex.Y == 4);
			cEnemyActor.NoGoldDrop = true;
			_spawnedQuestActors.Add(cEnemyActor.ActorGuid);
			UpdateSpawners(initial: true, startRound: false);
			break;
		}
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			if (OncePerRound())
			{
				UpdateSpawners(initial: false, startRound: true);
			}
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 139);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
			break;
		}
	}

	protected override void TickDeathInternal()
	{
		if (_spawnedQuestActors.Contains(_tickActor.ActorGuid))
		{
			CObjectQuestItem prop = new CObjectQuestItem("CoralPiece", ScenarioManager.ObjectImportType.CarryableQuestItem, new TileIndex(_tickActorTile.m_ArrayIndex), null, null, null, _tickActorTile.m_HexMap.MapGuid);
			_tickActorTile.SpawnProp(prop);
		}
	}

	private bool GetNearestTileToSpawn(CTile origin, out CTile nearestTile)
	{
		nearestTile = null;
		int num = 9999999;
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null)
			{
				int tileDistance = ScenarioManager.GetTileDistance(origin.m_ArrayIndex.X, origin.m_ArrayIndex.Y, cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y);
				if (CSpawner.IsEmptyTile(cTile) && ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y].Walkable && tileDistance < num)
				{
					num = tileDistance;
					nearestTile = cTile;
				}
			}
		}
		if (nearestTile == null)
		{
			return false;
		}
		return true;
	}

	private void UpdateSpawners(bool initial, bool startRound)
	{
		if (!((ScenarioScript.RoundNumber % 2 != 0) | initial))
		{
			return;
		}
		int returnedItems = ReturnedItems;
		if (returnedItems >= _spawners.Count)
		{
			return;
		}
		for (int i = 0; i < _spawners.Count; i++)
		{
			bool flag = i == returnedItems;
			bool isActive = _spawners[i].IsActive;
			if (flag != isActive)
			{
				if (flag && !startRound && !initial)
				{
					_spawners[i].CurrentSpawnRoundEntryIndex++;
				}
				_spawners[i].SetActive(flag, forceDontSpawn: true);
			}
		}
	}

	private void SpawnQuestEnemy()
	{
		int count = ScenarioManager.CurrentScenarioState.DestroyedProps.FindAll((CObjectProp prop) => prop.PropType == EPropType.QuestItem).Count;
		if (count >= 3)
		{
			return;
		}
		int x = _shadowWall.PropTile.m_ArrayIndex.X;
		int y = _shadowWall.PropTile.m_ArrayIndex.Y;
		CTile spawnTile = ScenarioManager.Tiles[x + 1, y];
		CTile spawnTile2 = ScenarioManager.Tiles[8, 4];
		CTile spawnTile3 = ScenarioManager.Tiles[5, 4];
		CEnemyActor cEnemyActor = SpawnEnemy("DeepTerrorEliteID", spawnTile, noDropGold: true);
		_spawnedQuestActors.Add(cEnemyActor.ActorGuid);
		if (count == 1)
		{
			switch (ScenarioManager.CurrentScenarioState.Players.Count)
			{
			case 2:
				SpawnEnemy("DeepTerrorID", spawnTile2, noDropGold: true);
				SpawnEnemy("DeepTerrorID", spawnTile2, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				break;
			case 3:
				SpawnEnemy("DeepTerrorEliteID", spawnTile2, noDropGold: true);
				SpawnEnemy("DeepTerrorID", spawnTile2, noDropGold: true);
				SpawnEnemy("DeepTerrorID", spawnTile2, noDropGold: true);
				SpawnEnemy("NightDemonEliteID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonEliteID", spawnTile3, noDropGold: true);
				break;
			case 4:
				SpawnEnemy("DeepTerrorEliteID", spawnTile2, noDropGold: true);
				SpawnEnemy("DeepTerrorEliteID", spawnTile2, noDropGold: true);
				SpawnEnemy("DeepTerrorEliteID", spawnTile2, noDropGold: true);
				SpawnEnemy("NightDemonEliteID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonEliteID", spawnTile3, noDropGold: true);
				break;
			}
		}
		if (count == 2)
		{
			switch (ScenarioManager.CurrentScenarioState.Players.Count)
			{
			case 2:
				SpawnEnemy("LivingDoomID", spawnTile2, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				break;
			case 3:
				SpawnEnemy("LivingDoomID", spawnTile2, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				break;
			case 4:
				SpawnEnemy("LivingDoomEliteID", spawnTile2, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				SpawnEnemy("NightDemonID", spawnTile3, noDropGold: true);
				break;
			}
		}
	}

	private CEnemyActor SpawnEnemy(string ID, CTile spawnTile, bool noDropGold = false)
	{
		CMonsterClass cMonsterClass = MonsterClassManager.Find(ID);
		if (cMonsterClass == null)
		{
			return null;
		}
		if (!GetNearestTileToSpawn(spawnTile, out var nearestTile))
		{
			return null;
		}
		int chosenModelIndex = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(cMonsterClass.Models.Count);
		int num = cMonsterClass.Health();
		int maxHealth = num;
		int level = 1;
		EnemyState enemyState = new EnemyState(ID, chosenModelIndex, null, nearestTile.m_HexMap.MapGuid, new TileIndex(nearestTile.m_ArrayIndex), num, maxHealth, level, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, CActor.EType.Enemy);
		CEnemyActor cEnemyActor = ScenarioManager.Scenario.AddEnemy(enemyState, initial: false, noIDRegen: false, fromSpawnerAtRoundStart: false, resetHealth: false);
		if (cEnemyActor != null)
		{
			ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
			currentScenarioState.Monsters.Add(enemyState);
			cEnemyActor.NoGoldDrop = noDropGold;
			cEnemyActor.MarkWontPlayThisRound(wontPlay: true);
			GameState.SortIntoInitiativeAndIDOrder();
			CSpawnerSpawningUnit_MessageData message = new CSpawnerSpawningUnit_MessageData
			{
				m_SpawnTile = nearestTile,
				m_SpawnEnemy = cEnemyActor
			};
			ScenarioRuleClient.MessageHandler(message);
			currentScenarioState.CheckObjectivesComplete();
			currentScenarioState.PerformOnSpawnedModifiers(cEnemyActor, currentScenarioState.Players.Count);
		}
		return cEnemyActor;
	}

	private void RemoveResidueWalls(int startingXcord)
	{
		for (int num = ScenarioManager.CurrentScenarioState.Props.Count - 1; num >= 0; num--)
		{
			CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props[num];
			if (!(cObjectProp.PrefabName != "ShadowWallResidue") && cObjectProp.ArrayIndex.X >= startingXcord)
			{
				cObjectProp.DestroyProp();
			}
		}
	}

	private void MoveShadowWall()
	{
		int count = ScenarioManager.CurrentScenarioState.DestroyedProps.FindAll((CObjectProp prop) => prop.PropType == EPropType.QuestItem).Count;
		if (count < 3)
		{
			int x = _shadowWall.PropTile.m_ArrayIndex.X;
			int y = _shadowWall.PropTile.m_ArrayIndex.Y;
			CTile propTile = _shadowWall.PropTile;
			CTile cTile = ScenarioManager.Tiles[x - 3, y];
			string prefabName = _shadowWall.PrefabName;
			float num = 0f;
			CVector3 rotation = _shadowWall.Rotation;
			_shadowWall.DestroyProp(0f, sendMessageToClient: false);
			UpdateShadowWallPF(propTile, valid: true);
			CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(string.Empty, null)
			{
				m_ActorDestroyingObstacle = null,
				m_Tiles = new List<CTile> { propTile },
				m_DestroyDelay = num,
				m_DestroyedProps = new List<CObjectProp> { _shadowWall },
				m_DestroyObstacleAbility = null,
				m_OverrideSetLastSelectedTile = true
			};
			ScenarioRuleClient.MessageHandler(message);
			List<TileIndex> pathingBlockers = _shadowWall.PathingBlockers.Select((TileIndex tileIndex) => new TileIndex(tileIndex.X - 3, tileIndex.Y)).ToList();
			if (count < 2)
			{
				CObjectObstacle cObjectObstacle = new CObjectObstacle(prefabName, ScenarioManager.ObjectImportType.Obstacle, new TileIndex(cTile.m_ArrayIndex), null, rotation, pathingBlockers, null, cTile.m_HexMap.MapGuid, ignoresFlyAndJump: false);
				cObjectObstacle.PropCustomLocKey = "FH_Scenario_Campaign_041_SCENARIO_OBJECT_1";
				cObjectObstacle.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
				_shadowWall = cObjectObstacle;
				_shadowWall.SetIgnoreFlyAndJump(ignoresFlyAndJump: true);
				RemoveResidueWalls(cTile.m_ArrayIndex.X);
				cTile.SpawnProp(cObjectObstacle, notifyClient: true, num);
				UpdateShadowWallPF();
			}
			else if (count == 2)
			{
				_shadowWall.SetLocation(new TileIndex(cTile.m_ArrayIndex), null, _shadowWall.Rotation);
				RemoveResidueWalls(0);
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor.Type == CActor.EType.Player)
		{
			CObjective_BringItemsToArea cObjective_BringItemsToArea = bringShardsToAltarObjective;
			if (cObjective_BringItemsToArea != null)
			{
				List<TileIndex> tiles = cObjective_BringItemsToArea.AreaPositions;
				if (tiles.Contains(new TileIndex(_tickActorTile.m_ArrayIndex)) && ScenarioManager.Scenario.AllPlayers.All((CPlayerActor player) => tiles.Contains(new TileIndex(player.ArrayIndex))) && ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor player) => player.CarriedQuestItems.Count > 0))
				{
					if (_tickAbility is CAbilityMove { RemainingMoves: >0 } cAbilityMove)
					{
						cAbilityMove.ForceInterrupt();
					}
					CPlayerActor cPlayerActor = ScenarioManager.Scenario.AllPlayers.Find((CPlayerActor player) => player.CarriedQuestItems.Count > 0);
					CObjectProp cObjectProp = cPlayerActor.CarriedQuestItems[0];
					cPlayerActor.CarriedQuestItems.Clear();
					cObjectProp?.DestroyProp();
					MoveShadowWall();
					SpawnQuestEnemy();
					UpdateSpawners(initial: false, startRound: false);
				}
			}
		}
		bringShardsToAltarObjective.CheckObjectiveComplete(ScenarioManager.Scenario.AllPlayers.Count);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		UpdatePedestalProp();
	}

	private void UpdatePedestalProp()
	{
		if (_pedestalProp != null && bringShardsToAltarObjective != null)
		{
			CObjective_BringItemsToArea cObjective_BringItemsToArea = bringShardsToAltarObjective;
			if (cObjective_BringItemsToArea != null)
			{
				int currentItemsAmount = cObjective_BringItemsToArea.CurrentItemsAmount;
				string stateName = $"State{currentItemsAmount}";
				CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
				cChangePropVisualState_MessageData.m_prop = _pedestalProp;
				cChangePropVisualState_MessageData.m_stateName = stateName;
				cChangePropVisualState_MessageData.m_delay = 0.001f;
				ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
			}
		}
	}

	private void TickPhaseEndTurn()
	{
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Actor)
		{
			return false;
		}
		CObjective_BringItemsToArea cObjective_BringItemsToArea = bringShardsToAltarObjective;
		if (cObjective_BringItemsToArea == null)
		{
			Error("_objective is not CObjective_BringItemsToArea", null, 481);
			return false;
		}
		if (cObjective_BringItemsToArea.CurrentItemsAmount == 0)
		{
			return false;
		}
		Trace("Checking message trigger for " + message.MessageName + " " + message.TitleKey + "...", null, 489);
		if (cObjective_BringItemsToArea.CurrentItemsAmount == 1 && message.MessageName.Contains("41_1"))
		{
			Trace("...success", null, 492);
			return true;
		}
		if (cObjective_BringItemsToArea.CurrentItemsAmount == 2 && message.MessageName.Contains("41_2"))
		{
			Trace("...success", null, 497);
			return true;
		}
		Trace("...fail", null, 501);
		return false;
	}
}
