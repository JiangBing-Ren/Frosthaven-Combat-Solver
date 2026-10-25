namespace ScenarioRuleLibrary;

internal class CPhaseAutosave : CPhase
{
	private bool m_StartedAutoSave;

	public CPhaseAutosave()
	{
		m_PhaseType = PhaseType.Autosave;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (!m_StartedAutoSave)
		{
			m_StartedAutoSave = true;
			CProcessAutosaves_MessageData message = new CProcessAutosaves_MessageData(null);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		NextStep();
	}
}
