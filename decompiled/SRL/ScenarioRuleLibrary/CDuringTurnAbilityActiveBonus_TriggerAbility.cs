using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CDuringTurnAbilityActiveBonus_TriggerAbility : CBespokeBehaviour
{
	private CActiveBonus.EActiveBonusBehaviourType m_type;

	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => m_type;

	public CDuringTurnAbilityActiveBonus_TriggerAbility(CActor actor, CAbility ability, CActiveBonus activeBonus, CActiveBonus.EActiveBonusBehaviourType type)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
		m_type = type;
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, CActiveBonus activeBonus, bool toggledOn)
	{
		base.OnActiveBonusToggled(currentAbility, activeBonus, toggledOn);
		CAbility abilityToAddSrc = GetAbilityToAdd(AbilityAddActiveBonus);
		if (toggledOn)
		{
			if (PhaseManager.CurrentPhaseAction != null && PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.ID == abilityToAddSrc.ID))
			{
				return;
			}
			CAbility cAbility = CAbility.CopyAbility(abilityToAddSrc, generateNewID: false);
			if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, currentAbility, null, null))
			{
				return;
			}
			List<CAbility> list = new List<CAbility> { cAbility };
			CActor currentActor = GameState.CurrentActor;
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				CActiveBonus activeBonus2 = m_ActiveBonus;
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true, activeBonus2);
				PhaseManager.StepComplete();
				return;
			}
			if (GameState.InternalCurrentActor != m_ActiveBonus.Caster)
			{
				GameState.OverrideCurrentActorForOneAction(m_ActiveBonus.Caster, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
			}
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true, m_ActiveBonus, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
		}
		else if (!toggledOn && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
		{
			cPhaseAction2.UnstackAbility(abilityToAddSrc, m_ActiveBonus);
		}
	}
}
