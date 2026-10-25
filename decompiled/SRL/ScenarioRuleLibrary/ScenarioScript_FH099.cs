using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH099 : ScenarioScript
{
	private List<Point> RoomSteps = new List<Point>
	{
		new Point(3, 18),
		new Point(6, 15),
		new Point(3, 4)
	};

	private CMap _treasureRoom;

	public override bool EnemiesAttackWithMinimalMovement()
	{
		return true;
	}

	protected override bool Initialize()
	{
		_treasureRoom = ScenarioScript.FindRoom("TreasureRoom");
		return true;
	}

	private bool IsInCentralRoom(CActor actor)
	{
		Point arrayIndex = actor.ArrayIndex;
		if (arrayIndex.X == 7 && arrayIndex.Y == 11)
		{
			return false;
		}
		if (arrayIndex.X <= 7)
		{
			int y = arrayIndex.Y;
			if (y >= 8)
			{
				return y <= 14;
			}
		}
		return false;
	}

	private bool IsInTreasureRoom(CActor actor)
	{
		if (actor.Tile.m_HexMap != _treasureRoom)
		{
			return actor.Tile.m_Hex2Map == _treasureRoom;
		}
		return true;
	}

	private bool IsOnHighGround(CActor actor)
	{
		if (!IsInCentralRoom(actor))
		{
			return !IsInTreasureRoom(actor);
		}
		return false;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 52);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			break;
		}
	}

	private void TickStartTurn()
	{
		UpdateTargeting();
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		UpdateTargeting();
	}

	private void UpdateTargeting()
	{
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.SetCanBeTargetedFromAnywhere(IsInCentralRoom(allAliveActor));
			allAliveActor.SetCanTargetFromAnywhere(IsOnHighGround(allAliveActor) ? CActor.TargetFromAnywhereFlags.Ranged : CActor.TargetFromAnywhereFlags.None);
		}
	}

	public override IEnumerable<CTile> TilesFullyBlockedForMovement()
	{
		if (!(_tickActor is CEnemyActor) || _tickActor.IsDead || _tickActor.MindControlAbility != null || !(_tickAbility is CAbilityMove))
		{
			yield break;
		}
		foreach (Point roomStep in RoomSteps)
		{
			CTile tile = ScenarioManager.GetTile(roomStep);
			if (tile != null)
			{
				yield return tile;
			}
		}
	}
}
