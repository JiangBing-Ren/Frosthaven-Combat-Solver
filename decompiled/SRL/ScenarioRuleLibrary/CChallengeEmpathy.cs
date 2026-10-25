using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeEmpathy : CChallenge
{
	public override uint ID => 1483u;

	public override string Name => "empathy";

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnEnemySufferDamage;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Attack, 0, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData
		{
			AttackHasAdvantage = true
		}, activeBonusData: new AbilityData.ActiveBonusData
		{
			Duration = CActiveBonus.EActiveBonusDurationType.Round,
			Behaviour = CActiveBonus.EActiveBonusBehaviourType.BuffAttack,
			ListLayoutOverride = ("$024_character_ability_cards_drifter_accurate_strikes_3$", "Positive_Advantage")
		}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
		cAbility.Name = "EmpathyChallengeAdvantage";
		CEnemyActor cEnemyActor = triggerParams.actor as CEnemyActor;
		if (cEnemyActor.Type == CActor.EType.Ally)
		{
			return;
		}
		foreach (CEnemyActor allEnemy in ScenarioManager.Scenario.AllEnemies)
		{
			if (allEnemy != null && allEnemy != cEnemyActor && !allEnemy.IsDead && !allEnemy.ActiveBonuses.Any((CActiveBonus x) => x.Ability.Name == "EmpathyChallengeAdvantage"))
			{
				CAbility cAbility2 = CAbility.CopyAbility(cAbility, generateNewID: true);
				cAbility2.CachedAbilityBaseCard = CBaseCard.CreateDummy("1483_challenge_cards_empathy_1", 0, null, allEnemy);
				cAbility2.Start(allEnemy, allEnemy);
				cAbility2.ApplyToActor(allEnemy);
			}
		}
	}
}
