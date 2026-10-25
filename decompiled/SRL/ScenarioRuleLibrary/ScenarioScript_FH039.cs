using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH039 : ScenarioScript
{
	private string frozenCorpseClassID = "FrozenCorpseID";

	private Dictionary<Point, Point> algoxToDoorConnections = new Dictionary<Point, Point>
	{
		{
			new Point(3, 4),
			new Point(3, 6)
		},
		{
			new Point(4, 17),
			new Point(5, 14)
		},
		{
			new Point(13, 17),
			new Point(12, 14)
		},
		{
			new Point(13, 5),
			new Point(12, 6)
		}
	};

	public Dictionary<Point, int> doorTimers = new Dictionary<Point, int>
	{
		{
			new Point(3, 6),
			2
		},
		{
			new Point(5, 14),
			5
		},
		{
			new Point(12, 14),
			8
		},
		{
			new Point(12, 6),
			11
		}
	};

	private bool loweredTimersThisRound;

	private CObjective_Scripted _doorTimeOutFailObjective;

	protected override string SerializeInternal()
	{
		string text = JsonConvert.SerializeObject(doorTimers.Values.ToList());
		Trace("[ffa000]Serialize: " + text + "[-]", null, 33);
		return text;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 39);
		if (!string.IsNullOrEmpty(s))
		{
			int idx = 0;
			List<int> doorTimersList = JsonConvert.DeserializeObject<List<int>>(s);
			doorTimers = doorTimers.ToDictionary((KeyValuePair<Point, int> p) => p.Key, (KeyValuePair<Point, int> p) => doorTimersList[idx++]);
		}
		CheckObjectivesComplete();
		RefreshDoorCounters();
	}

	protected override bool Initialize()
	{
		_doorTimeOutFailObjective = (CObjective_Scripted)GetObjective("DoorTimeOutFailObjective");
		return base.Initialize();
	}

	private void AllowAlgoxDowned()
	{
		if (_tickActor is CEnemyActor { Type: CActor.EType.Ally, PlayerSummoner: null })
		{
			_tickActor.CanBeDowned = true;
		}
	}

	private void ReduceAlgoxHP()
	{
		if (_tickActor is CEnemyActor { Type: CActor.EType.Ally, PlayerSummoner: null })
		{
			int num = (doorTimers[algoxToDoorConnections[_tickActor.ArrayIndex]] + ScenarioScript._css.Level) * 2;
			if (_tickActor.Health > num)
			{
				_tickActor.Health = num;
				CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(_tickActor, _tickActor, _tickActor.Health);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	private void LowerDoorTimers()
	{
		if (loweredTimersThisRound)
		{
			return;
		}
		loweredTimersThisRound = true;
		foreach (CObjectProp doorProp in ScenarioScript._css.DoorProps)
		{
			if ((doorProp as CObjectDoor).DoorIsClosed)
			{
				doorTimers[new Point(doorProp.ArrayIndex)]--;
				if (doorTimers[new Point(doorProp.ArrayIndex)] <= 0)
				{
					_doorTimeOutFailObjective.ScriptedIsComplete = true;
				}
			}
		}
		CheckObjectivesComplete();
		RefreshDoorCounters();
	}

	private void TurnDownedAlgoxToFrozenCorpses()
	{
		CActor tickActor = _tickActor;
		if (!tickActor.IsDead && tickActor.IsDowned && MonsterClassManager.Find(frozenCorpseClassID).AvailableIDs.Any())
		{
			CSpawner.SpawnUnitAtTile(ScenarioScript._css.Players.Count, CActor.EType.Enemy, frozenCorpseClassID, _tickActor.ArrayIndex.X, _tickActor.ArrayIndex.Y);
			tickActor.CanBeDowned = false;
			GameState.KillActor(tickActor, tickActor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _, null, actorWasAsleep: false, null, instantlyDeletePrefab: true);
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario)
		{
			AllowAlgoxDowned();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
		{
			AllowAlgoxDowned();
			ReduceAlgoxHP();
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound)
		{
			loweredTimersThisRound = false;
		}
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound)
		{
			TurnDownedAlgoxToFrozenCorpses();
			LowerDoorTimers();
		}
	}

	public override void ApplyReward(bool won)
	{
		base.ApplyReward(won);
		if (!won)
		{
			return;
		}
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			for (int i = 0; i < 2; i++)
			{
				allPlayer.GainLoot(endOfScenario: true, incLootTokenCount: false, i == 0);
			}
			if (!allPlayer.IsDead)
			{
				allPlayer.ShowLastLootReward("", "MoneyToken", null, focusCamera: true);
			}
		}
	}

	private void RefreshDoorCounters()
	{
		foreach (KeyValuePair<Point, int> doorTimer in doorTimers)
		{
			CTile tile = ScenarioManager.GetTile(doorTimer.Key);
			CObjectDoor cObjectDoor = tile.FindDoor();
			CObjectProp cObjectProp = tile.m_Props.FirstOrDefault((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.VFXProp && x.PrefabName == EVFXPropType.NumberCounter.ToString());
			if (cObjectDoor.DoorIsOpen)
			{
				cObjectProp?.DestroyProp();
				continue;
			}
			if (cObjectProp == null)
			{
				cObjectProp = SpawnVFXProp(tile, EVFXPropType.NumberCounter.ToString());
			}
			CChangePropVisualState_MessageData message = new CChangePropVisualState_MessageData
			{
				m_prop = cObjectProp,
				m_stateName = "State_" + doorTimer.Value
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		RefreshDoorCounters();
	}
}
