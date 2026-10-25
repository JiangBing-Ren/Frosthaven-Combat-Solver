using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH094 : ScenarioScript
{
	private CObjectObstacle _centralConsole;

	private bool roundStarted;

	private bool roundEnded;

	private CMap lastRoom;

	private CObjective vermlingWinObjective;

	public const string VermlingID = "VermlingScout94ID";

	private Point[][] _bladespinnerPaths = new Point[4][]
	{
		new Point[6]
		{
			new Point(2, 9),
			new Point(3, 10),
			new Point(4, 10),
			new Point(4, 9),
			new Point(4, 8),
			new Point(3, 8)
		},
		new Point[6]
		{
			new Point(4, 7),
			new Point(4, 6),
			new Point(3, 6),
			new Point(2, 7),
			new Point(3, 8),
			new Point(4, 8)
		},
		new Point[8]
		{
			new Point(7, 11),
			new Point(8, 10),
			new Point(7, 9),
			new Point(6, 9),
			new Point(5, 9),
			new Point(5, 10),
			new Point(5, 11),
			new Point(6, 11)
		},
		new Point[8]
		{
			new Point(7, 5),
			new Point(6, 5),
			new Point(5, 5),
			new Point(5, 6),
			new Point(5, 7),
			new Point(6, 7),
			new Point(7, 7),
			new Point(8, 6)
		}
	};

	protected override bool Initialize()
	{
		vermlingWinObjective = GetObjective("Vermlings Escape");
		lastRoom = ScenarioScript.FindRoom("Room_39");
		_centralConsole = (CObjectObstacle)GetProp(6, 8);
		_centralConsole.IsImmuneToDamage = true;
		foreach (CEnemyActor item in ScenarioManager.Scenario.Enemies.Where((CEnemyActor e) => e.Class.ID == "FlamingBladespinner_FH094ID"))
		{
			item.IsImmuneToDamage = true;
			item.ForcedUntargetable = true;
			CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(item, active: false);
			ScenarioRuleClient.MessageHandler(message);
		}
		return true;
	}

	private void CheckVermlingObjective()
	{
		if (ScenarioManager.Scenario.PlayerActors.Count == 0 && !vermlingWinObjective.IsComplete && !vermlingWinObjective.UnableToComplete)
		{
			vermlingWinObjective.UnableToComplete = true;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
		}
	}

	protected override string SerializeInternal()
	{
		return roundStarted.ToString();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			roundStarted = bool.Parse(storage);
		}
		UpdateSpawners();
		CheckVermlingObjective();
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.StartRoundCardSelection:
			if (!roundStarted)
			{
				roundStarted = true;
				UpdateSpawners();
			}
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			if (!roundEnded)
			{
				roundEnded = true;
				UpdateSpawners();
				CheckVermlingObjective();
			}
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			if (_tickActor is CEnemyActor { OriginalType: CActor.EType.Ally })
			{
				_tickActor.AllowEscape = true;
			}
			TryEscapeMap();
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			UpdateSpawners();
			break;
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		UpdateSpawners();
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		UpdateSpawners();
		CheckVermlingObjective();
	}

	private static string SpawnerGuidBasedOnPosition(Point pos)
	{
		IEnumerable<CSpawner> source = ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner x) => x.ArrayIndex.ToPoint() == pos);
		return $"Spawner_{pos.X}_{pos.Y}_{source.Count()}";
	}

	private void UpdateSpawners()
	{
		List<CEnemyActor> bladespinners = ScenarioManager.Scenario.Enemies.Where((CEnemyActor e) => e.Class.ID == "FlamingBladespinner_FH094ID").ToList();
		int num = ScenarioManager.Scenario.AllAliveActors.Count((CActor actor) => (actor is CPlayerActor || actor is CEnemyActor { Type: CActor.EType.Ally }) && bladespinners.Any((CEnemyActor bladespinner) => bladespinner.IsAdjacentTo(actor)));
		int num2 = ScenarioScript.RoundNumber + 1;
		if (PhaseManager.PhaseType == CPhase.PhaseType.StartRoundEffects)
		{
			num2 = ScenarioScript.RoundNumber;
		}
		bool flag = num2 % 3 == 0 && num2 > 0;
		bool flag2 = num2 % 6 == 0 && num2 > 0;
		List<CTile> spawnTiles = ScenarioManager.StartingTiles.ToList();
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		List<CTile> list = new List<CTile>();
		List<CTile> alreadyPresentSpawners = spawners.Select((CSpawner x) => x.GetTile()).ToList();
		for (int num3 = 0; num3 < num; num3++)
		{
			CTile item = PickSpawnTile(list, spawnTiles, alreadyPresentSpawners);
			list.Add(item);
		}
		if (flag)
		{
			CTile item2 = PickSpawnTile(list, spawnTiles, alreadyPresentSpawners);
			list.Add(item2);
		}
		if (list.Count >= spawners.Count && !list.Any((CTile tile) => !spawners.Any((CSpawner spawner) => spawner.ArrayIndex.ToPoint() == tile.m_ArrayIndex)))
		{
			return;
		}
		for (int num4 = spawners.Count - 1; num4 >= 0; num4--)
		{
			CSpawner cSpawner = spawners[num4];
			cSpawner.GetTile().m_Spawners.Remove(cSpawner);
			cSpawner.HideSpawnerProp(forced: true);
			ScenarioManager.CurrentScenarioState.Spawners.Remove(cSpawner);
		}
		spawners.Clear();
		if (flag)
		{
			List<string> spawnClass = ((!flag2) ? new List<string> { "RuinedMachineID", "RuinedMachineID", "RuinedMachineID", "RuinedMachineEliteID" } : new List<string> { "RuinedMachineID", "RuinedMachineID", "RuinedMachineEliteID", "RuinedMachineEliteID" });
			CTile cTile = list[0];
			list.RemoveAt(0);
			CSpawner cSpawner2 = new CSpawner(new SpawnerData(SpawnerData.ESpawnerTriggerType.StartRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 0, loopSpawnPattern: false, shouldCountTowardsKillAllEnemies: false, new List<int> { 1, 1, 1, 1 }, new Dictionary<string, List<SpawnRoundEntry>> { 
			{
				"Default",
				new List<SpawnRoundEntry>
				{
					new SpawnRoundEntry(spawnClass)
				}
			} }, null), new TileIndex(cTile.m_ArrayIndex), cTile.m_HexMap.MapGuid, SpawnerGuidBasedOnPosition(cTile.m_ArrayIndex));
			cTile.SpawnSpawner(cSpawner2);
			cSpawner2.SetActive(active: true, forceDontSpawn: true);
		}
		List<string> spawnClass2 = new List<string> { "RuinedMachineID", "RuinedMachineID", "RuinedMachineID", "RuinedMachineEliteID" };
		foreach (CTile item3 in list)
		{
			CSpawner cSpawner3 = new CSpawner(new SpawnerData(SpawnerData.ESpawnerTriggerType.EndRound, SpawnerData.ESpawnerActivationType.ScenarioStart, 0, loopSpawnPattern: false, shouldCountTowardsKillAllEnemies: false, new List<int> { 1, 1, 1, 1 }, new Dictionary<string, List<SpawnRoundEntry>> { 
			{
				"Default",
				new List<SpawnRoundEntry>
				{
					new SpawnRoundEntry(spawnClass2)
				}
			} }, null), new TileIndex(item3.m_ArrayIndex), item3.m_HexMap.MapGuid, SpawnerGuidBasedOnPosition(item3.m_ArrayIndex));
			item3.SpawnSpawner(cSpawner3);
			cSpawner3.SetActive(active: true, forceDontSpawn: true);
		}
	}

	private CTile PickSpawnTile(List<CTile> reservedTiles, List<CTile> spawnTiles, List<CTile> alreadyPresentSpawners)
	{
		List<CTile> list = new List<CTile>();
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && !reservedTiles.Contains(cTile) && CAbilityFilter.IsTileEmpty(cTile))
			{
				list.Add(cTile);
			}
		}
		return list.OrderBy((CTile x) => (!spawnTiles.Contains(x)) ? spawnTiles.Select((CTile spawnTile) => Point.HexDist(x.m_ArrayIndex, spawnTile.m_ArrayIndex)).Min() : 0).FirstOrDefault();
	}

	public override bool ShouldIgnoreImmunity(CActor actor, CAbility ability)
	{
		return ability?.Name?.Contains("DamageConsole") == true;
	}

	protected override bool AllowEscapeZoneInternal(CActor actor)
	{
		if (lastRoom != null && !lastRoom.Revealed)
		{
			return false;
		}
		return base.AllowEscapeZoneInternal(actor);
	}

	private void TickStartRound()
	{
		if (!OncePerRound())
		{
			return;
		}
		roundEnded = false;
		roundStarted = false;
		if (ScenarioScript.RoundNumber <= 1)
		{
			return;
		}
		HashSet<CEnemyActor> processedActors = new HashSet<CEnemyActor>();
		List<(CEnemyActor, CActor)> list = new List<(CEnemyActor, CActor)>();
		for (int i = 0; i < _bladespinnerPaths.Length; i++)
		{
			Point[] array = _bladespinnerPaths[i];
			Point point = array[(ScenarioScript.RoundNumber - 2) % array.Length];
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.FirstOrDefault((CEnemyActor e) => e.ArrayIndex == point && !processedActors.Contains(e));
			if (cEnemyActor == null || cEnemyActor.Class.ID != "FlamingBladespinner_FH094ID")
			{
				Log.Error($"No bladespinner {i} found at expected location {point} - Abort movement");
				continue;
			}
			Point targetPoint = array[(ScenarioScript.RoundNumber - 1) % array.Length];
			foreach (CActor item in ScenarioManager.Tiles[targetPoint.X, targetPoint.Y].LivingActors.Where((CActor a) => a.Class.ID != "FlamingBladespinner_FH094ID"))
			{
				list.Add((cEnemyActor, item));
			}
			GameState.MoveActorStraightLine(cEnemyActor, targetPoint, shouldWait: true);
			processedActors.Add(cEnemyActor);
		}
		list.Sort(((CEnemyActor Source, CActor Actor) p1, (CEnemyActor Source, CActor Actor) p2) => p2.Actor.Type.CompareTo(p1.Actor.Type));
		List<CTile> list2 = new List<CTile>();
		foreach (var item2 in list)
		{
			CAbilityPush cAbilityPush = (CAbilityPush)CAbilityPush.CreateDefaultPush(1, 1, 1, isMonster: false, "", new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
			cAbilityPush.TargetingActor = item2.Item1;
			cAbilityPush.TargetThisActorAutomatically = item2.Item2;
			cAbilityPush.MustMove = true;
			cAbilityPush.HelpBoxTooltipLocKey = "FH_Scenario_Campaign_094_SCENARIO_MODIFIER_6";
			cAbilityPush.SetMiscVar("IgnorePropActivation", true);
			CTile cTile = GameState.FindClosestEmptyFeaturelessTile(item2.Item2.Tile, 100, list2);
			if (cTile != null)
			{
				cAbilityPush.TryPushToThisTileAutomatically = cTile;
				list2.Add(cTile);
			}
			if (item2.Item2 is CPlayerActor || item2.Item2.Class.ID == "VermlingScout94ID")
			{
				CAbilityDamage cAbilityDamage = (CAbilityDamage)CAbilityDamage.CreateDefaultDamage(ScenarioManager.TrapDamage, 1, 1, isMonster: false, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbilityDamage.MiscAbilityData = new AbilityData.MiscAbilityData
				{
					UseParentTargets = true,
					AutotriggerAbility = true
				};
				cAbilityPush.AddSubAbility(cAbilityDamage);
			}
			GameState.PendingScenarioModifierAbilities.Add(Tuple.Create((CAbility)cAbilityPush, item2.Item2));
		}
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("VermlingScout94ID", 0, null, CActor.EType.Ally);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState;
	}
}
