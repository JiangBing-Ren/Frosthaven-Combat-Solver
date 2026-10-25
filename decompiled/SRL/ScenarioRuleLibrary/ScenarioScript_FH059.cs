using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH059 : ScenarioScript
{
	public List<CObjectObstacle> consoles = new List<CObjectObstacle>();

	private EnemyState _orphan;

	private CObjective _openDoorsObjective;

	private CObjective _finalObjective;

	private List<CSpawner> randomizedSpawners = new List<CSpawner>();

	protected override bool Initialize()
	{
		randomizedSpawners = ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner x) => x.SpawnerData.SpawnStartRound == 5 || x.SpawnerData.SpawnStartRound == 7 || x.SpawnerData.SpawnStartRound == 11).ToList();
		if (!GetObjective("OpenDoorsObjective", out _openDoorsObjective))
		{
			return false;
		}
		if (!GetObjective("FinalObjective", out _finalObjective))
		{
			return false;
		}
		foreach (CObjectProp prop in ScenarioScript._css.Props)
		{
			if (prop is CObjectObstacle { PropType: EPropType.TwoHexObstacle } cObjectObstacle)
			{
				consoles.Add(cObjectObstacle);
			}
		}
		consoles.Sort(GameState.ComparePropsByGUID);
		if (consoles.Count < 3)
		{
			Error("Needs three two-tile obstacles.", null, 46);
			return false;
		}
		_orphan = ScenarioScript._css.AllyMonsters.Find((EnemyState m) => m.ClassID.Contains("TheOrphan"));
		if (_orphan == null)
		{
			Error("Needs orphan ally.", null, 52);
			return false;
		}
		TryLockDoors();
		return true;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[ffa000]Deserialize " + s + "[-]", null, 62);
		ImmuneConsoles();
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		if (_finalObjective.IsActive && message.MessageName.EndsWith("_1"))
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 68);
			return true;
		}
		return false;
	}

	protected override void TickDeathInternal()
	{
		TryOpenDoors();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickPhaseStartRound();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 96);
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	private bool IsNextToLiveConsole(CActor actor, out int console)
	{
		console = 0;
		if (actor != _orphan.Actor)
		{
			return false;
		}
		int x = actor.ArrayIndex.X;
		int y = actor.ArrayIndex.Y;
		for (int i = 0; i < consoles.Count; i++)
		{
			CObjectObstacle cObjectObstacle = consoles[i];
			if (cObjectObstacle.RuntimeAttachedActor == null || cObjectObstacle.RuntimeAttachedActor.Health <= 0)
			{
				continue;
			}
			foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
			{
				int x2 = pathingBlocker.X;
				int y2 = pathingBlocker.Y;
				if (ScenarioManager.GetTileDistance(x, y, x2, y2) == 1)
				{
					console = i;
					return true;
				}
				if (x2 == 12 && y2 == 7 && ScenarioManager.GetTileDistance(x, y, 12, 6) == 1)
				{
					console = i;
					return true;
				}
				if (x2 == 3 && y2 == 12 && ScenarioManager.GetTileDistance(x, y, 2, 11) == 1)
				{
					console = i;
					return true;
				}
				if (x2 == 5 && y2 == 4 && ScenarioManager.GetTileDistance(x, y, 4, 3) == 1)
				{
					console = i;
					return true;
				}
			}
		}
		console = -1;
		return false;
	}

	public override Point GetActorIteractionTile(CActor actor)
	{
		if (!IsNextToLiveConsole(actor, out var console))
		{
			return default;
		}
		return consoles[console].PropTile.m_ArrayIndex;
	}

	public override bool IsActorInteracting(CActor actor)
	{
		int console;
		return IsNextToLiveConsole(actor, out console);
	}

	private void TickPhaseEndTurn()
	{
		if (_orphan.Actor != null && _tickActor == _orphan.Actor)
		{
			if (IsNextToLiveConsole(_tickActor, out var console))
			{
				CEnemyActor runtimeAttachedActor = consoles[console].RuntimeAttachedActor;
				runtimeAttachedActor.IsImmuneToDamage = false;
				DealScenarioDamage(runtimeAttachedActor, 1);
				Trace($"[fA0ff0]Deal damage to console {console}, console health: {runtimeAttachedActor.Health}[-]", null, 177);
				runtimeAttachedActor.IsImmuneToDamage = true;
			}
			TryOpenDoors();
		}
	}

	private void TickPhaseStartRound()
	{
		if (!OncePerRound())
		{
			return;
		}
		Trace($"Round Number: {ScenarioScript._css.RoundNumber}", null, 188);
		if (ScenarioScript._css.RoundNumber == 1)
		{
			Trace("Deactivating the final objective.", null, 190);
			_finalObjective.SetActivation(active: false);
		}
		if (ScenarioScript.RoundNumber != 4 && ScenarioScript.RoundNumber != 6 && ScenarioScript.RoundNumber != 10)
		{
			return;
		}
		List<CTile> list = ScenarioManager.StartingTiles.ToList();
		list.Shuffle(ScenarioScript._css.ScenarioRNG);
		int num = 0;
		foreach (CSpawner randomizedSpawner in randomizedSpawners)
		{
			CTile cTile = list[num % list.Count];
			randomizedSpawner.SetLocation(new TileIndex(cTile.m_ArrayIndex));
			num++;
		}
	}

	private int GetConsolesHealth()
	{
		ImmuneConsoles();
		int num = 0;
		foreach (CObjectObstacle console in consoles)
		{
			int val = ((console.RuntimeAttachedActor != null) ? console.RuntimeAttachedActor.Health : 0);
			val = Math.Max(0, val);
			num += val;
		}
		return num;
	}

	private void TryLockDoors()
	{
		int consolesHealth = GetConsolesHealth();
		foreach (CObjectProp doorProp in ScenarioScript._css.DoorProps)
		{
			if (doorProp is CObjectDoor { Activated: false } cObjectDoor)
			{
				cObjectDoor.SetHasExtraLock(consolesHealth > 0);
			}
		}
	}

	private void TryOpenDoors()
	{
		if (GetConsolesHealth() > 0)
		{
			return;
		}
		bool flag = false;
		foreach (CObjectProp doorProp in ScenarioScript._css.DoorProps)
		{
			if (doorProp is CObjectDoor { Activated: false } cObjectDoor)
			{
				Trace("Open door " + cObjectDoor.PrefabName, null, 242);
				cObjectDoor.ForceActivate(null);
				flag = true;
			}
		}
		if (flag)
		{
			_finalObjective.SetActivation(active: true);
			_openDoorsObjective.SetActivation(active: false);
		}
	}

	private void ImmuneConsoles()
	{
		foreach (CObjectObstacle console in consoles)
		{
			if (console.RuntimeAttachedActor != null && (!console.RuntimeAttachedActor.IsImmuneToDamage || !console.RuntimeAttachedActor.ForcedUntargetable))
			{
				console.RuntimeAttachedActor.ForcedUntargetable = true;
				console.RuntimeAttachedActor.IsImmuneToDamage = true;
				Trace("Setting console " + console.PropGuid + " immune to damage...", null, 259);
			}
		}
	}
}
