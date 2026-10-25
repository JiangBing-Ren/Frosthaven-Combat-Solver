using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus : CActiveBonus
{
	public bool PreventDamageAttackSourcesOnly { get; set; }

	public bool PreventOnlyIfLethal { get; set; }

	public CPreventDamageActiveBonus(CBaseCard baseCard, CAbilityPreventDamage ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		PreventDamageAttackSourcesOnly = ability.AttackSourcesOnly;
		AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
		PreventOnlyIfLethal = miscAbilityData != null && miscAbilityData.PreventOnlyIfLethal == true;
		foreach (EActiveBonusBehaviourType behaviour in ability.ActiveBonusData.Behaviours)
		{
			switch (behaviour)
			{
			case EActiveBonusBehaviourType.BlockRetaliate:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_BlockRetaliate(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventAndRetaliate:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventAndRetaliate(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventAndApplyToActiveBonusCaster:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventAndApplyToActiveBonusCaster(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventDamageByDiscardingPlayerCards:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventDamageByDiscardingPlayerCards(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventAndFullyHeal:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventDamageAndFullyHeal(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventFirstDamage:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventFirstDamage(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventAndDamageActiveBonusCaster:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventAndDamageActiveBonusCaster(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventPartialDamage:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventPartialDamage(actor, ability, this));
				break;
			case EActiveBonusBehaviourType.PreventBySettingRetaliateToValue:
				AddBespokeBehaviour(new CPreventDamageActiveBonus_PreventBySettingRetaliateToValue(actor, ability, this));
				break;
			}
		}
	}

	public override void TriggerPreventDamage(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		if (!IsActiveBonusToggledAndNotRestricted(actorDamaged))
		{
			return;
		}
		RestrictActiveBonus(Actor);
		foreach (ElementInfusionBoardManager.EElement item in Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, Actor);
		}
		actorDamaged.m_OnPreventDamageListeners?.Invoke(damagePrevented, damageSource, actorDamaged, damagingAbility, this);
		CPreventDamageTriggered_MessageData cPreventDamageTriggered_MessageData = new CPreventDamageTriggered_MessageData("", Actor);
		cPreventDamageTriggered_MessageData.m_PreventDamageBaseCard = Ability.GetAbilityBaseCard();
		cPreventDamageTriggered_MessageData.m_OverridePreventedDamage = ((GameState.CurrentDamageData != null && GameState.CurrentDamageData.DamageAvoided > 0 && HasBehaviour(EActiveBonusBehaviourType.PreventPartialDamage)) ? GameState.CurrentDamageData.DamageAvoided : (-1));
		CPreventDamageTriggered_MessageData message = cPreventDamageTriggered_MessageData;
		ScenarioRuleClient.MessageHandler(message);
		if (Ability.ActiveBonusData.IsToggleBonus)
		{
			ToggledBonus = false;
			ToggleLocked = false;
			ToggledElement = null;
		}
		if (HasTracker)
		{
			UpdateXPTracker();
			FinishIfNoRemaining();
		}
	}

	public override bool ShouldRestartAbilityOnToggle(CAbility currentAbility)
	{
		return false;
	}
}
