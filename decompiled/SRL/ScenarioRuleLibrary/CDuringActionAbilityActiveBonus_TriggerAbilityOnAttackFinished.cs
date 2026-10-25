using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttackFinished;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAttackFinished(CAbilityAttack attackAbility, CActor target, int damageDealt)
	{
		if (attackAbility.TargetingActor is CPlayerActor cPlayerActor && cPlayerActor.UsesTimeTokens())
		{
			AbilityData.EActiveBonusFlag flags = ActiveBonus.Ability.ActiveBonusData.Flags;
			if ((flags.HasFlag(AbilityData.EActiveBonusFlag.WhenSlow) && cPlayerActor.Agility != CPlayerActor.EAgility.Slow) || (flags.HasFlag(AbilityData.EActiveBonusFlag.WhenFast) && cPlayerActor.Agility != CPlayerActor.EAgility.Fast))
			{
				return;
			}
		}
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : attackAbility.TargetingActor);
		if (!IsValidTarget(attackAbility, target) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !RequiredDamageDone(damageDealt))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonusData.UseTriggerAbilityAsParent && attackAbility != null)
		{
			cAbility.ParentAbility = attackAbility;
		}
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, attackAbility, null, target, ("DamageDealt", damageDealt)))
		{
			List<CAbility> nextAbilities = new List<CAbility> { cAbility };
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, cActor, killAfter: false, stackToNextCurrent: true);
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

	public bool RequiredDamageDone(int damageDealt)
	{
		if (m_ActiveBonus.NeedsDamage && damageDealt < 1)
		{
			return false;
		}
		return true;
	}
}
