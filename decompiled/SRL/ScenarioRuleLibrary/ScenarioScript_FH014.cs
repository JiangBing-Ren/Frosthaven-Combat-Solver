using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH014 : ScenarioScript
{
	private bool _damagedThisRound;

	private CSpawner _bSpawner;

	private CSpawner _cSpawner1;

	private CSpawner _cSpawner2;

	private CSpawner _eSpawner1;

	private CSpawner _eSpawner2;

	private CObjectProp _snowRock;

	protected override bool Initialize()
	{
		_ = ScenarioManager.CurrentScenarioState.Props;
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_bSpawner = spawners[1];
		_eSpawner1 = spawners[2];
		_cSpawner1 = spawners[3];
		_eSpawner2 = spawners[4];
		_cSpawner2 = spawners[5];
		_snowRock = GetProp(12, 4, "OneHexObstacle", checkDestroyed: true);
		return true;
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (_snowRock != null)
		{
			_snowRock.IsImmuneToDamage = true;
			if (_snowRock.RuntimeAttachedActor != null)
			{
				_snowRock.RuntimeAttachedActor.ForcedUntargetable = true;
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 53);
			break;
		}
	}

	private void TickStartTurn()
	{
	}

	private void TickEndTurn()
	{
	}

	private void TickStartRound()
	{
		if (OncePerRound() && ScenarioScript.RoundNumber == 1)
		{
			_bSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
			_cSpawner1.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
			_cSpawner2.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
		}
		_damagedThisRound = false;
	}

	private void TickEndRound()
	{
		if (_damagedThisRound || _tickActor.Type != CActor.EType.Player || _tickActor.IsDead)
		{
			return;
		}
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(_tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y))
		{
			CObjectProp cObjectProp = allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle);
			if (cObjectProp == _snowRock)
			{
				int num = 1;
				Trace($"Damage the snow rock {num}", null, 95);
				GameState.ActorBeenDamaged(cObjectProp.RuntimeAttachedActor, num);
				GameState.ActorHealthCheck(cObjectProp.RuntimeAttachedActor, cObjectProp.RuntimeAttachedActor);
				_damagedThisRound = true;
				break;
			}
		}
	}
}
