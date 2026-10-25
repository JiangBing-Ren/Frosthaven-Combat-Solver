namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus_SingleShotRetaliate : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.SingleShotRetaliate;

	public CRetaliateActiveBonus_SingleShotRetaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnRetaliate()
	{
		Finish();
	}
}
