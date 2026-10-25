using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityHeal : CAbilityTargeting
{
	[JsonObject(MemberSerialization.OptIn)]
	public class HealAbilityData
	{
		[JsonProperty]
		public bool IgnoreTokens;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> PositiveConditionsToAddIfHealRemovesPoison;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> NegativeConditionsToAddIfHealRemovesPoison;

		[JsonProperty]
		public CAbilityFilterContainer ApplyConditionsOnRemovingPoisonFilter;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> PositiveConditionsToAddIfHealRemovesWound;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> NegativeConditionsToAddIfHealRemovesWound;

		[JsonProperty]
		public CAbilityFilterContainer ApplyConditionsOnRemovingWoundFilter;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> PositiveConditionsToAddIfHealRemovesBane;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> NegativeConditionsToAddIfHealRemovesBane;

		[JsonProperty]
		public CAbilityFilterContainer ApplyConditionsOnRemovingBaneFilter;

		[JsonProperty]
		public List<CCondition.EPositiveCondition> PositiveConditionsToAddIfHealRemovesBrittle;

		[JsonProperty]
		public List<CCondition.ENegativeCondition> NegativeConditionsToAddIfHealRemovesBrittle;

		[JsonProperty]
		public CAbilityFilterContainer ApplyConditionsOnRemovingBrittleFilter;

		public bool TriggerOnHealedListeners = true;

		public HealAbilityData()
		{
			IgnoreTokens = false;
			PositiveConditionsToAddIfHealRemovesPoison = new List<CCondition.EPositiveCondition>();
			NegativeConditionsToAddIfHealRemovesPoison = new List<CCondition.ENegativeCondition>();
			ApplyConditionsOnRemovingPoisonFilter = CAbilityFilterContainer.CreateDefaultFilter();
			PositiveConditionsToAddIfHealRemovesWound = new List<CCondition.EPositiveCondition>();
			NegativeConditionsToAddIfHealRemovesWound = new List<CCondition.ENegativeCondition>();
			ApplyConditionsOnRemovingWoundFilter = CAbilityFilterContainer.CreateDefaultFilter();
			PositiveConditionsToAddIfHealRemovesBane = new List<CCondition.EPositiveCondition>();
			NegativeConditionsToAddIfHealRemovesBane = new List<CCondition.ENegativeCondition>();
			ApplyConditionsOnRemovingBaneFilter = CAbilityFilterContainer.CreateDefaultFilter();
			PositiveConditionsToAddIfHealRemovesBrittle = new List<CCondition.EPositiveCondition>();
			NegativeConditionsToAddIfHealRemovesBrittle = new List<CCondition.ENegativeCondition>();
			ApplyConditionsOnRemovingBrittleFilter = CAbilityFilterContainer.CreateDefaultFilter();
		}

		public HealAbilityData Copy()
		{
			return new HealAbilityData
			{
				IgnoreTokens = IgnoreTokens,
				PositiveConditionsToAddIfHealRemovesPoison = PositiveConditionsToAddIfHealRemovesPoison.ToList(),
				NegativeConditionsToAddIfHealRemovesPoison = NegativeConditionsToAddIfHealRemovesPoison.ToList(),
				ApplyConditionsOnRemovingPoisonFilter = ApplyConditionsOnRemovingPoisonFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesWound = PositiveConditionsToAddIfHealRemovesWound.ToList(),
				NegativeConditionsToAddIfHealRemovesWound = NegativeConditionsToAddIfHealRemovesWound.ToList(),
				ApplyConditionsOnRemovingWoundFilter = ApplyConditionsOnRemovingWoundFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesBane = PositiveConditionsToAddIfHealRemovesBane.ToList(),
				NegativeConditionsToAddIfHealRemovesBane = NegativeConditionsToAddIfHealRemovesBane.ToList(),
				ApplyConditionsOnRemovingBaneFilter = ApplyConditionsOnRemovingBaneFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesBrittle = PositiveConditionsToAddIfHealRemovesBrittle.ToList(),
				NegativeConditionsToAddIfHealRemovesBrittle = NegativeConditionsToAddIfHealRemovesBrittle.ToList(),
				ApplyConditionsOnRemovingBrittleFilter = ApplyConditionsOnRemovingBrittleFilter.Copy(),
				TriggerOnHealedListeners = TriggerOnHealedListeners
			};
		}

		public HealAbilityData Merge(HealAbilityData dataToMerge)
		{
			return new HealAbilityData
			{
				IgnoreTokens = (IgnoreTokens || dataToMerge.IgnoreTokens),
				PositiveConditionsToAddIfHealRemovesPoison = PositiveConditionsToAddIfHealRemovesPoison.Concat(dataToMerge.PositiveConditionsToAddIfHealRemovesPoison).ToList(),
				NegativeConditionsToAddIfHealRemovesPoison = NegativeConditionsToAddIfHealRemovesPoison.Concat(dataToMerge.NegativeConditionsToAddIfHealRemovesPoison).ToList(),
				ApplyConditionsOnRemovingPoisonFilter = dataToMerge.ApplyConditionsOnRemovingPoisonFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesWound = PositiveConditionsToAddIfHealRemovesWound.Concat(dataToMerge.PositiveConditionsToAddIfHealRemovesWound).ToList(),
				NegativeConditionsToAddIfHealRemovesWound = NegativeConditionsToAddIfHealRemovesWound.Concat(dataToMerge.NegativeConditionsToAddIfHealRemovesWound).ToList(),
				ApplyConditionsOnRemovingWoundFilter = dataToMerge.ApplyConditionsOnRemovingWoundFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesBane = PositiveConditionsToAddIfHealRemovesBane.Concat(dataToMerge.PositiveConditionsToAddIfHealRemovesBane).ToList(),
				NegativeConditionsToAddIfHealRemovesBane = NegativeConditionsToAddIfHealRemovesBane.Concat(dataToMerge.NegativeConditionsToAddIfHealRemovesBane).ToList(),
				ApplyConditionsOnRemovingBaneFilter = dataToMerge.ApplyConditionsOnRemovingBaneFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesBrittle = PositiveConditionsToAddIfHealRemovesBrittle.Concat(dataToMerge.PositiveConditionsToAddIfHealRemovesBane).ToList(),
				NegativeConditionsToAddIfHealRemovesBrittle = NegativeConditionsToAddIfHealRemovesBrittle.Concat(dataToMerge.NegativeConditionsToAddIfHealRemovesBane).ToList(),
				ApplyConditionsOnRemovingBrittleFilter = dataToMerge.ApplyConditionsOnRemovingBrittleFilter.Copy(),
				TriggerOnHealedListeners = (TriggerOnHealedListeners || dataToMerge.TriggerOnHealedListeners)
			};
		}

		public static HealAbilityData DefaultHealData()
		{
			return new HealAbilityData();
		}
	}

	[JsonProperty]
	public HealAbilityData HealData { get; set; }

	[JsonIgnore]
	public int AmountReallyHealed { get; set; }

	public CAbilityHeal(HealAbilityData healData)
		: base(EAbilityType.Heal)
	{
		if (healData == null)
		{
			healData = HealAbilityData.DefaultHealData();
		}
		HealData = healData;
	}

	public static CAbility CreateDefaultHeal(int strength, int range, int numberOfTargets, bool isMonster, CAbilityFilterContainer abilityFilter = null, CAbilityRequirements startAbilityRequirements = null)
	{
		return CAbility.CreateAbility(EAbilityType.Heal, strength, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, (abilityFilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityFilter, "", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultHeal", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), (startAbilityRequirements != null) ? startAbilityRequirements : new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddHeal))
		{
			if (item.Ability?.ActiveBonusData == null || !item.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal))
			{
				m_ModifiedStrength += item.ReferenceStrength(this, TargetingActor);
			}
		}
		if (!m_HasAutoSelected)
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		SetHasStarted(val: true);
	}

	public override bool PreProcessBeforeActorApplies()
	{
		if (TargetingActor.Type != CActor.EType.Player && (MiscAbilityData == null || !MiscAbilityData.AutotriggerAbility.HasValue || !MiscAbilityData.AutotriggerAbility.Value))
		{
			bool flag = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				if (item.Health < item.MaxHealth || item.Tokens.HasKey(CCondition.ENegativeCondition.Wound) || item.Tokens.HasKey(CCondition.ENegativeCondition.Poison) || item.Tokens.HasKey(CCondition.ENegativeCondition.Bane) || item.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		base.PreProcessBeforeActorApplies();
		return true;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		AbilityData.EActiveBonusFlag eActiveBonusFlag = AbilityData.EActiveBonusFlag.None;
		Dictionary<CActor, int> dictionary = new Dictionary<CActor, int>();
		foreach (CActor item in actorsAppliedTo)
		{
			foreach (CActiveBonus item2 in CActiveBonus.FindApplicableActiveBonuses(item, EAbilityType.AddHeal))
			{
				if (item2.Ability?.ActiveBonusData != null && item2.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal))
				{
					Strength += item2.ReferenceStrength(this, item);
				}
			}
			foreach (CActiveBonus item3 in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddHeal))
			{
				if (item3.Ability?.ActiveBonusData == null || !item3.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal))
				{
					eActiveBonusFlag |= item3.ReferenceFlags(this, item);
				}
			}
			foreach (CActiveBonus item4 in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.GenericHealFinished))
			{
				if (item4 != null)
				{
					eActiveBonusFlag |= item4.ReferenceFlags(this, item);
				}
			}
			if (MiscAbilityData != null && MiscAbilityData.HealPercentageOfHealth.HasValue)
			{
				dictionary[item] = Strength + (int)Math.Floor((float)item.OriginalMaxHealth * MiscAbilityData.HealPercentageOfHealth.Value);
			}
			else
			{
				dictionary[item] = Strength;
			}
		}
		CActorIsHealing_MessageData message = new CActorIsHealing_MessageData(AnimOverload, actorApplying)
		{
			m_ActorsHealedAndHealStrength = dictionary,
			m_HealAbility = this,
			m_bonusesFlags = eActiveBonusFlag
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (actorApplying.Type != CActor.EType.Player)
		{
			validActorsInRange.Sort((CActor x, CActor y) => (100 * (y.MaxHealth - y.Health) + ((y.Tokens.HasKey(CCondition.ENegativeCondition.Wound) || y.Tokens.HasKey(CCondition.ENegativeCondition.Poison) || y.Tokens.HasKey(CCondition.ENegativeCondition.Brittle) || y.Tokens.HasKey(CCondition.ENegativeCondition.Bane)) ? 1 : 0)).CompareTo(100 * (x.MaxHealth - x.Health) + ((x.Tokens.HasKey(CCondition.ENegativeCondition.Wound) || x.Tokens.HasKey(CCondition.ENegativeCondition.Poison) || x.Tokens.HasKey(CCondition.ENegativeCondition.Brittle) || x.Tokens.HasKey(CCondition.ENegativeCondition.Bane)) ? 1 : 0)));
		}
	}

	public override bool ApplyToActor(CActor actor)
	{
		AmountReallyHealed = 0;
		int health = actor.Health;
		if (ScenarioManager.Scenario.HasActor(actor) && CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.BlockHealing).Count == 0)
		{
			AbilityHasHappened = true;
			int health2 = actor.Health;
			if (base.ApplyToActor(actor))
			{
				if (HealData.TriggerOnHealedListeners)
				{
					if (TargetingActor.m_OnHealApplyToActorListeners != null)
					{
						TargetingActor.m_OnHealApplyToActorListeners?.Invoke(this);
					}
					actor.m_OnBeingHealedListeners?.Invoke(this);
				}
				m_ModifiedStrength = Strength;
				if (MiscAbilityData != null && MiscAbilityData.HealPercentageOfHealth.HasValue)
				{
					m_ModifiedStrength += (int)Math.Floor((float)actor.OriginalMaxHealth * MiscAbilityData.HealPercentageOfHealth.Value);
				}
			}
			else
			{
				m_ModifiedStrength = 0;
			}
			actor.Healed(m_ModifiedStrength, this, TargetingActor, out var baneRemoved, out var woundRemoved, out var poisonRemoved, out var brittleRemoved, out var healedAmount, HealData.IgnoreTokens);
			if (poisonRemoved)
			{
				if (HealData.PositiveConditionsToAddIfHealRemovesPoison.Count > 0 && HealData.ApplyConditionsOnRemovingPoisonFilter.IsValidTarget(actor, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					foreach (CCondition.EPositiveCondition condition in HealData.PositiveConditionsToAddIfHealRemovesPoison)
					{
						CAbility cAbility = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility).ApplyToActor(actor);
					}
				}
				if (HealData.NegativeConditionsToAddIfHealRemovesPoison.Count > 0 && HealData.ApplyConditionsOnRemovingWoundFilter.IsValidTarget(actor, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					foreach (CCondition.ENegativeCondition condition2 in HealData.NegativeConditionsToAddIfHealRemovesPoison)
					{
						CAbility cAbility2 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition2.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility2.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility2).ApplyToActor(actor);
					}
				}
			}
			if (woundRemoved)
			{
				if (HealData.PositiveConditionsToAddIfHealRemovesWound.Count > 0)
				{
					foreach (CCondition.EPositiveCondition condition3 in HealData.PositiveConditionsToAddIfHealRemovesWound)
					{
						CAbility cAbility3 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition3.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility3.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility3).ApplyToActor(actor);
					}
				}
				if (HealData.NegativeConditionsToAddIfHealRemovesWound.Count > 0)
				{
					foreach (CCondition.ENegativeCondition condition4 in HealData.NegativeConditionsToAddIfHealRemovesWound)
					{
						CAbility cAbility4 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition4.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility4.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility4).ApplyToActor(actor);
					}
				}
			}
			if (baneRemoved)
			{
				if (HealData.PositiveConditionsToAddIfHealRemovesBane.Count > 0)
				{
					foreach (CCondition.EPositiveCondition condition5 in HealData.PositiveConditionsToAddIfHealRemovesBane)
					{
						CAbility cAbility5 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition5.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility5.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility5).ApplyToActor(actor);
					}
				}
				if (HealData.NegativeConditionsToAddIfHealRemovesBane.Count > 0)
				{
					foreach (CCondition.ENegativeCondition condition6 in HealData.NegativeConditionsToAddIfHealRemovesBane)
					{
						CAbility cAbility6 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition6.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility6.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility6).ApplyToActor(actor);
					}
				}
			}
			if (brittleRemoved)
			{
				if (HealData.PositiveConditionsToAddIfHealRemovesBrittle.Count > 0)
				{
					foreach (CCondition.EPositiveCondition condition7 in HealData.PositiveConditionsToAddIfHealRemovesBrittle)
					{
						CAbility cAbility7 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition7.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility7.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility7).ApplyToActor(actor);
					}
				}
				if (HealData.NegativeConditionsToAddIfHealRemovesBrittle.Count > 0)
				{
					foreach (CCondition.ENegativeCondition condition8 in HealData.NegativeConditionsToAddIfHealRemovesBrittle)
					{
						CAbility cAbility8 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition8.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility8.Start(TargetingActor, FilterActor);
						((CAbilityTargeting)cAbility8).ApplyToActor(actor);
					}
				}
			}
			CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(TargetingActor, actor, (!poisonRemoved) ? healedAmount : 0, health2, poisonRemoved, woundRemoved, baneRemoved, brittleRemoved);
			ScenarioRuleClient.MessageHandler(message);
		}
		if (m_PositiveConditions.Count > 0)
		{
			ProcessPositiveStatusEffects(actor);
		}
		if (m_NegativeConditions.Count > 0)
		{
			ProcessNegativeStatusEffects(actor);
		}
		AmountReallyHealed = actor.Health - health;
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override void Restart()
	{
		base.Restart();
		m_ModifiedStrength = Strength;
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddHeal))
		{
			if (item.Ability?.ActiveBonusData == null || !item.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal))
			{
				m_ModifiedStrength += item.ReferenceStrength(this, TargetingActor);
			}
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
	}

	public override void AbilityEnded()
	{
		if (m_AbilityHasHappened)
		{
			TargetingActor.m_OnHealApplyToActionListeners?.Invoke(this);
		}
		GameState.ShowToggleActiveBonusSelection(TargetingActor as CPlayerActor, TargetingActor, this, EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.GenericHealAbilityFinished);
		CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericHealAbilityFinished, this, TargetingActor, null);
		base.AbilityEnded();
	}
}
