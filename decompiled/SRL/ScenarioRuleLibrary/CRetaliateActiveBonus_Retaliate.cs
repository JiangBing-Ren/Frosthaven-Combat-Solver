using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus_Retaliate : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.Retaliate;

	public CRetaliateActiveBonus_Retaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack attackAbility, int modifiedStrength)
	{
		if (IsValidTarget(attackAbility, attackAbility.TargetingActor) && m_ActiveBonus.SetupTriggerContext(this, null, attackAbility, null, m_Actor, ("ModifiedStrength", modifiedStrength)))
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			if (m_ActiveBonus.HasTracker)
			{
				OnBehaviourTriggered();
			}
		}
	}

	public override void OnRetaliate()
	{
		if (m_ActiveBonus.HasTracker && m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public override void OnListenersRefreshed(CActor actor)
	{
		base.OnListenersRefreshed(actor);
		actor.CalculateRetaliate(null, 0);
	}

	public override void CleanUpBeforeSave()
	{
		base.CleanUpBeforeSave();
		if (m_Ability?.StatIsBasedOnXEntries != null && m_Ability.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && !x.AddTo))
		{
			m_Strength = ReferenceStrength(m_Ability, ActiveBonus.Actor);
		}
	}
}
