using System.Collections.Generic;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeHealthyVigor : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1524u;

	public override string Name => "healthy_vigor";

	public override void Reset()
	{
		base.Reset();
		actorsAppliedTo.Clear();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			actorsAppliedTo = JsonConvert.DeserializeObject<List<string>>(storage);
		}
	}

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnSpawn;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(actorsAppliedTo);
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor != null && triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
		{
			actorsAppliedTo.Add(cEnemyActor.ActorGuid);
			CAbilityShield obj = CAbilityShield.CreateDefaultShield(1, 1, 1, isMonster: true, null, "HealthyVigorChallengeShield") as CAbilityShield;
			obj.ActiveBonusData.Hidden = true;
			obj.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			obj.ActiveBonusData.IsAura = true;
			obj.ActiveBonusData.AuraFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
			obj.ActiveBonusData.Requirements = new CAbilityRequirements
			{
				Vars = new List<(string, TValue)> { ("ValidateHasNoConditions", new TValue(CExpression.Parse("Actor.Tokens.GetAllTransferableConditions().Count == 0"))) }
			};
			obj.Start(cEnemyActor, cEnemyActor);
			obj.ApplyToActor(cEnemyActor);
			CAbility cAbility = CAbilityDamage.CreateDefaultDamage(1, 1, 1, isMonster: true, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.MiscAbilityData.IgnoreStun = true;
			cAbility.SkipAnim = true;
			cAbility.IsMonsterAbility = true;
			CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.AddActiveBonus, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData(), activeBonusData: new AbilityData.ActiveBonusData
			{
				Duration = CActiveBonus.EActiveBonusDurationType.Persistent,
				Behaviour = CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnConditionEnded,
				Hidden = true,
				Restriction = CActiveBonus.EActiveBonusRestrictionType.OncePerRound,
				Vars = new List<(string, TValue)> { ("ValidateDecTriggerIsEndTurn", new TValue(CExpression.Parse("DecTrigger == \"Turns\""))) }
			}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: cAbility, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
			cAbility2.Start(cEnemyActor, cEnemyActor);
			cAbility2.ApplyToActor(cEnemyActor);
		}
	}
}
