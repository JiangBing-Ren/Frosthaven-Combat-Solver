using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnDamaged : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnDamaged;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnDamaged(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnDamaged(GameState.DamageData damageData)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : damageData.ActorDamaged);
		base.OnDamaged(damageData);
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, damageData.DamageSourceAbility, damageData.ActorDamaged, cActor, ("DamageAmount", damageData.PreDamageHealth - damageData.ActorDamaged.Health)))
		{
			return;
		}
		if (ActiveBonus.Ability.ActiveBonusData.GetMiscVar("ProcessIfDead").GetBool())
		{
			cAbility.ProcessIfDead = true;
		}
		cAbility.SetCanUndo(canUndo: false);
		if (cAbility.IsInlineSubAbility)
		{
			cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[damageData.ActorDamaged.ArrayIndex.X, damageData.ActorDamaged.ArrayIndex.Y]);
		}
		List<CAbility> list = new List<CAbility> { cAbility };
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			if (m_ActiveBonus.TriggerAbilitiesImmediately)
			{
				cPhaseAction.StackNextAbilities(list, cActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
			}
			else
			{
				cPhaseAction.StackNextAbilities(list, cActor);
			}
		}
		else if (PhaseManager.CurrentPhase.Type != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			CActor currentActor = GameState.CurrentActor;
			if (GameState.InternalCurrentActor != cActor)
			{
				GameState.OverrideCurrentActorForOneAction(cActor, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
			}
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true, null, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
		}
		m_ActiveBonus.RestrictActiveBonus(cActor);
		OnBehaviourTriggered();
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}
}
