using System;
using AStar;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH013 : ScenarioScript
{
	private static readonly Point ScenarioActorIndex = new Point(1, 3);

	private CObjective_Scripted _scenarioHealthLoseObjective;

	private CObjectActor _scenarioActor;

	public override bool EnemiesAttackWithMinimalMovement()
	{
		return true;
	}

	protected override bool Initialize()
	{
		_scenarioHealthLoseObjective = (CObjective_Scripted)GetObjective("ScenarioHealthLoseObjective");
		_scenarioActor = ScenarioManager.Scenario.FindObjectActorAt(ScenarioActorIndex);
		_scenarioActor.SetCanBeTargetedFromAnywhere(value: true);
		CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(_scenarioActor, active: false);
		ScenarioRuleClient.MessageHandler(message);
		if (_scenarioActor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).Count == 0)
		{
			CAbility cAbility = GetScenarioModifier("ImmunityAbility").TriggerAbility();
			cAbility.Start(_scenarioActor, _scenarioActor);
			cAbility.ApplyToActor(_scenarioActor);
		}
		UpdateObjectives();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartTurn)
		{
			TickStartTurn();
		}
		else
		{
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 45);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		UpdateObjectives();
	}

	private void TickStartTurn()
	{
		if (_tickActor.Type == CActor.EType.Enemy)
		{
			_tickActor.SetCanTargetFromAnywhere(CActor.TargetFromAnywhereFlags.AnyIfNoOtherTargets);
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickActor == _scenarioActor)
		{
			ScenarioRuleClient.MessageHandler(new CTriggerCameraShake_MessageData(null));
			ScenarioRuleClient.MessageHandler(new CPlaySound_MessageData("StructureAttack"));
		}
	}

	private void UpdateObjectives()
	{
		_scenarioHealthLoseObjective.Current = _scenarioActor.MaxHealth - (int)MathF.Max(0f, _scenarioActor.Health);
		_scenarioHealthLoseObjective.Total = _scenarioActor.MaxHealth;
		_scenarioHealthLoseObjective.ScriptedIsComplete = _scenarioHealthLoseObjective.Current == _scenarioHealthLoseObjective.Total;
		CheckObjectivesComplete();
	}

	public override bool ShouldSpawnProjectile(CActor attacker, CActor target)
	{
		return target != _scenarioActor;
	}

	public override bool IsScenarioActor(CActor actor)
	{
		return actor == _scenarioActor;
	}
}
