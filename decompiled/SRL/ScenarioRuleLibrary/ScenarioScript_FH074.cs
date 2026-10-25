using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH074 : ScenarioScript
{
	private Point _smallTree1 = new Point(4, 5);

	private Point _smallTree2 = new Point(11, 8);

	private CScenarioModifier _placeLogModifier;

	private CSpawner _alliesSpawner;

	private CTile _destroyedTreeTile;

	[StateDump(Skip = true)]
	private bool _room3;

	[StateDump(Skip = true)]
	private bool _room4;

	protected override int GetConclusionIndexInternal()
	{
		if (ScenarioManager.CurrentScenarioState.ScenarioSetup.IsAlternativeLevel)
		{
			return 1;
		}
		return 0;
	}

	protected override void TickPhaseInternal()
	{
		Trace("TickPhase " + _tickActor.Class.ID, null, 33);
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			if (ScenarioScript.RoundNumber == 1 && OncePerRound())
			{
				SpawnStartScenarioAllies();
			}
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			if (_tickRoomRevealed.RoomName == "Room_3")
			{
				_room3 = true;
			}
			if (_tickRoomRevealed.RoomName == "Room_4")
			{
				_room4 = true;
			}
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 72);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	protected override bool Initialize()
	{
		_placeLogModifier = GetScenarioModifier("PlaceLog");
		if (ScenarioManager.CurrentScenarioState.Spawners.Count > 0)
		{
			_alliesSpawner = ScenarioManager.CurrentScenarioState.Spawners[0];
		}
		return true;
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor is CObjectActor && (_tickActorTile.m_ArrayIndex == _smallTree1 || _tickActorTile.m_ArrayIndex == _smallTree2) && ValidatePlaceLogAbilityCanStart(_tickActor.Tile))
		{
			_destroyedTreeTile = _tickActor.Tile;
			CAreaEffect cAreaEffect = CardProcessingShared.CreateArea("0,0,R|0,1,R", "Editor", "Editor");
			cAreaEffect.IgnoreBlocked = true;
			List<(string, TValue)> miscVars = new List<(string, TValue)> { ("IsValidTile", new TValue(CExpression.Parse("ScenarioScript_FH074.IsValidTileForLog(obj)"))) };
			ScenarioScript.PickHexes("FH/GUI_PLACE_TREE_BRIDGE_HELPBOX", null, null, SpawnLogs, 1, animateActor: false, (CAbilityNullHex a) => CalcValidLogSpawnTiles(a), canSkip: false, canUndo: false, recalculateValidTilesOnSelect: true, preSelectValidTiles: false, "UseItem", cAreaEffect, CAbilityFilter.EFilterTile.Custom, ignoreLos: true, ignoreDoorLocks: false, stackAsSeparateAbilities: false, autoTrigger: false, miscVars);
		}
	}

	private bool ValidatePlaceLogAbilityCanStart(CTile treeTile)
	{
		if ((from tile in GameState.GetTilesInRange(treeTile.m_ArrayIndex, 2).Distinct()
			where IsValidTileForLog(tile)
			select tile).ToList().Count == 0)
		{
			return false;
		}
		return true;
	}

	private void SpawnLogs(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles == null || selectedTiles.Count == 0)
		{
			return;
		}
		string[] array = new string[2] { "TreeBridgeA", "TreeBridgeB" };
		int num = 0;
		CVector3 rotation = new CVector3(0f, 0f, 0f);
		if (selectedTiles.Count == 2)
		{
			CVector3 position = selectedTiles[0].m_Hex.Position;
			CVector3 cVector = selectedTiles[1].m_Hex.Position - position;
			float num2 = (float)(Math.Atan2(cVector.Z, cVector.X) * (180.0 / Math.PI));
			rotation = new CVector3(0f, 0f - num2, 0f);
		}
		foreach (CTile selectedTile in selectedTiles)
		{
			selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)?.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			ScenarioManager.ObjectImportType type = ScenarioManager.ObjectImportType.TerrainRubble;
			CObjectDifficultTerrain prop = new CObjectDifficultTerrain(array[num], type, new TileIndex(selectedTile.m_ArrayIndex), null, rotation, ability.TargetingActor, selectedTile.m_HexMap.MapGuid);
			selectedTile.SpawnProp(prop);
			num++;
		}
		_destroyedTreeTile = null;
	}

	private List<CTile> CalcValidLogSpawnTiles(CAbilityNullHex a)
	{
		new List<CTile>();
		List<CTile> list = new List<CTile>();
		GameState.GetAdjacentTiles(_destroyedTreeTile.m_ArrayIndex, list, clear: false);
		List<CTile> list2 = (from tile in list.Distinct()
			where IsValidTileForLog(tile)
			select tile).ToList();
		if (list2 == null || list2.Count == 0)
		{
			list = GameState.GetTilesInRange(_destroyedTreeTile.m_ArrayIndex, 2);
			list2 = (from tile in list.Distinct()
				where IsValidTileForLog(tile)
				select tile).ToList();
		}
		return list2;
	}

	public static bool IsValidTileForLog(CTile tile)
	{
		if (!TileHasChasm(tile))
		{
			return false;
		}
		List<CTile> list = new List<CTile>();
		GameState.GetAdjacentTiles(tile.m_ArrayIndex, list);
		if (!list.Any((CTile adjTile) => TileHasChasm(adjTile)))
		{
			return false;
		}
		return true;
		static bool TileHasChasm(CTile t)
		{
			CObjectProp cObjectProp = t.FindProp(ScenarioManager.ObjectImportType.Obstacle);
			if (cObjectProp == null)
			{
				return false;
			}
			if (cObjectProp.PropCustomLocKey != "FH_74_Trench_Hole_T")
			{
				return false;
			}
			return true;
		}
	}

	private void SpawnStartScenarioAllies()
	{
		ScenarioManager.CurrentScenarioState.Players.ConvertAll((PlayerState p) => p.Actor.Tile);
		List<CTile> list = ScenarioManager.StartingTiles.Where((CTile tile) => tile.Actors.Count() == 0).ToList();
		_alliesSpawner.SetLocation(list[0].m_Hex.ArrayIndex);
		_alliesSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: false, spawnFarthestOnAdjacent: false, useFirstRoundStartingTile: true);
		_alliesSpawner.SetLocation(list[1].m_Hex.ArrayIndex);
		_alliesSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: false, spawnFarthestOnAdjacent: false, useFirstRoundStartingTile: true);
		GameState.SortIntoInitiativeAndIDOrder();
	}

	private void SpawnPiranaPigs(CAbilityNullHex ability, List<CTile> selectedTiles)
	{
		if (selectedTiles == null || selectedTiles.Count == 0)
		{
			return;
		}
		foreach (CTile selectedTile in selectedTiles)
		{
			_alliesSpawner.SetLocation(selectedTile.m_Hex.ArrayIndex);
			_alliesSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: false, spawnFarthestOnAdjacent: false, useFirstRoundStartingTile: true);
		}
		GameState.SortIntoInitiativeAndIDOrder();
	}
}
