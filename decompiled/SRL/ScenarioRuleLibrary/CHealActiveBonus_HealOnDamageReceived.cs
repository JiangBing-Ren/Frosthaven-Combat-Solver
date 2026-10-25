namespace ScenarioRuleLibrary;

public class CHealActiveBonus_HealOnDamageReceived : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.HealOnDamageReceived;

	public CHealActiveBonus_HealOnDamageReceived(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage, CActor damageSourceActor)
	{
		base.OnTakenDamage(damageTaken, damagingAbility, damageReducedByShields, actualDamage, damageSourceActor);
		if (actualDamage <= 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < ScenarioManager.Scenario.AllActors.Count; i++)
		{
			CActor cActor = ScenarioManager.Scenario.AllActors[i];
			if ((m_ActiveBonusData.IsAura || cActor != m_Actor) && m_Ability.ActiveBonusData.Filter.IsValidTarget(cActor, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false) && m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, m_Actor, cActor, ("DamageTaken", damageTaken), ("DamageReducedByShields", damageReducedByShields), ("ActualDamage", actualDamage)))
			{
				int health = cActor.Health;
				cActor.Healed(damageTaken);
				flag = true;
				CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(cActor, cActor, damageTaken, health, poisonTokenRemoved: false, woundTokenRemoved: false, baneTokenRemoved: false, brittleTokenRemoved: false);
				ScenarioRuleClient.MessageHandler(message);
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
