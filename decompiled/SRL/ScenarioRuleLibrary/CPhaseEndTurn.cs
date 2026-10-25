namespace ScenarioRuleLibrary;

public class CPhaseEndTurn : CPhase
{
	public bool m_SentEndTurnMessage { get; private set; }

	public CPhaseEndTurn()
	{
		m_PhaseType = PhaseType.EndTurn;
		m_SentEndTurnMessage = false;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (!m_SentEndTurnMessage)
		{
			ScenarioRuleClient.MessageHandler(new CEndTurn_MessageData(GameState.InternalCurrentActor));
			m_SentEndTurnMessage = true;
		}
		else
		{
			Log.ErrorWithReport("Attempted to send EndTurn message twice in the same EndTurn phase");
		}
	}

	public void EndTurnSynchronise()
	{
		ElementInfusionBoardManager.ProcessEndTurnInfusions();
		GameState.NextPhase();
	}
}
