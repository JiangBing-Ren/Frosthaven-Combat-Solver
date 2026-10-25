using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH022 : ScenarioScript
{
	private class CSpawnWaterPath
	{
		public List<Point> m_ArrayIndices;

		public List<CTile> m_Waypoints;

		public CTile m_Tile;
	}

	public readonly Point _aLinkIndex = new Point(13, 11);

	public readonly List<Point> _bLinkIndexes = new List<Point>
	{
		new Point(8, 9),
		new Point(8, 10),
		new Point(8, 11)
	};

	private static readonly Point ScenarioActorIndex = new Point(12, 4);

	private CObjective_Scripted _scenarioHealthLoseObjective;

	private CScenarioModifier _scenarioBelowDeck1Modifier;

	private CScenarioModifier _scenarioBelowDeck2Modifier;

	private CScenarioModifier _scenarioBelowDeck3Modifier;

	private CObjectActor _scenarioActor;

	public CMap _lastRoom;

	[StateDump(Skip = true)]
	private bool showedTrapDoorNarration;

	[StateDump(Skip = true)]
	private bool trapDoorShown;

	[StateDump(Skip = true)]
	private bool connectionsShown;

	public List<EnemyState> RevealedMonstersAlive => ScenarioManager.CurrentScenarioState.Monsters.FindAll((EnemyState m) => m.IsRevealed && !m.IsDead && !m.Actor.IsDead);

	public override bool EnemiesAttackWithMinimalMovement()
	{
		return true;
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateObjectives();
	}

	private void TickStartTurn()
	{
		if (_tickActor.Type == CActor.EType.Enemy && _lastRoom.Revealed)
		{
			_tickActor.SetCanTargetFromAnywhere(CActor.TargetFromAnywhereFlags.AnyIfNoOtherTargets);
		}
	}

	private bool IsValidWaterTile(CTile tile)
	{
		if (!ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Walkable)
		{
			return false;
		}
		return CAbilityFilter.IsTileFeatureless(tile);
	}

	private List<CTile> GetValidWaterTiles()
	{
		CActor targetingActor = _tickAbility.TargetingActor;
		for (int i = 1; i < 99; i++)
		{
			List<CTile> list = (from t in GameState.GetTilesInRange(targetingActor.ArrayIndex, i)
				where IsValidWaterTile(t)
				select t).ToList();
			if (list != null && list.Count != 0)
			{
				return list;
			}
		}
		return null;
	}

	private void SpawnWaterTileAt(CTile tile)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = "DellingDestruction",
			m_ArrayIndex = tile.m_ArrayIndex
		});
		CAbilityCreateTerrain.SpawnTerrain(ScenarioManager.ObjectImportType.TerrainWater, tile);
	}

	private void SpawnWaterTile()
	{
		List<CTile> validWaterTiles = GetValidWaterTiles();
		if (validWaterTiles == null)
		{
			Error("No possible water tiles found", null, 91);
			return;
		}
		CActor targetingActor = _tickAbility.TargetingActor;
		if (targetingActor.AIMoveFocusActors.Count == 0)
		{
			targetingActor.Move(1, jump: false, fly: false, 99, allowMove: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, null, firstMove: true, moveTest: false, carryOtherActors: false);
		}
		if (targetingActor.AIMoveFocusActors.Count != 0)
		{
			List<CSpawnWaterPath> list = new List<CSpawnWaterPath>();
			foreach (CTile item in validWaterTiles)
			{
				CSpawnWaterPath cSpawnWaterPath = new CSpawnWaterPath();
				cSpawnWaterPath.m_Tile = item;
				cSpawnWaterPath.m_ArrayIndices = CAbilityMove.FindPathAndWaypoints(targetingActor, item.m_ArrayIndex, ScenarioManager.Tiles[targetingActor.AIMoveFocusActors[0].ArrayIndex.X, targetingActor.AIMoveFocusActors[0].ArrayIndex.Y], out cSpawnWaterPath.m_Waypoints, 100, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, null, excludeDestination: true, ignoreMoveCost: true, out var foundPath, shouldPathThroughDoors: false);
				if (foundPath)
				{
					list.Add(cSpawnWaterPath);
				}
			}
			list.Sort((CSpawnWaterPath x, CSpawnWaterPath y) => x.m_ArrayIndices.Count.CompareTo(y.m_ArrayIndices.Count));
			if (list.Count > 0 && list.Count > 0)
			{
				SpawnWaterTileAt(list[0].m_Tile);
				return;
			}
			if (validWaterTiles.Count > 0)
			{
				SpawnWaterTileAt(validWaterTiles[0]);
				return;
			}
		}
		else
		{
			int num = int.MaxValue;
			CTile cTile2 = null;
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				if (CActor.AreActorsAllied(targetingActor.Type, allAliveActor.Type, targetingActor, allAliveActor))
				{
					continue;
				}
				foreach (CTile item2 in validWaterTiles)
				{
					int num2 = Point.HexDist(item2.m_ArrayIndex, allAliveActor.ArrayIndex);
					if (num2 < num)
					{
						num = num2;
						cTile2 = item2;
					}
				}
			}
			if (cTile2 != null)
			{
				SpawnWaterTileAt(cTile2);
				return;
			}
		}
		Error("Didn't spawn any water", null, 193);
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor == _scenarioActor)
		{
			int num = _tickActor.Health + actualDamage;
			int num2 = _tickActor.MaxHealth - num;
			int num3 = _tickActor.MaxHealth - _tickActor.Health;
			int num4 = 4 + 2 * ScenarioManager.CurrentScenarioState.Level;
			int num5 = num2 / num4;
			int num6 = num3 / num4;
			for (int i = 0; i < num6 - num5; i++)
			{
				SpawnWaterTile();
			}
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null));
		}
	}

	private void UpdateObjectives()
	{
		if (_scenarioActor != null)
		{
			_scenarioHealthLoseObjective.Current = _scenarioActor.MaxHealth - (int)MathF.Max(0f, _scenarioActor.Health);
			_scenarioHealthLoseObjective.Total = _scenarioActor.MaxHealth;
			_scenarioHealthLoseObjective.ScriptedIsComplete = _scenarioHealthLoseObjective.Current == _scenarioHealthLoseObjective.Total;
		}
		CheckObjectivesComplete();
		if (!_lastRoom.Revealed && RevealedMonstersAlive.Count == 0 && !trapDoorShown)
		{
			ToggleHatchState(open: false);
			trapDoorShown = true;
			ScenarioManager.ToggleTileHighlights(new List<Point> { _aLinkIndex }, enabled: true);
		}
		else
		{
			if (!_lastRoom.Revealed || connectionsShown)
			{
				return;
			}
			ToggleHatchState(open: true);
			trapDoorShown = true;
			connectionsShown = true;
			List<Point> list = new List<Point>();
			list.Add(_aLinkIndex);
			list.AddRange(_bLinkIndexes);
			ScenarioManager.ToggleTileHighlights(list, enabled: true);
			List<(Point, Point)> list2 = new List<(Point, Point)>();
			foreach (Point bLinkIndex in _bLinkIndexes)
			{
				list2.Add((_aLinkIndex, bLinkIndex));
			}
			ScenarioManager.ToggleTileLinks(list2, enabled: true);
		}
	}

	private void RevealLastRoom()
	{
		_lastRoom.Reveal(initial: false);
		CRevealRooms_MessageData message = new CRevealRooms_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		GameState.SortIntoInitiativeAndIDOrder();
	}

	private void LinkRoomTiles()
	{
		CNode node = ScenarioManager.PathFinder.GetNode(_aLinkIndex);
		foreach (Point bLinkIndex in _bLinkIndexes)
		{
			CNode node2 = ScenarioManager.PathFinder.GetNode(bLinkIndex);
			CNavLink item = new CNavLink(1, node2);
			if (!node.NavLinks.Contains(item))
			{
				node.NavLinks.Add(item);
			}
			item = new CNavLink(1, node);
			if (!node2.NavLinks.Contains(item))
			{
				node2.NavLinks.Add(item);
			}
		}
	}

	public void ToggleHatchState(bool open)
	{
		CObjectProp prop = ScenarioManager.Tiles[13, 11].m_Props.Find((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect);
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = prop;
		cChangePropVisualState_MessageData.m_stateName = (open ? "State1" : "State0");
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	public bool ShouldShowHatchDoor()
	{
		if (!_lastRoom.Revealed)
		{
			return RevealedMonstersAlive.Count == 0;
		}
		return false;
	}

	protected override void TickDeathInternal()
	{
		if (ShouldShowHatchDoor())
		{
			ScenarioManager.Tiles[13, 11].m_Props.Find((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.Obstacle)?.DestroyProp();
			_scenarioBelowDeck1Modifier.SetHiddenState(shouldSetHidden: false);
			foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
			{
				TryOpenLastRoom(player.Actor);
			}
		}
		UpdateObjectives();
	}

	private void TryOpenLastRoom(CActor withPlayer)
	{
		if (ShouldShowHatchDoor() && withPlayer.Type == CActor.EType.Player && !(withPlayer is CHeroSummonActor) && !(withPlayer.ArrayIndex != _aLinkIndex))
		{
			_scenarioBelowDeck2Modifier.SetHiddenState(shouldSetHidden: false);
			_scenarioBelowDeck3Modifier.SetHiddenState(shouldSetHidden: false);
			_scenarioHealthLoseObjective.SetActivation(active: true);
			_scenarioActor.Deactivated = false;
			RevealLastRoom();
			LinkRoomTiles();
			ToggleHatchState(open: true);
		}
	}

	protected override void TickRepositionInternal()
	{
		TryOpenLastRoom(_tickActor);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (message.MessageName == "FS_022_Ice_Floes_Special_1_1")
		{
			if (ShouldShowHatchDoor())
			{
				showedTrapDoorNarration = true;
				return true;
			}
		}
		else if (message.MessageName == "FS_022_Ice_Floes_Special_2_1" && showedTrapDoorNarration && _lastRoom.Revealed)
		{
			showedTrapDoorNarration = false;
			return true;
		}
		return false;
	}

	protected override bool Initialize()
	{
		_scenarioHealthLoseObjective = (CObjective_Scripted)GetObjective("ScenarioHealthLoseObjective");
		_scenarioBelowDeck1Modifier = GetScenarioModifier("BelowDeck1");
		_scenarioBelowDeck2Modifier = GetScenarioModifier("BelowDeck2");
		_scenarioBelowDeck3Modifier = GetScenarioModifier("BelowDeck3");
		_lastRoom = ScenarioManager.CurrentScenarioState.Maps.Find((CMap map) => map.RoomName == "Room_7");
		_scenarioActor = ScenarioManager.Scenario.FindObjectActorAt(ScenarioActorIndex);
		_scenarioActor.SetCanBeTargetedFromAnywhere(value: true);
		_scenarioBelowDeck2Modifier.CustomLocKeyArgs = new List<object> { _scenarioActor.MaxHealth };
		CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(_scenarioActor, active: false);
		ScenarioRuleClient.MessageHandler(message);
		if (_scenarioActor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).Count == 0)
		{
			CAbility cAbility = GetScenarioModifier("ImmunityAbility").TriggerAbility();
			cAbility.Start(_scenarioActor, _scenarioActor);
			cAbility.ApplyToActor(_scenarioActor);
		}
		_scenarioActor.Deactivated = !_lastRoom.Revealed;
		if (_lastRoom.Revealed)
		{
			LinkRoomTiles();
		}
		if (ShouldShowHatchDoor())
		{
			showedTrapDoorNarration = true;
		}
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		UpdateObjectives();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			_scenarioHealthLoseObjective.SetActivation(active: false);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 414);
			break;
		}
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		int num = (int)Math.Floor((float)GetRoomTiles("Room_7").Count((CTile x) => x.FindProp(ScenarioManager.ObjectImportType.TerrainWater) != null) / 5f);
		return new List<object> { num };
	}

	public override bool IsScenarioActor(CActor actor)
	{
		return actor == _scenarioActor;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("water_hex_spawn_threshold", (4 + 2 * ScenarioManager.CurrentScenarioState.Level).ToString());
	}

	public override bool TreatTileAsNonFeatureless(CTile tile)
	{
		if (tile.m_ArrayIndex == _aLinkIndex)
		{
			return true;
		}
		return base.TreatTileAsNonFeatureless(tile);
	}
}
