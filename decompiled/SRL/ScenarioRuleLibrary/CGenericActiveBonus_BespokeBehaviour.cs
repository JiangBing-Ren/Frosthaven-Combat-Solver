using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CGenericActiveBonus_BespokeBehaviour : CBespokeBehaviour, CGenericActiveBonus.IListener
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => ActiveBonus.Ability.ActiveBonusData.Behaviour;

	public CGenericActiveBonus_BespokeBehaviour(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public bool PrepAbilityForEvent(CActiveBonus.EActiveBonusBehaviourType evt, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, out CAbility abilityToAdd, out CActor actorToUse, params (string, TValue)[] additionalVars)
	{
		actorToUse = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : (triggerActor ?? triggerAbility?.TargetingActor ?? m_ActiveBonus.Caster));
		abilityToAdd = null;
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(actorToUse))
		{
			return false;
		}
		abilityToAdd = CAbility.CopyAbility(AbilityAddActiveBonus?.AddAbility, generateNewID: false);
		if (ActiveBonus.Ability.ActiveBonusData.GetMiscVar("ProcessIfDead").GetBool())
		{
			abilityToAdd.ProcessIfDead = true;
		}
		if (m_ActiveBonusData.UseTriggerAbilityAsParent && triggerAbility != null && abilityToAdd != null)
		{
			abilityToAdd.ParentAbility = triggerAbility;
		}
		if (!m_ActiveBonus.SetupTriggerContext(this, abilityToAdd, triggerAbility, triggerActor, triggerTarget, additionalVars))
		{
			return false;
		}
		return true;
	}

	public bool OnEvent(CActiveBonus.EActiveBonusBehaviourType evt, CAbility triggerAbility, CActor triggerActor, CActor triggerTarget, params (string, TValue)[] additionalVars)
	{
		if (!PrepAbilityForEvent(evt, triggerAbility, triggerActor, triggerTarget, out var abilityToAdd, out var actorToUse, additionalVars))
		{
			return false;
		}
		if (abilityToAdd != null)
		{
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				List<CAbility> nextAbilities = new List<CAbility> { abilityToAdd };
				cPhaseAction.StackNextAbilities(nextAbilities, actorToUse, killAfter: false, stackToNextCurrent: true);
			}
			else
			{
				PhaseManager.StartAbility(abilityToAdd, abilityToAdd.GetAbilityBaseCard());
			}
		}
		m_ActiveBonus.RestrictActiveBonus(actorToUse);
		OnBehaviourTriggered();
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
		return true;
	}
}
