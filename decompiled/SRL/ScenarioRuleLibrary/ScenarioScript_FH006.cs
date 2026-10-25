using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH006 : ScenarioScript
{
	private CScenarioModifier _destroySnowRocksModifier;

	private CObjectDoor _door2;

	protected override bool Initialize()
	{
		_destroySnowRocksModifier = GetScenarioModifier("DestroySnowRocks");
		_door2 = GetProp(17, 4, "ThinDoor") as CObjectDoor;
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.OnRoomRevealed:
			TickPhaseRoomRevealed();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 39);
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	private void TickPhaseEndRound()
	{
		if (_tickActor.Type != CActor.EType.Player || _tickActor.IsDead)
		{
			return;
		}
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(_tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y))
		{
			if (allAdjacentTile.m_Props.Any((CObjectProp p) => p.PropHealthDetails?.CustomLocKey == "Snowdrift"))
			{
				int hazardousTerrainDamage = ScenarioManager.HazardousTerrainDamage;
				Trace($"Suffer hazardous damage from snow rock/snowdrift {hazardousTerrainDamage}", null, 59);
				GameState.ActorBeenDamaged(_tickActor, hazardousTerrainDamage, checkIfPlayerCanAvoidDamage: true);
				GameState.ActorHealthCheck(_tickActor, _tickActor);
				break;
			}
		}
	}

	private void TickPhaseRoomRevealed()
	{
		_destroySnowRocksModifier.SetHiddenState(_door2.DoorIsClosed);
	}
}
