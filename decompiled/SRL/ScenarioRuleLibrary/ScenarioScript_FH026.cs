using System.Collections.Generic;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH026 : ScenarioScript
{
	private static string ActAsRuinedMachine_cvar = "guard";

	private static bool SkipBlockerCreate_cvar;

	private static bool SkipBlockerAddTiles_cvar;

	private static bool TestAdjacentBlockers_cvar;

	private static int ShowBlockerDiagrams_cvar;

	[StateDump(Skip = true)]
	private List<Point> _blockers = new List<Point>();

	private CObjectProp _pressurePlate;

	[StateDump(Skip = true)]
	private CAIFocusOverrideDetails _focusOverride;

	private CObjective_Scripted _actorStepOnPositionObjective;

	protected override bool Initialize()
	{
		_focusOverride = new CAIFocusOverrideDetails();
		_focusOverride.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
		_focusOverride.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Prop;
		_actorStepOnPositionObjective = GetObjective("actorStepOnPosition") as CObjective_Scripted;
		_actorStepOnPositionObjective.ScriptedIsComplete = false;
		_pressurePlate = null;
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (prop.PropType == EPropType.PressurePlate)
			{
				_pressurePlate = prop;
				break;
			}
		}
		if (_pressurePlate == null)
		{
			Error("No pressure plate setup in Level Editor", null, 47);
			return false;
		}
		_focusOverride.TargetGUID = _pressurePlate.PropGuid;
		return true;
	}

	protected override void TickPhaseInternal()
	{
		if (_pressurePlate == null)
		{
			Error("No pressure plate setup in Level Editor", null, 57);
			return;
		}
		Trace("TickPhase " + _tickActor.Class.ID + " " + _tickActor.ActorGuid, null, 61);
		ActorState ruinedMachineState = null;
		foreach (EnemyState monster in ScenarioManager.CurrentScenarioState.Monsters)
		{
			if (monster.ActorGuid == _tickActor.ActorGuid)
			{
				ruinedMachineState = monster;
				break;
			}
		}
		TryTickRuinedMachine(_tickActor, ruinedMachineState);
		Trace("========= Phase end " + _tickActor.Class.ID + " ==========\n", null, 73);
	}

	protected override bool AllowTriggerMessageInternal(SEvent ev, CLevelMessage message)
	{
		int roundNumber = ScenarioManager.CurrentScenarioState.RoundNumber;
		if (message.MessageName.EndsWith($"_Round{roundNumber}"))
		{
			Trace($"Allow message {message.MessageName} {message.TitleKey} {ev.Type}...", null, 79);
			return true;
		}
		return false;
	}

	protected override string SerializeInternal()
	{
		CNode node = ScenarioManager.PathFinder.GetNode(_pressurePlate.ArrayIndex);
		string text = JsonConvert.SerializeObject(node.Blocked);
		string text2 = JsonConvert.SerializeObject(node.Walkable);
		string text3 = JsonConvert.SerializeObject(new string[2] { text, text2 });
		Trace("[FH026]Serialize: " + text3 + "[-]", null, 90);
		return text3;
	}

	protected override void DeserializeInternal(string s)
	{
		Trace("[FH026]Deserialize " + s + "[-]", null, 96);
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = JsonConvert.DeserializeObject<string[]>(s);
			CNode node = ScenarioManager.PathFinder.GetNode(_pressurePlate.ArrayIndex);
			node.Blocked = JsonConvert.DeserializeObject<bool>(array[0]);
			node.Walkable = JsonConvert.DeserializeObject<bool>(array[1]);
		}
	}

	private bool TryClearPressurePlateBlockers()
	{
		if (_pressurePlate == null)
		{
			Error("No pressure plate.", null, 124);
			return false;
		}
		CNode node = ScenarioManager.PathFinder.GetNode(_pressurePlate.ArrayIndex);
		node.Blocked = false;
		node.Walkable = true;
		return true;
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickActor is CEnemyActor)
		{
			TryTickEnemyAbility();
		}
		else
		{
			TryTickPlayerAbility(_tickActor);
		}
	}

	private void TryTickPlayerAbility(CActor playerChar)
	{
		if (TryClearPressurePlateBlockers())
		{
			Trace("Tick Player Character Special Rules...", null, 148);
			EnableBlockersOnPressurePlate();
		}
	}

	private void TryTickEnemyAbility()
	{
		if ((_tickActor.Type == CActor.EType.Enemy || _tickActor.Type == CActor.EType.Enemy2) && TryClearPressurePlateBlockers() && (_tickAbility is CAbilityPush || _tickAbility is CAbilityPull))
		{
			Trace("Enemy " + _tickActor.Class.ID + " performs Push/Pull Ability. Creating obstacle blockers.", null, 159);
			EnableBlockersOnPressurePlate();
		}
	}

	private void EnableBlockersOnPressurePlate()
	{
		bool walkable = false;
		if (_tickAbility != null)
		{
			bool flag = _tickAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA;
			bool flag2 = _tickActor is CEnemyActor;
			if (flag)
			{
				walkable = true;
			}
			else
			{
				CAbility.EAbilityType abilityType = _tickAbility.AbilityType;
				walkable = ((abilityType != CAbility.EAbilityType.Move && abilityType != CAbility.EAbilityType.Teleport2) ? ((_tickAbility.AbilityType != CAbility.EAbilityType.Push && _tickAbility.AbilityType != CAbility.EAbilityType.Pull) ? true : false) : flag2);
			}
		}
		if (SkipBlockerAddTiles_cvar)
		{
			return;
		}
		if (TestAdjacentBlockers_cvar)
		{
			foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(_pressurePlate.PropTile))
			{
				TileIndex index = new TileIndex(allAdjacentTile.m_ArrayIndex);
				CNode node = ScenarioManager.PathFinder.GetNode(index);
				node.Blocked = true;
				node.Walkable = walkable;
			}
			return;
		}
		CNode node2 = ScenarioManager.PathFinder.GetNode(_pressurePlate.ArrayIndex);
		node2.Blocked = true;
		node2.Walkable = walkable;
	}

	protected override void TickRepositionInternal()
	{
		foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
		{
			CActor cActor = ScenarioManager.Scenario.FindActor(actorState.ActorGuid);
			if (cActor != null && cActor.ArrayIndex.X == _pressurePlate.ArrayIndex.X && cActor.ArrayIndex.Y == _pressurePlate.ArrayIndex.Y)
			{
				_actorStepOnPositionObjective.ScriptedIsComplete = true;
				_actorStepOnPositionObjective.CheckObjectiveComplete(ScenarioManager.CurrentScenarioState.Players.Count);
				break;
			}
		}
	}

	private void TryTickRuinedMachine(CActor ruinedMachine, ActorState ruinedMachineState)
	{
		string iD = ruinedMachine.Class.ID;
		if (!iD.ToLowerInvariant().Contains("ruinedmachine") && !iD.ToLowerInvariant().Contains(ActAsRuinedMachine_cvar))
		{
			Trace(iD + " is not a Ruined Machine", null, 237);
			return;
		}
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartTurn)
		{
			Error("TickRuinedMachine expects to be placed on StartTurn phase.", null, 243);
			return;
		}
		if (!ruinedMachine.IsEnemyMonsterType)
		{
			Error(iD + " should be enemy EType.", null, 248);
			return;
		}
		if (ruinedMachineState == null)
		{
			Error("Couldn't find ActorState in 'Monsters', skipping...", null, 253);
			return;
		}
		Trace("Tick Runied Machine Special Rules...", null, 257);
		ruinedMachineState.ResetAIFocusOverride();
		_blockers.Clear();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			_blockers.Add(playerActor.ArrayIndex);
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			_blockers.Add(heroSummon.ArrayIndex);
		}
		TryClearPressurePlateBlockers();
		List<Point> list = ScenarioManager.PathFinder.FindPath(ruinedMachine, ruinedMachine.ArrayIndex, new Point(_pressurePlate.ArrayIndex), ignoreBlocked: false, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false, _blockers, _blockers);
		EnableBlockersOnPressurePlate();
		ruinedMachine.AIPreferMoveFocusPath = foundPath;
		if (foundPath)
		{
			ruinedMachineState.SetAIFocusOverride(_focusOverride);
			Trace($"Found path, num points: {list.Count}", null, 300);
			Trace($"Runied Machine focuses on pressure plate at {_pressurePlate.ArrayIndex}.", null, 301);
		}
		else
		{
			Trace("No path to pressure plate; Runied Machine focus override removed.", null, 304);
		}
	}
}
