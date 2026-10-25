namespace ScenarioRuleLibrary;

public class CMoveActiveBonus_BuffForcedMovement : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffForcedMovement;

	public CMoveActiveBonus_BuffForcedMovement(CActor actor, CAbilityMove ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		CAbilityMove cAbilityMove = ability as CAbilityMove;
		if (ability is CAbilityPull || ability is CAbilityPush || (cAbilityMove != null && cAbilityMove.IsControlAbility))
		{
			if (cAbilityMove != null && cAbilityMove.IsControlAbility && (cAbilityMove.ControllingActor != m_Actor || (cAbilityMove.ControllingActor == m_Actor && cAbilityMove.TargetingActor == m_Actor) || CustomAbility.IsAlly(target, m_Actor, checkOriginalType: true)))
			{
				return 0;
			}
			if (ability.GetMiscVar("IgnoreForcedMoveBuffs").GetBool())
			{
				return 0;
			}
			TValue var = ActiveBonus.Ability.ActiveBonusData.GetVar("BuffValue");
			if (var.isValid)
			{
				return var.GetInt();
			}
		}
		return 0;
	}

	public override void OnFinished()
	{
	}
}
