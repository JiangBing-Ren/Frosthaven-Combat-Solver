using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH116 : ScenarioScript
{
	private static readonly string WagonClassId = "WagonID";

	private static readonly int WagonCount = 5;

	private static readonly List<Point> RoadPositions = new List<Point>
	{
		new Point(2, 8),
		new Point(2, 7),
		new Point(3, 6),
		new Point(3, 5),
		new Point(4, 5),
		new Point(5, 6),
		new Point(6, 6),
		new Point(6, 5),
		new Point(7, 4),
		new Point(8, 4),
		new Point(9, 4),
		new Point(9, 5),
		new Point(10, 6),
		new Point(11, 6),
		new Point(11, 5),
		new Point(12, 4),
		new Point(12, 3),
		new Point(13, 3)
	};

	private int _savedWagonsCount;

	private CObjective_Scripted _winObjective;

	private int TrapDamage => ScenarioManager.TrapDamage;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private bool IsWagon(CActor actor)
	{
		return actor.Class.ID == WagonClassId;
	}

	private static bool WagonReachedRoadEnd(CActor actor)
	{
		return actor.Tile.m_ArrayIndex == RoadPositions.Last();
	}

	private static ActorState GetAllyState(CActor allyActor)
	{
		return ScenarioManager.CurrentScenarioState.AllyMonsters.FirstOrDefault((EnemyState am) => am.ActorGuid == allyActor.ActorGuid);
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			UpdateWagonInitiativePriority();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 49);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
		case EScenarioModifierTriggerPhase.AfterAbility:
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			break;
		}
	}

	protected override bool Initialize()
	{
		_winObjective = (CObjective_Scripted)ScenarioScript._css.WinObjectives[0];
		_winObjective.Total = WagonCount;
		UpdateWagonInitiativePriority();
		ScenarioManager.ToggleTileHighlights(new List<Point> { RoadPositions.Last() }, enabled: true);
		return true;
	}

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(_savedWagonsCount);
		Trace("[fs116]Serialize: " + text + "[-]", null, 64);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[fs116]Deserialize " + s + "[-]", null, 69);
		if (!string.IsNullOrEmpty(s))
		{
			_savedWagonsCount = JsonConvert.DeserializeObject<int>(s);
			CheckObjectivesComplete();
		}
	}

	private void TickEndRound()
	{
		if (IsWagon(_tickActor))
		{
			DestroyIfReachedRoadEnd(_tickActor);
		}
	}

	private void SetDeathDissolveLifeTime(CActor actor, float lifeTime)
	{
		CSetDeathDissolveLifeTime_MessageData cSetDeathDissolveLifeTime_MessageData = new CSetDeathDissolveLifeTime_MessageData(actor);
		cSetDeathDissolveLifeTime_MessageData.m_DissolveLifeTime = lifeTime;
		ScenarioRuleClient.MessageHandler(cSetDeathDissolveLifeTime_MessageData);
	}

	protected override void TickAbilityEndedInternal()
	{
		if (IsWagon(_tickActor) && _tickAbility is CAbilityNull cAbilityNull && (cAbilityNull.Flags & CAbility.EAbilityFlag.Special1) != 0)
		{
			MoveToNextRoadTile(_tickActor);
		}
	}

	protected override void TickDeathInternal()
	{
		if (IsWagon(_tickActor))
		{
			SetDeathDissolveLifeTime(_tickActor, 5f);
			ProgressWinObjective();
		}
	}

	private void MoveToNextRoadTile(CActor wagonActor)
	{
		if (WagonReachedRoadEnd(wagonActor))
		{
			ScenarioScript.current.Trace("MoveToNextRoadTile: Wagon $" + wagonActor.ActorGuid + " has already reached end of the road.", null, 109);
			return;
		}
		Point nextRoadPoint = GetNextRoadPoint(wagonActor);
		CTile tile = ScenarioManager.GetTile(nextRoadPoint);
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex);
		GameState.MoveActorStraightLine(wagonActor, nextRoadPoint, shouldWait: true);
		CObjectObstacle cObjectObstacle = tile.FindObstacleProp();
		if (cObjectObstacle != null && cObjectObstacle.PropType != EPropType.TerrainVisualEffect)
		{
			cObjectObstacle.DestroyProp();
			DealScenarioDamage(wagonActor, TrapDamage, isTrapDamage: true);
		}
		if (cActor != null)
		{
			if (GetNearestFreeTile(cActor.Tile.m_ArrayIndex.X, cActor.Tile.m_ArrayIndex.Y, out CTile nearestTile, (List<CTile>)null, (Func<int, int, bool>)AreCoordinatesNotOnRoad))
			{
				if (ScenarioManager.IsTileAdjacent(nearestTile, cActor.Tile))
				{
					CustomAbility.MoveActorInDirection(cActor, nearestTile);
				}
				else
				{
					TeleportActor(cActor, nearestTile);
				}
			}
			else
			{
				ScenarioScript.current.Trace("MoveToNextRoadTile: Can't find  nearest free tile for actor $" + cActor.ActorGuid + ".", null, 135);
			}
			DealScenarioDamage(cActor, TrapDamage, isTrapDamage: true);
		}
		UpdateWagonInitiativePriority();
		if (tile.FindObstacleProp() != null)
		{
			ScenarioScript.current.Error("MoveToNextRoadTile: Wagon $" + wagonActor.ActorGuid + " still has an obstacle on next road tile!", null, 142);
		}
		static bool AreCoordinatesNotOnRoad(int x, int y)
		{
			return !RoadPositions.Contains(new Point(x, y));
		}
	}

	private void UpdateWagonInitiativePriority()
	{
		int num = 0;
		foreach (Point rp in RoadPositions)
		{
			IEnumerable<EnemyState> enumerable = ScenarioScript._css.AllyMonsters.Where((EnemyState am) => am.Actor.Tile.m_ArrayIndex == rp);
			if (!enumerable.Any())
			{
				continue;
			}
			foreach (EnemyState item in enumerable)
			{
				if (item.Actor is CEnemyActor cEnemyActor)
				{
					cEnemyActor.SubInitiative = num;
				}
			}
			num++;
		}
	}

	private void DestroyIfReachedRoadEnd(CActor wagonActor)
	{
		if (wagonActor.CauseOfDeath == CActor.ECauseOfDeath.StillAlive && WagonReachedRoadEnd(wagonActor))
		{
			SetDeathDissolveLifeTime(wagonActor, 0.01f);
			GameState.KillActor(wagonActor, wagonActor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _);
			_savedWagonsCount++;
			ProgressWinObjective();
		}
	}

	private void ProgressWinObjective()
	{
		_winObjective.Current++;
		if (_winObjective.Current == _winObjective.Total)
		{
			_winObjective.ScriptedUnableToComplete = _savedWagonsCount == 0;
		}
		_winObjective.ScriptedIsComplete = _winObjective.Current >= _winObjective.Total;
		_winObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private Point GetNextRoadPoint(CActor wagonActor)
	{
		Point arrayIndex = wagonActor.Tile.m_ArrayIndex;
		if (!RoadPositions.Contains(arrayIndex))
		{
			Error("Wagon tile is not on the road!", null, 186);
			return RoadPositions.Last();
		}
		if (WagonReachedRoadEnd(wagonActor))
		{
			Trace("Wagon reached last road position.", null, 191);
			return arrayIndex;
		}
		int num = RoadPositions.IndexOf(arrayIndex);
		return RoadPositions[num + 1];
	}

	protected override List<object> GetConclusionParametersInternal()
	{
		return new List<object> { _savedWagonsCount };
	}
}
