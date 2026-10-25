using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnCreated : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnCreated;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnCreated(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnCreated(CAbility endedAbility)
	{
		base.OnCreated(endedAbility);
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
			List<CAbility> inlineSubAbilities = new List<CAbility> { cAbility };
			(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(inlineSubAbilities, null, performNow: true, stopPlayerSkipping: false, null, stopPlayerUndo: true);
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
