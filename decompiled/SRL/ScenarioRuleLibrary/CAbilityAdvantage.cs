using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityAdvantage : CAbilityCondition
{
	public CAbilityAdvantage(int duration, EConditionDecTrigger decTrigger)
		: base(EAbilityType.Advantage, duration, decTrigger)
	{
	}

	public static CAbilityAdvantage CreateDefaultAdvantageAbility()
	{
		return CAbility.CreateAbility(EAbilityType.Advantage, 1, useSpecialBaseStat: false, 1, 1, 0, EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, new AbilityData.ActiveBonusData
		{
			Duration = CActiveBonus.EActiveBonusDurationType.Round
		}, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 1, EConditionDecTrigger.Rounds, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, EAttackType.None, null, null, ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue) as CAbilityAdvantage;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			LogApplyToActor(actor);
			AbilityHasHappened = true;
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.AlreadyHasConditionDamageInstead.HasValue && actor.Tokens.HasKey(CCondition.EPositiveCondition.Advantage))
			{
				int health = actor.Health;
				bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(actor, MiscAbilityData.AlreadyHasConditionDamageInstead.Value, checkIfPlayerCanAvoidDamage: true, TargetingActor, this, EAbilityType.Damage);
				if ((actor.Type != CActor.EType.Player || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(actor)
					{
						m_ActorBeingDamaged = actor,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else
			{
				actor.Tokens.AddPositiveToken(CCondition.EPositiveCondition.Advantage, Duration, DecrementTrigger, actor, TargetingActor);
				CAdvantage_MessageData message2 = new CAdvantage_MessageData(AnimOverload, TargetingActor)
				{
					m_AdvantageAbility = this,
					m_AdvantagedActor = actor
				};
				ScenarioRuleClient.MessageHandler(message2);
				if (ActiveBonusYML != null)
				{
					GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
				}
				if (m_PositiveConditions.Count > 0)
				{
					ProcessPositiveStatusEffects(actor);
				}
			}
			return true;
		}
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
