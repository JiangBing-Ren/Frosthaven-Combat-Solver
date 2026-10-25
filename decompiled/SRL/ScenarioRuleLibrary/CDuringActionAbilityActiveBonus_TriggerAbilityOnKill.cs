using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnKill : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnKill;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnKill(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnKill(CActor target, CActor actor, CActor.ECauseOfDeath causeOfDeath, CAbility causeOfDeathAbility, bool onActorTurn)
	{
		bool flag = target != null && actor != null && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonusData.Filter.IsValidTarget(target, actor, m_Ability.IsTargetedAbility, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible);
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!(m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) & flag) || !m_ActiveBonus.RequirementsMet(cActor) || causeOfDeath == CActor.ECauseOfDeath.SummonActiveBonusCancelled)
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, causeOfDeathAbility, actor, target, ("CauseOfDeath", TValue.Enum(causeOfDeath)), ("OnActorTurn", onActorTurn)))
		{
			if (m_ActiveBonusData.UseTriggerAbilityAsParent && causeOfDeathAbility != null)
			{
				cAbility.ParentAbility = causeOfDeathAbility;
			}
			if (cAbility.IsInlineSubAbility)
			{
				cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
			}
			CActor currentActor = GameState.CurrentActor;
			List<CAbility> list = new List<CAbility> { cAbility };
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				cPhaseAction.StackNextAbilities(list, cActor, killAfter: false, stackToNextCurrent: true);
			}
			else
			{
				PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: false, null, GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, currentActor);
			}
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
			if (m_ActiveBonus.HasTracker)
			{
				m_ActiveBonus.UpdateXPTracker();
				m_ActiveBonus.FinishIfNoRemaining();
			}
		}
	}
}
