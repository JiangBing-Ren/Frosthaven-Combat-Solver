using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH051 : ScenarioScript
{
	[Serializable]
	public class Storage51
	{
		public List<(string, int, int)> BombPlayerIDs = new List<(string, int, int)>();
	}

	private static readonly Point FirstDoorTileIndex = new Point(3, 10);

	private static readonly Point SecondDoorTileIndex = new Point(4, 10);

	private static readonly Point LastDoorTileIndex = new Point(10, 16);

	private static readonly Point InvisibleDoorTileIndex = new Point(11, 20);

	private CObjectDoor _firstDoor;

	private CObjectDoor _secondDoor;

	private CObjectDoor _lastDoor;

	private CObjectActor _invisibleDoor;

	private CScenarioModifier _scenarioAbilityHolder;

	private CScenarioModifier _specialRule3;

	private CObjective_Scripted _winObjective_AllAlliesInLastRoom;

	private CObjective_Scripted _winObjective_NoEnemiesInLastRoom;

	private CObjective_Scripted _winObjective_DestroyDoor;

	private Storage51 serializedProperties;

	private readonly HashSet<Point> _lastRoomTileIndices = new HashSet<Point>
	{
		new Point(11, 17),
		new Point(10, 17),
		new Point(9, 17),
		new Point(8, 17),
		new Point(12, 18),
		new Point(11, 18),
		new Point(10, 18),
		new Point(9, 18),
		new Point(8, 18),
		new Point(12, 19),
		new Point(11, 19),
		new Point(10, 19),
		new Point(9, 19),
		new Point(8, 19),
		new Point(7, 19),
		new Point(12, 20),
		new Point(11, 20),
		new Point(10, 20),
		new Point(9, 20),
		new Point(8, 20),
		new Point(11, 21),
		new Point(10, 21),
		new Point(9, 21),
		new Point(8, 21)
	};

	private CObjectActor InvisibleDoor
	{
		get
		{
			if (_invisibleDoor == null)
			{
				_invisibleDoor = (ScenarioManager.CurrentScenarioState.Objects.SingleOrDefault((ObjectState x) => x.Location.ToPoint() == InvisibleDoorTileIndex)?.Actor ?? ScenarioManager.CurrentScenarioState.Objects.SingleOrDefault((ObjectState x) => x.Location.ToPoint() == LastDoorTileIndex)?.Actor) as CObjectActor;
			}
			return _invisibleDoor;
		}
	}

	protected override string SerializeInternal()
	{
		CAbility ability = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _scenarioAbilityHolder.ScenarioAbilityID)?.ScenarioAbilities[0];
		serializedProperties.BombPlayerIDs.Clear();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			CActiveBonus cActiveBonus = playerActor.FindApplicableActiveBonuses().SingleOrDefault((CActiveBonus x) => x.Ability.ID == ability.ID);
			if (cActiveBonus != null)
			{
				serializedProperties.BombPlayerIDs.Add((playerActor.ActorGuid, cActiveBonus.TrackerIndex, cActiveBonus.Ability.ActiveBonusData.Tracker.Count));
			}
		}
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		if (_lastDoor.Activated)
		{
			InitInvisibleDoor();
		}
		if (string.IsNullOrEmpty(storage))
		{
			serializedProperties = new Storage51();
			return;
		}
		serializedProperties = (Storage51)JsonConvert.DeserializeObject(storage, typeof(Storage51));
		CAbility ability = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _scenarioAbilityHolder.ScenarioAbilityID)?.ScenarioAbilities[0];
		foreach (var entry in serializedProperties.BombPlayerIDs)
		{
			CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.ActorGuid == entry.Item1);
			if (cPlayerActor == null)
			{
				continue;
			}
			CActiveBonus cActiveBonus = cPlayerActor.FindApplicableActiveBonuses().SingleOrDefault((CActiveBonus x) => x.Ability.ID == ability.ID);
			if (cActiveBonus != null)
			{
				cActiveBonus.TrackerIndex = entry.Item2;
				cActiveBonus.Ability.ActiveBonusData.Tracker = new List<int>();
				for (int num = 0; num < entry.Item3; num++)
				{
					cActiveBonus.Ability.ActiveBonusData.Tracker.Add(0);
				}
			}
		}
		if (InvisibleDoor != null && InvisibleDoor.IsDead)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(LastDoorTileIndex);
			node.Walkable = false;
			node.Blocked = true;
		}
	}

	protected override bool Initialize()
	{
		_firstDoor = ScenarioScript.FindDoor(FirstDoorTileIndex.X, FirstDoorTileIndex.Y);
		_secondDoor = ScenarioScript.FindDoor(SecondDoorTileIndex.X, SecondDoorTileIndex.Y);
		_lastDoor = ScenarioScript.FindDoor(LastDoorTileIndex.X, LastDoorTileIndex.Y);
		_firstDoor.TwinDoors = new CObjectDoor[1] { _secondDoor };
		_secondDoor.TwinDoors = new CObjectDoor[1] { _firstDoor };
		_scenarioAbilityHolder = GetScenarioModifier("ScenarioAbilityHolder");
		_specialRule3 = GetScenarioModifier("specialRule3");
		_winObjective_AllAlliesInLastRoom = (CObjective_Scripted)GetObjective("AllAlliesInLastRoom");
		_winObjective_NoEnemiesInLastRoom = (CObjective_Scripted)GetObjective("NoEnemiesInLastRoom");
		_winObjective_DestroyDoor = (CObjective_Scripted)GetObjective("DestroyDoor");
		return true;
	}

	private void InitInvisibleDoor()
	{
		if (InvisibleDoor != null)
		{
			InvisibleDoor.IsSuperTransientBlocker = true;
			InvisibleDoor.IsTransientBlocker = true;
			InvisibleDoor.SetIgnoreActorCollision(enable: true);
			if (!InvisibleDoor.IsDead)
			{
				GameState.TeleportActor(InvisibleDoor, _lastDoor.PropTile);
				CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(InvisibleDoor, active: true);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
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
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 170);
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	protected override void TickRepositionInternal()
	{
		_specialRule3.SetHiddenState(!_firstDoor.DoorIsOpen && !_secondDoor.DoorIsOpen);
		UpdateObjectiveActivation_DestroyDoor();
		if (_lastDoor.Activated)
		{
			if (_lastRoomTileIndices.Contains(_tickActor.ArrayIndex) && InvisibleDoor.ArrayIndex != LastDoorTileIndex)
			{
				InitInvisibleDoor();
			}
			UpdateObjectivesAndCheckForScenarioWinLose();
		}
	}

	protected override void TickDeathInternal()
	{
		CNode node = ScenarioManager.PathFinder.GetNode(LastDoorTileIndex);
		if (_tickActor == InvisibleDoor && !node.Blocked)
		{
			foreach (CActor item in from a in GetAllActorsOutsideLastRoom()
				where !a.IsDead
				select a)
			{
				GameState.KillActor(item, item, CActor.ECauseOfDeath.Undetermined, out var _);
			}
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				spawner.SetActive(active: false);
			}
			node.Walkable = false;
			node.Blocked = true;
		}
		UpdateObjectivesAndCheckForScenarioWinLose();
	}

	private void TickEndTurn()
	{
		UpdateObjectivesAndCheckForScenarioWinLose();
	}

	private void TickStartRound()
	{
		if (_tickActor != ScenarioManager.Scenario.PlayerActors[0] || ScenarioManager.CurrentScenarioState.RoundNumber != 1)
		{
			return;
		}
		CAbility ability = ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == _scenarioAbilityHolder.ScenarioAbilityID)?.ScenarioAbilities[0];
		Dictionary<CActor, int> dictionary = new Dictionary<CActor, int>();
		for (int num = 0; num < 4; num++)
		{
			int index = num % ScenarioManager.Scenario.PlayerActors.Count;
			CPlayerActor key = ScenarioManager.Scenario.PlayerActors[index];
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = 0;
			}
			dictionary[key]++;
		}
		foreach (KeyValuePair<CActor, int> item in dictionary)
		{
			CActor key2 = item.Key;
			int value = item.Value;
			CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
			cAbility.ActiveBonusData.Tracker = new List<int>();
			for (int num2 = 0; num2 < value; num2++)
			{
				cAbility.ActiveBonusData.Tracker.Add(0);
			}
			cAbility.IsScenarioModifierAbility = true;
			cAbility.Start(key2, key2);
			cAbility.ApplyToActor(key2);
		}
	}

	private void TickEndRound()
	{
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		CAbility tickAbility = _tickAbility;
		if (tickAbility != null && (tickAbility.Flags & CAbility.EAbilityFlag.Special1) != 0 && tickAbility.AbilityHasHappened)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "EMPWave3HEX",
				m_ArrayIndex = _tickActor.ArrayIndex
			});
		}
	}

	private void UpdateObjectivesAndCheckForScenarioWinLose()
	{
		UpdateObjective_AllAlliesInLastRoom();
		UpdateObjective_NoEnemiesInLastRoom();
		UpdateObjective_DestroyDoor();
		CheckObjectivesComplete();
	}

	private void UpdateObjective_AllAlliesInLastRoom()
	{
		if (_winObjective_AllAlliesInLastRoom.ScriptedUnableToComplete)
		{
			return;
		}
		if (GetAllActorsOutsideLastRoom().Any((CActor a) => a.Type == CActor.EType.Player && a.IsDead))
		{
			_winObjective_AllAlliesInLastRoom.ScriptedIsComplete = false;
			_winObjective_AllAlliesInLastRoom.ScriptedUnableToComplete = true;
		}
		else if (_lastDoor.Activated)
		{
			List<CActor> list = (from a in GetAllActorsInsideLastRoom()
				where a.Type == CActor.EType.Player
				select a).ToList();
			List<CPlayerActor> allPlayers = ScenarioManager.Scenario.AllPlayers;
			bool scriptedIsComplete = list.Count == allPlayers.Count;
			_winObjective_AllAlliesInLastRoom.ScriptedIsComplete = scriptedIsComplete;
		}
	}

	private void UpdateObjective_NoEnemiesInLastRoom()
	{
		if (_winObjective_NoEnemiesInLastRoom.ScriptedUnableToComplete)
		{
			return;
		}
		if (GetAllActorsOutsideLastRoom().Any((CActor a) => a.Type == CActor.EType.Player && a.IsDead))
		{
			_winObjective_NoEnemiesInLastRoom.ScriptedIsComplete = false;
			_winObjective_NoEnemiesInLastRoom.ScriptedUnableToComplete = true;
		}
		else if (_lastDoor.Activated)
		{
			bool flag = GetAllActorsInsideLastRoom().Any((CActor a) => !a.IsDead && (a.Type == CActor.EType.Enemy || a.Type == CActor.EType.Enemy2));
			_winObjective_NoEnemiesInLastRoom.ScriptedIsComplete = !flag;
		}
	}

	private void UpdateObjective_DestroyDoor()
	{
		UpdateObjectiveActivation_DestroyDoor();
		if (_winObjective_DestroyDoor.ScriptedUnableToComplete)
		{
			return;
		}
		if (GetAllActorsOutsideLastRoom().Any((CActor a) => a.Type == CActor.EType.Player && a.IsDead))
		{
			_winObjective_DestroyDoor.ScriptedIsComplete = false;
			_winObjective_DestroyDoor.ScriptedUnableToComplete = true;
		}
		else if (InvisibleDoor != null && InvisibleDoor.IsDeadForObjectives)
		{
			if (GetAllActorsOutsideLastRoom().Any((CActor a) => a.Type == CActor.EType.Player && !a.IsDead))
			{
				_winObjective_DestroyDoor.ScriptedIsComplete = false;
				_winObjective_DestroyDoor.ScriptedUnableToComplete = true;
			}
			else
			{
				_winObjective_DestroyDoor.ScriptedIsComplete = true;
			}
		}
	}

	private void UpdateObjectiveActivation_DestroyDoor()
	{
		if (_secondDoor.DoorIsOpen && _lastDoor.Activated)
		{
			_winObjective_DestroyDoor.SetActivation(active: true);
		}
	}

	private IEnumerable<CActor> GetAllActorsInsideLastRoom()
	{
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			if (_lastRoomTileIndices.Contains(allActor.ArrayIndex))
			{
				yield return allActor;
			}
		}
	}

	private IEnumerable<CActor> GetAllActorsOutsideLastRoom()
	{
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			if (!_lastRoomTileIndices.Contains(allActor.ArrayIndex))
			{
				yield return allActor;
			}
		}
	}
}
