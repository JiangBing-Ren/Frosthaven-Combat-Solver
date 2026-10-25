namespace ScenarioRuleLibrary;

public class CDamageActiveBonus_DamageOnCurseModApplied : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DamageOnCurseModApplied;

	public CDamageActiveBonus_DamageOnCurseModApplied(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public void TriggerAbility(CActor actorToDamage)
	{
		if (m_Ability.ActiveBonusData.Filter.IsValidTarget(actorToDamage, m_Actor, isTargetedAbility: false, useTargetOriginalType: false, false) && m_ActiveBonus.SetupTriggerContext(this, null, null, m_Actor, actorToDamage))
		{
			actorToDamage.ApplyImmediateDamage(m_Ability.Strength);
			GameState.ActorHealthCheck(m_Actor, actorToDamage);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}
}
