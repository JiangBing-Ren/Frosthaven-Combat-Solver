using System;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventPartialDamage : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventPartialDamage;

	public CPreventDamageActiveBonus_PreventPartialDamage(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor actorDamaged, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus == ActiveBonus)
		{
			base.OnPreventDamageTriggered(preventedDamage, damageSource, actorDamaged, damagingAbility, preventedByBonus);
			if (m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, actorDamaged, ("PreventedDamage", preventedDamage)))
			{
				OnBehaviourTriggered();
				int num = Math.Min(preventedDamage, ActiveBonus.Ability.Strength);
				GameState.CurrentDamageData.DamageAvoided += num;
				GameState.CurrentDamageData.DamageSourceStrength = Math.Max(0, GameState.CurrentDamageData.DamageSourceStrength - num);
				Log.SimpleInfo(GameState.CurrentDamageData.ActorDamaged.Class.ID + " prevented " + num + " damage");
				GameState.CurrentDamageData.ActorDamaged.Healed(num, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
			}
		}
	}
}
