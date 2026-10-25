using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilitySetNextAttackModifiers : CAbilityTargeting
{
	public List<AttackModifierYMLData> NextAttackModifiers;

	public List<AttackModifierYMLData> NewNextAttackModifiers;

	public CAbilitySetNextAttackModifiers()
		: base(EAbilityType.SetNextAttackModifiers)
	{
	}

	public static CAbility CreateDefaultNullTargeting(int range, int numberOfTargets, bool isMonster, string animationOverload = "", CAbilityFilterContainer abilityFilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.SetNextAttackModifiers, 0, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, (abilityFilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityFilter, animationOverload, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultNullTargeting", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public IEnumerable<AttackModifierYMLData> GetRemainingModifiersToChoose()
	{
		List<AttackModifierYMLData> toCheck = NextAttackModifiers.ToList();
		foreach (AttackModifierYMLData mod in NextAttackModifiers)
		{
			int num = toCheck.Count((AttackModifierYMLData x) => x == mod);
			if (NewNextAttackModifiers.Count((AttackModifierYMLData x) => x == mod) < num)
			{
				toCheck.Remove(mod);
				yield return mod;
			}
		}
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

	public override bool PreProcessBeforeActorApplies()
	{
		List<AttackModifierYMLData> attackModifierDeck = CustomAbility.GetAttackModifierDeck(TargetingActor);
		NextAttackModifiers = new List<AttackModifierYMLData>();
		NewNextAttackModifiers = new List<AttackModifierYMLData>();
		for (int i = 0; i < Math.Min(Strength, attackModifierDeck.Count); i++)
		{
			NextAttackModifiers.Add(attackModifierDeck[i]);
			NewNextAttackModifiers.Add(null);
		}
		CShowSetNextAttackModifiers_MessageData message = new CShowSetNextAttackModifiers_MessageData(TargetingActor)
		{
			m_ChooseAbility = this,
			m_IsFirstAbility = (PhaseManager.CurrentPhaseAction.FirstPhaseAbility == PhaseManager.CurrentPhaseAction.CurrentPhaseAbility)
		};
		ScenarioRuleClient.MessageHandler(message);
		base.PreProcessBeforeActorApplies();
		return false;
	}

	public void SetNextModifier(AttackModifierYMLData modifier, int id)
	{
		NewNextAttackModifiers[id] = modifier;
		CShowSetNextAttackModifiers_MessageData message = new CShowSetNextAttackModifiers_MessageData(TargetingActor)
		{
			m_ChooseAbility = this,
			m_IsFirstAbility = (PhaseManager.CurrentPhaseAction.FirstPhaseAbility == PhaseManager.CurrentPhaseAction.CurrentPhaseAbility)
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (NextAttackModifiers != null && NextAttackModifiers.Count > 0)
		{
			return NewNextAttackModifiers.All((AttackModifierYMLData x) => x != null);
		}
		return base.CanBePerformed(performer);
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			List<AttackModifierYMLData> attackModifierDeck = CustomAbility.GetAttackModifierDeck(TargetingActor);
			for (int i = 0; i < NewNextAttackModifiers.Count; i++)
			{
				attackModifierDeck.RemoveAt(0);
			}
			for (int num = NewNextAttackModifiers.Count - 1; num >= 0; num--)
			{
				attackModifierDeck.Insert(0, NewNextAttackModifiers[num]);
			}
			CShowNextAttackModifierActiveBonus.RefreshAllShowNextModifierBonuses(actor);
		}
		return true;
	}

	public override bool IsPositive()
	{
		return !GetMiscVar("IsNegative").GetBool();
	}
}
