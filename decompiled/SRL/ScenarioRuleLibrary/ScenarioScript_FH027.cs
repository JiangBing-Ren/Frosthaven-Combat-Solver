using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH027 : ScenarioScript
{
	protected override void TickDeathInternal()
	{
		if (_tickAbility == null || !(_tickActor is CObjectActor))
		{
			return;
		}
		CPlayerActor cPlayerActor = _tickAbility.TargetingActor as CPlayerActor;
		if (cPlayerActor == null)
		{
			if (!(_tickAbility.TargetingActor is CHeroSummonActor cHeroSummonActor))
			{
				return;
			}
			cPlayerActor = cHeroSummonActor.Summoner;
			if (cPlayerActor == null)
			{
				return;
			}
		}
		foreach (CAbilityCard item in cPlayerActor.CharacterClass.TakenCards.ToList())
		{
			Trace($"Returned {item.Name}: {item.ID}", null, 42);
			cPlayerActor.CharacterClass.MoveAbilityCard(item, cPlayerActor.CharacterClass.TakenCards, cPlayerActor.CharacterClass.HandAbilityCards, "TakenAbilityCards", "HandAbilityCards");
		}
	}

	private static void TakeCard(CPlayerActor player)
	{
		if (!player.IsDead)
		{
			CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.SelectAndMoveCard, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, miscVars: new List<(string, TValue)>
			{
				("SelectAndMoveCardPilesToMoveFrom", new TValue(new List<CBaseCard.ECardPile>
				{
					CBaseCard.ECardPile.Hand,
					CBaseCard.ECardPile.Discarded
				})),
				("SelectAndMoveCardPileToMoveTo", new TValue(CBaseCard.ECardPile.Taken))
			}, miscAbilityData: new AbilityData.MiscAbilityData
			{
				AutotriggerAbility = true,
				CanSkip = false
			}, abilityFilter: new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), animationOverload: null, animationOverload2: null, subAbilities: null, areaEffect: null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, aiFocusOverride: null, moveRestrictionType: CAbilityMove.EMoveRestrictionType.None, activeBonusData: null, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: false, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: "GUI_SELECT_CARD_TO_BANISH", extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
			cAbility.TargetingActor = player;
			cAbility.TargetThisActorAutomatically = player;
			cAbility.SkipAnim = true;
			if (PhaseManager.CurrentPhaseAction != null)
			{
				PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, player, killAfter: false, stackToNextCurrent: true);
			}
			else
			{
				GameState.PendingScenarioModifierAbilities.Add(Tuple.Create(cAbility, cAbility.TargetingActor));
				PhaseManager.StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
			}
		}
	}

	protected override void TickTakenDamageInternal(int damageTaken, int damageReducedByShields, int actualDamage)
	{
		if (_tickAbility is CAbilityAttack && _tickActor is CPlayerActor player && _tickAttacker != null && _tickAttacker.Class.ID.Contains("DeepTerror"))
		{
			TakeCard(player);
		}
	}

	protected override void OnDoorOpenedInternal(CObjectDoor door, CActor actor, CActor creditActor, bool visualOnly)
	{
		CPlayerActor cPlayerActor = (creditActor as CPlayerActor) ?? (actor as CPlayerActor);
		foreach (CPlayerActor item in Enumerable.Reverse(ScenarioManager.Scenario.PlayerActors))
		{
			if (cPlayerActor != item)
			{
				TakeCard(item);
			}
		}
		if (cPlayerActor != null)
		{
			TakeCard(cPlayerActor);
		}
	}

	public override string TakenPileTooltipKey()
	{
		return "FH_27_Clear_Your_Mind_D";
	}

	public override string TakenPileNameKey()
	{
		return "GUI_CARD_SECTION_TAKEN_027";
	}
}
