using System;

namespace ScenarioRuleLibrary;

public class CMoveActiveBonus : CActiveBonus
{
	public CMoveActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (!(ability is CAbilityMove ability2))
		{
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus MoveBuffStrength");
		}
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.BuffMove:
				AddBespokeBehaviour(new CMoveActiveBonus_BuffMove(actor, ability2, this));
				actor.RefreshFlyingFlag();
				break;
			case EActiveBonusBehaviourType.ShadowBuff:
				AddBespokeBehaviour(new CMoveActiveBonus_ShadowBuff(actor, ability2, this));
				break;
			case EActiveBonusBehaviourType.BuffForcedMovement:
				AddBespokeBehaviour(new CMoveActiveBonus_BuffForcedMovement(actor, ability2, this));
				break;
			case EActiveBonusBehaviourType.MoveScript:
				AddBespokeBehaviour(new CMoveActiveBonus_MoveScript(actor, ability2, this));
				break;
			}
		}
	}
}
