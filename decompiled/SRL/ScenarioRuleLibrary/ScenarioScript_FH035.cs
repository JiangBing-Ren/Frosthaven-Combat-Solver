using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH035 : ScenarioScript
{
	private static int SEARCH_DEBRIS_TURNS = 3;

	private static string[] _narrativeEventsNames = new string[3] { "FS035_1", "FS035_2", "FS035_3" };

	private CObjectProp[] debrises;

	private CObjective_Scripted[] _objectives;

	private CSpawner[] _spawnersF;

	private CSpawner[] _spawnersG;

	private CActor _crain;

	protected override bool Initialize()
	{
		_ = ScenarioManager.CurrentScenarioState.Props;
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		if (ScenarioManager.CurrentScenarioState.AllyMonsters.Count == 0)
		{
			Error("Couldn't find any AllyMonsters on the map.", null, 27);
			return false;
		}
		_crain = ScenarioManager.CurrentScenarioState.AllyMonsters[0].Actor;
		debrises = new CObjectProp[4]
		{
			GetProp(4, 9, "OneHexObstacle") as CObjectObstacle,
			GetProp(9, 6, "OneHexObstacle") as CObjectObstacle,
			GetProp(12, 7, "OneHexObstacle") as CObjectObstacle,
			GetProp(13, 4, "OneHexObstacle") as CObjectObstacle
		};
		if (debrises.Contains(null))
		{
			Error("Couldn't find all debris props on the map.", null, 41);
			return false;
		}
		_spawnersF = new CSpawner[2]
		{
			spawners[0],
			spawners[2]
		};
		_spawnersG = new CSpawner[1] { spawners[1] };
		_objectives = new CObjective_Scripted[4]
		{
			GetObjective("SearchDebrisB") as CObjective_Scripted,
			GetObjective("SearchDebrisC") as CObjective_Scripted,
			GetObjective("SearchDebrisD") as CObjective_Scripted,
			GetObjective("SearchDebrisE") as CObjective_Scripted
		};
		if (ScenarioScript.RoundNumber == 1)
		{
			for (int i = 0; i < _objectives.Length; i++)
			{
				CObjective_Scripted obj = _objectives[i];
				obj.Total = ((i == 3) ? (SEARCH_DEBRIS_TURNS + 1) : SEARCH_DEBRIS_TURNS);
				obj.Current = 0;
				obj.ScriptedIsComplete = false;
			}
		}
		UpdateCrainOverrideTarget(_tickActor);
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			TickPhaseStartScenario();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 85);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
			break;
		}
	}

	protected void TickPhaseStartRound()
	{
		if (_tickActor == _crain)
		{
			SpawnEnemies();
		}
	}

	protected void TickPhaseStartScenario()
	{
		if (_tickActor == _crain)
		{
			ScenarioManager.ToggleTileHighlights(new List<Point>
			{
				new Point(debrises[0].ArrayIndex)
			}, enabled: true);
		}
	}

	private void SpawnEnemies()
	{
		CObjective_Scripted currentObjective = GetCurrentObjective();
		if (currentObjective == null)
		{
			return;
		}
		CSpawner[] currentSpawners = GetCurrentSpawners(currentObjective);
		if (currentSpawners != null)
		{
			CSpawner[] array = currentSpawners;
			foreach (CSpawner obj in array)
			{
				obj.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: true);
				obj.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
			}
		}
	}

	private void TickPhaseEndRound()
	{
		if (_tickActor == _crain)
		{
			UpdateCrainObjectives(_tickActor);
			UpdateCrainOverrideTarget(_tickActor);
			UpdateObjectiveSpawnerIndicators();
		}
	}

	protected override void TickRepositionInternal()
	{
		UpdateCrainOverrideTarget(_crain);
	}

	public override Point GetActorIteractionTile(CActor actor)
	{
		if (actor != _crain)
		{
			return default;
		}
		CObjective_Scripted currentObjective = GetCurrentObjective();
		if (currentObjective != null)
		{
			int num = Array.IndexOf(_objectives, currentObjective);
			CObjectProp cObjectProp = debrises[num];
			if (ScenarioManager.IsTileAdjacent(_crain.ArrayIndex.X, _crain.ArrayIndex.Y, cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y))
			{
				return new Point(cObjectProp.ArrayIndex);
			}
		}
		return default;
	}

	public override bool IsActorInteracting(CActor actor)
	{
		return GetActorIteractionTile(actor) != default(Point);
	}

	private void UpdateCrainObjectives(CActor crain)
	{
		CObjective_Scripted currentObjective = GetCurrentObjective();
		if (currentObjective == null)
		{
			return;
		}
		int num = Array.IndexOf(_objectives, currentObjective);
		CObjectProp cObjectProp = debrises[num];
		if (!currentObjective.IsActive)
		{
			currentObjective.SetActivation(active: true);
			CObjectProp[] array = debrises;
			foreach (CObjectProp cObjectProp2 in array)
			{
				ScenarioManager.ToggleTileHighlights(new List<Point>
				{
					new Point(cObjectProp2.ArrayIndex)
				}, cObjectProp2 == cObjectProp);
			}
		}
		if (!ScenarioManager.IsTileAdjacent(crain.ArrayIndex.X, crain.ArrayIndex.Y, cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y))
		{
			return;
		}
		currentObjective.Current++;
		if (currentObjective.Current >= currentObjective.Total)
		{
			currentObjective.ScriptedIsComplete = true;
			currentObjective.HideOnComplete = true;
			ScenarioManager.ToggleTileHighlights(new List<Point>
			{
				new Point(cObjectProp.ArrayIndex)
			}, enabled: false);
			if (num < _objectives.Length - 1)
			{
				int num2 = num + 1;
				CObjective_Scripted obj = _objectives[num2];
				CObjectProp cObjectProp3 = debrises[num2];
				obj.SetActivation(active: true);
				ScenarioManager.ToggleTileHighlights(new List<Point>
				{
					new Point(cObjectProp3.ArrayIndex)
				}, enabled: true);
			}
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	private void UpdateObjectiveSpawnerIndicators()
	{
		CSpawner[][] array = new CSpawner[2][] { _spawnersF, _spawnersG };
		CObjective_Scripted currentObjective = GetCurrentObjective();
		CSpawner[] array2 = null;
		if (currentObjective != null)
		{
			array2 = GetCurrentSpawners(currentObjective);
		}
		CSpawner[][] array3 = array;
		foreach (CSpawner[] array4 in array3)
		{
			foreach (CSpawner cSpawner in array4)
			{
				if ((array2 == null || !array2.Contains(cSpawner)) && cSpawner.HasIndicator)
				{
					cSpawner.HideSpawnerProp();
				}
				else if (array2 != null && array2.Contains(cSpawner) && !cSpawner.HasIndicator)
				{
					cSpawner.UpdateWillSpawnNextRoundIndicator(ScenarioScript.PartySize);
				}
			}
		}
	}

	private CObjective_Scripted GetCurrentObjective()
	{
		if (_objectives == null || _objectives.Length == 0)
		{
			return null;
		}
		return _objectives.ToList().Find((CObjective_Scripted o) => !o.IsComplete);
	}

	private CSpawner[] GetCurrentSpawners(CObjective_Scripted objective)
	{
		int num = Array.FindIndex(_objectives, (CObjective_Scripted o) => o == objective);
		CSpawner[] result = null;
		switch (num)
		{
		case 2:
			result = _spawnersF;
			break;
		case 3:
			result = _spawnersG;
			break;
		}
		return result;
	}

	private void UpdateCrainOverrideTarget(CActor crain)
	{
		CObjective_Scripted objective = _objectives.ToList().Find((CObjective_Scripted o) => !o.IsComplete);
		if (objective != null)
		{
			EnemyState enemyState = ScenarioManager.CurrentScenarioState.AllyMonsters[0];
			int num = Array.FindIndex(_objectives, (CObjective_Scripted o) => o == objective);
			enemyState.AIFocusOverride.TargetGUID = debrises[num].PropGuid;
		}
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (ev.Type != ESEType.Phase)
		{
			return false;
		}
		if ((ev as SEventPhase).SubType != CPhase.PhaseType.EndRound)
		{
			return false;
		}
		for (int i = 0; i < _narrativeEventsNames.Length; i++)
		{
			if (message.MessageName == _narrativeEventsNames[i] && _objectives[i].IsComplete)
			{
				return true;
			}
		}
		return false;
	}
}
