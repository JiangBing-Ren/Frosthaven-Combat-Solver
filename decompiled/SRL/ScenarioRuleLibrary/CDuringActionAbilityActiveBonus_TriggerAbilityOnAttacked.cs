using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAttacked;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnBeingAttackedPostDamage(CAbilityAttack attackAbility, CAttackSummary.TargetSummary targetSummary)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : targetSummary.ActorToAttack);
		if (!IsValidAbilityType(attackAbility) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonus.RequirementsMet(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, attackAbility, null, cActor, ("DamageTaken", targetSummary.ActualDamageTaken)))
		{
			if (cAbility.IsInlineSubAbility)
			{
				cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[attackAbility.TargetingActor.ArrayIndex.X, attackAbility.TargetingActor.ArrayIndex.Y]);
			}
			List<CAbility> list = new List<CAbility> { cAbility };
			CActor currentActor = GameState.CurrentActor;
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				cPhaseAction.StackNextAbilities(list, cActor, killAfter: false, stackToNextCurrent: true);
			}
			else
			{
				PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: false, null, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
			}
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker && !m_ActiveBonus.Ability.ActiveBonusData.GetVar("DontIncTracker").GetBool())
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}
}
