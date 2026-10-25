namespace ScenarioRuleLibrary;

public class CRedirectActiveBonus : CActiveBonus
{
	public CRedirectActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}

	public virtual CActor ApplyRedirect(CAbility ability, CActor actor)
	{
		if (ability.AbilityType != CAbility.EAbilityType.Attack)
		{
			return actor;
		}
		return Actor;
	}
}
