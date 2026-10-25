namespace ScenarioRuleLibrary;

public class CPhaseAfterEndTurnLoot : CPhase
{
	public CPhaseAfterEndTurnLoot()
	{
		m_PhaseType = PhaseType.AfterEndTurnLoot;
	}

	protected override void OnNextStep(bool passing = false)
	{
		GameState.NextPhase();
	}
}
