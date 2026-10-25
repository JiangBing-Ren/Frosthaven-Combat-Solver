using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityNullTargeting : CAbilityTargeting
{
	public CAbilityNullTargeting()
		: base(EAbilityType.NullTargeting)
	{
	}

	public static CAbility CreateDefaultNullTargeting(int range, int numberOfTargets, bool isMonster, string animationOverload = "", CAbilityFilterContainer abilityFilter = null, List<CCondition.EPositiveCondition> positiveCnditions = null, List<CCondition.ENegativeCondition> negativeConditions = null, string name = "DefaultNullTargeting")
	{
		return CAbility.CreateAbility(EAbilityType.NullTargeting, 0, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, (abilityFilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityFilter, animationOverload, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", name, new List<CEnhancement>(), positiveCnditions, negativeConditions, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (GetMiscVar("DontAddActiveBonus").GetBool())
			{
				return true;
			}
			CActiveBonus cActiveBonus = null;
			if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				cActiveBonus = GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
			}
			if (cActiveBonus != null && cActiveBonus.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				CLogActiveBonus_MessageData message = new CLogActiveBonus_MessageData(AnimOverload, TargetingActor)
				{
					m_ActiveBonus = cActiveBonus,
					m_ActorAppliedTo = actor
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (cActiveBonus == null || !cActiveBonus.IsAura)
			{
				ProcessPositiveStatusEffects(actor);
				ProcessNegativeStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return !GetMiscVar("IsNegative").GetBool();
	}
}
