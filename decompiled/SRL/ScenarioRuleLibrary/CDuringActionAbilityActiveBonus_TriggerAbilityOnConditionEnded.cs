using System;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnConditionEnded : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnConditionEnded;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnConditionEnded(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnPositiveConditionEndedOnActor(CCondition.EPositiveCondition positiveConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
		Trigger(target, positiveConditionType, CCondition.ENegativeCondition.NA, decTrigger);
	}

	public override void OnNegativeConditionEndedOnActor(CCondition.ENegativeCondition negativeConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
		Trigger(target, CCondition.EPositiveCondition.NA, negativeConditionType, decTrigger);
	}

	private void Trigger(CActor target, CCondition.EPositiveCondition positiveConditionType, CCondition.ENegativeCondition negativeConditionType, EConditionDecTrigger decTrigger)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonusData.Filter.IsValidTarget(cActor, m_ActiveBonus.Caster, isTargetedAbility: false, useTargetOriginalType: false, false) || !m_ActiveBonus.RequirementsMet(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.ValidToWorkAccordingToTracker)
		{
			if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, null, target, cActor, ("PositiveCondition", new TValue(positiveConditionType)), ("NegativeCondition", new TValue(negativeConditionType)), ("DecTrigger", new TValue(decTrigger))))
			{
				return;
			}
			if (PhaseManager.CurrentPhase is CPhaseEndTurn)
			{
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, cActor));
			}
			else
			{
				PhaseManager.StartAbilities(cAbility, cActor, stackInsteadOfInject: true, target);
			}
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
