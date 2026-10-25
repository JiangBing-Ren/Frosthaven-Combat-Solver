namespace ScenarioRuleLibrary;

public class CImmunityActiveBonus_ChooseCondition : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ChooseCondition;

	public CImmunityActiveBonus_ChooseCondition(CActor actor, CAbilityImmunityTo ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		Log.Trace("OnBeingAttacked");
	}

	public override void OnActorImmuneToCondition(CAbility conditionAbility, CActor target, bool isActorImmune)
	{
		base.OnActorImmuneToCondition(conditionAbility, target, isActorImmune);
		Log.Trace("OnActorImmuneToCondition");
		if (!(m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(target) & isActorImmune))
		{
			return;
		}
		if (!m_ActiveBonus.SetupTriggerContext(this, null, conditionAbility, null, target))
		{
			Finish();
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(target);
		OnBehaviourTriggered();
		CPreventConditionCombatLog_MessageData message = new CPreventConditionCombatLog_MessageData(conditionAbility.TargetingActor, conditionAbility.AbilityType, target);
		ScenarioRuleClient.MessageHandler(message);
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}
}
