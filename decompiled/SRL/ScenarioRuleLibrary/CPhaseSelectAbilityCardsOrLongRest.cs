namespace ScenarioRuleLibrary;

public class CPhaseSelectAbilityCardsOrLongRest : CPhase
{
	public bool SyncMultiplayer;

	public CPhaseSelectAbilityCardsOrLongRest()
	{
		m_PhaseType = PhaseType.SelectAbilityCardsOrLongRest;
	}

	protected override void OnNextStep(bool passing = false)
	{
		_phaseEnded = false;
		CPlayerToSelectAbilityCardsOrLongRest_MessageData message = new CPlayerToSelectAbilityCardsOrLongRest_MessageData(null)
		{
			m_SyncMultiplayer = SyncMultiplayer
		};
		ScenarioRuleClient.MessageHandler(message);
		SyncMultiplayer = false;
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		NextStep();
	}

	protected override void OnEndPhase()
	{
		CPlayersHaveSelectedAbilityCardsOrLongRest_MessageData message = new CPlayersHaveSelectedAbilityCardsOrLongRest_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}
}
