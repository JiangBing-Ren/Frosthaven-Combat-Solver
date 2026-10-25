using System.Collections.Generic;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityReviveDowned : CAbilityTargeting
{
	public CAbilityReviveDowned()
		: base(EAbilityType.ReviveDowned)
	{
	}

	public static CAbility CreateDefaulReviveDowned(int range, int numberOfTargets, bool isMonster, CAbilityFilterContainer abilityFilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.ReviveDowned, 0, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, (abilityFilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Ally) : abilityFilter, "", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultReviveDowned", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (ScenarioManager.Scenario.HasActor(actor) && CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.BlockHealing).Count == 0 && base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			actor.SetDowned(downed: false);
		}
		return true;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsRevivingDowned_MessageData message = new CActorIsRevivingDowned_MessageData(AnimOverload, actorApplying)
		{
			m_ActorsRevived = actorsAppliedTo,
			m_ReviveDownedAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList, bool onlyCheck = true)
	{
		actorList.RemoveAll((CActor x) => !x.IsDowned);
		base.RemoveImmuneActorsFromList(ref actorList, onlyCheck);
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanSkipUntergetableCheck()
	{
		return true;
	}
}
