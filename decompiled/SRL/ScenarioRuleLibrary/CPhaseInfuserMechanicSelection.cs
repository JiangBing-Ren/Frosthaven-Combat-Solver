using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CPhaseInfuserMechanicSelection : CPhase
{
	public CActor TriggerActor;

	public CPhaseAction PreviousActionPhase;

	public CAbilitySelectAndPlayCard PreviousSelectAndPlayCardAbility;

	public GameState.EActionInitiator PreviousActionInitiator;

	public CAbilityCard PreviousRoundAbilityCardSelected;

	public IEnumerable<CActor> ValidActors()
	{
		return ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x.GetUnrestrictedInfusionBonuses().Any());
	}

	public CPhaseInfuserMechanicSelection()
	{
		if (GameState.PendingInfusionPhase != null)
		{
			TriggerActor = GameState.PendingInfusionPhase.TriggerActor;
			PreviousActionPhase = GameState.PendingInfusionPhase.PreviousActionPhase;
			PreviousSelectAndPlayCardAbility = GameState.PendingInfusionPhase.PreviousSelectAndPlayCardAbility;
			PreviousActionInitiator = GameState.PendingInfusionPhase.PreviousActionInitiator;
			PreviousRoundAbilityCardSelected = GameState.PendingInfusionPhase.PreviousRoundAbilityCardSelected;
		}
		else
		{
			TriggerActor = GameState.InternalCurrentActor;
			PreviousActionPhase = PhaseManager.CurrentPhaseAction;
			PreviousSelectAndPlayCardAbility = GameState.CurrentOutOfPhaseCardSelectionAbility as CAbilitySelectAndPlayCard;
			PreviousActionInitiator = GameState.CurrentActionInitiator;
			PreviousRoundAbilityCardSelected = GameState.RoundAbilityCardSelected;
		}
		GameState.PendingInfusionPhase = this;
		if (PreviousSelectAndPlayCardAbility != null)
		{
			GameState.CurrentOutOfPhaseCardSelectionAbility = null;
		}
		m_PhaseType = PhaseType.InfuserMechanicSelection;
		GameState.CurrentAction = null;
		GameState.InternalCurrentActor.ActionSelection();
		CInfuserMechanicSelectionPhaseStart_MessageData message = new CInfuserMechanicSelectionPhaseStart_MessageData(TriggerActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	private void FinishToggledInfusions()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			foreach (CActiveBonus item in playerActor.GetUnrestrictedInfusionBonuses().ToList())
			{
				if (item.ToggleLocked && item is CDuringTurnAbilityActiveBonus cDuringTurnAbilityActiveBonus)
				{
					cDuringTurnAbilityActiveBonus.TriggerAbility();
				}
			}
		}
	}

	protected override void OnNextStep(bool passing = false)
	{
		FinishToggledInfusions();
		if (!ValidActors().Any())
		{
			ManuallyEnd();
			return;
		}
		CInfuserMechanicSelection_MessageData message = new CInfuserMechanicSelection_MessageData(TriggerActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnEndPhase()
	{
		base.OnEndPhase();
		_phaseEnded = false;
	}

	public void ManuallyEnd()
	{
		GameState.PendingInfusionPhase = null;
		GameState.RoundAbilityCardSelected = PreviousRoundAbilityCardSelected;
		GameState.CurrentActionInitiator = PreviousActionInitiator;
		GameState.CurrentOutOfPhaseCardSelectionAbility = PreviousSelectAndPlayCardAbility;
		PhaseManager.ManuallySetCurrentPhase(PreviousActionPhase);
		if (PreviousSelectAndPlayCardAbility != null)
		{
			TriggerActor?.Inventory?.HandleUsedItems();
			GameState.NextPhase();
		}
		else
		{
			PhaseManager.StepComplete();
		}
	}
}
