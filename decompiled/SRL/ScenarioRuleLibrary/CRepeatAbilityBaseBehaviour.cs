namespace ScenarioRuleLibrary;

public class CRepeatAbilityBaseBehaviour : CBespokeBehaviour
{
	private CAbility _curRepeatingAbility;

	private CAbilityOverride _curRepeatingOverride;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.RepeatAbility;

	public CRepeatAbilityBaseBehaviour(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		base.OnAbilityEnded(endedAbility);
		if (endedAbility == m_Ability)
		{
			return;
		}
		CRepeatAbilityActiveBonus cRepeatAbilityActiveBonus = m_ActiveBonus as CRepeatAbilityActiveBonus;
		if (endedAbility.StartAbilityRequirements == null || !cRepeatAbilityActiveBonus.IsAgilityValid(endedAbility.StartAbilityRequirements.RequiredAgility) || !cRepeatAbilityActiveBonus.RequirementsMet(m_Actor))
		{
			return;
		}
		if (_curRepeatingAbility == null)
		{
			if (PhaseManager.Phase is CPhaseAction cPhaseAction)
			{
				_curRepeatingAbility = CAbility.CopyAbility(endedAbility.AbilityInitialState, generateNewID: false);
				_curRepeatingAbility.ActiveBonusYML = endedAbility.ActiveBonusYML;
				_curRepeatingAbility.Name = endedAbility.Name + "_Duplicate";
				_curRepeatingAbility.AbilityInitialState = endedAbility.AbilityInitialState;
				_curRepeatingAbility.TargetingActor = endedAbility.TargetingActor;
				_curRepeatingAbility.ParentAbility = endedAbility.ParentAbility;
				cPhaseAction.InjectAbility(_curRepeatingAbility, _curRepeatingAbility.TargetingActor, _curRepeatingAbility.GetAbilityBaseCard());
			}
		}
		else if (endedAbility.ID == _curRepeatingAbility.ID)
		{
			_curRepeatingAbility = null;
		}
	}

	public override void OnAbilityOverrideApplied(CActor actor, CAbilityOverride abilityOverride, CAbility ability, bool active)
	{
		if (_curRepeatingOverride != null)
		{
			return;
		}
		base.OnAbilityOverrideApplied(actor, abilityOverride, ability, active);
		CRepeatAbilityActiveBonus cRepeatAbilityActiveBonus = m_ActiveBonus as CRepeatAbilityActiveBonus;
		if (abilityOverride?.OriginalConditionalOverride != null && (!active || cRepeatAbilityActiveBonus.RequirementsMet(m_Actor)) && cRepeatAbilityActiveBonus.IsAgilityValid(abilityOverride.OriginalConditionalOverride.Requirements.RequiredAgility))
		{
			_curRepeatingOverride = CAbilityOverride.CopyAbilityOverride(abilityOverride, newGuid: true);
			if (active)
			{
				ability.OverrideAbilityValues(_curRepeatingOverride, perform: false);
			}
			else
			{
				ability.UndoOverride(_curRepeatingOverride, perform: false);
			}
			_curRepeatingOverride = null;
		}
	}
}
