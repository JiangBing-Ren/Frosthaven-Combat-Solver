namespace ScenarioRuleLibrary;

public class CMirrorDamageActiveBonus_MirrorAll : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.MirrorAll;

	public CActor MirrorTarget { get; set; }

	public CMirrorDamageActiveBonus_MirrorAll(CActor actor, CActor caster, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		MirrorTarget = caster;
	}

	public override void OnTakenDamage(int damageTaken, CAbility damagingAbility, int damageReducedByShields, int actualDamage, CActor damageSourceActor)
	{
		Log.Trace("on damage taken triggered");
		if (damagingAbility != null || !ActiveBonus.Ability.ActiveBonusData.GetMiscVar("IgnoreNonAbilities").GetBool())
		{
			if (MirrorTarget is CPlayerActor)
			{
				GameState.ActorBeenDamaged(MirrorTarget, damageTaken, checkIfPlayerCanAvoidDamage: true, m_Actor, damagingAbility, damagingAbility?.AbilityType ?? CAbility.EAbilityType.Damage, damageReducedByShields, isTrapDamage: false, isTerrainDamage: false, cannotPreventDamageWithActiveBonus: true);
				m_Actor.Healed(MirrorTarget.Health - m_Actor.Health, ignoreTokens: true, report: false, actualHeal: false, triggerGenericHealFinished: false);
			}
			else
			{
				int health = MirrorTarget.Health;
				MirrorTarget.Health = m_Actor.Health;
				CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(MirrorTarget)
				{
					m_ActorBeingDamaged = MirrorTarget,
					m_DamageAbility = null,
					m_ActorOriginalHealth = health,
					m_ActorWasAsleep = false
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			GameState.ActorHealthCheck(MirrorTarget, MirrorTarget);
		}
	}

	public override void OnConditionApplyToActor(CAbility conditionAbility, CActor target)
	{
		if (target == ActiveBonus.Actor && conditionAbility is CAbilityCondition)
		{
			CAbility cAbility = CAbility.CopyAbility(conditionAbility, generateNewID: false);
			if (m_ActiveBonus.SetupTriggerContext(this, cAbility, conditionAbility, null, target))
			{
				cAbility.PositiveConditions.Clear();
				cAbility.NegativeConditions.Clear();
				cAbility.IsScenarioModifierAbility = true;
				cAbility.Start(target, target);
				cAbility.ApplyToActor(MirrorTarget);
			}
		}
	}

	public override void OnPositiveConditionEndedOnActor(CCondition.EPositiveCondition positiveConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
		MirrorTarget.RemovePositiveConditionToken(positiveConditionType);
	}

	public override void OnNegativeConditionEndedOnActor(CCondition.ENegativeCondition negativeConditionType, CActor target, EConditionDecTrigger decTrigger)
	{
		MirrorTarget.RemoveNegativeConditionToken(negativeConditionType);
	}

	public override void OnBeingHealed(CAbilityHeal healAbility)
	{
	}

	public override void OnAfterBeingHealed()
	{
		Log.Trace("on after healed triggered");
		int health = MirrorTarget.Health;
		MirrorTarget.Healed(m_Actor.Health - MirrorTarget.Health, null, m_Actor, out var baneRemoved, out var woundRemoved, out var poisonRemoved, out var brittleRemoved, out var healedAmount);
		GameState.ActorHealthCheck(MirrorTarget, MirrorTarget);
		CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(m_Actor, MirrorTarget, (!poisonRemoved) ? healedAmount : 0, health, poisonRemoved, woundRemoved, baneRemoved, brittleRemoved);
		ScenarioRuleClient.MessageHandler(message);
	}
}
