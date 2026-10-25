using System;
using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH082 : ScenarioScript
{
	private const string BlizzardBarrierPrefabName = "BlizzardBarrier";

	private static readonly List<(Point, Point)> CorridorsPointsCA = new List<(Point, Point)>
	{
		(new Point(9, 4), new Point(10, 4)),
		(new Point(6, 7), new Point(-1, -1)),
		(new Point(7, 12), new Point(8, 12)),
		(new Point(10, 12), new Point(11, 12)),
		(new Point(13, 9), new Point(-1, -1)),
		(new Point(12, 4), new Point(13, 4))
	};

	private static readonly List<(Point, Point)> CorridorsPointsDB = new List<(Point, Point)>
	{
		(new Point(11, 9), new Point(-1, -1)),
		(new Point(11, 6), new Point(-1, -1)),
		(new Point(8, 7), new Point(-1, -1)),
		(new Point(9, 10), new Point(-1, -1))
	};

	private CScenarioModifier _moveDebuffScenarioModifier;

	private CScenarioModifier _searchTilesScenarioModifier;

	private CObjective_Scripted _searchObjective;

	[StateDump(Skip = true)]
	private int _lastCorridorsRelocateRound = -1;

	public static IEnumerable<CObjectActor> Snowdrifts(bool includeDead = false)
	{
		return from x in ScenarioManager.CurrentScenarioState.Objects
			where x?.Object?.AttachedProp != null && x.Object.AttachedProp.SpecificPropType == ESpecificPropType.SnowDriftPile && (includeDead || !x.Object.IsDead)
			select x.Object;
	}

	private void InitSnowdrifts()
	{
		foreach (CObjectActor item in Snowdrifts(includeDead: true))
		{
			item.IsTransientBlocker = false;
			item.SetIgnoreActorCollision(enable: true);
			(item.AttachedProp as CObjectObstacle).SetIsBlocker(isBlocker: false);
			item.ForcedUntargetable = true;
		}
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			if (_tickActor is CPlayerActor)
			{
				TryEscapeMap();
				if (ScenarioManager.Scenario.PlayerActors.Count == 0 && !_searchObjective.ScriptedIsComplete)
				{
					_searchObjective.ScriptedUnableToComplete = true;
				}
			}
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			RelocateAllCorridors();
			break;
		case EScenarioModifierTriggerPhase.StartScenario:
			TickStartScenario();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 78);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.AfterAbility:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override bool Initialize()
	{
		_moveDebuffScenarioModifier = GetScenarioModifier("moveDebuffScenarioModifier");
		_searchTilesScenarioModifier = GetScenarioModifier("searchTilesScenarioModifier");
		_searchObjective = GetObjective("searchObjective") as CObjective_Scripted;
		if (_searchObjective == null)
		{
			Trace("WinObjective: searchObjective not found!", null, 87);
			return false;
		}
		_searchObjective.HideOnComplete = true;
		UpdateCorrdidorsVisualState();
		ScenarioManager.ToggleTileHighlights(from x in CorridorsPointsCA.Select(((Point, Point) x) => x.Item1).Concat(CorridorsPointsCA.Select(((Point, Point) x) => x.Item2)).Concat(CorridorsPointsDB.Select(((Point, Point) x) => x.Item1))
				.Concat(CorridorsPointsDB.Select(((Point, Point) x) => x.Item2))
			where x.X != -1 && x.Y != -1
			select x, enabled: true, new CVector4(1f, 1f, 0f, 1f));
		CPathFinder.s_CanStepCallback = (CPathFinderContext context, Point ptFrom, Point ptTo) =>
		{
			if (context.actor == null)
			{
				return CPathFinder.ECanStepCallbackResult.Default;
			}
			Point point = new Point(5, 12);
			Point point2 = new Point(4, 11);
			Point point3 = new Point(6, 12);
			Point point4 = new Point(5, 11);
			if (ptFrom == point && (ptTo == point3 || ptTo == point4))
			{
				return CPathFinder.ECanStepCallbackResult.ForceCannotStep;
			}
			if (ptTo == point && (ptFrom == point3 || ptFrom == point4))
			{
				return CPathFinder.ECanStepCallbackResult.ForceCannotStep;
			}
			if (ptFrom == point2 && ptTo == point4)
			{
				return CPathFinder.ECanStepCallbackResult.ForceCannotStep;
			}
			return (ptTo == point2 && ptFrom == point4) ? CPathFinder.ECanStepCallbackResult.ForceCannotStep : CPathFinder.ECanStepCallbackResult.Default;
		};
		return base.Initialize();
	}

	protected override void Done()
	{
		base.Done();
		CPathFinder.s_CanStepCallback = null;
	}

	protected override void DeserializeInternal(string s)
	{
		InitSnowdrifts();
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PrefabName == "BlizzardBarrier" && x is CObjectObstacle))
		{
			item.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
		}
	}

	private void TickStartScenario()
	{
		if (OncePerRound())
		{
			IEnumerable<CObjectActor> source = Snowdrifts();
			_searchObjective.Total = source.Count();
			UpdateObjectives();
		}
	}

	private void RelocateAllCorridors()
	{
		if (_lastCorridorsRelocateRound != ScenarioManager.CurrentScenarioState.RoundNumber)
		{
			RelocateCorridors(CorridorsPointsCA);
			RelocateCorridors(CorridorsPointsDB);
			_lastCorridorsRelocateRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		}
	}

	private void RelocateCorridors(List<(Point, Point)> corridorsPoints)
	{
		for (int i = 0; i < corridorsPoints.Count; i++)
		{
			CTile tile = ScenarioManager.GetTile(corridorsPoints[i].Item1);
			CTile tile2 = ScenarioManager.GetTile(corridorsPoints[i].Item2);
			if ((ScenarioScript.RoundNumber - 1) % corridorsPoints.Count == i)
			{
				EnsureTileHasNoCorridor(tile);
				EnsureTileHasNoCorridor(tile2);
			}
			else
			{
				TryPlaceCorridor(tile, tile2);
			}
		}
		UpdateCorrdidorsVisualState();
	}

	private void TryPlaceCorridor(CTile tile1, CTile tile2)
	{
		if (tile1 != null && tile1.FindProps(ScenarioManager.ObjectImportType.Obstacle).FirstOrDefault((CObjectProp p) => p.PrefabName == "BlizzardBarrier") == null)
		{
			CObjectProp[] array = tile1.m_Props.Where((CObjectProp x) => !(x is CObjectDoor)).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				array[num].DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			}
			List<TileIndex> pathingBlockers = new List<TileIndex>
			{
				new TileIndex(tile1.m_ArrayIndex)
			};
			SpawnObstacleProp(tile1, pathingBlockers, "BlizzardBarrier", null, GetRandomObstacleRotation(tile1), ignoresFlyAndJump: true, blockLOS: true)?.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
			if (tile2 != null)
			{
				pathingBlockers = new List<TileIndex>
				{
					new TileIndex(tile2.m_ArrayIndex)
				};
				SpawnObstacleProp(tile2, pathingBlockers, "BlizzardBarrier", null, GetRandomObstacleRotation(tile2), ignoresFlyAndJump: true, blockLOS: true)?.SetOverrideDisallowMoveOrDestroy(shouldDisallow: true);
			}
			MoveAwayActors(tile1);
			MoveAwayActors(tile2);
		}
	}

	private CVector3 GetRandomObstacleRotation(CTile tile)
	{
		ScenarioManager.EAdjacentPosition eAdjacentPosition = (ScenarioManager.EAdjacentPosition)((tile.m_ArrayIndex.X * 3 + tile.m_ArrayIndex.Y * 5) % 6);
		if (eAdjacentPosition == ScenarioManager.EAdjacentPosition.ECenter)
		{
			eAdjacentPosition++;
		}
		CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eAdjacentPosition);
		CVector3 position = tile.m_Hex.Position;
		CVector3 cVector = adjacentTile.m_Hex.Position - position;
		float num = (float)(Math.Atan2(cVector.Z, cVector.X) * (180.0 / Math.PI));
		return new CVector3(0f, 0f - num, 0f);
	}

	private void MoveAwayActors(CTile tile)
	{
		if (tile == null)
		{
			return;
		}
		foreach (CActor livingActor in tile.LivingActors)
		{
			if (GetNearestFreeTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, out var nearestTile))
			{
				GameState.TeleportActor(livingActor, nearestTile);
			}
			else
			{
				Trace("Can't find nearest free tile to move actor from the corridor!", null, 223);
			}
		}
	}

	private void EnsureTileHasNoCorridor(CTile tile)
	{
		if (tile != null)
		{
			tile.FindProps(ScenarioManager.ObjectImportType.Obstacle).FirstOrDefault((CObjectProp p) => p.PrefabName == "BlizzardBarrier")?.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true);
			ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Walkable = true;
			ScenarioManager.PathFinder.GetNode(tile.m_ArrayIndex).Blocked = tile.FindProps(ScenarioManager.ObjectImportType.Obstacle).Count != 0;
		}
	}

	private void UpdateCorrdidorsVisualState()
	{
		UpdateCorrdidorsVisualState(CorridorsPointsCA);
		UpdateCorrdidorsVisualState(CorridorsPointsDB);
	}

	private void UpdateCorrdidorsVisualState(List<(Point, Point)> corridorsPoints)
	{
		for (int i = 0; i < corridorsPoints.Count; i++)
		{
			string text = "StateBlocked";
			string text2 = "FH_Scenario_Campaign_082_Barrier_";
			string descLocalization = null;
			if ((ScenarioScript.RoundNumber - 1) % corridorsPoints.Count == i)
			{
				text = "StateForming";
				text2 = null;
			}
			else if (ScenarioScript.RoundNumber % corridorsPoints.Count == i)
			{
				text = "StateWaining";
				text2 += text;
				descLocalization = "FH_82_Brittle_Ice_Wall_D";
			}
			else
			{
				text2 += text;
				descLocalization = "FH_82_Solid_Ice_Wall_D";
			}
			UpdateCorrdidorVisualState(corridorsPoints[i].Item1, text, text2, descLocalization);
			UpdateCorrdidorVisualState(corridorsPoints[i].Item2, text, text2, descLocalization);
		}
	}

	private void UpdateCorrdidorVisualState(Point corridor, string state, string localization, string descLocalization)
	{
		CTile tile = ScenarioManager.GetTile(corridor);
		if (tile == null)
		{
			return;
		}
		CObjectProp cObjectProp = tile.FindProps(ScenarioManager.ObjectImportType.Obstacle).FirstOrDefault((CObjectProp p) => p.PrefabName == "BlizzardBarrier");
		CObjectProp cObjectProp2 = tile.FindProps(ScenarioManager.ObjectImportType.VFXProp).FirstOrDefault((CObjectProp p) => p.PrefabName == "BlizzardBarrier");
		CObjectProp cObjectProp3 = null;
		if (cObjectProp != null)
		{
			cObjectProp2?.DestroyProp();
			cObjectProp3 = cObjectProp;
		}
		else
		{
			if (cObjectProp2 == null)
			{
				ScenarioManager.GetTile(corridor);
				cObjectProp2 = SpawnVFXProp(tile, "BlizzardBarrier");
			}
			cObjectProp3 = cObjectProp2;
		}
		cObjectProp3.PropCustomLocKey = localization;
		cObjectProp3.PropCustomDescriptionLocKey = descLocalization;
		CChangePropVisualState_MessageData cChangePropVisualState_MessageData = new CChangePropVisualState_MessageData();
		cChangePropVisualState_MessageData.m_prop = cObjectProp3;
		cChangePropVisualState_MessageData.m_stateName = state;
		cChangePropVisualState_MessageData.m_delay = 0.001f;
		ScenarioRuleClient.MessageHandler(cChangePropVisualState_MessageData);
	}

	public static List<CObjectActor> GetSnowDriftsInRange(CPlayerActor player)
	{
		return (from x in Snowdrifts()
			where x.GetDistanceTo(player) <= 1
			select x).ToList();
	}

	private void ApplyMoveDebuffBonusToPlayers()
	{
		foreach (PlayerState player in ScenarioScript._css.Players)
		{
			CAbility cAbility = _moveDebuffScenarioModifier.TriggerAbility();
			cAbility.Start(player.Actor, player.Actor);
			cAbility.ApplyToActor(player.Actor);
		}
	}

	private void UpdateObjectives()
	{
		if (!_searchObjective.ScriptedIsComplete)
		{
			int num = Snowdrifts().Count((CObjectActor x) => !x.IsDead);
			_searchObjective.Current = _searchObjective.Total - num;
			_searchObjective.ScriptedIsComplete = _searchObjective.Current >= _searchObjective.Total;
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			if (_searchObjective.ScriptedIsComplete)
			{
				ApplyMoveDebuffBonusToPlayers();
				_searchTilesScenarioModifier.Hide();
				_moveDebuffScenarioModifier.Show();
			}
		}
	}

	protected override bool AllowEscapeZoneInternal(CActor actor)
	{
		if (!_searchObjective.IsComplete)
		{
			return false;
		}
		return base.AllowEscapeZoneInternal(actor);
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		if (!_tickAbility.GetMiscVar("TargetUntargetAbleStart").GetBool())
		{
			return;
		}
		foreach (CObjectActor item in Snowdrifts())
		{
			item.ForcedUntargetable = false;
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		bool flag = _tickAbility.GetMiscVar("TargetUntargetAbleStart").GetBool();
		if (_tickAbility.GetMiscVar("TargetUntargetAbleEnd").GetBool() || (flag && (_tickAbility.ActorsToTarget.Count == 0 || _tickAbility.AbilityHasBeenCancelled)))
		{
			foreach (CObjectActor item in Snowdrifts(includeDead: true))
			{
				item.ForcedUntargetable = true;
			}
		}
		UpdateObjectives();
	}

	protected override void TickAbilityUndoneInternal()
	{
		base.TickAbilityUndoneInternal();
		bool flag = _tickAbility.GetMiscVar("TargetUntargetAbleStart").GetBool();
		if (_tickAbility.GetMiscVar("TargetUntargetAbleEnd").GetBool() || (flag && (_tickAbility.ActorsToTarget.Count == 0 || _tickAbility.AbilityHasBeenCancelled)))
		{
			foreach (CObjectActor item in Snowdrifts(includeDead: true))
			{
				item.ForcedUntargetable = true;
			}
		}
		UpdateObjectives();
	}

	public override bool TreatTilesAsSameRoom(CTile tileA, CTile tileB)
	{
		return true;
	}
}
