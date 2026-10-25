using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityImmunityTo : CAbilityTargeting
{
	[JsonProperty]
	public List<EAbilityType> ImmuneToAbilityTypes { get; set; }

	[JsonProperty]
	public List<EAttackType> ImmuneToAttackTypes { get; set; }

	[JsonProperty]
	public bool ImmuneToSelfDamage { get; set; }

	public bool NotImmuneToSelf => GetMiscVar("NotImmuneToSelf").GetBool();

	public CAbilityImmunityTo(List<EAbilityType> immuneToAbilityTypes, List<EAttackType> immuneToAttackTypes, bool immuneToSelfDamage)
		: base(EAbilityType.ImmunityTo)
	{
		ImmuneToAbilityTypes = immuneToAbilityTypes;
		ImmuneToAttackTypes = immuneToAttackTypes;
		ImmuneToSelfDamage = immuneToSelfDamage;
	}

	public static CAbility CreateDefaultImmunityTo(List<EAbilityType> immuneToAbilityTypes, List<EAttackType> immuneToAttackTypes, bool immuneToSelfDamage)
	{
		CAbilityFilterContainer abilityFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
		List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
		string empty = string.Empty;
		string empty2 = string.Empty;
		List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
		CAbilityRequirements startAbilityConditions = new CAbilityRequirements();
		string empty3 = string.Empty;
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>();
		string empty4 = string.Empty;
		List<CItem.EItemSlot> slotsToRefresh = new List<CItem.EItemSlot>();
		List<CItem.EItemSlotState> slotStatesToRefresh = new List<CItem.EItemSlotState>();
		List<EAbilityType> recoverCardsAbilityTypeFilter = new List<EAbilityType>();
		bool immuneToSelfDamage2 = immuneToSelfDamage;
		return CAbility.CreateAbility(EAbilityType.ImmunityTo, 0, useSpecialBaseStat: false, 1, 0, 0, EAbilityFlag.None, abilityFilter, "", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultImmunityTo", abilityEnhancements, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster: false, empty2, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, conditionalOverrides, startAbilityConditions, 0, empty3, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, statIsBasedOnXEntries, 0, empty4, slotsToRefresh, slotStatesToRefresh, null, EAttackType.None, null, recoverCardsAbilityTypeFilter, ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, new List<AbilityConsume>(), null, immuneToAbilityTypes, immuneToAttackTypes, immuneToSelfDamage2, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
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
			GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
			AbilityHasHappened = true;
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
