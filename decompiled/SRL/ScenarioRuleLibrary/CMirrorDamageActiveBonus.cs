namespace ScenarioRuleLibrary;

public class CMirrorDamageActiveBonus : CActiveBonus
{
	public CMirrorDamageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.MirrorAll:
				AddBespokeBehaviour(new CMirrorDamageActiveBonus_MirrorAll(actor, caster, ability, this));
				break;
			case EActiveBonusBehaviourType.MirrorAbilities:
				AddBespokeBehaviour(new CMirrorDamageActiveBonus_MirrorAbilities(actor, caster, ability, this));
				break;
			}
		}
	}
}
