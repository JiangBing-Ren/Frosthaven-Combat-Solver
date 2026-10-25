using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChallengeArcaneTactics : CChallenge
{
	private List<string> actorsAppliedTo = new List<string>();

	public override uint ID => 1516u;

	public override string Name => "arcane_tactics";

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

	public static int CountRevealedLootTokens()
	{
		return ScenarioManager.CurrentScenarioState.Props.Count((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken && x.PropTile.m_HexMap.Revealed);
	}

	public static bool ValidateHasElementsAboutToGoInert()
	{
		List<ElementInfusionBoardManager.EElement> waningToInertDisabled = ScenarioManager.CurrentScenarioState.ScenarioSetup.WaningToInertDisabled;
		int num = -1;
		ElementInfusionBoardManager.EColumn[] getElementColumn = ElementInfusionBoardManager.GetElementColumn;
		foreach (ElementInfusionBoardManager.EColumn num2 in getElementColumn)
		{
			num++;
			if (num2 == ElementInfusionBoardManager.EColumn.Waning && (waningToInertDisabled == null || !waningToInertDisabled.Contains((ElementInfusionBoardManager.EElement)num)))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ValidateNotRestricted()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			CActiveBonus cActiveBonus = playerActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.EndRoundAbility).FirstOrDefault((CActiveBonus x) => x.Ability.Name == "ArcaneTacticsEndRoundDamage");
			if (cActiveBonus != null && cActiveBonus.IsRestricted(playerActor))
			{
				return false;
			}
		}
		return true;
	}

	public static bool ValidateIsHost(CActor actor)
	{
		return actor == GameState.HostCharacter;
	}

	protected override void Perform(TriggerParams triggerParams = null)
	{
		if (triggerParams.actor is CPlayerActor cPlayerActor && !actorsAppliedTo.Contains(cPlayerActor.ActorGuid))
		{
			actorsAppliedTo.Add(cPlayerActor.ActorGuid);
			CAbility cAbility = CAbilityDamage.CreateDefaultDamage(ScenarioManager.HazardousTerrainDamage, 1, 1, isMonster: true, "", isMultipass: true, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self | CAbilityFilter.EFilterTargetType.Ally, CAbilityFilter.EFilterEnemy.None, CAbilityFilter.EFilterActorType.Player), "ArcaneTacticsDamage");
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.CanSkip = false;
			cAbility.MiscAbilityData.CanUndo = false;
			cAbility.MiscAbilityData.IgnoreStun = true;
			cAbility.SkipAnim = true;
			cAbility.IsMonsterAbility = true;
			cAbility.Targeting = CAbility.EAbilityTargeting.All;
			cAbility.MiscAbilityData.IgnoreLOS = true;
			cAbility.NumberTargets = 1;
			cAbility.HelpBoxTooltipLocKey = "1516_challenge_cards_arcane_tactics_3";
			cAbility.SetMiscVar("includeCasterTile", true);
			cAbility.SetMiscVar("HelpBoxArg0", ScenarioManager.HazardousTerrainDamage.ToString());
			CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.AddActiveBonus, 0, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData(), activeBonusData: new AbilityData.ActiveBonusData
			{
				Duration = CActiveBonus.EActiveBonusDurationType.Persistent,
				Behaviour = CActiveBonus.EActiveBonusBehaviourType.EndRoundAbility,
				Hidden = true,
				Requirements = new CAbilityRequirements
				{
					Vars = new List<(string, TValue)>
					{
						("ValidateHasElementsAboutToGoInert", new TValue(CExpression.Parse("CChallengeArcaneTactics.ValidateHasElementsAboutToGoInert()"))),
						("ValidateNotRestricted", new TValue(CExpression.Parse("CChallengeArcaneTactics.ValidateNotRestricted()"))),
						("ValidateIsHost", new TValue(CExpression.Parse("CChallengeArcaneTactics.ValidateIsHost(Actor)")))
					}
				},
				Restriction = CActiveBonus.EActiveBonusRestrictionType.OncePerRound
			}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: "ArcaneTacticsEndRoundDamage", abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: cAbility, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
			cAbility2.Start(cPlayerActor, cPlayerActor);
			cAbility2.ApplyToActor(cPlayerActor);
		}
	}
}
