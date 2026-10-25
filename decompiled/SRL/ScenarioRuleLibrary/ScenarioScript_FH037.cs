using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH037 : ScenarioScript
{
	private CSpawner _spawnerA;

	private CSpawner _spawnerB;

	private CSpawner _spawnerC;

	private CObjectDoor _door1;

	private CObjectDoor _door2;

	private CObjectDoor _door3;

	private CScenarioModifier _specialRule1;

	private CScenarioModifier _specialRule2;

	private CScenarioModifier _specialRule3;

	protected override bool Initialize()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_spawnerA = spawners[0];
		_spawnerB = spawners[1];
		_spawnerC = spawners[2];
		_door1 = GetProp(25, 2, "ThinDoor") as CObjectDoor;
		_door2 = GetProp(17, 2, "ThinDoor") as CObjectDoor;
		_door3 = GetProp(9, 2, "ThinDoor") as CObjectDoor;
		if (new CObjectDoor[3] { _door1, _door2, _door3 }.Contains(null))
		{
			Error("Couldn't find all doors on the map.", null, 26);
			return false;
		}
		_specialRule1 = GetScenarioModifier("specialRule1");
		_specialRule2 = GetScenarioModifier("specialRule2");
		_specialRule3 = GetScenarioModifier("specialRule3");
		UpdateSpecialRulesVisibility();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 51);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private void TickPhaseEndTurn()
	{
		TryEscapeMap();
	}

	protected override void TickRepositionInternal()
	{
		UpdateSpecialRulesVisibility();
		UpdateSpawnersIndicators();
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateSpecialRulesVisibility();
		UpdateSpawnersIndicators();
	}

	private void UpdateSpawnersIndicators()
	{
		CSpawner currentSpawner = GetCurrentSpawner();
		CSpawner[] array = new CSpawner[3] { _spawnerA, _spawnerB, _spawnerC };
		foreach (CSpawner obj in array)
		{
			obj.SetActive(obj == currentSpawner, forceDontSpawn: true);
		}
	}

	private CSpawner GetCurrentSpawner()
	{
		CSpawner result = null;
		if (_door3.DoorIsOpen)
		{
			result = _spawnerC;
		}
		else if (_door2.DoorIsOpen)
		{
			result = _spawnerB;
		}
		else if (_door1.DoorIsOpen)
		{
			result = _spawnerA;
		}
		return result;
	}

	private void UpdateSpecialRulesVisibility()
	{
		_specialRule1.SetHiddenState(!_door1.DoorIsOpen || _door2.DoorIsOpen);
		_specialRule2.SetHiddenState(!_door2.DoorIsOpen || _door3.DoorIsOpen);
		_specialRule3.SetHiddenState(!_door3.DoorIsOpen);
	}
}
