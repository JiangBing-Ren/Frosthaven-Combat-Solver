using System.Collections.Generic;
using System.Linq;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH015 : ScenarioScript
{
	private readonly HashSet<Point> _firstRoomTileIndices = new HashSet<Point>
	{
		new Point(3, 8),
		new Point(4, 8),
		new Point(5, 8),
		new Point(6, 8),
		new Point(2, 7),
		new Point(3, 7),
		new Point(4, 7),
		new Point(5, 7),
		new Point(6, 7),
		new Point(2, 6),
		new Point(3, 6),
		new Point(4, 6),
		new Point(5, 6),
		new Point(6, 6),
		new Point(7, 6),
		new Point(2, 5),
		new Point(3, 5),
		new Point(4, 5),
		new Point(5, 5),
		new Point(6, 5),
		new Point(2, 4),
		new Point(3, 4),
		new Point(4, 4),
		new Point(5, 4),
		new Point(6, 4),
		new Point(7, 4),
		new Point(2, 3),
		new Point(3, 3),
		new Point(4, 3),
		new Point(5, 3),
		new Point(6, 3),
		new Point(3, 2),
		new Point(4, 2),
		new Point(5, 2),
		new Point(6, 2)
	};

	private readonly List<Point> _escapeTileIndices = new List<Point>
	{
		new Point(4, 8),
		new Point(5, 8)
	};

	private readonly Point _escapeDestinationTileIndex = new Point(5, 10);

	private CObjective_Scripted _teleportedActorDeathLoseObjective;

	private CScenarioModifier _oneHeroCanPassModifier;

	private CScenarioModifier _healFormulaHolderModifier;

	protected override bool Initialize()
	{
		_teleportedActorDeathLoseObjective = (CObjective_Scripted)GetObjective("TeleportedActorDeath");
		_oneHeroCanPassModifier = GetScenarioModifier("oneHeroCanPass");
		_healFormulaHolderModifier = GetScenarioModifier("healFormulaHolder");
		ToggleTileHighlights(enabled: true);
		UpdateNavLinks();
		RefreshHealFormulaAmount();
		return true;
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		UpdateNavLinks();
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
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			RefreshHealFormulaAmount();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 69);
			break;
		}
	}

	private void TickStartTurn()
	{
		if (_tickActor is CPlayerActor)
		{
			UpdateNavLinks();
		}
		else
		{
			if (!(_tickActor.Class.ID == "ChaosLieutenantID"))
			{
				return;
			}
			_tickActor.SetCanTargetFromAnywhere(CActor.TargetFromAnywhereFlags.Ranged | CActor.TargetFromAnywhereFlags.IfNoOtherTargets);
			foreach (CActor item in ScenarioManager.Scenario.AllActors.Where((CActor a) => !CActor.AreActorsAllied(a.Type, CActor.EType.Enemy, a, null)))
			{
				item.SetCanBeTargetedFromAnywhere(IsActorInFirstRoom(item));
			}
		}
	}

	private void TickEndTurn()
	{
	}

	private void TickStartRound()
	{
	}

	private void TickEndRound()
	{
	}

	protected override void TickRepositionInternal()
	{
		if (_tickActor is CPlayerActor)
		{
			UpdateNavLinks();
			if (!IsActorInFirstRoom(_tickActor) && !_oneHeroCanPassModifier.IsHidden)
			{
				_oneHeroCanPassModifier.SetHiddenState(shouldSetHidden: true);
			}
		}
	}

	protected override void TickDeathInternal()
	{
		if (_tickActor is CPlayerActor && !IsActorInFirstRoom(_tickActor))
		{
			_teleportedActorDeathLoseObjective.ScriptedIsComplete = true;
			CheckObjectivesComplete();
		}
		RefreshHealFormulaAmount(_tickActor);
	}

	private void RefreshHealFormulaAmount(CActor deadActor = null)
	{
		if (_healFormulaHolderModifier != null)
		{
			int level = ScenarioManager.CurrentScenarioState.Level;
			int num = ScenarioManager.Scenario.AllAliveMonsters.Count((CEnemyActor x) => x?.MonsterClass != null && x.MonsterClass.ID == "RuinedMachineID");
			if (deadActor?.MonsterClass?.ID == "RuinedMachineID")
			{
				num--;
			}
			int num2 = level * num / 2;
			_healFormulaHolderModifier.CustomLocKeyArgs = new List<object> { num2 };
			ScenarioManager.CurrentScenarioState.UpdateScenarioModifiersDescriptions();
		}
	}

	private bool IsActorInFirstRoom(CActor actor)
	{
		return _firstRoomTileIndices.Contains(actor.ArrayIndex);
	}

	private void ToggleTileHighlights(bool enabled)
	{
		List<Point> list = new List<Point>();
		list.AddRange(_escapeTileIndices);
		list.Add(_escapeDestinationTileIndex);
		ScenarioManager.ToggleTileHighlights(list, enabled);
		List<(Point, Point)> list2 = new List<(Point, Point)>();
		foreach (Point escapeTileIndex in _escapeTileIndices)
		{
			list2.Add((escapeTileIndex, _escapeDestinationTileIndex));
		}
		ScenarioManager.ToggleTileLinks(list2, enabled);
	}

	private void UpdateNavLinks()
	{
		bool flag = ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => !IsActorInFirstRoom(x));
		bool flag2 = GameState.CurrentActor != null && GameState.CurrentActor.OriginalType != CActor.EType.Player;
		if (flag | flag2)
		{
			foreach (Point escapeTileIndex in _escapeTileIndices)
			{
				ScenarioManager.PathFinder.GetNode(escapeTileIndex).NavLinks.Clear();
			}
			ToggleTileHighlights(enabled: false);
			return;
		}
		foreach (Point escapeTileIndex2 in _escapeTileIndices)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(escapeTileIndex2);
			CNode node2 = ScenarioManager.PathFinder.GetNode(_escapeDestinationTileIndex);
			CNavLink item = new CNavLink(2, node2);
			if (!node.NavLinks.Contains(item))
			{
				node.NavLinks.Add(item);
			}
		}
		ToggleTileHighlights(enabled: true);
	}

	public override bool EnemiesAttackWithMinimalMovement()
	{
		return true;
	}

	public override IEnumerable<(string, string)> GetSpecialRulesParamsString()
	{
		yield return ("heal_from_ruined_machines_count", _healFormulaHolderModifier.CustomLocKeyArgs[0].ToString());
	}

	public override bool IsTeleportValid(CTile a, CTile b)
	{
		if (!ScenarioManager.Scenario.PlayerActors.Any((CPlayerActor x) => !IsActorInFirstRoom(x)))
		{
			return true;
		}
		if (a.m_HexMap.RoomName == "Room_2" && b.m_HexMap.RoomName != "Room_2")
		{
			return false;
		}
		if (b.m_HexMap.RoomName == "Room_2" && a.m_HexMap.RoomName != "Room_2")
		{
			return false;
		}
		return base.IsTeleportValid(a, b);
	}
}
