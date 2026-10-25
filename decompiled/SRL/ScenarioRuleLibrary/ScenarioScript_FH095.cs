using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH095 : ScenarioScript
{
	private CObjectDoor Door1;

	private CObjectDoor Door2;

	private CObjectDoor Door3;

	private CObjective_Scripted MoveSarcophagusToGoalTilesObjective;

	private CObjective_Scripted ReluctantGhostReachStartTileObjective;

	private CScenarioModifier ReluctantGhostInfoModifier;

	private List<Point> SarcophagusGoalPoints = new List<Point>
	{
		new Point(15, 14),
		new Point(15, 13)
	};

	private CObjectDifficultTerrain SarcophagusProp;

	private const string ReluctantGhostAID = "ReluctantGhostAID";

	private const string ReluctantGhostBID = "ReluctantGhostBID";

	[StateDump(Skip = true)]
	private int lastSarcophagusMoveRound = -1;

	private CEnemyActor ReluctantGhost => ScenarioManager.Scenario.Enemies.SingleOrDefault((CEnemyActor x) => x.Class.ID.Contains("ReluctantGhost"));

	protected override bool Initialize()
	{
		Door1 = ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp x) => x.PropTile.m_ArrayIndex.X == 10 && x.PropTile.m_ArrayIndex.Y == 6) as CObjectDoor;
		Door2 = ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp x) => x.PropTile.m_ArrayIndex.X == 9 && x.PropTile.m_ArrayIndex.Y == 5) as CObjectDoor;
		Door3 = ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp x) => x.PropTile.m_ArrayIndex.X == 13 && x.PropTile.m_ArrayIndex.Y == 12) as CObjectDoor;
		SarcophagusProp = ScenarioManager.CurrentScenarioState.Props.FirstOrDefault((CObjectProp x) => x.SpecificPropType == ESpecificPropType.Sarcophagus) as CObjectDifficultTerrain;
		if (SarcophagusProp?.PropTile != null)
		{
			SarcophagusProp.PropTile.AddProp(SarcophagusProp, recursiveTiles: true);
		}
		MoveSarcophagusToGoalTilesObjective = GetObjective("MoveSarcophagusToGoalTiles") as CObjective_Scripted;
		ReluctantGhostReachStartTileObjective = GetObjective("ReluctantGhostReachStartTile") as CObjective_Scripted;
		ReluctantGhostInfoModifier = GetScenarioModifier("Info_Modifier_3");
		ScenarioManager.ToggleTileHighlights(SarcophagusGoalPoints, enabled: true);
		ToggleReluctantGhostInfo(ReluctantGhost != null);
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			SetReluctantGhostFocusOverride();
			break;
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		if (_tickActor == ReluctantGhost && ReluctantGhost != null && ScenarioManager.StartingTiles.Contains(ReluctantGhost.Tile))
		{
			ReluctantGhostReachStartTileObjective.ScriptedIsComplete = true;
			ReluctantGhostReachStartTileObjective.CheckObjectiveComplete(ScenarioManager.CurrentScenarioState.Players.Count);
		}
	}

	private void TickStartRound()
	{
		if (lastSarcophagusMoveRound != ScenarioScript.RoundNumber)
		{
			lastSarcophagusMoveRound = ScenarioScript.RoundNumber;
			CPlayerActor cPlayerActor = GameState.HostCharacter;
			if (cPlayerActor == null)
			{
				cPlayerActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault();
			}
			CheckIfCanOfferMoveSarcophagus(cPlayerActor);
		}
	}

	private void CheckIfCanOfferMoveSarcophagus(CPlayerActor tickPlayerActor)
	{
		if (!ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor actor) => !SarcophagusProp.GetTiles().Any((Point tile) => actor.GetDistanceTo(tile) <= 1)))
		{
			OfferMoveSarcophagus(tickPlayerActor);
		}
	}

	public static bool IsTileValidForSarcophagusMove(CTile tile)
	{
		ScenarioScript_FH095 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH095;
		CObjectDoor cObjectDoor = tile.FindDoor();
		if (cObjectDoor != null && !cObjectDoor.DoorIsOpen && scenarioScript_FH.ReluctantGhost != null)
		{
			return false;
		}
		if (tile.LivingActor != null)
		{
			return false;
		}
		return true;
	}

	private void OfferMoveSarcophagus(CPlayerActor actor)
	{
		List<CTile> list = new List<CTile>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			List<CTile> list2 = new List<CTile>();
			GameState.GetAdjacentTiles(playerActor.ArrayIndex, list2, clear: false);
			foreach (CTile tile in list2)
			{
				if (list.Contains(tile))
				{
					continue;
				}
				CNode node = ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex);
				if ((node.Walkable || node.IsBridge) && ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => x.GetDistanceTo(tile) <= 1) && tile.LivingActor == null && tile.FindObstacleProp() == null)
				{
					CObjectDoor cObjectDoor = tile.FindDoor();
					if (cObjectDoor == null || cObjectDoor.DoorIsOpen || ReluctantGhost == null)
					{
						list.Add(tile);
					}
				}
			}
		}
		CAreaEffect areaEffect = CObjectProp.GetAreaEffect(SarcophagusProp.PrefabName, SarcophagusProp.PropType.ToString());
		areaEffect.CanFlip = false;
		List<(string, TValue)> miscVars = new List<(string, TValue)> { ("IsValidTile", new TValue(CExpression.Parse("ScenarioScript_FH095.IsTileValidForSarcophagusMove(obj)"))) };
		ScenarioScript.PickHexes("$FH/FH_Scenario_Campaign_095_MoveSarcophagus_HelpText$", actor, list, MoveSarcophagusCallback, 1, animateActor: false, null, canSkip: true, canUndo: true, recalculateValidTilesOnSelect: false, preSelectValidTiles: true, "UseItem", areaEffect, CAbilityFilter.EFilterTile.Custom, ignoreLos: true, ReluctantGhost == null, stackAsSeparateAbilities: false, autoTrigger: false, miscVars);
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if ((ReluctantGhost == null || ReluctantGhost.IsDead) && !ReluctantGhostReachStartTileObjective.IsComplete)
		{
			ToggleReluctantGhostInfo(toggled: false);
		}
	}

	private void MoveSarcophagusCallback(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		foreach (CTile selectedTile in selectedTiles)
		{
			foreach (CObjectProp item in selectedTile.m_Props.ToList())
			{
				if (item != SarcophagusProp && (item.PropType == EPropType.TerrainIce || item.PropType == EPropType.TerrainHotCoals || item.PropType == EPropType.TerrainRubble || item.PropType == EPropType.TerrainThorns || item.PropType == EPropType.TerrainWater || item.PropType == EPropType.Trap))
				{
					item.DestroyProp();
				}
			}
		}
		CTile propTile = SarcophagusProp.PropTile;
		CVector3 rotation = SarcophagusProp.Rotation;
		_ = SarcophagusProp.PropCustomLocKey;
		_ = SarcophagusProp;
		CTile cTile = selectedTiles[0];
		CVector3 cVector = new CVector3(0f, 360f - ability.AreaEffectAngle, 0f);
		SarcophagusProp.RemoveDifficultTerrain(sendMessageToClient: false);
		SarcophagusProp.SetLocation(new TileIndex(cTile.m_ArrayIndex), null, cVector);
		SarcophagusProp.InitDifficultTerrain();
		SarcophagusProp.StartingMapGuid = cTile.m_HexMap.MapGuid;
		selectedTiles[0].SpawnProp(SarcophagusProp);
		selectedTiles[0].AddProp(SarcophagusProp, recursiveTiles: true);
		CMoveProp_MessageData message = new CMoveProp_MessageData
		{
			m_MoveProp = SarcophagusProp,
			m_MoveToTile = cTile,
			m_MoveSpeed = 2f,
			m_MoveFromTile = propTile,
			m_MoveFromRotation = rotation,
			m_MoveToRotation = cVector
		};
		ScenarioRuleClient.MessageHandler(message);
		List<CObjectDoor> list = (from arrayIndex in SarcophagusProp.GetTiles()
			select ScenarioManager.GetTile(arrayIndex)?.FindDoor() into door
			where door != null
			select door).ToList();
		if (list.Contains(Door1) && !list.Contains(Door2))
		{
			list.Add(Door2);
		}
		if (list.Contains(Door2) && !list.Contains(Door1))
		{
			list.Add(Door1);
		}
		if (list.Any((CObjectDoor x) => x.DoorIsClosed))
		{
			foreach (CObjectDoor item2 in list)
			{
				item2.ForceActivate(null);
			}
			SpawnReluctantGhost();
		}
		if (SarcophagusProp.GetTiles().All((Point x) => SarcophagusGoalPoints.Contains(x)))
		{
			MoveSarcophagusToGoalTilesObjective.ScriptedIsComplete = true;
			MoveSarcophagusToGoalTilesObjective.CheckObjectiveComplete(ScenarioScript.PartySize, isEndOfRound: true);
		}
	}

	private void SpawnReluctantGhost()
	{
		List<CTile> list = new List<CTile>();
		List<Point> tiles = SarcophagusProp.GetTiles();
		foreach (Point item in tiles)
		{
			List<CTile> list2 = new List<CTile>();
			GameState.GetAdjacentTiles(item, list2, clear: false);
			foreach (CTile item2 in list2)
			{
				if (!list.Contains(item2) && item2.IsEmpty && !tiles.Contains(item2.m_ArrayIndex))
				{
					list.Add(item2);
				}
			}
		}
		List<CTile> startingTiles = ScenarioManager.StartingTiles;
		list = list.OrderBy((CTile tile) => startingTiles.Sum((CTile startingTile) => Point.HexDist(tile.m_ArrayIndex, startingTile.m_ArrayIndex))).ToList();
		CTile cTile2 = list[0];
		if (Door3.DoorIsOpen)
		{
			TrySpawnActorAtNearestTile(ScenarioManager.GetTile(cTile2.m_ArrayIndex), "ReluctantGhostBID", CActor.EType.Enemy, out var _);
		}
		else
		{
			TrySpawnActorAtNearestTile(ScenarioManager.GetTile(cTile2.m_ArrayIndex), "ReluctantGhostAID", CActor.EType.Enemy, out var _);
		}
		ToggleReluctantGhostInfo(toggled: true);
	}

	private void SetReluctantGhostFocusOverride()
	{
		if (_tickActor == ReluctantGhost && ReluctantGhost != null)
		{
			List<CTile> startingTiles = ScenarioManager.StartingTiles;
			startingTiles = startingTiles.OrderBy((CTile x) => Point.HexDist(x.m_ArrayIndex, ReluctantGhost.ArrayIndex)).ToList();
			CTile focusTile = startingTiles.FirstOrDefault((CTile x) => x.IsEmpty) ?? startingTiles.First();
			CAIFocusOverrideDetails.SetAIFocusOverrideOnTileOrNearestEmpty(ReluctantGhost, focusTile);
		}
	}

	private void ToggleReluctantGhostInfo(bool toggled)
	{
		ScenarioManager.ToggleTileHighlights(ScenarioManager.StartingTiles.Select((CTile x) => x.m_ArrayIndex), toggled, new CVector4(1f, 0f, 0f, 1f));
		ReluctantGhostReachStartTileObjective.SetActivation(toggled);
		ReluctantGhostInfoModifier.SetDeactivated(!toggled);
	}

	public override IEnumerable<EnemyState> GetMonstersThatMightSpawn(ScenarioState scenarioState)
	{
		foreach (EnemyState item in base.GetMonstersThatMightSpawn(scenarioState))
		{
			yield return item;
		}
		EnemyState enemyState = new EnemyState("ReluctantGhostAID", 0, null, CActor.EType.Enemy);
		enemyState.ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>();
		enemyState.ConfigPerPartySize[1] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[2] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[3] = ScenarioManager.EPerPartySizeConfig.Normal;
		enemyState.ConfigPerPartySize[4] = ScenarioManager.EPerPartySizeConfig.Normal;
		yield return enemyState;
	}
}
