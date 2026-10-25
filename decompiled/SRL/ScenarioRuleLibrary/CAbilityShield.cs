using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityShield : CAbilityTargeting
{
	public CAbilityShield()
		: base(EAbilityType.Shield)
	{
	}

	public static CAbility CreateDefaultShield(int strength, int range, int numberOfTargets, bool isMonster, CAbilityFilterContainer abilityFilter = null, string name = "DefaultShield")
	{
		return CAbility.CreateAbility(EAbilityType.Shield, strength, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, (abilityFilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityFilter, "", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", name, new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
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
			CShield_MessageData message = new CShield_MessageData(AnimOverload, TargetingActor)
			{
				m_ShieldAbility = this,
				m_ActorAppliedTo = actor
			};
			ScenarioRuleClient.MessageHandler(message);
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, TargetingActor);
			}
			else if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find base ability card for ability " + Name);
			}
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		if (m_NegativeConditions.Count > 0)
		{
			ProcessNegativeStatusEffects(actor);
		}
		return true;
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
