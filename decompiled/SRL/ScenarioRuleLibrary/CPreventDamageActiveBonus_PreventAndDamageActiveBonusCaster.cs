namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventAndDamageActiveBonusCaster : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventAndDamageActiveBonusCaster;

	public CPreventDamageActiveBonus_PreventAndDamageActiveBonusCaster(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus == ActiveBonus && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(damagedActor))
		{
			int? casterDamageToTakeOnPrevent = m_Ability.MiscAbilityData.CasterDamageToTakeOnPrevent;
			if (casterDamageToTakeOnPrevent.HasValue && casterDamageToTakeOnPrevent > 0)
			{
				CAbility cAbility = CAbilityDamage.CreateDefaultDamage(casterDamageToTakeOnPrevent.Value, 1, 1, isMonster: false, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbility.AnimOverload = "SoulPact";
				cAbility.MiscAbilityData.AutotriggerAbility = true;
				cAbility.SetMiscVar("InterruptCurrentAbility", true);
				PhaseManager.StartAbilities(cAbility, m_ActiveBonus.Caster, stackInsteadOfInject: true);
			}
			int? preventDamageSetTargetHealthTo = m_Ability.MiscAbilityData.PreventDamageSetTargetHealthTo;
			if (preventDamageSetTargetHealthTo.HasValue)
			{
				damagedActor.ApplyImmediateDamage(damagedActor.PreDamageHealth - preventDamageSetTargetHealthTo.Value, cannotPrevent: true);
			}
			OnBehaviourTriggered();
		}
	}
}
