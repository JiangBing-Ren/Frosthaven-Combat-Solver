using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeIncreasingAggression : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1500u;

	public override string Name => "increasing_aggression";

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

	public static void RemoveMinusOneTwoModifiers(CActor actor, AttackModifierYMLData cardDrawn)
	{
		if (actor.MonsterClass == null || (!(cardDrawn.Name == "AMC_Basic_M1") && !(cardDrawn.Name == "AMC_Basic_M2")))
		{
			return;
		}
		List<AttackModifierYMLData> list = null;
		if (actor.Type == CActor.EType.Enemy)
		{
			list = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards;
		}
		else if (actor.Type == CActor.EType.Enemy2)
		{
			list = MonsterClassManager.Enemy2MonsterAttackModifierDeck.DiscardedAttackModifierCards;
		}
		if (list == null)
		{
			return;
		}
		AttackModifierYMLData cardInDeck = list.FirstOrDefault((AttackModifierYMLData x) => x.Name.Equals(cardDrawn.Name));
		if (cardInDeck != null)
		{
			list.Remove(cardInDeck);
			if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityAttack)
			{
				CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
				currentPhaseAbility.m_EndAction = (Action)Delegate.Combine(currentPhaseAbility.m_EndAction, new Action(Log));
			}
		}
		void Log()
		{
			CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
			{
				m_MessageKey = "COMBAT_LOG_ATTACK_MODIFIER_REMOVED_FROM_DECK",
				m_Params = new List<string>
				{
					"Actor_0",
					cardInDeck.Card.AttackModifierLogString()
				},
				m_Actors = new List<CActor> { actor }
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor is CEnemyActor cEnemyActor && (cEnemyActor.Type == CActor.EType.Enemy || cEnemyActor.Type == CActor.EType.Enemy2) && !actorsAppliedTo.Contains(cEnemyActor.ActorGuid))
		{
			actorsAppliedTo.Add(cEnemyActor.ActorGuid);
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.NullTargeting, 0, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData(), activeBonusData: new AbilityData.ActiveBonusData
			{
				Duration = CActiveBonus.EActiveBonusDurationType.Persistent,
				Behaviour = CActiveBonus.EActiveBonusBehaviourType.GenericOnDrawAttackModifier,
				Hidden = true,
				Vars = new List<(string, TValue)>
				{
					("ValidateIsCaster", new TValue(CExpression.Parse("TriggerActor == ActiveBonus.Actor"))),
					("RemoveModifier", new TValue(CExpression.Parse("CChallengeIncreasingAggression.RemoveMinusOneTwoModifiers(TriggerActor, attackModifier)")))
				}
			}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
			cAbility.Start(cEnemyActor, cEnemyActor);
			cAbility.ApplyToActor(cEnemyActor);
		}
	}
}
