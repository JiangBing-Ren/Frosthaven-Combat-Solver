namespace ScenarioRuleLibrary;

public class CSummonActiveBonus_CastAbilityFromSummon : CBespokeBehaviour
{
	private CAbility m_NewAbility;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.CastAbilityFromSummon;

	public CSummonActiveBonus_CastAbilityFromSummon(CActor actor, CAbilitySummon ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentAbility, activeBonus, toggledOn);
		if (IsValidAbilityType(currentAbility) & toggledOn)
		{
			CHeroSummonActor cHeroSummonActor = m_Actor as CHeroSummonActor;
			m_NewAbility = cHeroSummonActor.ApplyBaseStats(currentAbility, copyCurrentOverrides: true, overrideAttackZero: true);
			if (m_ActiveBonus.SetupTriggerContext(this, m_NewAbility, currentAbility, cHeroSummonActor, null))
			{
				m_NewAbility.SetCanUndo(canUndo: false);
				m_NewAbility.OriginalTargetingActor = currentAbility.TargetingActor;
				CActorIsControlled_MessageData message = new CActorIsControlled_MessageData(m_Actor)
				{
					m_ControlActorAbility = null,
					m_ControlledActor = m_Actor
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.OverrideCurrentActorForOneAction(m_Actor, CActor.EType.Player, killActorAfterAction: false, null, currentAbility.TargetingActor, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				(PhaseManager.CurrentPhase as CPhaseAction).SwapAbility(currentAbility, m_NewAbility, canUndo: false);
			}
		}
		else if (!toggledOn)
		{
			CAbility cAbility = CAbility.CopyAbility(currentAbility, generateNewID: false, fullCopy: false, copyCurrentOverrides: true);
			CHeroSummonActor cHeroSummonActor2 = m_Actor as CHeroSummonActor;
			cAbility.Range -= cHeroSummonActor2.SummonData.Range + cHeroSummonActor2.GetEnhancementRangeStrength();
			GameState.EndOverrideCurrentActorForOneAction();
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			(PhaseManager.CurrentPhase as CPhaseAction).SwapAbility(currentAbility, cAbility);
		}
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		if (attackAbility != null && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(endedAbility.TargetingActor))
		{
			OnBehaviourTriggered();
		}
	}
}
