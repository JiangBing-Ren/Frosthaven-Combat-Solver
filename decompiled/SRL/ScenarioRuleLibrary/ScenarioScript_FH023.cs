namespace ScenarioRuleLibrary;

public class ScenarioScript_FH023 : ScenarioScript
{
	private CObjectDoor _door2;

	protected override bool Initialize()
	{
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		if (scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartScenario && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartTurn && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndTurn && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartRound && scenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.EndRound)
		{
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 30);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
	}

	protected override void TickRepositionInternal()
	{
	}

	private void UpdateRuleVisibility()
	{
	}
}
