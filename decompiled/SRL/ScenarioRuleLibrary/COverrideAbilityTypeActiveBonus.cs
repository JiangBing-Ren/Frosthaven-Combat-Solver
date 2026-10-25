namespace ScenarioRuleLibrary;

internal class COverrideAbilityTypeActiveBonus : CActiveBonus
{
	public COverrideAbilityTypeActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			if (behaviour == EActiveBonusBehaviourType.OverrideAbility || behaviour == EActiveBonusBehaviourType.OverrideMoveAbility)
			{
				AddBespokeBehaviour(new COverrideAbilityTypeActiveBonus_OverrideAbility(actor, ability, this, behaviour));
			}
		}
	}
}
