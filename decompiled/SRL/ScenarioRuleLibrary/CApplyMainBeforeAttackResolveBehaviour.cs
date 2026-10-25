using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CApplyMainBeforeAttackResolveBehaviour : CBespokeBehaviour
{
	public CApplyMainBeforeAttackResolveBehaviour(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (!IsValidTarget(ability, m_Actor) || m_ActiveBonusData.IsToggleBonus)
		{
			return;
		}
		CAbilityAddActiveBonus cAbilityAddActiveBonus = m_Ability as CAbilityAddActiveBonus;
		CAbility cAbility = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (!m_ActiveBonus.SetupTriggerContext(this, cAbilityAddActiveBonus, ability, ability.TargetingActor, m_Actor, ("ModifiedStrength", modifiedStrength)))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(ability.TargetingActor);
		cAbility.ApplyToActor(m_Actor);
		if (PhaseManager.CurrentPhaseAction != null)
		{
			cAbility.ActorsTargeted.Clear();
			cAbility.ActorsTargeted.Add(m_Actor);
			cAbility.TilesSelected.Clear();
			cAbility.TilesSelected.Add(m_Actor.Tile);
			List<CAbility> list = new List<CAbility>();
			foreach (CAbility subAbility in cAbility.SubAbilities)
			{
				subAbility.ParentAbility = cAbility;
				list.Add(subAbility);
				subAbility.AddParentContext(cAbilityAddActiveBonus.VarsContext);
			}
			PhaseManager.CurrentPhaseAction.StackNextAbilities(list, m_Actor, killAfter: false, stackToNextCurrent: true);
		}
		if (m_ActiveBonus.HasTracker)
		{
			OnBehaviourTriggered();
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}

	protected override bool IsValidTarget(CAbility ability, CActor target, bool useTargetOriginalType = false)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			bool flag = m_ActiveBonusData.Filter.IsValidTarget(cAbilityAttack.TargetingActor, target, cAbilityAttack.IsTargetedAbility, useTargetOriginalType: false, true, skipUntargetableCheck: true);
			if (((target != null && cAbilityAttack != null) & flag) && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonus.IsValidAttackType(cAbilityAttack))
			{
				return true;
			}
			return false;
		}
		return m_ActiveBonusData.Filter.IsValidTarget(target, target, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true);
	}
}
