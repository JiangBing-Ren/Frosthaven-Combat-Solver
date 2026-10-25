namespace ScenarioRuleLibrary;

public class CPhaseStartTurn : CPhase
{
	public CPhaseStartTurn()
	{
		m_PhaseType = PhaseType.StartTurn;
		CLogStartTurn_MessageData message = new CLogStartTurn_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnNextStep(bool passing = false)
	{
		CStartTurn_MessageData message = new CStartTurn_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		PhaseManager.SetNextPhase(PhaseType.ActionSelection);
	}
}
