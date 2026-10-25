using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnAbilityEnded;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAbilityEnded(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		base.OnAbilityEnded(endedAbility);
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!IsValidAbilityType(endedAbility) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonus.SetupTriggerContext(this, cAbility, endedAbility, null, cActor))
		{
			if (m_ActiveBonusData.UseTriggerAbilityAsParent)
			{
				cAbility.ParentAbility = endedAbility;
			}
			if (cAbility.IsInlineSubAbility)
			{
				cAbility.InlineSubAbilityTiles = AbilityAddActiveBonus.AddAbility.InlineSubAbilityTiles;
			}
			List<CAbility> nextAbilities = new List<CAbility> { cAbility };
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, cActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true);
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
