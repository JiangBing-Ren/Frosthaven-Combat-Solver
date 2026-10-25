using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityAfterDeath : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityAfterDeath;

	public CDuringActionAbilityActiveBonus_TriggerAbilityAfterDeath(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void AfterDeath(CActor actor, CAbility causeOfDeathAbility)
	{
		CActor actorToTriggerOn = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : actor);
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(actor) || !m_ActiveBonus.RequirementsMet(actor))
		{
			return;
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		cAbility.TargetingActor = AbilityAddActiveBonus.TargetingActor;
		cAbility.ProcessIfDead = true;
		if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, causeOfDeathAbility, actor, actorToTriggerOn))
		{
			return;
		}
		if (m_ActiveBonusData.UseTriggerAbilityAsParent)
		{
			if (causeOfDeathAbility != null)
			{
				cAbility.ParentAbility = causeOfDeathAbility;
			}
			else
			{
				cAbility.TargetThisActorAutomatically = actor;
			}
		}
		if (cAbility.IsInlineSubAbility)
		{
			cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
		}
		List<CAbility> list = new List<CAbility> { cAbility };
		m_ActiveBonus.StackXPToAbilities(list);
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			if (m_Ability.Targeting == CAbility.EAbilityTargeting.All && m_Ability.NumberTargets == -1)
			{
				m_Ability.TargetingActor = m_Actor;
				foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x != actorToTriggerOn))
				{
					if (!IsValidTarget(m_Ability, item))
					{
						continue;
					}
					if (item.IsDead)
					{
						bool immediatelySetCurrentActor = !m_ActiveBonus.TriggerAbilitiesImmediately;
						GameState.OverrideCurrentActorForOneAction(item, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor);
						if (m_ActiveBonus.TriggerAbilitiesImmediately)
						{
							cPhaseAction.StackNextAbilities(list, item, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
						}
						else
						{
							cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
						}
					}
					else if (m_ActiveBonus.TriggerAbilitiesImmediately)
					{
						cPhaseAction.StackNextAbilities(list, item, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
					}
					else
					{
						cPhaseAction.StackNextAbilities(list, item);
					}
				}
			}
			else if (actorToTriggerOn.IsDead)
			{
				CActor actor2 = actorToTriggerOn;
				bool immediatelySetCurrentActor = !m_ActiveBonus.TriggerAbilitiesImmediately;
				GameState.OverrideCurrentActorForOneAction(actor2, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor);
				if (m_ActiveBonus.TriggerAbilitiesImmediately)
				{
					cPhaseAction.StackNextAbilities(list, actorToTriggerOn, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
				}
				else
				{
					cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				}
			}
			else if (m_ActiveBonus.TriggerAbilitiesImmediately)
			{
				cPhaseAction.StackNextAbilities(list, actorToTriggerOn, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true);
			}
			else
			{
				cPhaseAction.StackNextAbilities(list, actorToTriggerOn);
			}
		}
		else
		{
			CActor currentActor = GameState.CurrentActor;
			if (m_Ability.Targeting == CAbility.EAbilityTargeting.All && m_Ability.NumberTargets == -1)
			{
				m_Ability.TargetingActor = m_Actor;
				foreach (CActor item2 in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x != actorToTriggerOn))
				{
					if (!IsValidTarget(m_Ability, item2) || item2.IsDead)
					{
						continue;
					}
					if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
					{
						cPhaseAction2.StackNextAbilities(list, item2);
						continue;
					}
					if (GameState.InternalCurrentActor != item2)
					{
						GameState.OverrideCurrentActorForOneAction(item2, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
					}
					PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true);
				}
			}
			else
			{
				if (GameState.InternalCurrentActor != actorToTriggerOn)
				{
					GameState.OverrideCurrentActorForOneAction(actorToTriggerOn, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
				}
				PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true, null, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
			}
		}
		m_ActiveBonus.RestrictActiveBonus(actorToTriggerOn);
		OnBehaviourTriggered();
		if (m_ActiveBonus.HasTracker && cAbility.MeetsStartAbilityRequirements(GameState.InternalCurrentActor))
		{
			m_ActiveBonus.UpdateXPTracker(processTracker: true, processXP: false);
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}
}
