namespace ScenarioRuleLibrary;

internal class CPhaseStartScenarioEffects : CPhase
{
	public CPhaseStartScenarioEffects()
	{
		m_PhaseType = PhaseType.StartScenarioEffects;
	}

	protected override void OnNextStep(bool passing = false)
	{
		GameState.NextPhase();
	}
}
