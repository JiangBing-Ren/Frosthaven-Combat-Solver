namespace ScenarioRuleLibrary;

public class CDamageActiveBonus_DamageOnReceivedDamage : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DamageOnReceivedDamage;

	public CDamageActiveBonus_DamageOnReceivedDamage(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage, CActor damageSourceActor)
	{
		base.OnTakenDamage(damageTaken, damagingAbility, damageReducedByShields, actualDamage, damageSourceActor);
		if (!m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, null, null, ("DamageTaken", damageTaken), ("DamageReduceByShields", damageReducedByShields), ("ActualDamage", actualDamage)))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < ScenarioManager.Scenario.AllActors.Count; i++)
		{
			if (ScenarioManager.Scenario.AllActors[i] != m_Actor && m_Ability.ActiveBonusData.Filter.IsValidTarget(ScenarioManager.Scenario.AllActors[i], m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false))
			{
				ScenarioManager.Scenario.AllActors[i].ApplyImmediateDamage(damageTaken, cannotPrevent: false, pierceInvulnerable: true);
				GameState.ActorHealthCheck(m_Actor, ScenarioManager.Scenario.AllActors[i]);
				flag = true;
			}
		}
		if (flag)
		{
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}
}
