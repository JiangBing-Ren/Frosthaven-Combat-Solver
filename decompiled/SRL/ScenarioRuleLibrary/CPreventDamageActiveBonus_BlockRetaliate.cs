namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_BlockRetaliate : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.BlockRetaliate;

	public CPreventDamageActiveBonus_BlockRetaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}
}
