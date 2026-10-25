using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnShortRest : CBespokeBehaviour
{
	public CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnShortRest;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnShortRest(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public bool TriggerAbility(CPlayerActor playerActor)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : playerActor);
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: true);
			if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, null, m_Actor, playerActor))
			{
				return false;
			}
			if (!(cAbility is CAbilityChooseAbility))
			{
				cAbility.SetCanUndo(canUndo: false);
			}
			CActor currentActor = GameState.CurrentActor;
			List<CAbility> list = new List<CAbility> { cAbility };
			if (PhaseManager.CurrentPhaseAction == null)
			{
				if (GameState.InternalCurrentActor != m_ActiveBonus.Caster)
				{
					GameState.OverrideCurrentActorForOneAction(m_ActiveBonus.Caster, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				}
				PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true, m_ActiveBonus, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
			}
			else
			{
				PhaseManager.CurrentPhaseAction.StackNextAbilities(list, m_ActiveBonus.Caster, killAfter: false, stackToNextCurrent: true);
			}
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
			return true;
		}
		return false;
	}
}
