using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEnded : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnActionEnded;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnActionEnded(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnActionEnded(CActor actorEndingAction)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (cActor != actorEndingAction)
		{
			return;
		}
		if (cActor is CPlayerActor cPlayerActor)
		{
			AbilityData.EActiveBonusFlag flags = ActiveBonus.Ability.ActiveBonusData.Flags;
			if ((flags.HasFlag(AbilityData.EActiveBonusFlag.WhenSlow) && cPlayerActor.Agility != CPlayerActor.EAgility.Slow) || (flags.HasFlag(AbilityData.EActiveBonusFlag.WhenFast) && cPlayerActor.Agility != CPlayerActor.EAgility.Fast))
			{
				return;
			}
		}
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonusData.Filter.IsValidTarget(cActor, m_ActiveBonus.Caster, isTargetedAbility: false, useTargetOriginalType: false, false) || !m_ActiveBonus.RequirementsMet(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.ValidToWorkAccordingToTracker)
		{
			if (!m_ActiveBonus.SetupTriggerContext(this, cAbility, null, actorEndingAction, cActor))
			{
				return;
			}
			List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
			((CPhaseAction)PhaseManager.CurrentPhase).StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true);
			m_ActiveBonus.RestrictActiveBonus(cActor);
			OnBehaviourTriggered();
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			m_ActiveBonus.FinishIfNoRemaining();
		}
	}
}
