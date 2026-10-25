using System;

namespace ScenarioRuleLibrary;

public class CShieldActiveBonus : CActiveBonus
{
	public CShieldActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.ShieldIncomingAttacks:
			case EActiveBonusBehaviourType.BuffShield:
				if (ability is CAbilityShield ability4)
				{
					AddBespokeBehaviour(new CShieldActiveBonus_BuffShield(actor, ability4, this, behaviour));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
			case EActiveBonusBehaviourType.ShieldAndRetaliate:
				if (ability is CAbilityShield ability3)
				{
					AddBespokeBehaviour(new CShieldActiveBonus_ShieldAndRetaliate(actor, ability3, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
			case EActiveBonusBehaviourType.ShieldTargetedAttacks:
				if (ability is CAbilityShield ability5)
				{
					AddBespokeBehaviour(new CShieldActiveBonus_TargetedShield(actor, ability5, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
			case EActiveBonusBehaviourType.ShieldAndDisadvantage:
				if (ability is CAbilityShield ability2)
				{
					AddBespokeBehaviour(new CShieldActiveBonus_ShieldAndDisadvantage(actor, ability2, this));
					break;
				}
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
			}
		}
	}

	public override void TriggerPreventDamage(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		if (IsActiveBonusToggledAndNotRestricted(actorDamaged))
		{
			actorDamaged.m_OnPreventDamageListeners?.Invoke(damagePrevented, damageSource, actorDamaged, damagingAbility, this);
		}
	}
}
