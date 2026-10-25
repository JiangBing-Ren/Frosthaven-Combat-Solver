namespace ScenarioRuleLibrary;

public class CPhaseCheckForForgoActionActiveBonuses : CPhase
{
	public CPhaseCheckForForgoActionActiveBonuses()
	{
		m_PhaseType = PhaseType.CheckForForgoActionActiveBonuses;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (passing)
		{
			PhaseManager.SetNextPhase(PhaseType.StartTurn);
		}
		else
		{
			GameState.NextPhase();
		}
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		NextStep(passing: true);
	}
}
