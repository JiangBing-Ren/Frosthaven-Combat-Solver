namespace ScenarioRuleLibrary;

public class CPhaseEndTurnLoot : CPhase
{
	public CPhaseEndTurnLoot()
	{
		m_PhaseType = PhaseType.EndTurnLoot;
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		GameState.NextPhase();
	}
}
