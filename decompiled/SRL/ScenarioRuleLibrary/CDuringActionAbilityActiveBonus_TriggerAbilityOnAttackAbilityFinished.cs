using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackAbilityFinished : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttackAbilityFinished;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackAbilityFinished(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAttackAbilityFinished(CAbilityAttack attackAbility)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : attackAbility.TargetingActor);
		if (!IsValidTarget(attackAbility, attackAbility.TargetingActor) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !RequiredDamageDone(attackAbility, cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, attackAbility, null, cActor))
		{
			if (m_ActiveBonusData.UseTriggerAbilityAsParent && attackAbility != null)
			{
				cAbility.ParentAbility = attackAbility;
			}
			List<CAbility> nextAbilities = new List<CAbility> { cAbility };
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, attackAbility.TargetingActor, killAfter: false, stackToNextCurrent: true);
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

	public bool RequiredDamageDone(CAbilityAttack attackAbility, CActor actor)
	{
		if (m_ActiveBonus.NeedsDamage && attackAbility.DamageInflictedByAbility < 1)
		{
			return false;
		}
		return true;
	}
}
