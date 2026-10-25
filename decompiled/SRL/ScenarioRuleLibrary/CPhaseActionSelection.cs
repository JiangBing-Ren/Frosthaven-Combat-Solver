namespace ScenarioRuleLibrary;

public class CPhaseActionSelection : CPhase
{
	public CPhaseActionSelection()
	{
		m_PhaseType = PhaseType.ActionSelection;
		GameState.CurrentAction = null;
		GameState.InternalCurrentActor.ActionSelection();
		CActionSelectionPhaseStart_MessageData message = new CActionSelectionPhaseStart_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (GameState.ActorsToOverrideTurns.TryGetValue(GameState.InternalCurrentActor, out var value) && value.OverrideActionAbilities != null && value.OverrideActionAbilities.Count > 0 && value.OverrideActionBaseCard != null)
		{
			GameState.CurrentAction = null;
			PhaseManager.StartOverrideTurnAbilities(value.OverrideActionAbilities, value.OverrideActionBaseCard);
			return;
		}
		if (!GameState.ActorHealthCheck(GameState.InternalCurrentActor, GameState.InternalCurrentActor))
		{
			GameState.EndTurnCheckNextActorAndMoveToNextPhase();
			return;
		}
		if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor)
		{
			CItem.EItemTrigger eItemTrigger = CItem.EItemTrigger.AtEndOfTurn;
			if (!cPlayerActor.CharacterClass.HasLongRested || cPlayerActor.IsTakingExtraTurn)
			{
				int selectedRoundCardsCount = cPlayerActor.SelectedRoundCardsCount;
				if (GameState.GetCurrentActionSequenceType(selectedRoundCardsCount) == GameState.EActionSequenceType.FirstAction || GameState.GetCurrentActionSequenceType(selectedRoundCardsCount) == GameState.EActionSequenceType.SecondAction || GameState.GetCurrentActionSequenceType(selectedRoundCardsCount) == GameState.EActionSequenceType.ThirdAction)
				{
					eItemTrigger = CItem.EItemTrigger.DuringOwnTurn;
				}
			}
			cPlayerActor.Inventory.HighlightUsableItems(null, eItemTrigger, CItem.EItemTrigger.DuringOwnTurn);
		}
		if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor2 && cPlayerActor2.CharacterClass.HasLongRested)
		{
			ScenarioRuleClient.MessageHandler(new CPlayerLongRested_MessageData(cPlayerActor2.Health, null, cPlayerActor2)
			{
				m_AfterLongRestFinishedHealing = true
			});
		}
		else
		{
			CActionSelection_MessageData message = new CActionSelection_MessageData(GameState.InternalCurrentActor);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void OnEndPhase()
	{
		base.OnEndPhase();
		_phaseEnded = false;
	}
}
