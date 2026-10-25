using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest : CBespokeBehaviour
{
	public CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnLongRest;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnLongRest(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public bool TriggerAbility()
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		bool result = false;
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			if (m_ActiveBonus.RequirementsMet(cActor))
			{
				if (m_ActiveBonus.ValidToWorkAccordingToTracker)
				{
					CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: true);
					if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, null, m_Actor, GameState.InternalCurrentActor))
					{
						return false;
					}
					cAbility.SetCanUndo(canUndo: false);
					CActor currentActor = GameState.CurrentActor;
					if (PhaseManager.CurrentPhaseAction != null)
					{
						PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, cActor, killAfter: false, stackToNextCurrent: true);
					}
					else
					{
						PhaseManager.StartAbilities(new List<CAbility> { cAbility }, m_ActiveBonus.BaseCard, fullCopy: true, null, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
					}
					m_ActiveBonus.RestrictActiveBonus(cActor);
					OnBehaviourTriggered();
					result = true;
				}
				if (m_ActiveBonus.HasTracker)
				{
					m_ActiveBonus.UpdateXPTracker();
					m_ActiveBonus.FinishIfNoRemaining();
				}
			}
			return result;
		}
		return false;
	}
}
