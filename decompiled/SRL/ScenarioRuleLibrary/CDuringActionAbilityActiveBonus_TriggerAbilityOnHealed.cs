using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnHealed : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnHealed;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnHealed(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAfterBeingHealed()
	{
		base.OnAfterBeingHealed();
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonusData.Filter.IsValidTarget(cActor, m_ActiveBonus.Caster, isTargetedAbility: false, useTargetOriginalType: false, false))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, null, m_Actor, cActor))
		{
			List<CAbility> list = new List<CAbility> { cAbility };
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				cPhaseAction.StackNextAbilities(list, cActor);
			}
			else
			{
				PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard);
			}
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}
}
