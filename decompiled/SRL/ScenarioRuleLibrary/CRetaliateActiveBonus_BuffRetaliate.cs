namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus_BuffRetaliate : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BuffRetaliate;

	public CRetaliateActiveBonus_BuffRetaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}
}
