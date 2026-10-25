using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH044 : ScenarioScript
{
	private static int MONSTERS_TRAP_DAMAGE_DISTANCE = 2;

	private static string SECOND_ROOM_NAME = "Room_5";

	private CSpawner _normalSpawner;

	private CSpawner _eliteSpawner;

	private CObjectProp[] _powerTowers;

	private bool[] _isPowerTowerDestroyed;

	private CScenarioModifier _specialRule1;

	private CScenarioModifier _specialRule2;

	private CObjectDoor _door1;

	private int TrapDamage => ScenarioManager.TrapDamage;

	private bool IsInSecondRoom(CActor actor)
	{
		return actor.Tile.m_HexMap.RoomName == SECOND_ROOM_NAME;
	}

	protected override bool Initialize()
	{
		List<CSpawner> spawners = ScenarioManager.CurrentScenarioState.Spawners;
		_normalSpawner = spawners[0];
		_eliteSpawner = spawners[1];
		_specialRule1 = GetScenarioModifier("specialRule1");
		_specialRule2 = GetScenarioModifier("specialRule2");
		_powerTowers = new CObjectProp[6]
		{
			GetProp(5, 17, "PowerTower", checkDestroyed: true),
			GetProp(6, 12, "PowerTower", checkDestroyed: true),
			GetProp(6, 8, "PowerTower", checkDestroyed: true),
			GetProp(10, 12, "PowerTower", checkDestroyed: true),
			GetProp(10, 8, "PowerTower", checkDestroyed: true),
			GetProp(8, 2, "PowerTower", checkDestroyed: true)
		};
		_isPowerTowerDestroyed = new bool[_powerTowers.Length];
		for (int i = 0; i < _powerTowers.Length; i++)
		{
			_isPowerTowerDestroyed[i] = _powerTowers[i] == null || _powerTowers[i].IsDestroyed;
		}
		_door1 = GetProp(6, 14, "ThickDoor") as CObjectDoor;
		UpdateSpecialRulesVisibility();
		_specialRule1.CustomLocKeyArgs = new List<object> { TrapDamage };
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
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 70);
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
		if (_tickActor is CEnemyActor cEnemyActor && cEnemyActor.MonsterClass.ID.StartsWith("AncientArtillery") && IsInSecondRoom(cEnemyActor))
		{
			CSpawner cSpawner = ((cEnemyActor.MonsterClass.ID == "AncientArtilleryID") ? _normalSpawner : _eliteSpawner);
			cSpawner.SetLocation(new TileIndex(cEnemyActor.ArrayIndex));
			cSpawner.SpawnUnit(ScenarioScript.PartySize, treatAsSummon: false, initial: false, startRound: false, forceUseAnyEntry: false, addIntoInitiativeOrder: false, spawnFarthestOnAdjacent: true);
		}
	}

	protected override void TickRepositionInternal()
	{
		UpdateSpecialRulesVisibility();
	}

	protected override void TickAbilityEndedInternal()
	{
		for (int i = 0; i < _powerTowers.Length; i++)
		{
			CObjectProp cObjectProp = _powerTowers[i];
			if (cObjectProp == null || !cObjectProp.IsDestroyed || _isPowerTowerDestroyed[i])
			{
				continue;
			}
			TileIndex arrayIndex = cObjectProp.ArrayIndex;
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "EMPWave3HEX",
				m_ArrayIndex = cObjectProp.ArrayIndex.ToPoint()
			});
			foreach (CEnemyActor allMonster in ScenarioManager.Scenario.AllMonsters)
			{
				if (ScenarioManager.GetTileDistance(arrayIndex.X, arrayIndex.Y, allMonster.ArrayIndex.X, allMonster.ArrayIndex.Y) <= MONSTERS_TRAP_DAMAGE_DISTANCE)
				{
					DealScenarioDamage(allMonster, TrapDamage, isTrapDamage: true, isTerrainDamage: false, cObjectProp.PropHealthDetails.CustomLocKey, null, null, null, showInflictMessage: false, checkIfPlayerCanAvoidDamage: false);
				}
			}
			_isPowerTowerDestroyed[i] = true;
		}
		UpdateSpecialRulesVisibility();
	}

	protected override void TickDeathInternal()
	{
		UpdateSpecialRulesVisibility();
	}

	private void UpdateSpecialRulesVisibility()
	{
		_specialRule1.SetHiddenState(!_isPowerTowerDestroyed.Contains(value: false));
		if (_door1.DoorIsOpen)
		{
			List<CEnemyActor> source = ScenarioManager.Scenario.AllMonsters.FindAll((CEnemyActor m) => IsInSecondRoom(m));
			_specialRule2.SetHiddenState(!source.Any((CEnemyActor m) => m.MonsterClass.ID.StartsWith("AncientArtillery") && !m.IsDead));
		}
		else
		{
			_specialRule2.SetHiddenState(shouldSetHidden: true);
		}
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("damage_from_explosion", TrapDamage.ToString());
	}
}
