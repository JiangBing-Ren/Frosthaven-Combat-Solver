using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnPreventedDamage;

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnPreventedDamage(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus)
		{
			return;
		}
		base.OnPreventDamageTriggered(damagePrevented, damageSource, actorDamaged, damagingAbility, preventedByBonus);
		if (!IsValidTarget(m_Ability, actorDamaged) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(actorDamaged))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, damagingAbility, damageSource, actorDamaged, ("DamagePrevented", damagePrevented)))
		{
			List<CAbility> nextAbilities = new List<CAbility> { cAbility };
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, actorDamaged, killAfter: false, stackToNextCurrent: true);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_ActiveBonus.RestrictActiveBonus(actorDamaged);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}
}
