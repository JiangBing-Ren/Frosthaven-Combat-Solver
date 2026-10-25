namespace ScenarioRuleLibrary;

public class CMoveActiveBonus_ShadowBuff : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ShadowBuff;

	public CMoveActiveBonus_ShadowBuff(CActor actor, CAbilityMove ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnFinished()
	{
	}
}
