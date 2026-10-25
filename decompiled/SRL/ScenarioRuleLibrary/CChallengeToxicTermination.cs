using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeToxicTermination : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1495u;

	public override string Name => "toxic_termination";

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

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(actorsAppliedTo);
	}

	public override IEnumerable<Trigger> GetTriggers()
	{
		yield return Trigger.OnSpawn;
	}

	public static bool MonstersDiedThisRound()
	{
		if (ScenarioManager.CurrentScenarioState?.Stats == null)
		{
			return false;
		}
		int curRound = ScenarioManager.CurrentScenarioState.RoundNumber;
		return ScenarioManager.CurrentScenarioState.Stats.m_MonsterStats.Deaths.Any((CPlayerStatsKill x) => x.Round == curRound);
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
		{
			actorsAppliedTo.Add(cEnemyActor.ActorGuid);
			CAbilityAddActiveBonus obj = CAbility.CreateAbility(CAbility.EAbilityType.AddActiveBonus, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData(), activeBonusData: new AbilityData.ActiveBonusData
			{
				Behaviour = CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnDeath,
				Hidden = true,
				Duration = CActiveBonus.EActiveBonusDurationType.Persistent,
				Requirements = new CAbilityRequirements
				{
					Vars = new List<(string, TValue)> { ("ValidateFirstDeathThisRound", new TValue(CExpression.Parse("CChallengeToxicTermination.MonstersDiedThisRound() == False"))) }
				},
				Vars = new List<(string, TValue)> { ("ValidateSelfDeath", new TValue(CExpression.Parse("TriggerActor == ActiveBonus.Caster"))) },
				TriggerAbilitiesImmediately = true
			}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: false, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue) as CAbilityAddActiveBonus;
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Poison, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.All, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player), isMonster: false, isTargetedAbility: true, 0, 1, 1, 0, CAbility.EAbilityFlag.None, 1, EConditionDecTrigger.Never);
			cAbility.IsScenarioModifierAbility = true;
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.SkipAnim = true;
			cAbility.SetMiscVar("IsMapCondition", true);
			cAbility.Targeting = CAbility.EAbilityTargeting.Range;
			cAbility.Range = 1;
			cAbility.NumberTargets = -1;
			obj.AddAbility = cAbility;
			obj.Start(cEnemyActor, cEnemyActor);
			obj.ApplyToActor(cEnemyActor);
		}
	}
}
