using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityXP : CAbility
{
	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		SetHasStarted(val: true);
	}

	public override bool Perform()
	{
		base.Perform();
		if (ShowCancelledHelpText())
		{
			return true;
		}
		TargetingActor.GainXP(Strength, XPSourceKey);
		PhaseManager.NextStep();
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}

	public static CAbility CreateDefaultXP(int strength, int range, int numberOfTargets, bool isMonster, CAbilityFilterContainer abilityFilter = null)
	{
		CAbilityFilterContainer abilityFilter2 = ((abilityFilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityFilter);
		List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
		string empty = string.Empty;
		bool isMonster2 = isMonster;
		string empty2 = string.Empty;
		List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
		CAbilityRequirements startAbilityConditions = new CAbilityRequirements();
		AbilityData.MiscAbilityData miscAbilityData = new AbilityData.MiscAbilityData();
		return CAbility.CreateAbility(parentName: string.Empty, miscAbilityData: miscAbilityData, skipAnim: false, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: new List<AbilityData.StatIsBasedOnXData>(), moveObstacleRange: 0, moveObstacleAnimOverload: string.Empty, slotsToRefresh: new List<CItem.EItemSlot>(), slotStatesToRefresh: new List<CItem.EItemSlotState>(), addActiveBonusAbility: null, overrideAugmentAttackType: EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: new List<EAbilityType>(), cardAction: ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), abilityType: EAbilityType.GainXP, strength: strength, useSpecialBaseStat: false, range: range, numberTargets: numberOfTargets, tokens: 0, flags: EAbilityFlag.None, abilityFilter: abilityFilter2, animationOverload: "", animationOverload2: null, subAbilities: null, areaEffect: null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, aiFocusOverride: null, moveRestrictionType: CAbilityMove.EMoveRestrictionType.None, activeBonusData: null, activeBonusYML: null, areaEffectYMLString: "", areaEffectLayoutOverrideYMLString: "", areaEffectFlipAxis: "", name: "DefaultXP", abilityEnhancements: abilityEnhancements, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: EAbilityTargeting.Range, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: isMonster2, propName: empty2, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 1, isTargetedAbility: true, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: conditionalOverrides, startAbilityConditions: startAbilityConditions, abilityXP: 0, consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}
}
