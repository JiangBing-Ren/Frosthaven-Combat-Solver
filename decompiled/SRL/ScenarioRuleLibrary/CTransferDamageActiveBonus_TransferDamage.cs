namespace ScenarioRuleLibrary;

public class CTransferDamageActiveBonus_TransferDamage : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.MirrorAll;

	public CTransferDamageActiveBonus_TransferDamage(CActor actor, CActor caster, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}
}
