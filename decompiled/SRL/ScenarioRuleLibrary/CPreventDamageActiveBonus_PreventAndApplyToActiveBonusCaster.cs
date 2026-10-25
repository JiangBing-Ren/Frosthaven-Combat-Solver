using System;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.PreventAndApplyToActiveBonusCaster;

	public CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility, CActiveBonus preventedByBonus)
	{
		if (preventedByBonus != ActiveBonus || !ActiveBonus.IsActiveBonusToggledAndNotRestricted(damagedActor))
		{
			return;
		}
		int num = Math.Max(0, damagePrevented - m_Ability.Strength);
		if (!m_ActiveBonus.SetupTriggerContext(this, null, damagingAbility, damageSource, damagedActor, ("DamagePrevented", damagePrevented)))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(damagedActor);
		if (num > 0)
		{
			GameState.RedirectedDamageToActor = new Tuple<CActor, int>(m_ActiveBonus.Caster, num);
			if (m_ActiveBonus?.Ability?.ActiveBonusData?.MiscVars != null)
			{
				(string, TValue) tuple = m_ActiveBonus.Ability.ActiveBonusData.MiscVars.Find(((string, TValue) p) => p.Item1 == "RedirectAnim");
				(string, TValue) tuple2 = tuple;
				if (tuple2.Item1 != null || tuple2.Item2 != default(TValue))
				{
					CRedirectDamageAnimated_MessageData message = new CRedirectDamageAnimated_MessageData(tuple.Item2.GetString(), m_ActiveBonus.Caster)
					{
						m_RedirectingActor = m_ActiveBonus.Caster,
						m_TargetedActor = damagedActor
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
		OnBehaviourTriggered();
	}
}
