using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CChooseAbilityActiveBonus_EndOfActionAbilityChoice : CBespokeBehaviour
{
	private CChooseAbilityActiveBonus ChooseAbilityActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.EndActionAbility;

	public CChooseAbilityActiveBonus_EndOfActionAbilityChoice(CActor actor, CAbility ability, CChooseAbilityActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		ChooseAbilityActiveBonus = activeBonus;
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(endedAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(endedAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnActiveBonusToggled(CActor currentActor, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentActor, activeBonus, toggledOn);
		if (toggledOn)
		{
			CAbility cAbility = CAbility.CopyAbility(ChooseAbilityActiveBonus.chosenAbility, generateNewID: false);
			if (m_ActiveBonus.SetupTriggerContext(this, cAbility, null, currentActor, null))
			{
				List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, currentActor, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: true, m_ActiveBonus);
			}
		}
		else if (!toggledOn)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).UnstackAbility(ChooseAbilityActiveBonus.chosenAbility, m_ActiveBonus);
		}
	}
}
