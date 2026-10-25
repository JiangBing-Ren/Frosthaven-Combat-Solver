using System;

namespace ScenarioRuleLibrary;

internal class CActiveBonus_TriggerOnFinished : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public CActiveBonus_TriggerOnFinished(CActor actor, CAbility ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType behaviourType)
		: base(actor, ability, activeBonus)
	{
		m_type = behaviourType;
	}

	public override void OnFinished()
	{
		CAbility cAbility = CAbility.CopyAbility(m_ActiveBonusData.AbilityData, generateNewID: true, fullCopy: true);
		cAbility.IsScenarioModifierAbility = true;
		GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, m_Actor));
	}
}
