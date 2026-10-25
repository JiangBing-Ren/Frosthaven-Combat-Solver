namespace ScenarioRuleLibrary;

public class ScenarioScript_FH042 : ScenarioScript
{
	protected override bool Initialize()
	{
		PutSnorkelsOn();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UnderwaterStart();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			PutSnorkelsOn(_tickActor);
			break;
		default:
			Error($"Unhandled phase {scenarioModifierTriggerPhase}", null, 31);
			break;
		}
	}
}
