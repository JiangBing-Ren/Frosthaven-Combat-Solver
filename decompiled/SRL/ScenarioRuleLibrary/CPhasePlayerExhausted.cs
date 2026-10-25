using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPhasePlayerExhausted : CPhase
{
	public CPhasePlayerExhausted()
	{
		m_PhaseType = PhaseType.PlayerExhausted;
	}

	protected override void OnNextStep(bool passing = false)
	{
		List<CPlayerActor> list = ScenarioManager.Scenario.PlayerActors.FindAll((CPlayerActor p) => p.IsExhausted());
		List<CPlayerActor> list2 = new List<CPlayerActor>();
		if (list.Count > 0)
		{
			foreach (CPlayerActor item in list)
			{
				bool actorWasAsleep = item.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				int count = item.CharacterClass.HandAbilityCards.Count;
				int count2 = item.CharacterClass.DiscardedAbilityCards.Count;
				int num = 0;
				foreach (CAbilityCard activatedAbilityCard in item.CharacterClass.ActivatedAbilityCards)
				{
					if (activatedAbilityCard.CanBeRemovedFromActiveBonuses() && activatedAbilityCard.SelectedAction.CardPile == CBaseCard.ECardPile.Discarded)
					{
						num++;
					}
				}
				int suggestedCancelCount = 2 - count2;
				if (!CPlayerActor.CalcExhausted(count, 0, count2 + num, ScenarioManager.CurrentScenarioState.FreeLongRest, restDisabled: false))
				{
					ScenarioRuleClient.MessageHandler(new COfferCancelAbilityCards_MessageData(item)
					{
						m_SuggestedCancelCount = suggestedCancelCount
					});
					GameState.WaitForConfirmation("OfferCancelAbilityCards");
				}
				if (item.IsExhausted())
				{
					list2.Add(item);
					GameState.KillActor(item, item, CActor.ECauseOfDeath.NoMoreCards, out var _, null, actorWasAsleep);
				}
			}
		}
		CPlayersExhausted_MessageData cPlayersExhausted_MessageData = new CPlayersExhausted_MessageData(null);
		cPlayersExhausted_MessageData.m_Players = list2;
		ScenarioRuleClient.MessageHandler(cPlayersExhausted_MessageData);
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		GameState.NextPhase();
	}
}
