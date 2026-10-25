using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackStart : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttackStart;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackStart(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAttackStart(CAbilityAttack attackAbility)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!IsValidAbilityType(attackAbility) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, attackAbility, null, cActor))
		{
			List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
			(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true);
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
