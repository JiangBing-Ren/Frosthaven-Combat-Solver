namespace ScenarioRuleLibrary;

public class CPhaseEndRound : CPhase
{
	public CPhaseEndRound()
	{
		m_PhaseType = PhaseType.EndRound;
	}

	protected override void OnNextStep(bool passing = false)
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			playerActor.IsLongRestSelected = false;
			playerActor.IsLongRestActionSelected = false;
			playerActor.CharacterClass.LongRest = false;
		}
		foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
		{
			allActor.InitiativeLockedDuringRound = null;
			if (!allActor.IsDead)
			{
				SEventLogMessageHandler.AddEventLogMessage(new SEventActorEndRound(allActor));
			}
		}
		ScenarioRuleClient.MessageHandler(new CEndRound_MessageData(GameState.InternalCurrentActor));
	}

	public void EndRoundSynchronise()
	{
		GameState.NextPhase();
	}
}
