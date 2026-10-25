using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEndedAndLongRest : CBespokeBehaviour
{
	public CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnActionEndedAndLongRest;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEndedAndLongRest(CActor actor, CAbility ability, CActiveBonus activeBonus)
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
					if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, null, m_Actor, cActor))
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

	public override void OnActionEnded(CActor actorEndingAction)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (cActor != actorEndingAction)
		{
			return;
		}
		if (cActor is CPlayerActor cPlayerActor)
		{
			AbilityData.EActiveBonusFlag flags = ActiveBonus.Ability.ActiveBonusData.Flags;
			if ((flags.HasFlag(AbilityData.EActiveBonusFlag.WhenSlow) && cPlayerActor.Agility != CPlayerActor.EAgility.Slow) || (flags.HasFlag(AbilityData.EActiveBonusFlag.WhenFast) && cPlayerActor.Agility != CPlayerActor.EAgility.Fast))
			{
				return;
			}
		}
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonusData.Filter.IsValidTarget(cActor, m_ActiveBonus.Caster, isTargetedAbility: false, useTargetOriginalType: false, false) || !m_ActiveBonus.RequirementsMet(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.ValidToWorkAccordingToTracker)
		{
			if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, null, actorEndingAction, cActor))
			{
				return;
			}
			List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
			((CPhaseAction)PhaseManager.CurrentPhase).StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true);
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}
}
