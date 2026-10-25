using System.Collections.Generic;
using AStar;

namespace ScenarioRuleLibrary;

public class CShieldActiveBonus_ShieldAndRetaliate : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ShieldAndRetaliate;

	public CShieldActiveBonus_ShieldAndRetaliate(CActor actor, CAbilityShield ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (IsValidTarget(ability, m_Actor) && !m_ActiveBonusData.IsToggleBonus && modifiedStrength > 0 && m_ActiveBonus.SetupTriggerContext(this, null, ability, null, m_Actor, ("ModifiedStrength", modifiedStrength)))
		{
			Retaliate(ability.TargetingActor, ability);
			m_ActiveBonus.RestrictActiveBonus(ability.TargetingActor);
			if (m_ActiveBonus.HasTracker)
			{
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus)
		{
			return;
		}
		base.OnPreventDamageTriggered(damagePrevented, damageSource, damagedActor, damagingAbility, preventedByBonus);
		if (!IsValidTarget(damagingAbility, damagedActor))
		{
			return;
		}
		Retaliate(damageSource, damagingAbility);
		if (m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, damagedActor, ("PreventedDamage", damagePrevented)))
		{
			m_ActiveBonus.RestrictActiveBonus(damagedActor);
			if (m_ActiveBonus.HasTracker)
			{
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}

	public void Retaliate(CActor damageSource, CAbility damagingAbility)
	{
		CAbilityRetaliate obj = m_Ability.ActiveBonusData.AbilityData as CAbilityRetaliate;
		int strength = obj.Strength;
		int retaliateRange = obj.RetaliateRange;
		if (damageSource != null && !damageSource.IsDead && damagingAbility != null && strength > 0 && retaliateRange > 0)
		{
			List<Point> list = ScenarioManager.PathFinder.FindPath(damageSource, damageSource.ArrayIndex, m_Actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
			if (foundPath && list.Count <= retaliateRange)
			{
				CAbilityRetaliate retaliateAbility = new CAbilityRetaliate(retaliateRange)
				{
					Strength = strength
				};
				CRetaliate_MessageData message = new CRetaliate_MessageData("", m_Actor)
				{
					m_RetaliateAbility = retaliateAbility,
					m_ActorAppliedTo = m_Actor
				};
				ScenarioRuleClient.MessageHandler(message);
				damageSource.ApplyRetaliateToAttack(m_Actor, damagingAbility, strength);
			}
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (m_ActiveBonusData.StrengthIsScalar)
		{
			return 0;
		}
		return m_Strength;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (!m_ActiveBonusData.StrengthIsScalar)
		{
			return 1;
		}
		return m_Strength;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		return m_ActiveBonusData.ProcXP;
	}
}
