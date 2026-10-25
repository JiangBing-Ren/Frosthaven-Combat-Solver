using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH117 : ScenarioScript
{
	private static readonly string PolarBearTrapPrefabName = "PolarBearTrap";

	private static readonly string PolarBearTrapLocKey = "FH_Scenario_Campaign_117_SCENARIO_OBJECT_1";

	private static readonly string PolarBearTrapDescLocKey = "FH_117_Polar_Bear_Trap_D";

	private List<Point> TrapPoints = new List<Point>
	{
		new Point(2, 3),
		new Point(14, 7),
		new Point(8, 3)
	};

	private CObjective_Scripted _trapBearsObjective;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	private bool IsTrapped(CActor actor)
	{
		return TrapPoints.Contains(actor.ArrayIndex);
	}

	private bool IsPolarBear(CActor actor)
	{
		return actor.Class.ID.StartsWith("PolarBear");
	}

	protected override bool Initialize()
	{
		ScenarioManager.ToggleTileHighlights(TrapPoints, enabled: true);
		_trapBearsObjective = (CObjective_Scripted)GetObjective("TrapBearsObjective");
		_trapBearsObjective.Total = TrapPoints.Count;
		return base.Initialize();
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			TickStartScenario();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 54);
			break;
		case EScenarioModifierTriggerPhase.StartRound:
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.EndTurn:
		case EScenarioModifierTriggerPhase.AfterAbility:
			break;
		}
	}

	private void TickStartScenario()
	{
		if (!OncePerRound())
		{
			return;
		}
		foreach (Point trapPoint in TrapPoints)
		{
			CTile tile = ScenarioManager.GetTile(trapPoint);
			SpawnVFXProp(tile, PolarBearTrapPrefabName, PolarBearTrapLocKey, PolarBearTrapDescLocKey);
		}
	}

	protected override void TickAbilityPerformStartedInternal()
	{
		if (_tickAbility.AbilityType == CAbility.EAbilityType.Move || _tickAbility.AbilityType == CAbility.EAbilityType.Teleport)
		{
			UpdatePathFinderNodes(_tickActor, _tickAbility);
		}
		else if (_tickAbility is CAbilityPush { CurrentTarget: var currentTarget })
		{
			UpdatePathFinderNodes(currentTarget, _tickAbility);
		}
		else if (_tickAbility is CAbilityPull { CurrentTarget: var currentTarget2 })
		{
			UpdatePathFinderNodes(currentTarget2, _tickAbility);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility is CAbilityPush cAbilityPush && (cAbilityPush.Flags & CAbility.EAbilityFlag.Special2) != 0)
		{
			CActor cActor = cAbilityPush.CurrentTarget;
			if (cActor == null)
			{
				cActor = cAbilityPush.TargetThisActorAutomatically;
			}
			int num = cAbilityPush.GetMiscVar("StrengthBeforeAttack").GetInt();
			int num2 = cAbilityPush.Strength - num;
			int num3 = cAbilityPush.DistanceMoved - num;
			if (cAbilityPush.AbilityHasHappened && num3 > 0)
			{
				DealScenarioDamage(cAbilityPush.TargetingActor, num3);
				int num4 = num2 - num3;
				if (num4 > 0)
				{
					DealScenarioDamage(cActor, num4);
				}
			}
			else
			{
				DealScenarioDamage(cActor, num2);
			}
		}
		base.TickAbilityEndedInternal();
	}

	protected override void TickRepositionInternal()
	{
		CActor tickActor = _tickActor;
		if (tickActor != null && IsPolarBear(tickActor))
		{
			if (IsTrapped(tickActor))
			{
				ApplyImmunityAbility(tickActor);
			}
			UpdateTrapBearsObjective();
		}
	}

	protected override void TickDeathInternal()
	{
		UpdateTrapBearsObjective();
	}

	private void UpdatePathFinderNodes(CActor actor, CAbility ability)
	{
		if (actor == null)
		{
			return;
		}
		foreach (Point trapPoint in TrapPoints)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(trapPoint);
			node.Blocked = !IsPolarBear(actor);
			node.IsNegativeHex = true;
		}
	}

	public override List<Point> CustomFindEnemyPathTransientSuperBlockedToAdd()
	{
		List<Point> list = new List<Point>();
		foreach (Point trapPoint in TrapPoints)
		{
			CActor livingActor = ScenarioManager.GetTile(trapPoint).LivingActor;
			if (livingActor != null && IsPolarBear(livingActor))
			{
				list.Add(trapPoint);
			}
		}
		return list;
	}

	public void ExecutePushAbility(CAbility ability, CActor playerActor, CActor bearActor, int damagePrevented)
	{
		CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
		if (ability is CAbilityAttack cAbilityAttack)
		{
			bool flag = false;
			foreach (CAttackSummary.TargetSummary target in cAbilityAttack.AttackSummary.Targets)
			{
				foreach (CAbility item in from x in target.AttackAbilityWithOverrides.SubAbilities
					where x.IsInlineSubAbility
					where x.AbilityType == CAbility.EAbilityType.Push
					select x)
				{
					if (target.ActorToAttack == bearActor)
					{
						flag = true;
						item.SetMiscVar("StrengthBeforeAttack", item.Strength);
						item.Strength = 2;
						item.Flags |= CAbility.EAbilityFlag.Special2;
					}
				}
			}
			if (flag)
			{
				return;
			}
		}
		CAbilityPush cAbilityPush = CAbility.CreateAbility(CAbility.EAbilityType.Push, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), isMonster: false, isTargetedAbility: true, damagePrevented, 99) as CAbilityPush;
		cAbilityPush.MiscAbilityData = new AbilityData.MiscAbilityData();
		cAbilityPush.MiscAbilityData.AutotriggerAbility = true;
		cAbilityPush.SetMiscVar("StrengthBeforeAttack", 0);
		cAbilityPush.TargetThisActorAutomatically = bearActor;
		cAbilityPush.Flags |= CAbility.EAbilityFlag.Special2;
		GameState.OverrideCurrentActorForOneAction(playerActor, CActor.EType.Player, killActorAfterAction: false, null, playerActor, useControllingActorModifierDeck: false, useControllingActorPlayerControl: true, immediatelySetCurrentActor: true);
		if (currentPhaseAction != null)
		{
			currentPhaseAction.InjectAbility(cAbilityPush, playerActor, ability.GetAbilityBaseCard());
		}
		else
		{
			PhaseManager.StartAbility(cAbilityPush, ability.GetAbilityBaseCard(), fullCopy: true);
		}
	}

	public override bool WillMovingOnTileEndMovement(CTile tile, CActor actor, CAbility ability)
	{
		if (IsPolarBear(actor) && TrapPoints.Any((Point x) => tile.m_ArrayIndex == x))
		{
			return true;
		}
		return base.WillMovingOnTileEndMovement(tile, actor, ability);
	}

	private void UpdateTrapBearsObjective()
	{
		List<CEnemyActor> list = ScenarioManager.Scenario.Enemy2Monsters.Where((CEnemyActor e2) => IsPolarBear(e2) && !e2.IsDead).ToList();
		_trapBearsObjective.Current = list.Count((CEnemyActor bear) => TrapPoints.Contains(bear.ArrayIndex));
		_trapBearsObjective.ScriptedIsComplete = _trapBearsObjective.Current == _trapBearsObjective.Total;
		_trapBearsObjective.ScriptedUnableToComplete = list.Count < _trapBearsObjective.Total;
		_trapBearsObjective.CheckObjectiveComplete(ScenarioScript.PartySize);
		ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
	}

	public static bool ValidateIsPolarBearNotTrapped(CActor actor)
	{
		if (!(ScenarioScript.current is ScenarioScript_FH117 scenarioScript_FH) || !scenarioScript_FH.IsTrapped(actor))
		{
			return true;
		}
		scenarioScript_FH.Trace("Polar Bear $" + actor.ActorGuid + " is trapped and will not act now.", null, 202);
		return false;
	}

	private void ApplyImmunityAbility(CActor actor)
	{
		if (actor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).Count == 0)
		{
			CAbility cAbility = GetScenarioModifier("ImmunityAbility").TriggerAbility();
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}
}
