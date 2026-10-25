namespace ScenarioRuleLibrary;

internal class CPhaseStartRoundEffects : CPhase
{
	public CPhaseStartRoundEffects()
	{
		m_PhaseType = PhaseType.StartRoundEffects;
	}

	protected override void OnNextStep(bool passing = false)
	{
		GameState.NextPhase();
	}
}
