using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbility : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbility;

	public CDuringActionAbilityActiveBonus_TriggerAbility(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentAbility, activeBonus, toggledOn);
		if (IsValidAbilityType(currentAbility) & toggledOn)
		{
			CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
			if (m_ActiveBonus.SetupTriggerContext(this, cAbility, currentAbility, null, null))
			{
				List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
				CPhaseAction obj = PhaseManager.CurrentPhase as CPhaseAction;
				CActiveBonus activeBonus2 = m_ActiveBonus;
				obj.StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true, activeBonus2);
			}
		}
		else if (!toggledOn)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(AbilityAddActiveBonus.AddAbility, m_ActiveBonus);
		}
	}
}
