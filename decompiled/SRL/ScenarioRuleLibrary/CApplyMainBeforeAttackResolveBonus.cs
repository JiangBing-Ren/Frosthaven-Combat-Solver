namespace ScenarioRuleLibrary;

internal class CApplyMainBeforeAttackResolveBonus : CActiveBonus
{
	public CApplyMainBeforeAttackResolveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.ApplyMainBeforeAttackResolve)
			{
				AddBespokeBehaviour(new CApplyMainBeforeAttackResolveBehaviour(actor, ability, this));
			}
		}
	}
}
