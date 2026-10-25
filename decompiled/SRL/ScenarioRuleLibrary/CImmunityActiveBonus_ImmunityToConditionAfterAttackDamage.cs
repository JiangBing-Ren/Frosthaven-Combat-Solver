using System.Linq;

namespace ScenarioRuleLibrary;

public class CImmunityActiveBonus_ImmunityToConditionAfterAttackDamage : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ImmunityToConditionAfterAttackDamage;

	public CImmunityActiveBonus_ImmunityToConditionAfterAttackDamage(CActor actor, CAbilityImmunityTo ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttackedPostDamage(CAbilityAttack attackAbility, CAttackSummary.TargetSummary targetSummary)
	{
		base.OnBeingAttackedPostDamage(attackAbility, targetSummary);
		if (!m_ActiveBonus.SetupTriggerContext(this, null, attackAbility, null, null) || ActiveBonus.Finishing())
		{
			return;
		}
		bool isImmune = (m_Ability as CAbilityImmunityTo).ImmuneToAbilityTypes.Any((CAbility.EAbilityType at) => attackAbility.NegativeConditions.Values.FirstOrDefault((CAbility a) => a.AbilityType == at) != null);
		TickIfImmune(isImmune);
	}

	public override void OnActorImmuneToCondition(CAbility conditionAbility, CActor target, bool isActorImmune)
	{
		base.OnActorImmuneToCondition(conditionAbility, target, isActorImmune);
		TickIfImmune(isActorImmune);
	}

	private void TickIfImmune(bool isImmune)
	{
		if (isImmune)
		{
			TickTracker();
		}
	}
}
