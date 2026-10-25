using System.Linq;

namespace ScenarioRuleLibrary;

public class CAbilityDuplicateCondition : CAbilityTargeting
{
	public CAbilityDuplicateCondition(EAbilityType abilityType)
		: base(abilityType)
	{
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			if (m_State != TargetingState.ActorIsSelectingTargetingFocus)
			{
				return ProcessVarsStatusEffects(actor);
			}
			if (PhaseManager.Phase is CPhaseAction cPhaseAction)
			{
				cPhaseAction.InjectAbility(this, TargetingActor, GetAbilityBaseCard());
			}
		}
		return false;
	}

	public bool ProcessVarsStatusEffects(CActor target)
	{
		if (VarsContext == null)
		{
			return false;
		}
		CAbilityCondition condition = VarsContext.GetVar("ConditionFromVars").GetObject<CAbilityCondition>();
		if (condition == null)
		{
			return false;
		}
		if (condition.IsPositive())
		{
			CCondition.EPositiveCondition condition2 = CCondition.PositiveConditions.Single((CCondition.EPositiveCondition x) => x.ToString().Equals(condition.AbilityType.ToString()));
			target.ApplyCondition(TargetingActor, condition2, condition.Duration, condition.DecrementTrigger);
		}
		else
		{
			CCondition.ENegativeCondition condition3 = CCondition.NegativeConditions.Single((CCondition.ENegativeCondition x) => x.ToString().Equals(condition.AbilityType.ToString()));
			target.ApplyCondition(TargetingActor, condition3);
		}
		return false;
	}
}
