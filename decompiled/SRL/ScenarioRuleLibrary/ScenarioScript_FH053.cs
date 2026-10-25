namespace ScenarioRuleLibrary;

public class ScenarioScript_FH053 : ScenarioScript
{
	protected override bool Initialize()
	{
		PutSnorkelsOn();
		return true;
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			UnderwaterStart();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			PutSnorkelsOn(_tickActor);
			break;
		}
	}
}
