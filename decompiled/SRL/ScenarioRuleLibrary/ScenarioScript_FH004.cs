using System;
using System.Collections.Generic;
using System.Linq;
using Libs.GameConsole;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH004 : ScenarioScript
{
	[ConsoleVariable(Alias = "fh004_debug_max_damage")]
	private static int DebugMaxDamage_cvar;

	[ConsoleVariable(Alias = "fh004_debug_prevent_unlock")]
	private static bool DebugPreventUnlock_cvar;

	private CObjective_Scripted[] _damageObjectives = new CObjective_Scripted[2];

	private EnemyState[] _bossStates = new EnemyState[2];

	private CObjective_Scripted _killObjective;

	private CObjectDoor _scriptedDoor;

	private CMap _room1;

	private CMap _room3;

	private CObjective_Scripted _openDoorObjective;

	protected override bool Initialize()
	{
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState.WinObjectives == null || currentScenarioState.WinObjectives.Count == 0)
		{
			Error("No Win Objectives specified in the Level Editor. Create two 'Scripted' Win Objective in the Level Editor.", null, 65);
			return false;
		}
		_openDoorObjective = GetObjective("OpenDoor") as CObjective_Scripted;
		_openDoorObjective.HideOnComplete = true;
		_room1 = ScenarioScript.FindRoom("Room_1");
		_room3 = ScenarioScript.FindRoom("Room_3");
		_scriptedDoor = null;
		string roomName = ScenarioManager.StartingTiles[0].m_HexMap.RoomName;
		foreach (CObjectDoor item in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>().ToList())
		{
			if (item.PropTile.m_HexMap.RoomName == roomName || item.PropTile.m_Hex2Map?.RoomName == roomName)
			{
				Trace("Found the scripted door.", null, 79);
				_scriptedDoor = item;
			}
		}
		if (_scriptedDoor == null)
		{
			Error("Place a door between rooms 1 and 2", null, 85);
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			_damageObjectives[i] = null;
		}
		foreach (CObjective winObjective in currentScenarioState.WinObjectives)
		{
			if (winObjective is CObjective_Scripted cObjective_Scripted)
			{
				if (_damageObjectives[0] == null)
				{
					_damageObjectives[0] = cObjective_Scripted;
				}
				else if (_damageObjectives[1] == null)
				{
					_damageObjectives[1] = cObjective_Scripted;
				}
				else if (_killObjective == null)
				{
					_killObjective = cObjective_Scripted;
				}
			}
		}
		CObjective_Scripted[] damageObjectives = _damageObjectives;
		for (int j = 0; j < damageObjectives.Length; j++)
		{
			if (damageObjectives[j] == null)
			{
				Error("Null damage objective. Create three 'Scripted' Win Objectives in the Level Editor.", null, 107);
				return false;
			}
		}
		if (_killObjective == null)
		{
			Error("Null kill boss objective. Create three 'Scripted' Win Objectives in the Level Editor.", null, 113);
			return false;
		}
		foreach (EnemyState monster in currentScenarioState.Monsters)
		{
			tryAssign(monster);
		}
		foreach (EnemyState enemy2Monster in currentScenarioState.Enemy2Monsters)
		{
			tryAssign(enemy2Monster);
		}
		EnemyState[] bossStates = _bossStates;
		for (int j = 0; j < bossStates.Length; j++)
		{
			if (bossStates[j] == null)
			{
				Error("No Boss States. Create the two FS-04 Bosses in the Level Editor.", null, 139);
				return false;
			}
		}
		InitKillObjectives();
		return true;
		void tryAssign(EnemyState enemyState)
		{
			if (enemyState.IsBoss)
			{
				if (_bossStates[0] == null && enemyState.ClassID.ToLowerInvariant().Contains("dancer"))
				{
					_bossStates[0] = enemyState;
					Trace("Assigned " + enemyState.ClassID + " at 0", null, 121);
				}
				else if (_bossStates[1] == null && enemyState.ClassID.ToLowerInvariant().Contains("fist"))
				{
					_bossStates[1] = enemyState;
					Trace("Assigned " + enemyState.ClassID + " at 1", null, 124);
				}
			}
		}
	}

	private void UpdateOpenDoorObjective()
	{
		if (_room1 != null && _room3 != null && _openDoorObjective != null)
		{
			List<CActor> list = (from x in ScenarioManager.CurrentScenarioState.AllEnemyStates.Where((EnemyState x) => (x.Type == CActor.EType.Enemy || x.Type == CActor.EType.Enemy2) && (x.StartingMapGuid == _room1.MapGuid || x.StartingMapGuid == _room3.MapGuid)).ToList()
				select x.Actor into x
				where x != null
				select x).ToList();
			_openDoorObjective.Total = list.Count;
			_openDoorObjective.Current = list.Count((CActor x) => x.IsDead);
			_openDoorObjective.ScriptedIsComplete = _openDoorObjective.Current == _openDoorObjective.Total;
		}
	}

	private void InitKillObjectives()
	{
		if (ScenarioManager.CurrentScenarioState.IsFirstLoad || !_killObjective.IsInitialized())
		{
			Trace("Initialize objectives... ", null, 162);
			int i;
			for (i = 0; i < 2; i++)
			{
				CActor actor = _bossStates[i].Actor;
				if (actor == null)
				{
					return;
				}
				MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == _bossStates[i].ClassID);
				_damageObjectives[i].Total = ((DebugMaxDamage_cvar == 0) ? actor.MaxHealthHalfRoundedUp : DebugMaxDamage_cvar);
				_damageObjectives[i].Current = 0;
				_damageObjectives[i].ScriptedIsComplete = false;
				_damageObjectives[i].ScriptedLocKey = ((i == 0) ? "GUI_OBJECTIVE_FH004_DAMAGE_DANCER" : "GUI_OBJECTIVE_FH004_DAMAGE_FIST");
				_damageObjectives[i].SetActivation(active: true);
			}
			_killObjective.Total = 1;
			_killObjective.Current = 0;
			_killObjective.ScriptedIsComplete = false;
			_killObjective.ScriptedLocKey = "GUI_OBJECTIVE_FH004_KILL_A_BOSS";
			_killObjective.ScriptedEnoughToWinAlone = true;
			_killObjective.SetActivation(active: true);
			CheckObjectivesComplete();
		}
		else
		{
			Trace("Objectives already initialized.", null, 190);
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (!IsBoss(out var boss))
		{
			return;
		}
		if (_damageObjectives[boss].ScriptedIsComplete)
		{
			Trace("Objective is complete, ignore taken damage...", null, 201);
			return;
		}
		if (_tickAttacker == null)
		{
			Trace("Boss is taking damage, but no attacker.", null, 206);
			return;
		}
		CObjective_Scripted cObjective_Scripted = _damageObjectives[boss];
		EnemyState es = _bossStates[boss];
		if (_tickAttacker.Type == CActor.EType.Player || _tickAttacker.Type == CActor.EType.HeroSummon)
		{
			Trace("Attacking the boss...", null, 214);
			CMonsterClass cMonsterClass = MonsterClassManager.Classes.SingleOrDefault((CMonsterClass x) => x.ID == es.ClassID);
			cObjective_Scripted.Total = ((DebugMaxDamage_cvar == 0) ? ((int)Math.Ceiling((float)cMonsterClass.Health() / 2f)) : DebugMaxDamage_cvar);
			cObjective_Scripted.Current = Math.Min(cObjective_Scripted.Total, cObjective_Scripted.Current + actualDamage);
			cObjective_Scripted.ScriptedIsComplete = cObjective_Scripted.Current == cObjective_Scripted.Total;
		}
		CActor actor = es.Actor;
		int num = Math.Max(0, actor.Health);
		if (cObjective_Scripted.Current + num < cObjective_Scripted.Total)
		{
			cObjective_Scripted.ScriptedUnableToComplete = true;
			if (_damageObjectives[1 - boss].ScriptedUnableToComplete)
			{
				_killObjective.ScriptedUnableToComplete = true;
			}
		}
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	protected override void TickDeathInternal()
	{
		if (IsBoss(out var boss))
		{
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				if (!_bossStates[i].Enemy.IsDeadForObjectives)
				{
					Trace(i + ": alive for objectives", null, 242);
					num++;
				}
			}
			Trace($"A boss is dying... num bosses alive: {num}", null, 247);
			if (_damageObjectives[boss].IsComplete && num > 0)
			{
				Trace("The damage condition was met, win the scenario...", null, 250);
				_killObjective.Current = 1;
				_killObjective.ScriptedIsComplete = true;
			}
			else
			{
				Trace("The damage condition was not met, the scenario failed...", null, 254);
				for (int j = 0; j < 2; j++)
				{
					_damageObjectives[j].Current = (_damageObjectives[j].Total = 0);
					_damageObjectives[j].ScriptedIsComplete = false;
					_damageObjectives[j].ScriptedUnableToComplete = true;
				}
				_killObjective.Current = 1;
				_killObjective.ScriptedIsComplete = false;
				_killObjective.ScriptedUnableToComplete = true;
			}
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			Trace("conclusion index: " + GetConclusionIndexInternal(), null, 265);
		}
		else if (_openDoorObjective != null)
		{
			UpdateOpenDoorObjective();
			ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			if (!DebugPreventUnlock_cvar && _scriptedDoor.DoorExtraLockIsLocked && _openDoorObjective.IsComplete)
			{
				_scriptedDoor.UnlockLockedDoorWithoutOpening();
			}
		}
	}

	protected override int GetConclusionIndexInternal()
	{
		for (int i = 0; i < 2; i++)
		{
			CActor actor = _bossStates[i].Actor;
			if ((actor != null && actor.IsDead) || _bossStates[i].IsDead)
			{
				return i;
			}
		}
		return -1;
	}

	protected override void DeserializeInternal(string s)
	{
		UpdateOpenDoorObjective();
		CheckObjectivesComplete();
		if (!DebugPreventUnlock_cvar && _scriptedDoor.DoorExtraLockIsLocked && _openDoorObjective.IsComplete)
		{
			_scriptedDoor.UnlockLockedDoorWithoutOpening();
		}
	}

	private bool IsBoss(out int boss)
	{
		for (boss = 0; boss < 2; boss++)
		{
			if (_bossStates[boss].Enemy == _tickActor)
			{
				return true;
			}
		}
		return false;
	}

	protected int GetAliveEnemies()
	{
		int num = 0;
		foreach (EnemyState monster in ScenarioManager.CurrentScenarioState.Monsters)
		{
			num += (isAlive(monster) ? 1 : 0);
		}
		foreach (EnemyState enemy2Monster in ScenarioManager.CurrentScenarioState.Enemy2Monsters)
		{
			num += (isAlive(enemy2Monster) ? 1 : 0);
		}
		Trace($"Num alive: {num}.", null, 328);
		return num;
		static bool isAlive(EnemyState enemyState)
		{
			if (!enemyState.IsDead && enemyState.Actor != null)
			{
				return !enemyState.Actor.IsDead;
			}
			return false;
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		if (_scenarioModifier.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed)
		{
			TickPhaseRoomRevealed();
		}
	}

	private void TickPhaseRoomRevealed()
	{
		InitKillObjectives();
	}

	public override string GetOverrideCharacterModelPrefabName(CActor actor)
	{
		string prefabName = actor.GetPrefabName();
		if (prefabName == "AlgoxScout")
		{
			return "AlgoxScoutSnowspeaker";
		}
		if (prefabName == "AlgoxScout Elite")
		{
			return "AlgoxScoutSnowspeaker Elite";
		}
		return null;
	}
}
